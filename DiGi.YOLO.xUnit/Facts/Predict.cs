using System.IO;
using System.Linq;

namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Modify.Predict(Classes.YOLOPredictionOptions?, System.Threading.CancellationToken)"/> answers a source directory holding no images without starting an interpreter.
        /// <para>predict.py writes no result file in that case, so a run over an empty directory would otherwise be indistinguishable from a crash. The interpreter path is deliberately nonsense: reaching it would fail the test.</para>
        /// </summary>
        [Fact]
        public void Predict_NoImages()
        {
            string directory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Test_" + Path.GetRandomFileName());

            try
            {
                Directory.CreateDirectory(directory);

                string sourceDirectory = Path.Combine(directory, "input");
                Directory.CreateDirectory(sourceDirectory);

                Classes.YOLOPredictionOptions yOLOPredictionOptions = new()
                {
                    ModelPath = Path.Combine(directory, "best.pt"),
                    OutputPath = Path.Combine(directory, "output", "results.bbrf"),
                    PythonPath = Path.Combine(directory, "no_such_interpreter.exe"),
                    SourceDirectory = sourceDirectory,
                    WorkingDirectory = directory
                };

                Classes.YOLOPredictionResult? yOLOPredictionResult = Modify.Predict(yOLOPredictionOptions);

                Assert.NotNull(yOLOPredictionResult);
                Assert.Equal(0, yOLOPredictionResult!.ExitCode);
                Assert.Equal(0, yOLOPredictionResult.ImageCount);
                Assert.True(yOLOPredictionResult.Succeeded);
                Assert.Empty(yOLOPredictionResult.Values!);
                Assert.Empty(Create.BoundingBoxResultFile(yOLOPredictionResult)!);
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        /// <summary>
        /// Verifies that an interpreter that cannot be started is reported as a failed run rather than thrown, and that the reason reaches the caller.
        /// </summary>
        [Fact]
        public void Predict_InterpreterMissing()
        {
            string directory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Test_" + Path.GetRandomFileName());

            try
            {
                Directory.CreateDirectory(directory);

                string sourceDirectory = Path.Combine(directory, "input");
                Directory.CreateDirectory(sourceDirectory);
                File.WriteAllBytes(Path.Combine(sourceDirectory, "0207_2021.jpeg"), [0xFF, 0xD8, 0xFF, 0xD9]);

                Classes.YOLOPredictionOptions yOLOPredictionOptions = new()
                {
                    ModelPath = Path.Combine(directory, "best.pt"),
                    OutputPath = Path.Combine(directory, "output", "results.bbrf"),
                    PythonPath = Path.Combine(directory, "no_such_interpreter.exe"),
                    SourceDirectory = sourceDirectory,
                    WorkingDirectory = directory
                };

                Classes.YOLOPredictionResult? yOLOPredictionResult = Modify.Predict(yOLOPredictionOptions);

                Assert.NotNull(yOLOPredictionResult);
                Assert.NotEqual(0, yOLOPredictionResult!.ExitCode);
                Assert.Equal(1, yOLOPredictionResult.ImageCount);
                Assert.False(yOLOPredictionResult.Succeeded);
                Assert.NotEmpty(yOLOPredictionResult.StandardError!);
                Assert.Null(Create.BoundingBoxResultFile(yOLOPredictionResult));

                //The scripts have to be laid down together, because predict.py imports utils.py from its own directory
                Assert.True(File.Exists(Path.Combine(directory, "predict.py")));
                Assert.True(File.Exists(Path.Combine(directory, "utils.py")));
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        /// <summary>
        /// Verifies that a stale result file left by an earlier run is removed before a new run, so that a failed run cannot hand back the previous run's detections.
        /// </summary>
        [Fact]
        public void Predict_StaleOutput()
        {
            string directory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Test_" + Path.GetRandomFileName());

            try
            {
                Directory.CreateDirectory(directory);

                string sourceDirectory = Path.Combine(directory, "input");
                Directory.CreateDirectory(sourceDirectory);
                File.WriteAllBytes(Path.Combine(sourceDirectory, "0207_2021.jpeg"), [0xFF, 0xD8, 0xFF, 0xD9]);

                string outputPath = Path.Combine(directory, "results.bbrf");
                File.WriteAllText(outputPath, "0207_2021\t0\t12.5\t20.25\t40.5\t60.75\t0.93");

                Classes.YOLOPredictionOptions yOLOPredictionOptions = new()
                {
                    ModelPath = Path.Combine(directory, "best.pt"),
                    OutputPath = outputPath,
                    PythonPath = Path.Combine(directory, "no_such_interpreter.exe"),
                    SourceDirectory = sourceDirectory,
                    WorkingDirectory = directory
                };

                Modify.Predict(yOLOPredictionOptions);

                Assert.False(File.Exists(outputPath));
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        /// <summary>
        /// Verifies that <see cref="Modify.Predict(Classes.YOLOPredictionOptions?, System.Threading.CancellationToken)"/> rejects options that name no run at all.
        /// </summary>
        [Fact]
        public void Predict_Incomplete()
        {
            Assert.Null(Modify.Predict(null));
            Assert.Null(Modify.Predict(new Classes.YOLOPredictionOptions()));
        }

        /// <summary>
        /// Verifies that the prediction bounding-box coordinate contract computes genuine extents (width = x2 - x1, height = y2 - y1) rather than echoing corner coordinates.
        /// </summary>
        [Fact]
        public void Predict_BoundingBoxCoordinateContract()
        {
            string? pythonPath = Query.PythonPaths().FirstOrDefault();
            if (string.IsNullOrWhiteSpace(pythonPath))
            {
                return;
            }

            string directory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Test_" + Path.GetRandomFileName());

            try
            {
                Directory.CreateDirectory(directory);

                string sourceDirectory = Path.Combine(directory, "input");
                Directory.CreateDirectory(sourceDirectory);
                File.WriteAllBytes(Path.Combine(sourceDirectory, "sample_001.jpg"), [0xFF, 0xD8, 0xFF, 0xD9]);

                string path_Model = Path.Combine(directory, "best.pt");
                File.WriteAllText(path_Model, "dummy_model");

                string directory_MockUltralytics = Path.Combine(directory, "ultralytics");
                Directory.CreateDirectory(directory_MockUltralytics);

                string[] lines_MockUltralytics =
                [
                    "class MockTensor:",
                    "    def __init__(self, data):",
                    "        self.data = data",
                    "    def tolist(self):",
                    "        return self.data",
                    "    def item(self):",
                    "        return self.data",
                    "",
                    "class MockBox:",
                    "    def __init__(self, xyxy, conf, cls):",
                    "        self.xyxy = [MockTensor(xyxy)]",
                    "        self.conf = MockTensor(conf)",
                    "        self.cls = MockTensor(cls)",
                    "",
                    "class MockResult:",
                    "    def __init__(self, boxes):",
                    "        self.boxes = boxes",
                    "",
                    "class YOLO:",
                    "    def __init__(self, model_path):",
                    "        pass",
                    "    def __call__(self, source, **kwargs):",
                    "        count = len(source) if isinstance(source, list) else 1",
                    "        return [MockResult([MockBox([100.0, 50.0, 300.0, 150.0], 0.95, 0)]) for _ in range(count)]"
                ];
                File.WriteAllLines(Path.Combine(directory_MockUltralytics, "__init__.py"), lines_MockUltralytics);

                string path_Output = Path.Combine(directory, "output", "results.bbrf");

                Classes.YOLOPredictionOptions yOLOPredictionOptions = new()
                {
                    Confidence = 0.25,
                    ModelPath = path_Model,
                    OutputPath = path_Output,
                    PythonPath = pythonPath,
                    SourceDirectory = sourceDirectory,
                    WorkingDirectory = directory
                };

                Classes.YOLOPredictionResult? yOLOPredictionResult = Modify.Predict(yOLOPredictionOptions);

                Assert.NotNull(yOLOPredictionResult);
                Assert.True(yOLOPredictionResult!.Succeeded);

                Classes.BoundingBoxResultFile? boundingBoxResultFile = Create.BoundingBoxResultFile(yOLOPredictionResult);
                Assert.NotNull(boundingBoxResultFile);
                Assert.Single(boundingBoxResultFile!);

                Classes.BoundingBoxResult boundingBoxResult = boundingBoxResultFile![0];
                Assert.Equal("sample_001", boundingBoxResult.Name);
                Assert.Equal(0, boundingBoxResult.LabelIndex);
                Assert.Equal(100.0, boundingBoxResult.X);
                Assert.Equal(50.0, boundingBoxResult.Y);
                Assert.Equal(200.0, boundingBoxResult.Width);
                Assert.Equal(100.0, boundingBoxResult.Height);
                Assert.Equal(0.95, boundingBoxResult.Confidence);
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        /// <summary>
        /// Verifies that <see cref="Modify.Predict(Classes.YOLOPredictionOptions?, System.Threading.CancellationToken)"/> passes the batch size and processes multiple images in slices.
        /// </summary>
        [Fact]
        public void Predict_Batch()
        {
            string? pythonPath = Query.PythonPaths().FirstOrDefault();
            if (string.IsNullOrWhiteSpace(pythonPath))
            {
                return;
            }

            string directory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Test_" + Path.GetRandomFileName());

            try
            {
                Directory.CreateDirectory(directory);

                string sourceDirectory = Path.Combine(directory, "input");
                Directory.CreateDirectory(sourceDirectory);
                File.WriteAllBytes(Path.Combine(sourceDirectory, "sample_001.jpg"), [0xFF, 0xD8, 0xFF, 0xD9]);
                File.WriteAllBytes(Path.Combine(sourceDirectory, "sample_002.jpg"), [0xFF, 0xD8, 0xFF, 0xD9]);
                File.WriteAllBytes(Path.Combine(sourceDirectory, "sample_003.jpg"), [0xFF, 0xD8, 0xFF, 0xD9]);

                string path_Model = Path.Combine(directory, "best.pt");
                File.WriteAllText(path_Model, "dummy_model");

                string directory_MockUltralytics = Path.Combine(directory, "ultralytics");
                Directory.CreateDirectory(directory_MockUltralytics);

                string[] lines_MockUltralytics =
                [
                    "class MockTensor:",
                    "    def __init__(self, data):",
                    "        self.data = data",
                    "    def tolist(self):",
                    "        return self.data",
                    "    def item(self):",
                    "        return self.data",
                    "",
                    "class MockBox:",
                    "    def __init__(self, xyxy, conf, cls):",
                    "        self.xyxy = [MockTensor(xyxy)]",
                    "        self.conf = MockTensor(conf)",
                    "        self.cls = MockTensor(cls)",
                    "",
                    "class MockResult:",
                    "    def __init__(self, boxes):",
                    "        self.boxes = boxes",
                    "",
                    "class YOLO:",
                    "    def __init__(self, model_path):",
                    "        pass",
                    "    def __call__(self, source, **kwargs):",
                    "        count = len(source) if isinstance(source, list) else 1",
                    "        return [MockResult([MockBox([100.0, 50.0, 300.0, 150.0], 0.95, 0)]) for _ in range(count)]"
                ];
                File.WriteAllLines(Path.Combine(directory_MockUltralytics, "__init__.py"), lines_MockUltralytics);

                string path_Output = Path.Combine(directory, "output", "results.bbrf");

                Classes.YOLOPredictionOptions yOLOPredictionOptions = new()
                {
                    BatchSize = 2,
                    Confidence = 0.25,
                    ModelPath = path_Model,
                    OutputPath = path_Output,
                    PythonPath = pythonPath,
                    SourceDirectory = sourceDirectory,
                    WorkingDirectory = directory
                };

                Classes.YOLOPredictionResult? yOLOPredictionResult = Modify.Predict(yOLOPredictionOptions);

                Assert.NotNull(yOLOPredictionResult);
                Assert.True(yOLOPredictionResult!.Succeeded);
                Assert.Equal(3, yOLOPredictionResult.ImageCount);

                Classes.BoundingBoxResultFile? boundingBoxResultFile = Create.BoundingBoxResultFile(yOLOPredictionResult);
                Assert.NotNull(boundingBoxResultFile);
                Assert.Equal(3, boundingBoxResultFile!.Count);
                Assert.Equal("sample_001", boundingBoxResultFile[0].Name);
                Assert.Equal("sample_002", boundingBoxResultFile[1].Name);
                Assert.Equal("sample_003", boundingBoxResultFile[2].Name);
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }
    }
}
