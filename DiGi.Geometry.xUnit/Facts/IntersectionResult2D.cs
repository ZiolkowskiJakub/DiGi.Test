using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the optimized <see cref="Point2D.Distance"/> calculation.
        /// </summary>
        [Fact]
        public void Point2D_Distance_Correctness()
        {
            Point2D point2D_1 = new (0, 0);
            Point2D point2D_2 = new (3, 4); // Distance should be sqrt(3^2 + 4^2) = 5

            double distance = point2D_1.Distance(point2D_2);

            Assert.Equal(5.0, distance, 5);
        }

        /// <summary>
        /// Tests <see cref="Segment2D"/> similarity check for identical, reversed, and different segments.
        /// </summary>
        [Fact]
        public void Segment2D_Similar_Correctness()
        {
            Point2D point2D_Start = new (0, 0);
            Point2D point2D_End = new (10, 10);

            Segment2D segment2D_1 = new (point2D_Start, point2D_End);
            Segment2D segment2D_2 = new (point2D_Start, point2D_End);
            Segment2D segment2D_Reversed = new (point2D_End, point2D_Start);
            Segment2D segment2D_Different = new (point2D_Start, new Point2D(5, 5));

            Assert.True(segment2D_1.Similar(segment2D_2));
            Assert.True(segment2D_1.Similar(segment2D_Reversed));
            Assert.False(segment2D_1.Similar(segment2D_Different));
        }

        /// <summary>
        /// Tests <see cref="BoundingBox2D.On"/> boundary checking for allocation-free checks.
        /// </summary>
        [Fact]
        public void BoundingBox2D_On_Correctness()
        {
            Point2D point2D_Min = new (-5, -5);
            Point2D point2D_Max = new (5, 5);
            BoundingBox2D boundingBox2D = new (point2D_Min, point2D_Max);

            // Point on left boundary
            Assert.True(boundingBox2D.On(new Point2D(-5, 0)));
            // Point on top boundary
            Assert.True(boundingBox2D.On(new Point2D(0, 5)));
            // Point inside
            Assert.False(boundingBox2D.On(new Point2D(0, 0)));
            // Point outside
            Assert.False(boundingBox2D.On(new Point2D(6, 6)));
        }

        /// <summary>
        /// Tests BoundingBox2D.InRange for line and segment intersections (Slab Method).
        /// </summary>
        [Fact]
        public void BoundingBox2D_InRange_Correctness()
        {
            Point2D point2D_Min = new (-5, -5);
            Point2D point2D_Max = new (5, 5);
            BoundingBox2D boundingBox2D = new (point2D_Min, point2D_Max);

            // 1. Line passing directly through AABB
            Line2D line2D_Through = new (new Point2D(-10, 0), new Vector2D(1, 0));
            Assert.True(boundingBox2D.InRange(line2D_Through));

            // 2. Line missing AABB
            Line2D line2D_Miss = new (new Point2D(-10, 10), new Vector2D(1, 0));
            Assert.False(boundingBox2D.InRange(line2D_Miss));

            // 3. Segment completely inside AABB
            Segment2D segment2D_Inside = new (new Point2D(-2, -2), new Point2D(2, 2));
            Assert.True(boundingBox2D.InRange(segment2D_Inside));

            // 4. Segment outside AABB but aligned
            Segment2D segment2D_Outside = new (new Point2D(10, 0), new Point2D(15, 0));
            Assert.False(boundingBox2D.InRange(segment2D_Outside));
        }

        /// <summary>
        /// Tests standard Segment-Segment intersections (non-intersecting, single point, parallel, collinear, adjacent).
        /// </summary>
        [Fact]
        public void Segment_Segment_Intersection_Correctness()
        {
            // Standard crossing intersection
            Segment2D segment2D_1 = new (new Point2D(-2, 0), new Point2D(2, 0));
            Segment2D segment2D_2 = new (new Point2D(0, -2), new Point2D(0, 2));
            IntersectionResult2D? intersectionResult2D_Cross = segment2D_1.IntersectionResult2D(segment2D_2);

            Assert.NotNull(intersectionResult2D_Cross);
            Assert.True(intersectionResult2D_Cross.Intersect);
            Assert.Equal(1, intersectionResult2D_Cross.Count);
            Point2D? point2D_Cross = intersectionResult2D_Cross.GetGeometry2Ds<Point2D>()?[0];
            Assert.NotNull(point2D_Cross);
            Assert.Equal(0, point2D_Cross.X, 5);
            Assert.Equal(0, point2D_Cross.Y, 5);

            // Parallel non-intersecting
            Segment2D segment2D_Parallel1 = new (new Point2D(-2, 0), new Point2D(2, 0));
            Segment2D segment2D_Parallel2 = new (new Point2D(-2, 2), new Point2D(2, 2));
            IntersectionResult2D? intersectionResult2D_Parallel = segment2D_Parallel1.IntersectionResult2D(segment2D_Parallel2);
            Assert.NotNull(intersectionResult2D_Parallel);
            Assert.False(intersectionResult2D_Parallel.Intersect);

            // Collinear overlapping (results in a Segment)
            Segment2D segment2D_Collinear1 = new (new Point2D(0, 0), new Point2D(4, 0));
            Segment2D segment2D_Collinear2 = new (new Point2D(2, 0), new Point2D(6, 0));
            IntersectionResult2D? intersectionResult2D_Overlapping = segment2D_Collinear1.IntersectionResult2D(segment2D_Collinear2);
            Assert.NotNull(intersectionResult2D_Overlapping);
            Assert.True(intersectionResult2D_Overlapping.Intersect);
            Assert.True(intersectionResult2D_Overlapping.Contains<Segment2D>());

            // Adjacent sharing end points (results in a Point)
            Segment2D segment2D_Adjacent1 = new (new Point2D(0, 0), new Point2D(2, 2));
            Segment2D segment2D_Adjacent2 = new (new Point2D(2, 2), new Point2D(4, 0));
            IntersectionResult2D? intersectionResult2D_Adjacent = segment2D_Adjacent1.IntersectionResult2D(segment2D_Adjacent2);
            Assert.NotNull(intersectionResult2D_Adjacent);
            Assert.True(intersectionResult2D_Adjacent.Intersect);
            Assert.True(intersectionResult2D_Adjacent.Contains<Point2D>());
            Point2D? point2D_Shared = intersectionResult2D_Adjacent.GetGeometry2Ds<Point2D>()?[0];
            Assert.NotNull(point2D_Shared);
            Assert.Equal(2, point2D_Shared.X, 5);
            Assert.Equal(2, point2D_Shared.Y, 5);
        }

        /// <summary>
        /// Tests Segment-Collection intersections.
        /// </summary>
        [Fact]
        public void Segment_Collection_Intersection_Correctness()
        {
            Segment2D segment2D_Source = new (new Point2D(0, -5), new Point2D(0, 5));
            List<Segment2D> segment2Ds =
            [
                new Segment2D(new Point2D(-2, 0), new Point2D(2, 0)),  // Intersects at (0,0)
                new Segment2D(new Point2D(-2, 2), new Point2D(2, 2)),  // Intersects at (0,2)
                new Segment2D(new Point2D(10, 10), new Point2D(12, 12)) // Far away (should skip via InRange bounding box check)
            ];

            IntersectionResult2D? intersectionResult2D = segment2D_Source.IntersectionResult2D(segment2Ds);
            Assert.NotNull(intersectionResult2D);
            Assert.True(intersectionResult2D.Intersect);
            Assert.Equal(2, intersectionResult2D.Count);
        }

        /// <summary>
        /// Tests Self-Intersection of segmentable geometry with maxCount constraints.
        /// </summary>
        [Fact]
        public void SelfIntersection_MaxCount_Correctness()
        {
            // Create a self-intersecting polyline/segment list (like a star or crossed shape)
            List<Point2D> point2Ds =
            [
                new Point2D(-2, 2),
                new Point2D(2, -2),
                new Point2D(2, 2),
                new Point2D(-2, -2)
            ];
            Polyline2D polyline2D = new (point2Ds);

            // Self intersection with maxCount = 1
            IntersectionResult2D? intersectionResult2D_Limited = polyline2D.IntersectionResult2D(1);
            Assert.NotNull(intersectionResult2D_Limited);
            Assert.True(intersectionResult2D_Limited.Intersect);
            Assert.Equal(1, intersectionResult2D_Limited.Count);
        }

        /// <summary>
        /// Tests PolygonalFace vs Linear Geometry intersections.
        /// </summary>
        [Fact]
        public void PolygonalFace_Linear_Intersection_Correctness()
        {
            PolygonalFace2D? polygonalFace2D = Create.PolygonalFace2D(new Point2D(-2, -2), new Point2D(-2, 2), new Point2D(2, 2), new Point2D(2, -2));
            Assert.NotNull(polygonalFace2D);

            if (polygonalFace2D == null)
            {
                return;
            }

            // Line passing through the box
            Line2D line2D_Through = new (new Point2D(0, -10), new Vector2D(0, 1));
            IntersectionResult2D? intersectionResult2D = polygonalFace2D.IntersectionResult2D(line2D_Through);

            Assert.NotNull(intersectionResult2D);
            Assert.True(intersectionResult2D.Intersect);
            Assert.True(intersectionResult2D.Contains<Segment2D>()); // Intersects face as a segment from (0,-2) to (0,2)
        }
    }
}