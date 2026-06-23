using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the creation of a 2D mesh from a polygonal face.
        /// </summary>
        [Fact]
        public void Mesh2D()
        {
            Polygon2D polygon2D = new(
            [
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(11, 10),
                new Point2D(11, 0),
                new Point2D(11, 0),
                new Point2D(20, 0),
                new Point2D(20, 11),
                new Point2D(0, 11)
            ]);

            PolygonalFace2D? polygonalFace2D = Planar.Create.PolygonalFace2D(polygon2D);
            Assert.NotNull(polygonalFace2D);

            Mesh2D? mesh2D = Planar.Create.Mesh2D(polygonalFace2D);
            Assert.NotNull(mesh2D);
        }
    }
}