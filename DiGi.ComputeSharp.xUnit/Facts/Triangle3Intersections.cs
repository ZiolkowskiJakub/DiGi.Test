using DiGi.ComputeSharp.Core.Classes;
using DiGi.ComputeSharp.Core.Constants;
using DiGi.ComputeSharp.Spatial.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Contains unit tests for the chunked GPU triangle/triangle intersection path.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the chunked <see cref="Spatial.Create.Triangle3Intersections(Triangle3, System.Collections.Generic.IEnumerable{Triangle3}, double)"/>
        /// path covers every input triangle exactly once and produces the same set of intersections as the
        /// sequential CPU routine, for a list length that is not a round thread-group multiple.
        /// This guards the one-thread-per-triangle dispatch.
        /// Handles UnsupportedDoubleOperationException gracefully for FP64 unsupported GPUs.
        /// </summary>
        [Fact]
        public void Triangle3Intersections_ChunkedCoverage()
        {
            if (!Query.IsComputeSharpSupported(testOutputHelper))
            {
                return;
            }

            double tolerance = Tolerance.Distance;

            // Large flat solid triangle on the z = 0 plane.
            Triangle3 probe = new(new Bool(true), -50, -50, 0, 50, -50, 0, 0, 50, 0);

            List<Triangle3> triangles = [];

            // 7 vertical triangles whose bottom edge pierces the probe plane within its footprint -> intersect.
            for (int i = 0; i < 7; i++)
            {
                double x = -6 + (i * 2);
                triangles.Add(new Triangle3(new Bool(true), x, 0, -1, x, 0, 1, x + 2, 2, 0));
            }

            // 6 triangles parked far above the probe -> no intersection.
            for (int i = 0; i < 6; i++)
            {
                double x = -6 + (i * 2);
                triangles.Add(new Triangle3(new Bool(true), x, 0, 50, x, 0, 52, x + 2, 2, 50));
            }

            try
            {
                // CPU reference using the exact same intersection routine.
                int expected = 0;
                foreach (Triangle3 triangle in triangles)
                {
                    if (!Spatial.Create.Triangle3Intersection(probe, triangle, tolerance).IsNaN())
                    {
                        expected++;
                    }
                }

                // Sanity: the scenario must contain both hits and misses, otherwise the comparison is weak.
                Assert.True(expected > 0 && expected < triangles.Count, "Expected a mix of intersecting and non-intersecting triangles.");

                // GPU chunked path (uses threadsCount = 1024 internally).
                IEnumerable<Triangle3Intersection>? results = Spatial.Create.Triangle3Intersections(probe, triangles, tolerance);
                Assert.NotNull(results);
                Assert.Equal(expected, results!.Count());
            }
            catch (Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
            {
                testOutputHelper.WriteLine("WARNING: GPU FP64 double-precision operations are not supported on this graphics card: " + exception.Message);
            }
        }

        /// <summary>
        /// Two-collection overload (triangles_1 x triangles_2) with crossing (point/segment) intersections.
        /// Verifies the GPU result equals the sequential CPU reference in both count and row-major order.
        /// This is the before/after harness for the single-dispatch optimization of the two-collection path.
        /// </summary>
        [Fact]
        public void Triangle3Intersections_TwoCollections_Crossing()
        {
            if (!Query.IsComputeSharpSupported(testOutputHelper))
            {
                return;
            }

            double tolerance = Tolerance.Distance;

            // 3 flat triangles on z = 0, spread far apart along X.
            Triangle3[] triangles_1 =
            [
                new Triangle3(new Bool(true), -5, -5, 0,  5, -5, 0,  0, 5, 0),
                new Triangle3(new Bool(true), 20, -5, 0, 30, -5, 0, 25, 5, 0),
                new Triangle3(new Bool(true), 45, -5, 0, 55, -5, 0, 50, 5, 0),
            ];

            // 3 vertical triangles piercing z = 0 (one under each flat triangle) + 1 parked far above.
            Triangle3[] triangles_2 =
            [
                new Triangle3(new Bool(true),  0, 0, -1,  0, 0, 1,  2, 2, 0),
                new Triangle3(new Bool(true), 25, 0, -1, 25, 0, 1, 27, 2, 0),
                new Triangle3(new Bool(true), 50, 0, -1, 50, 0, 1, 52, 2, 0),
                new Triangle3(new Bool(true),  0, 0, 100, 0, 0, 102, 2, 2, 100),
            ];

            try
            {
                List<Triangle3Intersection> expected = CpuReference(triangles_1, triangles_2, tolerance);

                IEnumerable<Triangle3Intersection>? gpu = Spatial.Create.Triangle3Intersections(triangles_1, triangles_2, tolerance);
                Assert.NotNull(gpu);
                List<Triangle3Intersection> actual = [.. gpu!];

                testOutputHelper.WriteLine($"[Crossing] grid={triangles_1.Length}x{triangles_2.Length}, cpu non-NaN={expected.Count}, gpu non-NaN={actual.Count}");

                // The scenario: exactly one hit per flat triangle (diagonal), 3 total out of 12 pairs.
                Assert.Equal(3, expected.Count);
                AssertSameIntersections(expected, actual);
            }
            catch (Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
            {
                testOutputHelper.WriteLine("WARNING: GPU FP64 double-precision operations are not supported on this graphics card: " + exception.Message);
            }
        }

        /// <summary>
        /// Two-collection overload with overlapping coplanar triangles (polygon/segment intersections).
        /// Verifies the GPU result equals the sequential CPU reference in count and order.
        /// </summary>
        [Fact]
        public void Triangle3Intersections_TwoCollections_Coplanar()
        {
            if (!Query.IsComputeSharpSupported(testOutputHelper))
            {
                return;
            }

            double tolerance = Tolerance.Distance;

            Triangle3[] triangles_1 =
            [
                new Triangle3(new Bool(true),   0, 0, 0,  10, 0, 0,   0, 10, 0),
                new Triangle3(new Bool(true), 100, 0, 0, 110, 0, 0, 100, 10, 0),
            ];

            Triangle3[] triangles_2 =
            [
                new Triangle3(new Bool(true),   3, 3, 0,  13, 3, 0,   3, 13, 0), // overlaps triangles_1[0]
                new Triangle3(new Bool(true), 200, 0, 0, 210, 0, 0, 200, 10, 0), // overlaps nothing
            ];

            try
            {
                List<Triangle3Intersection> expected = CpuReference(triangles_1, triangles_2, tolerance);

                IEnumerable<Triangle3Intersection>? gpu = Spatial.Create.Triangle3Intersections(triangles_1, triangles_2, tolerance);
                Assert.NotNull(gpu);
                List<Triangle3Intersection> actual = [.. gpu!];

                testOutputHelper.WriteLine($"[Coplanar] grid={triangles_1.Length}x{triangles_2.Length}, cpu non-NaN={expected.Count}, gpu non-NaN={actual.Count}");

                Assert.True(expected.Count >= 1, "Expected at least one coplanar overlap.");
                AssertSameIntersections(expected, actual);
            }
            catch (Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
            {
                testOutputHelper.WriteLine("WARNING: GPU FP64 double-precision operations are not supported on this graphics card: " + exception.Message);
            }
        }

        /// <summary>
        /// Sequential CPU reference: row-major (triangles_1 outer, triangles_2 inner) non-NaN intersections,
        /// matching the documented ordering of the GPU two-collection overload.
        /// </summary>
        private static List<Triangle3Intersection> CpuReference(Triangle3[] triangles_1, Triangle3[] triangles_2, double tolerance)
        {
            List<Triangle3Intersection> result = [];
            foreach (Triangle3 triangle_1 in triangles_1)
            {
                foreach (Triangle3 triangle_2 in triangles_2)
                {
                    Triangle3Intersection triangle3Intersection = Spatial.Create.Triangle3Intersection(triangle_1, triangle_2, tolerance);
                    if (!triangle3Intersection.IsNaN())
                    {
                        result.Add(triangle3Intersection);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Asserts the GPU intersection list matches the CPU reference in count and order (first point within 1e-3).
        /// </summary>
        private static void AssertSameIntersections(List<Triangle3Intersection> expected, List<Triangle3Intersection> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.False(actual[i].IsNaN());
                Assert.True(actual[i].Point_1.AlmostEquals(expected[i].Point_1, 1e-3),
                    $"Intersection {i} point mismatch: gpu={actual[i].Point_1} cpu={expected[i].Point_1}");
            }
        }
    }
}
