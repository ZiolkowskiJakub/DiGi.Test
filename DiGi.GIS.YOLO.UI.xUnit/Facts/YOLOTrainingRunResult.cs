using System;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that YOLOTrainingRunResult keeps every value it is given, survives its string form, clones identically and that its copy constructor copies every member.
        /// <para>The digests are strings and the counts are plain numbers, so the round trip through text is exact; a numeric member held as <c>object</c> would not be.</para>
        /// </summary>
        [Fact]
        public void YOLOTrainingRunResult()
        {
            DateTimeOffset start = new(2026, 9, 30, 8, 0, 0, TimeSpan.FromHours(2));
            DateTimeOffset end = start.AddHours(3);

            Classes.YOLODetectorEvaluation yOLODetectorEvaluation = new(@"C:\YOLO\runs\train9_fresh\train9_fresh.pt", new string('b', 64), Enums.YOLODetectorEvaluationSubset.Clean, 120, 1.5, 2.25, 0.4);

            Classes.YOLOTrainingRunResult yOLOTrainingRunResult = new("train9_fresh", @"C:\YOLO\models\model.pt", new string('a', 64), @"C:\YOLO\runs\train9_fresh\train9_fresh.pt", new string('b', 64), 0.75, 0.5, [yOLODetectorEvaluation], false, ["YOLOTrainingStep.Validate"], ["a note"], start, end, true, 7);

            void AssertMembers(Classes.YOLOTrainingRunResult? yOLOTrainingRunResult_Actual)
            {
                Assert.NotNull(yOLOTrainingRunResult_Actual);
                Assert.Equal("train9_fresh", yOLOTrainingRunResult_Actual!.RunName);
                Assert.Equal(@"C:\YOLO\models\model.pt", yOLOTrainingRunResult_Actual.StartWeightsPath);
                Assert.Equal(new string('a', 64), yOLOTrainingRunResult_Actual.StartWeightsSHA256);
                Assert.Equal(@"C:\YOLO\runs\train9_fresh\train9_fresh.pt", yOLOTrainingRunResult_Actual.WeightsPath);
                Assert.Equal(new string('b', 64), yOLOTrainingRunResult_Actual.WeightsSHA256);
                Assert.Equal(0.75, yOLOTrainingRunResult_Actual.MAP50);
                Assert.Equal(0.5, yOLOTrainingRunResult_Actual.MAP50_95);
                Assert.False(yOLOTrainingRunResult_Actual.Cancelled);
                Assert.True(yOLOTrainingRunResult_Actual.Resumed);
                Assert.Equal(7, yOLOTrainingRunResult_Actual.ResumedFromEpoch);
                Assert.Equal(["YOLOTrainingStep.Validate"], yOLOTrainingRunResult_Actual.FailedStepNames);
                Assert.Equal(["a note"], yOLOTrainingRunResult_Actual.Messages);
                Assert.Equal(start, yOLOTrainingRunResult_Actual.Start);
                Assert.Equal(end, yOLOTrainingRunResult_Actual.End);
                Assert.Single(yOLOTrainingRunResult_Actual.YOLODetectorEvaluations);
                Assert.Equal(new string('b', 64), yOLOTrainingRunResult_Actual.YOLODetectorEvaluations[0].SHA256);
            }

            AssertMembers(yOLOTrainingRunResult);

            string? json = Core.Convert.ToSystem_String(yOLOTrainingRunResult);
            Assert.False(string.IsNullOrWhiteSpace(json));

            AssertMembers(Core.Convert.ToDiGi<Classes.YOLOTrainingRunResult>(json)?.FirstOrDefault());
            AssertMembers(new Classes.YOLOTrainingRunResult(yOLOTrainingRunResult));

            Core.xUnit.Query.SerializationCheck(yOLOTrainingRunResult);
        }
    }
}
