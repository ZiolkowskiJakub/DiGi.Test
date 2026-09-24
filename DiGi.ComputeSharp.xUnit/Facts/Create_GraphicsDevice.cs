using ComputeSharp;

namespace DiGi.ComputeSharp.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Core.Create.GraphicsDevice"/> returns either null or a supported (hardware-accelerated, double-precision) device,
        /// and that repeated calls return the same shared ComputeSharp default device.
        /// </summary>
        [Fact]
        public void Create_GraphicsDevice()
        {
            GraphicsDevice? graphicsDevice = Core.Create.GraphicsDevice();
            if (graphicsDevice == null)
            {
                testOutputHelper.WriteLine("WARNING: no supported graphics device on this machine.");
                return;
            }

            Assert.True(graphicsDevice.IsHardwareAccelerated);
            Assert.True(graphicsDevice.IsDoublePrecisionSupportAvailable());
            Assert.True(Core.Query.IsSupported(graphicsDevice));

            Assert.Same(graphicsDevice, Core.Create.GraphicsDevice());
            Assert.Same(graphicsDevice, GraphicsDevice.GetDefault());
        }
    }
}
