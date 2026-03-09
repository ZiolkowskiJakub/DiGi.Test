using DiGi.Geometry.Planar.Classes;
using System.Reflection;

namespace DiGi.Geometry.xUnit
{
    public partial class Query
    {
        [Fact]
        public void InRange()
        {
            string? path;

            path = DiGi.Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "Polygonal2D_InRange_1.json");
            if(string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }

            Polygon2D? polygon2D_1 = DiGi.Core.Convert.ToDiGi<Polygon2D>((DiGi.Core.Classes.Path)path)?.FirstOrDefault();
            Assert.NotNull(polygon2D_1);

            path = DiGi.Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "Polygonal2D_InRange_2.json");
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }

            Polygon2D? polygon2D_2 = DiGi.Core.Convert.ToDiGi<Polygon2D>((DiGi.Core.Classes.Path)path)?.FirstOrDefault();
            Assert.NotNull(polygon2D_2);

            path = DiGi.Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "Polygonal2D_InRange_3.json");
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }

            Polygon2D? polygon2D_3 = DiGi.Core.Convert.ToDiGi<Polygon2D>((DiGi.Core.Classes.Path)path)?.FirstOrDefault();
            Assert.NotNull(polygon2D_3);

            bool inRange_1 = polygon2D_3.InRange(polygon2D_1);

            bool inRange_2 = polygon2D_3.InRange(polygon2D_2);

            Assert.True(inRange_1 != inRange_2);
        }
    }
}