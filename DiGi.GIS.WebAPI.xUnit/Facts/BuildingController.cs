using DiGi.GIS.WebAPI.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// A transient database failure is a "retry can succeed" class, so it must answer 503 with a Retry-After header rather than the uniform 500.
        /// <para>Drives a real transient failure through the real stack with no server: a dead loopback host makes the converter's OpenAsync throw a genuine NpgsqlException, which Npgsql classifies IsTransient = true. Every read action of <see cref="BuildingController"/> is asserted, so a 503 mapping added to one action and forgotten in a sibling cannot pass.</para>
        /// <para>Red before the 503 mapping existed (the path answered 500); green after.</para>
        /// </summary>
        [Fact]
        public async Task BuildingController_TransientDatabaseFailure_Answers503()
        {
            string path = ConfigurationFilePath();

            try
            {
                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher = new(path);
                BuildingController controller = new(gISWebAPIConfigurationFileWatcher, new PostgreSQL.Classes.BuildingPostgreSQLConverter(new DiGi.PostgreSQL.Classes.ConnectionData("127.0.0.1", "user", "pass", "db", 1)), new PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));
                controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

                AssertTransient503(controller, await controller.ContainsByReferencesAsync(["ref1"], null, null));
                AssertTransient503(controller, await controller.GetCountAsync(null));
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        /// <summary>
        /// Asserts the transient-failure contract shared by the <c>_TransientDatabaseFailure_Answers503</c> facts: the action answered 503 Service Unavailable rather than the uniform 500, and advertised the 30-second retry interval through the Retry-After header.
        /// </summary>
        private static void AssertTransient503(ControllerBase controller, IActionResult actionResult)
        {
            ObjectResult objectResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(503, objectResult.StatusCode);
            Assert.Equal("30", (string?)controller.HttpContext.Response.Headers["Retry-After"]);
        }
    }
}
