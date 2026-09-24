using ComputeSharp;
using Xunit.Abstractions;

namespace DiGi.ComputeSharp.xUnit
{
    public static partial class Query
    {
        /// <summary>
        /// Determines whether the DiGi.ComputeSharp shaders can run on the current machine: the ComputeSharp default device must be hardware-accelerated and support double precision
        /// (<see cref="Core.Create.GraphicsDevice"/>). Writes a warning to the test output helper if not supported.
        /// <para>The default device is shared by ComputeSharp and is never disposed here: disposing it would invalidate the device held by every other fact running in parallel (ZiolkowskiJakub/DiGi.ComputeSharp#2).</para>
        /// </summary>
        /// <param name="testOutputHelper">The test output helper to write warning messages to.</param>
        /// <returns>True if ComputeSharp is supported; otherwise, false.</returns>
        public static bool IsComputeSharpSupported(ITestOutputHelper testOutputHelper)
        {
            try
            {
                if (Core.Create.GraphicsDevice() != null)
                {
                    return true;
                }

                GraphicsDevice graphicsDevice = GraphicsDevice.GetDefault();
                if (!graphicsDevice.IsHardwareAccelerated)
                {
                    testOutputHelper.WriteLine("WARNING: ComputeSharp is not supported on this machine (default graphics device '" + graphicsDevice.Name + "' is not hardware-accelerated).");
                }
                else
                {
                    testOutputHelper.WriteLine("WARNING: ComputeSharp is not supported on this machine (graphics device does not support double precision operations).");
                }

                return false;
            }
            catch (Exception exception)
            {
                testOutputHelper.WriteLine("WARNING: ComputeSharp is not supported on this machine: " + exception.Message);
                return false;
            }
        }
    }
}
