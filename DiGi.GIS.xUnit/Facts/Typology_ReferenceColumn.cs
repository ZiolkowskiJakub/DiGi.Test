using DiGi.Core.IO.Table.Classes;
using DiGi.Typology.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that ReferenceColumnTypologyFilterSolver identifies a row by the value of its reference column, where the inherited identification cannot.
        /// <para>A table row implements neither IUniqueObject nor ToString, so the inherited identification returns the same type name for every row. Because a node stores its references in a set, every node then collapses to one meaningless reference - which is what this fact demonstrates before showing the subclass resolving it.</para>
        /// </summary>
        [Fact]
        public void Typology_ReferenceColumn()
        {
            Table table = TypologyTable();

            Assert.True(table.TryGetColumn("County name", out Column? column_CountyName));
            Assert.NotNull(column_CountyName);

            Assert.True(table.TryGetColumn("Reference", out Column? column_Reference));
            Assert.NotNull(column_Reference);

            ColumnTypologyFilter<Column> columnTypologyFilter = new()
            {
                Value = column_CountyName,
                Rule = new UniqueValueFilterRule()
            };

            ColumnTypologyFilterSolver<Column, Row> columnTypologyFilterSolver = new()
            {
                Input = columnTypologyFilter,
                Objects = table
            };

            Assert.True(columnTypologyFilterSolver.Solve());
            Assert.NotNull(columnTypologyFilterSolver.Output);

            HashSet<string> references_Inherited = Typology.Query.ReferenceSet(columnTypologyFilterSolver.Output, true);
            Assert.Single(references_Inherited);
            Assert.Contains(typeof(Row).FullName!, references_Inherited);

            Classes.ReferenceColumnTypologyFilterSolver referenceColumnTypologyFilterSolver = new()
            {
                Input = columnTypologyFilter,
                Objects = table,
                ReferenceColumn = column_Reference
            };

            Assert.True(referenceColumnTypologyFilterSolver.Solve());
            Assert.NotNull(referenceColumnTypologyFilterSolver.Output);

            HashSet<string> references = Typology.Query.ReferenceSet(referenceColumnTypologyFilterSolver.Output, true);
            Assert.Equal(7, references.Count);
            Assert.Contains("b1", references);
            Assert.Contains("b7", references);
            Assert.DoesNotContain(typeof(Row).FullName!, references);

            // A blank cell is not an identity. Stored as one it would sit on every node from the matched row
            // up to the root, saying nothing about any object.
            Table table_Blank = new();
            Assert.NotNull(table_Blank.AddColumn(IO.Constants.Column.CountyName));
            Assert.NotNull(table_Blank.AddColumn(IO.Constants.Column.Reference));

            List<object?> values_Reference = ["Alpha", "b1"];
            List<object?> values_Blank = ["Alpha", "   "];
            Assert.NotNull(table_Blank.AddRow(values_Reference));
            Assert.NotNull(table_Blank.AddRow(values_Blank));

            ColumnTypologyFilter<Column> columnTypologyFilter_Blank = new()
            {
                Value = IO.Constants.Column.CountyName,
                Rule = new UniqueValueFilterRule()
            };

            Typology.Classes.Typology? typology_Blank = Create.Typology(table_Blank, columnTypologyFilter_Blank, IO.Constants.Column.Reference);
            Assert.NotNull(typology_Blank);

            HashSet<string> references_Blank = Typology.Query.ReferenceSet(typology_Blank, true);
            Assert.Single(references_Blank);
            Assert.Contains("b1", references_Blank);
        }
    }
}
