using DiGi.Core.Classes;
using DiGi.Core.IO.Table.Classes;
using System.Collections.Generic;

namespace DiGi.Core.xUnit
{
    public partial class Tests
    {
        [Fact]
        public void Table()
        {
            List<Column> columns = [new Column("Index", typeof(int)), new Column("Address", typeof(Address)), new Column("Description", typeof(string))];

            Table table = new(columns);

            Assert.Equal(3, table.ColumnCount);

            Assert.Equal(2, table.GetColumnIndex("Description"));

            int count = 100;

            for (int i = 0; i < count; i++)
            {
                table.AddRow([1, new Address($"Street {i}", "Warsaw", "00-409", Core.Enums.CountryCode.PL), $"Address {i}"]);
            }

            Assert.Equal(count, table.RowCount);

            string? description = table.GetValue<string>(2, count - 1);

            Assert.Equal($"Address {count - 1}", description);

            IEnumerable<Column> columns_Temp = table.Columns;

            foreach (Column column_Temp in columns_Temp)
            {
                Column? column = columns.Find(x => x.Name == column_Temp.Name);

                Assert.NotNull(column);
            }
        }
    }
}