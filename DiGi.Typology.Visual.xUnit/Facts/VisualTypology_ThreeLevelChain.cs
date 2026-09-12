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
        /// Tests a three level Visual chain end to end: county bucketed by unique value, occupancy bucketed by unique value with an appearance filed for a mapped value and for the NULL bucket, then year built bucketed into ranges.
        /// <para>Asserted, not measured: the NULL bucket of a unique value rule solves as a node named "occupancy null" carrying the appearance filed for it, a leaf three levels deep carries the full three segment path, the solved structure equals what DiGi.GIS Create.Typology solves over an equivalent plain chain - names, descriptions, references and filing indexes at every level - and the caller's chain is left exactly as it was: the same rule instances, the same links and the original column instances with no table index.</para>
        /// </summary>
        [Fact]
        public void VisualTypology_ThreeLevelChain()
        {
            TypologyAppearance typologyAppearance_Null = Create.TypologyAppearance(System.Drawing.Color.Gray);
            TypologyAppearance typologyAppearance_Residential = Create.TypologyAppearance(System.Drawing.Color.Red);
            TypologyAppearance typologyAppearance_Old = Create.TypologyAppearance(System.Drawing.Color.Brown);

            Range<int> range_New = new(2021, int.MaxValue);
            Range<int> range_Old = new(0, 2020);

            Table table = new();

            Assert.NotNull(table.AddColumn("building_reference", typeof(string)));
            Assert.NotNull(table.AddColumn("county", typeof(string)));
            Assert.NotNull(table.AddColumn("occupancy", typeof(string)));
            Assert.NotNull(table.AddColumn("year_built", typeof(int)));

            AddRow("b1", "Alpha", "Residential", 1990);
            AddRow("b2", "Alpha", null, 2000);
            AddRow("b3", "Beta", "Residential", 2021);
            AddRow("b4", "Beta", "Industrial", 1995);

            VisualUniqueValueFilterRule visualUniqueValueFilterRule_County = new();

            VisualUniqueValueFilterRule visualUniqueValueFilterRule_Occupancy = new();

            visualUniqueValueFilterRule_Occupancy.TypologyAppearanceCollection[null] = typologyAppearance_Null;
            visualUniqueValueFilterRule_Occupancy.TypologyAppearanceCollection["Residential"] = typologyAppearance_Residential;

            VisualIntegerRangeFilterRule visualIntegerRangeFilterRule_Year = new([range_New, range_Old]);
            visualIntegerRangeFilterRule_Year.TypologyAppearanceCollection[range_Old] = typologyAppearance_Old;

            VisualColumnTypologyFilter<Column> visualColumnTypologyFilter = new()
            {
                Value = new Column(-1, "county", typeof(string)),
                Rule = visualUniqueValueFilterRule_County,
                Filter = new VisualColumnTypologyFilter<Column>()
                {
                    Value = new Column(-1, "occupancy", typeof(string)),
                    Rule = visualUniqueValueFilterRule_Occupancy,
                    Filter = new VisualColumnTypologyFilter<Column>()
                    {
                        Value = new Column(-1, "year_built", typeof(int)),
                        Rule = visualIntegerRangeFilterRule_Year
                    }
                }
            };

            Column column_Reference = new("building_reference", typeof(string));

            VisualTypology? visualTypology = Typology.Visual.Create.VisualTypology(table, visualColumnTypologyFilter, column_Reference);

            Assert.NotNull(visualTypology);

            // The NULL bucket of a unique value rule is a node like any other, carrying the appearance filed for it.
            VisualTypology visualTypology_OccupancyNull = SubTypologyByName(SubTypologyByName(visualTypology, "county Alpha"), "occupancy null");

            Assert.NotNull(visualTypology_OccupancyNull.TypologyItem?.Appearance);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance_Null), Core.Convert.ToSystem_String(visualTypology_OccupancyNull.TypologyItem?.Appearance));

            // A leaf three levels deep carries the full three segment path.
            VisualTypology visualTypology_Leaf = SubTypologyByName(SubTypologyByName(SubTypologyByName(visualTypology, "county Beta"), "occupancy Industrial"), $"year_built [{range_Old.Min}, {range_Old.Max}]");

            Assert.NotNull(visualTypology_Leaf.TypologyPath);
            Assert.Equal(3, visualTypology_Leaf.TypologyPath!.Count);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance_Old), Core.Convert.ToSystem_String(visualTypology_Leaf.TypologyItem?.Appearance));

            // The plain solver answers the same structure over an equivalent chain.
            ColumnTypologyFilter<Column> columnTypologyFilter = new()
            {
                Value = new Column(-1, "county", typeof(string)),
                Rule = new UniqueValueFilterRule(),
                Filter = new ColumnTypologyFilter<Column>()
                {
                    Value = new Column(-1, "occupancy", typeof(string)),
                    Rule = new UniqueValueFilterRule(),
                    Filter = new ColumnTypologyFilter<Column>()
                    {
                        Value = new Column(-1, "year_built", typeof(int)),
                        Rule = new IntegerRangeFilterRule([range_New, range_Old])
                    }
                }
            };

            Typology.Classes.Typology? typology = GIS.Create.Typology(table, columnTypologyFilter, column_Reference);

            Assert.NotNull(typology);
            AssertNode(typology, visualTypology);

            // The caller's chain is never modified: the same rule instances, the same links, and the chain's own column instances with no table index.
            Assert.Same(visualUniqueValueFilterRule_County, visualColumnTypologyFilter.Rule);

            VisualColumnTypologyFilter<Column>? visualColumnTypologyFilter_Level2 = visualColumnTypologyFilter.Filter;
            Assert.NotNull(visualColumnTypologyFilter_Level2);

            Assert.Same(visualUniqueValueFilterRule_Occupancy, visualColumnTypologyFilter_Level2!.Rule);
            Assert.NotNull(visualColumnTypologyFilter_Level2.Filter);
            Assert.Same(visualIntegerRangeFilterRule_Year, visualColumnTypologyFilter_Level2.Filter!.Rule);
            Assert.Null(visualColumnTypologyFilter_Level2.Filter!.Filter);
            Assert.Equal(-1, visualColumnTypologyFilter.Value?.Index);
            Assert.Equal(-1, visualColumnTypologyFilter_Level2.Value?.Index);

            void AddRow(string buildingReference, string? county, string? occupancy, int? yearBuilt)
            {
                List<object?> values = [buildingReference, county, occupancy, yearBuilt];

                Assert.NotNull(table.AddRow(values));
            }

            static VisualTypology SubTypologyByName(VisualTypology typology, string name)
            {
                VisualTypology? subTypology_Result = null;
                Assert.NotNull(typology.SubTypologies);
                foreach (VisualTypology subTypology in typology.SubTypologies!)
                {
                    if (string.Equals(subTypology.Name, name, StringComparison.Ordinal))
                    {
                        subTypology_Result = subTypology;
                        break;
                    }
                }

                Assert.NotNull(subTypology_Result);
                return subTypology_Result;
            }

            static void AssertNode(Typology.Classes.Typology typology_Level, VisualTypology visualTypology_Level)
            {
                Assert.Equal(typology_Level.Name, visualTypology_Level.Name);
                Assert.Equal(typology_Level.Description, visualTypology_Level.Description);

                List<string> references = [.. typology_Level.References.OrderBy(x => x)];
                Assert.Equal(references, [.. visualTypology_Level.References.OrderBy(x => x)]);

                List<int> indexes = [.. typology_Level.Indexes.OrderBy(x => x)];
                Assert.Equal(indexes, [.. visualTypology_Level.Indexes.OrderBy(x => x)]);

                foreach (int index in indexes)
                {
                    Assert.NotNull(typology_Level[index]);
                    Assert.NotNull(visualTypology_Level[index]);

                    AssertNode(typology_Level[index]!, visualTypology_Level[index]!);
                }
            }
        }
    }
}
