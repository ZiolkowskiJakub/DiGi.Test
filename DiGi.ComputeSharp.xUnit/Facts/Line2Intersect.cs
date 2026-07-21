using DiGi.ComputeSharp.Core.Classes;
using DiGi.ComputeSharp.Planar.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Contains unit tests for the GPU-accelerated 2D line/triangle intersection predicate shader.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Tests parallel GPU intersection queries between a set of 2D lines and 2D triangles.
        /// Handles UnsupportedDoubleOperationException gracefully for FP64 unsupported GPUs.
        /// </summary>
        [Fact]
        public void Line2Intersect()
        {
            if (!Query.IsComputeSharpSupported(testOutputHelper))
            {
                return;
            }

            Triangle2 triangle2 = new(new Bool(true), new Coordinate2(0, 0), new Coordinate2(10, 0), new Coordinate2(5, 10));

            Line2 line2_Hit = new(new Coordinate2(-5, 5), new Coordinate2(15, 5));
            Line2 line2_Miss = new(new Coordinate2(-5, 20), new Coordinate2(15, 20));

            Line2[] line2s = [line2_Hit, line2_Miss];
            Triangle2[] triangle2s = [triangle2];

            try
            {
                List<bool>? bools_Results = Planar.Query.Intersect(line2s, triangle2s, true, true);

                Assert.NotNull(bools_Results);
                Assert.Equal(2, bools_Results!.Count);
                Assert.True(bools_Results[0]);  // line2_Hit crosses the triangle
                Assert.False(bools_Results[1]); // line2_Miss is clear of the triangle
            }
            catch (Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
            {
                testOutputHelper.WriteLine("WARNING: GPU FP64 double-precision operations are not supported on this graphics card: " + exception.Message);
            }
        }
    }
}