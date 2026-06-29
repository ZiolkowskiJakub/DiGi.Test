using DiGi.Core.IO.DelimitedData;
using DiGi.Core.IO.DelimitedData.Classes;
using DiGi.Core.IO.Table.Classes;
using System.IO;
using System.Text;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that Table.Write still writes the correct header and row values after replacing
        /// the throwaway ToList().ConvertAll() call
        /// with a direct Select() projection for the column-name header row.
        /// </summary>
        [Fact]
        public void Table_Write_WritesHeaderAndRows()
        {
            Table table = new();
            table.AddColumn(new Column("Col0", typeof(string)));
            table.AddColumn(new Column("Col1", typeof(int)));

            table.AddRow(["A", 1]);
            table.AddRow(["B", 2]);

            using MemoryStream stream = new();
            using (DelimitedDataWriter writer = new(',', stream) { AutoFlush = true })
            {
                bool result = table.Write(writer);
                Assert.True(result);
            }

            string text = Encoding.UTF8.GetString(stream.ToArray()).Replace("\r\n", "\n");

            Assert.Contains("Col0,Col1", text);
            Assert.Contains("A,1", text);
            Assert.Contains("B,2", text);
        }
    }
}
