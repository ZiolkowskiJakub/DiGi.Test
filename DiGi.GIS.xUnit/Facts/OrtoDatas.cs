using DiGi.GIS.Classes;
using System.Linq;
using System.Reflection;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the loading and processing of ortho-data and building data from JSON files, verifying that bounding boxes can be calculated for buildings and sizes can be retrieved for ortho-data entries.
        /// </summary>
        [Fact]
        public void OrtoDatas()
        {
            string? path;

            path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "OrtoDatas_BoundingBox2D_Building2D.json");
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            {
                return;
            }

            Building2D? building2D = Core.Convert.ToDiGi<Building2D>((Core.Classes.Path)path)?.FirstOrDefault();
            Assert.NotNull(building2D);

            path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "OrtoDatas_BoundingBox2D_OrtoDatas.json");
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            {
                return;
            }

            OrtoDatas? ortoDatas = Core.Convert.ToDiGi<OrtoDatas>((Core.Classes.Path)path)?.FirstOrDefault();
            Assert.NotNull(ortoDatas);

            Geometry.Planar.Classes.BoundingBox2D? boundingBox2D = building2D.PolygonalFace2D?.GetBoundingBox();
            Assert.NotNull(boundingBox2D);

            Core.Classes.Size size_Image = new(320, 320);

            foreach (OrtoData ortoData in ortoDatas)
            {
                Assert.NotNull(ortoData);

                Core.Classes.Size? size_Local = ortoData.GetSize(Enums.GeometryContext.Local);

                Assert.NotNull(size_Local);

                Assert.Equal(size_Local.Width, size_Image.Width);
                Assert.Equal(size_Local.Height, size_Image.Height);

                Core.Classes.Size? size_Global = ortoData.GetSize(Enums.GeometryContext.Global);

                Geometry.Planar.Classes.BoundingBox2D? boundingBox2D_Global = ortoData.GetBoundingBox(Enums.GeometryContext.Global);
                Assert.NotNull(boundingBox2D_Global);

                Assert.True(boundingBox2D_Global.Inside(boundingBox2D));
            }
        }
    }
}