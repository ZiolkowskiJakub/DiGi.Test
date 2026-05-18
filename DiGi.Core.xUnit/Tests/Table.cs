using DiGi.Core.Classes;
using DiGi.Core.IO.Table.Classes;
using System;
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

        [Fact]
        public void Table_AddColumn_ShouldAssignCorrectIndexAndName()
        {
            // Arrange
            Table table = new();
            string colName = "TestColumn";
            Type colType = typeof(int);

            // Act
            var column = table.AddColumn(colName, colType);

            // Assert
            Assert.NotNull(column);
            Assert.Equal(colName, column.Name);
            Assert.Equal(colType, column.Type);
            Assert.Equal(0, column.Index);
            Assert.Equal(1, table.ColumnCount);
        }

        [Fact]
        public void Table_GetColumnIndex_ByName_ShouldReturnCorrectIndex()
        {
            // Arrange
            Table table = new();
            table.AddColumn("Col1", typeof(string));
            table.AddColumn("Col2", typeof(int));

            // Act
            int index = table.GetColumnIndex("Col2");

            // Assert
            Assert.Equal(1, index);
        }

        [Fact]
        public void Table_AddRow_WithDictionary_ShouldInsertValuesCorrectly()
        {
            // Arrange
            Table table = new();
            table.AddColumn("Name", typeof(string));
            table.AddColumn("Age", typeof(int));

            var rowData = new Dictionary<string, object?>
            {
                { "Name", "Jan Kowalski" },
                { "Age", 30 }
            };

            // Act
            var row = table.AddRow(rowData);

            // Assert
            Assert.NotNull(row);
            Assert.Equal("Jan Kowalski", table[0, 0]); // ColIndex 0, RowIndex 0
            Assert.Equal(30, table[1, 0]);             // ColIndex 1, RowIndex 0
        }

        [Fact]
        public void Table_SetValue_WithCorrectType_ShouldReturnTrueAndStoreValue()
        {
            // Arrange
            Table table = new();
            table.AddColumn("Price", typeof(decimal));
            table.AddRow(); // Create row at index 0

            // Act
            bool result = table.SetValue(0, 0, 99.99m);

            // Assert
            Assert.True(result);
            Assert.Equal(99.99m, table[0, 0]);
        }

        [Fact]
        public void Table_SetValue_WithInvalidType_ShouldReturnFalse()
        {
            // Arrange
            Table table = new();
            table.AddColumn("Age", typeof(int));
            table.AddRow();

            bool result = table.SetValue(0, 0, "NotANumber", tryConvert: false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Table_RemoveRow_ShouldDecreaseRowCountAndReindex()
        {
            // Arrange
            Table table = new();
            table.AddRow(); // 0
            table.AddRow(); // 1
            table.AddRow(); // 2

            // Act
            table.RemoveRow(1);

            // Assert
            Assert.Equal(2, table.RowCount);
            Assert.NotNull(table.GetRow(0));
            Assert.NotNull(table.GetRow(1));
            Assert.Null(table.GetRow(2));
        }

        [Fact]
        public void Table_RemoveColumn_ShouldRemoveValueFromAllRows()
        {
            // Arrange
            Table table = new();
            table.AddColumn("Col1", typeof(string));
            table.AddColumn("Col2", typeof(string));

            table.AddRow(new Dictionary<string, object?> { { "Col1", "A" }, { "Col2", "B" } });
            table.AddRow(new Dictionary<string, object?> { { "Col1", "C" }, { "Col2", "D" } });

            // Act
            table.RemoveColumn(0);

            // Assert
            Assert.Equal(1, table.ColumnCount);
            Row? row = table.GetRow(0);
            Assert.Equal("B", row?[0]);
        }

        [Fact]
        public void Table_Indexer_WithColumnAndRowObjects_ShouldReturnCorrectValue()
        {
            // Arrange
            Table table = new();
            var col = table.AddColumn("Test", typeof(string));
            var row = table.AddRow();
            table.SetValue(col.Index, row.Index, "Hello World");

            // Act
            var value = table[col, row];

            // Assert
            Assert.Equal("Hello World", value);
        }
    }
}