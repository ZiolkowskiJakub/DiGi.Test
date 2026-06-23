using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the basic Convex Hull computation on a set of points (a grid where only the 4 corner points form the hull).
        /// </summary>
        [Fact]
        public void ConvexHull_Basic()
        {
            List<Point2D> points =
            [
                new(0, 0), new(5, 0), new(10, 0),
                new(0, 5), new(5, 5), new(10, 5),
                new(0, 10), new(5, 10), new(10, 10)
            ];

            List<Point2D>? hull = Planar.Query.ConvexHull(points);
            Assert.NotNull(hull);

            // For a grid [0,10]x[0,10], the convex hull should be the 4 corner points.
            Assert.Equal(4, hull.Count);

            // Verify corners exist in the hull
            Assert.Contains(hull, p => p.X == 0 && p.Y == 0);
            Assert.Contains(hull, p => p.X == 10 && p.Y == 0);
            Assert.Contains(hull, p => p.X == 10 && p.Y == 10);
            Assert.Contains(hull, p => p.X == 0 && p.Y == 10);
        }

        /// <summary>
        /// Tests the keepOrder parameter of the Convex Hull computation.
        /// </summary>
        [Fact]
        public void ConvexHull_KeepOrder()
        {
            // Point list with corners mixed with interior points
            List<Point2D> points =
            [
                new(5, 5),   // index 0
                new(0, 0),   // index 1 - corner
                new(10, 0),  // index 2 - corner
                new(5, 0),   // index 3
                new(10, 10), // index 4 - corner
                new(0, 10)   // index 5 - corner
            ];

            List<Point2D>? hull = Planar.Query.ConvexHull(points, keepOrder: true);
            Assert.NotNull(hull);
            Assert.Equal(4, hull.Count);

            // The order should match the original list:
            // 1st corner: (0,0) (index 1)
            // 2nd corner: (10,0) (index 2)
            // 3rd corner: (10,10) (index 4)
            // 4th corner: (0,10) (index 5)
            Assert.Equal(points[1], hull[0]);
            Assert.Equal(points[2], hull[1]);
            Assert.Equal(points[4], hull[2]);
            Assert.Equal(points[5], hull[3]);
        }

        /// <summary>
        /// Tests that duplicate coordinates in the input point collection are correctly handled.
        /// </summary>
        [Fact]
        public void ConvexHull_Duplicates()
        {
            List<Point2D> points =
            [
                new(0, 0), new(0, 0),
                new(10, 0), new(10, 0),
                new(10, 10),
                new(0, 10), new(0, 10)
            ];

            List<Point2D>? hull = Planar.Query.ConvexHull(points);
            Assert.NotNull(hull);
            Assert.Equal(4, hull.Count);
        }

        /// <summary>
        /// Tests calculating the Convex Hull from a list of Segment2D segments.
        /// </summary>
        [Fact]
        public void ConvexHull_Segments()
        {
            List<Segment2D> segments =
            [
                new(new Point2D(0, 0), new Point2D(10, 0)),
                new(new Point2D(10, 0), new Point2D(10, 10)),
                new(new Point2D(10, 10), new Point2D(0, 10)),
                new(new Point2D(0, 10), new Point2D(0, 0))
            ];

            List<Point2D>? hull = Planar.Query.ConvexHull(segments);
            Assert.NotNull(hull);
            Assert.Equal(4, hull.Count);
        }

        /// <summary>
        /// Tests calculating the Convex Hull from a Segmentable geometry.
        /// </summary>
        [Fact]
        public void ConvexHull_Segmentable()
        {
            Polygon2D poly = new((IEnumerable<Point2D>)[
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(5, 5),
                new Point2D(10, 10),
                new Point2D(0, 10)
            ]);

            List<Point2D>? hull = Planar.Query.ConvexHull(poly);
            Assert.NotNull(hull);
            Assert.Equal(4, hull.Count); // (5,5) should be excluded since it's concave and inside the hull boundary
            Assert.DoesNotContain(hull, p => p.X == 5 && p.Y == 5);
        }
    }
}