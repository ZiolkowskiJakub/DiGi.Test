using DiGi.Core.IO.DelimitedData.Classes;
using System.IO;
using System.Text;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests reading a standard CSV string using DelimitedDataReader, verifying headers, row values, type conversion of elements, and termination of reading.
        /// </summary>
        [Fact]
        public void DelimitedDataReader_StandardCsv()
        {
            string csvData = "ID,Name,Age\n1,Jan Kowalski,30\n2,Anna Nowak,25";
            using MemoryStream stream = new(Encoding.UTF8.GetBytes(csvData));
            using DelimitedDataReader reader = new(',', stream);

            DelimitedDataRow? headers = reader.ReadRow();
            Assert.NotNull(headers);
            Assert.Equal(3, headers.Count);
            Assert.Equal("ID", headers[0]);
            Assert.Equal("Name", headers[1]);
            Assert.Equal("Age", headers[2]);

            DelimitedDataRow? row1 = reader.ReadRow();
            Assert.NotNull(row1);
            Assert.Equal("1", row1[0]);
            Assert.Equal("Jan Kowalski", row1[1]);
            Assert.Equal("30", row1[2]);

            // Try conversion on row elements
            bool hasId = row1.TryGetValue(0, out int idVal);
            Assert.True(hasId);
            Assert.Equal(1, idVal);

            bool hasAge = row1.TryGetValue(2, out int ageVal);
            Assert.True(hasAge);
            Assert.Equal(30, ageVal);

            DelimitedDataRow? row2 = reader.ReadRow();
            Assert.NotNull(row2);
            Assert.Equal("Anna Nowak", row2[1]);

            DelimitedDataRow? row3 = reader.ReadRow();
            Assert.Null(row3);
        }

        /// <summary>
        /// Tests reading a delimited string with custom semicolon separator and nested/escaped double quotes.
        /// </summary>
        [Fact]
        public void DelimitedDataReader_CustomDelimiterAndQuotes()
        {
            // Semicolon separator, with double quotes around fields containing quotes/spaces
            string csvData = "ID;Name;Comment\n1;\"John \"\"The Boss\"\" Davis\";\"Special; comment\"";
            using MemoryStream stream = new(Encoding.UTF8.GetBytes(csvData));
            using DelimitedDataReader reader = new(';', stream);

            DelimitedDataRow? headers = reader.ReadRow();
            Assert.NotNull(headers);

            DelimitedDataRow? row = reader.ReadRow();
            Assert.NotNull(row);
            Assert.Equal(3, row.Count);
            Assert.Equal("1", row[0]);
            Assert.Equal("John \"The Boss\" Davis", row[1]); // Escaped quotes resolved
            Assert.Equal("Special; comment", row[2]);       // Semicolon inside quotes preserved
        }

        /// <summary>
        /// Tests writing rows to a CSV stream using DelimitedDataWriter, ensuring correct formatting and separator handling.
        /// </summary>
        [Fact]
        public void DelimitedDataWriter_WritingRows()
        {
            using MemoryStream stream = new();
            // Wrap writer, but leave open so we can read the memory stream
            using (DelimitedDataWriter writer = new(',', stream) { AutoFlush = true })
            {
                DelimitedDataRow row1 = new(["ID", "Name"]);
                DelimitedDataRow row2 = new(["1", "John \"The Boss\""]);

                writer.WriteRow(row1);
                writer.WriteRow(row2);
            }

            string result = Encoding.UTF8.GetString(stream.ToArray());
            // Normalize line endings
            result = result.Replace("\r\n", "\n");

            // DelimitedDataRow.Text uses custom separator formatting. Let's make sure it writes the expected text.
            Assert.Contains("ID,Name", result);
        }
    }
}