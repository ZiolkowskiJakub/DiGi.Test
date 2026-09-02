using DiGi.GIS.YOLO.UI.Classes;
using System;
using System.Linq;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that YearBuiltPredictionResult keeps the tallies it is given, survives the round trip through its string form, and clones identically.
        /// <para>The timestamps are the part worth pinning: they are stored as offsets rather than plain date times, so the round trip has to return the same instant rather than one shifted by the machine's offset from UTC.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictionResult()
        {
            DateTimeOffset start = new(2026, 9, 1, 8, 30, 0, TimeSpan.FromHours(2));
            DateTimeOffset end = start.AddMinutes(45);

            Classes.YearBuiltPredictionResult yearBuiltPredictionResult = new([73485, 73482], start, start, end, 1200, 3400, 900, 900, 880, 880, 1760, ["ExportPredictionImagesAsync"], ["ultralytics is not installed"], false);

            Assert.Equal(2, yearBuiltPredictionResult.CountyIds.Count);
            Assert.Equal(1200, yearBuiltPredictionResult.ImageCount);
            Assert.Equal(3400, yearBuiltPredictionResult.DetectionCount);
            Assert.Equal(900, yearBuiltPredictionResult.BuildingCount);
            Assert.Equal(900, yearBuiltPredictionResult.FeatureRowCount);
            Assert.Equal(880, yearBuiltPredictionResult.PredictionCount);
            Assert.Equal(880, yearBuiltPredictionResult.YearBuiltDataUpdatedCount);
            Assert.Equal(1760, yearBuiltPredictionResult.BuildingDataUpdatedCount);
            Assert.Single(yearBuiltPredictionResult.FailedStepNames);
            Assert.Single(yearBuiltPredictionResult.Messages);
            Assert.False(yearBuiltPredictionResult.Cancelled);
            Assert.Equal(TimeSpan.FromMinutes(45), yearBuiltPredictionResult.Duration);

            string? json = Core.Convert.ToSystem_String(yearBuiltPredictionResult);
            Assert.False(string.IsNullOrWhiteSpace(json));

            Classes.YearBuiltPredictionResult? yearBuiltPredictionResult_Actual = Core.Convert.ToDiGi<Classes.YearBuiltPredictionResult>(json)?.FirstOrDefault();
            Assert.NotNull(yearBuiltPredictionResult_Actual);
            Assert.Equal(yearBuiltPredictionResult.DetectionCount, yearBuiltPredictionResult_Actual!.DetectionCount);
            Assert.Equal(yearBuiltPredictionResult.BuildingDataUpdatedCount, yearBuiltPredictionResult_Actual.BuildingDataUpdatedCount);
            Assert.Equal(yearBuiltPredictionResult.CountyIds, yearBuiltPredictionResult_Actual.CountyIds);
            Assert.Equal(yearBuiltPredictionResult.FailedStepNames, yearBuiltPredictionResult_Actual.FailedStepNames);
            Assert.Equal(yearBuiltPredictionResult.Messages, yearBuiltPredictionResult_Actual.Messages);

            //The same instant, whatever offset it comes back carrying
            Assert.NotNull(yearBuiltPredictionResult_Actual.RunTimestamp);
            Assert.Equal(start.UtcDateTime, yearBuiltPredictionResult_Actual.RunTimestamp!.Value.UtcDateTime);
            Assert.NotNull(yearBuiltPredictionResult_Actual.End);
            Assert.Equal(end.UtcDateTime, yearBuiltPredictionResult_Actual.End!.Value.UtcDateTime);
            Assert.Equal(yearBuiltPredictionResult.Duration, yearBuiltPredictionResult_Actual.Duration);

            Core.xUnit.Query.SerializationCheck(yearBuiltPredictionResult);
        }

        /// <summary>
        /// Verifies that a run reporting nothing carries empty collections rather than nulls, so a caller can read the tallies of a run that did nothing without guarding every one of them.
        /// </summary>
        [Fact]
        public void YearBuiltPredictionResult_Empty()
        {
            Classes.YearBuiltPredictionResult yearBuiltPredictionResult = new(null, null, null, null, 0, 0, 0, 0, 0, 0, 0, null, null, true);

            Assert.Empty(yearBuiltPredictionResult.CountyIds);
            Assert.Empty(yearBuiltPredictionResult.FailedStepNames);
            Assert.Empty(yearBuiltPredictionResult.Messages);
            Assert.Null(yearBuiltPredictionResult.RunTimestamp);
            Assert.Null(yearBuiltPredictionResult.Duration);
            Assert.True(yearBuiltPredictionResult.Cancelled);

            Core.xUnit.Query.SerializationCheck(yearBuiltPredictionResult);
        }
    }
}
