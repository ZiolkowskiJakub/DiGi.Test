using System.IO;

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
            string? pythonPath = PythonPath_Runnable();
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

                MockUltralytics(directory);

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
            string? pythonPath = PythonPath_Runnable();
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

                MockUltralytics(directory);

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

        /// <summary>
        /// Verifies that a run reaching process start isolates the ultralytics settings in the working directory's .yolo-config folder.
        /// <para>The interpreter path is deliberately nonsense so the process cannot run; the folder is created by the runner before the start is attempted, which is what the assertion needs.</para>
        /// </summary>
        [Fact]
        public void Predict_ConfigDirectory()
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

                Modify.Predict(yOLOPredictionOptions);

                Assert.True(Directory.Exists(Path.Combine(directory, Constants.DirectoryName.YoloConfig)));
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
        /// Verifies that the overlapped reader (<see cref="Classes.YOLOPredictionOptions.Workers"/> above zero) names every image as ultralytics' own reader (zero) does, in the same order, and that only the settings a run asked for reach the model.
        /// <para>Runs predict.py against the mock ultralytics package, which records the keyword arguments of every model call. The default run must pass neither a precision nor imgsz, so it is the call earlier builds made; the second run must pass both, the precision as quantize=16, because ultralytics 8.4 deprecates half and logs a warning on every call that passes it.</para>
        /// </summary>
        [Fact]
        public void Predict_Workers()
        {
            string? pythonPath = PythonPath_Runnable();
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
                foreach (string fileName in new string[] { "sample_001.jpg", "sample_002.jpg", "sample_003.jpg" })
                {
                    File.WriteAllBytes(Path.Combine(sourceDirectory, fileName), [0xFF, 0xD8, 0xFF, 0xD9]);
                }

                string path_Model = Path.Combine(directory, "best.pt");
                File.WriteAllText(path_Model, "dummy_model");

                MockUltralytics(directory);

                string path_Calls = Path.Combine(directory, "calls.jsonl");

                Classes.YOLOPredictionOptions yOLOPredictionOptions = new()
                {
                    BatchSize = 2,
                    ModelPath = path_Model,
                    OutputPath = Path.Combine(directory, "output", "results_default.bbrf"),
                    PythonPath = pythonPath,
                    SourceDirectory = sourceDirectory,
                    Workers = 0,
                    WorkingDirectory = directory
                };

                Classes.YOLOPredictionResult? yOLOPredictionResult_Default = Modify.Predict(yOLOPredictionOptions);
                Assert.NotNull(yOLOPredictionResult_Default);
                Assert.True(yOLOPredictionResult_Default!.Succeeded);

                string[] lines_Calls_Default = File.ReadAllLines(path_Calls);
                Assert.Equal(2, lines_Calls_Default.Length);
                Assert.All(lines_Calls_Default, line => Assert.DoesNotContain("half", line));
                Assert.All(lines_Calls_Default, line => Assert.DoesNotContain("quantize", line));
                Assert.All(lines_Calls_Default, line => Assert.DoesNotContain("imgsz", line));
                File.Delete(path_Calls);

                Classes.YOLOPredictionOptions yOLOPredictionOptions_Workers = new(yOLOPredictionOptions)
                {
                    HalfPrecision = true,
                    ImageSize = 320,
                    OutputPath = Path.Combine(directory, "output", "results_workers.bbrf"),
                    Workers = 2
                };

                Classes.YOLOPredictionResult? yOLOPredictionResult_Workers = Modify.Predict(yOLOPredictionOptions_Workers);
                Assert.NotNull(yOLOPredictionResult_Workers);
                Assert.True(yOLOPredictionResult_Workers!.Succeeded);

                //The text, not only the parsed boxes: chunk results are joined by concatenation, so the lines themselves are the contract
                Assert.Equal(yOLOPredictionResult_Default.Values!, yOLOPredictionResult_Workers.Values!);
                Assert.Equal(3, yOLOPredictionResult_Workers.Values!.Count);
                Assert.StartsWith("sample_001\t", yOLOPredictionResult_Workers.Values[0]);
                Assert.StartsWith("sample_003\t", yOLOPredictionResult_Workers.Values[2]);

                string[] lines_Calls_Workers = File.ReadAllLines(path_Calls);
                Assert.Equal(2, lines_Calls_Workers.Length);
                Assert.All(lines_Calls_Workers, line => Assert.Contains("\"quantize\": 16", line));
                Assert.All(lines_Calls_Workers, line => Assert.DoesNotContain("half", line));
                Assert.All(lines_Calls_Workers, line => Assert.Contains("\"imgsz\": 320", line));
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
        /// Verifies that a predict.py left in the working directory by an earlier build is replaced before the run, so a run never executes an older script that does not know the flags it is handed.
        /// <para>The interpreter path is deliberately nonsense; the scripts are written before the start is attempted, which is what the assertion needs.</para>
        /// </summary>
        [Fact]
        public void Predict_RewritesScripts()
        {
            string directory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Test_" + Path.GetRandomFileName());

            try
            {
                Directory.CreateDirectory(directory);

                string sourceDirectory = Path.Combine(directory, "input");
                Directory.CreateDirectory(sourceDirectory);
                File.WriteAllBytes(Path.Combine(sourceDirectory, "0207_2021.jpeg"), [0xFF, 0xD8, 0xFF, 0xD9]);

                string path_Predict = Path.Combine(directory, "predict.py");
                File.WriteAllText(path_Predict, "print('stale')");
                File.WriteAllText(Path.Combine(directory, "utils.py"), string.Empty);

                Classes.YOLOPredictionOptions yOLOPredictionOptions = new()
                {
                    ModelPath = Path.Combine(directory, "best.pt"),
                    OutputPath = Path.Combine(directory, "output", "results.bbrf"),
                    PythonPath = Path.Combine(directory, "no_such_interpreter.exe"),
                    SourceDirectory = sourceDirectory,
                    Workers = 2,
                    WorkingDirectory = directory
                };

                Modify.Predict(yOLOPredictionOptions);

                string content = File.ReadAllText(path_Predict);
                Assert.DoesNotContain("stale", content);
                Assert.Contains("--workers", content);
                Assert.Contains("GetModelPath", File.ReadAllText(Path.Combine(directory, "utils.py")));
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
        /// Writes a mock ultralytics package into the given directory, which predict.py imports in place of the real one because Python puts the script's own directory first on its path.
        /// <para>It mirrors the parts of the ultralytics API predict.py touches: a result per source with its path and per-image speeds, a Boxes object with cpu() and per-column tensors, and ultralytics.utils.patches.imread for the overlapped reader. Every model call appends its half, quantize, imgsz and batch keyword arguments to calls.jsonl in the current directory. Each image gets one box from (100, 50) to (300, 150) at confidence 0.95.</para>
        /// </summary>
        /// <param name="directory">The directory predict.py runs from.</param>
        private static void MockUltralytics(string directory)
        {
            string directory_MockUltralytics = Path.Combine(directory, "ultralytics");
            Directory.CreateDirectory(Path.Combine(directory_MockUltralytics, "utils"));

            string[] lines_MockUltralytics =
            [
                "import json",
                "",
                "class MockTensor:",
                "    def __init__(self, data):",
                "        self.data = data",
                "    def tolist(self):",
                "        return self.data",
                "",
                "class MockBoxes:",
                "    def __init__(self, xyxy, conf, cls):",
                "        self.xyxy = MockTensor(xyxy)",
                "        self.conf = MockTensor(conf)",
                "        self.cls = MockTensor(cls)",
                "    def __len__(self):",
                "        return len(self.conf.data)",
                "    def cpu(self):",
                "        return self",
                "",
                "class MockResult:",
                "    def __init__(self, path, boxes):",
                "        self.path = path",
                "        self.boxes = boxes",
                "        self.speed = {\"preprocess\": 1.0, \"inference\": 2.0, \"postprocess\": 0.5}",
                "",
                "class YOLO:",
                "    def __init__(self, model_path):",
                "        pass",
                "    def __call__(self, source, **kwargs):",
                "        with open(\"calls.jsonl\", \"a\") as file:",
                "            file.write(json.dumps({key: kwargs[key] for key in (\"half\", \"quantize\", \"imgsz\", \"batch\") if key in kwargs}) + \"\\n\")",
                "        sources = source if isinstance(source, list) else [source]",
                "        return [MockResult(item if isinstance(item, str) else \"image%d.jpg\" % index, MockBoxes([[100.0, 50.0, 300.0, 150.0]], [0.95], [0.0])) for index, item in enumerate(sources)]"
            ];
            File.WriteAllLines(Path.Combine(directory_MockUltralytics, "__init__.py"), lines_MockUltralytics);
            File.WriteAllText(Path.Combine(directory_MockUltralytics, "utils", "__init__.py"), string.Empty);

            //Stands in for the decoder: any value other than None is a decoded image to the script
            string[] lines_MockPatches =
            [
                "def imread(filename, flags=None):",
                "    return object()"
            ];
            File.WriteAllLines(Path.Combine(directory_MockUltralytics, "utils", "patches.py"), lines_MockPatches);
        }

        /// <summary>
        /// Returns the first interpreter on PATH that actually starts, or <c>null</c> when none does.
        /// <para>A path that exists is not enough: with no Python installed, Windows still puts the App Installer redirector (a Microsoft Store app execution alias) on PATH as python.exe, which exits non-zero without running anything.</para>
        /// </summary>
        /// <returns>The path of a working interpreter, or <c>null</c>.</returns>
        private static string? PythonPath_Runnable()
        {
            foreach (string pythonPath in Python.Query.PythonPaths())
            {
                int exitCode = Python.Query.ExecuteProcess(pythonPath, "-c \"import sys\"", Path.GetTempPath()).ExitCode;
                if (exitCode == 0)
                {
                    return pythonPath;
                }
            }

            return null;
        }
    }
}
