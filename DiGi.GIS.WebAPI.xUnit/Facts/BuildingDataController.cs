using DiGi.GIS.WebAPI.Classes;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="BuildingDataController"/> write endpoints return BadRequest when provided invalid parameters.
        /// </summary>
        [Fact]
        public async Task BuildingDataController_Validation_AnswersBadRequest()
        {
            string path = ConfigurationFilePath();

            try
            {
                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher = new(path);
                BuildingDataController controller = new(gISWebAPIConfigurationFileWatcher, new PostgreSQL.Classes.BuildingDataPostgreSQLConverter(null), new PostgreSQL.Classes.Building2DPostgreSQLConverter(null));

                Assert.IsType<BadRequestResult>(await controller.UpdateItemsByCountyIdsAsync(new JsonObject(), null));
                Assert.IsType<BadRequestResult>(await controller.UpdateItemsByCountyIdsAsync(new JsonObject(), []));
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        /// <summary>
        /// Verifies that <see cref="BuildingDataController"/> write endpoints return Unauthorized when write permissions are disabled in the configuration.
        /// </summary>
        [Fact]
        public async Task BuildingDataController_DisabledUpdates_AnswersUnauthorized()
        {
            string path = System.IO.Path.GetTempFileName();

            try
            {
                System.IO.File.WriteAllLines(path,
                [
                    $"{nameof(GISWebAPIConfigurationFileWatcher.AllowUpdateBuildingData)}=false",
                ]);

                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher = new(path);
                BuildingDataController controller = new(gISWebAPIConfigurationFileWatcher, new PostgreSQL.Classes.BuildingDataPostgreSQLConverter(null), new PostgreSQL.Classes.Building2DPostgreSQLConverter(null));

                Assert.IsType<UnauthorizedResult>(await controller.UpdateItemsByCountyIdsAsync(new JsonObject(), [1]));
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        /// <summary>
        /// Verifies that <see cref="BuildingDataController"/> write endpoints return BadRequest when the table does not contain a Reference column.
        /// </summary>
        [Fact]
        public async Task BuildingDataController_MissingReferenceColumn_AnswersBadRequest()
        {
            string path = ConfigurationFilePath();

            try
            {
                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher = new(path);
                BuildingDataController controller = new(gISWebAPIConfigurationFileWatcher, new PostgreSQL.Classes.BuildingDataPostgreSQLConverter(null), new PostgreSQL.Classes.Building2DPostgreSQLConverter(null));

                DiGi.Core.IO.Table.Classes.Table table_NoReference = new();
                table_NoReference.AddColumn("Other", typeof(string));
                DiGi.Core.IO.Table.Classes.Row? row = table_NoReference.AddRow();
                if (row is not null)
                {
                    row[0] = "val";
                }
                string? json_NoReference = Core.IO.Table.Convert.ToSystem_String<DiGi.Core.IO.Table.Classes.Table, DiGi.Core.IO.Table.Classes.Column, DiGi.Core.IO.Table.Classes.Row>(table_NoReference);
                Assert.NotNull(json_NoReference);
                JsonObject? jsonObject_NoReference = JsonNode.Parse(json_NoReference) as JsonObject;

                Assert.IsType<BadRequestResult>(await controller.UpdateItemsByCountyIdsAsync(jsonObject_NoReference, [1]));

                DiGi.Core.IO.Table.Classes.Table table_LowercaseReference = new();
                table_LowercaseReference.AddColumn("reference", typeof(string));
                DiGi.Core.IO.Table.Classes.Row? row_Lower = table_LowercaseReference.AddRow();
                if (row_Lower is not null)
                {
                    row_Lower[0] = "ref1";
                }
                string? json_Lower = Core.IO.Table.Convert.ToSystem_String<DiGi.Core.IO.Table.Classes.Table, DiGi.Core.IO.Table.Classes.Column, DiGi.Core.IO.Table.Classes.Row>(table_LowercaseReference);
                Assert.NotNull(json_Lower);
                JsonObject? jsonObject_Lower = JsonNode.Parse(json_Lower) as JsonObject;

                IActionResult actionResult = await controller.UpdateItemsByCountyIdsAsync(jsonObject_Lower, [1]);
                ObjectResult objectResult = Assert.IsType<ObjectResult>(actionResult);
                Assert.Equal(500, objectResult.StatusCode);
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }
    }
}
