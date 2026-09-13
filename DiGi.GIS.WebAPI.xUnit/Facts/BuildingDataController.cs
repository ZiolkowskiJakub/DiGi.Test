using DiGi.GIS.WebAPI.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
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

                Core.IO.Table.Classes.Table table_NoReference = new();
                table_NoReference.AddColumn("Other", typeof(string));
                Core.IO.Table.Classes.Row? row = table_NoReference.AddRow();
                if (row is not null)
                {
                    row[0] = "val";
                }
                string? json_NoReference = Core.IO.Table.Convert.ToSystem_String<Core.IO.Table.Classes.Table, Core.IO.Table.Classes.Column, Core.IO.Table.Classes.Row>(table_NoReference);
                Assert.NotNull(json_NoReference);
                JsonObject? jsonObject_NoReference = JsonNode.Parse(json_NoReference) as JsonObject;

                Assert.IsType<BadRequestResult>(await controller.UpdateItemsByCountyIdsAsync(jsonObject_NoReference, [1]));

                Core.IO.Table.Classes.Table table_LowercaseReference = new();
                table_LowercaseReference.AddColumn("reference", typeof(string));
                Core.IO.Table.Classes.Row? row_Lower = table_LowercaseReference.AddRow();
                if (row_Lower is not null)
                {
                    row_Lower[0] = "ref1";
                }
                string? json_Lower = Core.IO.Table.Convert.ToSystem_String<Core.IO.Table.Classes.Table, Core.IO.Table.Classes.Column, Core.IO.Table.Classes.Row>(table_LowercaseReference);
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

        /// <summary>
        /// A transient database failure is a "retry can succeed" class, so it must answer 503 with a Retry-After header rather than the uniform 500.
        /// <para>Drives a real transient failure through the real stack with no server: a dead loopback host makes the converter's OpenAsync throw a genuine NpgsqlException wrapping a SocketException, which Npgsql classifies IsTransient = true (verified against the deployed Npgsql 10.0.2). The 2026-09-12 production read-timeout failures and the pool-exhaustion shape are the same IsTransient = true class, so this fact covers them.</para>
        /// <para>Red before the 503 mapping existed (the path answered 500); green after.</para>
        /// </summary>
        [Fact]
        public async Task BuildingDataController_TransientDatabaseFailure_Answers503()
        {
            string path = ConfigurationFilePath();

            try
            {
                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher = new(path);
                BuildingDataController controller = new(
                    gISWebAPIConfigurationFileWatcher,
                    new PostgreSQL.Classes.BuildingDataPostgreSQLConverter(new DiGi.PostgreSQL.Classes.ConnectionData("127.0.0.1", "user", "pass", "db", 1)),
                    new PostgreSQL.Classes.Building2DPostgreSQLConverter(null));
                controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

                ObjectResult objectResult = Assert.IsType<ObjectResult>(await controller.GetCategoriesAsync());

                Assert.Equal(503, objectResult.StatusCode);
                Assert.Equal("30", (string?)controller.HttpContext.Response.Headers["Retry-After"]);
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        /// <summary>
        /// The 503 branch is guarded by <c>NpgsqlException.IsTransient</c>, so non-transient database failures keep the uniform 500 and only the "retry can succeed" class is reclassified.
        /// <para>Pins the classification contract the guard relies on (verified against Npgsql 10.0.2): a statement timeout (57014) and a plain NpgsqlException are not transient; pool exhaustion (53300) and socket read timeouts are.</para>
        /// </summary>
        [Fact]
        public void BuildingDataController_TransientPredicate_RoutesOnlyTransientTo503()
        {
            Assert.True(new NpgsqlException("read", new System.TimeoutException()).IsTransient);
            Assert.True(new NpgsqlException("read", new System.IO.IOException()).IsTransient);
            Assert.True(new PostgresException("too many clients", "FATAL", "FATAL", "53300").IsTransient);

            Assert.False(new NpgsqlException("boom").IsTransient);
            Assert.False(new PostgresException("cancelling statement", "ERROR", "ERROR", "57014").IsTransient);
        }
    }
}
