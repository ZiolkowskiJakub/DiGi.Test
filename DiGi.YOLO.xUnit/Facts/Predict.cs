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
    }
}
