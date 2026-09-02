using System.Linq;

namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.YOLOPredictionOptions"/> keeps the values it is given, survives the round trip through its string form, and clones identically.
        /// </summary>
        [Fact]
        public void YOLOPredictionOptions()
        {
            Classes.YOLOPredictionOptions yOLOPredictionOptions = new()
            {
                BatchSize = 16,
                Confidence = 0.25,
                ModelPath = @"C:\YOLO\models\best.pt",
                OutputPath = @"C:\YOLO\output\results.bbrf",
                PythonPath = @"C:\Python\python.exe",
                SourceDirectory = @"C:\YOLO\input",
                WorkingDirectory = null
            };

            Assert.Equal(16, yOLOPredictionOptions.BatchSize);
            Assert.Equal(0.25, yOLOPredictionOptions.Confidence);
            Assert.Equal(@"C:\YOLO\models\best.pt", yOLOPredictionOptions.ModelPath);
            Assert.Equal(@"C:\YOLO\output\results.bbrf", yOLOPredictionOptions.OutputPath);
            Assert.Equal(@"C:\Python\python.exe", yOLOPredictionOptions.PythonPath);
            Assert.Equal(@"C:\YOLO\input", yOLOPredictionOptions.SourceDirectory);
            Assert.Null(yOLOPredictionOptions.WorkingDirectory);

            string? json = Core.Convert.ToSystem_String(yOLOPredictionOptions);
            Assert.False(string.IsNullOrWhiteSpace(json));

            Classes.YOLOPredictionOptions? yOLOPredictionOptions_Actual = Core.Convert.ToDiGi<Classes.YOLOPredictionOptions>(json)?.FirstOrDefault();
            Assert.NotNull(yOLOPredictionOptions_Actual);
            Assert.Equal(yOLOPredictionOptions.BatchSize, yOLOPredictionOptions_Actual!.BatchSize);
            Assert.Equal(yOLOPredictionOptions.Confidence, yOLOPredictionOptions_Actual.Confidence);
            Assert.Equal(yOLOPredictionOptions.ModelPath, yOLOPredictionOptions_Actual.ModelPath);
            Assert.Equal(yOLOPredictionOptions.SourceDirectory, yOLOPredictionOptions_Actual.SourceDirectory);
            Assert.Null(yOLOPredictionOptions_Actual.WorkingDirectory);

            Core.xUnit.Query.SerializationCheck(yOLOPredictionOptions);
        }

        /// <summary>
        /// Verifies that the default values of <see cref="Classes.YOLOPredictionOptions"/> match the defaults predict.py applies when the matching argument is not passed.
        /// </summary>
        [Fact]
        public void YOLOPredictionOptions_Defaults()
        {
            Classes.YOLOPredictionOptions yOLOPredictionOptions = new();

            Assert.Equal(32, yOLOPredictionOptions.BatchSize);
            Assert.Equal(0.1, yOLOPredictionOptions.Confidence);
            Assert.Null(yOLOPredictionOptions.ModelPath);
            Assert.Null(yOLOPredictionOptions.OutputPath);
            Assert.Null(yOLOPredictionOptions.PythonPath);
            Assert.Null(yOLOPredictionOptions.SourceDirectory);
            Assert.Null(yOLOPredictionOptions.WorkingDirectory);
        }

        /// <summary>
        /// Verifies that <see cref="Create.YOLOPredictionOptions(string?, string?, string?, string?, string?, double, int)"/> validates the batch size.
        /// </summary>
        [Fact]
        public void YOLOPredictionOptions_Create()
        {
            string directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "DiGi_YOLO_Test_" + System.IO.Path.GetRandomFileName());

            try
            {
                System.IO.Directory.CreateDirectory(directory);
                string path_Model = System.IO.Path.Combine(directory, "best.pt");
                System.IO.File.WriteAllText(path_Model, "dummy");

                string path_Source = System.IO.Path.Combine(directory, "input");
                System.IO.Directory.CreateDirectory(path_Source);

                string path_Output = System.IO.Path.Combine(directory, "output", "results.bbrf");

                string? pythonPath = Query.PythonPaths().FirstOrDefault();
                if (string.IsNullOrWhiteSpace(pythonPath))
                {
                    return;
                }

                Assert.Null(Create.YOLOPredictionOptions(pythonPath, path_Model, path_Source, path_Output, null, 0.1, 0));
                Assert.Null(Create.YOLOPredictionOptions(pythonPath, path_Model, path_Source, path_Output, null, 0.1, -5));

                Classes.YOLOPredictionOptions? yOLOPredictionOptions = Create.YOLOPredictionOptions(pythonPath, path_Model, path_Source, path_Output, null, 0.1, 16);
                Assert.NotNull(yOLOPredictionOptions);
                Assert.Equal(16, yOLOPredictionOptions!.BatchSize);
            }
            finally
            {
                if (System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.Delete(directory, true);
                }
            }
        }
    }
}
