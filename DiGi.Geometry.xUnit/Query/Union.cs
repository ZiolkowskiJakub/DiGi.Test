using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Query
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
    }
}
