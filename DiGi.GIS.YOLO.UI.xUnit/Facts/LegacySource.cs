using DiGi.GIS.Classes;
using DiGi.GIS.Enums;
using System;
using System.Collections.Generic;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the <c>train8</c> Legacy rule as a truth table: the legacy list alone, an undated user entry, an entry dated before the cut-off, an entry dated only after it, a bounded early entry, an early entry on a second stored row, and both records together.
        /// <para>The cut-off is exclusive: an entry dated exactly at it cannot have been seen, one a tick earlier could. Any user entry counts, bounded or exact, because <c>train8</c> saw a building&apos;s images whatever it was labelled with; and every stored row is looked at, because one reference can carry several.</para>
        /// </summary>
        [Fact]
        public void LegacySource()
        {
            DateTimeOffset cutoff = new(2025, 5, 23, 0, 0, 0, TimeSpan.Zero);
            HashSet<string> legacyReferences = new(StringComparer.Ordinal) { "TSV" };

            List<YearBuiltData> YearBuiltDatas(string reference, params UserYearBuilt?[] userYearBuilts)
            {
                List<YearBuiltData> result = [];
                foreach (UserYearBuilt? userYearBuilt in userYearBuilts)
                {
                    YearBuiltData yearBuiltData = new(reference);
                    if (userYearBuilt is not null)
                    {
                        Assert.True(yearBuiltData.SetUserYearBuilt(userYearBuilt));
                    }

                    result.Add(yearBuiltData);
                }

                return result;
            }

            UserYearBuilt userYearBuilt_After = new(1990, YearBuiltRelation.Exact, cutoff.AddDays(10), "user");
            UserYearBuilt userYearBuilt_Before = new(1990, YearBuiltRelation.Exact, cutoff.AddDays(-10), "user");

            // The list alone.
            Assert.Equal(Enums.LegacySource.Tsv, Query.LegacySource(legacyReferences, "TSV", YearBuiltDatas("TSV", userYearBuilt_After), cutoff));
            Assert.Equal(Enums.LegacySource.Tsv, Query.LegacySource(legacyReferences, "TSV", null, cutoff));

            // An undated entry is a legacy entry.
            Assert.Equal(Enums.LegacySource.Timestamp, Query.LegacySource(legacyReferences, "A", YearBuiltDatas("A", new UserYearBuilt(1990)), cutoff));

            // Dated before the cut-off, and the boundary on both sides.
            Assert.Equal(Enums.LegacySource.Timestamp, Query.LegacySource(legacyReferences, "A", YearBuiltDatas("A", userYearBuilt_Before), cutoff));
            Assert.Equal(Enums.LegacySource.Timestamp, Query.LegacySource(legacyReferences, "A", YearBuiltDatas("A", new UserYearBuilt(1990, YearBuiltRelation.Exact, cutoff.AddTicks(-1))), cutoff));
            Assert.Equal(Enums.LegacySource.None, Query.LegacySource(legacyReferences, "A", YearBuiltDatas("A", new UserYearBuilt(1990, YearBuiltRelation.Exact, cutoff)), cutoff));

            // Dated only after the cut-off: clean.
            Assert.Equal(Enums.LegacySource.None, Query.LegacySource(legacyReferences, "A", YearBuiltDatas("A", userYearBuilt_After), cutoff));

            // A bounded entry counts as much as an exact one.
            Assert.Equal(Enums.LegacySource.Timestamp, Query.LegacySource(legacyReferences, "A", YearBuiltDatas("A", new UserYearBuilt(1990, YearBuiltRelation.AtOrBefore, cutoff.AddDays(-1))), cutoff));
            Assert.Equal(Enums.LegacySource.Timestamp, Query.LegacySource(legacyReferences, "A", YearBuiltDatas("A", new UserYearBuilt(1990, YearBuiltRelation.After, null)), cutoff));

            // The early entry on the second stored row is not missed.
            Assert.Equal(Enums.LegacySource.Timestamp, Query.LegacySource(legacyReferences, "A", YearBuiltDatas("A", userYearBuilt_After, userYearBuilt_Before), cutoff));

            // A row with no user entry says nothing.
            Assert.Equal(Enums.LegacySource.None, Query.LegacySource(legacyReferences, "A", YearBuiltDatas("A", null, userYearBuilt_After), cutoff));

            // Both records.
            Assert.Equal(Enums.LegacySource.Both, Query.LegacySource(legacyReferences, "TSV", YearBuiltDatas("TSV", userYearBuilt_Before), cutoff));

            // Nothing read and not on the list.
            Assert.Equal(Enums.LegacySource.None, Query.LegacySource(legacyReferences, "A", null, cutoff));

            // The list is matched ordinally.
            Assert.Equal(Enums.LegacySource.None, Query.LegacySource(legacyReferences, "tsv", null, cutoff));

            // Every source but None is Legacy, and so is a decision that could not be taken.
            Assert.False(new Classes.DatasetReference("A", 1, DiGi.YOLO.Enums.Category.Test, 1990, Enums.LegacySource.None).Legacy);
            Assert.True(new Classes.DatasetReference("A", 1, DiGi.YOLO.Enums.Category.Test, 1990, Enums.LegacySource.Tsv).Legacy);
            Assert.True(new Classes.DatasetReference("A", 1, DiGi.YOLO.Enums.Category.Test, 1990, Enums.LegacySource.Timestamp).Legacy);
            Assert.True(new Classes.DatasetReference("A", 1, DiGi.YOLO.Enums.Category.Test, 1990, Enums.LegacySource.Both).Legacy);
            Assert.True(new Classes.DatasetReference("A", 1, DiGi.YOLO.Enums.Category.Test, 1990, Enums.LegacySource.Unknown).Legacy);
        }
    }
}
