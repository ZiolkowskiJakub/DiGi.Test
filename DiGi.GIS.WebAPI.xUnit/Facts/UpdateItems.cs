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
        /// Asserts that a write endpoint answers 500 rather than Ok when the database stored nothing.
        /// <para>The converters are built on null connection data, so <c>DiGi.PostgreSQL.Create.NpgsqlConnection</c> hands back null and every <c>UpdateAsync</c> returns null without touching a server. That is exactly the incident this guards: the storage database was unreachable, a county regeneration posted 33 687 models into nothing, every batch came back with no identifiers, and the task reported success because the controller answered Ok on an empty result.</para>
        /// <para>Every one of these tables is written with <c>INSERT … ON CONFLICT … DO UPDATE … RETURNING id</c>, which yields a row for both branches - so past the earlier <see cref="NoContentResult"/> guards, an empty result cannot mean "correctly matched nothing".</para>
        /// </summary>
        [Fact]
        public async Task UpdateItems_NothingStoredAnswersInternalServerError()
        {
            string path = ConfigurationFilePath();

            try
            {
                using GISWebAPIConfigurationFileWatcher GISWebAPIConfigurationFileWatcher = new(path);

                Building2DController building2DController = new(GISWebAPIConfigurationFileWatcher, new DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter(null));
                AssertInternalServerError(await building2DController.UpdateItemsAsync(JsonArray(Building2D()), null));
                AssertInternalServerError(await building2DController.UpdateItemsByCountyIdsAsync(JsonArray(Building2D()), [1]));
                AssertInternalServerError(await building2DController.UpdateItemAsync(Building2D().ToJsonObject(), null));

                EPWFileController ePWFileController = new(GISWebAPIConfigurationFileWatcher, new DiGi.GIS.PostgreSQL.Classes.EPWFilePostgreSQLConverter(null));
                AssertInternalServerError(await ePWFileController.UpdateItemsAsync(JsonArray(new DiGi.EPW.Classes.EPWFile((DiGi.EPW.Classes.Location?)null))));

                OccupancyDataController occupancyDataController = new(GISWebAPIConfigurationFileWatcher, new DiGi.GIS.PostgreSQL.Classes.Building2DOccupancyDataPostgreSQLConverter(null), new DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DOccupancyDataPostgreSQLConverter(null), new DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));
                AssertInternalServerError(await occupancyDataController.Building2DUpdateItemsByCountyIdsAsync(JsonArray(OccupancyData()), [1]));
                AssertInternalServerError(await occupancyDataController.AdministrativeAreal2DUpdateItemsAsync(JsonArray(OccupancyData())));

                OrtoDatasController ortoDatasController = new(GISWebAPIConfigurationFileWatcher, new DiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter(null), new DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));
                AssertInternalServerError(await ortoDatasController.UpdateItemsByCountyIdsAsync(JsonArray(new DiGi.GIS.Classes.OrtoDatas("reference", null)), [1]));

                YearBuiltDataController yearBuiltDataController = new(GISWebAPIConfigurationFileWatcher, new DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter(null), new DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));
                AssertInternalServerError(await yearBuiltDataController.UpdateItemsByCountyIdsAsync(JsonArray(new DiGi.GIS.Classes.YearBuiltData("reference")), [1]));
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        /// <summary>
        /// Asserts that a write endpoint given nothing to write still answers 204, not 500.
        /// <para>This is the branch that makes a later empty result unambiguous, and it is the one that must not regress: an upload that correctly matches nothing has to stay a success. Every route above reaches its database call only after this guard, which is why zero identifiers past it can be read as a failure.</para>
        /// </summary>
        [Fact]
        public async Task UpdateItems_NothingToWriteAnswersNoContent()
        {
            string path = ConfigurationFilePath();

            try
            {
                using GISWebAPIConfigurationFileWatcher GISWebAPIConfigurationFileWatcher = new(path);

                JsonArray jsonArray = [];

                Building2DController building2DController = new(GISWebAPIConfigurationFileWatcher, new DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter(null));
                Assert.IsType<NoContentResult>(await building2DController.UpdateItemsAsync(jsonArray, null));
                Assert.IsType<NoContentResult>(await building2DController.UpdateItemsByCountyIdsAsync(jsonArray, [1]));

                EPWFileController ePWFileController = new(GISWebAPIConfigurationFileWatcher, new DiGi.GIS.PostgreSQL.Classes.EPWFilePostgreSQLConverter(null));
                Assert.IsType<NoContentResult>(await ePWFileController.UpdateItemsAsync(jsonArray));

                OccupancyDataController occupancyDataController = new(GISWebAPIConfigurationFileWatcher, new DiGi.GIS.PostgreSQL.Classes.Building2DOccupancyDataPostgreSQLConverter(null), new DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DOccupancyDataPostgreSQLConverter(null), new DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));
                Assert.IsType<NoContentResult>(await occupancyDataController.Building2DUpdateItemsByCountyIdsAsync(jsonArray, [1]));
                Assert.IsType<NoContentResult>(await occupancyDataController.AdministrativeAreal2DUpdateItemsAsync(jsonArray));

                OrtoDatasController ortoDatasController = new(GISWebAPIConfigurationFileWatcher, new DiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter(null), new DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));
                Assert.IsType<NoContentResult>(await ortoDatasController.UpdateItemsByCountyIdsAsync(jsonArray, [1]));

                YearBuiltDataController yearBuiltDataController = new(GISWebAPIConfigurationFileWatcher, new DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter(null), new DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));
                Assert.IsType<NoContentResult>(await yearBuiltDataController.UpdateItemsByCountyIdsAsync(jsonArray, [1]));
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        private static void AssertInternalServerError(IActionResult actionResult)
        {
            ObjectResult objectResult = Assert.IsType<ObjectResult>(actionResult);

            Assert.Equal(500, objectResult.StatusCode);
        }

        private static DiGi.GIS.Classes.Building2D Building2D()
        {
            return new(Guid.NewGuid(), "reference", null, 1, null, null, []);
        }

        private static string ConfigurationFilePath()
        {
            // The watcher reads plain 'Name=Value' lines and its constructor throws when the file is
            // missing, so the flags have to exist on disk before the controllers are built.
            string result = System.IO.Path.GetTempFileName();

            System.IO.File.WriteAllLines(result,
            [
                $"{nameof(GISWebAPIConfigurationFileWatcher.AllowUpdateBuilding2D)}=true",
                $"{nameof(GISWebAPIConfigurationFileWatcher.AllowUpdateEPWFile)}=true",
                $"{nameof(GISWebAPIConfigurationFileWatcher.AllowUpdateOrtoDatas)}=true",
                $"{nameof(GISWebAPIConfigurationFileWatcher.AllowUpdateYearBuiltData)}=true",
            ]);

            return result;
        }

        private static JsonArray JsonArray(Core.Interfaces.ISerializableObject serializableObject)
        {
            return [serializableObject.ToJsonObject()];
        }

        private static DiGi.GIS.Classes.OccupancyData OccupancyData()
        {
            return new("reference", 100, 4);
        }
    }
}
