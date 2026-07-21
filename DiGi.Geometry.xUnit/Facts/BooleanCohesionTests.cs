using DiGi.Geometry.Core.Enums;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Exhaustive test verifying boolean operation cohesion (A \ B) U (A n B) = A.
        /// </summary>
        [Fact]
        public void BooleanOperations_Cohesion_Reconstruction()
        {
            // Box A: [-2, 2]^3
            Point3D point3D_MinA = new(-2, -2, -2);
            Point3D point3D_MaxA = new(2, 2, 2);
            BoundingBox3D bboxA = new(point3D_MinA, point3D_MaxA);
            Polyhedron? polyhedron_A = Create.Polyhedron(bboxA);
            Assert.NotNull(polyhedron_A);

            // Box B: [0, 4]^3 (overlaps A)
            Point3D point3D_MinB = new(0, 0, 0);
            Point3D point3D_MaxB = new(4, 4, 4);
            BoundingBox3D bboxB = new(point3D_MinB, point3D_MaxB);
            Polyhedron? polyhedron_B = Create.Polyhedron(bboxB);
            Assert.NotNull(polyhedron_B);

            if (polyhedron_A == null || polyhedron_B == null)
            {
                return;
            }

            // 1. Calculate Difference: A \ B (removes a corner from A)
            DifferenceResult3D? diffResult = polyhedron_A.DifferenceResult3D(polyhedron_B);
            Assert.NotNull(diffResult);
            Assert.True(diffResult.Any());
            Polyhedron? diffPoly = diffResult.GetGeometry3Ds<Polyhedron>()?.FirstOrDefault();
            Assert.NotNull(diffPoly);

            // 2. Calculate Intersection: A n B (the corner itself)
            IntersectionResult3D? interResult = polyhedron_A.IntersectionResult3D(polyhedron_B);
            Assert.NotNull(interResult);
            Assert.True(interResult.Any());
            Polyhedron? interPoly = interResult.GetGeometry3Ds<Polyhedron>()?.FirstOrDefault();
            Assert.NotNull(interPoly);

            if (diffPoly == null || interPoly == null)
            {
                return;
            }

            // 3. Union the parts: (A \ B) U (A n B)
            // This should cleanly merge the two disjoint/touching boundaries back into the original box A.
            UnionResult3D? unionResult = diffPoly.UnionResult3D(interPoly);
            Assert.NotNull(unionResult);
            Assert.True(unionResult.Any());

            List<Polyhedron>? unifiedPolyhedrons = unionResult.GetGeometry3Ds<Polyhedron>();
            Assert.NotNull(unifiedPolyhedrons);
            Assert.Single(unifiedPolyhedrons);

            Polyhedron? reconstructedA = unifiedPolyhedrons.FirstOrDefault();
            Assert.NotNull(reconstructedA);

            if (reconstructedA != null)
            {
                BoundingBox3D? bboxResult = reconstructedA.GetBoundingBox();
                Assert.NotNull(bboxResult);
                if (bboxResult != null)
                {
                    // Bounding box of reconstructed A must match the original box A volume (64.0)
                    Assert.Equal(64.0, bboxResult.GetVolume(), 5);
                    Assert.Equal(-2.0, bboxResult.Min.X, 5);
                    Assert.Equal(2.0, bboxResult.Max.X, 5);
                }
            }
        }

        /// <summary>
        /// Exhaustive test verifying boolean operation cohesion when one polyhedron is fully inside another.
        /// </summary>
        [Fact]
        public void BooleanOperations_Cohesion_FullyContained()
        {
            // Outer Box A: [-3, 3]^3
            Point3D point3D_MinA = new(-3, -3, -3);
            Point3D point3D_MaxA = new(3, 3, 3);
            BoundingBox3D bboxA = new(point3D_MinA, point3D_MaxA);
            Polyhedron? polyhedron_A = Create.Polyhedron(bboxA);
            Assert.NotNull(polyhedron_A);

            // Inner Box B: [-1, 1]^3
            Point3D point3D_MinB = new(-1, -1, -1);
            Point3D point3D_MaxB = new(1, 1, 1);
            BoundingBox3D bboxB = new(point3D_MinB, point3D_MaxB);
            Polyhedron? polyhedron_B = Create.Polyhedron(bboxB);
            Assert.NotNull(polyhedron_B);

            if (polyhedron_A == null || polyhedron_B == null)
            {
                return;
            }

            // A n B = B (Inner box)
            IntersectionResult3D? interResult = polyhedron_A.IntersectionResult3D(polyhedron_B);
            Assert.NotNull(interResult);
            Polyhedron? interPoly = interResult.GetGeometry3Ds<Polyhedron>()?.FirstOrDefault();
            Assert.NotNull(interPoly);
            if (interPoly != null)
            {
                BoundingBox3D? bboxResult = interPoly.GetBoundingBox();
                Assert.NotNull(bboxResult);
                Assert.Equal(8.0, bboxResult?.GetVolume() ?? 0.0, 5); // 2^3 = 8
            }

            // B \ A = empty (Inner box minus Outer box)
            DifferenceResult3D? diffBA = polyhedron_B.DifferenceResult3D(polyhedron_A);
            Assert.NotNull(diffBA);
            Assert.False(diffBA.Any());

            // A U B = A (Outer box)
            UnionResult3D? unionResult = polyhedron_A.UnionResult3D(polyhedron_B);
            Assert.NotNull(unionResult);
            Polyhedron? unionPoly = unionResult.GetGeometry3Ds<Polyhedron>()?.FirstOrDefault();
            Assert.NotNull(unionPoly);
            if (unionPoly != null)
            {
                BoundingBox3D? bboxResult = unionPoly.GetBoundingBox();
                Assert.NotNull(bboxResult);
                Assert.Equal(216.0, bboxResult?.GetVolume() ?? 0.0, 5); // 6^3 = 216
            }
        }

        /// <summary>
        /// Tests the routing extension method on <see cref="BooleanOpertaionType"/> enum.
        /// </summary>
        [Fact]
        public void BooleanOpertaionType_Routing_Correctness()
        {
            Point3D point3D_MinA = new(-2, -2, -2);
            Point3D point3D_MaxA = new(2, 2, 2);
            BoundingBox3D bboxA = new(point3D_MinA, point3D_MaxA);
            Polyhedron? polyhedron_A = Create.Polyhedron(bboxA);
            Assert.NotNull(polyhedron_A);

            Point3D point3D_MinB = new(0, 0, 0);
            Point3D point3D_MaxB = new(4, 4, 4);
            BoundingBox3D bboxB = new(point3D_MinB, point3D_MaxB);
            Polyhedron? polyhedron_B = Create.Polyhedron(bboxB);
            Assert.NotNull(polyhedron_B);

            if (polyhedron_A == null || polyhedron_B == null)
            {
                return;
            }

            // Test Intersection Routing
            BooleanOperationResult3D? resultIntersection = BooleanOpertaionType.Intersection.BooleanOperationResult3D(polyhedron_A, polyhedron_B);
            Assert.NotNull(resultIntersection);
            Assert.IsType<IntersectionResult3D>(resultIntersection);
            Assert.Equal(BooleanOpertaionType.Intersection, resultIntersection.BooleanOpertaionType);

            // Test Difference Routing
            BooleanOperationResult3D? resultDifference = BooleanOpertaionType.Difference.BooleanOperationResult3D(polyhedron_A, polyhedron_B);
            Assert.NotNull(resultDifference);
            Assert.IsType<DifferenceResult3D>(resultDifference);
            Assert.Equal(BooleanOpertaionType.Difference, resultDifference.BooleanOpertaionType);

            // Test Union Routing
            BooleanOperationResult3D? resultUnion = BooleanOpertaionType.Union.BooleanOperationResult3D(polyhedron_A, polyhedron_B);
            Assert.NotNull(resultUnion);
            Assert.IsType<UnionResult3D>(resultUnion);
            Assert.Equal(BooleanOpertaionType.Union, resultUnion.BooleanOpertaionType);
        }
    }
}