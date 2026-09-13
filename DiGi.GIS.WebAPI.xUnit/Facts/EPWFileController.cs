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
        /// <para>Drives a real transient failure through the real stack with no server: a dead loopback host makes the converter's OpenAsync throw a genuine NpgsqlException, which Npgsql classifies IsTransient = true. The read action of <see cref="EPWFileController"/> is asserted, so a 503 mapping added to it cannot be missing.</para>
        /// <para>Red before the 503 mapping existed (the path answered 500); green after.</para>
        /// </summary>
        [Fact]
        public async Task EPWFileController_TransientDatabaseFailure_Answers503()
        {
            string path = ConfigurationFilePath();

            try
            {
                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher = new(path);
                EPWFileController controller = new(gISWebAPIConfigurationFileWatcher, new PostgreSQL.Classes.EPWFilePostgreSQLConverter(new DiGi.PostgreSQL.Classes.ConnectionData("127.0.0.1", "user", "pass", "db", 1)));
                controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

                AssertTransient503(controller, await controller.GetEPWFileAsync(0, 0));
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }
    }
}
