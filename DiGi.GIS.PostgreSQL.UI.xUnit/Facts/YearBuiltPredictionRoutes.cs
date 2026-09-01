using DiGi.GIS.WebAPI.Classes;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the routes the pipeline resolves from the controller types are the ones the deployed host actually serves.
        /// <para>The client reaches the Web API over HTTP only, so a renamed route or action produces no compile error and no runtime error either - an unknown query parameter is ignored and an unknown path is a 404 that reads like missing data. The routes below were confirmed against api.digiproject.uk on 2026-09-01, so a drift on either side fails here instead of in a run.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictionRoutes()
        {
            string? path_Table = DiGi.WebAPI.Query.Path<BuildingDataController>(nameof(BuildingDataController.GetTableByBuildingDataByReferencesParameterAsync));
            Assert.Equal("gis/buildingdata/tablebybuildingdatabyreferencesparameter", path_Table, ignoreCase: true);

            string? path_BuildingDataUpdate = DiGi.WebAPI.Query.Path<BuildingDataController>(nameof(BuildingDataController.UpdateItemsByCountyIdsAsync));
            Assert.Equal("gis/buildingdata/updateitemsbycountyids", path_BuildingDataUpdate, ignoreCase: true);

            string? path_YearBuiltData = DiGi.WebAPI.Query.Path<YearBuiltDataController>(nameof(YearBuiltDataController.GetItemsByReferenceAsync));
            Assert.Equal("gis/yearbuiltdata/itemsbyreference", path_YearBuiltData, ignoreCase: true);

            string? path_Counties = DiGi.WebAPI.Query.Path<AdministrativeAreal2DController>(nameof(AdministrativeAreal2DController.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync));
            Assert.Equal("gis/administrativeareal2d/administrativeareal2Dreferencesbyadministrativearealtype", path_Counties, ignoreCase: true);

            string? path_OrtoDatasReferences = DiGi.WebAPI.Query.Path<OrtoDatasController>(nameof(OrtoDatasController.GetOrtoDatasReferencesByCountyIdAsync));
            Assert.Equal("gis/ortodatas/ortodatasreferencesbycountyid", path_OrtoDatasReferences, ignoreCase: true);
        }
    }
}
