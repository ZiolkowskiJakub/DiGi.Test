namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the neighbour radius of the Building Viewer's "Solar radiation" panel: an omitted page radius is the scene default <see cref="Constants.Default.TerrainRadius"/>, a radius within <see cref="Constants.Default.SolarSurroundingRadiusMax"/> is kept, and a larger one is clamped to it.
        /// <para>Covers the acceptance examples of DiGi.GIS.WebAPI.UI#66 (empty → 100, 30 → 30, 300 → 100) and both sides of the limit.</para>
        /// </summary>
        [Fact]
        public void Query_SolarRadius()
        {
            Assert.Equal(Constants.Default.TerrainRadius, Query.SolarRadius(null));
            Assert.Equal(100, Query.SolarRadius(null));
            Assert.Equal(30, Query.SolarRadius(30));
            Assert.Equal(100, Query.SolarRadius(300));

            Assert.Equal(Constants.Default.SolarSurroundingRadiusMax, Query.SolarRadius(Constants.Default.SolarSurroundingRadiusMax));
            Assert.Equal(Constants.Default.SolarSurroundingRadiusMax - 0.001, Query.SolarRadius(Constants.Default.SolarSurroundingRadiusMax - 0.001));
            Assert.Equal(Constants.Default.SolarSurroundingRadiusMax, Query.SolarRadius(Constants.Default.SolarSurroundingRadiusMax + 0.001));
        }
    }
}
