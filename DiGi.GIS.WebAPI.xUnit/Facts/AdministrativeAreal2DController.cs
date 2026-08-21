using DiGi.GIS.PostgreSQL.Enums;
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
        /// Verifies that <see cref="AdministrativeAreal2DController"/> endpoints return BadRequest when provided invalid parameters.
        /// </summary>
        [Fact]
        public async Task AdministrativeAreal2DController_Validation_AnswersBadRequest()
        {
            string path = ConfigurationFilePath();

            try
            {
                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher = new(path);
                AdministrativeAreal2DController controller = new(gISWebAPIConfigurationFileWatcher, new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));

                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferenceByCodeAsync(string.Empty, null));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferenceByCodeAsync("2212", AdministrativeArealType.Undefined));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferenceByIdAsync(0));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferenceByIdAsync(-1));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencePathByIdAsync(0));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencePathByIdAsync(-1));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencePathsByNameAsync(string.Empty));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencePathsByNameParameterAsync(null!));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencePathsByNameParameterAsync(new AdministrativeAreal2DReferencePathsByNameParameter(" ")));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencesByNameParameterAsync(null!));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencesByNameParameterAsync(new AdministrativeAreal2DReferencesByNameParameter(" ")));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.Undefined, null, null));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.County, 0, null));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.County, -1, null));

                // An omitted administrativearealtype binds to null, not to Undefined. Guarding only against
                // Undefined (which is -1) would let an omitted parameter through as Country (which is 0),
                // so the caller would silently be answered with countries instead of being rejected. These
                // three endpoints are the ones that take the type as their only filter.
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(null, null, null));
                Assert.IsType<BadRequestResult>(await controller.GetIdsByAdministrativeArealTypeAsync(null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByAdministrativeArealTypeAsync(null));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencesByCodeAsync(string.Empty, null));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencesByCodeAsync("2212", AdministrativeArealType.Undefined));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencesByIdsAsync(null!));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencesByIdsAsync([]));
                Assert.IsType<BadRequestResult>(await controller.GetIdByCodeAsync(string.Empty, null));
                Assert.IsType<BadRequestResult>(await controller.GetIdByCodeAsync("2212", AdministrativeArealType.Undefined));
                Assert.IsType<BadRequestResult>(await controller.GetIdsByCodeAsync(string.Empty, null));
                Assert.IsType<BadRequestResult>(await controller.GetIdsByCodeAsync("2212", AdministrativeArealType.Undefined));
                Assert.IsType<BadRequestResult>(await controller.GetIdsByAdministrativeArealTypeAsync(AdministrativeArealType.Undefined));
                Assert.IsType<BadRequestResult>(await controller.GetItemByCodeAsync(string.Empty, null));
                Assert.IsType<BadRequestResult>(await controller.GetItemByCodeAsync("2212", AdministrativeArealType.Undefined));
                Assert.IsType<BadRequestResult>(await controller.GetItemByIdAsync(0));
                Assert.IsType<BadRequestResult>(await controller.GetItemByIdAsync(-1));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByAdministrativeArealTypeAsync(AdministrativeArealType.Undefined));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByBoundingBoxAsync(double.NaN, 0, 1, 1, null, null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByBoundingBoxAsync(0, double.NaN, 1, 1, null, null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByBoundingBoxAsync(0, 0, double.NaN, 1, null, null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByBoundingBoxAsync(0, 0, 1, double.NaN, null, null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByBoundingBoxAsync(0, 0, 1, 1, -1, null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByBoundingBoxAsync(0, 0, 1, 1, null, AdministrativeArealType.Undefined));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByCircleAsync(double.NaN, 0, 10, null, null, null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByCircleAsync(0, double.NaN, 10, null, null, null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByCircleAsync(0, 0, null, null, null, null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByCircleAsync(0, 0, -5, null, null, null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByCircleAsync(0, 0, 0, null, null, null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByCircleAsync(0, 0, 10, null, -1, null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByCircleAsync(0, 0, 10, null, null, AdministrativeArealType.Undefined));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByCodeAsync(string.Empty, null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByCodeAsync("2212", AdministrativeArealType.Undefined));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByCodesAsync(null!));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByCodesAsync([]));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByIdsAsync(null!));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByIdsAsync([]));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByPointAsync(double.NaN, 0, null, null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByPointAsync(0, double.NaN, null, null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByPointAsync(0, 0, -1, null));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByPointAsync(0, 0, null, AdministrativeArealType.Undefined));
                Assert.IsType<BadRequestResult>(await controller.GetSubCodesAsync(string.Empty));
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        /// <summary>
        /// Verifies that <see cref="AdministrativeAreal2DController"/> write endpoints return Unauthorized when write permissions are disabled in the configuration.
        /// </summary>
        [Fact]
        public async Task AdministrativeAreal2DController_DisabledUpdates_AnswersUnauthorized()
        {
            string path = System.IO.Path.GetTempFileName();

            try
            {
                System.IO.File.WriteAllLines(path,
                [
                    $"{nameof(GISWebAPIConfigurationFileWatcher.AllowUpdateAdministrativeAreal2D)}=false",
                ]);

                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher = new(path);
                AdministrativeAreal2DController controller = new(gISWebAPIConfigurationFileWatcher, new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));

                Assert.IsType<UnauthorizedResult>(await controller.UpdateItemAsync(new JsonObject()));
                Assert.IsType<UnauthorizedResult>(await controller.UpdateItemsAsync(new JsonArray()));
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        /// <summary>
        /// Verifies that <see cref="AdministrativeAreal2DController"/> endpoints return BadRequest when converter is null.
        /// </summary>
        [Fact]
        public async Task AdministrativeAreal2DController_NullConverter_AnswersBadRequest()
        {
            string path = ConfigurationFilePath();

            try
            {
                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher = new(path);
                AdministrativeAreal2DController controller = new(gISWebAPIConfigurationFileWatcher, null!);

                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferenceByCodeAsync("2212", AdministrativeArealType.County));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferenceByIdAsync(1));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencePathByIdAsync(1));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencePathsByNameAsync("test"));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencePathsByNameParameterAsync(new AdministrativeAreal2DReferencePathsByNameParameter("test")));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencesByNameParameterAsync(new AdministrativeAreal2DReferencesByNameParameter("test")));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.County, null, null));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencesByCodeAsync("2212", AdministrativeArealType.County));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencesByIdsAsync([1]));
                Assert.IsType<BadRequestResult>(await controller.GetBoundingBox2DAsync());
                Assert.IsType<BadRequestResult>(await controller.GetCodesAsync());
                Assert.IsType<BadRequestResult>(await controller.GetCountAsync());
                Assert.IsType<BadRequestResult>(await controller.GetIdByCodeAsync("2212", AdministrativeArealType.County));
                Assert.IsType<BadRequestResult>(await controller.GetIdsByCodeAsync("2212", AdministrativeArealType.County));
                Assert.IsType<BadRequestResult>(await controller.GetIdsByAdministrativeArealTypeAsync(AdministrativeArealType.County));
                Assert.IsType<BadRequestResult>(await controller.GetItemByCodeAsync("2212", AdministrativeArealType.County));
                Assert.IsType<BadRequestResult>(await controller.GetItemByIdAsync(1));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByAdministrativeArealTypeAsync(AdministrativeArealType.County));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByBoundingBoxAsync(0, 0, 10, 10, null, AdministrativeArealType.County));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByCircleAsync(0, 0, 10, null, null, AdministrativeArealType.County));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByCodeAsync("2212", AdministrativeArealType.County));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByCodesAsync(["2212"]));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByIdsAsync([1]));
                Assert.IsType<BadRequestResult>(await controller.GetItemsByPointAsync(0, 0, null, AdministrativeArealType.County));
                Assert.IsType<BadRequestResult>(await controller.GetSubCodesAsync("2212"));
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }
    }
}
