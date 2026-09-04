using DiGi.GIS.WebAPI.Classes;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="GISWebAPIConfigurationFileWatcher.AllowUpdateOccupancyData"/> reflects configuration settings and defaults to false when absent.
        /// </summary>
        [Fact]
        public void GISWebAPIConfigurationFileWatcher_AllowUpdateOccupancyData()
        {
            string path = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(path,
                [
                    "Enabled=true",
                    "Key=\"key\"",
                    $"{nameof(GISWebAPIConfigurationFileWatcher.AllowUpdateOccupancyData)}=true",
                ]);

                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher_Enabled = new(path);
                Assert.True(gISWebAPIConfigurationFileWatcher_Enabled.AllowUpdateOccupancyData);

                File.WriteAllLines(path,
                [
                    "Enabled=true",
                    "Key=\"key\"",
                    $"{nameof(GISWebAPIConfigurationFileWatcher.AllowUpdateOccupancyData)}=false",
                ]);

                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher_Disabled = new(path);
                Assert.False(gISWebAPIConfigurationFileWatcher_Disabled.AllowUpdateOccupancyData);

                File.WriteAllLines(path,
                [
                    "Enabled=true",
                    "Key=\"key\"",
                ]);

                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher_Default = new(path);
                Assert.False(gISWebAPIConfigurationFileWatcher_Default.AllowUpdateOccupancyData);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        /// <summary>
        /// Verifies that <see cref="OccupancyDataController"/> write endpoints return BadRequest when write permissions are disabled in the configuration.
        /// </summary>
        [Fact]
        public async Task OccupancyDataController_DisabledUpdates_AnswersBadRequest()
        {
            string path = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(path,
                [
                    $"{nameof(GISWebAPIConfigurationFileWatcher.Open)}=true",
                    $"{nameof(GISWebAPIConfigurationFileWatcher.AllowUpdateOccupancyData)}=false",
                ]);

                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher = new(path);
                OccupancyDataController occupancyDataController = new(
                    gISWebAPIConfigurationFileWatcher,
                    new PostgreSQL.Classes.Building2DOccupancyDataPostgreSQLConverter(null),
                    new PostgreSQL.Classes.AdministrativeAreal2DOccupancyDataPostgreSQLConverter(null),
                    new PostgreSQL.Classes.Building2DPostgreSQLConverter(null),
                    new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));

                Assert.IsType<BadRequestResult>(await occupancyDataController.AdministrativeAreal2DUpdateItemsAsync(new JsonArray()));
                Assert.IsType<BadRequestResult>(await occupancyDataController.Building2DUpdateItemsAsync(new JsonArray(), "code"));
                Assert.IsType<BadRequestResult>(await occupancyDataController.Building2DUpdateItemsByCountyIdsAsync(new JsonArray(), [1]));
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        /// <summary>
        /// Verifies that <see cref="OccupancyDataController"/> write endpoints are independent from <see cref="GISWebAPIConfigurationFileWatcher.AllowUpdateYearBuiltData"/>.
        /// </summary>
        [Fact]
        public async Task OccupancyDataController_FlagIndependence()
        {
            string path = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(path,
                [
                    $"{nameof(GISWebAPIConfigurationFileWatcher.Open)}=true",
                    $"{nameof(GISWebAPIConfigurationFileWatcher.AllowUpdateOccupancyData)}=false",
                    $"{nameof(GISWebAPIConfigurationFileWatcher.AllowUpdateYearBuiltData)}=true",
                ]);

                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher = new(path);
                OccupancyDataController occupancyDataController = new(
                    gISWebAPIConfigurationFileWatcher,
                    new PostgreSQL.Classes.Building2DOccupancyDataPostgreSQLConverter(null),
                    new PostgreSQL.Classes.AdministrativeAreal2DOccupancyDataPostgreSQLConverter(null),
                    new PostgreSQL.Classes.Building2DPostgreSQLConverter(null),
                    new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));

                Assert.IsType<BadRequestResult>(await occupancyDataController.AdministrativeAreal2DUpdateItemsAsync(new JsonArray()));
                Assert.IsType<BadRequestResult>(await occupancyDataController.Building2DUpdateItemsAsync(new JsonArray(), "code"));
                Assert.IsType<BadRequestResult>(await occupancyDataController.Building2DUpdateItemsByCountyIdsAsync(new JsonArray(), [1]));

                YearBuiltDataController yearBuiltDataController = new(
                    gISWebAPIConfigurationFileWatcher,
                    new PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter(null),
                    new PostgreSQL.Classes.Building2DPostgreSQLConverter(null),
                    new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));

                Assert.IsType<NoContentResult>(await yearBuiltDataController.UpdateItemsAsync([], "code"));
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }
}
