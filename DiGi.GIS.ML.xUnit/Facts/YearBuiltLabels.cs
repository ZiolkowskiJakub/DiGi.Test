using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.Classes;
using DiGi.GIS.Enums;
using DiGi.GIS.ML;
using System;
using System.Collections.Generic;

namespace DiGi.GIS.ML.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the training labels are read from the stored User year built column and never from a stored prediction or a bound.
        /// <para>The column is written by the real modifier rather than by hand, so the fact exercises the stored shape: the user year where one is exact, nothing where the entries are bounds or predictions alone, and the most frequent exact year where several records are stored. The label the column holds is asserted against the rule it was written from, per reference, so the two cannot drift apart.</para>
        /// </summary>
        [Fact]
        public void YearBuiltLabels_ReadsStoredUserYearBuilt()
        {
            int countyId = 104106;

            // The shape actually stored: a user year, and a prediction from the incumbent model disagreeing with it.
            YearBuiltData yearBuiltData_Both = new("REF-BOTH");
            Assert.True(yearBuiltData_Both.Add(new UserYearBuilt(1975)));
            Assert.True(yearBuiltData_Both.Add(new PredictedYearBuilt(new DateTime(2025, 5, 29, 7, 41, 47, DateTimeKind.Utc), 2008)));

            // A building the pipeline has scored but nobody has confirmed. It is unlabelled, not labelled zero.
            YearBuiltData yearBuiltData_PredictionOnly = new("REF-PREDICTION");
            Assert.True(yearBuiltData_PredictionOnly.Add(new PredictedYearBuilt(new DateTime(2025, 5, 29, 7, 41, 47, DateTimeKind.Utc), 2008)));

            YearBuiltData yearBuiltData_UserOnly = new("REF-USER");
            Assert.True(yearBuiltData_UserOnly.Add(new UserYearBuilt(1932)));

            // A bound is a relation to the year, not a year. It never votes, so the building is unlabelled.
            YearBuiltData yearBuiltData_Bound = new("REF-BOUND");
            Assert.True(yearBuiltData_Bound.Add(new UserYearBuilt(1950, YearBuiltRelation.AtOrBefore)));

            // Three records for one building: the most frequent exact year wins over the newest.
            YearBuiltData yearBuiltData_Multi_1 = new("REF-MULTI");
            Assert.True(yearBuiltData_Multi_1.Add(new UserYearBuilt(1975, YearBuiltRelation.Exact, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))));
            YearBuiltData yearBuiltData_Multi_2 = new("REF-MULTI");
            Assert.True(yearBuiltData_Multi_2.Add(new UserYearBuilt(1975, YearBuiltRelation.Exact, new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero))));
            YearBuiltData yearBuiltData_Multi_3 = new("REF-MULTI");
            Assert.True(yearBuiltData_Multi_3.Add(new UserYearBuilt(1981, YearBuiltRelation.Exact, new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero))));

            YearBuiltData yearBuiltData_Empty = new("REF-EMPTY");

            List<YearBuiltData> yearBuiltDatas = [yearBuiltData_Both, yearBuiltData_PredictionOnly, yearBuiltData_UserOnly, yearBuiltData_Bound, yearBuiltData_Multi_1, yearBuiltData_Multi_2, yearBuiltData_Multi_3, yearBuiltData_Empty];

            // The real writer, so the column holds exactly what the stored records derive to.
            Table table = new();
            int rowsWritten = IO.Modify.Update_Building2D_YearBuilt(table, countyId, yearBuiltDatas);

            // REF-BOTH, REF-PREDICTION, REF-USER and REF-MULTI carry at least one value. REF-BOUND (bounds only) and REF-EMPTY (no entry) leave no row.
            Assert.Equal(4, rowsWritten);

            Dictionary<string, short> labels = table.YearBuiltLabels();

            Assert.Equal(3, labels.Count);
            Assert.Equal((short)1975, labels["REF-BOTH"]);
            Assert.Equal((short)1932, labels["REF-USER"]);
            Assert.Equal((short)1975, labels["REF-MULTI"]);
            Assert.False(labels.ContainsKey("REF-PREDICTION"));
            Assert.False(labels.ContainsKey("REF-BOUND"));
            Assert.False(labels.ContainsKey("REF-EMPTY"));

            // Parity: the label the column holds is the rule it was written from, per reference.
            Dictionary<string, List<YearBuiltData>> recordsByReference = [];
            foreach (YearBuiltData yearBuiltData in yearBuiltDatas)
            {
                if (!recordsByReference.TryGetValue(yearBuiltData.Reference!, out List<YearBuiltData>? group))
                {
                    group = [];
                    recordsByReference[yearBuiltData.Reference!] = group;
                }

                group.Add(yearBuiltData);
            }

            foreach (KeyValuePair<string, List<YearBuiltData>> keyValuePair in recordsByReference)
            {
                if (GIS.Query.MostFrequentUserYearBuilt(keyValuePair.Value)?.Year is short year_Expected)
                {
                    Assert.True(labels.TryGetValue(keyValuePair.Key, out short year_Actual));
                    Assert.Equal(year_Expected, year_Actual);
                }
                else
                {
                    Assert.False(labels.ContainsKey(keyValuePair.Key));
                }
            }

            // Edge inputs. null is ambiguous between the two overloads, so it is cast to the one under test.
            Assert.Empty(Query.YearBuiltLabels((IEnumerable<Table?>?)null));
            Assert.Empty(((Table?)null).YearBuiltLabels());

            // The pre-#94 shape: a table that carries the reference but not the User year built column gives no labels and does not throw.
            Table table_NoColumn = new();
            table_NoColumn.AddColumn(IO.Constants.Column.Reference);
            table_NoColumn.AddRow(["REF-NONE"]);
            Assert.Empty(table_NoColumn.YearBuiltLabels());
        }
    }
}
