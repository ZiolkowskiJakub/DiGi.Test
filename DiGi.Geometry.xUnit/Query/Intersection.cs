using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using System.Reflection;

namespace DiGi.Geometry.xUnit
{
    public partial class Query
    {
        [Fact]
        public void Intersection()
        {
            string? path;

            path = DiGi.Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "Polygonal2D_Intersection_1.json");
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }

            Polygon2D? polygon2D_1 = DiGi.Core.Convert.ToDiGi<Polygon2D>((DiGi.Core.Classes.Path)path)?.FirstOrDefault();
            Assert.NotNull(polygon2D_1);

            path = DiGi.Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "Polygonal2D_Intersection_2.json");
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }

            Polygon2D? polygon2D_2 = DiGi.Core.Convert.ToDiGi<Polygon2D>((DiGi.Core.Classes.Path)path)?.FirstOrDefault();
            Assert.NotNull(polygon2D_2);

            path = DiGi.Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "Polygonal2D_Intersection_3.json");
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }

            Polygon2D? polygon2D_3 = DiGi.Core.Convert.ToDiGi<Polygon2D>((DiGi.Core.Classes.Path)path)?.FirstOrDefault();
            Assert.NotNull(polygon2D_3);


            List<Polygon2D>? polygon2Ds_1 = Planar.Query.Intersection<Polygon2D, Polygon2D>([polygon2D_1, polygon2D_3]);
            Assert.NotNull(polygon2Ds_1);
            Assert.True(polygon2Ds_1.Count == 1);

            List<Polygon2D>? polygon2Ds_2 = Planar.Query.Intersection<Polygon2D, Polygon2D>([polygon2D_2, polygon2D_3]);
            Assert.NotNull(polygon2Ds_2);
            Assert.True(polygon2Ds_2.Count == 1);

            Assert.True(DiGi.Core.Query.AlmostEquals(polygon2Ds_1[0].GetArea() + polygon2Ds_2[0].GetArea(), polygon2D_3.GetArea(), DiGi.Core.Constans.Tolerance.Distance));
        }
    }
}