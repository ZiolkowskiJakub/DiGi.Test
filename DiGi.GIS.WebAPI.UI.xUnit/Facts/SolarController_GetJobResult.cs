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
        /// Tests a background solar radiation job end to end on a scripted GIS Web API and the real calculation: before it runs, its results and scene answer 409; once its consumer has run it, the results are the JSON the synchronous route answers for the same building, and the scene is a binary glTF built from them without solving again. An unknown job answers 404.
        /// </summary>
        [Fact]
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
            SolarController_AssertStatus(solarController.GetJobGLB(id), StatusCodes.Status409Conflict);

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

            // The scene, from the stored results: no further upstream request is sent for it.
            int count = scriptedWebApi.Requests.Count;
            FileContentResult fileContentResult = Assert.IsType<FileContentResult>(solarController.GetJobGLB(id));
            Assert.Equal("model/gltf-binary", fileContentResult.ContentType);
            Assert.True(fileContentResult.FileContents.Length > 0);
            Assert.Equal(count, scriptedWebApi.Requests.Count);

            // The calculation and its inputs are released; the building and neighbours are kept for the scene.
            Assert.Null(solarJob.Calculation);
            Assert.NotNull(solarJob.BuildingModel);
            Assert.NotNull(solarJob.BuildingModels_Surrounding);

            SolarController_AssertStatus(solarController.GetJobResult(Guid.NewGuid()), StatusCodes.Status404NotFound);
            SolarController_AssertStatus(solarController.GetJobGLB(Guid.NewGuid()), StatusCodes.Status404NotFound);
        }
    }
}
