using DiGi.GIS.WebAPI.Classes;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the read endpoints added for verifying the subdivision links reject what they cannot act on, without touching a database.
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
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        /// <summary>
        /// Verifies that a sample count and a county count sitting exactly on their ceilings are not refused by the guards.
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
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }
    }
}
