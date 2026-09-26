using DiGi.Analytical.Building.Classes;
using DiGi.GIS.WebAPI.UI.Classes;
using DiGi.GIS.WebAPI.UI.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the expiry of background solar radiation jobs on both sides of <see cref="Constants.Default.SolarJobResultRetentionMinutes"/>: a completed job is still answered one second before the retention has passed since it finished and answers 404 one second after, while a queued job never expires.
        /// </summary>
        [Fact]
        public async Task Query_SolarJob_Expiry()
        {
            ManualTimeProvider manualTimeProvider = new();
            SolarJobQueue solarJobQueue = new(manualTimeProvider);

            BuildingModel buildingModel = SolarFixture_Box();
            SolarController solarController = SolarController_Controller(SolarController_ScriptedWebApi(buildingModel, [buildingModel], SolarFixture_EPWFile()), null, solarJobQueue);

            SolarJob solarJob_Completed = SolarJobFixture_Job(() => []);
            Assert.True(solarJobQueue.TryEnqueue(solarJob_Completed));

            // Queued before the other one finishes, and never run.
            SolarJob solarJob_Queued = SolarJobFixture_Job(() => []);
            Assert.True(solarJobQueue.TryEnqueue(solarJob_Queued));

            manualTimeProvider.Advance(TimeSpan.FromMinutes(5));
            await solarJobQueue.SolveAsync(solarJob_Completed, new SemaphoreSlim(1, 1));
            Assert.Equal(manualTimeProvider.GetUtcNow(), solarJob_Completed.FinishedAt);

            TimeSpan timeSpan_Retention = TimeSpan.FromMinutes(Constants.Default.SolarJobResultRetentionMinutes);

            manualTimeProvider.Advance(timeSpan_Retention - TimeSpan.FromSeconds(1));
            Assert.NotNull(solarJobQueue.SolarJob(solarJob_Completed.Id));
            Assert.IsType<OkObjectResult>(solarController.GetJob(solarJob_Completed.Id));
            Assert.IsType<ContentResult>(solarController.GetJobResult(solarJob_Completed.Id));

            manualTimeProvider.Advance(TimeSpan.FromSeconds(2));
            Assert.Null(solarJobQueue.SolarJob(solarJob_Completed.Id));
            SolarController_AssertStatus(solarController.GetJob(solarJob_Completed.Id), 404);
            SolarController_AssertStatus(solarController.GetJobResult(solarJob_Completed.Id), 404);
            SolarController_AssertStatus(solarController.DeleteJob(solarJob_Completed.Id), 404);

            // A queued job has not finished, so it does not expire however long it waits.
            manualTimeProvider.Advance(TimeSpan.FromDays(1));
            Assert.NotNull(solarJobQueue.SolarJob(solarJob_Queued.Id));
            Assert.Equal(1, solarJobQueue.QueuePosition(solarJob_Queued.Id));
        }
    }
}
