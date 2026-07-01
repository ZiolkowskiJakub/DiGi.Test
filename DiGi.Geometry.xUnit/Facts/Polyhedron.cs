using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using System.Diagnostics;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that point classification against a box <see cref="Polyhedron"/> is correct for interior, boundary, and exterior points.
        /// <para>The bounding-box early-out added to <see cref="Polyhedron{TPolygonalFace3D}.InRange(Point3D, double)"/> must not change any of these results.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_PointClassification()
        {
            BoundingBox3D boundingBox3D = new(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
            Polyhedron? polyhedron = Create.Polyhedron(boundingBox3D);
            Assert.NotNull(polyhedron);
            Assert.Equal(6, polyhedron.Count);

            // Interior point.
            Point3D point3D_Centroid = new(5, 5, 5);
            Assert.True(polyhedron.InRange(point3D_Centroid));
            Assert.True(polyhedron.Inside(point3D_Centroid));
            Assert.False(polyhedron.On(point3D_Centroid));

            // Boundary point (on a face).
            Point3D point3D_Boundary = new(0, 5, 5);
            Assert.True(polyhedron.InRange(point3D_Boundary));
            Assert.False(polyhedron.Inside(point3D_Boundary));
            Assert.True(polyhedron.On(point3D_Boundary));

            // Exterior point just outside a face.
            Point3D point3D_OutsideNear = new(11, 5, 5);
            Assert.False(polyhedron.InRange(point3D_OutsideNear));
            Assert.False(polyhedron.Inside(point3D_OutsideNear));
            Assert.False(polyhedron.On(point3D_OutsideNear));

            // Exterior point far away (the case the early-out short-circuits).
            Point3D point3D_OutsideFar = new(1000, 1000, 1000);
            Assert.False(polyhedron.InRange(point3D_OutsideFar));
            Assert.False(polyhedron.Inside(point3D_OutsideFar));
            Assert.False(polyhedron.On(point3D_OutsideFar));
        }

        /// <summary>
        /// Verifies that the bounding-box early-out in <see cref="Polyhedron{TPolygonalFace3D}.InRange(Point3D, double)"/> preserves the on-boundary tolerance band.
        /// <para>A point within <c>tolerance</c> of a face must still report in range; a point beyond the tolerance band must not. This pins that the early-out margin (2 * tolerance) never rejects a point that the tolerant boundary test would accept.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_InRange_ToleranceBoundary()
        {
            BoundingBox3D boundingBox3D = new(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
            Polyhedron? polyhedron = Create.Polyhedron(boundingBox3D);
            Assert.NotNull(polyhedron);

            double tolerance = 0.1;

            // Just inside the tolerance band of the x = 10 face -> on the surface -> in range.
            Point3D point3D_WithinTolerance = new(10.0 + 0.5 * tolerance, 5, 5);
            Assert.True(polyhedron.InRange(point3D_WithinTolerance, tolerance));
            Assert.True(polyhedron.On(point3D_WithinTolerance, tolerance));

            // Just outside the tolerance band -> neither on the surface nor inside.
            Point3D point3D_BeyondTolerance = new(10.0 + 1.5 * tolerance, 5, 5);
            Assert.False(polyhedron.InRange(point3D_BeyondTolerance, tolerance));
            Assert.False(polyhedron.On(point3D_BeyondTolerance, tolerance));

            // Well outside -> rejected outright.
            Point3D point3D_Outside = new(10.0 + 5.0 * tolerance, 5, 5);
            Assert.False(polyhedron.InRange(point3D_Outside, tolerance));
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the bounding-box early-out for exterior points in <see cref="Polyhedron{TPolygonalFace3D}.InRange(Point3D, double)"/>.
        /// <para>Without the early-out every exterior point runs the full ray-casting loop; with it an out-of-box point is rejected after a single bounding-box test. A large batch of far-outside points must classify as out of range well within the threshold.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_InRange_OutsidePoint_Performance()
        {
            BoundingBox3D boundingBox3D = new(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
            Polyhedron? polyhedron = Create.Polyhedron(boundingBox3D);
            Assert.NotNull(polyhedron);

            // Warm-up (JIT).
            Assert.False(polyhedron.InRange(new Point3D(1000, 1000, 1000)));

            int count = 5000;

            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                Assert.False(polyhedron.InRange(new Point3D(1000 + i, 1000, 1000)));
            }
            stopwatch.Stop();

            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            Assert.True(elapsedMilliseconds < 500, $"Polyhedron InRange exterior-point performance check failed! {count} calls took {elapsedMilliseconds} ms.");
        }
    }
}
