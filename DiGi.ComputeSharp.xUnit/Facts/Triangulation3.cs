using DiGi.ComputeSharp.Core.Classes;
using DiGi.ComputeSharp.Core.Constants;
using DiGi.ComputeSharp.Spatial.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Contains unit tests for creating 3D triangulations.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Tests the triangulation of a 3D triangle intersected by a 3D line.
        /// Handles UnsupportedDoubleOperationException gracefully for FP64 unsupported GPUs.
        /// </summary>
        [Fact]
        public void Triangulation3()
        {
            if (!Query.IsComputeSharpSupported(testOutputHelper))
            {
                return;
            }

            Triangle3 triangle3_Base = new(new(true), 0, 0, 0, 0, 10, 0, 10, 0, 0);
            Line3 line3_Splitter = new(4, 4, 0, 11, 11, 0);

            try
            {
                Triangulation3 triangulation3_Result = Spatial.Create.Triangulation3(triangle3_Base, line3_Splitter, Tolerance.Distance);

                Assert.False(triangulation3_Result.IsNaN());
                Assert.False(triangulation3_Result.triangle_1.IsNaN());
            }
            catch (Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
            {
                testOutputHelper.WriteLine("WARNING: GPU FP64 double-precision operations are not supported on this graphics card: " + exception.Message);
            }
        }
    }
}
