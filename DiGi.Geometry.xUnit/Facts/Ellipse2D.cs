using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the functionality of the <see cref="Ellipse2D"/> class, verifying operations such as projection, bounding box calculation, intersection points, and point sampling.
        /// </summary>
        [Fact]
        public void Ellipse2D()
        {
            Ellipse2D ellipse2D = new(new Point2D(0, 0), 5, 10);

            Point2D? point2D_1 = ellipse2D.Project(new Point2D(11, 0));

            Point2D? point2D_2 = ellipse2D.Project(new Point2D(5, 5));

            Point2D? point2D_3 = ellipse2D.Project(new Point2D(0, 11));

            Point2D point2D_Input = new(10, 100);

            BoundingBox2D? boundingBox2D = new Ellipse2D(new Point2D(0, 0), 5, 10).GetBoundingBox();

            List<Point2D>? intersectionPoint2Ds = Planar.Query.IntersectionPoints(ellipse2D, new Segment2D(new Point2D(0, -20), new Point2D(0, 20)));
            Assert.NotNull(intersectionPoint2Ds);

            List<Point2D> point2Ds = [];
            point2Ds.Add(ellipse2D.GetPoint(new Vector2D(1, 0))!);
            point2Ds.Add(ellipse2D.GetPoint(new Vector2D(1, 1))!);
            point2Ds.Add(ellipse2D.GetPoint(new Vector2D(0, 1))!);
            point2Ds.Add(ellipse2D.GetPoint(new Vector2D(-1, 1))!);
            point2Ds.Add(ellipse2D.GetPoint(new Vector2D(-1, 0))!);
            point2Ds.Add(ellipse2D.GetPoint(new Vector2D(-1, -1))!);
            point2Ds.Add(ellipse2D.GetPoint(new Vector2D(0, -1))!);
            point2Ds.Add(ellipse2D.GetPoint(new Vector2D(1, -1))!);

            Assert.NotNull(point2D_1);
            Assert.Equal(5, point2D_1.X, 4);
            Assert.Equal(0, point2D_1.Y, 4);

            Assert.NotNull(point2D_2);

            Assert.NotNull(point2D_3);
            Assert.Equal(0, point2D_3.X, 4);
            Assert.Equal(10, point2D_3.Y, 4);

            Assert.NotNull(point2D_Input);

            Assert.NotNull(boundingBox2D);
            Assert.NotNull(boundingBox2D.Min);
            Assert.Equal(-5, boundingBox2D.Min.X, 4);
            Assert.Equal(-10, boundingBox2D.Min.Y, 4);
            Assert.NotNull(boundingBox2D.Max);
            Assert.Equal(5, boundingBox2D.Max.X, 4);
            Assert.Equal(10, boundingBox2D.Max.Y, 4);

            Assert.Equal(2, intersectionPoint2Ds!.Count);
            Assert.Contains(intersectionPoint2Ds, p => Math.Abs(p.X) < 1e-4 && Math.Abs(p.Y - 10) < 1e-4);
            Assert.Contains(intersectionPoint2Ds, p => Math.Abs(p.X) < 1e-4 && Math.Abs(p.Y + 10) < 1e-4);

            Assert.Equal(8, point2Ds.Count);
        }
    }
}