using DiGi.Analytical.Building.Classes;
using DiGi.GIS.WebAPI.UI.Classes;
using DiGi.GIS.WebAPI.UI.Controllers;
using DiGi.GIS.WebAPI.UI.HostedServices;
using DiGi.GIS.WebAPI.UI.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that background jobs and synchronous requests share one solve gate and never solve at the same time.
        /// <para>While a synchronous solve holds the gate, a job stays queued at position 1 and its calculation does not start. While the job calculates, a synchronous request is refused with a 503 and a <c>Retry-After</c> once its (shortened) wait for the gate runs out, instead of waiting for the job to finish; after the job, the same request is answered.</para>
        /// </summary>
        [Fact]
        public async Task Modify_SolveAsync_SharedGate()
        {
            SemaphoreSlim semaphoreSlim = new(1, 1);
            SolarJobQueue solarJobQueue = new(TimeProvider.System);
            SolarJobHostedService solarJobHostedService = new(solarJobQueue, semaphoreSlim, NullLogger<SolarJobHostedService>.Instance);

            BuildingModel buildingModel = SolarFixture_BuildingModel(SolarFixture_BoxComponents(SolarFixture_Origin.X, SolarFixture_Origin.Y, 10), new Geometry.Spatial.Classes.Point3D(SolarFixture_Origin.X + 5, SolarFixture_Origin.Y + 5, 5));
            SolarController solarController = SolarController_Controller(SolarController_ScriptedWebApi(buildingModel, [buildingModel], SolarFixture_EPWFile()), semaphoreSlim, solarJobQueue);
            solarController.SolveGateWait = TimeSpan.FromMilliseconds(200);

            using ManualResetEventSlim manualResetEventSlim_Started = new(false);
            using ManualResetEventSlim manualResetEventSlim_Release = new(false);
            SolarJob solarJob = SolarJobFixture_Job(() =>
            {
                manualResetEventSlim_Started.Set();
                manualResetEventSlim_Release.Wait(TimeSpan.FromSeconds(30));
                return [];
            });

            await solarJobHostedService.StartAsync(CancellationToken.None);
            try
            {
                // A synchronous solve holds the gate: the job waits, queued, and never starts.
                await semaphoreSlim.WaitAsync();
                Assert.True(solarJobQueue.TryEnqueue(solarJob));
                await Task.Delay(300);
                SolarJobViewModel solarJobViewModel_Waiting = Assert.IsType<SolarJobViewModel>(solarJobQueue.SolarJobViewModel(solarJob.Id));
                Assert.Equal(nameof(Enums.SolarJobStatus.Queued), solarJobViewModel_Waiting.Status);
                Assert.Equal(1, solarJobViewModel_Waiting.QueuePosition);
                Assert.False(manualResetEventSlim_Started.IsSet);

                // Released: the job takes the gate and calculates.
                semaphoreSlim.Release();
                Assert.True(manualResetEventSlim_Started.Wait(TimeSpan.FromSeconds(10)));
                SolarJobViewModel solarJobViewModel_Running = await SolarJobFixture_WaitAsync(solarJobQueue, solarJob.Id, x => x.Status == nameof(Enums.SolarJobStatus.Running));
                Assert.Equal(0, solarJobViewModel_Running.QueuePosition);

                // A synchronous request meanwhile is refused, with the time to retry after.
                IActionResult actionResult_Busy = await solarController.GetRadiationByBuildingModelIdAsync(7, 1465, null);
                SolarController_AssertStatus(actionResult_Busy, StatusCodes.Status503ServiceUnavailable);
                Assert.Equal(Constants.Default.SolarSolveGateWaitSeconds.ToString(CultureInfo.InvariantCulture), solarController.Response.Headers.RetryAfter.ToString());

                // After the job, the request is answered.
                manualResetEventSlim_Release.Set();
                await SolarJobFixture_WaitAsync(solarJobQueue, solarJob.Id, x => x.Status == nameof(Enums.SolarJobStatus.Completed));

                IActionResult actionResult_Free = await solarController.GetRadiationByBuildingModelIdAsync(7, 1465, null);
                Assert.IsType<ContentResult>(actionResult_Free);
            }
            finally
            {
                manualResetEventSlim_Release.Set();
                await solarJobHostedService.StopAsync(CancellationToken.None);
            }
        }
    }
}
