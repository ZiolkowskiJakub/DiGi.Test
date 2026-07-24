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

            // A fully closed cube should return true under both closure criteria.
            Assert.True(polyhedron_Closed.IsClosed());
            Assert.True(polyhedron_Closed.IsClosed(true));

            // A null polyhedron should return false.
            Polyhedron? polyhedron_Null = null;
            Assert.False(polyhedron_Null.IsClosed());
            Assert.False(polyhedron_Null.IsClosed(true));

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
        /// <para>The polyhedron is a 500-sided extrusion. The call is repeated so the measurement is not dominated by stopwatch and scheduling noise, and the assertion is made on the per-call cost rather than on a single sample.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_Performance()
        {
            Polyhedron? polyhedron_Complex = Polyhedron_IsClosed_Extrusion(500);
            Assert.NotNull(polyhedron_Complex);

            // Warm-up / JIT compile.
            _ = polyhedron_Complex.IsClosed();

            // Enough repeats for tiered JIT to reach steady state; below roughly a hundred the measurement is dominated
            // by tier-0 code and reports a higher per-call cost in Release than in Debug.
            int repeats = 200;

            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int int_I = 0; int_I < repeats; int_I++)
            {
                Assert.True(polyhedron_Complex.IsClosed());
            }
            stopwatch.Stop();

            double microseconds_PerCall = stopwatch.Elapsed.TotalMilliseconds * 1000.0 / repeats;
            Assert.True(microseconds_PerCall < 8000.0, $"Polyhedron IsClosed performance check failed! {microseconds_PerCall:F1} us per call over {repeats} calls.");
        }

        /// <summary>
        /// Verifies that the minimum valid closed solid - a tetrahedron with exactly four faces - is reported as closed.
        /// <para>This pins the lower boundary of the face-count guard in <see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, bool, double)"/>, which rejects anything with fewer than four faces.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_Tetrahedron()
        {
            Polyhedron? polyhedron_Tetrahedron = Create.Polyhedron(Polyhedron_IsClosed_TetrahedronFaces());
            Assert.NotNull(polyhedron_Tetrahedron);
            Assert.Equal(4, polyhedron_Tetrahedron.Count);

            Assert.True(polyhedron_Tetrahedron.IsClosed());
            Assert.True(polyhedron_Tetrahedron.IsClosed(true));
        }

        /// <summary>
        /// Verifies that faces carrying internal rings (holes) are handled by <see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, bool, double)"/>.
        /// <para>A block with a square through-hole is closed only once the four faces lining the hole are present. Removing them leaves the hole ring edges used once each, so the solid must be reported open even though its outer surface is untouched.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_FaceWithHole()
        {
            List<IPolygonalFace3D> polygonalFace3Ds_Lined = Polyhedron_IsClosed_BlockWithHoleFaces(true);
            Polyhedron? polyhedron_Lined = Create.Polyhedron(polygonalFace3Ds_Lined);
            Assert.NotNull(polyhedron_Lined);
            Assert.Equal(10, polyhedron_Lined.Count);

            Assert.True(polyhedron_Lined.IsClosed());
            Assert.True(polyhedron_Lined.IsClosed(true));

            // The same block without the faces lining the hole leaves the hole boundary naked.
            List<IPolygonalFace3D> polygonalFace3Ds_Unlined = Polyhedron_IsClosed_BlockWithHoleFaces(false);
            Polyhedron? polyhedron_Unlined = Create.Polyhedron(polygonalFace3Ds_Unlined);
            Assert.NotNull(polyhedron_Unlined);
            Assert.Equal(6, polyhedron_Unlined.Count);

            Assert.False(polyhedron_Unlined.IsClosed());
            Assert.False(polyhedron_Unlined.IsClosed(true));
        }

        /// <summary>
        /// Verifies that <see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, bool, double)"/> honours the distance tolerance when welding coincident vertices.
        /// <para>One face of a box is displaced along its normal so its corners no longer coincide with the corners of the adjoining faces. A displacement below the tolerance must still weld and report closed; a displacement above it must not.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_ToleranceBoundary()
        {
            double tolerance = 0.1;

            // Displacement well inside the tolerance band -> the corners weld -> closed.
            Polyhedron? polyhedron_Within = Create.Polyhedron(Polyhedron_IsClosed_BoxFacesWithLiftedTop(0.0, 10.0, 0.5 * tolerance));
            Assert.NotNull(polyhedron_Within);
            Assert.True(polyhedron_Within.IsClosed(tolerance));

            // Displacement beyond the tolerance band -> the corners stay distinct -> open.
            Polyhedron? polyhedron_Beyond = Create.Polyhedron(Polyhedron_IsClosed_BoxFacesWithLiftedTop(0.0, 10.0, 2.0 * tolerance));
            Assert.NotNull(polyhedron_Beyond);
            Assert.False(polyhedron_Beyond.IsClosed(tolerance));

            // The same displaced box is open at the default tolerance, which is far tighter.
            Assert.False(polyhedron_Within.IsClosed());
        }

        /// <summary>
        /// Verifies the difference between the two closure criteria of <see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, bool, double)"/> on a non-manifold solid.
        /// <para>Two boxes glued along a shared face leave every edge of the seam used by four faces. The default criterion counts parity and accepts it; the manifold criterion requires exactly two uses per edge and rejects it.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_NonManifold()
        {
            List<IPolygonalFace3D> polygonalFace3Ds = Polyhedron_IsClosed_BoxFaces(0.0, 10.0, 0.0);
            polygonalFace3Ds.AddRange(Polyhedron_IsClosed_BoxFaces(0.0, 10.0, 10.0));

            Polyhedron? polyhedron_Glued = Create.Polyhedron(polygonalFace3Ds);
            Assert.NotNull(polyhedron_Glued);
            Assert.Equal(12, polyhedron_Glued.Count);

            // Every seam edge is used four times: even parity, but not 2-manifold.
            Assert.True(polyhedron_Glued.IsClosed());
            Assert.False(polyhedron_Glued.IsClosed(true));
        }

        /// <summary>
        /// Verifies that a duplicated face is rejected under both closure criteria of <see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, bool, double)"/>.
        /// <para>Repeating one face of a closed box drives the four edges of that face to three uses, which is neither an even count nor exactly two.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_DuplicatedFace()
        {
            List<IPolygonalFace3D> polygonalFace3Ds = Polyhedron_IsClosed_BoxFaces(0.0, 10.0, 0.0);
            polygonalFace3Ds.Add(polygonalFace3Ds[0]);

            Polyhedron? polyhedron_Duplicated = Create.Polyhedron(polygonalFace3Ds);
            Assert.NotNull(polyhedron_Duplicated);
            Assert.Equal(7, polyhedron_Duplicated.Count);

            Assert.False(polyhedron_Duplicated.IsClosed());
            Assert.False(polyhedron_Duplicated.IsClosed(true));
        }

        /// <summary>
        /// Verifies that a structurally malformed face disqualifies the whole polyhedron in <see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, bool, double)"/>.
        /// <para>Regression for a face whose 2D geometry is missing being silently skipped: the six faces of the box around it are watertight on their own, so skipping the broken face reported the polyhedron as closed. A face that cannot be read in full must never be treated as contributing no edges.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_MalformedFace()
        {
            List<IPolygonalFace3D> polygonalFace3Ds = Polyhedron_IsClosed_BoxFaces(0.0, 10.0, 0.0);

            // Sanity check: the box on its own is closed.
            Polyhedron? polyhedron_Closed = Create.Polyhedron(polygonalFace3Ds);
            Assert.NotNull(polyhedron_Closed);
            Assert.True(polyhedron_Closed.IsClosed());

            Plane? plane = polygonalFace3Ds[0].Plane;
            Assert.NotNull(plane);

            polygonalFace3Ds.Add(new PolygonalFace3D(plane, null));

            Polyhedron? polyhedron_Malformed = Create.Polyhedron(polygonalFace3Ds);
            Assert.NotNull(polyhedron_Malformed);
            Assert.Equal(7, polyhedron_Malformed.Count);

            Assert.False(polyhedron_Malformed.IsClosed());
            Assert.False(polyhedron_Malformed.IsClosed(true));
        }

        /// <summary>
        /// Verifies that a zero-length edge does not change the result of <see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, bool, double)"/>.
        /// <para>One face of a closed box carries a repeated corner, so its ring holds a degenerate edge whose endpoints weld onto the same vertex. That edge must be ignored rather than counted as a naked edge.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_DegenerateEdge()
        {
            List<IPolygonalFace3D> polygonalFace3Ds = Polyhedron_IsClosed_BoxFaces(0.0, 10.0, 0.0);

            // Replace the bottom face with an equivalent ring holding a repeated corner.
            IPolygonalFace3D? polygonalFace3D_Degenerate = Polyhedron_IsClosed_Face(
                new Point3D(0, 0, 0), new Point3D(10, 0, 0), new Point3D(10, 0, 0), new Point3D(10, 10, 0), new Point3D(0, 10, 0));
            Assert.NotNull(polygonalFace3D_Degenerate);
            polygonalFace3Ds[0] = polygonalFace3D_Degenerate;

            Polyhedron? polyhedron_Degenerate = Create.Polyhedron(polygonalFace3Ds);
            Assert.NotNull(polyhedron_Degenerate);

            Assert.True(polyhedron_Degenerate.IsClosed());
            Assert.True(polyhedron_Degenerate.IsClosed(true));
        }

        /// <summary>
        /// Documents that <see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, bool, double)"/> is a purely topological check that does not inspect winding.
        /// <para>Reversing the vertex order of one face of a closed box makes that face traverse its shared edges in the same direction as its neighbours rather than the opposite one. The polyhedron is still reported closed, because orientation is the job of <see cref="Polyhedron{TPolygonalFace3D}.Orient(DiGi.Geometry.Core.Enums.Orientation?, DiGi.Geometry.Core.Enums.Orientation?)"/>.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_InvertedFace()
        {
            List<IPolygonalFace3D> polygonalFace3Ds = Polyhedron_IsClosed_BoxFaces(0.0, 10.0, 0.0);

            // The bottom face with its ring reversed.
            IPolygonalFace3D? polygonalFace3D_Reversed = Polyhedron_IsClosed_Face(
                new Point3D(0, 10, 0), new Point3D(10, 10, 0), new Point3D(10, 0, 0), new Point3D(0, 0, 0));
            Assert.NotNull(polygonalFace3D_Reversed);
            polygonalFace3Ds[0] = polygonalFace3D_Reversed;

            Polyhedron? polyhedron_Inverted = Create.Polyhedron(polygonalFace3Ds);
            Assert.NotNull(polyhedron_Inverted);

            Assert.True(polyhedron_Inverted.IsClosed());
            Assert.True(polyhedron_Inverted.IsClosed(true));
        }

        /// <summary>
        /// Documents the T-junction limitation of <see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, bool, double)"/>.
        /// <para>The top face of a box is split into two halves while the four side faces remain single quads, so each side face contributes one long edge that the two halves meet mid-span. No vertex pair matches, and the geometrically watertight solid is reported open. This is a known limitation of vertex-based edge matching, not a defect to be worked around by widening the tolerance.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_TJunction()
        {
            Polyhedron? polyhedron_TJunction = Create.Polyhedron(Polyhedron_IsClosed_BoxFacesWithSplitTop(0.0, 10.0));
            Assert.NotNull(polyhedron_TJunction);
            Assert.Equal(7, polyhedron_TJunction.Count);

            Assert.False(polyhedron_TJunction.IsClosed());
            Assert.False(polyhedron_TJunction.IsClosed(true));
        }

        /// <summary>
        /// Verifies that <see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, bool, double)"/> survives coordinates far from the origin.
        /// <para>The spatial hash cell index is derived from the coordinate divided by the tolerance, so at GIS-scale coordinates that quotient grows very large. The grid is only an accelerator - a match is always confirmed by an explicit distance test - so a degraded grid can cost time or report open, but must never fabricate a closure.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_LargeCoordinates()
        {
            List<IPolygonalFace3D> polygonalFace3Ds = Polyhedron_IsClosed_BoxFaces(500000.0, 500010.0, 0.0);

            Polyhedron? polyhedron_Far = Create.Polyhedron(polygonalFace3Ds);
            Assert.NotNull(polyhedron_Far);

            Assert.True(polyhedron_Far.IsClosed());
            Assert.True(polyhedron_Far.IsClosed(true));

            // A box that is genuinely open stays open at the same coordinates.
            polygonalFace3Ds.RemoveAt(0);
            Polyhedron? polyhedron_FarOpen = Create.Polyhedron(polygonalFace3Ds);
            Assert.NotNull(polyhedron_FarOpen);
            Assert.False(polyhedron_FarOpen.IsClosed());
            Assert.False(polyhedron_FarOpen.IsClosed(true));
        }

        /// <summary>
        /// Verifies the tolerance fallback of <see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, bool, double)"/>.
        /// <para>A tolerance of zero or less falls back to <see cref="DiGi.Core.Constants.Tolerance.MicroDistance"/> rather than dividing by zero or producing an infinite cell index.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_ZeroTolerance()
        {
            BoundingBox3D boundingBox3D = new(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
            Polyhedron? polyhedron = Create.Polyhedron(boundingBox3D);
            Assert.NotNull(polyhedron);

            Assert.True(polyhedron.IsClosed(0.0));
            Assert.True(polyhedron.IsClosed(-1.0));
            Assert.True(polyhedron.IsClosed(true, 0.0));
            Assert.True(polyhedron.IsClosed(true, -1.0));
        }

        /// <summary>
        /// Verifies that the polyhedron and mesh closedness checks agree with each other.
        /// <para><see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, bool, double)"/> with the manifold criterion and <see cref="Core.Classes.Mesh{TPoint}.IsClosed"/> both require every edge to be used exactly twice, so triangulating a polyhedron into a <see cref="Mesh3D"/> must not change the verdict. The two implementations are independent, so this pins them against drifting apart.</para>
        /// <para>Faces carrying holes are deliberately excluded. Triangulating such a face bridges the external ring to the internal one and inserts extra vertices part-way along the external ring; the adjoining faces do not carry those vertices, so the triangulated mesh acquires T-junctions and is genuinely open even though the polyhedron is watertight. That is a property of the triangulation, not a disagreement between the two closedness checks.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_MatchesMesh3D()
        {
            List<Polyhedron> polyhedrons = [];

            Polyhedron? polyhedron_Box = Create.Polyhedron(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10)));
            Assert.NotNull(polyhedron_Box);
            polyhedrons.Add(polyhedron_Box);

            Polyhedron? polyhedron_Tetrahedron = Create.Polyhedron(Polyhedron_IsClosed_TetrahedronFaces());
            Assert.NotNull(polyhedron_Tetrahedron);
            polyhedrons.Add(polyhedron_Tetrahedron);

            Polyhedron? polyhedron_Ellipsoid = Create.Polyhedron(new Ellipsoid(new Point3D(1, 2, 3), 1, 2, 3), 8, 12);
            Assert.NotNull(polyhedron_Ellipsoid);
            polyhedrons.Add(polyhedron_Ellipsoid);

            Polyhedron? polyhedron_Extrusion = Polyhedron_IsClosed_Extrusion(32);
            Assert.NotNull(polyhedron_Extrusion);
            polyhedrons.Add(polyhedron_Extrusion);

            for (int int_I = 0; int_I < polyhedrons.Count; int_I++)
            {
                Polyhedron polyhedron = polyhedrons[int_I];

                Mesh3D? mesh3D = Create.Mesh3D(polyhedron);
                Assert.NotNull(mesh3D);

                Assert.True(polyhedron.IsClosed(true), $"Polyhedron {int_I} was expected to be closed.");
                Assert.True(mesh3D.IsClosed() == polyhedron.IsClosed(true), $"Polyhedron {int_I}: mesh reported {mesh3D.IsClosed()}, polyhedron reported {polyhedron.IsClosed(true)}.");
            }
        }

        /// <summary>
        /// Builds a planar face from the supplied ring of points.
        /// </summary>
        /// <param name="point3Ds">The ring of points defining the face boundary.</param>
        /// <returns>The face, or null if the points do not define a valid polygon.</returns>
        private static IPolygonalFace3D? Polyhedron_IsClosed_Face(params Point3D[] point3Ds)
        {
            Polygon3D? polygon3D = Create.Polygon3D(point3Ds);
            return polygon3D == null ? null : new PolygonalFace3D(polygon3D);
        }

        /// <summary>
        /// Builds the six faces of an axis-aligned box.
        /// </summary>
        /// <param name="min">The minimum coordinate on the X and Y axes, and the base of the box on the Z axis.</param>
        /// <param name="max">The maximum coordinate on the X and Y axes.</param>
        /// <param name="offsetZ">The offset applied to the box on the Z axis, used to stack two boxes into a non-manifold solid.</param>
        /// <returns>The six faces of the box.</returns>
        private static List<IPolygonalFace3D> Polyhedron_IsClosed_BoxFaces(double min, double max, double offsetZ)
        {
            double minZ = min + offsetZ;
            double maxZ = max + offsetZ;

            List<IPolygonalFace3D?> polygonalFace3Ds =
            [
                Polyhedron_IsClosed_Face(new Point3D(min, min, minZ), new Point3D(max, min, minZ), new Point3D(max, max, minZ), new Point3D(min, max, minZ)),
                Polyhedron_IsClosed_Face(new Point3D(min, min, maxZ), new Point3D(max, min, maxZ), new Point3D(max, max, maxZ), new Point3D(min, max, maxZ)),
                Polyhedron_IsClosed_Face(new Point3D(min, min, minZ), new Point3D(max, min, minZ), new Point3D(max, min, maxZ), new Point3D(min, min, maxZ)),
                Polyhedron_IsClosed_Face(new Point3D(min, max, minZ), new Point3D(max, max, minZ), new Point3D(max, max, maxZ), new Point3D(min, max, maxZ)),
                Polyhedron_IsClosed_Face(new Point3D(min, min, minZ), new Point3D(min, max, minZ), new Point3D(min, max, maxZ), new Point3D(min, min, maxZ)),
                Polyhedron_IsClosed_Face(new Point3D(max, min, minZ), new Point3D(max, max, minZ), new Point3D(max, max, maxZ), new Point3D(max, min, maxZ)),
            ];

            List<IPolygonalFace3D> result = [];
            for (int int_I = 0; int_I < polygonalFace3Ds.Count; int_I++)
            {
                IPolygonalFace3D? polygonalFace3D = polygonalFace3Ds[int_I];
                Assert.NotNull(polygonalFace3D);
                result.Add(polygonalFace3D);
            }

            return result;
        }

        /// <summary>
        /// Builds the four faces of a tetrahedron, the smallest closed solid.
        /// </summary>
        /// <returns>The four faces of the tetrahedron.</returns>
        private static List<IPolygonalFace3D> Polyhedron_IsClosed_TetrahedronFaces()
        {
            Point3D point3D_Origin = new(0, 0, 0);
            Point3D point3D_X = new(1, 0, 0);
            Point3D point3D_Y = new(0, 1, 0);
            Point3D point3D_Z = new(0, 0, 1);

            List<IPolygonalFace3D?> polygonalFace3Ds =
            [
                Polyhedron_IsClosed_Face(point3D_Origin, point3D_X, point3D_Y),
                Polyhedron_IsClosed_Face(point3D_Origin, point3D_X, point3D_Z),
                Polyhedron_IsClosed_Face(point3D_Origin, point3D_Y, point3D_Z),
                Polyhedron_IsClosed_Face(point3D_X, point3D_Y, point3D_Z),
            ];

            List<IPolygonalFace3D> result = [];
            for (int int_I = 0; int_I < polygonalFace3Ds.Count; int_I++)
            {
                IPolygonalFace3D? polygonalFace3D = polygonalFace3Ds[int_I];
                Assert.NotNull(polygonalFace3D);
                result.Add(polygonalFace3D);
            }

            return result;
        }

        /// <summary>
        /// Builds the faces of a box whose top face is displaced along the Z axis, leaving its corners short of the corners of the four side faces.
        /// </summary>
        /// <param name="min">The minimum coordinate of the box.</param>
        /// <param name="max">The maximum coordinate of the box.</param>
        /// <param name="displacement">The distance by which the top face is lifted above the side faces.</param>
        /// <returns>The six faces of the box.</returns>
        private static List<IPolygonalFace3D> Polyhedron_IsClosed_BoxFacesWithLiftedTop(double min, double max, double displacement)
        {
            List<IPolygonalFace3D> result = Polyhedron_IsClosed_BoxFaces(min, max, 0.0);

            double z = max + displacement;
            IPolygonalFace3D? polygonalFace3D_Top = Polyhedron_IsClosed_Face(
                new Point3D(min, min, z), new Point3D(max, min, z), new Point3D(max, max, z), new Point3D(min, max, z));
            Assert.NotNull(polygonalFace3D_Top);

            result[1] = polygonalFace3D_Top;

            return result;
        }

        /// <summary>
        /// Builds the faces of a box whose top face is split into two halves while the side faces stay single quads, producing a T-junction along each of the two side edges the split meets.
        /// </summary>
        /// <param name="min">The minimum coordinate of the box.</param>
        /// <param name="max">The maximum coordinate of the box.</param>
        /// <returns>The seven faces of the box.</returns>
        private static List<IPolygonalFace3D> Polyhedron_IsClosed_BoxFacesWithSplitTop(double min, double max)
        {
            List<IPolygonalFace3D> result = Polyhedron_IsClosed_BoxFaces(min, max, 0.0);

            double mid = (min + max) * 0.5;

            IPolygonalFace3D? polygonalFace3D_TopNear = Polyhedron_IsClosed_Face(
                new Point3D(min, min, max), new Point3D(max, min, max), new Point3D(max, mid, max), new Point3D(min, mid, max));
            Assert.NotNull(polygonalFace3D_TopNear);

            IPolygonalFace3D? polygonalFace3D_TopFar = Polyhedron_IsClosed_Face(
                new Point3D(min, mid, max), new Point3D(max, mid, max), new Point3D(max, max, max), new Point3D(min, max, max));
            Assert.NotNull(polygonalFace3D_TopFar);

            result[1] = polygonalFace3D_TopNear;
            result.Add(polygonalFace3D_TopFar);

            return result;
        }

        /// <summary>
        /// Builds a rectangular block pierced by a square hole along the Z axis. The top and bottom faces carry the hole as an internal ring.
        /// </summary>
        /// <param name="lined">When true, the four faces lining the hole are included, making the block watertight; when false they are omitted, leaving the hole boundary naked.</param>
        /// <returns>The faces of the block.</returns>
        private static List<IPolygonalFace3D> Polyhedron_IsClosed_BlockWithHoleFaces(bool lined)
        {
            double min = 0.0;
            double max = 10.0;
            double minHole = 3.0;
            double maxHole = 7.0;
            double height = 5.0;

            List<IPolygonalFace3D> result = [];

            // Bottom and top faces, each carrying the hole as an internal ring.
            double[] elevations = [min, height];
            for (int int_I = 0; int_I < elevations.Length; int_I++)
            {
                double z = elevations[int_I];

                Polygon3D? polygon3D_External = Create.Polygon3D([new Point3D(min, min, z), new Point3D(max, min, z), new Point3D(max, max, z), new Point3D(min, max, z)]);
                Assert.NotNull(polygon3D_External);

                Polygon3D? polygon3D_Internal = Create.Polygon3D([new Point3D(minHole, minHole, z), new Point3D(maxHole, minHole, z), new Point3D(maxHole, maxHole, z), new Point3D(minHole, maxHole, z)]);
                Assert.NotNull(polygon3D_Internal);

                PolygonalFace3D? polygonalFace3D = Create.PolygonalFace3D(polygon3D_External, [polygon3D_Internal]);
                Assert.NotNull(polygonalFace3D);

                result.Add(polygonalFace3D);
            }

            // Outer side faces.
            result.AddRange(Polyhedron_IsClosed_SideFaces(min, max, min, height));

            if (lined)
            {
                // Faces lining the hole.
                result.AddRange(Polyhedron_IsClosed_SideFaces(minHole, maxHole, min, height));
            }

            return result;
        }

        /// <summary>
        /// Builds the four vertical side faces of an axis-aligned square prism.
        /// </summary>
        /// <param name="min">The minimum coordinate on the X and Y axes.</param>
        /// <param name="max">The maximum coordinate on the X and Y axes.</param>
        /// <param name="minZ">The base elevation.</param>
        /// <param name="maxZ">The top elevation.</param>
        /// <returns>The four side faces.</returns>
        private static List<IPolygonalFace3D> Polyhedron_IsClosed_SideFaces(double min, double max, double minZ, double maxZ)
        {
            List<IPolygonalFace3D?> polygonalFace3Ds =
            [
                Polyhedron_IsClosed_Face(new Point3D(min, min, minZ), new Point3D(max, min, minZ), new Point3D(max, min, maxZ), new Point3D(min, min, maxZ)),
                Polyhedron_IsClosed_Face(new Point3D(min, max, minZ), new Point3D(max, max, minZ), new Point3D(max, max, maxZ), new Point3D(min, max, maxZ)),
                Polyhedron_IsClosed_Face(new Point3D(min, min, minZ), new Point3D(min, max, minZ), new Point3D(min, max, maxZ), new Point3D(min, min, maxZ)),
                Polyhedron_IsClosed_Face(new Point3D(max, min, minZ), new Point3D(max, max, minZ), new Point3D(max, max, maxZ), new Point3D(max, min, maxZ)),
            ];

            List<IPolygonalFace3D> result = [];
            for (int int_I = 0; int_I < polygonalFace3Ds.Count; int_I++)
            {
                IPolygonalFace3D? polygonalFace3D = polygonalFace3Ds[int_I];
                Assert.NotNull(polygonalFace3D);
                result.Add(polygonalFace3D);
            }

            return result;
        }

        /// <summary>
        /// Builds a closed polyhedron by extruding a regular polygon along the Z axis.
        /// </summary>
        /// <param name="count">The number of sides of the polygon being extruded.</param>
        /// <returns>The extruded polyhedron, or null if it could not be built.</returns>
        private static Polyhedron? Polyhedron_IsClosed_Extrusion(int count)
        {
            List<Point3D> point3Ds = new(count);
            for (int int_I = 0; int_I < count; int_I++)
            {
                double angle = int_I * 2.0 * System.Math.PI / count;
                point3Ds.Add(new Point3D(System.Math.Cos(angle) * 10.0, System.Math.Sin(angle) * 10.0, 0.0));
            }

            Polygon3D? polygon3D = Create.Polygon3D(point3Ds);
            if (polygon3D == null)
            {
                return null;
            }

            return Create.Polyhedron(new PolygonalFace3D(polygon3D), new Spatial.Classes.Vector3D(0, 0, 10));
        }
    }
}