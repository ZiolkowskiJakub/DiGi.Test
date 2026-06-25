namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the generic Distance, generic ClosestPoint, and bounding box distance queries in Planar.Query.
        /// </summary>
        [Fact]
        public void PlanarQueries_DistanceAndClosestPoint()
        {
            // Use a target point outside the rectangle to have a unique closest point on the boundary
            DiGi.Geometry.Planar.Classes.Point2D point2D_Target = new(5.0, -10.0);

            // Create a rectangle as the segmentable shape (origin at (0,0), width 10, height 10)
            DiGi.Geometry.Planar.Classes.Rectangle2D rectangle2D_Shape = new(10.0, 10.0);

            // 1. Verify generic Distance<T> with out parameter populates closestPoint2D correctly
            System.Collections.Generic.List<DiGi.Geometry.Planar.Classes.Rectangle2D> list_Shapes = [rectangle2D_Shape];
            DiGi.Geometry.Planar.Classes.Point2D? point2D_Closest;
            double double_Distance = DiGi.Geometry.Planar.Query.Distance(point2D_Target, list_Shapes, out point2D_Closest);

            Assert.Equal(10.0, double_Distance, 9);
            Assert.NotNull(point2D_Closest);
            if (point2D_Closest is not null)
            {
                Assert.Equal(5.0, point2D_Closest.X, 9);
                Assert.Equal(0.0, point2D_Closest.Y, 9);
            }

            // 2. Verify generic ClosestPoint<T> with out parameter populates distance correctly
            double double_ClosestDistance;
            DiGi.Geometry.Planar.Classes.Point2D? point2D_ClosestPoint = DiGi.Geometry.Planar.Query.ClosestPoint(point2D_Target, list_Shapes, out double_ClosestDistance);

            Assert.NotNull(point2D_ClosestPoint);
            Assert.Equal(10.0, double_ClosestDistance, 9);
            if (point2D_ClosestPoint is not null)
            {
                Assert.Equal(5.0, point2D_ClosestPoint.X, 9);
                Assert.Equal(0.0, point2D_ClosestPoint.Y, 9);
            }

            // 3. Verify Distance(ISegmentable2D, BoundingBox2D) output parameter ordering
            DiGi.Geometry.Planar.Classes.BoundingBox2D boundingBox2D_Box = new(
                new DiGi.Geometry.Planar.Classes.Point2D(20.0, 0.0),
                new DiGi.Geometry.Planar.Classes.Point2D(30.0, 10.0)
            );

            DiGi.Geometry.Planar.Classes.Point2D? point2D_ClosestOnSegmentable;
            DiGi.Geometry.Planar.Classes.Point2D? point2D_ClosestOnBox;

            // Distance between Rectangle2D [0,10]x[0,10] and BoundingBox2D [20,30]x[0,10]
            double double_BoxDistance = DiGi.Geometry.Planar.Query.Distance(
                rectangle2D_Shape,
                boundingBox2D_Box,
                out point2D_ClosestOnSegmentable,
                out point2D_ClosestOnBox
            );

            // The closest distance between [0,10] and [20,30] along X is 10.0 (from X=10 to X=20)
            Assert.Equal(10.0, double_BoxDistance, 9);
            Assert.NotNull(point2D_ClosestOnSegmentable);
            Assert.NotNull(point2D_ClosestOnBox);

            if (point2D_ClosestOnSegmentable is not null)
            {
                // Must be on the rectangle: X should be 10.0
                Assert.Equal(10.0, point2D_ClosestOnSegmentable.X, 9);
            }

            if (point2D_ClosestOnBox is not null)
            {
                // Must be on the bounding box: X should be 20.0
                Assert.Equal(20.0, point2D_ClosestOnBox.X, 9);
            }
        }
    }
}
