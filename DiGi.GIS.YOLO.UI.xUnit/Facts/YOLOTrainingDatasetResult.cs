using System;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that YOLOTrainingDatasetResult and the per-county counts it carries keep their values, survive the string form, clone identically, and that the copy constructors copy every member.
        /// <para>Every tally of the count is set to a distinct value, so a tally a copy constructor or <see cref="Modify.Add"/> forgot shows up as a mismatch rather than as a coincidental zero.</para>
        /// </summary>
        [Fact]
        public void YOLOTrainingDatasetResult()
        {
            Classes.YOLOTrainingDatasetCount yOLOTrainingDatasetCount = new(73485);

            List<System.Reflection.PropertyInfo> propertyInfos = [.. typeof(Classes.YOLOTrainingDatasetCount).GetProperties().Where(x => x.PropertyType == typeof(long))];
            Assert.True(propertyInfos.Count > 20);

            for (int i = 0; i < propertyInfos.Count; i++)
            {
                propertyInfos[i].SetValue(yOLOTrainingDatasetCount, (long)(i + 1));
            }

            Classes.YOLOTrainingDatasetCount yOLOTrainingDatasetCount_Copy = new(yOLOTrainingDatasetCount);
            Assert.Equal(73485, yOLOTrainingDatasetCount_Copy.CountyId);
            foreach (System.Reflection.PropertyInfo propertyInfo in propertyInfos)
            {
                Assert.Equal(propertyInfo.GetValue(yOLOTrainingDatasetCount), propertyInfo.GetValue(yOLOTrainingDatasetCount_Copy));
            }

            Core.xUnit.Query.SerializationCheck(yOLOTrainingDatasetCount);

            // Adding a count to an empty total gives the count back, tally by tally; the total keeps its own county.
            Classes.YOLOTrainingDatasetCount total = new((int?)null);
            Assert.True(total.Add(yOLOTrainingDatasetCount));
            Assert.True(total.Add(yOLOTrainingDatasetCount));
            Assert.Null(total.CountyId);
            foreach (System.Reflection.PropertyInfo propertyInfo in propertyInfos)
            {
                Assert.Equal(2 * (long)propertyInfo.GetValue(yOLOTrainingDatasetCount)!, (long)propertyInfo.GetValue(total)!);
            }

            Classes.YOLOTrainingDatasetResult yOLOTrainingDatasetResult = new([73485], @"C:\YOLO\dataset", true, total, [yOLOTrainingDatasetCount], [nameof(Query.UserYearBuiltsAsync)], ["message"], new DateTimeOffset(2026, 9, 29, 10, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 9, 29, 11, 0, 0, TimeSpan.Zero), true);

            void AssertMembers(Classes.YOLOTrainingDatasetResult? yOLOTrainingDatasetResult_Actual)
            {
                Assert.NotNull(yOLOTrainingDatasetResult_Actual);
                Assert.Equal(73485, Assert.Single(yOLOTrainingDatasetResult_Actual!.CountyIds));
                Assert.Equal(@"C:\YOLO\dataset", yOLOTrainingDatasetResult_Actual.OutputDirectory);
                Assert.True(yOLOTrainingDatasetResult_Actual.CountOnly);
                Assert.True(yOLOTrainingDatasetResult_Actual.Cancelled);
                Assert.Equal(TimeSpan.FromHours(1), yOLOTrainingDatasetResult_Actual.Duration);
                Assert.Equal(nameof(Query.UserYearBuiltsAsync), Assert.Single(yOLOTrainingDatasetResult_Actual.FailedStepNames));
                Assert.Equal("message", Assert.Single(yOLOTrainingDatasetResult_Actual.Messages));
                Assert.Equal(total.ImageCount, yOLOTrainingDatasetResult_Actual.Total!.ImageCount);
                Assert.Single(yOLOTrainingDatasetResult_Actual.YOLOTrainingDatasetCounts);
                Assert.Equal(yOLOTrainingDatasetCount.LegacyUnknownCount, yOLOTrainingDatasetResult_Actual.YOLOTrainingDatasetCounts[0].LegacyUnknownCount);
            }

            AssertMembers(yOLOTrainingDatasetResult);
            AssertMembers(Core.Convert.ToDiGi<Classes.YOLOTrainingDatasetResult>(Core.Convert.ToSystem_String(yOLOTrainingDatasetResult))?.FirstOrDefault());
            AssertMembers(new Classes.YOLOTrainingDatasetResult(yOLOTrainingDatasetResult));

            Core.xUnit.Query.SerializationCheck(yOLOTrainingDatasetResult);
        }

        /// <summary>
        /// Verifies that YOLOLabelCheckResult keeps its values, survives the string form, clones identically, and that its copy constructor copies every member.
        /// </summary>
        [Fact]
        public void YOLOLabelCheckResult()
        {
            Classes.YOLOLabelCheckResult yOLOLabelCheckResult = new(500, 480, 0.71, 0.74, 0.83, new Dictionary<string, double>() { ["73485"] = 0.7, ["73482"] = 0.72 }, @"C:\reports\label_check", ["step"], ["message"], new DateTimeOffset(2026, 9, 29, 10, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 9, 29, 10, 5, 0, TimeSpan.Zero));

            void AssertMembers(Classes.YOLOLabelCheckResult? yOLOLabelCheckResult_Actual)
            {
                Assert.NotNull(yOLOLabelCheckResult_Actual);
                Assert.Equal(500, yOLOLabelCheckResult_Actual!.SampleCount);
                Assert.Equal(480, yOLOLabelCheckResult_Actual.DetectedCount);
                Assert.Equal(0.71, yOLOLabelCheckResult_Actual.MeanIntersectionOverUnion);
                Assert.Equal(0.74, yOLOLabelCheckResult_Actual.MedianIntersectionOverUnion);
                Assert.Equal(0.83, yOLOLabelCheckResult_Actual.ShareAboveHalf);
                Assert.Equal(2, yOLOLabelCheckResult_Actual.CountyIntersectionOverUnions.Count);
                Assert.Equal(0.72, yOLOLabelCheckResult_Actual.CountyIntersectionOverUnions["73482"]);
                Assert.Equal(@"C:\reports\label_check", yOLOLabelCheckResult_Actual.OverlayDirectory);
                Assert.Equal("step", Assert.Single(yOLOLabelCheckResult_Actual.FailedStepNames));
                Assert.Equal("message", Assert.Single(yOLOLabelCheckResult_Actual.Messages));
                Assert.Equal(new DateTimeOffset(2026, 9, 29, 10, 0, 0, TimeSpan.Zero), yOLOLabelCheckResult_Actual.Start);
                Assert.Equal(new DateTimeOffset(2026, 9, 29, 10, 5, 0, TimeSpan.Zero), yOLOLabelCheckResult_Actual.End);
            }

            AssertMembers(yOLOLabelCheckResult);
            AssertMembers(Core.Convert.ToDiGi<Classes.YOLOLabelCheckResult>(Core.Convert.ToSystem_String(yOLOLabelCheckResult))?.FirstOrDefault());
            AssertMembers(new Classes.YOLOLabelCheckResult(yOLOLabelCheckResult));

            Core.xUnit.Query.SerializationCheck(yOLOLabelCheckResult);
        }

        /// <summary>
        /// Verifies that YOLODetectorEvaluationResult and its rows keep their values, survive the string form, clone identically, and that the copy constructors copy every member.
        /// </summary>
        [Fact]
        public void YOLODetectorEvaluationResult()
        {
            Classes.YOLODetectorEvaluation yOLODetectorEvaluation = new(@"C:\YOLO\models\model.pt", "9fdd44a3", Enums.YOLODetectorEvaluationSubset.Clean, 812, 1.25, 2.5, 0.4);

            Classes.YOLODetectorEvaluation yOLODetectorEvaluation_Copy = new(yOLODetectorEvaluation);
            Assert.Equal(yOLODetectorEvaluation.WeightsPath, yOLODetectorEvaluation_Copy.WeightsPath);
            Assert.Equal(yOLODetectorEvaluation.SHA256, yOLODetectorEvaluation_Copy.SHA256);
            Assert.Equal(yOLODetectorEvaluation.Subset, yOLODetectorEvaluation_Copy.Subset);
            Assert.Equal(yOLODetectorEvaluation.Count, yOLODetectorEvaluation_Copy.Count);
            Assert.Equal(yOLODetectorEvaluation.MeanAbsoluteError, yOLODetectorEvaluation_Copy.MeanAbsoluteError);
            Assert.Equal(yOLODetectorEvaluation.RootMeanSquareError, yOLODetectorEvaluation_Copy.RootMeanSquareError);
            Assert.Equal(yOLODetectorEvaluation.ExactShare, yOLODetectorEvaluation_Copy.ExactShare);

            Core.xUnit.Query.SerializationCheck(yOLODetectorEvaluation);

            Classes.YOLODetectorEvaluationResult yOLODetectorEvaluationResult = new(@"C:\YOLO\dataset", [yOLODetectorEvaluation], ["step"], ["message"], new DateTimeOffset(2026, 9, 29, 10, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 9, 29, 10, 5, 0, TimeSpan.Zero));

            void AssertMembers(Classes.YOLODetectorEvaluationResult? yOLODetectorEvaluationResult_Actual)
            {
                Assert.NotNull(yOLODetectorEvaluationResult_Actual);
                Assert.Equal(@"C:\YOLO\dataset", yOLODetectorEvaluationResult_Actual!.OutputDirectory);
                Assert.Single(yOLODetectorEvaluationResult_Actual.YOLODetectorEvaluations);
                Assert.Equal(Enums.YOLODetectorEvaluationSubset.Clean, yOLODetectorEvaluationResult_Actual.YOLODetectorEvaluations[0].Subset);
                Assert.Equal(812, yOLODetectorEvaluationResult_Actual.YOLODetectorEvaluations[0].Count);
                Assert.Equal("step", Assert.Single(yOLODetectorEvaluationResult_Actual.FailedStepNames));
                Assert.Equal("message", Assert.Single(yOLODetectorEvaluationResult_Actual.Messages));
                Assert.Equal(new DateTimeOffset(2026, 9, 29, 10, 0, 0, TimeSpan.Zero), yOLODetectorEvaluationResult_Actual.Start);
                Assert.Equal(new DateTimeOffset(2026, 9, 29, 10, 5, 0, TimeSpan.Zero), yOLODetectorEvaluationResult_Actual.End);
            }

            AssertMembers(yOLODetectorEvaluationResult);
            AssertMembers(Core.Convert.ToDiGi<Classes.YOLODetectorEvaluationResult>(Core.Convert.ToSystem_String(yOLODetectorEvaluationResult))?.FirstOrDefault());
            AssertMembers(new Classes.YOLODetectorEvaluationResult(yOLODetectorEvaluationResult));

            Core.xUnit.Query.SerializationCheck(yOLODetectorEvaluationResult);
        }
    }
}
