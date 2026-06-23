using DiGi.Core.Drawing;
using System.Drawing;
using Color = DiGi.Core.Classes.Color;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        [Fact]
        public void Pen_PropertiesAndCloning()
        {
            var color = new Color(255, 255, 0, 0); // Red
            var pen = new DiGi.Core.Drawing.Classes.Pen(color, 2.5);

            Assert.NotNull(pen.Color);
            Assert.Equal(255, pen.Color.Red);
            Assert.Equal(2.5, pen.Thickness);

            // Test cloning
            var clonedPen = (DiGi.Core.Drawing.Classes.Pen?)pen.Clone();
            Assert.NotNull(clonedPen);
            Assert.Equal(pen.Thickness, clonedPen.Thickness);
            Assert.NotNull(clonedPen.Color);
            Assert.Equal(pen.Color.Red, clonedPen.Color.Red);
        }

        /// <summary>
        /// Tests the CompareByPixels extension method with various Bitmap scenarios on Windows.
        /// </summary>
        [Fact]
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        public void CompareByPixels_ShouldEvaluateBitmaps()
        {
            // Create two identical bitmaps
            using Bitmap bmp1 = new Bitmap(10, 10);
            using Bitmap bmp2 = new Bitmap(10, 10);

            // Set a pixel on both
            bmp1.SetPixel(5, 5, System.Drawing.Color.Blue);
            bmp2.SetPixel(5, 5, System.Drawing.Color.Blue);

            // Compare identical using extension method syntax
            Assert.True(bmp1.CompareByPixels(bmp2));

            // Modify one pixel in bmp2
            bmp2.SetPixel(5, 5, System.Drawing.Color.Red);
            Assert.False(bmp1.CompareByPixels(bmp2));

            // Create a bitmap of a different size
            using Bitmap bmpDifferentSize = new Bitmap(10, 11);
            Assert.False(bmp1.CompareByPixels(bmpDifferentSize));

            // Compare with nulls by fully qualifying the static query method
            Assert.True(DiGi.Core.Drawing.Query.CompareByPixels(null, null));
            Assert.False(bmp1.CompareByPixels(null));
        }
    }
}