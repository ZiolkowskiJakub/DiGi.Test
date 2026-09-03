using System.IO;

namespace DiGi.YOLO.ONNX.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Modify.Predict(Classes.YOLOONNXPredictionOptions, System.Threading.CancellationToken)"/> rejects options that name no run at all.
        /// </summary>
        [Fact]
        public void Predict_Invalid()
        {
            Assert.Null(Modify.Predict(null));
            Assert.Null(Modify.Predict(new Classes.YOLOONNXPredictionOptions()));
            Assert.Null(Modify.Predict(new Classes.YOLOONNXPredictionOptions() { ModelPath = @"C:\YOLO\models\model.onnx" }));
        }

        /// <summary>
        /// Verifies that a source directory holding no images is answered without the model ever being loaded.
        /// <para>The model path names nothing on disk here, so a run that reached the session would fail. It succeeds instead, which is the point: an empty directory is a run with nothing to do rather than a run that went wrong, and the CPython path draws the same distinction.</para>
        /// </summary>
        [Fact]
        public void Predict_NoImages()
        {
            string directory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_ONNX_Test_" + Path.GetRandomFileName());

            try
            {
                string directory_Source = Path.Combine(directory, "input");
                Directory.CreateDirectory(directory_Source);

                Classes.YOLOONNXPredictionOptions yOLOONNXPredictionOptions = new()
                {
                    ModelPath = Path.Combine(directory, "model.onnx"),
                    OutputPath = Path.Combine(directory, "output", "results.bbrf"),
                    SourceDirectory = directory_Source
                };

                File.WriteAllText(yOLOONNXPredictionOptions.ModelPath!, "not a model, but a file that exists");

                Classes.YOLOONNXPredictionResult? yOLOONNXPredictionResult = Modify.Predict(yOLOONNXPredictionOptions);

                Assert.NotNull(yOLOONNXPredictionResult);
                Assert.True(yOLOONNXPredictionResult!.Succeeded);
                Assert.Equal(0, yOLOONNXPredictionResult.ImageCount);
                Assert.Empty(yOLOONNXPredictionResult.Values!);
                Assert.False(File.Exists(yOLOONNXPredictionResult.OutputPath));
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
        /// Verifies that a missing source directory is reported rather than thrown, so an unattended run learns why it scored nothing.
        /// </summary>
        [Fact]
        public void Predict_MissingSourceDirectory()
        {
            Classes.YOLOONNXPredictionOptions yOLOONNXPredictionOptions = new()
            {
                ModelPath = Path.Combine(Path.GetTempPath(), "model.onnx"),
                OutputPath = Path.Combine(Path.GetTempPath(), "results.bbrf"),
                SourceDirectory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_ONNX_Missing_" + Path.GetRandomFileName())
            };

            Classes.YOLOONNXPredictionResult? yOLOONNXPredictionResult = Modify.Predict(yOLOONNXPredictionOptions);

            Assert.NotNull(yOLOONNXPredictionResult);
            Assert.False(yOLOONNXPredictionResult!.Succeeded);
            Assert.NotNull(yOLOONNXPredictionResult.Messages);
            Assert.Contains(yOLOONNXPredictionResult.Messages!, x => x.Contains("Source directory does not exist"));
        }

        /// <summary>
        /// Verifies that a model which will not load is reported with the reason, and that the stale result file of an earlier run is gone rather than left to be read back as this run's answer.
        /// </summary>
        [Fact]
        public void Predict_BadModel()
        {
            string directory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_ONNX_Test_" + Path.GetRandomFileName());

            try
            {
                string directory_Source = Path.Combine(directory, "input");
                Directory.CreateDirectory(directory_Source);

                //One image, so the run gets as far as loading the model
                File.WriteAllBytes(Path.Combine(directory_Source, "0207_2021.jpeg"), [0xFF, 0xD8, 0xFF, 0xD9]);

                string path_Model = Path.Combine(directory, "model.onnx");
                File.WriteAllText(path_Model, "not a model");

                string path_Output = Path.Combine(directory, "output", "results.bbrf");
                Directory.CreateDirectory(Path.GetDirectoryName(path_Output)!);
                File.WriteAllText(path_Output, "0000_1900\t0\t1\t2\t3\t4\t0.5");

                Classes.YOLOONNXPredictionOptions yOLOONNXPredictionOptions = new()
                {
                    ModelPath = path_Model,
                    OutputPath = path_Output,
                    SourceDirectory = directory_Source
                };

                Classes.YOLOONNXPredictionResult? yOLOONNXPredictionResult = Modify.Predict(yOLOONNXPredictionOptions);

                Assert.NotNull(yOLOONNXPredictionResult);
                Assert.False(yOLOONNXPredictionResult!.Succeeded);
                Assert.Null(yOLOONNXPredictionResult.Values);
                Assert.NotNull(yOLOONNXPredictionResult.Messages);
                Assert.Contains(yOLOONNXPredictionResult.Messages!, x => x.Contains("Model could not be loaded"));

                //The earlier run's answer must not survive a failed run
                Assert.False(File.Exists(path_Output));
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
