using DiGi.GIS.WebAPI.Classes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the GET <c>referencepathsbyname</c> and <c>referencesbyname</c> actions return the same
        /// results as their POST siblings for both invalid input (<see cref="BadRequestResult"/>) and
        /// no-result cases (<see cref="NotFoundResult"/>), confirming both delegate to the same converter method.
        /// </summary>
        [Fact]
        public async Task GetReferencesByName_ParityWithPost_SameValidation()
        {
            string path = ConfigurationFilePath();

            try
            {
                using GISWebAPIConfigurationFileWatcher gISWebAPIConfigurationFileWatcher = new(path);
                AdministrativeAreal2DController controller = new(gISWebAPIConfigurationFileWatcher, new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));

                // Both GET and POST return BadRequest for empty text
                Assert.IsType<BadRequestResult>(await controller.GetReferencePathsByNameAsync(string.Empty));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencePathsByNameParameterAsync(new AdministrativeAreal2DReferencePathsByNameParameter(string.Empty)));

                Assert.IsType<BadRequestResult>(await controller.GetReferencesByNameAsync(string.Empty));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencesByNameParameterAsync(new AdministrativeAreal2DReferencesByNameParameter(string.Empty)));

                // Both GET and POST return BadRequest for whitespace text
                Assert.IsType<BadRequestResult>(await controller.GetReferencePathsByNameAsync("   "));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencePathsByNameParameterAsync(new AdministrativeAreal2DReferencePathsByNameParameter("   ")));

                Assert.IsType<BadRequestResult>(await controller.GetReferencesByNameAsync("   "));
                Assert.IsType<BadRequestResult>(await controller.GetAdministrativeAreal2DReferencesByNameParameterAsync(new AdministrativeAreal2DReferencesByNameParameter("   ")));

                // Both GET and POST return NotFound when the converter cannot produce a result (no database in unit test)
                Assert.IsType<NotFoundResult>(await controller.GetReferencePathsByNameAsync("Warszawa"));
                Assert.IsType<NotFoundResult>(await controller.GetAdministrativeAreal2DReferencePathsByNameParameterAsync(new AdministrativeAreal2DReferencePathsByNameParameter("Warszawa")));

                Assert.IsType<NotFoundResult>(await controller.GetReferencesByNameAsync("Warszawa"));
                Assert.IsType<NotFoundResult>(await controller.GetAdministrativeAreal2DReferencesByNameParameterAsync(new AdministrativeAreal2DReferencesByNameParameter("Warszawa")));
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }
    }
}
