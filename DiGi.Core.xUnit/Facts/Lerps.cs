using System.Collections.Generic;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests Lerps end-point behaviour, verifying the list includes both anchors, a single-element list returns the start color, and null is returned for count below 1.
        /// <para>Colors are compared by ARGB value, because System.Drawing.Color equality on .NET distinguishes named colors from unnamed ones with the same value.</para>
        /// </summary>
        [Fact]
        public void Lerps()
        {
            System.Drawing.Color color_1 = System.Drawing.Color.Red;
            System.Drawing.Color color_2 = System.Drawing.Color.Blue;

            Assert.Null(Core.Query.Lerps(color_1, color_2, 0));

            List<System.Drawing.Color> colors_1 = Core.Query.Lerps(color_1, color_2, 1)!;
            Assert.Single(colors_1);
            Assert.Equal(color_1.ToArgb(), colors_1[0].ToArgb());

            List<System.Drawing.Color> colors_2 = Core.Query.Lerps(color_1, color_2, 2)!;
            Assert.Equal(2, colors_2.Count);
            Assert.Equal(color_1.ToArgb(), colors_2[0].ToArgb());
            Assert.Equal(color_2.ToArgb(), colors_2[1].ToArgb());

            List<System.Drawing.Color> colors_3 = Core.Query.Lerps(color_1, color_2, 3)!;
            Assert.Equal(3, colors_3.Count);
            Assert.Equal(color_1.ToArgb(), colors_3[0].ToArgb());
            Assert.Equal(color_2.ToArgb(), colors_3[2].ToArgb());
            Assert.Equal(Core.Query.Lerp(color_1, color_2, 0.5).ToArgb(), colors_3[1].ToArgb());
        }
    }
}
