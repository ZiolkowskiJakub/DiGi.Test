using DiGi.Geometry.Planar.Classes;
using System.Collections.Generic;

namespace DiGi.BDOT10k.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="Convert.ToDiGi(GML.Classes.LinearRing?)"/> removes the duplicate closing point from a closed ring.
        /// <para>OGC GML linear rings repeat the starting coordinate pair as the final pair. Verifies that the converted <see cref="Polygon2D"/> contains no redundant closing vertex and produces no zero-length segments.</para>
        /// </summary>
        [Fact]
        public void Polygon2D_ClosedRing_RemovesDuplicateClosingPoint()
        {
            GML.Classes.LinearRing linearRing = new()
            {
                posList = [0, 0, 10, 0, 10, 10, 0, 10, 0, 0]
            };

            Polygon2D? polygon2D = linearRing.ToDiGi();

            Assert.NotNull(polygon2D);

            List<Point2D>? point2Ds = polygon2D.GetPoints();
            Assert.NotNull(point2Ds);
            Assert.Equal(4, point2Ds.Count);
            Assert.Equal(new Point2D(0, 0), point2Ds[0]);
            Assert.Equal(new Point2D(10, 0), point2Ds[1]);
            Assert.Equal(new Point2D(10, 10), point2Ds[2]);
            Assert.Equal(new Point2D(0, 10), point2Ds[3]);

            List<Segment2D>? segment2Ds = polygon2D.GetSegments();
            Assert.NotNull(segment2Ds);
            Assert.Equal(4, segment2Ds.Count);
            foreach (Segment2D segment2D in segment2Ds)
            {
                Assert.True(segment2D.Length > 0);
            }
        }

        /// <summary>
        /// Tests that <see cref="Convert.ToDiGi(GML.Classes.LinearRing?)"/> correctly converts an already open ring.
        /// </summary>
        [Fact]
        public void Polygon2D_OpenRing()
        {
            GML.Classes.LinearRing linearRing = new()
            {
                posList = [0, 0, 10, 0, 10, 10, 0, 10]
            };

            Polygon2D? polygon2D = linearRing.ToDiGi();

            Assert.NotNull(polygon2D);

            List<Point2D>? point2Ds = polygon2D.GetPoints();
            Assert.NotNull(point2Ds);
            Assert.Equal(4, point2Ds.Count);
        }

        /// <summary>
        /// Tests that <see cref="Convert.ToDiGi(GML.Classes.LinearRing?)"/> returns null for degenerate rings with fewer than 3 unique vertices.
        /// </summary>
        [Fact]
        public void Polygon2D_DegenerateRing()
        {
            GML.Classes.LinearRing linearRing = new()
            {
                posList = [0, 0, 10, 0, 0, 0]
            };

            Polygon2D? polygon2D = linearRing.ToDiGi();

            Assert.Null(polygon2D);
        }

        /// <summary>
        /// Tests that <see cref="Convert.ToDiGi(GML.Classes.LinearRing?)"/> returns null for null, empty, or odd-length coordinate inputs.
        /// </summary>
        [Fact]
        public void Polygon2D_InvalidInput()
        {
            GML.Classes.LinearRing? linearRing_Null = null;
            Assert.Null(linearRing_Null.ToDiGi());

            GML.Classes.LinearRing linearRing_Empty = new()
            {
                posList = []
            };
            Assert.Null(linearRing_Empty.ToDiGi());

            GML.Classes.LinearRing linearRing_Odd = new()
            {
                posList = [0, 0, 10]
            };
            Assert.Null(linearRing_Odd.ToDiGi());
        }
    }
}
