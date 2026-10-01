using DiGi.Analytical.Building.Classes;
using DiGi.GIS.WebAPI.UI.Classes;
using DiGi.GIS.WebAPI.UI.Controllers;
using DiGi.GIS.WebAPI.UI.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests a background solar radiation job end to end on a scripted GIS Web API and the real calculation: before it runs, its results and view answer 409; once its consumer has run it, the results are the JSON the synchronous route answers for the same building, and the view - built from them without solving again - matches the synchronous view route surface by surface and names the weather station. An unknown job answers 404.
        /// <para>Medium test (6.7 s): runs when DIGI_TEST_MAX_DURATION is Medium (the default) or Long.</para>
        /// </summary>
        [MediumFact]
        public async Task SolarController_GetJobResult()
        {
            BuildingModel buildingModel = SolarFixture_Box();
            ScriptedWebApi scriptedWebApi = SolarController_ScriptedWebApi(buildingModel, [buildingModel], SolarFixture_EPWFile());
            SolarJobQueue solarJobQueue = new(TimeProvider.System);
            SemaphoreSlim semaphoreSlim = new(1, 1);
            SolarController solarController = SolarController_Controller(scriptedWebApi, semaphoreSlim, solarJobQueue);

            AcceptedResult acceptedResult = Assert.IsType<AcceptedResult>(await solarController.PostJobAsync(7, 1465, 20));
            Guid id = Guid.Parse(Assert.IsType<SolarJobViewModel>(acceptedResult.Value).JobId);

            SolarController_AssertStatus(solarController.GetJobResult(id), StatusCodes.Status409Conflict);
            SolarController_AssertStatus(solarController.GetJobView(id), StatusCodes.Status409Conflict);

            SolarJob solarJob = Assert.IsType<SolarJob>(solarJobQueue.SolarJob(id));
            await solarJobQueue.SolveAsync(solarJob, semaphoreSlim);
            Assert.Equal(nameof(Enums.SolarJobStatus.Completed), Assert.IsType<SolarJobViewModel>(Assert.IsType<OkObjectResult>(solarController.GetJob(id)).Value).Status);

            // The same results as the synchronous route for the same building and radius.
            ContentResult contentResult = Assert.IsType<ContentResult>(solarController.GetJobResult(id));
            Assert.Equal("application/json", contentResult.ContentType);
            ContentResult contentResult_Synchronous = Assert.IsType<ContentResult>(await solarController.GetRadiationByBuildingModelIdAsync(7, 1465, 20));

            List<SurfaceSolarRadiationResult>? surfaceSolarRadiationResults = Core.Convert.ToDiGi<SurfaceSolarRadiationResult>(contentResult.Content);
            List<SurfaceSolarRadiationResult>? surfaceSolarRadiationResults_Synchronous = Core.Convert.ToDiGi<SurfaceSolarRadiationResult>(contentResult_Synchronous.Content);
            Assert.NotNull(surfaceSolarRadiationResults);
            Assert.NotNull(surfaceSolarRadiationResults_Synchronous);
            Assert.Equal(5, surfaceSolarRadiationResults.Count);
            Assert.Equal(surfaceSolarRadiationResults_Synchronous.Count, surfaceSolarRadiationResults.Count);
            for (int i = 0; i < surfaceSolarRadiationResults.Count; i++)
            {
                Assert.Equal(surfaceSolarRadiationResults_Synchronous[i].Reference, surfaceSolarRadiationResults[i].Reference);
                Assert.Equal(surfaceSolarRadiationResults_Synchronous[i].Irradiation, surfaceSolarRadiationResults[i].Irradiation, 9);
            }

            // The view, from the stored results: no further upstream request is sent for it.
            int count = scriptedWebApi.Requests.Count;
            SolarRadiationViewModel solarRadiationViewModel = Assert.IsType<SolarRadiationViewModel>(Assert.IsType<OkObjectResult>(solarController.GetJobView(id)).Value);
            Assert.Equal(count, scriptedWebApi.Requests.Count);
            Assert.Equal(20, solarRadiationViewModel.Radius);
            Assert.False(string.IsNullOrWhiteSpace(solarRadiationViewModel.StationName));
            Assert.StartsWith("/epwfile/item?x=", solarRadiationViewModel.StationUrl);

            SolarRadiationViewModel solarRadiationViewModel_Synchronous = Assert.IsType<SolarRadiationViewModel>(Assert.IsType<OkObjectResult>(await solarController.GetViewByBuildingModelIdAsync(7, 1465, 20)).Value);
            Assert.Equal(5, solarRadiationViewModel.Surfaces.Count);
            Assert.Equal(solarRadiationViewModel_Synchronous.Surfaces.Count, solarRadiationViewModel.Surfaces.Count);
            for (int i = 0; i < solarRadiationViewModel.Surfaces.Count; i++)
            {
                Assert.Equal(solarRadiationViewModel_Synchronous.Surfaces[i].Reference, solarRadiationViewModel.Surfaces[i].Reference);
                Assert.Equal(solarRadiationViewModel_Synchronous.Surfaces[i].Color, solarRadiationViewModel.Surfaces[i].Color);
            }

            Assert.Equal(solarRadiationViewModel_Synchronous.StationName, solarRadiationViewModel.StationName);
            Assert.Equal(solarRadiationViewModel_Synchronous.StationUrl, solarRadiationViewModel.StationUrl);

            // The calculation and its inputs are released; the building is kept for the view.
            Assert.Null(solarJob.Calculation);
            Assert.NotNull(solarJob.BuildingModel);

            SolarController_AssertStatus(solarController.GetJobResult(Guid.NewGuid()), StatusCodes.Status404NotFound);
            SolarController_AssertStatus(solarController.GetJobView(Guid.NewGuid()), StatusCodes.Status404NotFound);
        }
    }
}
