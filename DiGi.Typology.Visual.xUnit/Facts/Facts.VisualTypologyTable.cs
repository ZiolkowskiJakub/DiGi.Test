using DiGi.Core.Classes;
using DiGi.Core.IO.Table.Classes;
using DiGi.Typology.Classes;
using DiGi.Typology.Visual.Classes;
using System.Collections.Generic;

namespace DiGi.Typology.Visual.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Builds the building data table the Visual typology facts classify.
        /// <para>Columns are deliberately added in an order matching neither the order the filter chain names them nor the chain's own nesting, so a fact exercises resolution by unique id rather than one that happens to agree with a column's own index.</para>
        /// <para>The last row carries no year built, which is the value a range level drops.</para>
        /// </summary>
        /// <returns>A table of seven rows over the reference, year built and occupancy columns.</returns>
        private static Table VisualTypologyTable()
        {
            Table table = new();

            Assert.NotNull(table.AddColumn("building_reference", typeof(string)));
            Assert.NotNull(table.AddColumn("year_built", typeof(int)));
            Assert.NotNull(table.AddColumn("occupancy", typeof(string)));

            AddRow("b1", 1990, "Residential");
            AddRow("b2", 2010, "Residential");
            AddRow("b3", 2021, "Residential");
            AddRow("b4", 1995, "Industrial");
            AddRow("b5", 2030, "Industrial");
            AddRow("b6", 2000, "Agricultural");
            AddRow("b7", null, "Agricultural");

            return table;

            void AddRow(string? buildingReference, int? yearBuilt, string occupancy)
            {
                List<object?> values = [buildingReference, yearBuilt, occupancy];

                Assert.NotNull(table.AddRow(values));
            }
        }

        /// <summary>
        /// Builds the two level Visual filter chain the Visual typology facts group by - occupancy bucketed by unique value, then year built bucketed into three ranges - with an appearance filed for two of the three buckets of each level.
        /// <para>The chain names columns that carry no table index (an index belongs to the table a column was added to), so resolving them against the table is the work the factory does. The ranges are declared in descending order to exercise the rule's own ordering rather than one the declaration happens to agree with.</para>
        /// </summary>
        /// <returns>The root of the Visual chain.</returns>
        private static VisualColumnTypologyFilter<Column> VisualTypologyFilter()
        {
            Range<int> range_Old = new(0, 2003);
            Range<int> range_Middle = new(2004, 2020);
            Range<int> range_New = new(2021, int.MaxValue);

            VisualUniqueValueFilterRule visualUniqueValueFilterRule = new();

            visualUniqueValueFilterRule.TypologyAppearanceCollection["Residential"] = Create.TypologyAppearance(System.Drawing.Color.Red);
            visualUniqueValueFilterRule.TypologyAppearanceCollection["Industrial"] = Create.TypologyAppearance(System.Drawing.Color.Purple);

            VisualIntegerRangeFilterRule visualIntegerRangeFilterRule = new([range_New, range_Middle, range_Old]);

            visualIntegerRangeFilterRule.TypologyAppearanceCollection[range_Old] = Create.TypologyAppearance(System.Drawing.Color.Brown);
            visualIntegerRangeFilterRule.TypologyAppearanceCollection[range_New] = Create.TypologyAppearance(System.Drawing.Color.Green);

            return new VisualColumnTypologyFilter<Column>()
            {
                Value = new Column(-1, "occupancy", typeof(string)),
                Rule = visualUniqueValueFilterRule,
                Filter = new VisualColumnTypologyFilter<Column>()
                {
                    Value = new Column(-1, "year_built", typeof(int)),
                    Rule = visualIntegerRangeFilterRule
                }
            };
        }

        /// <summary>
        /// Builds the plain counterpart of the Visual chain - the same two levels over the same buckets, through base rules that carry no appearance - for the structure parity fact to solve with DiGi.GIS Create.Typology.
        /// </summary>
        /// <returns>The root of the plain chain.</returns>
        private static ColumnTypologyFilter<Column> PlainTypologyFilter()
        {
            return new ColumnTypologyFilter<Column>()
            {
                Value = new Column(-1, "occupancy", typeof(string)),
                Rule = new UniqueValueFilterRule(),
                Filter = new ColumnTypologyFilter<Column>()
                {
                    Value = new Column(-1, "year_built", typeof(int)),
                    Rule = new IntegerRangeFilterRule([new Range<int>(2021, int.MaxValue), new Range<int>(2004, 2020), new Range<int>(0, 2003)])
                }
            };
        }
    }
}
