using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;
using System.Collections.Generic;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// The tolerance ladder swept by the monotonicity facts, spanning six orders of magnitude around the feature sizes of the solids under test.
        /// </summary>
        private static readonly double[] Polyhedron_IsClosed_Tolerances = [1E-06, 1E-05, 0.0001, 0.001, 0.005, 0.01, 0.02, 0.03, 0.04, 0.05, 0.06, 0.09, 0.1, 0.11, 0.15, 0.2, 0.5];

        /// <summary>
        /// Verifies that the default criterion of <see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, bool, double)"/> is monotonic in tolerance on solids carrying a feature at the scale of the tolerance.
        /// <para>Regression for the reported defect: a real building was closed from 1E-06 to 0.04, open at 0.05 and closed again at 0.1, because welding at a tolerance equal to a genuine 5 cm feature collapsed some instances of it and not others. Nothing is welded now, so broadening the tolerance can only add compatible pairs and the verdict can never go from closed back to open.</para>
        /// <para>Covers the offline reproduction from the report - an extrusion carrying a 0.03 m step - a solid with a 0.1 m deep slot, and two thin slabs standing 0.02 m apart.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_Monotonic()
        {
            Polyhedron? polyhedron_Step = Polyhedron_IsClosed_StepExtrusion();
            Assert.NotNull(polyhedron_Step);
            Polyhedron_IsClosed_AssertMonotonic(polyhedron_Step);

            Polyhedron? polyhedron_Slot = Polyhedron_IsClosed_SlotExtrusion();
            Assert.NotNull(polyhedron_Slot);
            Polyhedron_IsClosed_AssertMonotonic(polyhedron_Slot);

            Polyhedron? polyhedron_Slabs = Polyhedron_IsClosed_ThinSlabSolid();
            Assert.NotNull(polyhedron_Slabs);
            Polyhedron_IsClosed_AssertMonotonic(polyhedron_Slabs);
        }

        /// <summary>
        /// Verifies that a genuine feature smaller than the tolerance no longer opens a closed solid in <see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, bool, double)"/>.
        /// <para>The 0.03 m step of the reported offline reproduction is a real corner of the footprint, not a duplicated point. Welding it away collapsed the faces meeting there and drove the shared edge to four uses, so the solid reported open at 0.05 while reporting closed at 0.02.</para>
        /// <para>The manifold criterion still goes false once the tolerance reaches the size of the step, which is by design: requiring an edge to be used exactly twice is a statement about a single edge, so a feature below the tolerance stops resolving as an edge of its own at that scale. The default criterion is the one that carries the monotonicity guarantee.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_SubToleranceFeature()
        {
            Polyhedron? polyhedron = Polyhedron_IsClosed_StepExtrusion();
            Assert.NotNull(polyhedron);

            // The value the report measured as open.
            Assert.True(polyhedron.IsClosed(0.05));

            // Well below, at and well above the 0.03 m step.
            Assert.True(polyhedron.IsClosed(0.02));
            Assert.True(polyhedron.IsClosed(0.03));
            Assert.True(polyhedron.IsClosed(0.5));

            // Manifold is scale-relative: the step resolves as an edge below its own size and stops resolving above it.
            Assert.True(polyhedron.IsClosed(true, 0.02));
            Assert.False(polyhedron.IsClosed(true, 0.05));
        }

        /// <summary>
        /// Verifies that <see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, bool, double)"/> keeps several distinct features apart when the tolerance spans all of them.
        /// <para>Two slabs 0.05 m and 0.035 m thick stand 0.02 m apart, so at a tolerance of 0.05 four parallel vertical edges lie within reach of one another. Welding chained them into a single vertex and collapsed both slabs; deciding the pairing per group of mutually reachable edges instead keeps every edge distinct and pairs each one off with its own partner.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_ThinSlabs()
        {
            Polyhedron? polyhedron = Polyhedron_IsClosed_ThinSlabSolid();
            Assert.NotNull(polyhedron);
            Assert.Equal(12, polyhedron.Count);

            Assert.True(polyhedron.IsClosed(0.001));
            Assert.True(polyhedron.IsClosed(0.05));
            Assert.True(polyhedron.IsClosed(0.5));
        }

        /// <summary>
        /// Verifies that <see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, bool, double)"/> does not depend on the order the faces are held in.
        /// <para>Welding compared a vertex only against the vertices already inserted, so which vertices merged depended on the order the faces were walked in and the same solid could be judged differently in two runs. No vertex is merged now, and the pairing is decided per group rather than by taking the first partner found, so the verdict is a property of the geometry alone.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_FaceOrderIndependence()
        {
            List<IPolygonalFace3D> polygonalFace3Ds = Polyhedron_IsClosed_ThinSlabFaces();

            Polyhedron? polyhedron = Create.Polyhedron(polygonalFace3Ds);
            Assert.NotNull(polyhedron);

            List<IPolygonalFace3D> polygonalFace3Ds_Reversed = new(polygonalFace3Ds);
            polygonalFace3Ds_Reversed.Reverse();

            Polyhedron? polyhedron_Reversed = Create.Polyhedron(polygonalFace3Ds_Reversed);
            Assert.NotNull(polyhedron_Reversed);

            // Seed 20260824 - fixed so a failure is reproducible.
            System.Random random = new(20260824);
            List<IPolygonalFace3D> polygonalFace3Ds_Shuffled = new(polygonalFace3Ds);
            for (int i = polygonalFace3Ds_Shuffled.Count - 1; i > 0; i--)
            {
                int index = random.Next(i + 1);
                (polygonalFace3Ds_Shuffled[i], polygonalFace3Ds_Shuffled[index]) = (polygonalFace3Ds_Shuffled[index], polygonalFace3Ds_Shuffled[i]);
            }

            Polyhedron? polyhedron_Shuffled = Create.Polyhedron(polygonalFace3Ds_Shuffled);
            Assert.NotNull(polyhedron_Shuffled);

            for (int i = 0; i < Polyhedron_IsClosed_Tolerances.Length; i++)
            {
                double tolerance = Polyhedron_IsClosed_Tolerances[i];

                Assert.Equal(polyhedron.IsClosed(tolerance), polyhedron_Reversed.IsClosed(tolerance));
                Assert.Equal(polyhedron.IsClosed(tolerance), polyhedron_Shuffled.IsClosed(tolerance));

                Assert.Equal(polyhedron.IsClosed(true, tolerance), polyhedron_Reversed.IsClosed(true, tolerance));
                Assert.Equal(polyhedron.IsClosed(true, tolerance), polyhedron_Shuffled.IsClosed(true, tolerance));
            }
        }

        /// <summary>
        /// Verifies that <see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, bool, double)"/> holds up when a coarse tolerance draws many edges into one group.
        /// <para>At the poles of a finely tessellated ellipsoid a hundred triangles meet, so a tolerance of 0.1 m or more puts far more than the thirty-two edges a bitmask search can enumerate into a single group. The pairing is decided by a general graph matching, which has no such ceiling; a ceiling would return open for a watertight solid and would break monotonicity at exactly the tolerance where the group outgrows it.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_LargeComponent()
        {
            Ellipsoid ellipsoid = new(new Point3D(1, 2, 3), 3, 2, 1);

            Polyhedron? polyhedron = Create.Polyhedron(ellipsoid, 50, 100);
            Assert.NotNull(polyhedron);
            Assert.Equal(9800, polyhedron.Count);

            Assert.True(polyhedron.IsClosed());
            Assert.True(polyhedron.IsClosed(0.1));
            Assert.True(polyhedron.IsClosed(0.5));
        }

        /// <summary>
        /// Verifies that <see cref="Query.IsClosed{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, bool, double)"/> bridges a gap exactly up to the tolerance and no further.
        /// <para>The top face of a box is lifted clear of the four side faces, so closure turns on whether the tolerance reaches the gap. Asserted at half the gap and at well over it rather than exactly at it, since a distance equal to the tolerance falls on the comparison boundary and is decided by the last bits of the projection.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_IsClosed_GapThreshold()
        {
            double displacement = 0.05;

            Polyhedron? polyhedron = Create.Polyhedron(Polyhedron_IsClosed_BoxFacesWithLiftedTop(0.0, 10.0, displacement));
            Assert.NotNull(polyhedron);

            Assert.False(polyhedron.IsClosed(0.5 * displacement));
            Assert.False(polyhedron.IsClosed(0.8 * displacement));
            Assert.True(polyhedron.IsClosed(1.2 * displacement));
            Assert.True(polyhedron.IsClosed(2.0 * displacement));

            Polyhedron_IsClosed_AssertMonotonic(polyhedron);
        }

        /// <summary>
        /// Verifies <see cref="Query.ClosingTolerance{TPolygonalFace3D}(Polyhedron{TPolygonalFace3D}?, IEnumerable{double}?, bool)"/> against the ladders it replaces.
        /// <para>The ladder is sorted before it is searched, so a caller-supplied order does not change the answer, and the bisection used for the default criterion must agree with a plain walk of the same ladder.</para>
        /// </summary>
        [Fact]
        public void Polyhedron_ClosingTolerance()
        {
            double[] tolerances = [1E-06, 1E-05, 0.0001, 0.001, 0.01, 0.05, 0.1, 0.2, 0.5];

            Polyhedron? polyhedron_Box = Create.Polyhedron(Polyhedron_IsClosed_BoxFaces(0.0, 10.0, 0.0));
            Assert.NotNull(polyhedron_Box);

            // A watertight box closes at the finest candidate on the ladder.
            Assert.Equal(1E-06, polyhedron_Box.ClosingTolerance(tolerances));

            // A gap of 0.05 needs the first candidate above it.
            Polyhedron? polyhedron_Lifted = Create.Polyhedron(Polyhedron_IsClosed_BoxFacesWithLiftedTop(0.0, 10.0, 0.05));
            Assert.NotNull(polyhedron_Lifted);
            Assert.Equal(0.1, polyhedron_Lifted.ClosingTolerance(tolerances));

            // The order the candidates arrive in does not matter.
            double[] tolerances_Shuffled = [0.1, 1E-05, 0.5, 0.001, 0.05, 0.2, 1E-06, 0.01, 0.0001];
            Assert.Equal(0.1, polyhedron_Lifted.ClosingTolerance(tolerances_Shuffled));

            // The bisection must agree with a plain walk of the same ladder.
            double? closingTolerance_Walked = null;
            for (int i = 0; i < tolerances.Length; i++)
            {
                if (polyhedron_Lifted.IsClosed(tolerances[i]))
                {
                    closingTolerance_Walked = tolerances[i];
                    break;
                }
            }

            Assert.Equal(closingTolerance_Walked, polyhedron_Lifted.ClosingTolerance(tolerances));

            // Nothing on the ladder reaches the gap.
            Assert.Null(polyhedron_Lifted.ClosingTolerance([1E-06, 1E-05, 0.0001, 0.001, 0.01]));

            // A solid that is open at every scale never closes.
            List<IPolygonalFace3D> polygonalFace3Ds_Open = Polyhedron_IsClosed_BoxFaces(0.0, 10.0, 0.0);
            polygonalFace3Ds_Open.RemoveAt(0);

            Polyhedron? polyhedron_Open = Create.Polyhedron(polygonalFace3Ds_Open);
            Assert.NotNull(polyhedron_Open);
            Assert.Null(polyhedron_Open.ClosingTolerance(tolerances));

            // Degenerate ladders and a null polyhedron.
            Assert.Null(polyhedron_Box.ClosingTolerance(null));
            Assert.Null(polyhedron_Box.ClosingTolerance([]));
            Assert.Null(polyhedron_Box.ClosingTolerance([0.0, -1.0]));
            Assert.Null(((Polyhedron?)null).ClosingTolerance(tolerances));

            // The manifold criterion is walked rather than bisected, and two glued boxes never satisfy it.
            List<IPolygonalFace3D> polygonalFace3Ds_Glued = Polyhedron_IsClosed_BoxFaces(0.0, 10.0, 0.0);
            polygonalFace3Ds_Glued.AddRange(Polyhedron_IsClosed_BoxFaces(0.0, 10.0, 10.0));

            Polyhedron? polyhedron_Glued = Create.Polyhedron(polygonalFace3Ds_Glued);
            Assert.NotNull(polyhedron_Glued);

            Assert.Equal(1E-06, polyhedron_Glued.ClosingTolerance(tolerances));
            Assert.Null(polyhedron_Glued.ClosingTolerance(tolerances, true));
        }

        /// <summary>
        /// Asserts that the default closure criterion never goes from closed back to open as the tolerance is broadened.
        /// </summary>
        /// <param name="polyhedron">The polyhedron to sweep.</param>
        private static void Polyhedron_IsClosed_AssertMonotonic(Polyhedron polyhedron)
        {
            bool closed = false;

            for (int i = 0; i < Polyhedron_IsClosed_Tolerances.Length; i++)
            {
                double tolerance = Polyhedron_IsClosed_Tolerances[i];
                bool closed_Temp = polyhedron.IsClosed(tolerance);

                Assert.True(!closed || closed_Temp, $"IsClosed went from closed back to open at tolerance {tolerance}.");
                closed = closed_Temp;
            }
        }

        /// <summary>
        /// Builds the extrusion of a footprint carrying a 0.03 m step, the offline reproduction from the report.
        /// </summary>
        /// <returns>The extruded solid, or null when the footprint is not a valid face.</returns>
        private static Polyhedron? Polyhedron_IsClosed_StepExtrusion()
        {
            Plane? plane = Create.Plane(0);
            Assert.NotNull(plane);

            PolygonalFace3D? polygonalFace3D = Create.PolygonalFace3D(
                plane,
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(5, 10),
                new Point2D(5, 9.97),
                new Point2D(0, 9.97));

            return polygonalFace3D == null ? null : Create.Polyhedron(polygonalFace3D, new Spatial.Classes.Vector3D(0, 0, 3));
        }

        /// <summary>
        /// Builds the extrusion of a 2 by 1 m footprint with a slot 0.1 m deep and 0.2 m wide cut into one edge.
        /// </summary>
        /// <returns>The extruded solid, or null when the footprint is not a valid face.</returns>
        private static Polyhedron? Polyhedron_IsClosed_SlotExtrusion()
        {
            Plane? plane = Create.Plane(0);
            Assert.NotNull(plane);

            PolygonalFace3D? polygonalFace3D = Create.PolygonalFace3D(
                plane,
                new Point2D(0, 0),
                new Point2D(2, 0),
                new Point2D(2, 0.4),
                new Point2D(1.9, 0.4),
                new Point2D(1.9, 0.6),
                new Point2D(2, 0.6),
                new Point2D(2, 1),
                new Point2D(0, 1));

            return polygonalFace3D == null ? null : Create.Polyhedron(polygonalFace3D, new Spatial.Classes.Vector3D(0, 0, 3));
        }

        /// <summary>
        /// Builds two thin slabs standing 0.02 m apart, 0.05 m and 0.035 m thick.
        /// </summary>
        /// <returns>The twelve faces of the two slabs.</returns>
        private static List<IPolygonalFace3D> Polyhedron_IsClosed_ThinSlabFaces()
        {
            List<IPolygonalFace3D> result = Polyhedron_IsClosed_BoxFaces(0.0, 0.05, 0.0, 4.0, 0.0, 3.0);
            result.AddRange(Polyhedron_IsClosed_BoxFaces(0.07, 0.105, 0.0, 4.0, 0.0, 3.0));

            return result;
        }

        /// <summary>
        /// Builds the polyhedron of two thin slabs standing 0.02 m apart.
        /// </summary>
        /// <returns>The polyhedron, or null when the faces do not form one.</returns>
        private static Polyhedron? Polyhedron_IsClosed_ThinSlabSolid()
        {
            return Create.Polyhedron(Polyhedron_IsClosed_ThinSlabFaces());
        }

        /// <summary>
        /// Builds the six faces of an axis-aligned box from explicit bounds on all three axes.
        /// </summary>
        /// <param name="minX">The minimum X coordinate.</param>
        /// <param name="maxX">The maximum X coordinate.</param>
        /// <param name="minY">The minimum Y coordinate.</param>
        /// <param name="maxY">The maximum Y coordinate.</param>
        /// <param name="minZ">The minimum Z coordinate.</param>
        /// <param name="maxZ">The maximum Z coordinate.</param>
        /// <returns>The six faces of the box.</returns>
        private static List<IPolygonalFace3D> Polyhedron_IsClosed_BoxFaces(double minX, double maxX, double minY, double maxY, double minZ, double maxZ)
        {
            List<IPolygonalFace3D?> polygonalFace3Ds =
            [
                Polyhedron_IsClosed_Face(new Point3D(minX, minY, minZ), new Point3D(maxX, minY, minZ), new Point3D(maxX, maxY, minZ), new Point3D(minX, maxY, minZ)),
                Polyhedron_IsClosed_Face(new Point3D(minX, minY, maxZ), new Point3D(maxX, minY, maxZ), new Point3D(maxX, maxY, maxZ), new Point3D(minX, maxY, maxZ)),
                Polyhedron_IsClosed_Face(new Point3D(minX, minY, minZ), new Point3D(maxX, minY, minZ), new Point3D(maxX, minY, maxZ), new Point3D(minX, minY, maxZ)),
                Polyhedron_IsClosed_Face(new Point3D(minX, maxY, minZ), new Point3D(maxX, maxY, minZ), new Point3D(maxX, maxY, maxZ), new Point3D(minX, maxY, maxZ)),
                Polyhedron_IsClosed_Face(new Point3D(minX, minY, minZ), new Point3D(minX, maxY, minZ), new Point3D(minX, maxY, maxZ), new Point3D(minX, minY, maxZ)),
                Polyhedron_IsClosed_Face(new Point3D(maxX, minY, minZ), new Point3D(maxX, maxY, minZ), new Point3D(maxX, maxY, maxZ), new Point3D(maxX, minY, maxZ)),
            ];

            List<IPolygonalFace3D> result = [];
            for (int i = 0; i < polygonalFace3Ds.Count; i++)
            {
                IPolygonalFace3D? polygonalFace3D = polygonalFace3Ds[i];
                Assert.NotNull(polygonalFace3D);
                result.Add(polygonalFace3D);
            }

            return result;
        }
    }
}
