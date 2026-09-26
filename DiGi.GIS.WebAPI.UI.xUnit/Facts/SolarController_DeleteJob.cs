using DiGi.Analytical.Building.Classes;
using DiGi.GIS.WebAPI.UI.Classes;
using DiGi.GIS.WebAPI.UI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the cancellation of background solar radiation jobs: a queued job is never calculated; a running calculation, which cannot be interrupted, runs to its end and its results are discarded, so the job stays Cancelled and its results answer 409. An unknown job answers 404.
        /// </summary>
        [Fact]
        public async Task SolarController_DeleteJob()
        {
            BuildingModel buildingModel = SolarFixture_Box();
            SolarJobQueue solarJobQueue = new(TimeProvider.System);
            SemaphoreSlim semaphoreSlim = new(1, 1);
            SolarController solarController = SolarController_Controller(SolarController_ScriptedWebApi(buildingModel, [buildingModel], SolarFixture_EPWFile()), semaphoreSlim, solarJobQueue);

            // Queued: never calculated.
            int count = 0;
            SolarJob solarJob_Queued = SolarJobFixture_Job(() =>
            {
                Interlocked.Increment(ref count);
                return [];
            });
            Assert.True(solarJobQueue.TryEnqueue(solarJob_Queued));

            Assert.IsType<NoContentResult>(solarController.DeleteJob(solarJob_Queued.Id));
            Assert.Equal(nameof(Enums.SolarJobStatus.Cancelled), solarJobQueue.SolarJobViewModel(solarJob_Queued.Id)?.Status);

            await solarJobQueue.SolveAsync(solarJob_Queued, semaphoreSlim);
            Assert.Equal(0, count);
            Assert.Null(solarJob_Queued.Calculation);
            Assert.Equal(nameof(Enums.SolarJobStatus.Cancelled), solarJobQueue.SolarJobViewModel(solarJob_Queued.Id)?.Status);

            // Running: finishes, and its results are discarded.
            using ManualResetEventSlim manualResetEventSlim_Started = new(false);
            using ManualResetEventSlim manualResetEventSlim_Release = new(false);
            SolarJob solarJob_Running = SolarJobFixture_Job(() =>
            {
                manualResetEventSlim_Started.Set();
                manualResetEventSlim_Release.Wait(TimeSpan.FromSeconds(30));
                return [];
            });
            Assert.True(solarJobQueue.TryEnqueue(solarJob_Running));

            Task task = Task.Run(() => solarJobQueue.SolveAsync(solarJob_Running, semaphoreSlim));
            Assert.True(manualResetEventSlim_Started.Wait(TimeSpan.FromSeconds(10)));

            Assert.IsType<NoContentResult>(solarController.DeleteJob(solarJob_Running.Id));
            Assert.Equal(nameof(Enums.SolarJobStatus.Cancelled), solarJobQueue.SolarJobViewModel(solarJob_Running.Id)?.Status);
            Assert.Null(solarJob_Running.FinishedAt);

            manualResetEventSlim_Release.Set();
            await task;

            Assert.Equal(nameof(Enums.SolarJobStatus.Cancelled), solarJobQueue.SolarJobViewModel(solarJob_Running.Id)?.Status);
            Assert.Null(solarJob_Running.SurfaceSolarRadiationResults);
            Assert.NotNull(solarJob_Running.FinishedAt);
            SolarController_AssertStatus(solarController.GetJobResult(solarJob_Running.Id), StatusCodes.Status409Conflict);
            SolarController_AssertStatus(solarController.GetJobGLB(solarJob_Running.Id), StatusCodes.Status409Conflict);

            // Unknown.
            SolarController_AssertStatus(solarController.DeleteJob(Guid.NewGuid()), StatusCodes.Status404NotFound);
        }
    }
}
