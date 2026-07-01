using DiGi.Core.IO.Table.Classes;
using System.Collections.Generic;
using System.Text.Json;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests Row and Table functionalities, including empty row handling, column shifting during removal, and JSON serialization.
        /// </summary>
        [Fact]
        public void Table_OperationsAndSerialization()
        {
            // 1. Test Row.GetValues on empty row (Verifying the fix for Bug 2)
            Row row_Empty = new(0);
            object?[] values_Empty = row_Empty.GetValues();
            Assert.NotNull(values_Empty);
            Assert.Empty(values_Empty);

            // 2. Test Table.RemoveColumns and shifting (Verifying the fix for Bug 1)
            Table table_Test = new();

            // Add three columns
            Column? column_0 = table_Test.AddColumn(new Column("Col0", typeof(string)));
            Column? column_1 = table_Test.AddColumn(new Column("Col1", typeof(int)));
            Column? column_2 = table_Test.AddColumn(new Column("Col2", typeof(double)));

            // Add a row with values (using collection plural naming)
            List<object?> rowValues = ["A", 10, 2.5];
            Row? row_Added = table_Test.AddRow(rowValues);
            Assert.NotNull(row_Added);

            // Verify initial values
            Assert.Equal("A", table_Test[0, 0]);
            Assert.Equal(10, table_Test[0, 1]);
            Assert.Equal(2.5, table_Test[0, 2]);

            // Remove column at index 1 (middle column). This should shift Col2 to index 1 and its value 2.5 to index 1
            Assert.True(table_Test.RemoveColumn(1));

            // Verify column count is now 2
            Assert.Equal(2, table_Test.ColumnCount);

            // Verify that the remaining columns and their values are correctly preserved and shifted
            Assert.Equal("A", table_Test[0, 0]);
            Assert.Equal(2.5, table_Test[0, 1]); // Col2 shifted to index 1
            Assert.Null(table_Test[0, 2]);       // Index 2 is now empty/null

            // 3. Test TableConverter deserialization with column/row mismatch (Verifying the fix for Bug 3)
            // Create a JSON representing a table with 2 columns, but where a row has 3 elements
            string string_Json = @"{
                ""Columns"": [
                    { ""Index"": 0, ""Name"": ""Col0"", ""Type"": ""System.String"" },
                    { ""Index"": 1, ""Name"": ""Col1"", ""Type"": ""System.Int32"" }
                ],
                ""Rows"": [
                    [ ""A"", 10, 2.5 ]
                ]
            }";

            // Deserialize using TableConverter
            JsonSerializerOptions jsonSerializerOptions = new();
            jsonSerializerOptions.Converters.Add(new TableConverter<Table, Column, Row>());
            Table? table_Deserialized = JsonSerializer.Deserialize<Table>(string_Json, jsonSerializerOptions);
            Assert.NotNull(table_Deserialized);
            Assert.Equal(2, table_Deserialized.ColumnCount);
            Assert.Equal(1, table_Deserialized.RowCount);

            // Verify that the extra element was ignored or handled and didn't crash
            Assert.Equal("A", table_Deserialized[0, 0]);
            Assert.Equal(10, table_Deserialized[0, 1]);
        }
    }
}