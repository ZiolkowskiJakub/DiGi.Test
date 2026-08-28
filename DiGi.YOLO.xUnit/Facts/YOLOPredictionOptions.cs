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
                Confidence = 0.25,
                ModelPath = @"C:\YOLO\models\best.pt",
                OutputPath = @"C:\YOLO\output\results.bbrf",
                PythonPath = @"C:\Python\python.exe",
                SourceDirectory = @"C:\YOLO\input",
                WorkingDirectory = null
            };

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
            Assert.Equal(yOLOPredictionOptions.Confidence, yOLOPredictionOptions_Actual!.Confidence);
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

            Assert.Equal(0.1, yOLOPredictionOptions.Confidence);
            Assert.Null(yOLOPredictionOptions.ModelPath);
            Assert.Null(yOLOPredictionOptions.OutputPath);
            Assert.Null(yOLOPredictionOptions.PythonPath);
            Assert.Null(yOLOPredictionOptions.SourceDirectory);
            Assert.Null(yOLOPredictionOptions.WorkingDirectory);
        }
    }
}
