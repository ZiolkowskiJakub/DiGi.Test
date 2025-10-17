using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Query
    {
        [Fact]
        public void IntersectionPoint()
        {
            Ray2D ray2D_1;
            Ray2D ray2D_2;

            Point2D? intersectionPoint;

            ray2D_1 = new Ray2D(new Point2D(0, 0), new Vector2D(0, 10));
            ray2D_2 = new Ray2D(new Point2D(1, 2), new Vector2D(-1, 0));

            intersectionPoint = Planar.Query.IntersectionPoint(ray2D_1, ray2D_2);
            Assert.NotNull(intersectionPoint);
            Assert.Equal(intersectionPoint, new Point2D(0, 2));

            ray2D_1 = new Ray2D(new Point2D(0, 0), new Vector2D(0, 10));
            ray2D_2 = new Ray2D(new Point2D(1, 2), new Vector2D(1, 0));

            intersectionPoint = Planar.Query.IntersectionPoint(ray2D_1, ray2D_2);
            Assert.Null(intersectionPoint);

            ray2D_1 = new Ray2D(new Point2D(0, 0), new Vector2D(0, -10));
            ray2D_2 = new Ray2D(new Point2D(1, 2), new Vector2D(-1, 0));

            intersectionPoint = Planar.Query.IntersectionPoint(ray2D_1, ray2D_2);
            Assert.Null(intersectionPoint);

            ray2D_1 = new Ray2D(new Point2D(0, 1), new Vector2D(1, 0));
            ray2D_2 = new Ray2D(new Point2D(3, 0), new Vector2D(0, 1));

            intersectionPoint = Planar.Query.IntersectionPoint(ray2D_1, ray2D_2);
            Assert.NotNull(intersectionPoint);
            Assert.Equal(intersectionPoint, new Point2D(3, 1));
        }
    }
}