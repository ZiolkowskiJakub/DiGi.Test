using System;
using System.Linq;

namespace DiGi.YOLO.ONNX.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.YOLOONNXPredictionResult"/> keeps the values it is given, survives the round trip through its string form, and clones identically.
        /// <para>The timestamps are held as <see cref="DateTimeOffset"/> so that the offset survives the round trip; a <see cref="DateTime"/> comes back with a local offset and fails the check.</para>
        /// </summary>
        [Fact]
        public void YOLOONNXPredictionResult()
        {
            DateTimeOffset start = new(2026, 9, 3, 9, 15, 0, TimeSpan.FromHours(2));
            DateTimeOffset end = start.AddMinutes(7);

            Classes.YOLOONNXPredictionResult yOLOONNXPredictionResult = new(3, @"C:\YOLO\output\results.bbrf", ["0207_2021\t0\t12.5\t20.25\t40.5\t60.75\t0.93", "0208_2021"], ["Image could not be decoded: 0209_2021.jpeg"], start, end);

            Assert.Equal(3, yOLOONNXPredictionResult.ImageCount);
            Assert.True(yOLOONNXPredictionResult.Succeeded);
            Assert.Equal(@"C:\YOLO\output\results.bbrf", yOLOONNXPredictionResult.OutputPath);
            Assert.Equal(2, yOLOONNXPredictionResult.Values?.Count);
            Assert.Single(yOLOONNXPredictionResult.Messages!);
            Assert.Equal(start, yOLOONNXPredictionResult.Start);
            Assert.Equal(end, yOLOONNXPredictionResult.End);
            Assert.Equal(TimeSpan.FromMinutes(7), yOLOONNXPredictionResult.Duration);

            //The line holding only an image name records an image with no detections and is not a detection itself, exactly as the CPython path writes it
            DiGi.YOLO.Classes.BoundingBoxResultFile? boundingBoxResultFile = Create.BoundingBoxResultFile(yOLOONNXPredictionResult);
            Assert.NotNull(boundingBoxResultFile);
            Assert.Single(boundingBoxResultFile!);
            Assert.Equal(0.93, boundingBoxResultFile![0].Confidence);
            Assert.Equal(40.5, boundingBoxResultFile[0].Width);

            string? json = Core.Convert.ToSystem_String(yOLOONNXPredictionResult);
            Assert.False(string.IsNullOrWhiteSpace(json));

            Classes.YOLOONNXPredictionResult? yOLOONNXPredictionResult_Actual = Core.Convert.ToDiGi<Classes.YOLOONNXPredictionResult>(json)?.FirstOrDefault();
            Assert.NotNull(yOLOONNXPredictionResult_Actual);
            Assert.Equal(start, yOLOONNXPredictionResult_Actual!.Start);
            Assert.Equal(end, yOLOONNXPredictionResult_Actual.End);
            Assert.Equal(yOLOONNXPredictionResult.ImageCount, yOLOONNXPredictionResult_Actual.ImageCount);
            Assert.Equal(yOLOONNXPredictionResult.Values, yOLOONNXPredictionResult_Actual.Values);
            Assert.Equal(yOLOONNXPredictionResult.Messages, yOLOONNXPredictionResult_Actual.Messages);

            Core.xUnit.Query.SerializationCheck(yOLOONNXPredictionResult);
        }

        /// <summary>
        /// Verifies that a run which did not complete yields no detections, so that a failed run cannot be read as a run that found nothing.
        /// <para>The two are told apart by <see cref="Classes.YOLOONNXPredictionResult.Values"/> being null rather than empty, which is the same distinction the CPython path draws.</para>
        /// </summary>
        [Fact]
        public void YOLOONNXPredictionResult_Failed()
        {
            DateTimeOffset start = DateTimeOffset.Now;

            Classes.YOLOONNXPredictionResult yOLOONNXPredictionResult = new(12, @"C:\YOLO\output\results.bbrf", null, ["Model could not be loaded: invalid protobuf"], start, start.AddSeconds(1));

            Assert.False(yOLOONNXPredictionResult.Succeeded);
            Assert.Null(yOLOONNXPredictionResult.Values);
            Assert.Null(Create.BoundingBoxResultFile(yOLOONNXPredictionResult));

            Classes.YOLOONNXPredictionResult yOLOONNXPredictionResult_Empty = new(0, @"C:\YOLO\output\results.bbrf", [], null, start, start.AddSeconds(1));

            Assert.True(yOLOONNXPredictionResult_Empty.Succeeded);
            Assert.Empty(Create.BoundingBoxResultFile(yOLOONNXPredictionResult_Empty)!);
        }
    }
}
