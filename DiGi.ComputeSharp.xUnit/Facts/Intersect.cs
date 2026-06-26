using DiGi.ComputeSharp.Core.Classes;
using DiGi.ComputeSharp.Spatial.Classes;
using Xunit.Abstractions;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Contains unit tests for querying 3D geometric intersections.
    /// </summary>
    public partial class Facts
    {
        private readonly ITestOutputHelper testOutputHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="Facts"/> class.
        /// </summary>
        /// <param name="testOutputHelper">The test output helper used to write log/warning messages.</param>
        public Facts(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Tests parallel GPU intersection queries between a set of 3D lines and 3D triangles.
        /// Handles UnsupportedDoubleOperationException gracefully for FP64 unsupported GPUs.
        /// </summary>
        [Fact]
        public void Intersect()
        {
            if (!Query.IsComputeSharpSupported(testOutputHelper))
            {
                return;
            }

            Triangle3 triangle3_1 = new(new(true), -1, -1, 0, 10, 10, 0, 10, -10, 0);
            Triangle3 triangle3_2 = new(new(true), -1, -1, 1, 10, 10, 1, 10, -10, 1);
            Triangle3 triangle3_3 = new(new(true), -1, -1, 0, 10, 10, 0, 10, -10, 0);

            Line3 line3_1 = new(0, 0, 0, 20, 0, 0);
            Line3 line3_2 = new(0, 0, 1, 20, 0, 1);
            Line3 line3_3 = new(0, 0, 2, 20, 0, 2);

            Line3[] line3s = [line3_2, line3_1, line3_3];
            Triangle3[] triangle3s = [triangle3_1, triangle3_2, triangle3_3];

            try
            {
                List<bool>? bools_Results = Spatial.Query.Intersect(line3s, triangle3s, true, true);

                Assert.NotNull(bools_Results);
                Assert.Equal(3, bools_Results!.Count);
                Assert.True(bools_Results[0]); // line3_2 (z=1) intersects triangle3_2 (z=1)
                Assert.True(bools_Results[1]); // line3_1 (z=0) intersects triangle3_1 (z=0)
                Assert.False(bools_Results[2]); // line3_3 (z=2) does not intersect any triangle
            }
            catch (Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
            {
                testOutputHelper.WriteLine("WARNING: GPU FP64 double-precision operations are not supported on this graphics card: " + exception.Message);
            }
        }
    }
}
