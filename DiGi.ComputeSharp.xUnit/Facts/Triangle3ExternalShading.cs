using ComputeSharp;
using DiGi.ComputeSharp.Core.Constants;
using DiGi.ComputeSharp.Spatial.Classes;
using Bool = DiGi.ComputeSharp.Core.Classes.Bool;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Contains unit tests for <see cref="Triangle3ExternalShadingComputeShader"/>.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the external-shading shader writes results at a row stride equal to the number of
        /// external triangles. Regression test for the bug where the stride used the (shaded) triangle count
        /// instead, which collided/over-ran the output buffer whenever the two counts differed.
        /// Handles UnsupportedDoubleOperationException gracefully for FP64 unsupported GPUs.
        /// </summary>
        [Fact]
        public void Triangle3ExternalShading_NonSquareStride()
        {
            if (!Query.IsComputeSharpSupported(testOutputHelper))
            {
                return;
            }

            double tolerance = Tolerance.Distance;
            Coordinate3 vector = new(0, 0, 1);

            // 3 "shaded" triangles at z = 1, spread far apart in X so overlaps are unambiguous.
            Triangle3[] triangles =
            [
                new Triangle3(new Bool(true),   0, 0, 1,   5, 5, 1,   5, 0, 1),
                new Triangle3(new Bool(true), 100, 0, 1, 105, 5, 1, 105, 0, 1),
                new Triangle3(new Bool(true), 200, 0, 1, 205, 5, 1, 205, 0, 1),
            ];

            // 2 "shading-only" triangles at z = 0; E0 sits under T0, E1 sits under T2.
            // Note: triangles.Length (3) != externalTriangles.Length (2), which is what exposes the stride bug.
            Triangle3[] externalTriangles =
            [
                new Triangle3(new Bool(true),   0, 0, 0,   5, 5, 0,   5, 0, 0),
                new Triangle3(new Bool(true), 200, 0, 0, 205, 5, 0, 205, 0, 0),
            ];

            int count = triangles.Length;
            int count_External = externalTriangles.Length;

            try
            {
                GraphicsDevice graphicsDevice = GraphicsDevice.GetDefault();

                using ReadOnlyBuffer<Triangle3> trianglesBuffer = graphicsDevice.AllocateReadOnlyBuffer(triangles);
                using ReadOnlyBuffer<Triangle3> externalBuffer = graphicsDevice.AllocateReadOnlyBuffer(externalTriangles);
                using ReadWriteBuffer<Triangle3Intersection> resultBuffer = graphicsDevice.AllocateReadWriteBuffer<Triangle3Intersection>(count * count_External);

                graphicsDevice.For(count, count_External, new Triangle3ExternalShadingComputeShader(trianglesBuffer, externalBuffer, resultBuffer, vector, tolerance));

                List<Triangle3Intersection>? results = DiGi.ComputeSharp.Core.Create.List(resultBuffer);
                Assert.NotNull(results);
                Assert.Equal(count * count_External, results!.Count);

                int nonNaN = 0;
                for (int i = 0; i < count; i++)
                {
                    for (int j = 0; j < count_External; j++)
                    {
                        // Reference computed identically on the CPU.
                        Triangle3Intersection expected = Spatial.Create.Triangle3Intersection(triangles[i], externalTriangles[j], vector, true, false, tolerance);

                        // Result must live at row stride == externalTriangles.Length (the corrected stride).
                        Triangle3Intersection actual = results[i * count_External + j];

                        Assert.Equal(expected.IsNaN(), actual.IsNaN());

                        if (!expected.IsNaN())
                        {
                            nonNaN++;
                        }
                    }
                }

                // Sanity: the chosen geometry must actually produce shading hits, otherwise the stride
                // check above would be vacuous.
                Assert.True(nonNaN > 0, "Expected at least one shading intersection; test geometry produced none.");
            }
            catch (Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
            {
                testOutputHelper.WriteLine("WARNING: GPU FP64 double-precision operations are not supported on this graphics card: " + exception.Message);
            }
        }
    }
}
