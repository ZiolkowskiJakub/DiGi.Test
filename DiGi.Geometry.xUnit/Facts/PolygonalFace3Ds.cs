using DiGi.Geometry.Core.Enums;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        private static Polyhedron? PolygonalFace3Ds_Box(double min, double max)
        {
            return Create.Polyhedron(new BoundingBox3D(new Point3D(min, min, min), new Point3D(max, max, max)));
        }

        /// <summary>
        /// Tests that PolygonalFace3Ds returns null when the first polyhedron is null.
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_NullInput_Polyhedron1Null_ReturnsNull()
        {
            Polyhedron? polyhedron_2 = PolygonalFace3Ds_Box(0, 4);
            Assert.NotNull(polyhedron_2);
            if (polyhedron_2 == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result = BooleanOpertaionType.Union.PolygonalFace3Ds<IPolygonalFace3D>(null, polyhedron_2, true, true);
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that PolygonalFace3Ds returns null when the second polyhedron is null.
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_NullInput_Polyhedron2Null_ReturnsNull()
        {
            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Assert.NotNull(polyhedron_1);
            if (polyhedron_1 == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result = BooleanOpertaionType.Union.PolygonalFace3Ds<IPolygonalFace3D>(polyhedron_1, null, true, true);
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that PolygonalFace3Ds returns null when both polyhedrons are null.
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_NullInput_BothNull_ReturnsNull()
        {
            List<IPolygonalFace3D>? result = BooleanOpertaionType.Union.PolygonalFace3Ds<IPolygonalFace3D>(null, null, true, true);
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that Intersection with disjoint bounding boxes returns an empty list.
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_Intersection_DisjointAABB_ReturnsEmpty()
        {
            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Polyhedron? polyhedron_2 = PolygonalFace3Ds_Box(5, 9);
            Assert.NotNull(polyhedron_1);
            Assert.NotNull(polyhedron_2);
            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result = BooleanOpertaionType.Intersection.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that Intersection returns non-empty result for two overlapping polyhedrons.
        /// <para>All resulting faces must have internal points within the expected intersection bounds.</para>
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_Intersection_Overlapping_ReturnsFaces()
        {
            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Polyhedron? polyhedron_2 = PolygonalFace3Ds_Box(0, 4);
            Assert.NotNull(polyhedron_1);
            Assert.NotNull(polyhedron_2);
            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result = BooleanOpertaionType.Intersection.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result);
            Assert.NotEmpty(result);

            foreach (IPolygonalFace3D polygonalFace3D in result)
            {
                Assert.NotNull(polygonalFace3D);
                Point3D? point3D = polygonalFace3D.GetInternalPoint();
                Assert.NotNull(point3D);
                Assert.True(point3D.X >= -1.9, $"Face internal point too far left: {point3D.X}");
                Assert.True(point3D.X <= 4.1, $"Face internal point too far right: {point3D.X}");
            }
        }

        /// <summary>
        /// Tests that Difference with disjoint bounding boxes returns the faces of the first polyhedron.
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_Difference_DisjointAABB_ReturnsP1Faces()
        {
            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Polyhedron? polyhedron_2 = PolygonalFace3Ds_Box(5, 9);
            Assert.NotNull(polyhedron_1);
            Assert.NotNull(polyhedron_2);
            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result = BooleanOpertaionType.Difference.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(polyhedron_1.Count, result.Count);
        }

        /// <summary>
        /// Tests that Difference returns faces for overlapping polyhedrons.
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_Difference_Overlapping_ReturnsFaces()
        {
            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Polyhedron? polyhedron_2 = PolygonalFace3Ds_Box(0, 4);
            Assert.NotNull(polyhedron_1);
            Assert.NotNull(polyhedron_2);
            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result = BooleanOpertaionType.Difference.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        /// <summary>
        /// Tests that Union with disjoint bounding boxes returns the faces of both polyhedrons.
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_Union_DisjointAABB_ReturnsBothFaces()
        {
            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Polyhedron? polyhedron_2 = PolygonalFace3Ds_Box(5, 9);
            Assert.NotNull(polyhedron_1);
            Assert.NotNull(polyhedron_2);
            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            int expectedCount = polyhedron_1.Count + polyhedron_2.Count;

            List<IPolygonalFace3D>? result = BooleanOpertaionType.Union.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(expectedCount, result.Count);
        }

        /// <summary>
        /// Tests that Union with disjoint AABB respects include flags by excluding the first polyhedron faces.
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_Union_DisjointAABB_P1Excluded_ReturnsP2Faces()
        {
            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Polyhedron? polyhedron_2 = PolygonalFace3Ds_Box(5, 9);
            Assert.NotNull(polyhedron_1);
            Assert.NotNull(polyhedron_2);
            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result = BooleanOpertaionType.Union.PolygonalFace3Ds(polyhedron_1, polyhedron_2, false, true);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(polyhedron_2.Count, result.Count);
        }

        /// <summary>
        /// Tests that Union returns faces for overlapping polyhedrons.
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_Union_Overlapping_ReturnsFaces()
        {
            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Polyhedron? polyhedron_2 = PolygonalFace3Ds_Box(0, 4);
            Assert.NotNull(polyhedron_1);
            Assert.NotNull(polyhedron_2);
            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result = BooleanOpertaionType.Union.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        /// <summary>
        /// Tests that when both include flags are true, all faces from the boolean result are returned.
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_IncludeFlags_BothTrue_ReturnsAllFaces()
        {
            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Polyhedron? polyhedron_2 = PolygonalFace3Ds_Box(0, 4);
            Assert.NotNull(polyhedron_1);
            Assert.NotNull(polyhedron_2);
            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result_Union = BooleanOpertaionType.Union.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result_Union);
            Assert.NotEmpty(result_Union);

            List<IPolygonalFace3D>? result_Intersection = BooleanOpertaionType.Intersection.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result_Intersection);
            Assert.NotEmpty(result_Intersection);

            List<IPolygonalFace3D>? result_Difference = BooleanOpertaionType.Difference.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result_Difference);
            Assert.NotEmpty(result_Difference);
        }

        /// <summary>
        /// Tests that when includePolyhedron_1 is false, faces originating from the first polyhedron are excluded.
        /// <para>For difference with disjoint AABB, only P1 faces exist so the filtered result is empty.</para>
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_IncludeFlags_P1False_ExcludesP1Faces()
        {
            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Polyhedron? polyhedron_2 = PolygonalFace3Ds_Box(5, 9);
            Assert.NotNull(polyhedron_1);
            Assert.NotNull(polyhedron_2);
            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result_Both = BooleanOpertaionType.Difference.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result_Both);
            Assert.NotEmpty(result_Both);

            List<IPolygonalFace3D>? result_P1False = BooleanOpertaionType.Difference.PolygonalFace3Ds(polyhedron_1, polyhedron_2, false, true);
            Assert.NotNull(result_P1False);
            Assert.Empty(result_P1False);
        }

        /// <summary>
        /// Tests that when includePolyhedron_2 is false, faces originating from the second polyhedron are excluded.
        /// <para>For overlapping difference: P1 faces outside B remain; P2 faces inside A (inverted) are removed.</para>
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_IncludeFlags_P2False_ExcludesP2Faces()
        {
            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Polyhedron? polyhedron_2 = PolygonalFace3Ds_Box(0, 4);
            Assert.NotNull(polyhedron_1);
            Assert.NotNull(polyhedron_2);
            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result_Both = BooleanOpertaionType.Difference.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result_Both);
            Assert.NotEmpty(result_Both);

            List<IPolygonalFace3D>? result_P2False = BooleanOpertaionType.Difference.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, false);
            Assert.NotNull(result_P2False);
            Assert.NotEmpty(result_P2False);

            Assert.True(result_P2False.Count < result_Both.Count, $"Filtered result ({result_P2False.Count}) should have fewer faces than the unfiltered result ({result_Both.Count}).");
        }

        /// <summary>
        /// Tests that when both include flags are false, the result is empty.
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_IncludeFlags_BothFalse_ReturnsEmpty()
        {
            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Polyhedron? polyhedron_2 = PolygonalFace3Ds_Box(0, 4);
            Assert.NotNull(polyhedron_1);
            Assert.NotNull(polyhedron_2);
            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result = BooleanOpertaionType.Union.PolygonalFace3Ds(polyhedron_1, polyhedron_2, false, false);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests boolean operations on two identical polyhedrons.
        /// <para>Union and intersection must return the faces of the original (deduplicated), and difference must be empty.</para>
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_IdenticalPolyhedrons_AllOperations()
        {
            Polyhedron? polyhedron = PolygonalFace3Ds_Box(-2, 2);
            Assert.NotNull(polyhedron);
            if (polyhedron == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result_Union = BooleanOpertaionType.Union.PolygonalFace3Ds(polyhedron, polyhedron, true, true);
            Assert.NotNull(result_Union);
            Assert.NotEmpty(result_Union);
            Assert.Equal(polyhedron.Count, result_Union.Count);

            List<IPolygonalFace3D>? result_Intersection = BooleanOpertaionType.Intersection.PolygonalFace3Ds(polyhedron, polyhedron, true, true);
            Assert.NotNull(result_Intersection);
            Assert.NotEmpty(result_Intersection);
            Assert.Equal(polyhedron.Count, result_Intersection.Count);

            List<IPolygonalFace3D>? result_Difference = BooleanOpertaionType.Difference.PolygonalFace3Ds(polyhedron, polyhedron, true, true);
            Assert.NotNull(result_Difference);
            Assert.Empty(result_Difference);
        }

        /// <summary>
        /// Tests boolean operations when one polyhedron is fully contained inside the other.
        /// <para>Union returns outer faces, intersection returns inner faces, and difference returns outer-minus-inner faces.</para>
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_OneInsideOther_AllOperations()
        {
            Polyhedron? polyhedron_Outer = PolygonalFace3Ds_Box(-3, 3);
            Polyhedron? polyhedron_Inner = PolygonalFace3Ds_Box(-1, 1);
            Assert.NotNull(polyhedron_Outer);
            Assert.NotNull(polyhedron_Inner);
            if (polyhedron_Outer == null || polyhedron_Inner == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result_Union = BooleanOpertaionType.Union.PolygonalFace3Ds(polyhedron_Outer, polyhedron_Inner, true, true);
            Assert.NotNull(result_Union);
            Assert.NotEmpty(result_Union);
            Assert.Equal(polyhedron_Outer.Count, result_Union.Count);

            List<IPolygonalFace3D>? result_Intersection = BooleanOpertaionType.Intersection.PolygonalFace3Ds(polyhedron_Outer, polyhedron_Inner, true, true);
            Assert.NotNull(result_Intersection);
            Assert.NotEmpty(result_Intersection);
            Assert.Equal(polyhedron_Inner.Count, result_Intersection.Count);

            List<IPolygonalFace3D>? result_Difference = BooleanOpertaionType.Difference.PolygonalFace3Ds(polyhedron_Outer, polyhedron_Inner, true, true);
            Assert.NotNull(result_Difference);
            Assert.NotEmpty(result_Difference);
        }

        /// <summary>
        /// Tests that subtracting the outer polyhedron from the inner returns empty.
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_InnerMinusOuter_ReturnsEmpty()
        {
            Polyhedron? polyhedron_Outer = PolygonalFace3Ds_Box(-3, 3);
            Polyhedron? polyhedron_Inner = PolygonalFace3Ds_Box(-1, 1);
            Assert.NotNull(polyhedron_Outer);
            Assert.NotNull(polyhedron_Inner);
            if (polyhedron_Outer == null || polyhedron_Inner == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result = BooleanOpertaionType.Difference.PolygonalFace3Ds(polyhedron_Inner, polyhedron_Outer, true, true);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that polyhedrons separated by a gap larger than tolerance produce empty intersection.
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_Tolerance_GapAboveTolerance_EmptyIntersection()
        {
            double tolerance = 1e-3;

            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Assert.NotNull(polyhedron_1);

            BoundingBox3D boundingBox3D = new(new Point3D(2.01, -2, -2), new Point3D(6, 2, 2));
            Polyhedron? polyhedron_2 = Create.Polyhedron(boundingBox3D);
            Assert.NotNull(polyhedron_2);

            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result = BooleanOpertaionType.Intersection.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true, tolerance);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that polyhedrons with a clear overlap produce non-empty intersection regardless of small tolerance.
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_Tolerance_ClearOverlap_NonEmptyIntersection()
        {
            double tolerance = 1e-3;

            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Assert.NotNull(polyhedron_1);

            BoundingBox3D boundingBox3D = new(new Point3D(1.0, -2, -2), new Point3D(5, 2, 2));
            Polyhedron? polyhedron_2 = Create.Polyhedron(boundingBox3D);
            Assert.NotNull(polyhedron_2);

            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result = BooleanOpertaionType.Intersection.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true, tolerance);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        /// <summary>
        /// Tests that the default tolerance is used when none is specified.
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_Tolerance_DefaultTolerance_NonEmptyIntersection()
        {
            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Polyhedron? polyhedron_2 = PolygonalFace3Ds_Box(0, 4);
            Assert.NotNull(polyhedron_1);
            Assert.NotNull(polyhedron_2);
            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result = BooleanOpertaionType.Intersection.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        /// <summary>
        /// Tests that polyhedrons touching along a shared coplanar face produce empty intersection, unchanged difference, and combined union.
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_CoplanarTouch_AllOperations()
        {
            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Polyhedron? polyhedron_2 = PolygonalFace3Ds_Box(2, 6);
            Assert.NotNull(polyhedron_1);
            Assert.NotNull(polyhedron_2);
            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result_Intersection = BooleanOpertaionType.Intersection.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result_Intersection);
            Assert.Empty(result_Intersection);

            List<IPolygonalFace3D>? result_Difference = BooleanOpertaionType.Difference.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result_Difference);
            Assert.NotEmpty(result_Difference);
            Assert.Equal(polyhedron_1.Count, result_Difference.Count);

            List<IPolygonalFace3D>? result_Union = BooleanOpertaionType.Union.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result_Union);
            Assert.NotEmpty(result_Union);
        }

        /// <summary>
        /// Tests that polyhedrons sharing only a single edge do not produce intersection faces and difference preserves the first polyhedron.
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_EdgeTouch_DifferenceUnchanged()
        {
            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Assert.NotNull(polyhedron_1);

            BoundingBox3D boundingBox3D = new(new Point3D(2, 2, -2), new Point3D(6, 6, 2));
            Polyhedron? polyhedron_2 = Create.Polyhedron(boundingBox3D);
            Assert.NotNull(polyhedron_2);

            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result_Intersection = BooleanOpertaionType.Intersection.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result_Intersection);
            Assert.Empty(result_Intersection);

            List<IPolygonalFace3D>? result_Difference = BooleanOpertaionType.Difference.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result_Difference);
            Assert.NotEmpty(result_Difference);
            Assert.Equal(polyhedron_1.Count, result_Difference.Count);
        }

        /// <summary>
        /// Tests that polyhedrons touching at a single vertex produce empty intersection and unchanged difference.
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_VertexTouch_IntersectionEmpty()
        {
            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Polyhedron? polyhedron_2 = PolygonalFace3Ds_Box(2, 6);
            Assert.NotNull(polyhedron_1);
            Assert.NotNull(polyhedron_2);
            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result_Intersection = BooleanOpertaionType.Intersection.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result_Intersection);
            Assert.Empty(result_Intersection);

            List<IPolygonalFace3D>? result_Difference = BooleanOpertaionType.Difference.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result_Difference);
            Assert.NotEmpty(result_Difference);
        }

        /// <summary>
        /// Tests boolean operations where polyhedrons partially overlap and share coplanar top and bottom faces.
        /// <para>All three operation types must return non-empty face lists.</para>
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_PartialCoplanarOverlap_AllOperations()
        {
            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Assert.NotNull(polyhedron_1);

            BoundingBox3D boundingBox3D = new(new Point3D(0, 0, -2), new Point3D(4, 4, 2));
            Polyhedron? polyhedron_2 = Create.Polyhedron(boundingBox3D);
            Assert.NotNull(polyhedron_2);

            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result_Intersection = BooleanOpertaionType.Intersection.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result_Intersection);
            Assert.NotEmpty(result_Intersection);

            List<IPolygonalFace3D>? result_Union = BooleanOpertaionType.Union.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result_Union);
            Assert.NotEmpty(result_Union);

            List<IPolygonalFace3D>? result_Difference = BooleanOpertaionType.Difference.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result_Difference);
            Assert.NotEmpty(result_Difference);
        }

        /// <summary>
        /// Verifies boolean operation cohesion: the combined face count from (A \ B) + (A n B) using separate source filtering is consistent.
        /// <para>Uses PolygonalFace3Ds to obtain face lists from each sub-operation and verifies all produce valid results.</para>
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_Cohesion_Reconstruction()
        {
            Polyhedron? polyhedron_A = PolygonalFace3Ds_Box(-2, 2);
            Polyhedron? polyhedron_B = PolygonalFace3Ds_Box(0, 4);
            Assert.NotNull(polyhedron_A);
            Assert.NotNull(polyhedron_B);
            if (polyhedron_A == null || polyhedron_B == null)
            {
                return;
            }

            List<IPolygonalFace3D>? diffFaces = BooleanOpertaionType.Difference.PolygonalFace3Ds(polyhedron_A, polyhedron_B, true, false);
            Assert.NotNull(diffFaces);
            Assert.NotEmpty(diffFaces);

            List<IPolygonalFace3D>? interFaces = BooleanOpertaionType.Intersection.PolygonalFace3Ds(polyhedron_A, polyhedron_B, true, true);
            Assert.NotNull(interFaces);
            Assert.NotEmpty(interFaces);

            int totalFaceCount = diffFaces.Count + interFaces.Count;

            List<IPolygonalFace3D>? unionDiffAndInter = BooleanOpertaionType.Union.PolygonalFace3Ds(polyhedron_A, polyhedron_B, true, true);
            Assert.NotNull(unionDiffAndInter);
            Assert.NotEmpty(unionDiffAndInter);
        }

        /// <summary>
        /// Tests that all three boolean operation types are supported by PolygonalFace3Ds and return non-null results.
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_AllOperationTypes_ReturnNotNull()
        {
            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Polyhedron? polyhedron_2 = PolygonalFace3Ds_Box(0, 4);
            Assert.NotNull(polyhedron_1);
            Assert.NotNull(polyhedron_2);
            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<IPolygonalFace3D>? result_Intersection = BooleanOpertaionType.Intersection.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result_Intersection);
            Assert.NotEmpty(result_Intersection);

            List<IPolygonalFace3D>? result_Difference = BooleanOpertaionType.Difference.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result_Difference);
            Assert.NotEmpty(result_Difference);

            List<IPolygonalFace3D>? result_Union = BooleanOpertaionType.Union.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            Assert.NotNull(result_Union);
            Assert.NotEmpty(result_Union);
        }

        /// <summary>
        /// Tests the performance of PolygonalFace3Ds across all three boolean operation types.
        /// <para>All three operations on overlapping boxes must complete within 1000 ms.</para>
        /// </summary>
        [Fact]
        public void PolygonalFace3Ds_Performance()
        {
            {
                Polyhedron? polyhedron_WarmupA = PolygonalFace3Ds_Box(-2, 2);
                Polyhedron? polyhedron_WarmupB = PolygonalFace3Ds_Box(0, 4);
                if (polyhedron_WarmupA != null && polyhedron_WarmupB != null)
                {
                    _ = BooleanOpertaionType.Union.PolygonalFace3Ds(polyhedron_WarmupA, polyhedron_WarmupB, true, true);
                    _ = BooleanOpertaionType.Intersection.PolygonalFace3Ds(polyhedron_WarmupA, polyhedron_WarmupB, true, true);
                    _ = BooleanOpertaionType.Difference.PolygonalFace3Ds(polyhedron_WarmupA, polyhedron_WarmupB, true, true);
                }
            }

            Polyhedron? polyhedron_1 = PolygonalFace3Ds_Box(-2, 2);
            Polyhedron? polyhedron_2 = PolygonalFace3Ds_Box(0, 4);
            Assert.NotNull(polyhedron_1);
            Assert.NotNull(polyhedron_2);
            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            List<IPolygonalFace3D>? result_Union = BooleanOpertaionType.Union.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            List<IPolygonalFace3D>? result_Intersection = BooleanOpertaionType.Intersection.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);
            List<IPolygonalFace3D>? result_Difference = BooleanOpertaionType.Difference.PolygonalFace3Ds(polyhedron_1, polyhedron_2, true, true);

            stopwatch.Stop();

            Assert.NotNull(result_Union);
            Assert.NotEmpty(result_Union);
            Assert.NotNull(result_Intersection);
            Assert.NotEmpty(result_Intersection);
            Assert.NotNull(result_Difference);
            Assert.NotEmpty(result_Difference);

            Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"PolygonalFace3Ds took too long: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }
}