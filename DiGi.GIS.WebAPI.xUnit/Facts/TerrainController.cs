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
        /// <para>Drives a real transient failure through the real stack with no server: a dead loopback host makes the converter's OpenAsync throw a genuine NpgsqlException, which Npgsql classifies IsTransient = true. Every read action of <see cref="TerrainController"/> is asserted with arguments inside their validation ceilings, so a 503 mapping added to one action and forgotten in a sibling cannot pass.</para>
        /// <para>Red before the 503 mapping existed (the path answered 500); green after.</para>
        /// </summary>
        [Fact]
        public async Task TerrainController_TransientDatabaseFailure_Answers503()
        {
            string path = ConfigurationFilePath();

            try
            {
                TerrainController controller = new(new PostgreSQL.Classes.TerrainPointPostgreSQLConverter(new DiGi.PostgreSQL.Classes.ConnectionData("127.0.0.1", "user", "pass", "db", 1)), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));
                controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

                AssertTransient503(controller, await controller.GetMesh3DByCircleAsync(0, 0, 100, null, null));
                AssertTransient503(controller, await controller.GetMesh3DByBoundingBoxAsync(0, 0, 1000, 1000, null));
                AssertTransient503(controller, await controller.GetCountByCountyIdAsync(1));
                AssertTransient503(controller, await controller.GetSummariesByCountyIdsAsync(null));
                AssertTransient503(controller, await controller.GetDensitiesByCountyIdsAsync([1], null));
                AssertTransient503(controller, await controller.GetCoverageByCountyIdAsync(1, 100, 0, 0, null));
                AssertTransient503(controller, await controller.GetGapsByBoundingBoxAsync(0, 0, 1000, 1000, 100, 0, 0, null));
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }
    }
}
