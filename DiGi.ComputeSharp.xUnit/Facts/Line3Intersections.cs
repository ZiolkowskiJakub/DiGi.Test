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
        /// Tests the intersection of one 3D line against a collection of other 3D lines.
        /// Handles UnsupportedDoubleOperationException gracefully for FP64 unsupported GPUs.
        /// </summary>
        [Fact]
        public void Line3Intersections()
        {
            if (!Query.IsComputeSharpSupported(testOutputHelper))
            {
                return;
            }

            Line3 line3_1 = new(0, 0, 0, 10, 0, 0);
            Line3 line3_2 = new(1, -1, 0, 1, 1, 0);
            Line3[] line3s_Target = [line3_2];

            try
            {
                IEnumerable<Line3Intersection>? line3Intersections_Result = DiGi.ComputeSharp.Spatial.Create.Line3Intersections(line3_1, line3s_Target, Tolerance.Distance);
                Assert.NotNull(line3Intersections_Result);
                Line3Intersection line3Intersection_First = line3Intersections_Result!.First();

                Assert.False(line3Intersection_First.IsNaN());
                Assert.Equal(1.0, line3Intersection_First.Point_1.X);
                Assert.Equal(0.0, line3Intersection_First.Point_1.Y);
                Assert.Equal(0.0, line3Intersection_First.Point_1.Z);
            }
            catch (Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
            {
                testOutputHelper.WriteLine("WARNING: GPU FP64 double-precision operations are not supported on this graphics card: " + exception.Message);
            }
        }
    }
}
