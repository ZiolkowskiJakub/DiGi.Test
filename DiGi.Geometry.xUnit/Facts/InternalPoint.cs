using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a convex polygon (like a square) returns its centroid as the internal point.
        /// </summary>
        [Fact]
        public void InternalPoint_Convex()
        {
            List<Point2D> points =
            [
                new(0, 0),
                new(10, 0),
                new(10, 10),
                new(0, 10)
            ];

            Point2D? internalPoint = Planar.Query.InternalPoint(points);
            Assert.NotNull(internalPoint);

            // Centroid of square is (5, 5)
            Assert.Equal(5.0, internalPoint.X, 4);
            Assert.Equal(5.0, internalPoint.Y, 4);
        }

        /// <summary>
        /// Tests a concave U-shaped polygon where the centroid lies in the hollow space (outside the polygon).
        /// </summary>
        [Fact]
        public void InternalPoint_Concave()
        {
            // A U-shaped polygon with tall towers where the centroid lies in the middle hollow area
            List<Point2D> points =
            [
                new(0, 0),
                new(6, 0),
                new(6, 10),
                new(4, 10),
                new(4, 2),
                new(2, 2),
                new(2, 10),
                new(0, 10)
            ];

            Point2D? internalPoint = Planar.Query.InternalPoint(points);
            Assert.NotNull(internalPoint);

            // Verify that the returned point is strictly inside the U-shape
            List<Segment2D>? segments = Create.Segment2Ds(points, true);
            Assert.NotNull(segments);

            Assert.True(Planar.Query.Inside(points, internalPoint));
            Assert.False(Planar.Query.On(segments, internalPoint));
        }

        /// <summary>
        /// Tests that duplicate/collinear vertices do not break the calculation.
        /// </summary>
        [Fact]
        public void InternalPoint_CollinearPoints()
        {
            // A square with a collinear point (5,0) on the bottom edge
            List<Point2D> points =
            [
                new(0, 0),
                new(5, 0), // collinear
                new(10, 0),
                new(10, 10),
                new(0, 10)
            ];

            Point2D? internalPoint = Planar.Query.InternalPoint(points);
            Assert.NotNull(internalPoint);

            List<Segment2D>? segments = Create.Segment2Ds(points, true);
            Assert.NotNull(segments);

            Assert.True(Planar.Query.Inside(points, internalPoint));
            Assert.False(Planar.Query.On(segments, internalPoint));
        }

        /// <summary>
        /// Tests that a large concave polygon is processed extremely fast (showing the O(N^4) -> O(N^2) speedup).
        /// </summary>
        [Fact]
        public void InternalPoint_LargePolygon()
        {
            // Generate a large star/comb shape with 100 vertices
            List<Point2D> point2Ds = [];
            for (int i = 0; i < 100; i++)
            {
                double angle = i * 2 * Math.PI / 100;
                double radius = (i % 2 == 0) ? 10.0 : 1.0;
                point2Ds.Add(new Point2D(radius * Math.Cos(angle), radius * Math.Sin(angle)));
            }

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Point2D? internalPoint = Planar.Query.InternalPoint(point2Ds);
            stopwatch.Stop();

            Assert.NotNull(internalPoint);
            Assert.True(stopwatch.ElapsedMilliseconds < 50, $"Optimization failed! Took {stopwatch.ElapsedMilliseconds} ms which is too slow.");

            List<Segment2D>? segment2Ds = Create.Segment2Ds(point2Ds, true);
            Assert.NotNull(segment2Ds);

            Assert.True(Planar.Query.Inside(point2Ds, internalPoint));
            Assert.False(Planar.Query.On(segment2Ds, internalPoint));
        }

        /// <summary>
        /// Tests how the algorithm handles complex or self-intersecting invalid polygons.
        /// </summary>
        [Fact]
        public void InternalPoint_SelfIntersectingOrInvalid()
        {
            // Self-intersecting figure-8 shape
            List<Point2D> points =
            [
                new(0, 0),
                new(10, 10),
                new(10, 0),
                new(0, 10)
            ];

            // Should handle it gracefully, possibly returning null or an internal point of one of the loops
            Point2D? internalPoint = Planar.Query.InternalPoint(points);
            // We just verify it does not throw an exception
        }

        /// <summary>
        /// Tests degenerate inputs (null, empty, or less than 3 points).
        /// </summary>
        [Fact]
        public void InternalPoint_NullAndEmpty()
        {
            Assert.Null(Planar.Query.InternalPoint(null));
            Assert.Null(Planar.Query.InternalPoint([]));
            Assert.Null(Planar.Query.InternalPoint([new Point2D(0, 0)]));
            Assert.Null(Planar.Query.InternalPoint([new Point2D(0, 0), new Point2D(5, 5)]));
        }
    }
}