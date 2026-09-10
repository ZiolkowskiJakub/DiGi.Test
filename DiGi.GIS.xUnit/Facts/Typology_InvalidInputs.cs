using DiGi.Core.IO.Table.Classes;
using DiGi.Typology.Classes;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that Create.Typology returns null rather than throwing for every input it cannot classify.
        /// <para>A missing reference column is refused only when references are actually being stored - asking for references while naming no column to take them from is the case a guard that skips absent input would let through.</para>
        /// <para>A chain that cannot be honoured in full is refused rather than solved in part, because a tree ending above the level that was asked for looks like an answer.</para>
        /// </summary>
        [Fact]
        public void Typology_InvalidInputs()
        {
            Table table = TypologyTable();

            Assert.Null(Create.Typology(null, TypologyFilter(), IO.Constants.Column.Reference));
            Assert.Null(Create.Typology(table, null, IO.Constants.Column.Reference));
            Assert.Null(Create.Typology(table, new ColumnTypologyFilter<Column>(), IO.Constants.Column.Reference));

            ColumnTypologyFilter<Column> columnTypologyFilter_Absent = new()
            {
                Value = IO.Constants.Column.Storeys,
                Rule = new UniqueValueFilterRule()
            };
            Assert.Null(Create.Typology(table, columnTypologyFilter_Absent, IO.Constants.Column.Reference));

            ColumnTypologyFilter<Column> columnTypologyFilter_AbsentNested = new()
            {
                Value = IO.Constants.Column.CountyName,
                Rule = new UniqueValueFilterRule(),
                Filter = columnTypologyFilter_Absent
            };
            Assert.Null(Create.Typology(table, columnTypologyFilter_AbsentNested, IO.Constants.Column.Reference));

            // A level carrying no rule groups nothing, so the solver would end the tree above it.
            ColumnTypologyFilter<Column> columnTypologyFilter_NoRule = new()
            {
                Value = IO.Constants.Column.CountyName,
                Rule = new UniqueValueFilterRule(),
                Filter = new ColumnTypologyFilter<Column>()
                {
                    Value = IO.Constants.Column.IsOccupied
                }
            };
            Assert.Null(Create.Typology(table, columnTypologyFilter_NoRule, IO.Constants.Column.Reference));

            // A chain linking back on itself would otherwise be solved as a silently truncated one.
            ColumnTypologyFilter<Column> columnTypologyFilter_Cyclic = new()
            {
                Value = IO.Constants.Column.CountyName,
                Rule = new UniqueValueFilterRule()
            };
            columnTypologyFilter_Cyclic.Filter = columnTypologyFilter_Cyclic;
            Assert.Null(Create.Typology(table, columnTypologyFilter_Cyclic, IO.Constants.Column.Reference));

            Assert.Null(Create.Typology(table, TypologyFilter(), null));
            Assert.Null(Create.Typology(table, TypologyFilter(), IO.Constants.Column.Storeys));

            DiGi.Typology.Classes.Typology? typology = Create.Typology(table, TypologyFilter(), null, null, false);
            Assert.NotNull(typology);
            Assert.Empty(DiGi.Typology.Query.ReferenceSet(typology, true));
        }
    }
}
