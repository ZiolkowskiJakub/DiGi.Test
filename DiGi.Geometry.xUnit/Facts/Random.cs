using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the creation of a random 2D polygon inside a bounding box.
        /// </summary>
        [Fact]
        public void RandomPolygon2D()
        {
            Polygon2D? polygon2D = Planar.Random.Create.Polygon2D(new BoundingBox2D(new Point2D(0, 0), new Point2D(10, 10)), 4);
            Assert.NotNull(polygon2D);
            List<Point2D>? points = polygon2D.GetPoints();
            Assert.NotNull(points);
            Assert.NotEmpty(points);
        }

        /// <summary>
        /// Tests the creation of a random 3D polyhedron inside a bounding box with a seed.
        /// </summary>
        [Fact]
        public void RandomPolyhedron()
        {
            Polyhedron? polyhedron = null;
            for (int seed = 1; seed <= 100; seed++)
            {
                polyhedron = Spatial.Random.Create.Polyhedron(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10)), 4, seed);
                if (polyhedron != null)
                {
                    break;
                }
            }
            Assert.NotNull(polyhedron);
        }
    }
}