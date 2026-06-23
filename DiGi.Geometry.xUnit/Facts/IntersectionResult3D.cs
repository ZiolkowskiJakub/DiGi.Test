using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the optimized <see cref="Point3D.Distance"/> calculation.
        /// </summary>
        [Fact]
        public void Point3D_Distance_Correctness()
        {
            Point3D point3D_1 = new Point3D(0, 0, 0);
            Point3D point3D_2 = new Point3D(3, 4, 12); // Distance should be sqrt(3^2 + 4^2 + 12^2) = sqrt(9 + 16 + 144) = 13

            double distance = point3D_1.Distance(point3D_2);

            Assert.Equal(13.0, distance, 5);
        }

        /// <summary>
        /// Tests <see cref="Segment3D"/> similarity check for identical, reversed, and different segments.
        /// </summary>
        [Fact]
        public void Segment3D_Similar_Correctness()
        {
            Point3D point3D_Start = new Point3D(0, 0, 0);
            Point3D point3D_End = new Point3D(10, 10, 10);

            Segment3D segment3D_1 = new Segment3D(point3D_Start, point3D_End);
            Segment3D segment3D_2 = new Segment3D(point3D_Start, point3D_End);
            Segment3D segment3D_Reversed = new Segment3D(point3D_End, point3D_Start);
            Segment3D segment3D_Different = new Segment3D(point3D_Start, new Point3D(5, 5, 5));

            Assert.True(segment3D_1.Similar(segment3D_2));
            Assert.True(segment3D_1.Similar(segment3D_Reversed));
            Assert.False(segment3D_1.Similar(segment3D_Different));
        }

        /// <summary>
        /// Tests BoundingBox3D.InRange for line, ray, and segment Slab Method correctness.
        /// </summary>
        [Fact]
        public void BoundingBox3D_InRange_SlabMethod()
        {
            Point3D point3D_Min = new Point3D(-5, -5, -5);
            Point3D point3D_Max = new Point3D(5, 5, 5);
            BoundingBox3D boundingBox3D = new BoundingBox3D(point3D_Min, point3D_Max);

            // 1. Line passing directly through AABB
            Line3D line3D_Through = new Line3D(new Point3D(-10, 0, 0), new Spatial.Classes.Vector3D(1, 0, 0));
            Assert.True(boundingBox3D.InRange(line3D_Through));

            // 2. Line missing AABB
            Line3D line3D_Miss = new Line3D(new Point3D(-10, 10, 0), new Spatial.Classes.Vector3D(1, 0, 0));
            Assert.False(boundingBox3D.InRange(line3D_Miss));

            // 3. Segment completely inside AABB
            Segment3D segment3D_Inside = new Segment3D(new Point3D(-2, -2, -2), new Point3D(2, 2, 2));
            Assert.True(boundingBox3D.InRange(segment3D_Inside));

            // 4. Segment outside AABB but aligned
            Segment3D segment3D_Outside = new Segment3D(new Point3D(10, 0, 0), new Point3D(15, 0, 0));
            Assert.False(boundingBox3D.InRange(segment3D_Outside));

            // 5. Ray starting inside and pointing away
            Ray3D ray3D_Inside = new Ray3D(new Point3D(0, 0, 0), new Spatial.Classes.Vector3D(0, 0, 1));
            Assert.True(boundingBox3D.InRange(ray3D_Inside));

            // 6. Ray pointing towards AABB
            Ray3D ray3D_Towards = new Ray3D(new Point3D(-10, 0, 0), new Spatial.Classes.Vector3D(1, 0, 0));
            Assert.True(boundingBox3D.InRange(ray3D_Towards));

            // 7. Ray pointing away from AABB
            Ray3D ray3D_Away = new Ray3D(new Point3D(-10, 0, 0), new Spatial.Classes.Vector3D(-1, 0, 0));
            Assert.False(boundingBox3D.InRange(ray3D_Away));
        }

        /// <summary>
        /// Tests <see cref="IntersectionResult3D"/> calculations for sphere-linear intersections.
        /// </summary>
        [Fact]
        public void Sphere_IntersectionResult3D_Correctness()
        {
            Sphere sphere = new Sphere(new Point3D(0, 0, 0), 5.0);

            // Line passing through the center: 2 intersection points
            Line3D line3D_Through = new Line3D(new Point3D(-10, 0, 0), new Spatial.Classes.Vector3D(1, 0, 0));
            IntersectionResult3D? intersectionResult3D_1 = sphere.IntersectionResult3D(line3D_Through);

            Assert.NotNull(intersectionResult3D_1);
            Assert.True(intersectionResult3D_1.Intersect);
            Assert.Equal(2, intersectionResult3D_1.Count);

            // Segment inside/crossing
            Segment3D segment3D_Crossing = new Segment3D(new Point3D(-3, 0, 0), new Point3D(6, 0, 0));
            IntersectionResult3D? intersectionResult3D_2 = sphere.IntersectionResult3D(segment3D_Crossing);

            Assert.NotNull(intersectionResult3D_2);
            Assert.True(intersectionResult3D_2.Intersect);
            Assert.Equal(1, intersectionResult3D_2.Count); // intersects at (5, 0, 0)
        }

        /// <summary>
        /// Tests <see cref="IntersectionResult3D"/> for a Polyhedron AABB intersection with linear geometry.
        /// </summary>
        [Fact]
        public void Polyhedron_IntersectionResult3D_Correctness()
        {
            Point3D point3D_Min = new Point3D(-2, -2, -2);
            Point3D point3D_Max = new Point3D(2, 2, 2);
            BoundingBox3D boundingBox3D = new BoundingBox3D(point3D_Min, point3D_Max);

            Polyhedron? polyhedron = Create.Polyhedron(boundingBox3D);
            Assert.NotNull(polyhedron);

            if (polyhedron == null)
            {
                return;
            }

            Line3D line3D_Through = new Line3D(new Point3D(0, 0, -10), new Spatial.Classes.Vector3D(0, 0, 1));
            IntersectionResult3D? intersectionResult3D = polyhedron.IntersectionResult3D(line3D_Through);

            Assert.NotNull(intersectionResult3D);
            Assert.True(intersectionResult3D.Intersect);
            Assert.Equal(2, intersectionResult3D.Count); // crosses Z=-2 and Z=2 faces
        }

        /// <summary>
        /// Tests 3D intersection and planar intersection with an extruded polyhedron.
        /// </summary>
        [Fact]
        public void PolyhedronIntersection()
        {
            Polygon2D polygon2D = new(new List<Point2D>()
            {
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(0, 10)
            });

            PolygonalFace2D? polygonalFace2D = Planar.Create.PolygonalFace2D(polygon2D);
            Assert.NotNull(polygonalFace2D);

            PolygonalFace3D polygonalFace3D = new(Spatial.Constants.Plane.WorldZ, polygonalFace2D);

            Spatial.Classes.Vector3D vector3D = new(0, 0, 10);

            Polyhedron? polyhedron = Create.Polyhedron(polygonalFace3D, vector3D);
            Assert.NotNull(polyhedron);

            IntersectionResult3D? intersectionResult3D = Create.IntersectionResult3D(polyhedron, new Segment3D(-1, 5, 5, 11, 5, 5));
            Assert.NotNull(intersectionResult3D);
            Assert.True(intersectionResult3D.Intersect);

            PlanarIntersectionResult? planarIntersectionResult = Create.PlanarIntersectionResult(new Plane(new Point3D(0, 0, 5), Spatial.Constants.Vector3D.WorldZ), polyhedron);
            Assert.NotNull(planarIntersectionResult);
            Assert.True(planarIntersectionResult.Intersect);
        }
    }
}