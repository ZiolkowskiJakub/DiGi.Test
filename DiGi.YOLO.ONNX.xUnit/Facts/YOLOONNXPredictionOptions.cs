using System.IO;
using System.Linq;

namespace DiGi.YOLO.ONNX.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.YOLOONNXPredictionOptions"/> keeps the values it is given, survives the round trip through its string form, and clones identically.
        /// </summary>
        [Fact]
        public void YOLOONNXPredictionOptions()
        {
            Classes.YOLOONNXPredictionOptions yOLOONNXPredictionOptions = new()
            {
                BatchSize = 4,
                Confidence = 0.25,
                InputSize = 512,
                IoU = 0.45,
                MaxDetections = 100,
                ModelPath = @"C:\YOLO\models\model.onnx",
                OutputPath = @"C:\YOLO\output\results.bbrf",
                SourceDirectory = @"C:\YOLO\input"
            };

            Assert.Equal(4, yOLOONNXPredictionOptions.BatchSize);
            Assert.Equal(0.25, yOLOONNXPredictionOptions.Confidence);
            Assert.Equal(512, yOLOONNXPredictionOptions.InputSize);
            Assert.Equal(0.45, yOLOONNXPredictionOptions.IoU);
            Assert.Equal(100, yOLOONNXPredictionOptions.MaxDetections);
            Assert.Equal(@"C:\YOLO\models\model.onnx", yOLOONNXPredictionOptions.ModelPath);
            Assert.Equal(@"C:\YOLO\output\results.bbrf", yOLOONNXPredictionOptions.OutputPath);
            Assert.Equal(@"C:\YOLO\input", yOLOONNXPredictionOptions.SourceDirectory);

            string? json = Core.Convert.ToSystem_String(yOLOONNXPredictionOptions);
            Assert.False(string.IsNullOrWhiteSpace(json));

            Classes.YOLOONNXPredictionOptions? yOLOONNXPredictionOptions_Actual = Core.Convert.ToDiGi<Classes.YOLOONNXPredictionOptions>(json)?.FirstOrDefault();
            Assert.NotNull(yOLOONNXPredictionOptions_Actual);
            Assert.Equal(yOLOONNXPredictionOptions.BatchSize, yOLOONNXPredictionOptions_Actual!.BatchSize);
            Assert.Equal(yOLOONNXPredictionOptions.Confidence, yOLOONNXPredictionOptions_Actual.Confidence);
            Assert.Equal(yOLOONNXPredictionOptions.InputSize, yOLOONNXPredictionOptions_Actual.InputSize);
            Assert.Equal(yOLOONNXPredictionOptions.IoU, yOLOONNXPredictionOptions_Actual.IoU);
            Assert.Equal(yOLOONNXPredictionOptions.MaxDetections, yOLOONNXPredictionOptions_Actual.MaxDetections);
            Assert.Equal(yOLOONNXPredictionOptions.ModelPath, yOLOONNXPredictionOptions_Actual.ModelPath);
            Assert.Equal(yOLOONNXPredictionOptions.OutputPath, yOLOONNXPredictionOptions_Actual.OutputPath);
            Assert.Equal(yOLOONNXPredictionOptions.SourceDirectory, yOLOONNXPredictionOptions_Actual.SourceDirectory);

            Core.xUnit.Query.SerializationCheck(yOLOONNXPredictionOptions);
        }

        /// <summary>
        /// Verifies that the defaults of <see cref="Classes.YOLOONNXPredictionOptions"/> are the ones ultralytics applies, because agreement with the CPython path is what this path is for.
        /// <para>The confidence and the canvas match predict.py and the frozen weights. The overlap and the detection cap match what ultralytics applies internally without ever exposing them, which is why they are easy to get wrong here and easy to miss when they are wrong.</para>
        /// </summary>
        [Fact]
        public void YOLOONNXPredictionOptions_Defaults()
        {
            Classes.YOLOONNXPredictionOptions yOLOONNXPredictionOptions = new();

            Assert.Equal(0.1, yOLOONNXPredictionOptions.Confidence);
            Assert.Equal(0.7, yOLOONNXPredictionOptions.IoU);
            Assert.Equal(300, yOLOONNXPredictionOptions.MaxDetections);
            Assert.Equal(640, yOLOONNXPredictionOptions.InputSize);
            Assert.Null(yOLOONNXPredictionOptions.ModelPath);
            Assert.Null(yOLOONNXPredictionOptions.OutputPath);
            Assert.Null(yOLOONNXPredictionOptions.SourceDirectory);
        }

        /// <summary>
        /// Verifies that <see cref="Create.YOLOONNXPredictionOptions(string, string, string, double, double, int)"/> rejects a combination that cannot make a run rather than handing back options that fail later.
        /// </summary>
        [Fact]
        public void YOLOONNXPredictionOptions_Create()
        {
            string directory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_ONNX_Test_" + Path.GetRandomFileName());

            try
            {
                Directory.CreateDirectory(directory);

                string path_Model = Path.Combine(directory, "model.onnx");
                File.WriteAllText(path_Model, "not a model, but a file that exists");

                string path_Source = Path.Combine(directory, "input");
                Directory.CreateDirectory(path_Source);

                string path_Output = Path.Combine(directory, "output", "results.bbrf");

                Assert.Null(Create.YOLOONNXPredictionOptions(Path.Combine(directory, "missing.onnx"), path_Source, path_Output));
                Assert.Null(Create.YOLOONNXPredictionOptions(path_Model, Path.Combine(directory, "missing"), path_Output));
                Assert.Null(Create.YOLOONNXPredictionOptions(path_Model, path_Source, null));
                Assert.Null(Create.YOLOONNXPredictionOptions(path_Model, path_Source, path_Output, 1.5));
                Assert.Null(Create.YOLOONNXPredictionOptions(path_Model, path_Source, path_Output, 0.1, 1.5, 8));
                Assert.Null(Create.YOLOONNXPredictionOptions(path_Model, path_Source, path_Output, 0.1, 0.7, 0));

                Classes.YOLOONNXPredictionOptions? yOLOONNXPredictionOptions = Create.YOLOONNXPredictionOptions(path_Model, path_Source, path_Output, 0.2, 0.5, 4);
                Assert.NotNull(yOLOONNXPredictionOptions);
                Assert.Equal(0.2, yOLOONNXPredictionOptions!.Confidence);
                Assert.Equal(0.5, yOLOONNXPredictionOptions.IoU);
                Assert.Equal(4, yOLOONNXPredictionOptions.BatchSize);

                //The factory resolves the paths, so a source directory given with a trailing separator names the same run as one given without
                Classes.YOLOONNXPredictionOptions? yOLOONNXPredictionOptions_Trailing = Create.YOLOONNXPredictionOptions(path_Model, path_Source + Path.DirectorySeparatorChar, path_Output);
                Assert.NotNull(yOLOONNXPredictionOptions_Trailing);
                Assert.Equal(yOLOONNXPredictionOptions.SourceDirectory, yOLOONNXPredictionOptions_Trailing!.SourceDirectory);
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
