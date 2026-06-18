using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using System.Reflection;

namespace DiGi.Geometry.xUnit
{
    public partial class Query
    {
        /// <summary>
        /// Tests the intersection of two-dimensional polygons by loading data from JSON files and verifying that the resulting intersection set contains exactly one polygon.
        /// </summary>
        [Fact]
        public void Intersection_1()
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

            Assert.True(DiGi.Core.Query.AlmostEquals(polygon2Ds_1[0].GetArea() + polygon2Ds_2[0].GetArea(), polygon2D_3.GetArea(), DiGi.Core.Constants.Tolerance.Distance));
        }

        /// <summary>
        /// Tests the intersection of a <see cref="Rectangle2D"/> and a <see cref="Polygon2D"/> using data loaded from JSON files to verify that the resulting collection contains the expected number of polygons.
        /// </summary>
        [Fact]
        public void Intersection_2()
        {
            string? path;

            path = DiGi.Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "Rectangle2D_Intersection_1.json");
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }

            Rectangle2D? rectangle2D = DiGi.Core.Convert.ToDiGi<Rectangle2D>((DiGi.Core.Classes.Path)path)?.FirstOrDefault();
            Assert.NotNull(rectangle2D);

            path = DiGi.Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "Polygon2D_Intersection_1.json");
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }

            Polygon2D? polygon2D = DiGi.Core.Convert.ToDiGi<Polygon2D>((DiGi.Core.Classes.Path)path)?.FirstOrDefault();
            Assert.NotNull(polygon2D);

            List<Polygon2D>? polygon2Ds = Planar.Query.Intersection<Polygon2D, IPolygonal2D>([rectangle2D, polygon2D]);
            Assert.NotNull(polygon2Ds);

            Assert.Equal(2, polygon2Ds.Count);
        }
    }
}
