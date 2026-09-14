namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the OKLab round trip ToOKLab and ToDiGi, verifying every color returns within one byte per channel.
        /// <para>The sample set is the white point, black, mid-grey, all six default ramp anchors, and the 12 curated categorical colors, so the conversion is pinned on the exact values Create.Colors generates.</para>
        /// </summary>
        [Fact]
        public void OKLab()
        {
            string[] hexes = ["ffffff", "000000", "808080", "deebf7", "08306b", "b2182b", "f7f7f7", "2166ac", "1f77b4", "ff7f0e", "2ca02c", "d62728", "9467bd", "8c564b", "e377c2", "7f7f7f", "bcbd22", "17becf", "393b79", "e7ba52"];

            foreach (string hex in hexes)
            {
                Core.Classes.Color color = new(unchecked((int)System.Convert.ToUInt32("ff" + hex, 16)));

                Core.Classes.Color color_RoundTrip = color.ToOKLab().ToDiGi();

                Assert.True(System.Math.Abs(color.Alpha - color_RoundTrip.Alpha) <= 1, $"Alpha for #{hex} is {color.Alpha}, round trip is {color_RoundTrip.Alpha}.");
                Assert.True(System.Math.Abs(color.Red - color_RoundTrip.Red) <= 1, $"Red for #{hex} is {color.Red}, round trip is {color_RoundTrip.Red}.");
                Assert.True(System.Math.Abs(color.Green - color_RoundTrip.Green) <= 1, $"Green for #{hex} is {color.Green}, round trip is {color_RoundTrip.Green}.");
                Assert.True(System.Math.Abs(color.Blue - color_RoundTrip.Blue) <= 1, $"Blue for #{hex} is {color.Blue}, round trip is {color_RoundTrip.Blue}.");
            }
        }
    }
}
