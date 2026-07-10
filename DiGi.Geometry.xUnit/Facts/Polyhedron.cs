using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;
using System.Diagnostics;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the generation of a <see cref="Polyhedron"/> from an <see cref="Ellipsoid"/>.
        /// <para>Verifies the exact face count, closedness, rejection of degenerate inputs, geometric fidelity via a round-trip back to <see cref="Mesh3D"/> whose volume must match the directly generated mesh, and the JSON serialization round-trip.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_Ellipsoid()
        {
            Ellipsoid ellipsoid = new(new Point3D(1, 2, 3), 1, 2, 3);

            Polyhedron? polyhedron = Create.Polyhedron(ellipsoid, 8, 12);
            Assert.NotNull(polyhedron);
            Assert.Equal(2 * 12 * (8 - 1), polyhedron.Count);
            Assert.True(polyhedron.IsClosed());

            Assert.Null(Create.Polyhedron((Ellipsoid?)null, 8, 12));
            Assert.Null(Create.Polyhedron(ellipsoid, 1, 12));
            Assert.Null(Create.Polyhedron(ellipsoid, 8, 2));
            Assert.Null(Create.Polyhedron(ellipsoid, 0.0));

            Polyhedron? polyhedron_AngleFactor = Create.Polyhedron(ellipsoid, System.Math.PI / 8);
            Assert.NotNull(polyhedron_AngleFactor);
            Assert.True(polyhedron_AngleFactor.IsClosed());

            // Round trip back to a mesh must reproduce the same enclosed volume as the directly generated mesh
            Mesh3D? mesh3D = Create.Mesh3D(ellipsoid, 8, 12);
            Assert.NotNull(mesh3D);

            Mesh3D? mesh3D_RoundTrip = Create.Mesh3D(polyhedron);
            Assert.NotNull(mesh3D_RoundTrip);
            Assert.True(mesh3D_RoundTrip.IsClosed());
            Assert.Equal(mesh3D.GetVolume(), mesh3D_RoundTrip.GetVolume(), 6);

            DiGi.Core.xUnit.Query.SerializationCheck(polyhedron);
        }

        /// <summary>
        /// Tests the performance of generating a <see cref="Polyhedron"/> from an <see cref="Ellipsoid"/> and checking its closedness.
        /// <para>After a warm-up call, generating a polyhedron with 9800 triangular faces plus running <see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}, double)"/> must complete within the stated threshold.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_Ellipsoid_Performance()
        {
            Ellipsoid ellipsoid = new(new Point3D(1, 2, 3), 3, 2, 1);

            // Warm-up (JIT)
            Polyhedron? polyhedron_WarmUp = Create.Polyhedron(ellipsoid, 6, 8);
            Assert.NotNull(polyhedron_WarmUp);
            Assert.True(polyhedron_WarmUp.IsClosed());

            Stopwatch stopwatch = Stopwatch.StartNew();

            Polyhedron? polyhedron = Create.Polyhedron(ellipsoid, 50, 100);
            Assert.NotNull(polyhedron);

            bool closed = polyhedron.IsClosed();

            stopwatch.Stop();

            Assert.Equal(2 * 100 * (50 - 1), polyhedron.Count);
            Assert.True(closed);

            Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Polyhedron generation and evaluation took {stopwatch.ElapsedMilliseconds} ms, expected less than 1000 ms.");
        }

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

        /// <summary>
        /// Verifies that the <see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, double)"/> method correctly identifies closed and open polyhedra.
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed()
        {
            BoundingBox3D boundingBox3D = new(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
            Polyhedron? polyhedron_Closed = Create.Polyhedron(boundingBox3D);
            Assert.NotNull(polyhedron_Closed);

            // A fully closed cube should return true.
            Assert.True(polyhedron_Closed.IsClosed());

            // A null polyhedron should return false.
            Polyhedron? polyhedron_Null = null;
            Assert.False(polyhedron_Null.IsClosed());

            // Get the faces of the closed cube.
            List<IPolygonalFace3D>? polygonalFace3Ds = polyhedron_Closed.PolygonalFaces;
            Assert.NotNull(polygonalFace3Ds);
            Assert.Equal(6, polygonalFace3Ds.Count);

            // Create a polyhedron with one face missing (5 faces).
            List<IPolygonalFace3D> polygonalFace3Ds_Open = new();
            for (int int_I = 0; int_I < 5; int_I++)
            {
                polygonalFace3Ds_Open.Add(polygonalFace3Ds[int_I]);
            }
            Polyhedron? polyhedron_Open = Create.Polyhedron(polygonalFace3Ds_Open);
            Assert.NotNull(polyhedron_Open);

            // An open cube (missing one face) should return false.
            Assert.False(polyhedron_Open.IsClosed());
        }

        /// <summary>
        /// Verifies the performance and correctness of the <see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, double)"/> method on a complex shape.
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_Performance()
        {
            // Create a complex closed polyhedron by extruding a 500-sided polygon.
            List<Point3D> point3Ds = new();
            for (int int_I = 0; int_I < 500; int_I++)
            {
                double double_Angle = int_I * 2.0 * System.Math.PI / 500.0;
                point3Ds.Add(new Point3D(System.Math.Cos(double_Angle) * 10.0, System.Math.Sin(double_Angle) * 10.0, 0.0));
            }
            Polygon3D? polygon3D = Create.Polygon3D(point3Ds);
            Assert.NotNull(polygon3D);
            PolygonalFace3D? polygonalFace3D = new(polygon3D);
            Polyhedron? polyhedron_Complex = Create.Polyhedron(polygonalFace3D, new Spatial.Classes.Vector3D(0, 0, 10));
            Assert.NotNull(polyhedron_Complex);

            // Warm-up / JIT compile.
            _ = polyhedron_Complex.IsClosed();

            // Measure execution time.
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            bool bool_IsClosed = polyhedron_Complex.IsClosed();
            stopwatch.Stop();

            Assert.True(bool_IsClosed);
            Assert.True(stopwatch.ElapsedMilliseconds < 50, $"Polyhedron IsClosed performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }
    }
}
