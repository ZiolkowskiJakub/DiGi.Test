using DiGi.Core.Classes;
using DiGi.Solar.Classes;
using DiGi.Solar.ComputeSharp.Classes;
using System.Runtime.Versioning;
using Xunit.Abstractions;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        private readonly ITestOutputHelper testOutputHelper;

        /// <summary>
        /// Initializes a new instance of the Facts class.
        /// </summary>
        /// <param name="testOutputHelper">The test output helper for logging.</param>
        public Facts(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Tests the instantiation and execution of the ShadingSolver, running the GPU-based solver if ComputeSharp is supported on the current system.
        /// </summary>
        [Fact]
        [SupportedOSPlatform("windows")]
        public void ShadingSolver_Solve()
        {
            Coordinates coordinates = new(50.0, 20.0);
            ShadingModel shadingModel = new(Core.Enums.UTC.Plus0100, coordinates);

            DateTime[] dateTimes = [new DateTime(2026, 6, 26, 12, 0, 0)];
            ShadingSolver shadingSolver = new(shadingModel, dateTimes);

            Assert.NotNull(shadingSolver);
            Assert.Equal(shadingModel, shadingSolver.ShadingModel);

            if (IsComputeSharpSupported(testOutputHelper))
            {
                bool isSolved = shadingSolver.Solve();
                Assert.True(isSolved);
            }
            else
            {
                testOutputHelper.WriteLine("Skipping GPU ShadingSolver.Solve test because ComputeSharp is not supported on this machine.");
            }
        }

        /// <summary>
        /// Determines whether ComputeSharp is supported on the current machine and graphics device.
        /// </summary>
        /// <param name="testOutputHelper">The test output helper to write warning messages to.</param>
        /// <returns>True if ComputeSharp is supported; otherwise, false.</returns>
        [SupportedOSPlatform("windows")]
        private static bool IsComputeSharpSupported(ITestOutputHelper testOutputHelper)
        {
            try
            {
                using global::ComputeSharp.GraphicsDevice graphicsDevice = global::ComputeSharp.GraphicsDevice.GetDefault();
                if (graphicsDevice == null)
                {
                    testOutputHelper.WriteLine("WARNING: ComputeSharp is not supported on this machine (no default GraphicsDevice found).");
                    return false;
                }
                if (!graphicsDevice.IsDoublePrecisionSupportAvailable())
                {
                    testOutputHelper.WriteLine("WARNING: ComputeSharp is not supported on this machine (graphics device does not support double precision operations).");
                    return false;
                }
                return true;
            }
            catch (Exception exception)
            {
                testOutputHelper.WriteLine("WARNING: ComputeSharp is not supported on this machine: " + exception.Message);
                return false;
            }
        }
    }
}