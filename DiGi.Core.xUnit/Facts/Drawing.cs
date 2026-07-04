using DiGi.Core.Drawing;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Color = DiGi.Core.Classes.Color;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        [Fact]
        public void Pen_PropertiesAndCloning()
        {
            Color color = new(255, 255, 0, 0); // Red
            DiGi.Core.Drawing.Classes.Pen pen = new(color, 2.5);

            Assert.NotNull(pen.Color);
            Assert.Equal(255, pen.Color.Red);
            Assert.Equal(2.5, pen.Thickness);

            // Test cloning
            DiGi.Core.Drawing.Classes.Pen? clonedPen = (DiGi.Core.Drawing.Classes.Pen?)pen.Clone();
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
            using Bitmap bmp1 = new(10, 10);
            using Bitmap bmp2 = new(10, 10);

            // Set a pixel on both
            bmp1.SetPixel(5, 5, System.Drawing.Color.Blue);
            bmp2.SetPixel(5, 5, System.Drawing.Color.Blue);

            // Compare identical using extension method syntax
            Assert.True(bmp1.CompareByPixels(bmp2));

            // Modify one pixel in bmp2
            bmp2.SetPixel(5, 5, System.Drawing.Color.Red);
            Assert.False(bmp1.CompareByPixels(bmp2));

            // Create a bitmap of a different size
            using Bitmap bmpDifferentSize = new(10, 11);
            Assert.False(bmp1.CompareByPixels(bmpDifferentSize));

            // Compare with nulls by fully qualifying the static query method
            Assert.True(DiGi.Core.Drawing.Query.CompareByPixels(null, null));
            Assert.False(bmp1.CompareByPixels(null));
        }

        /// <summary>
        /// Tests that <see cref="DiGi.Core.Drawing.Query.CompareByPixels(Image, Image)"/> correctly normalizes
        /// bitmaps that originate from different native pixel formats (24bpp opaque RGB vs 32bpp ARGB) before
        /// comparing them, treating visually identical opaque colors as equal regardless of source format.
        /// </summary>
        [Fact]
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        public void CompareByPixels_DifferentSourceFormats_StillEqual()
        {
            using Bitmap bitmap24bpp = new(8, 8, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (Graphics graphics = Graphics.FromImage(bitmap24bpp))
            {
                graphics.Clear(System.Drawing.Color.Green);
            }

            using Bitmap bitmap32bpp = new(8, 8, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bitmap32bpp))
            {
                graphics.Clear(System.Drawing.Color.Green);
            }

            Assert.True(bitmap24bpp.CompareByPixels(bitmap32bpp));

            using (Graphics graphics = Graphics.FromImage(bitmap32bpp))
            {
                graphics.Clear(System.Drawing.Color.Red);
            }

            Assert.False(bitmap24bpp.CompareByPixels(bitmap32bpp));
        }

        /// <summary>
        /// Tests that two fully-distinct bitmaps (every pixel different) are correctly reported as not equal,
        /// and that two larger identical bitmaps complete quickly, guarding against a regression back to a
        /// per-pixel GetPixel comparison.
        /// </summary>
        [Fact]
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        public void CompareByPixels_LargeBitmaps_PerformanceAndCorrectness()
        {
            int size = 200;

            // Warm up / JIT compile before measuring performance.
            {
                using Bitmap bitmap_Warmup1 = new(2, 2);
                using Bitmap bitmap_Warmup2 = new(2, 2);
                _ = bitmap_Warmup1.CompareByPixels(bitmap_Warmup2);
            }

            using Bitmap bitmap_1 = new(size, size);
            using (Graphics graphics = Graphics.FromImage(bitmap_1))
            {
                graphics.Clear(System.Drawing.Color.CornflowerBlue);
            }

            using Bitmap bitmap_2 = new(size, size);
            using (Graphics graphics = Graphics.FromImage(bitmap_2))
            {
                graphics.Clear(System.Drawing.Color.CornflowerBlue);
            }

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            bool result_Identical = bitmap_1.CompareByPixels(bitmap_2);
            stopwatch.Stop();

            Assert.True(result_Identical);
            Assert.True(stopwatch.ElapsedMilliseconds < 50, $"CompareByPixels took {stopwatch.ElapsedMilliseconds} ms for a {size}x{size} bitmap.");

            using Bitmap bitmap_3 = new(size, size);
            using (Graphics graphics = Graphics.FromImage(bitmap_3))
            {
                graphics.Clear(System.Drawing.Color.Crimson);
            }

            Assert.False(bitmap_1.CompareByPixels(bitmap_3));
        }

        /// <summary>
        /// Tests that bitmaps using an indexed pixel format (palette-based, where each pixel stores a palette
        /// index rather than a direct color value) are still compared correctly. This is the specific scenario
        /// the Format32bppArgb normalization step was introduced to handle - without it, two indexed bitmaps
        /// with identical visible colors but different raw palette indices would be incorrectly reported as
        /// different.
        /// </summary>
        [Fact]
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        public void CompareByPixels_IndexedFormat_NormalizesCorrectly()
        {
            Rectangle rectangle = new(0, 0, 8, 8);

            using Bitmap bitmapSource_1 = new(8, 8);
            using (Graphics graphics = Graphics.FromImage(bitmapSource_1))
            {
                graphics.Clear(System.Drawing.Color.Red);
            }

            using Bitmap bitmapSource_2 = new(8, 8);
            using (Graphics graphics = Graphics.FromImage(bitmapSource_2))
            {
                graphics.Clear(System.Drawing.Color.Red);
            }

            using Bitmap bitmapIndexed_1 = bitmapSource_1.Clone(rectangle, PixelFormat.Format8bppIndexed);
            using Bitmap bitmapIndexed_2 = bitmapSource_2.Clone(rectangle, PixelFormat.Format8bppIndexed);

            Assert.True(bitmapIndexed_1.CompareByPixels(bitmapIndexed_2));

            using Bitmap bitmapSource_3 = new(8, 8);
            using (Graphics graphics = Graphics.FromImage(bitmapSource_3))
            {
                graphics.Clear(System.Drawing.Color.Blue);
            }

            using Bitmap bitmapIndexed_3 = bitmapSource_3.Clone(rectangle, PixelFormat.Format8bppIndexed);

            Assert.False(bitmapIndexed_1.CompareByPixels(bitmapIndexed_3));
        }

        /// <summary>
        /// Tests that two pixels with identical RGB but a single-unit difference in the alpha channel are
        /// correctly reported as not equal, confirming the byte-level comparison is sensitive to alpha and not
        /// just the visible RGB channels.
        /// </summary>
        [Fact]
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        public void CompareByPixels_AlphaOnlyDifference_ReturnsFalse()
        {
            using Bitmap bitmap_1 = new(4, 4, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bitmap_1))
            {
                graphics.Clear(System.Drawing.Color.FromArgb(200, 10, 20, 30));
            }

            using Bitmap bitmap_2 = new(4, 4, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bitmap_2))
            {
                graphics.Clear(System.Drawing.Color.FromArgb(200, 10, 20, 30));
            }

            Assert.True(bitmap_1.CompareByPixels(bitmap_2));

            using (Graphics graphics = Graphics.FromImage(bitmap_2))
            {
                graphics.Clear(System.Drawing.Color.FromArgb(201, 10, 20, 30));
            }

            Assert.False(bitmap_1.CompareByPixels(bitmap_2));
        }

        /// <summary>
        /// Tests that two Image instances loaded through the real image-decoding pipeline (encoded to PNG bytes
        /// and reloaded via Image.FromStream, rather than constructed directly as in-memory Bitmaps) are still
        /// compared correctly, exercising the initial `new Bitmap(image)` conversion step with a realistic input.
        /// </summary>
        [Fact]
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        public void CompareByPixels_ImageLoadedFromStream_ComparesCorrectly()
        {
            using Bitmap bitmapSource_1 = new(6, 6);
            using (Graphics graphics = Graphics.FromImage(bitmapSource_1))
            {
                graphics.Clear(System.Drawing.Color.Orange);
            }

            using Bitmap bitmapSource_2 = new(6, 6);
            using (Graphics graphics = Graphics.FromImage(bitmapSource_2))
            {
                graphics.Clear(System.Drawing.Color.Orange);
            }

            using MemoryStream memoryStream_1 = new();
            bitmapSource_1.Save(memoryStream_1, ImageFormat.Png);
            memoryStream_1.Position = 0;

            using MemoryStream memoryStream_2 = new();
            bitmapSource_2.Save(memoryStream_2, ImageFormat.Png);
            memoryStream_2.Position = 0;

            using Image image_1 = Image.FromStream(memoryStream_1);
            using Image image_2 = Image.FromStream(memoryStream_2);

            Assert.True(image_1.CompareByPixels(image_2));

            using Bitmap bitmapSource_3 = new(6, 6);
            using (Graphics graphics = Graphics.FromImage(bitmapSource_3))
            {
                graphics.Clear(System.Drawing.Color.Purple);
            }

            using MemoryStream memoryStream_3 = new();
            bitmapSource_3.Save(memoryStream_3, ImageFormat.Png);
            memoryStream_3.Position = 0;

            using Image image_3 = Image.FromStream(memoryStream_3);

            Assert.False(image_1.CompareByPixels(image_3));
        }

        /// <summary>
        /// Tests that an odd, non-multiple-of-4 image width is handled correctly, guarding against any
        /// off-by-one or row-stride miscalculation in the per-row byte comparison.
        /// </summary>
        [Fact]
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        public void CompareByPixels_OddWidth_ComparesCorrectly()
        {
            using Bitmap bitmap_1 = new(7, 5);
            using (Graphics graphics = Graphics.FromImage(bitmap_1))
            {
                graphics.Clear(System.Drawing.Color.Teal);
            }

            using Bitmap bitmap_2 = new(7, 5);
            using (Graphics graphics = Graphics.FromImage(bitmap_2))
            {
                graphics.Clear(System.Drawing.Color.Teal);
            }

            Assert.True(bitmap_1.CompareByPixels(bitmap_2));

            bitmap_2.SetPixel(6, 4, System.Drawing.Color.Black);

            Assert.False(bitmap_1.CompareByPixels(bitmap_2));
        }

        /// <summary>
        /// Tests that comparison is symmetric: comparing A to B yields the same result as comparing B to A,
        /// for both an equal pair and a differing pair.
        /// </summary>
        [Fact]
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        public void CompareByPixels_IsSymmetric()
        {
            using Bitmap bitmap_1 = new(5, 5);
            using (Graphics graphics = Graphics.FromImage(bitmap_1))
            {
                graphics.Clear(System.Drawing.Color.Yellow);
            }

            using Bitmap bitmap_2 = new(5, 5);
            using (Graphics graphics = Graphics.FromImage(bitmap_2))
            {
                graphics.Clear(System.Drawing.Color.Yellow);
            }

            Assert.Equal(bitmap_1.CompareByPixels(bitmap_2), bitmap_2.CompareByPixels(bitmap_1));
            Assert.True(bitmap_1.CompareByPixels(bitmap_2));

            using Bitmap bitmap_3 = new(5, 5);
            using (Graphics graphics = Graphics.FromImage(bitmap_3))
            {
                graphics.Clear(System.Drawing.Color.Magenta);
            }

            Assert.Equal(bitmap_1.CompareByPixels(bitmap_3), bitmap_3.CompareByPixels(bitmap_1));
            Assert.False(bitmap_1.CompareByPixels(bitmap_3));
        }

        /// <summary>
        /// Tests that repeated calls do not leak native GDI+ resources (Bitmap/Graphics/BitmapData handles).
        /// Each comparison allocates two normalized bitmaps and locks/unlocks their pixel buffers; a mistake in
        /// the disposal logic (e.g. a missing UnlockBits or Bitmap.Dispose) would surface as a handle leak or
        /// out-of-memory failure under repeated iterations rather than on a single call.
        /// </summary>
        [Fact]
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        public void CompareByPixels_RepeatedCalls_DoNotLeakResources()
        {
            using Bitmap bitmap_1 = new(16, 16);
            using (Graphics graphics = Graphics.FromImage(bitmap_1))
            {
                graphics.Clear(System.Drawing.Color.SeaGreen);
            }

            using Bitmap bitmap_2 = new(16, 16);
            using (Graphics graphics = Graphics.FromImage(bitmap_2))
            {
                graphics.Clear(System.Drawing.Color.SeaGreen);
            }

            for (int i = 0; i < 1000; i++)
            {
                Assert.True(bitmap_1.CompareByPixels(bitmap_2));
            }
        }
    }
}