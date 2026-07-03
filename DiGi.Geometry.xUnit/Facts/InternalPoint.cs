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
                double angle = i * 2 * System.Math.PI / 100;
                double radius = (i % 2 == 0) ? 10.0 : 1.0;
                point2Ds.Add(new Point2D(radius * System.Math.Cos(angle), radius * System.Math.Sin(angle)));
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
        /// Tests the mean-center (Average) fallback candidate. Uses an L-notched square whose bottom edge is
        /// densely subdivided with collinear points: the area <see cref="Planar.Query.Centroid(IEnumerable{Point2D}?)"/>
        /// is unaffected by the subdivision and falls inside the notch (invalid), while the vertex
        /// <see cref="Planar.Query.Average(IEnumerable{Point2D}?)"/> is pulled toward the dense bottom edge and lands
        /// strictly inside the polygon body, so <see cref="Planar.Query.InternalPoint(IEnumerable{Point2D}?, double)"/>
        /// must return it.
        /// </summary>
        [Fact]
        public void InternalPoint_MeanCenterFallback()
        {
            // Square (0,0)-(20,20) with a narrow deep notch cut from the top between x=9 and x=11 down to y=5.
            // The bottom edge is subdivided with 21 collinear points, which does not change the area centroid
            // but pulls the vertex mean well below y=5, clear of the notch.
            List<Point2D> point2Ds = [];
            for (int i = 0; i <= 20; i++)
            {
                point2Ds.Add(new Point2D(i, 0));
            }
            point2Ds.Add(new Point2D(20, 20));
            point2Ds.Add(new Point2D(11, 20));
            point2Ds.Add(new Point2D(11, 5));
            point2Ds.Add(new Point2D(9, 5));
            point2Ds.Add(new Point2D(9, 20));
            point2Ds.Add(new Point2D(0, 20));

            Point2D? point2D_Centroid = Planar.Query.Centroid(point2Ds);
            Point2D? point2D_Mean = Planar.Query.Average(point2Ds);
            Assert.NotNull(point2D_Centroid);
            Assert.NotNull(point2D_Mean);

            List<Segment2D>? segments = Create.Segment2Ds(point2Ds, true);
            Assert.NotNull(segments);

            // Precondition: the area centroid must fall inside the notch (outside the polygon) so the primary
            // candidate is rejected and the mean-center fallback is exercised.
            Assert.False(Planar.Query.Inside(point2Ds, point2D_Centroid));

            // Precondition: the vertex mean must land strictly inside the polygon body.
            Assert.True(Planar.Query.Inside(point2Ds, point2D_Mean));
            Assert.False(Planar.Query.On(segments, point2D_Mean));

            Point2D? internalPoint = Planar.Query.InternalPoint(point2Ds);
            Assert.NotNull(internalPoint);

            // InternalPoint must have returned the mean-center fallback candidate, not the invalid centroid.
            Assert.Equal(point2D_Mean.X, internalPoint.X, 6);
            Assert.Equal(point2D_Mean.Y, internalPoint.Y, 6);
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