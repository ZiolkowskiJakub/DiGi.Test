using DiGi.ComputeSharp.Core.Classes;
using DiGi.ComputeSharp.Core.Constants;
using DiGi.ComputeSharp.Spatial.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Contains unit tests for creating 3D triangle intersections.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Tests the intersection of two 3D triangles and checks the intersection results.
        /// Handles UnsupportedDoubleOperationException gracefully for FP64 unsupported GPUs.
        /// </summary>
        [Fact]
        public void Triangle3Intersection()
        {
            if (!Query.IsComputeSharpSupported(testOutputHelper))
            {
                return;
            }

            Triangle3 triangle3_Case1_1 = new(new(true), 2, 2, 0, 2, 8, 0, 8, 2, 0);
            Triangle3 triangle3_Case1_2 = new(new(true), 4, 4, 0, 4, 9, 0, 9, 4, 0);

            Triangle3 triangle3_Case2_1 = new(new(true), 2, 2, 0, 2, 8, 0, 8, 2, 0);
            Triangle3 triangle3_Case2_2 = new(new(true), 5, 5, 0, 4, 9, 0, 9, 4, 0);

            Triangle3 triangle3_Case3_1 = new(new(true), 1, 1, 0, 1, 6, 0, 6, 1, 0);
            Triangle3 triangle3_Case3_2 = new(new(true), 0, 5, 0, 5, 0, 0, 5, 5, 0);

            Triangle3 triangle3_Case4_1 = new(new(true), 0, 0, 0, 5, 5, 0, 5, 0, 0);
            Triangle3 triangle3_Case4_2 = new(new(true), 0, 0, 1, 10, 10, 1, 10, 0, 1);
            Coordinate3 coordinate3_Case4 = new(0, 0, 1);

            try
            {
                Triangle3Intersection triangle3Intersection_1 = Spatial.Create.Triangle3Intersection(triangle3_Case1_1, triangle3_Case1_2, Tolerance.Distance);
                Assert.False(triangle3Intersection_1.IsNaN());

                Triangle3Intersection triangle3Intersection_2 = Spatial.Create.Triangle3Intersection(triangle3_Case2_1, triangle3_Case2_2, Tolerance.Distance);
                Assert.False(triangle3Intersection_2.IsNaN());

                Triangle3Intersection triangle3Intersection_3 = Spatial.Create.Triangle3Intersection(triangle3_Case3_1, triangle3_Case3_2, Tolerance.Distance);
                Assert.False(triangle3Intersection_3.IsNaN());

                Triangle3Intersection triangle3Intersection_4 = Spatial.Create.Triangle3Intersection(triangle3_Case4_2, triangle3_Case4_1, coordinate3_Case4, true, false, Tolerance.Distance);
                Assert.False(triangle3Intersection_4.IsNaN());
            }
            catch (Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
            {
                testOutputHelper.WriteLine("WARNING: GPU FP64 double-precision operations are not supported on this graphics card: " + exception.Message);
            }
        }
    }
}
