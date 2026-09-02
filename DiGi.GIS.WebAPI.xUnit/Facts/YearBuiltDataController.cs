using DiGi.GIS.Classes;
using DiGi.GIS.WebAPI;
using DiGi.GIS.WebAPI.Classes;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that route templates on <see cref="YearBuiltDataController"/> resolve to the expected paths.
        /// </summary>
        [Fact]
        public void YearBuiltDataController_Routes_MatchTemplates()
        {
            Assert.Equal("gis/yearbuiltdata/referencesbycountyid", DiGi.WebAPI.Query.Path<YearBuiltDataController>(nameof(YearBuiltDataController.GetReferencesByCountyIdAsync)));
            Assert.Equal("gis/yearbuiltdata/countbycountyid", DiGi.WebAPI.Query.Path<YearBuiltDataController>(nameof(YearBuiltDataController.GetCountByCountyIdAsync)));
            Assert.Equal("gis/yearbuiltdata/itemsbyreferences", DiGi.WebAPI.Query.Path<YearBuiltDataController>(nameof(YearBuiltDataController.GetItemsByReferencesAsync)));
            Assert.Equal("gis/yearbuiltdata/itemsbyreference", DiGi.WebAPI.Query.Path<YearBuiltDataController>(nameof(YearBuiltDataController.GetItemsByReferenceAsync)));
        }

        /// <summary>
        /// Verifies that <see cref="YearBuiltDataController"/> endpoints return BadRequest when provided invalid parameters.
        /// </summary>
        [Fact]
        public async Task YearBuiltDataController_Validation_AnswersBadRequest()
        {
            string path = ConfigurationFilePath();

            try
            {
                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher = new(path);
                YearBuiltDataController controller = new(
                    gISWebAPIConfigurationFileWatcher,
                    new PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter(null),
                    new PostgreSQL.Classes.Building2DPostgreSQLConverter(null),
                    new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));

                Assert.IsType<BadRequestResult>(await controller.GetReferencesByCountyIdAsync(0));
                Assert.IsType<BadRequestResult>(await controller.GetReferencesByCountyIdAsync(-1));
                Assert.IsType<BadRequestResult>(await controller.GetReferencesByCountyIdAsync(1, -1));

                Assert.IsType<BadRequestResult>(await controller.GetCountByCountyIdAsync(0));
                Assert.IsType<BadRequestResult>(await controller.GetCountByCountyIdAsync(-1));
                Assert.IsType<BadRequestResult>(await controller.GetCountByCountyIdAsync(1, commandTimeout: -1));

                Assert.IsType<BadRequestResult>(await controller.GetItemsByReferencesAsync(null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByReferencesAsync([], countyId: -1));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByReferencesAsync([], commandTimeout: -1));

                string[] references_OverLimit = new string[10001];
                for (int i = 0; i < references_OverLimit.Length; i++)
                {
                    references_OverLimit[i] = "ref";
                }
                Assert.IsType<BadRequestObjectResult>(await controller.GetItemsByReferencesAsync(references_OverLimit));

                Assert.IsType<BadRequestResult>(await controller.GetItemsByReferenceAsync(string.Empty, null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByReferenceAsync(" ", null));
                Assert.IsType<BadRequestResult>(await controller.UpdateItemsAsync(null, string.Empty));
                Assert.IsType<BadRequestResult>(await controller.UpdateItemsByCountyIdsAsync(new JsonArray(), null));
                Assert.IsType<BadRequestResult>(await controller.UpdateItemsByCountyIdsAsync(new JsonArray(), []));
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        /// <summary>
        /// Verifies that client query extension methods for YearBuiltData validate input parameters properly.
        /// </summary>
        [Fact]
        public async Task YearBuiltDataController_ClientQuery_Validation()
        {
            GISWebAPIManager? gisWebAPIManager_Null = null;

            List<YearBuiltData>? items_NullManager = await gisWebAPIManager_Null.YearBuiltDatasAsync(["ref1"]);
            Assert.Null(items_NullManager);

            HashSet<string>? references_NullManager = await gisWebAPIManager_Null.YearBuiltDataReferencesAsync(1);
            Assert.Null(references_NullManager);

            long? count_NullManager = await gisWebAPIManager_Null.YearBuiltDataCountAsync(1);
            Assert.Null(count_NullManager);
        }
    }
}
