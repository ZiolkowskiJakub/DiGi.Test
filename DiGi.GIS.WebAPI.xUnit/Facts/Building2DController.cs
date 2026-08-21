using DiGi.GIS.WebAPI.Classes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Building2DController"/> endpoints return BadRequest when provided invalid parameters.
        /// </summary>
        [Fact]
        public async Task Building2DController_Validation_AnswersBadRequest()
        {
            string path = ConfigurationFilePath();

            try
            {
                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher = new(path);
                Building2DController controller = new(gISWebAPIConfigurationFileWatcher, new PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));

                Assert.IsType<BadRequestResult>(await controller.CountAsync(null!));
                Assert.IsType<BadRequestResult>(await controller.GetBuilding2DReferenceByIdAsync(0, null));
                Assert.IsType<BadRequestResult>(await controller.GetBuilding2DReferenceByIdAsync(-1, null));
                Assert.IsType<BadRequestResult>(await controller.GetBuilding2DReferenceByReferenceAsync(string.Empty, null));
                Assert.IsType<BadRequestResult>(await controller.GetBuilding2DReferencesByAdministrativeAreal2DIdAsync(0));
                Assert.IsType<BadRequestResult>(await controller.GetBuilding2DReferencesByPagingParameterAsync(null!));
                Assert.IsType<BadRequestResult>(await controller.GetItemByIdAsync(0, null));
                Assert.IsType<BadRequestResult>(await controller.GetItemByPointAsync(double.NaN, 0, null));
                Assert.IsType<BadRequestResult>(await controller.GetItemByReferenceAsync(" ", null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByBoundingBoxAsync(double.NaN, 0, 1, 1, null));
                Assert.IsType<BadRequestObjectResult>(await controller.GetItemsByBuilding2DReferencesAsync(null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByReferencesAsync(null, null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByCountyIdAsync(0));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByCircleAsync(0, 0, null, null, null));
                Assert.IsType<BadRequestResult>(await controller.GetPoint2DsByReferencesAsync(null, null));
                Assert.IsType<BadRequestResult>(await controller.GetReferencesByCountyIdAsync(0));
                Assert.IsType<BadRequestResult>(await controller.GetReferenceDuplicatesAsync(0));
                Assert.IsType<BadRequestResult>(await controller.GetReferenceUniquenessSummaryAsync(-1));
                Assert.IsType<BadRequestResult>(await controller.UpdateItemAsync(null, "code"));
                Assert.IsType<BadRequestResult>(await controller.UpdateItemsByCountyIdsAsync(new JsonArray(), []));
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        /// <summary>
        /// Verifies that <see cref="Building2DController"/> write endpoints return Unauthorized when write permissions are disabled in the configuration.
        /// </summary>
        [Fact]
        public async Task Building2DController_DisabledUpdates_AnswersUnauthorized()
        {
            string path = System.IO.Path.GetTempFileName();

            try
            {
                System.IO.File.WriteAllLines(path,
                [
                    $"{nameof(GISWebAPIConfigurationFileWatcher.AllowUpdateBuilding2D)}=false",
                ]);

                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher = new(path);
                Building2DController controller = new(gISWebAPIConfigurationFileWatcher, new PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));

                Assert.IsType<UnauthorizedResult>(await controller.UpdateItemAsync(new JsonObject(), "code"));
                Assert.IsType<UnauthorizedResult>(await controller.UpdateItemsAsync(new JsonArray(), "code"));
                Assert.IsType<UnauthorizedResult>(await controller.UpdateItemsByCountyIdsAsync(new JsonArray(), [1]));
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }
    }
}
