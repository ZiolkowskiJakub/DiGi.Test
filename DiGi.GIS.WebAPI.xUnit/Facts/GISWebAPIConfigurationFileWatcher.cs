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
        /// Verifies that <see cref="Query.IsAuthorized(GISWebAPIConfigurationFileWatcher?, string?)"/> correctly determines authorization across all configuration states.
        /// </summary>
        [Fact]
        public void GISWebAPIConfigurationFileWatcher_IsAuthorized()
        {
            GISWebAPIConfigurationFileWatcher? gISWebAPIConfigurationFileWatcher_Null = null;
            Assert.False(gISWebAPIConfigurationFileWatcher_Null.IsAuthorized("secret-key"));

            string path = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(path,
                [
                    "Enabled=false",
                    "Key=\"\"",
                    "Open=false",
                ]);

                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher_Disabled = new(path);
                Assert.False(gISWebAPIConfigurationFileWatcher_Disabled.Enabled);
                Assert.False(gISWebAPIConfigurationFileWatcher_Disabled.Open);
                Assert.False(gISWebAPIConfigurationFileWatcher_Disabled.IsAuthorized("any-key"));
                Assert.False(gISWebAPIConfigurationFileWatcher_Disabled.IsAuthorized(null));

                File.WriteAllLines(path,
                [
                    "Enabled=true",
                    "Key=\"test-secret-123\"",
                    "Open=false",
                ]);

                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher_Configured = new(path);
                Assert.True(gISWebAPIConfigurationFileWatcher_Configured.Enabled);
                Assert.Equal("test-secret-123", gISWebAPIConfigurationFileWatcher_Configured.Key);
                Assert.False(gISWebAPIConfigurationFileWatcher_Configured.Open);

                Assert.False(gISWebAPIConfigurationFileWatcher_Configured.IsAuthorized(null));
                Assert.False(gISWebAPIConfigurationFileWatcher_Configured.IsAuthorized(""));
                Assert.False(gISWebAPIConfigurationFileWatcher_Configured.IsAuthorized("wrong-key"));
                Assert.True(gISWebAPIConfigurationFileWatcher_Configured.IsAuthorized("test-secret-123"));

                File.WriteAllLines(path,
                [
                    "Enabled=false",
                    "Key=\"\"",
                    "Open=true",
                ]);

                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher_Open = new(path);
                Assert.True(gISWebAPIConfigurationFileWatcher_Open.Open);
                Assert.True(gISWebAPIConfigurationFileWatcher_Open.IsAuthorized(null));
                Assert.True(gISWebAPIConfigurationFileWatcher_Open.IsAuthorized("any-key"));
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
        /// Verifies that controller write endpoints enforce authorization headers and deny unauthorized callers.
        /// </summary>
        [Fact]
        public async Task GISWebAPIController_WriteEndpoints_AuthorizationEnforcement()
        {
            string path = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(path,
                [
                    "Enabled=true",
                    "Key=\"authorized-token\"",
                    "Open=false",
                    $"{nameof(GISWebAPIConfigurationFileWatcher.AllowUpdateBuilding2D)}=true",
                    $"{nameof(GISWebAPIConfigurationFileWatcher.AllowUpdateAdministrativeAreal2D)}=true",
                ]);

                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher = new(path);
                Building2DController building2DController = new(gISWebAPIConfigurationFileWatcher, new PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));

                Assert.IsType<UnauthorizedResult>(await building2DController.UpdateItemAsync(new JsonObject(), "code", null, key: null));
                Assert.IsType<UnauthorizedResult>(await building2DController.UpdateItemAsync(new JsonObject(), "code", null, key: "invalid-key"));
                Assert.IsType<UnauthorizedResult>(await building2DController.UpdateItemsAsync(new JsonArray(), "code", key: "invalid-key"));
                Assert.IsType<UnauthorizedResult>(await building2DController.UpdateItemsByCountyIdsAsync(new JsonArray(), [1], key: "invalid-key"));

                Assert.IsType<NoContentResult>(await building2DController.UpdateItemsAsync(new JsonArray(), "code", key: "authorized-token"));
                Assert.IsType<NoContentResult>(await building2DController.UpdateItemsByCountyIdsAsync(new JsonArray(), [1], key: "authorized-token"));

                AdministrativeAreal2DController administrativeAreal2DController = new(gISWebAPIConfigurationFileWatcher, new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));
                Assert.IsType<UnauthorizedResult>(await administrativeAreal2DController.UpdateItemAsync(new JsonObject(), key: "wrong-key"));
                Assert.IsType<UnauthorizedResult>(await administrativeAreal2DController.UpdateItemsAsync(new JsonArray(), key: "wrong-key"));
                Assert.IsType<NoContentResult>(await administrativeAreal2DController.UpdateItemAsync(null, key: "authorized-token"));
                Assert.IsType<NoContentResult>(await administrativeAreal2DController.UpdateItemsAsync(new JsonArray(), key: "authorized-token"));
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
