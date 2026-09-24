using ComputeSharp;

namespace DiGi.ComputeSharp.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Core.Query.IsSupported(GraphicsDevice?)"/> rejects the WARP software device although it reports double precision as available,
        /// and rejects a null device.
        /// <para>Differential: WARP passes <see cref="GraphicsDevice.IsDoublePrecisionSupportAvailable"/>, so only the hardware-acceleration term can reject it.</para>
        /// </summary>
        [Fact]
        public void Query_IsSupported()
        {
            Assert.False(Core.Query.IsSupported(null));

            GraphicsDevice? graphicsDevice_Warp;
            try
            {
                graphicsDevice_Warp = GraphicsDevice.QueryDevices(x => !x.IsHardwareAccelerated).FirstOrDefault();
            }
            catch (Exception exception)
            {
                testOutputHelper.WriteLine("WARNING: graphics devices cannot be enumerated on this machine: " + exception.Message);
                return;
            }

            if (graphicsDevice_Warp == null)
            {
                testOutputHelper.WriteLine("WARNING: no software (WARP) graphics device on this machine.");
                return;
            }

            Assert.False(graphicsDevice_Warp.IsHardwareAccelerated);
            Assert.True(graphicsDevice_Warp.IsDoublePrecisionSupportAvailable());
            Assert.False(Core.Query.IsSupported(graphicsDevice_Warp));
        }
    }
}
