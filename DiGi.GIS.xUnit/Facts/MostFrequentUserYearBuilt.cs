using System;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that only exact user years vote, so a majority of exact entries wins while bounds are skipped, and that bounds alone answer nothing.
        /// </summary>
        [Fact]
        public void MostFrequentUserYearBuilt_ExactOnly()
        {
            Classes.YearBuiltData yearBuiltData_1975_1 = new("b_ref_001");
            yearBuiltData_1975_1.SetUserYearBuilt(1975);

            Classes.YearBuiltData yearBuiltData_Bound = new("b_ref_001");
            yearBuiltData_Bound.SetUserYearBuilt(new Classes.UserYearBuilt((short)1990, Enums.YearBuiltRelation.AtOrBefore));

            Classes.YearBuiltData yearBuiltData_1975_2 = new("b_ref_001");
            yearBuiltData_1975_2.SetUserYearBuilt(1975);

            Classes.UserYearBuilt? userYearBuilt = Query.MostFrequentUserYearBuilt([yearBuiltData_1975_1, yearBuiltData_Bound, yearBuiltData_1975_2]);
            Assert.NotNull(userYearBuilt);
            Assert.Equal((short)1975, userYearBuilt!.Year);

            //A bound is a bound rather than a year, so it takes no part in the count
            Assert.Null(Query.MostFrequentUserYearBuilt([yearBuiltData_Bound]));
        }

        /// <summary>
        /// Verifies that a count tie goes to the year whose newest entry is the most recent, a missing timestamp counting as the oldest.
        /// </summary>
        [Fact]
        public void MostFrequentUserYearBuilt_TieNewest()
        {
            Classes.YearBuiltData yearBuiltData_2001 = new("b_ref_001");
            yearBuiltData_2001.SetUserYearBuilt(new Classes.UserYearBuilt((short)2001, Enums.YearBuiltRelation.Exact, new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero), null));

            Classes.YearBuiltData yearBuiltData_1998 = new("b_ref_001");
            yearBuiltData_1998.SetUserYearBuilt(new Classes.UserYearBuilt((short)1998, Enums.YearBuiltRelation.Exact, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), null));

            Classes.UserYearBuilt? userYearBuilt = Query.MostFrequentUserYearBuilt([yearBuiltData_2001, yearBuiltData_1998]);
            Assert.NotNull(userYearBuilt);
            Assert.Equal((short)1998, userYearBuilt!.Year);

            //The undated entry sorts oldest, so the dated one still wins the tie
            Classes.YearBuiltData yearBuiltData_Undated = new("b_ref_001");
            yearBuiltData_Undated.SetUserYearBuilt(2001);

            userYearBuilt = Query.MostFrequentUserYearBuilt([yearBuiltData_Undated, yearBuiltData_1998]);
            Assert.NotNull(userYearBuilt);
            Assert.Equal((short)1998, userYearBuilt!.Year);
        }
    }
}
