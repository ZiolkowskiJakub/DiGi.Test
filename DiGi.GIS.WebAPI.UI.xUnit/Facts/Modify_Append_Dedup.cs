using DiGi.Core.IO.Table.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the deduplicating <see cref="Modify.Append(Table?, Table?, string?, HashSet{string}?)"/> the paged read merges its pages with (DiGi.GIS.WebAPI.UI#51).
        /// <para>A key already appended is skipped, whether it came from an earlier page or earlier in the same page, and the first copy is the one kept. Columns merge by name when a page lists them in another order. A row without a key value is always appended. With no key column the overload is the plain append, and a null or empty page leaves the table as it was.</para>
        /// </summary>
        [Fact]
        public void Modify_Append_Dedup()
        {
            HashSet<string> keys = [];

            Table page_1 = new([new Column(0, "Reference", typeof(string)), new Column(1, "Value", typeof(int))]);
            page_1.AddRow(new Dictionary<int, object?>() { [0] = "R1", [1] = 1 });
            page_1.AddRow(new Dictionary<int, object?>() { [0] = "R2", [1] = 2 });
            page_1.AddRow(new Dictionary<int, object?>() { [0] = "R2", [1] = 99 });

            Table? table = Modify.Append(null, page_1, "Reference", keys);
            Assert.NotNull(table);
            Assert.Equal(2, table.RowCount);

            // The second page lists its columns in the other order, repeats R1 and brings a row without a key.
            Table page_2 = new([new Column(0, "Value", typeof(int)), new Column(1, "Reference", typeof(string))]);
            page_2.AddRow(new Dictionary<int, object?>() { [0] = 100, [1] = "R1" });
            page_2.AddRow(new Dictionary<int, object?>() { [0] = 3, [1] = "R3" });
            page_2.AddRow(new Dictionary<int, object?>() { [0] = 4 });

            table = Modify.Append(table, page_2, "Reference", keys);
            Assert.NotNull(table);
            Assert.Equal(4, table.RowCount);

            int index_Reference = table.GetColumnIndex("Reference");
            int index_Value = table.GetColumnIndex("Value");
            List<(object? Reference, object? Value)> rows = [.. table.Select(x => (x[index_Reference], x[index_Value]))];
            Assert.Equal([("R1", 1), ("R2", 2), ("R3", 3), (null, 4)], rows);
            Assert.Equal(["R1", "R2", "R3"], keys.Order());

            // No key column: the plain append.
            Table page_NoKey = new([new Column(0, "Value", typeof(int))]);
            page_NoKey.AddRow(new Dictionary<int, object?>() { [0] = 5 });
            Assert.Equal(5, Modify.Append(table, page_NoKey, "Reference", keys)!.RowCount);
            Assert.Equal(6, Modify.Append(table, page_NoKey, null, null)!.RowCount);

            // A null or empty page leaves the table as it was.
            Assert.Same(table, Modify.Append(table, null, "Reference", keys));
            Assert.Same(table, Modify.Append(table, new Table([new Column(0, "Reference", typeof(string))]), "Reference", keys));
            Assert.Null(Modify.Append(null, null, "Reference", keys));
        }
    }
}
