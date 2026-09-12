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
        /// Tests that a solved VisualTypology carries the appearance of the bucket each node came from, filed on the rule of its level.
        /// <para>Asserted, not measured: every node of a two level chain holds the appearance its rule data hands over - a mapped unique value and a mapped range carry theirs, an unmapped bucket and a value below a level carrying a base rule carry none, the dropped null-year row is absent from the tree, references reach every node from the matched one up to the root, stripping the references keeps every appearance, and the solved tree round trips through serialization.</para>
        /// </summary>
        [Fact]
        public void VisualTypology_Appearance()
        {
            TypologyAppearance typologyAppearance_Residential = Create.TypologyAppearance(System.Drawing.Color.Red);
            TypologyAppearance typologyAppearance_Industrial = Create.TypologyAppearance(System.Drawing.Color.Purple);
            TypologyAppearance typologyAppearance_Old = Create.TypologyAppearance(System.Drawing.Color.Brown);
            TypologyAppearance typologyAppearance_New = Create.TypologyAppearance(System.Drawing.Color.Green);

            Range<int> range_Old = new(0, 2003);
            Range<int> range_Middle = new(2004, 2020);
            Range<int> range_New = new(2021, int.MaxValue);

            VisualUniqueValueFilterRule visualUniqueValueFilterRule = new();

            visualUniqueValueFilterRule.TypologyAppearanceCollection["Residential"] = typologyAppearance_Residential;
            visualUniqueValueFilterRule.TypologyAppearanceCollection["Industrial"] = typologyAppearance_Industrial;

            VisualIntegerRangeFilterRule visualIntegerRangeFilterRule = new([range_New, range_Middle, range_Old]);

            visualIntegerRangeFilterRule.TypologyAppearanceCollection[range_Old] = typologyAppearance_Old;
            visualIntegerRangeFilterRule.TypologyAppearanceCollection[range_New] = typologyAppearance_New;

            VisualColumnTypologyFilter<Column> visualColumnTypologyFilter = new()
            {
                Value = new Column(-1, "occupancy", typeof(string)),
                Rule = visualUniqueValueFilterRule,
                Filter = new VisualColumnTypologyFilter<Column>()
                {
                    Value = new Column(-1, "year_built", typeof(int)),
                    Rule = visualIntegerRangeFilterRule
                }
            };

            VisualTypology? visualTypology = Typology.Visual.Create.VisualTypology(VisualTypologyTable(), visualColumnTypologyFilter, new Column("building_reference", typeof(string)));

            Assert.NotNull(visualTypology);
            Assert.Equal(string.Empty, visualTypology.Name);

            VisualTypology visualTypology_Residential = SubTypologyByName(visualTypology, "occupancy Residential");
            VisualTypology visualTypology_Industrial = SubTypologyByName(visualTypology, "occupancy Industrial");
            VisualTypology visualTypology_Agricultural = SubTypologyByName(visualTypology, "occupancy Agricultural");

            // Level 1: a mapped unique value carries its appearance, an unmapped one carries none, and each node holds its subtree's references.
            AssertAppearance(typologyAppearance_Residential, visualTypology_Residential);
            Assert.Equal(["b1", "b2", "b3"], [.. visualTypology_Residential.References.OrderBy(x => x)]);

            AssertAppearance(typologyAppearance_Industrial, visualTypology_Industrial);
            Assert.Equal(["b4", "b5"], [.. visualTypology_Industrial.References.OrderBy(x => x)]);

            Assert.Null(visualTypology_Agricultural.TypologyItem?.Appearance);
            Assert.Equal(["b6", "b7"], [.. visualTypology_Agricultural.References.OrderBy(x => x)]);

            // Level 2: a mapped range carries its appearance, an unmapped one carries none, and the null-year row is absent entirely.
            AssertAppearance(typologyAppearance_Old, SubTypologyByName(visualTypology_Residential, $"year_built [{range_Old.Min}, {range_Old.Max}]"));
            AssertAppearance(null, SubTypologyByName(visualTypology_Residential, $"year_built [{range_Middle.Min}, {range_Middle.Max}]"));
            AssertAppearance(typologyAppearance_New, SubTypologyByName(visualTypology_Residential, $"year_built [{range_New.Min}, {range_New.Max}]"));

            Assert.Equal(3, SubTypologies(visualTypology_Residential).Count);
            Assert.Equal(2, SubTypologies(visualTypology_Industrial).Count);

            // The row with no year built resolves to no range bucket, so the Agricultural branch holds its single remaining row only.
            Assert.Single(SubTypologies(visualTypology_Agricultural));
            AssertAppearance(typologyAppearance_Old, SubTypologyByName(visualTypology_Agricultural, $"year_built [{range_Old.Min}, {range_Old.Max}]"));

            // Every reference reaches the root, because an ancestor holds its whole subtree's.
            Assert.Equal(7, visualTypology.ReferenceSet(true).Count);

            // A level carrying a base rule is legal: it buckets as its base does and resolves no appearance.
            VisualColumnTypologyFilter<Column> visualColumnTypologyFilter_BaseRule = new()
            {
                Value = new Column(-1, "occupancy", typeof(string)),
                Rule = new UniqueValueFilterRule()
            };

            VisualTypology? visualTypology_BaseRule = Typology.Visual.Create.VisualTypology(VisualTypologyTable(), visualColumnTypologyFilter_BaseRule, new Column("building_reference", typeof(string)));

            Assert.NotNull(visualTypology_BaseRule);
            Assert.Equal(3, SubTypologies(visualTypology_BaseRule).Count);
            foreach (VisualTypology subTypology in SubTypologies(visualTypology_BaseRule))
            {
                Assert.StartsWith("occupancy ", subTypology.Name);
                Assert.Null(subTypology.TypologyItem?.Appearance);
            }

            Core.xUnit.Query.SerializationCheck(visualTypology);

            // Stripping the references keeps the structure and every appearance: the appearance is node metadata, not link data.
            Assert.True(visualTypology.RemoveReferences(true));
            Assert.Empty(visualTypology.ReferenceSet(true));
            AssertAppearance(typologyAppearance_Residential, visualTypology_Residential);
            Assert.Equal(3, SubTypologies(visualTypology_Residential).Count);

            static List<VisualTypology> SubTypologies(VisualTypology typology)
            {
                Assert.NotNull(typology.SubTypologies);
                return typology.SubTypologies!;
            }

            static VisualTypology SubTypologyByName(VisualTypology typology, string name)
            {
                VisualTypology? subTypology_Result = null;
                foreach (VisualTypology subTypology in SubTypologies(typology))
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

            static void AssertAppearance(TypologyAppearance? typologyAppearance_Expected, VisualTypology typology)
            {
                TypologyAppearance? typologyAppearance_Actual = typology.TypologyItem?.Appearance;

                if (typologyAppearance_Expected is null)
                {
                    Assert.Null(typologyAppearance_Actual);
                    return;
                }

                Assert.NotNull(typologyAppearance_Actual);
                Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance_Expected), Core.Convert.ToSystem_String(typologyAppearance_Actual));
            }
        }
    }
}
