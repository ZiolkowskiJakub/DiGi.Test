using System;
using System.Linq;

namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.YOLOPredictionResult"/> keeps the values it is given, survives the round trip through its string form, and clones identically.
        /// <para>The timestamps are held as <see cref="DateTimeOffset"/> so that the offset survives the round trip; a <see cref="DateTime"/> comes back with a local offset and fails the check.</para>
        /// </summary>
        [Fact]
        public void YOLOPredictionResult()
        {
            DateTimeOffset start = new(2026, 8, 28, 9, 15, 0, TimeSpan.FromHours(2));
            DateTimeOffset end = start.AddMinutes(12);

            Classes.YOLOPredictionResult yOLOPredictionResult = new(
                0,
                3,
                @"C:\YOLO\output\results.bbrf",
                ["0207_2021\t0\t12.5\t20.25\t40.5\t60.75\t0.93", "0208_2021"],
                ["Processing: 0207_2021.jpeg", "Processing: 0208_2021.jpeg"],
                null,
                start,
                end);

            Assert.Equal(0, yOLOPredictionResult.ExitCode);
            Assert.Equal(3, yOLOPredictionResult.ImageCount);
            Assert.True(yOLOPredictionResult.Succeeded);
            Assert.Equal(@"C:\YOLO\output\results.bbrf", yOLOPredictionResult.OutputPath);
            Assert.Equal(2, yOLOPredictionResult.Values?.Count);
            Assert.Equal(2, yOLOPredictionResult.StandardOutput?.Count);
            Assert.Null(yOLOPredictionResult.StandardError);
            Assert.Equal(start, yOLOPredictionResult.Start);
            Assert.Equal(end, yOLOPredictionResult.End);
            Assert.Equal(TimeSpan.FromMinutes(12), yOLOPredictionResult.Duration);

            //The line holding only an image name records an image with no detections and is not a detection itself
            Classes.BoundingBoxResultFile? boundingBoxResultFile = Create.BoundingBoxResultFile(yOLOPredictionResult);
            Assert.NotNull(boundingBoxResultFile);
            Assert.Single(boundingBoxResultFile!);
            Assert.Equal(0.93, boundingBoxResultFile![0].Confidence);

            string? json = Core.Convert.ToSystem_String(yOLOPredictionResult);
            Assert.False(string.IsNullOrWhiteSpace(json));

            Classes.YOLOPredictionResult? yOLOPredictionResult_Actual = Core.Convert.ToDiGi<Classes.YOLOPredictionResult>(json)?.FirstOrDefault();
            Assert.NotNull(yOLOPredictionResult_Actual);
            Assert.Equal(start, yOLOPredictionResult_Actual!.Start);
            Assert.Equal(end, yOLOPredictionResult_Actual.End);
            Assert.Equal(yOLOPredictionResult.Values, yOLOPredictionResult_Actual.Values);

            Core.xUnit.Query.SerializationCheck(yOLOPredictionResult);
        }

        /// <summary>
        /// Verifies that a run which did not complete yields no detections, so that a failed run cannot be read as a run that found nothing.
        /// </summary>
        [Fact]
        public void YOLOPredictionResult_Failed()
        {
            Classes.YOLOPredictionResult yOLOPredictionResult = new(1, 3, @"C:\YOLO\output\results.bbrf", null, null, ["ModuleNotFoundError: No module named 'ultralytics'"], DateTimeOffset.Now, DateTimeOffset.Now);

            Assert.False(yOLOPredictionResult.Succeeded);
            Assert.Null(Create.BoundingBoxResultFile(yOLOPredictionResult));

            Core.xUnit.Query.SerializationCheck(yOLOPredictionResult);
        }
    }
}
