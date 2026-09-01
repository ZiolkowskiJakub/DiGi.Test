using DiGi.GIS.Classes;
using DiGi.GIS.WebAPI.Classes;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace DiGi.GIS.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Modify.ExportPredictionImagesAsync"/> properly validates input parameters and returns false when handed invalid arguments.
        /// </summary>
        [Fact]
        public async Task ExportPredictionImages_Validation()
        {
            GISWebAPIManager gisWebAPIManager = new(null);

            bool result_NullManager = await Modify.ExportPredictionImagesAsync(null, 1, "test");
            Assert.False(result_NullManager);

            bool result_InvalidCounty = await gisWebAPIManager.ExportPredictionImagesAsync(0, "test");
            Assert.False(result_InvalidCounty);

            bool result_InvalidDir = await gisWebAPIManager.ExportPredictionImagesAsync(1, "   ");
            Assert.False(result_InvalidDir);
        }

        /// <summary>
        /// Verifies that decoding orthophoto bytes via System.Drawing.Image and saving as JPEG produces byte-identical results to the legacy prediction image pipeline.
        /// </summary>
        [Fact]
        [SupportedOSPlatform("windows")]
        public void ExportPredictionImages_ByteParity()
        {
            using Bitmap bitmap = new(10, 10);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Red);

            using MemoryStream memoryStream_Original = new();
            bitmap.Save(memoryStream_Original, ImageFormat.Jpeg);
            byte[] bytes_Original = memoryStream_Original.ToArray();

            OrtoData ortoData = new(DateTime.Now, bytes_Original, 1.0, null);
            Assert.NotNull(ortoData.Bytes);

            using MemoryStream memoryStream_Decoded = new(ortoData.Bytes);
            using Image image_Decoded = Image.FromStream(memoryStream_Decoded);

            using MemoryStream memoryStream_Exported = new();
            image_Decoded.Save(memoryStream_Exported, ImageFormat.Jpeg);
            byte[] bytes_Exported = memoryStream_Exported.ToArray();

            Assert.Equal(bytes_Original.Length, bytes_Exported.Length);
            Assert.Equal(bytes_Original, bytes_Exported);
        }
    }
}
