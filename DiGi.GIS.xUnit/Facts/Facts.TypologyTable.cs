using DiGi.Core.Classes;
using DiGi.Core.IO.Table.Classes;
using DiGi.Typology.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Builds the building data table the typology facts classify.
        /// <para>Columns are deliberately added in an order matching neither the declaration order of the shared column constants nor the order the filter chain names them, so a fact exercises resolution by unique id rather than one that happens to agree with a constant's own index.</para>
        /// <para>The last row carries no predicted year built, which is the value a range level drops.</para>
        /// </summary>
        /// <returns>A table of seven rows over the predicted year built, county name, reference and occupancy columns.</returns>
        private static Table TypologyTable()
        {
            Table table = new();

            Assert.NotNull(table.AddColumn(IO.Constants.Column.PredictedYearBuilt));
            Assert.NotNull(table.AddColumn(IO.Constants.Column.CountyName));
            Assert.NotNull(table.AddColumn(IO.Constants.Column.Reference));
            Assert.NotNull(table.AddColumn(IO.Constants.Column.IsOccupied));

            AddRow((ushort)1990, "Alpha", "b1", true);
            AddRow((ushort)2003, "Alpha", "b2", true);
            AddRow((ushort)2004, "Alpha", "b3", true);
            AddRow((ushort)2020, "Alpha", "b4", false);
            AddRow((ushort)2021, "Beta", "b5", true);
            AddRow((ushort)2025, "Beta", "b6", false);
            AddRow(null, "Beta", "b7", true);

            return table;

            void AddRow(object? predictedYearBuilt, string countyName, string reference, bool isOccupied)
            {
                List<object?> values = [predictedYearBuilt, countyName, reference, isOccupied];

                Assert.NotNull(table.AddRow(values));
            }
        }

        /// <summary>
        /// Builds the three level filter chain the typology facts group by - county name, then occupancy, then predicted year built bucketed into three ranges.
        /// <para>The chain names the shared column constants, whose index is -1 because an index belongs to the table a column was added to. Resolving them against the table is the work the factory does.</para>
        /// </summary>
        /// <returns>The root of the chain.</returns>
        private static ColumnTypologyFilter<Column> TypologyFilter()
        {
            return new ColumnTypologyFilter<Column>()
            {
                Value = IO.Constants.Column.CountyName,
                Rule = new UniqueValueFilterRule(),
                Filter = new ColumnTypologyFilter<Column>()
                {
                    Value = IO.Constants.Column.IsOccupied,
                    Rule = new UniqueValueFilterRule(),
                    Filter = new ColumnTypologyFilter<Column>()
                    {
                        Value = IO.Constants.Column.PredictedYearBuilt,
                        Rule = new IntegerRangeFilterRule([new Range<int>(0, 2003), new Range<int>(2004, 2020), new Range<int>(2021, int.MaxValue)])
                    }
                }
            };
        }
    }
}
