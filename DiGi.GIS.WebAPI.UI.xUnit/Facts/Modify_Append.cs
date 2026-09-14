using DiGi.Core.IO.Table.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the name-based merge of building data pages (#22): a page whose column order differs from the table's is
        /// re-aligned by column name rather than trusted to its position, null cells are not filed, a null target adopts the
        /// page as the new table, and a null page leaves the target unchanged.
        /// </summary>
        [Fact]
        public void Modify_Append()
        {
            Table into = new([
                new Column(0, "Reference", typeof(string)),
                new Column(1, "Storeys", typeof(ushort)),
                new Column(2, "Internal Point X", typeof(double))
            ]);
            into.AddRow(new Dictionary<int, object?>() { [0] = "A1", [1] = (ushort)2, [2] = 100.0 });

            // The page carries the same columns in a different order, and a cell it does not carry at all is left null.
            Table page = new([
                new Column(0, "Storeys", typeof(ushort)),
                new Column(1, "Internal Point X", typeof(double)),
                new Column(2, "Reference", typeof(string))
            ]);
            page.AddRow(new Dictionary<int, object?>() { [0] = (ushort)3, [1] = 200.0, [2] = "B1" });
            page.AddRow(new Dictionary<int, object?>() { [0] = (ushort)1, [2] = "C1" });

            Table? result = into.Append(page);
            Assert.Same(into, result);
            Assert.Equal(3, result!.RowCount);

            List<Row> rows = [.. result.Rows];

            DiGi.Core.IO.Table.Classes.Row row_1 = rows[1];
            Assert.Equal("B1", row_1[0]);
            Assert.Equal((ushort)3, row_1[1]);
            Assert.Equal(200.0, row_1[2]);

            // The page's second row omits Internal Point X; the merge does not invent a value for it.
            DiGi.Core.IO.Table.Classes.Row row_2 = rows[2];
            Assert.Equal("C1", row_2[0]);
            Assert.Equal((ushort)1, row_2[1]);
            Assert.Null(row_2[2]);

            // A null target adopts the page as the new table; a null page leaves the target unchanged.
            Table? adopted = ((Table?)null).Append(page);
            Assert.Same(page, adopted);
            Table? unchanged = into.Append(null);
            Assert.Same(into, unchanged);
            Assert.Equal(3, unchanged!.RowCount);
        }
    }
}
