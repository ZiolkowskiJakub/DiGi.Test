using DiGi.ComputeSharp.Core.Constants;
using DiGi.ComputeSharp.Spatial.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Contains unit tests for creating 3D line intersections.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Tests the intersection of two 3D lines and checks if the calculated point is correct.
        /// Handles UnsupportedDoubleOperationException gracefully for FP64 unsupported GPUs.
        /// </summary>
        [Fact]
        public void Line3Intersection()
        {
            if (!Query.IsComputeSharpSupported(testOutputHelper))
            {
                return;
            }

            Line3 line3_1 = new(0, 0, 0, 10, 0, 0);
            Line3 line3_2 = new(1, -1, 0, 1, 1, 0);

            try
            {
                Line3Intersection line3Intersection_Result = Spatial.Create.Line3Intersection(line3_1, line3_2, Tolerance.Distance);

                Assert.False(line3Intersection_Result.IsNaN());
                Assert.Equal(1.0, line3Intersection_Result.Point_1.X);
                Assert.Equal(0.0, line3Intersection_Result.Point_1.Y);
                Assert.Equal(0.0, line3Intersection_Result.Point_1.Z);
            }
            catch (Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
            {
                testOutputHelper.WriteLine("WARNING: GPU FP64 double-precision operations are not supported on this graphics card: " + exception.Message);
            }
        }
    }
}