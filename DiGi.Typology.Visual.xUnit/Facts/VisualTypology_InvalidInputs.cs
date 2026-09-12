using DiGi.Core.IO.Table.Classes;
using DiGi.Typology.Classes;
using DiGi.Typology.Visual.Classes;

namespace DiGi.Typology.Visual.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that Create.VisualTypology returns null rather than throwing for every input it cannot classify.
        /// <para>A missing reference column is refused only when references are actually being stored - asking for references while naming no column to take them from is the case a guard that skips absent input would let through.</para>
        /// <para>A chain that cannot be honoured in full is refused rather than solved in part, because a tree ending above the level that was asked for looks like an answer. An empty table is refused for the same reason the base solver refuses it: there is nothing to answer with.</para>
        /// </summary>
        [Fact]
        public void VisualTypology_InvalidInputs()
        {
            Table table = VisualTypologyTable();

            Assert.Null(Typology.Visual.Create.VisualTypology(null, VisualTypologyFilter(), new Column("building_reference", typeof(string))));
            Assert.Null(Typology.Visual.Create.VisualTypology(table, null, new Column("building_reference", typeof(string))));

            Table table_Empty = new();
            Assert.NotNull(table_Empty.AddColumn("occupancy", typeof(string)));
            Assert.Null(Typology.Visual.Create.VisualTypology(table_Empty, VisualTypologyFilter(), new Column("building_reference", typeof(string))));

            VisualColumnTypologyFilter<Column> visualColumnTypologyFilter_Absent = new()
            {
                Value = new Column(-1, "storeys", typeof(int)),
                Rule = new VisualUniqueValueFilterRule()
            };
            Assert.Null(Typology.Visual.Create.VisualTypology(table, visualColumnTypologyFilter_Absent, new Column("building_reference", typeof(string))));

            VisualColumnTypologyFilter<Column> visualColumnTypologyFilter_AbsentNested = new()
            {
                Value = new Column(-1, "occupancy", typeof(string)),
                Rule = new VisualUniqueValueFilterRule(),
                Filter = visualColumnTypologyFilter_Absent
            };
            Assert.Null(Typology.Visual.Create.VisualTypology(table, visualColumnTypologyFilter_AbsentNested, new Column("building_reference", typeof(string))));

            // A level carrying no rule groups nothing, so the solve would end the tree above it.
            VisualColumnTypologyFilter<Column> visualColumnTypologyFilter_NoRule = new()
            {
                Value = new Column(-1, "occupancy", typeof(string)),
                Rule = new VisualUniqueValueFilterRule(),
                Filter = new VisualColumnTypologyFilter<Column>()
                {
                    Value = new Column(-1, "year_built", typeof(int))
                }
            };
            Assert.Null(Typology.Visual.Create.VisualTypology(table, visualColumnTypologyFilter_NoRule, new Column("building_reference", typeof(string))));

            // A chain linking back on itself would otherwise be solved as a silently truncated one.
            VisualColumnTypologyFilter<Column> visualColumnTypologyFilter_Cyclic = new()
            {
                Value = new Column(-1, "occupancy", typeof(string)),
                Rule = new VisualUniqueValueFilterRule()
            };
            visualColumnTypologyFilter_Cyclic.Filter = visualColumnTypologyFilter_Cyclic;
            Assert.Null(Typology.Visual.Create.VisualTypology(table, visualColumnTypologyFilter_Cyclic, new Column("building_reference", typeof(string))));

            // A required reference column that is absent or unresolvable is refused; asking for no references leaves it free to be missing.
            Assert.Null(Typology.Visual.Create.VisualTypology(table, VisualTypologyFilter(), null));
            Assert.Null(Typology.Visual.Create.VisualTypology(table, VisualTypologyFilter(), new Column(-1, "storeys", typeof(int))));

            VisualTypology? visualTypology = Typology.Visual.Create.VisualTypology(table, VisualTypologyFilter(), null, null, false);
            Assert.NotNull(visualTypology);
            Assert.Empty(visualTypology.ReferenceSet(true));
        }
    }
}
