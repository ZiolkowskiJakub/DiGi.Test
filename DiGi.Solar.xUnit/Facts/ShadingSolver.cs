using DiGi.Core.Classes;
using DiGi.Solar.Classes;
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
        /// Tests the instantiation and execution of the ShadingSolver: the CPU solver always, the ComputeSharp (GPU) solver if ComputeSharp is supported on the current system.
        /// </summary>
        /// <param name="computeSharp">True to test the ComputeSharp solver; false to test the CPU solver.</param>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [SupportedOSPlatform("windows")]
        public void ShadingSolver_Solve(bool computeSharp)
        {
            Coordinates coordinates = new(50.0, 20.0);
            ShadingModel shadingModel = new(Core.Enums.UTC.Plus0100, coordinates);

            DateTime[] dateTimes = [new DateTime(2026, 6, 26, 12, 0, 0)];
            ShadingSolver shadingSolver = CreateShadingSolver(shadingModel, dateTimes, computeSharp);

            Assert.NotNull(shadingSolver);
            Assert.Equal(shadingModel, shadingSolver.ShadingModel);
            Assert.Equal(computeSharp, shadingSolver is ComputeSharp.Classes.ShadingSolver);

            if (IsShadingSolverSupported(computeSharp, testOutputHelper))
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
        /// Creates the CPU <see cref="ShadingSolver"/> or the ComputeSharp (GPU) solver derived from it for the given model and date-times.
        /// </summary>
        /// <param name="shadingModel">The shading model to solve.</param>
        /// <param name="dateTimes">The date-times to solve.</param>
        /// <param name="computeSharp">True to create the ComputeSharp solver; false to create the CPU solver.</param>
        /// <returns>The created solver.</returns>
        [SupportedOSPlatform("windows")]
        private static ShadingSolver CreateShadingSolver(ShadingModel shadingModel, DateTime[] dateTimes, bool computeSharp)
        {
            return computeSharp ? new ComputeSharp.Classes.ShadingSolver(shadingModel, dateTimes) : new ShadingSolver(shadingModel, dateTimes);
        }

        /// <summary>
        /// Determines whether the requested solver can run on the current machine: the CPU solver always can, the ComputeSharp solver only where ComputeSharp is supported.
        /// </summary>
        /// <param name="computeSharp">True for the ComputeSharp solver; false for the CPU solver.</param>
        /// <param name="testOutputHelper">The test output helper to write warning messages to.</param>
        /// <returns>True if the solver can run; otherwise, false.</returns>
        [SupportedOSPlatform("windows")]
        private static bool IsShadingSolverSupported(bool computeSharp, ITestOutputHelper testOutputHelper)
        {
            return !computeSharp || IsComputeSharpSupported(testOutputHelper);
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