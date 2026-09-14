using DiGi.Core.IO.Table.Classes;
using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the clip of a building data table to an area boundary (#22): the rows whose internal point falls inside the
        /// polygon are kept, the rows outside are dropped, and the input table is left untouched. A null polygon, and a table
        /// that carries neither internal-point column, leave the table unchanged rather than reporting the area as empty.
        /// </summary>
        [Fact]
        public void ClipByPolygon()
        {
            Table table = new([
                new Column(0, "Reference", typeof(string)),
                new Column(1, "Internal Point X", typeof(double)),
                new Column(2, "Internal Point Y", typeof(double))
            ]);
            table.AddRow(new Dictionary<int, object?>() { [0] = "In1", [1] = 5.0, [2] = 5.0 });
            table.AddRow(new Dictionary<int, object?>() { [0] = "Out1", [1] = 50.0, [2] = 5.0 });
            table.AddRow(new Dictionary<int, object?>() { [0] = "In2", [1] = 1.0, [2] = 9.0 });
            table.AddRow(new Dictionary<int, object?>() { [0] = "Out2", [1] = 1.0, [2] = 20.0 });

            List<Point2D> points = [new(0, 0), new(10, 0), new(10, 10), new(0, 10)];
            PolygonalFace2D polygonalFace2D = new Polygon2D(points).PolygonalFace2D()!;

            Table? clipped = table.ClipByPolygon(polygonalFace2D);
            Assert.NotNull(clipped);

            // The clip answers a new table; the caller's table must not change under it.
            Assert.NotSame(table, clipped);
            Assert.Equal(4, table.RowCount);

            Assert.Equal(2, clipped!.RowCount);
            List<string> references = [];
            foreach (Row row in clipped.Rows)
            {
                references.Add(row[0] as string ?? string.Empty);
            }
            Assert.Contains("In1", references);
            Assert.Contains("In2", references);
            Assert.DoesNotContain("Out1", references);
            Assert.DoesNotContain("Out2", references);

            // A null polygon leaves the table unchanged.
            Table? unchanged = table.ClipByPolygon(null);
            Assert.Same(table, unchanged);

            // A table that carries neither internal-point column is left as it came.
            Table noPoints = new([new Column(0, "Reference", typeof(string))]);
            noPoints.AddRow(new Dictionary<int, object?>() { [0] = "A" });
            Table? unchanged_NoPoints = noPoints.ClipByPolygon(polygonalFace2D);
            Assert.Same(noPoints, unchanged_NoPoints);
            Assert.Single(unchanged_NoPoints!.Rows);
        }
    }
}
