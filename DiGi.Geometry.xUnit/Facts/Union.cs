using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the union operation of two <see cref="Triangle2D"/> objects sharing a common edge results in a single <see cref="Polygon2D"/> with an area equal to the sum of the individual triangle areas.
        /// </summary>
        [Fact]
        public void Union()
        {
            Triangle2D triangle2D_1 = new((0, 0), (10, 0), (0, 10));
            Triangle2D triangle2D_2 = new((10, 10), (10, 0), (0, 10));

            List<Polygon2D>? polygon2Ds = Planar.Query.Union(triangle2D_1, triangle2D_2);
            Assert.NotNull(polygon2Ds);
            Assert.True(polygon2Ds.Count == 1);

            Assert.True(DiGi.Core.Query.AlmostEquals(polygon2Ds[0].GetArea(), triangle2D_1.GetArea() + triangle2D_2.GetArea(), DiGi.Core.Constants.Tolerance.MacroDistance));
        }

        /// <summary>
        /// Tests that unioning two disjoint geometries returns both geometries in the result list.
        /// </summary>
        [Fact]
        public void Union_Disjoint()
        {
            var p1 = new Polygon2D([new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2)]);
            var p2 = new Polygon2D([new Point2D(5, 5), new Point2D(7, 5), new Point2D(7, 7), new Point2D(5, 7)]);

            // Test 1: Polygon2D Union
            var result = Planar.Query.Union(p1, p2);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(8.0, result.Sum(x => x.GetArea()), 4);

            // Test 2: PolygonalFace2D Union
            var face1 = p1.ToNTS_Polygon().ToDiGi();
            var face2 = p2.ToNTS_Polygon().ToDiGi();
            var faceResult = Planar.Query.Union(face1, face2);
            Assert.NotNull(faceResult);
            Assert.Equal(2, faceResult.Count);
            Assert.Equal(8.0, faceResult.Sum(x => x.GetArea()), 4);
        }

        /// <summary>
        /// Tests that unioning geometries with some invalid ones resolves correctly.
        /// </summary>
        [Fact]
        public void Union_InvalidPolygons()
        {
            // Create a self-intersecting polygon (invalid)
            var invalidPoly = new Polygon2D([
                new Point2D(0, 0),
                new Point2D(2, 0),
                new Point2D(0, 2),
                new Point2D(2, 2)
            ]);

            var poly2 = new Polygon2D([
                new Point2D(1, 0),
                new Point2D(3, 0),
                new Point2D(3, 2),
                new Point2D(1, 2)
            ]);

            // Just calling Union should handle it via individual geometry fixes and complete successfully
            var result = Planar.Query.Union<IPolygonal2D>([invalidPoly, poly2]);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        /// <summary>
        /// Tests that passing empty or single collections returns empty or single result.
        /// </summary>
        [Fact]
        public void Union_EmptyAndSingle()
        {
            List<Polygon2D> emptyList = [];
            var resultEmpty = Planar.Query.Union(emptyList);
            Assert.NotNull(resultEmpty);
            Assert.Empty(resultEmpty);

            var poly = new Polygon2D([new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2)]);
            var resultSingle = Planar.Query.Union([poly]);
            Assert.NotNull(resultSingle);
            Assert.Single(resultSingle);
            Assert.Equal(4.0, resultSingle[0].GetArea(), 4);
        }

        /// <summary>
        /// Tests unioning a larger set of overlapping polygons (cascaded union).
        /// </summary>
        [Fact]
        public void Union_CascadedOverlap()
        {
            var p1 = new Polygon2D([new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2)]);
            var p2 = new Polygon2D([new Point2D(1, 0), new Point2D(3, 0), new Point2D(3, 2), new Point2D(1, 2)]);
            var p3 = new Polygon2D([new Point2D(2, 0), new Point2D(4, 0), new Point2D(4, 2), new Point2D(2, 2)]);
            var p4 = new Polygon2D([new Point2D(3, 0), new Point2D(5, 0), new Point2D(5, 2), new Point2D(3, 2)]);

            var result = Planar.Query.Union([p1, p2, p3, p4]);
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(10.0, result[0].GetArea(), 4);
        }

        /// <summary>
        /// Tests that unioning two topologically equal geometries returns a single geometry.
        /// </summary>
        [Fact]
        public void Union_TopologicallyEqual()
        {
            var p1 = new Polygon2D([new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2)]);
            var p2 = new Polygon2D([new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2)]);

            var result = Planar.Query.Union(p1, p2);
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(4.0, result[0].GetArea(), 4);
        }
    }
}