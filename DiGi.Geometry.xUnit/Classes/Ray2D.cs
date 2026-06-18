using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Classes
    {
        /// <summary>
        /// Tests the projection functionality of a <see cref="Ray2D"/> instance to verify that points are correctly projected onto the ray's path.
        /// </summary>
        [Fact]
        public void Ray2D()
        {
            Ray2D ray2D = new(new Point2D(0.0, 0.0), new Vector2D(0.0, 1.0));

            Point2D? point2D_Project;

            point2D_Project = ray2D.Project(new Point2D(2, 10));
            Assert.NotNull(point2D_Project);

            if (point2D_Project is not null)
            {
                Assert.Equal(point2D_Project, new Point2D(0, 10));
            }

            point2D_Project = ray2D.Project(new Point2D(2, -10));
            Assert.NotNull(point2D_Project);

            if (point2D_Project is not null)
            {
                Assert.Equal(point2D_Project, new Point2D(0, -10));
            }
        }
    }
}
