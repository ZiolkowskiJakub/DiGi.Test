using DiGi.Core.Classes;
using DiGi.Core.IO.Table.Classes;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace DiGi.Core.xUnit
{
    public partial class Tests
    {
        /// <summary>
        /// Tests the basic functionality of the Table class, including column initialization, row addition, and data retrieval.
        /// </summary>
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

            string? description = table.GetValue<string>(count - 1, 2);

            Assert.Equal($"Address {count - 1}", description);

            IEnumerable<Column> columns_Temp = table.Columns;

            foreach (Column column_Temp in columns_Temp)
            {
                Column? column = columns.Find(x => x.Name == column_Temp.Name);

                Assert.NotNull(column);
            }
        }

        /// <summary>
        /// Verifies that a <see cref="Table"/> object is correctly serialized to a JSON string and subsequently deserialized back into a <see cref="Table"/> instance, ensuring that the row and column counts remain consistent.
        /// </summary>
        [Fact]
        public void Table_Serialization()
        {
            Row Row = new(0);
            Row[1] = 10;
            Row[2] = 20;

            Table table_In = new Table();
            table_In.AddColumn(new Column("Empty", typeof(string)));
            table_In.AddColumn(new Column("Integer_1", typeof(int)));
            table_In.AddColumn(new Column("Integer_2", typeof(int)));

            table_In.AddRow([null, 10, 100]);
            table_In.AddRow([null, 20, 200]);
            table_In.AddRow([null, 30, 300]);
            table_In.AddRow([null, 40, 400]);

            JsonSerializerOptions jsonSerializerOptions = new();
            jsonSerializerOptions.Converters.Add(new TableConverter<Table, Column, Row>());

            // Zero string allocations - pipes the bytes directly into the destination stream asynchronously
            string json = JsonSerializer.Serialize(table_In, jsonSerializerOptions);
            Assert.True(!string.IsNullOrWhiteSpace(json));


            Table? table_Out = JsonSerializer.Deserialize<Table>(json, jsonSerializerOptions);
            Assert.NotNull(table_Out);

            Assert.True(table_In.RowCount == table_Out.RowCount);
            Assert.True(table_In.ColumnCount == table_Out.ColumnCount);

            for (int i = 0; i < table_In.RowCount; i++)
            {
                for (int j = 0; j < table_In.ColumnCount; j++)
                {
                    bool similar = false;
                    if(table_In[i, j] is null && table_Out[i, j] is null)
                    {
                        similar = true;
                    }
                    else if(table_In[i, j] is null || table_Out[i, j] is null)
                    {
                        similar = false;
                    }
                    else
                    {
                        similar = table_In[i, j]!.Equals(table_Out[i, j]);
                    }

                    Assert.True(similar);
                }
            }

        }

        /// <summary>
        /// Verifies that adding a new column to the table correctly assigns the specified name, type, and the expected zero-based index.
        /// </summary>
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

        /// <summary>
        /// Verifies that the GetColumnIndex method returns the correct zero-based index of a column when searched for by its name.
        /// </summary>
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

        /// <summary>
        /// Verifies that the AddRow method correctly inserts values into the table when a dictionary containing column names and corresponding values is provided.
        /// </summary>
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
            Assert.Equal("Jan Kowalski", table[0, 0]); // RowIndex 0, ColumnIndex 0
            Assert.Equal(30, table[0, 1]);             // RowIndex 0, ColumnIndex 1
        }

        /// <summary>
        /// Verifies that the SetValue method returns true and correctly stores the value in the table when the provided value matches the defined type of the column.
        /// </summary>
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

        /// <summary>
        /// Verifies that the SetValue method returns false when attempting to set a cell value with a type that does not match the column's defined type and conversion is disabled.
        /// </summary>
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

        /// <summary>
        /// Verifies that removing a row from the table decreases the total row count and ensures that subsequent rows are reindexed correctly.
        /// </summary>
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

        /// <summary>
        /// Verifies that removing a column from the table removes the corresponding values from all rows in the table.
        /// </summary>
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


        /// <summary>
        /// Verifies that the TryGetValue method returns true and the expected value when provided with valid row and column indices.
        /// </summary>
        [Fact]
        public void Table_TryGetValue_ShouldReturnCorrectValueAndTrue()
        {
            // Arrange
            Table table = new();
            table.AddColumn("Name", typeof(string));
            table.AddRow();
            table.SetValue(0, 0, "Test Value");

            // Act
            bool result = table.TryGetValue<string>(0, 0, out string? value);

            // Assert
            Assert.True(result);
            Assert.Equal("Test Value", value);
        }

        /// <summary>
        /// Verifies that the TryGetValue method returns false when provided with invalid row or column indices.
        /// </summary>
        [Fact]
        public void Table_TryGetValue_WithInvalidIndices_ShouldReturnFalse()
        {
            // Arrange
            Table table = new();
            table.AddColumn("Name", typeof(string));
            table.AddRow();

            // Act & Assert
            Assert.False(table.TryGetValue<string>(1, 0, out _)); // Invalid Col
            Assert.False(table.TryGetValue<string>(0, 1, out _)); // Invalid Row
        }

        /// <summary>
        /// Verifies that updating a column's type converts existing values to the new type when possible, or sets them to their default value if conversion fails.
        /// </summary>
        [Fact]
        public void Table_UpdateColumn_ChangeType_ShouldConvertValuesOrRemovethem()
        {
            // Arrange
            Table table = new();
            table.AddColumn("Value", typeof(string));
            table.AddRow([ "123" ]); 
            table.AddRow([ "NotANumber" ]);

            // Act: Change type to int (assuming Column has a way to tryConvert)
            // The implementation uses column_Temp.TryGetValidValue(value, out value, tryConvert)
            // We need to check if this actually works for the provided Column class.
            table.UpdateColumn(0, "Value", typeof(int), tryConvert: true);

            // Assert
            Assert.Equal(123, table[0, 0]); // Should be converted
            Assert.Equal(default(int), table[1, 0]); //Shoud be default
        }

        /// <summary>
        /// Verifies that the <see cref="DiGi.Core.IO.Table.Classes.Table"/>'s UpdateRow method correctly updates a row's cells when provided with a dictionary containing string keys and corresponding values.
        /// </summary>
        [Fact]
        public void Table_UpdateRow_WithDictionaryStringKeys_ShouldUpdateCorrectly()
        {
            // Arrange
            Table table = new();
            table.AddColumn("Name", typeof(string));
            table.AddColumn("Age", typeof(int));
            table.AddRow(["Old Name", 20]);

            var updates = new Dictionary<string, object?> { { "Name", "New Name" }, { "Age", 21 } };

            // Act
            table.UpdateRow(0, updates);

            // Assert
            Assert.Equal("New Name", table[0, 0]);
            Assert.Equal(21, table[0, 1]);
        }

        /// <summary>
        /// Verifies that the <see cref="DiGi.Core.IO.Table.Classes.Table"/>'s UpdateRow method correctly updates a row's cells when provided with an enumerable collection of values.
        /// </summary>
        [Fact]
        public void Table_UpdateRow_WithEnumerableValues_ShouldUpdateCorrectly()
        {
            // Arrange
            Table table = new();
            table.AddColumn("Col1", typeof(string));
            table.AddColumn("Col2", typeof(int));
            table.AddRow(["V1", 1]);

            var updates = new object[] { "V2", 2 };

            // Act
            table.UpdateRow(0, updates);

            // Assert
            Assert.Equal("V2", table[0, 0]);
            Assert.Equal(2, table[0, 1]);
        }


        /// <summary>
        /// Verifies that the <see cref="DiGi.Core.IO.Table.Classes.Table"/>'s UpdateRow method correctly updates a row's cells when provided with a dictionary where keys are integer column indices.
        /// </summary>
        [Fact]
        public void Table_UpdateRow_WithIntKeyDictionary_ShouldUpdateCorrectly()
        {
            // Arrange
            Table table = new();
            table.AddColumn("Col0", typeof(string));
            table.AddColumn("Col1", typeof(int));
            table.AddRow(["Old0", 0]);

            var updates = new Dictionary<int, object?> { { 0, "New0" }, { 1, 1 } };

            // Act
            table.UpdateRow(0, updates);

            // Assert
            Assert.Equal("New0", table[0, 0]);
            Assert.Equal(1, table[0, 1]);
        }

        /// <summary>
        /// Verifies that the <see cref="DiGi.Core.IO.Table.Classes.Table.UpdateColumn"/> method correctly updates only the name of a column when the provided type remains unchanged.
        /// </summary>
        [Fact]
        public void Table_UpdateColumn_RenameOnly_ShouldUpdateName()
        {
            // Arrange
            Table table = new();
            table.AddColumn("OldName", typeof(string));

            // Act
            var updatedCol = table.UpdateColumn(0, "NewName", typeof(string));

            // Assert
            Assert.NotNull(updatedCol);
            Assert.Equal("NewName", updatedCol.Name);
        }

        /// <summary>
        /// Verifies that the <see cref="DiGi.Core.IO.Table.Classes.Table"/>'s GetRow method returns the expected result when provided with both valid and invalid indices.
        /// </summary>
        [Fact]
        public void Table_GetRow_ValidAndInvalidIndex_ShouldReturnExpected()
        {
            // Arrange
            Table table = new();
            table.AddRow();

            // Act & Assert
            Assert.NotNull(table.GetRow(0));
            Assert.Null(table.GetRow(-1));
            Assert.Null(table.GetRow(1));
        }
    }
}
