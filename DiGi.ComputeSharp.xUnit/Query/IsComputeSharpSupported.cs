using ComputeSharp;
using Xunit.Abstractions;

namespace DiGi.ComputeSharp.xUnit
{
    public static partial class Query
    {
        /// <summary>
        /// Determines whether ComputeSharp is supported on the current machine and graphics device.
        /// Writes a warning to the test output helper if not supported.
        /// </summary>
        /// <param name="testOutputHelper">The test output helper to write warning messages to.</param>
        /// <returns>True if ComputeSharp is supported; otherwise, false.</returns>
        public static bool IsComputeSharpSupported(ITestOutputHelper testOutputHelper)
        {
            try
            {
                using GraphicsDevice graphicsDevice = GraphicsDevice.GetDefault();
                if (graphicsDevice == null)
                {
                    testOutputHelper.WriteLine("WARNING: ComputeSharp is not supported on this machine (no default GraphicsDevice found).");
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