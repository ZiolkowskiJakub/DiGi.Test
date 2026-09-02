using DiGi.GIS.Classes;
using DiGi.GIS.ML;
using System;
using System.Collections.Generic;

namespace DiGi.GIS.ML.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the training labels are taken from the ground-truth entry of the stored year built data and never from a stored prediction.
        /// <para>Every record on the counties this model trains on carries the previous model's answer alongside the user supplied year, and the two disagree on roughly a quarter of them. A label taken from the prediction would train the regressor on its own predecessor, which shows up as accuracy rather than as a defect, so it is asserted rather than assumed.</para>
        /// </summary>
        [Fact]
        public void YearBuiltLabels_ExcludesStoredPredictions()
        {
            // The shape actually stored: a user year, and a prediction from the incumbent model disagreeing with it.
            YearBuiltData yearBuiltData_Both = new("REF-BOTH");
            Assert.True(yearBuiltData_Both.Add(new UserYearBuilt(1975)));
            Assert.True(yearBuiltData_Both.Add(new PredictedYearBuilt(new DateTime(2025, 5, 29, 7, 41, 47, DateTimeKind.Utc), 2008)));

            // A building the pipeline has scored but nobody has confirmed. It is unlabelled, not labelled zero.
            YearBuiltData yearBuiltData_PredictionOnly = new("REF-PREDICTION");
            Assert.True(yearBuiltData_PredictionOnly.Add(new PredictedYearBuilt(new DateTime(2025, 5, 29, 7, 41, 47, DateTimeKind.Utc), 2008)));

            YearBuiltData yearBuiltData_UserOnly = new("REF-USER");
            Assert.True(yearBuiltData_UserOnly.Add(new UserYearBuilt(1932)));

            YearBuiltData yearBuiltData_Empty = new("REF-EMPTY");

            List<YearBuiltData?> yearBuiltDatas = [yearBuiltData_Both, yearBuiltData_PredictionOnly, yearBuiltData_UserOnly, yearBuiltData_Empty, null];

            Dictionary<string, short> labels = yearBuiltDatas.YearBuiltLabels();

            Assert.Equal(2, labels.Count);

            Assert.True(labels.ContainsKey("REF-BOTH"));
            Assert.Equal((short)1975, labels["REF-BOTH"]);

            Assert.True(labels.ContainsKey("REF-USER"));
            Assert.Equal((short)1932, labels["REF-USER"]);

            Assert.False(labels.ContainsKey("REF-PREDICTION"));
            Assert.False(labels.ContainsKey("REF-EMPTY"));

            Assert.Empty(Query.YearBuiltLabels(null));
        }
    }
}
