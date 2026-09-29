using DiGi.GIS.WebAPI.UI.Classes;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the view range of a scene loaded for a circular area (#64): the radius is both the maximum and the initial value, the minimum is 100 m (or the radius when smaller), and an unusable radius yields no range, so the viewer keeps its defaults.
        /// </summary>
        [Fact]
        public void Create_ViewRange()
        {
            ViewRange? viewRange_500 = DiGi.GIS.WebAPI.UI.Create.ViewRange(500);
            Assert.NotNull(viewRange_500);
            Assert.Equal(100, viewRange_500!.Minimum);
            Assert.Equal(500, viewRange_500.Maximum);
            Assert.Equal(500, viewRange_500.Default);

            // A radius below the usual minimum collapses the slider onto itself instead of putting the minimum above the maximum.
            ViewRange? viewRange_50 = DiGi.GIS.WebAPI.UI.Create.ViewRange(50);
            Assert.NotNull(viewRange_50);
            Assert.Equal(50, viewRange_50!.Minimum);
            Assert.Equal(50, viewRange_50.Maximum);
            Assert.Equal(50, viewRange_50.Default);

            Assert.Null(DiGi.GIS.WebAPI.UI.Create.ViewRange(null));
            Assert.Null(DiGi.GIS.WebAPI.UI.Create.ViewRange(0));
            Assert.Null(DiGi.GIS.WebAPI.UI.Create.ViewRange(-1));
            Assert.Null(DiGi.GIS.WebAPI.UI.Create.ViewRange(double.NaN));
            Assert.Null(DiGi.GIS.WebAPI.UI.Create.ViewRange(double.PositiveInfinity));
        }

        /// <summary>
        /// Tests the view range of a single building scene (#64): without a radius the slider starts at the default terrain extent and spans three times it, with a radius R it spans R to 3R, and a radius whose maximum exceeds the terrain limit (just inside and just outside the boundary) or is unusable yields no range.
        /// </summary>
        [Fact]
        public void Create_BuildingViewRange()
        {
            ViewRange? viewRange_Null = DiGi.GIS.WebAPI.UI.Create.BuildingViewRange(null);
            Assert.NotNull(viewRange_Null);
            Assert.Equal(Constants.Default.TerrainRadius, viewRange_Null!.Minimum);
            Assert.Equal(Constants.Default.TerrainRadius, viewRange_Null.Default);
            Assert.Equal(Constants.Default.TerrainRadius * Constants.Default.BuildingViewRangeFactor, viewRange_Null.Maximum);

            ViewRange? viewRange_250 = DiGi.GIS.WebAPI.UI.Create.BuildingViewRange(250);
            Assert.NotNull(viewRange_250);
            Assert.Equal(250, viewRange_250!.Minimum);
            Assert.Equal(250, viewRange_250.Default);
            Assert.Equal(750, viewRange_250.Maximum);

            double radius_Boundary = Constants.Default.TerrainRadiusMax / Constants.Default.BuildingViewRangeFactor;
            Assert.NotNull(DiGi.GIS.WebAPI.UI.Create.BuildingViewRange(radius_Boundary));
            Assert.Null(DiGi.GIS.WebAPI.UI.Create.BuildingViewRange(radius_Boundary + 0.001));

            Assert.Null(DiGi.GIS.WebAPI.UI.Create.BuildingViewRange(0));
            Assert.Null(DiGi.GIS.WebAPI.UI.Create.BuildingViewRange(-5));
            Assert.Null(DiGi.GIS.WebAPI.UI.Create.BuildingViewRange(double.NaN));
        }
    }
}
