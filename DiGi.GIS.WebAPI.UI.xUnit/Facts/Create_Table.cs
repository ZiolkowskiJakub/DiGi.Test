using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the parse of one page of building data (#22) against the deployed GIS Web API's actual wire JSON
        /// (DiGi.Test/files/BuildingDataTable_Sample.json): every column arrives with its canonical name, index and CLR type,
        /// each row's cells are typed to the column's declared type, and the reference and internal-point columns - the two
        /// keys the paging cursor and the clip read - resolve by name. A null or blank input answers null.
        /// </summary>
        [Fact]
        public void Create_Table()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "BuildingDataTable_Sample.json");
            string json = File.ReadAllText(path!);

            DiGi.Core.IO.Table.Classes.Table? table = DiGi.GIS.WebAPI.UI.Create.Table(json);
            Assert.NotNull(table);

            List<DiGi.Core.IO.Table.Classes.Column> columns = [.. table!.Columns];
            Assert.Equal(5, columns.Count);
            Assert.Equal(["Storeys", "Internal Point X", "Internal Point Y", "Reference", "County Id"], columns.Select(c => c.Name).ToList());
            Assert.Equal(typeof(ushort), columns.Single(c => c.Name == "Storeys").Type);
            Assert.Equal(typeof(double), columns.Single(c => c.Name == "Internal Point X").Type);
            Assert.Equal(typeof(double), columns.Single(c => c.Name == "Internal Point Y").Type);
            Assert.Equal(typeof(string), columns.Single(c => c.Name == "Reference").Type);
            Assert.Equal(typeof(int), columns.Single(c => c.Name == "County Id").Type);

            List<DiGi.Core.IO.Table.Classes.Row> rows = [.. table.Rows];
            Assert.Equal(3, rows.Count);

            // The cells are typed to the column's declared type, not carried as JSON primitives.
            DiGi.Core.IO.Table.Classes.Row row_0 = rows[0];
            Assert.Equal((ushort)2, row_0[0]);
            Assert.Equal("00085c3f-bde5-40aa-b466-ba60b7e2f672", row_0[3]);
            Assert.Equal((int)55417, row_0[4]);

            // The two columns the client reads back by name: the paging cursor and the clip's coordinates.
            int index_Reference = table.GetColumnIndex("Reference");
            Assert.Equal(3, index_Reference);
            Assert.Equal("000cceb7-255c-46af-adeb-7d3df4102b9f", rows[^1][index_Reference]);

            int index_InternalPointX = table.GetColumnIndex("Internal Point X");
            int index_InternalPointY = table.GetColumnIndex("Internal Point Y");
            Assert.Equal(1, index_InternalPointX);
            Assert.Equal(2, index_InternalPointY);
            Assert.True(rows[^1][index_InternalPointX] is double);
            Assert.True(rows[^1][index_InternalPointY] is double);

            // A null or blank input answers null.
            Assert.Null(DiGi.GIS.WebAPI.UI.Create.Table(null));
            Assert.Null(DiGi.GIS.WebAPI.UI.Create.Table(""));
            Assert.Null(DiGi.GIS.WebAPI.UI.Create.Table("   "));
        }
    }
}
