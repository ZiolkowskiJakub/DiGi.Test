using System;
using System.Linq;

namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.YOLOTrainingResult"/> keeps the start and output weights identities it is given, succeeds only with a confirmed output digest, survives the round trip through its string form, and clones identically.
        /// </summary>
        [Fact]
        public void YOLOTrainingResult()
        {
            DateTimeOffset start = new(2026, 9, 29, 20, 0, 0, TimeSpan.FromHours(2));
            DateTimeOffset end = start.AddHours(5);

            Classes.YOLOTrainingResult yOLOTrainingResult = new(
                0,
                @"C:\YOLO\models\base\yolo26x.pt",
                "9fdd44a31c504547ffb81d2c6d9e6dac3493c8eaa8b0398d3f43bae6c7003e92",
                Enums.ModelKind.Checkpoint,
                @"C:\YOLO\runs\detect\train9_fresh\weights\best.pt",
                118311525,
                "a9d76b442833f35a6bd38bdd4937a40209e40ecbae32121088f2e26eca10deb5",
                true,
                ["Weights: C:\\YOLO\\runs\\detect\\train9_fresh\\weights\\best.pt", "Bytes: 118311525"],
                null,
                start,
                end);

            Assert.True(yOLOTrainingResult.Succeeded);
            Assert.Equal(Enums.ModelKind.Checkpoint, yOLOTrainingResult.StartModelKind);
            Assert.Equal(118311525, yOLOTrainingResult.Bytes);
            Assert.True(yOLOTrainingResult.Amp);
            Assert.Null(yOLOTrainingResult.StandardError);
            Assert.Equal(TimeSpan.FromHours(5), yOLOTrainingResult.Duration);

            string? json = Core.Convert.ToSystem_String(yOLOTrainingResult);
            Classes.YOLOTrainingResult? yOLOTrainingResult_Actual = Core.Convert.ToDiGi<Classes.YOLOTrainingResult>(json)?.FirstOrDefault();
            Assert.NotNull(yOLOTrainingResult_Actual);
            Assert.Equal(yOLOTrainingResult.StartModelSHA256, yOLOTrainingResult_Actual!.StartModelSHA256);
            Assert.Equal(Enums.ModelKind.Checkpoint, yOLOTrainingResult_Actual.StartModelKind);
            Assert.Equal(yOLOTrainingResult.SHA256, yOLOTrainingResult_Actual.SHA256);
            Assert.Equal(118311525, yOLOTrainingResult_Actual.Bytes);
            Assert.Equal(start, yOLOTrainingResult_Actual.Start);

            Core.xUnit.Query.SerializationCheck(yOLOTrainingResult);

            //A run that wrote weights whose digest could not be confirmed does not succeed
            Classes.YOLOTrainingResult yOLOTrainingResult_Unconfirmed = new(0, null, null, Enums.ModelKind.Definition, @"C:\best.pt", 10, null, null, null, ["mismatch"], start, end);
            Assert.False(yOLOTrainingResult_Unconfirmed.Succeeded);
        }

        /// <summary>
        /// Verifies that <see cref="Classes.YOLOValidationResult"/> keeps the weights identity, split and mAP values it is given, survives the round trip through its string form, and clones identically.
        /// </summary>
        [Fact]
        public void YOLOValidationResult()
        {
            DateTimeOffset start = new(2026, 9, 30, 8, 0, 0, TimeSpan.FromHours(2));

            Classes.YOLOValidationResult yOLOValidationResult = new(
                0,
                @"C:\YOLO\models\model.pt",
                "c79d2edfb776c913494d698cd1899ed1a6c92d7caad594b674d39ce5ada7ea38",
                Enums.Category.Test,
                0.9385,
                0.69585,
                ["mAP50: 0.9385", "mAP50-95: 0.69585"],
                null,
                start,
                start.AddMinutes(3));

            Assert.True(yOLOValidationResult.Succeeded);
            Assert.Equal(Enums.Category.Test, yOLOValidationResult.Split);

            string? json = Core.Convert.ToSystem_String(yOLOValidationResult);
            Classes.YOLOValidationResult? yOLOValidationResult_Actual = Core.Convert.ToDiGi<Classes.YOLOValidationResult>(json)?.FirstOrDefault();
            Assert.NotNull(yOLOValidationResult_Actual);
            Assert.Equal(0.9385, yOLOValidationResult_Actual!.MAP50);
            Assert.Equal(0.69585, yOLOValidationResult_Actual.MAP50_95);
            Assert.Equal(yOLOValidationResult.ModelSHA256, yOLOValidationResult_Actual.ModelSHA256);

            Core.xUnit.Query.SerializationCheck(yOLOValidationResult);

            Assert.False(new Classes.YOLOValidationResult(0, null, null, Enums.Category.Test, 0.5, null, null, null, start, start).Succeeded);
        }
    }
}
