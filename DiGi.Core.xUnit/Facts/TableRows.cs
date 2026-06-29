using System.Collections.Generic;
using System.Linq;
using DiGi.Core.IO.Table.Classes;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that Table.Rows still returns cloned rows (not the same instances as the internal storage)
        /// after replacing the ToList().ConvertAll() double-materialization with a single Select().ToList() pass.
        /// </summary>
        [Fact]
        public void Table_Rows_ReturnsClonedRowsInOrder()
        {
            Table table = new();
            table.AddColumn(new Column("Col0", typeof(string)));
            table.AddColumn(new Column("Col1", typeof(int)));

            table.AddRow(["A", 1]);
            table.AddRow(["B", 2]);
            table.AddRow(["C", 3]);

            List<Row> rows = table.Rows.ToList();

            Assert.Equal(3, rows.Count);
            Assert.Equal("A", rows[0][0]);
            Assert.Equal(1, rows[0][1]);
            Assert.Equal("B", rows[1][0]);
            Assert.Equal(2, rows[1][1]);
            Assert.Equal("C", rows[2][0]);
            Assert.Equal(3, rows[2][1]);

            // Rows are clones - mutating the returned row must not affect the table's internal state.
            rows[0][0] = "Mutated";
            List<Row> rows_Again = table.Rows.ToList();
            Assert.Equal("A", rows_Again[0][0]);
        }
    }
}
