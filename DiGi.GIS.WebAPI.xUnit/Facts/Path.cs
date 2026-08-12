using DiGi.GIS.WebAPI.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Asserts the route templates of the county-keyed write endpoints.
        /// <para>These routes are resolved by reflection at runtime through <c>GISWebAPIManager.CreateHttpClient</c>. A renamed or mistyped template does not throw - the manager hands back a null path and the upload returns <see langword="false"/> with nothing logged - so the templates are pinned here rather than discovered in production.</para>
        /// <para>The <c>bycountyid</c> routes are the unambiguous ones: a county code maps to one row per polygon part of a multi-part county, so a code-keyed upload lets the server choose a part.</para>
        /// </summary>
        [Fact]
        public void Path_UpdateItemsByCountyId()
        {
            Assert.Equal("gis/buildingmodel/updateitemsbycountyid", DiGi.WebAPI.Query.Path<BuildingModelController>(nameof(BuildingModelController.UpdateItemsByCountyIdAsync)));
            Assert.Equal("gis/building/updateitemsbycountyid", DiGi.WebAPI.Query.Path<BuildingController>(nameof(BuildingController.UpdateItemsByCountyIdAsync)));
            Assert.Equal("gis/ortodatas/updateitemsbycountyid", DiGi.WebAPI.Query.Path<OrtoDatasController>(nameof(OrtoDatasController.UpdateItemsByCountyIdAsync)));
            Assert.Equal("gis/yearbuiltdata/updateitemsbycountyid", DiGi.WebAPI.Query.Path<YearBuiltDataController>(nameof(YearBuiltDataController.UpdateItemsByCountyIdAsync)));
            Assert.Equal("gis/occupancydata/building2d/updateitemsbycountyid", DiGi.WebAPI.Query.Path<OccupancyDataController>(nameof(OccupancyDataController.Building2DUpdateItemsByCountyIdAsync)));
        }

        /// <summary>
        /// Asserts the route templates of the code-keyed write endpoints, which remain as a fallback for callers that only hold a BDOT10k county code.
        /// <para>Pinned for the same reason as the county-keyed routes: a broken template fails silently at runtime.</para>
        /// </summary>
        [Fact]
        public void Path_UpdateItemsByCode()
        {
            Assert.Equal("gis/buildingmodel/updateitems", DiGi.WebAPI.Query.Path<BuildingModelController>(nameof(BuildingModelController.UpdateItemsAsync)));
            Assert.Equal("gis/building/updateitems", DiGi.WebAPI.Query.Path<BuildingController>(nameof(BuildingController.UpdateItemsAsync)));
            Assert.Equal("gis/ortodatas/updateitemsbycode", DiGi.WebAPI.Query.Path<OrtoDatasController>(nameof(OrtoDatasController.UpdateItemsByCodeAsync)));
            Assert.Equal("gis/yearbuiltdata/updateitems", DiGi.WebAPI.Query.Path<YearBuiltDataController>(nameof(YearBuiltDataController.UpdateItemsAsync)));
            Assert.Equal("gis/occupancydata/building2d/updateitems", DiGi.WebAPI.Query.Path<OccupancyDataController>(nameof(OccupancyDataController.Building2DUpdateItemsAsync)));
        }

        /// <summary>
        /// Asserts that every county-keyed write endpoint has a distinct route template.
        /// <para>Two actions sharing a template would make <c>CreateHttpClient</c> post one payload to the other's endpoint, which is silent at compile time and at runtime.</para>
        /// </summary>
        [Fact]
        public void Path_UpdateEndpointsAreDistinct()
        {
            string?[] paths =
            [
                DiGi.WebAPI.Query.Path<BuildingModelController>(nameof(BuildingModelController.UpdateItemsAsync)),
                DiGi.WebAPI.Query.Path<BuildingModelController>(nameof(BuildingModelController.UpdateItemsByCountyIdAsync)),
                DiGi.WebAPI.Query.Path<BuildingController>(nameof(BuildingController.UpdateItemsAsync)),
                DiGi.WebAPI.Query.Path<BuildingController>(nameof(BuildingController.UpdateItemsByCountyIdAsync)),
                DiGi.WebAPI.Query.Path<OrtoDatasController>(nameof(OrtoDatasController.UpdateItemsByCodeAsync)),
                DiGi.WebAPI.Query.Path<OrtoDatasController>(nameof(OrtoDatasController.UpdateItemsByCountyIdAsync)),
                DiGi.WebAPI.Query.Path<YearBuiltDataController>(nameof(YearBuiltDataController.UpdateItemsAsync)),
                DiGi.WebAPI.Query.Path<YearBuiltDataController>(nameof(YearBuiltDataController.UpdateItemsByCountyIdAsync)),
                DiGi.WebAPI.Query.Path<OccupancyDataController>(nameof(OccupancyDataController.Building2DUpdateItemsAsync)),
                DiGi.WebAPI.Query.Path<OccupancyDataController>(nameof(OccupancyDataController.Building2DUpdateItemsByCountyIdAsync)),
            ];

            HashSet<string?> paths_Distinct = [.. paths];

            Assert.Equal(paths.Length, paths_Distinct.Count);
        }

        /// <summary>
        /// Asserts that an unknown action name resolves to the bare controller route rather than throwing.
        /// <para>This is the failure mode the other facts guard against: a renamed action silently degrades to the controller root, so a POST lands somewhere that is not the intended endpoint instead of erroring.</para>
        /// </summary>
        [Fact]
        public void Path_UnknownActionFallsBackToControllerRoute()
        {
            Assert.Equal("gis/buildingmodel", DiGi.WebAPI.Query.Path<BuildingModelController>("ThisActionDoesNotExist"));
        }
    }
}
