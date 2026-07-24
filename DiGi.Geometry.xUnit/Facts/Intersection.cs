using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using System.Reflection;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
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

            List<Polygon2D>? polygon2Ds_1 = Query.Intersection<Polygon2D, Polygon2D>([polygon2D_1, polygon2D_3]);
            Assert.NotNull(polygon2Ds_1);
            Assert.True(polygon2Ds_1.Count == 1);

            List<Polygon2D>? polygon2Ds_2 = Query.Intersection<Polygon2D, Polygon2D>([polygon2D_2, polygon2D_3]);
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

            List<Polygon2D>? polygon2Ds = Query.Intersection<Polygon2D, IPolygonal2D>([rectangle2D, polygon2D]);
            Assert.NotNull(polygon2Ds);

            Assert.Equal(2, polygon2Ds.Count);
        }

        /// <summary>
        /// Tests that intersecting two topologically identical geometries returns the geometry itself.
        /// </summary>
        [Fact]
        public void Intersection_TopologicallyEqual()
        {
            var p1 = new Point2D(0, 0);
            var p2 = new Point2D(10, 0);
            var p3 = new Point2D(10, 10);
            var p4 = new Point2D(0, 10);

            var poly1 = new Polygon2D([p1, p2, p3, p4]);
            var poly2 = new Polygon2D([p1, p2, p3, p4]);

            // Test 1: PolygonalFace2D Intersection
            var face1 = poly1.ToNTS_Polygon().ToDiGi();
            var face2 = poly2.ToNTS_Polygon().ToDiGi();
            var faceResult = Query.Intersection(face1, face2);
            Assert.NotNull(faceResult);
            Assert.Single(faceResult);
            Assert.Equal(100.0, faceResult[0].GetArea(), 4);

            // Test 2: Polygon2D Intersection
            var polyResult = Query.Intersection(poly1, poly2);
            Assert.NotNull(polyResult);
            Assert.Single(polyResult);
            Assert.Equal(100.0, polyResult[0].GetArea(), 4);
        }

        /// <summary>
        /// Tests that intersecting disjoint geometries returns an empty list.
        /// </summary>
        [Fact]
        public void Intersection_Disjoint()
        {
            var poly1 = new Polygon2D([
                new Point2D(0, 0),
                new Point2D(2, 0),
                new Point2D(2, 2),
                new Point2D(0, 2)
            ]);

            var poly2 = new Polygon2D([
                new Point2D(5, 5),
                new Point2D(7, 5),
                new Point2D(7, 7),
                new Point2D(5, 7)
            ]);

            var result = Query.Intersection(poly1, poly2);
            Assert.NotNull(result);
            Assert.Empty(result);

            var face1 = poly1.ToNTS_Polygon().ToDiGi();
            var face2 = poly2.ToNTS_Polygon().ToDiGi();
            var faceResult = Query.Intersection(face1, face2);
            Assert.NotNull(faceResult);
            Assert.Empty(faceResult);
        }

        /// <summary>
        /// Tests that generic intersection returns all intersecting components of type X.
        /// </summary>
        [Fact]
        public void Intersection_MultipleResults()
        {
            var uShape = new Polygon2D([
                new Point2D(0, 0),
                new Point2D(6, 0),
                new Point2D(6, 4),
                new Point2D(4, 4),
                new Point2D(4, 2),
                new Point2D(2, 2),
                new Point2D(2, 4),
                new Point2D(0, 4)
            ]);

            var rect = new Polygon2D([
                new Point2D(0, 3),
                new Point2D(6, 3),
                new Point2D(6, 5),
                new Point2D(0, 5)
            ]);

            List<Polygon2D>? result = Query.Intersection<Polygon2D, IPolygonal2D>([uShape, rect]);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(4.0, result.Sum(x => x.GetArea()), 4);
        }

        /// <summary>
        /// Tests that when two 2D polygons share a single boundary edge (yielding a line intersection in NTS),
        /// the intersection query gracefully returns an empty collection of polygons instead of throwing a NotImplementedException.
        /// </summary>
        [Fact]
        public void Intersection_BoundaryTouch()
        {
            Polygon2D polygon2D_1 = new([
                new Point2D(0.0, 0.0),
                new Point2D(2.0, 0.0),
                new Point2D(2.0, 2.0),
                new Point2D(0.0, 2.0)
            ]);

            Polygon2D polygon2D_2 = new([
                new Point2D(2.0, 0.0),
                new Point2D(4.0, 0.0),
                new Point2D(4.0, 2.0),
                new Point2D(2.0, 2.0)
            ]);

            List<Polygon2D>? list_Result = Query.Intersection<Polygon2D, Polygon2D>([polygon2D_1, polygon2D_2]);
            Assert.NotNull(list_Result);
            Assert.Empty(list_Result);
        }

        /// <summary>
        /// Tests that attempting an unsupported polygonal conversion using TryConvert gracefully returns false instead of throwing a NotImplementedException.
        /// </summary>
        [Fact]
        public void TryConvert_Unsupported()
        {
            Polygon2D polygon2D_Test = new([
                new Point2D(0.0, 0.0),
                new Point2D(2.0, 0.0),
                new Point2D(0.0, 2.0)
            ]);

            bool success_Unsupported = Query.TryConvert(polygon2D_Test, out List<Rectangle2D>? rectangle2Ds_Converted);
            Assert.False(success_Unsupported);
            Assert.Null(rectangle2Ds_Converted);
        }
    }
}