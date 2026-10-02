using DiGi.GIS.WebAPI.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the read endpoints added for verifying the subdivision links reject what they cannot act on, without touching a database - and that each action with a commandTimeout guard refuses a negative value (ZiolkowskiJakub/DiGi.GIS.WebAPI#48).
        /// <para>Each of the ceilings is asserted from both sides of its boundary, because a limit that is off by one either refuses a legitimate request or admits the one it exists to stop.</para>
        /// </summary>
        [Fact]
        public async Task OrtoDatasController_Validation_AnswersBadRequest()
        {
            string path = ConfigurationFilePath();

            try
            {
                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher = new(path);
                OrtoDatasController controller = new(gISWebAPIConfigurationFileWatcher, new PostgreSQL.Classes.OrtoDatasPostgreSQLConverter(null), new PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));

                // A negative sample count is meaningless and one past the ceiling is what the ceiling exists
                // for; the value either side of each is accepted, so the boundary itself is pinned.
                Assert.IsType<BadRequestObjectResult>(await controller.GetSubdivisionLinksByCountyIdAsync(55417, -1));
                Assert.IsType<BadRequestObjectResult>(await controller.GetSubdivisionLinksByCountyIdAsync(55417, Constants.OrtoDatas.MaximumSampleCount + 1));

                // Naming more counties than the ceiling allows is refused; naming none is not, because that
                // asks for every partition in a single grouped statement rather than one per county.
                Assert.IsType<BadRequestObjectResult>(await controller.GetSummariesByCountyIdsAsync([.. System.Linq.Enumerable.Range(0, Constants.OrtoDatas.MaximumSummaryCountyCount + 1)]));
                Assert.IsType<BadRequestObjectResult>(await controller.GetQueueSummariesByCountyIdsAsync([.. System.Linq.Enumerable.Range(0, Constants.OrtoDatas.MaximumSummaryCountyCount + 1)]));

                // The existing endpoints, for the same reason: an empty body has nothing to check.
                Assert.IsType<BadRequestObjectResult>(await controller.ContainsByReferencesAsync(null, null, null));
                Assert.IsType<BadRequestObjectResult>(await controller.ContainsByReferencesAsync([" "], null, null));
                Assert.IsType<BadRequestObjectResult>(await controller.NextBuilding2DReferencesAsync(0));
                Assert.IsType<BadRequestObjectResult>(await controller.NextBuilding2DReferencesAsync(10, 0));
                Assert.IsType<BadRequestObjectResult>(await controller.NextBuilding2DReferencesAsync(10, -1));
                Assert.IsType<BadRequestObjectResult>(await controller.AcknowledgeBuilding2DReferencesAsync(null));
                Assert.IsType<BadRequestObjectResult>(await controller.AcknowledgeBuilding2DReferencesAsync([]));
                Assert.IsType<BadRequestObjectResult>(await controller.GetItemByReferenceAsync(" "));
                Assert.IsType<BadRequestObjectResult>(await controller.GetImageByReferenceAsync(" ", 2024));

                // OrtoDatasReference endpoints validation
                Assert.IsType<BadRequestResult>(await controller.GetOrtoDatasReferenceByReferenceAsync(string.Empty));
                Assert.IsType<BadRequestResult>(await controller.GetOrtoDatasReferenceByReferenceAsync("  "));
                Assert.IsType<BadRequestResult>(await controller.GetOrtoDatasReferencesByReferencesAsync(null!));
                Assert.IsType<BadRequestResult>(await controller.GetOrtoDatasReferencesByReferencesAsync([]));
                Assert.IsType<BadRequestResult>(await controller.GetOrtoDatasReferencesByBuilding2DReferencesAsync(null!));
                Assert.IsType<BadRequestResult>(await controller.GetOrtoDatasReferencesByBuilding2DReferencesAsync([]));
                Assert.IsType<BadRequestResult>(await controller.GetOrtoDatasReferencesByCountyIdAsync(0));
                Assert.IsType<BadRequestResult>(await controller.GetOrtoDatasReferencesByCountyIdAsync(-1));

                // Estimated coverage endpoints validation
                Assert.IsType<BadRequestResult>(await controller.GetEstimatedCoverageFactorAsync(0));
                Assert.IsType<BadRequestResult>(await controller.GetEstimatedCoverageFactorAsync(-1));
                Assert.IsType<BadRequestResult>(await controller.GetEstimatedCoverageFactorsAsync(null!, null));
                Assert.IsType<BadRequestResult>(await controller.GetEstimatedCoverageFactorsAsync([], null));

                // commandTimeout: a negative value is refused by every action's own guard, ahead of the
                // lookup or the ceiling that guard precedes (DiGi.GIS.WebAPI#48).
                Assert.IsType<BadRequestResult>(await controller.GetEstimatedCoverageFactorAsync(1, -1));
                Assert.IsType<BadRequestResult>(await controller.GetEstimatedCoverageFactorsAsync([1], null, -1));
                Assert.IsType<BadRequestResult>(await controller.GetSummariesByCountyIdsAsync(null, -1));
                Assert.IsType<BadRequestResult>(await controller.GetQueueSummariesByCountyIdsAsync(null, -1));
                Assert.IsType<BadRequestResult>(await controller.GetSubdivisionLinksByCountyIdAsync(55417, 20, -1));
                Assert.IsType<BadRequestResult>(await controller.NextBuilding2DReferencesAsync(1, 1, 5, -1));
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        /// <summary>
        /// Verifies that a sample count and a county count sitting exactly on their ceilings, and a commandTimeout of 0 - the value that disables the timeout - are not refused by the guards (ZiolkowskiJakub/DiGi.GIS.WebAPI#48).
        /// <para>Nothing here reaches a database - the converters have no connection data, so each call answers 404 or 500 once past validation. What is being asserted is only that the guard let it through.</para>
        /// </summary>
        [Fact]
        public async Task OrtoDatasController_Validation_AcceptsBoundary()
        {
            string path = ConfigurationFilePath();

            try
            {
                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher = new(path);
                OrtoDatasController controller = new(gISWebAPIConfigurationFileWatcher, new PostgreSQL.Classes.OrtoDatasPostgreSQLConverter(null), new PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));

                Assert.IsNotType<BadRequestObjectResult>(await controller.GetSubdivisionLinksByCountyIdAsync(55417, Constants.OrtoDatas.MaximumSampleCount));
                Assert.IsNotType<BadRequestObjectResult>(await controller.GetSubdivisionLinksByCountyIdAsync(55417, 0));
                Assert.IsNotType<BadRequestObjectResult>(await controller.GetSummariesByCountyIdsAsync([.. System.Linq.Enumerable.Range(0, Constants.OrtoDatas.MaximumSummaryCountyCount)]));
                Assert.IsNotType<BadRequestObjectResult>(await controller.GetSummariesByCountyIdsAsync(null));
                Assert.IsNotType<BadRequestObjectResult>(await controller.GetQueueSummariesByCountyIdsAsync(null));
                Assert.IsNotType<BadRequestObjectResult>(await controller.NextBuilding2DReferencesAsync(1, 1));

                // The claim is the one endpoint whose DDL can need a real timeout, so the parameter must exist
                // and must be accepted. Pre-fix this line does not compile - the signature is the defect.
                Assert.IsNotType<BadRequestObjectResult>(await controller.NextBuilding2DReferencesAsync(1, 1, 600));

                // commandTimeout: 0 disables the timeout and passes the same guards - the other side of
                // every boundary asserted above (DiGi.GIS.WebAPI#48). Past the guard these answer 404 or
                // 204; a guard that refused 0 would answer the guard's plain 400 instead.
                Assert.IsNotType<BadRequestResult>(await controller.GetSummariesByCountyIdsAsync(null, 0));
                Assert.IsNotType<BadRequestResult>(await controller.GetQueueSummariesByCountyIdsAsync(null, 0));
                Assert.IsNotType<BadRequestResult>(await controller.GetSubdivisionLinksByCountyIdAsync(55417, 20, 0));
                Assert.IsNotType<BadRequestResult>(await controller.NextBuilding2DReferencesAsync(1, 1, 5, 0));

                Assert.Equal(500, Constants.OrtoDatas.MaximumCoverageCountyCount);
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        /// <summary>
        /// Asserts the commandTimeout boundary on the two estimated-coverage actions, whose lookup miss answers BadRequest as well - so a status comparison alone cannot tell the guard's refusal from the miss it precedes.
        /// <para>Driven against a dead loopback host, the two sides separate: -1 is refused by the guard before a connection is opened, while 0 runs past it into the lookup, where OpenAsync throws a genuine NpgsqlException. A guard that refused 0 - or one placed after the lookup - would answer BadRequest instead of throwing, so neither half passes vacuously (ZiolkowskiJakub/DiGi.GIS.WebAPI#48).</para>
        /// </summary>
        [Fact]
        public async Task OrtoDatasController_CoverageEstimate_CommandTimeoutBoundaries()
        {
            string path = ConfigurationFilePath();

            try
            {
                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher = new(path);
                DiGi.PostgreSQL.Classes.ConnectionData connectionData = new("127.0.0.1", "user", "pass", "db", 1);
                OrtoDatasController controller = new(gISWebAPIConfigurationFileWatcher, new PostgreSQL.Classes.OrtoDatasPostgreSQLConverter(connectionData), new PostgreSQL.Classes.Building2DPostgreSQLConverter(connectionData), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(connectionData));

                Assert.IsType<BadRequestResult>(await controller.GetEstimatedCoverageFactorAsync(1, -1));
                await Assert.ThrowsAnyAsync<NpgsqlException>(() => controller.GetEstimatedCoverageFactorAsync(1, 0));

                Assert.IsType<BadRequestResult>(await controller.GetEstimatedCoverageFactorsAsync([1], null, -1));
                await Assert.ThrowsAnyAsync<NpgsqlException>(() => controller.GetEstimatedCoverageFactorsAsync([1], null, 0));
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        /// <summary>
        /// Verifies that <see cref="Constants.OrtoDatas.MaximumCoverageCountyCount"/> is set to 500 to allow nation-wide coverage queries.
        /// </summary>
        [Fact]
        public void OrtoDatas_MaximumCoverageCountyCount_IsFiveHundred()
        {
            Assert.Equal(500, Constants.OrtoDatas.MaximumCoverageCountyCount);
        }

        /// <summary>
        /// A transient database failure is a "retry can succeed" class, so it must answer 503 with a Retry-After header rather than the uniform 500.
        /// <para>Drives a real transient failure through the real stack with no server: a dead loopback host makes the converter's OpenAsync throw a genuine NpgsqlException, which Npgsql classifies IsTransient = true. Every read action of <see cref="OrtoDatasController"/> is asserted, so a 503 mapping added to one action and forgotten in a sibling cannot pass.</para>
        /// <para>Red before the 503 mapping existed (the path answered 500); green after.</para>
        /// <para>Medium test (10.2 s): runs when DIGI_TEST_MAX_DURATION is Medium (the default) or Long.</para>
        /// </summary>
        [MediumFact]
        public async Task OrtoDatasController_TransientDatabaseFailure_Answers503()
        {
            string path = ConfigurationFilePath();

            try
            {
                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher = new(path);
                OrtoDatasController controller = new(gISWebAPIConfigurationFileWatcher, new PostgreSQL.Classes.OrtoDatasPostgreSQLConverter(new DiGi.PostgreSQL.Classes.ConnectionData("127.0.0.1", "user", "pass", "db", 1)), new PostgreSQL.Classes.Building2DPostgreSQLConverter(new DiGi.PostgreSQL.Classes.ConnectionData("127.0.0.1", "user", "pass", "db", 1)), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));
                controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

                AssertTransient503(controller, await controller.ContainsByReferencesAsync(["ref1"], null, null));
                AssertTransient503(controller, await controller.GetCountByCountyIdAsync(1));
                AssertTransient503(controller, await controller.GetSummariesByCountyIdsAsync(null));
                AssertTransient503(controller, await controller.GetSubdivisionLinksByCountyIdAsync(1));
                AssertTransient503(controller, await controller.GetQueueSummariesByCountyIdsAsync(null));
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }
    }
}
