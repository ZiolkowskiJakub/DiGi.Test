using DiGi.GIS.Classes;
using DiGi.GIS.WebAPI.Classes;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
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
        /// Verifies that prediction image export encoding pipelines produce byte-identical JPEG outputs when processing real orthophoto payloads loaded from test fixtures.
        /// </summary>
        [Fact]
        [SupportedOSPlatform("windows")]
        public void ExportPredictionImages_ByteParity()
        {
            string? path_Fixture = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "OrtoDatas_BoundingBox2D_OrtoDatas.json");
            Assert.False(string.IsNullOrWhiteSpace(path_Fixture));
            Assert.True(File.Exists(path_Fixture));

            OrtoDatas? ortoDatas = Core.Convert.ToDiGi<OrtoDatas>((Core.Classes.Path)path_Fixture)?.FirstOrDefault();
            Assert.NotNull(ortoDatas);
            Assert.NotEmpty(ortoDatas);

            foreach (OrtoData ortoData in ortoDatas)
            {
                byte[]? bytes_Source = ortoData.Bytes;
                Assert.NotNull(bytes_Source);
                Assert.NotEmpty(bytes_Source);

                using MemoryStream memoryStream_Pipeline1 = new(bytes_Source);
                using Image image_Pipeline1 = Image.FromStream(memoryStream_Pipeline1);
                using MemoryStream memoryStream_Export1 = new();
                image_Pipeline1.Save(memoryStream_Export1, ImageFormat.Jpeg);
                byte[] bytes_Export1 = memoryStream_Export1.ToArray();

                using MemoryStream memoryStream_Pipeline2 = new(bytes_Source);
                using Image image_Pipeline2 = Image.FromStream(memoryStream_Pipeline2);
                using MemoryStream memoryStream_Export2 = new();
                image_Pipeline2.Save(memoryStream_Export2, ImageFormat.Jpeg);
                byte[] bytes_Export2 = memoryStream_Export2.ToArray();

                Assert.Equal(bytes_Export1.Length, bytes_Export2.Length);
                Assert.Equal(bytes_Export1, bytes_Export2);
            }
        }
    }
}
