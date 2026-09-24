using System.Runtime.Versioning;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="ComputeSharp.Query.IsSupported(global::ComputeSharp.GraphicsDevice?)"/> rejects the WARP software device, which the ComputeSharp default device falls back to on a machine without a hardware adapter.
        /// <para>Differential: WARP passes the double-precision check that alone used to gate the device selection, so the fact first asserts that check admits it and then that the full gate does not (ZiolkowskiJakub/DiGi.Solar#10).
        /// A hardware device with double precision, when present, must still be accepted, and a null device is rejected.</para>
        /// </summary>
        [Fact]
        [SupportedOSPlatform("windows")]
        public void Query_IsSupported()
        {
            Assert.False(ComputeSharp.Query.IsSupported(null));

            global::ComputeSharp.GraphicsDevice? graphicsDevice_WARP = global::ComputeSharp.GraphicsDevice.QueryDevices(x => !x.IsHardwareAccelerated).FirstOrDefault();
            if (graphicsDevice_WARP is null)
            {
                testOutputHelper.WriteLine("No WARP device is available on this machine.");
            }
            else
            {
                testOutputHelper.WriteLine($"WARP device: {graphicsDevice_WARP.Name}, double precision {graphicsDevice_WARP.IsDoublePrecisionSupportAvailable()}.");

                Assert.True(graphicsDevice_WARP.IsDoublePrecisionSupportAvailable());
                Assert.False(ComputeSharp.Query.IsSupported(graphicsDevice_WARP));
            }

            global::ComputeSharp.GraphicsDevice? graphicsDevice_Hardware = global::ComputeSharp.GraphicsDevice.QueryDevices(x => x.IsHardwareAccelerated).FirstOrDefault(x => x.IsDoublePrecisionSupportAvailable());
            if (graphicsDevice_Hardware is null)
            {
                testOutputHelper.WriteLine("No hardware device with double precision is available on this machine.");
                return;
            }

            Assert.True(ComputeSharp.Query.IsSupported(graphicsDevice_Hardware));
        }
    }
}
