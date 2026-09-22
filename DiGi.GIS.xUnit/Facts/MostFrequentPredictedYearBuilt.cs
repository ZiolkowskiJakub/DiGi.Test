using System;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the most frequent predicted year built wins over the newest one.
        /// </summary>
        [Fact]
        public void MostFrequentPredictedYearBuilt_Majority()
        {
            Classes.YearBuiltData yearBuiltData_2001_Older = new("b_ref_001");
            yearBuiltData_2001_Older.SetPredictedYearBuilt(new DateTime(2025, 1, 1), 2001);

            Classes.YearBuiltData yearBuiltData_2001_Newer = new("b_ref_001");
            yearBuiltData_2001_Newer.SetPredictedYearBuilt(new DateTime(2025, 6, 1), 2001);

            Classes.YearBuiltData yearBuiltData_1998 = new("b_ref_001");
            yearBuiltData_1998.SetPredictedYearBuilt(new DateTime(2026, 1, 1), 1998);

            Classes.PredictedYearBuilt? predictedYearBuilt = Query.MostFrequentPredictedYearBuilt([yearBuiltData_2001_Older, yearBuiltData_2001_Newer, yearBuiltData_1998]);
            Assert.NotNull(predictedYearBuilt);
            Assert.Equal((short)2001, predictedYearBuilt!.Year);
        }

        /// <summary>
        /// Verifies that a count tie goes to the year whose newest prediction is the most recent.
        /// </summary>
        [Fact]
        public void MostFrequentPredictedYearBuilt_TieNewest()
        {
            Classes.YearBuiltData yearBuiltData_2001 = new("b_ref_001");
            yearBuiltData_2001.SetPredictedYearBuilt(new DateTime(2025, 1, 1), 2001);

            Classes.YearBuiltData yearBuiltData_1998 = new("b_ref_001");
            yearBuiltData_1998.SetPredictedYearBuilt(new DateTime(2026, 1, 1), 1998);

            Classes.PredictedYearBuilt? predictedYearBuilt = Query.MostFrequentPredictedYearBuilt([yearBuiltData_2001, yearBuiltData_1998]);
            Assert.NotNull(predictedYearBuilt);
            Assert.Equal((short)1998, predictedYearBuilt!.Year);
        }

        /// <summary>
        /// Verifies that two years sharing both count and recency are answered with the greater year, so the selection stays deterministic.
        /// </summary>
        [Fact]
        public void MostFrequentPredictedYearBuilt_TieSameStampGreaterYear()
        {
            Classes.YearBuiltData yearBuiltData_2001 = new("b_ref_001");
            yearBuiltData_2001.SetPredictedYearBuilt(new DateTime(2026, 1, 1), 2001);

            Classes.YearBuiltData yearBuiltData_1998 = new("b_ref_001");
            yearBuiltData_1998.SetPredictedYearBuilt(new DateTime(2026, 1, 1), 1998);

            Classes.PredictedYearBuilt? predictedYearBuilt = Query.MostFrequentPredictedYearBuilt([yearBuiltData_2001, yearBuiltData_1998]);
            Assert.NotNull(predictedYearBuilt);
            Assert.Equal((short)2001, predictedYearBuilt!.Year);
        }

        /// <summary>
        /// Verifies that null and empty input answer null.
        /// </summary>
        [Fact]
        public void MostFrequentPredictedYearBuilt_Empty()
        {
            Assert.Null(Query.MostFrequentPredictedYearBuilt(null));
            Assert.Null(Query.MostFrequentPredictedYearBuilt([]));
        }
    }
}
