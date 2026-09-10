using DiGi.Core.IO.Table.Classes;
using DiGi.Typology.Classes;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a chain declared over the shared column constants classifies correctly against a table whose column order differs from theirs, and that neither the caller's chain nor the constants themselves are modified in the process.
        /// <para>A column index belongs to the table a column was added to, and the constants are shared static fields whose Index is -1 and whose setter is public. Resolving by unique id rather than assigning an index to a constant is what keeps one classification from corrupting the next.</para>
        /// </summary>
        [Fact]
        public void Typology_ColumnResolution()
        {
            Table table = TypologyTable();

            ColumnTypologyFilter<Column> columnTypologyFilter = TypologyFilter();

            Assert.NotNull(columnTypologyFilter.Value);
            Assert.Equal(-1, columnTypologyFilter.Value.Index);

            ColumnTypologyFilter<Column>? columnTypologyFilter_Level2 = columnTypologyFilter.Filter;
            Assert.NotNull(columnTypologyFilter_Level2);

            Typology.Classes.Typology? typology = Create.Typology(table, columnTypologyFilter, IO.Constants.Column.Reference);
            Assert.NotNull(typology);

            Assert.Equal(7, Typology.Query.ReferenceSet(typology, true).Count);

            Assert.Equal(-1, columnTypologyFilter.Value.Index);
            Assert.Same(columnTypologyFilter_Level2, columnTypologyFilter.Filter);

            Assert.Equal(-1, IO.Constants.Column.CountyName.Index);
            Assert.Equal(-1, IO.Constants.Column.IsOccupied.Index);
            Assert.Equal(-1, IO.Constants.Column.PredictedYearBuilt.Index);
            Assert.Equal(-1, IO.Constants.Column.Reference.Index);

            Assert.True(table.TryGetColumn("County name", out Column? column_CountyName));
            Assert.NotNull(column_CountyName);
            Assert.NotEqual(-1, column_CountyName.Index);
            Assert.NotSame(IO.Constants.Column.CountyName, column_CountyName);
        }
    }
}
