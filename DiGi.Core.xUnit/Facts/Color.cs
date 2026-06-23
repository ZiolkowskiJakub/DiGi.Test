using System.Linq;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the functionality of the Color class, verifying the conversion between System.Drawing.Color and string representations, as well as ensuring that ARGB values are preserved during these conversions and validating serialization.
        /// </summary>
        [Fact]
        public void Color()
        {
            System.Drawing.Color drawingColor_1 = System.Drawing.Color.Aqua;

            Core.Classes.Color color_1 = new(drawingColor_1);

            string? string_1 = color_1.ToSystem_String();

            Assert.NotNull(string_1);

            Core.Classes.Color? color_2 = Convert.ToDiGi<Core.Classes.Color>(string_1)?.FirstOrDefault();

            Assert.NotNull(color_2);

            string? string_2 = color_2.ToSystem_String();

            Assert.NotNull(string_2);

            Assert.Equal(string_1, string_2);

            Core.Classes.Color? color_3 = Convert.ToDiGi<Core.Classes.Color>(string_2)?.FirstOrDefault();

            Assert.Equal(color_3.ToSystem_String(), string_2);

            System.Drawing.Color drawingColor_2 = color_3.ToDrawing();

            Assert.Equal(drawingColor_1.A, drawingColor_2.A);
            Assert.Equal(drawingColor_1.R, drawingColor_2.R);
            Assert.Equal(drawingColor_1.G, drawingColor_2.G);
            Assert.Equal(drawingColor_1.B, drawingColor_2.B);

            Query.SerializationCheck(color_1);
        }

        /// <summary>
        /// Tests the Convert.ToDrawing query methods for string inputs, verifying support for named colors, shorthand CSS hex, standard hex, and ensuring exception-safety for invalid strings.
        /// </summary>
        [Fact]
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        public void ToDrawing_Conversion()
        {
            // Test named color (Verifying the fix for Bug 3)
            System.Drawing.Color color_Named = Convert.ToDrawing("Red");
            Assert.Equal(System.Drawing.Color.Red.ToArgb(), color_Named.ToArgb());

            // Test shorthand CSS hex
            System.Drawing.Color color_Shorthand = Convert.ToDrawing("#F00");
            Assert.Equal(System.Drawing.Color.Red.ToArgb(), color_Shorthand.ToArgb());

            // Test standard hex
            System.Drawing.Color color_Standard = Convert.ToDrawing("#FF0000");
            Assert.Equal(System.Drawing.Color.Red.ToArgb(), color_Standard.ToArgb());

            // Test hex with alpha (RGBA format)
            System.Drawing.Color color_Alpha = Convert.ToDrawing("#FF0000FF");
            Assert.Equal(System.Drawing.Color.Red.ToArgb(), color_Alpha.ToArgb());

            // Test invalid string (Verifying exception safety and fallback to Color.Empty)
            System.Drawing.Color color_Invalid = Convert.ToDrawing("NotAColorName");
            Assert.Equal(System.Drawing.Color.Empty.ToArgb(), color_Invalid.ToArgb());

            // Test null string
            System.Drawing.Color color_Null = Convert.ToDrawing((string?)null);
            Assert.Equal(System.Drawing.Color.Empty.ToArgb(), color_Null.ToArgb());
        }
    }
}