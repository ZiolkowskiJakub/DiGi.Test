using DiGi.Analytical.Building.Classes;
using DiGi.GIS.WebAPI.UI.Classes;
using DiGi.GIS.WebAPI.UI.Controllers;
using DiGi.GIS.WebAPI.UI.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the queue limit of the background solar radiation jobs: with no consumer running, <see cref="Constants.Default.SolarJobQueueLengthMax"/> posts are accepted with 202, a <c>Location</c> and increasing queue positions; the next is refused with a 503 and a <c>Retry-After</c> of <see cref="Constants.Default.SolarJobRetryAfterSeconds"/> before any upstream request is sent; cancelling a queued job frees its place at once and releases its models.
        /// </summary>
        [Fact]
        public async Task SolarController_PostJobAsync_QueueFull()
        {
            BuildingModel buildingModel = SolarFixture_Box();
            ScriptedWebApi scriptedWebApi = SolarController_ScriptedWebApi(buildingModel, [buildingModel], SolarFixture_EPWFile());
            SolarJobQueue solarJobQueue = new(TimeProvider.System);

            List<Guid> ids = [];
            for (int i = 1; i <= Constants.Default.SolarJobQueueLengthMax; i++)
            {
                IActionResult actionResult = await SolarController_Controller(scriptedWebApi, null, solarJobQueue).PostJobAsync(7, 1465, null);
                AcceptedResult acceptedResult = Assert.IsType<AcceptedResult>(actionResult);
                SolarJobViewModel solarJobViewModel = Assert.IsType<SolarJobViewModel>(acceptedResult.Value);
                Assert.Equal(nameof(Enums.SolarJobStatus.Queued), solarJobViewModel.Status);
                Assert.Equal(i, solarJobViewModel.QueuePosition);
                Assert.Equal(5, solarJobViewModel.ReceiverCount);
                Assert.Equal($"/solar/jobs/{solarJobViewModel.JobId}", acceptedResult.Location);
                ids.Add(Guid.Parse(solarJobViewModel.JobId));
            }

            int count = scriptedWebApi.Requests.Count;
            SolarController solarController_Full = SolarController_Controller(scriptedWebApi, null, solarJobQueue);
            SolarController_AssertStatus(await solarController_Full.PostJobAsync(7, 1465, null), StatusCodes.Status503ServiceUnavailable);
            Assert.Equal(Constants.Default.SolarJobRetryAfterSeconds.ToString(CultureInfo.InvariantCulture), solarController_Full.Response.Headers.RetryAfter.ToString());
            Assert.Equal(count, scriptedWebApi.Requests.Count);

            // A cancelled job leaves the queue at once: its place is free, and the jobs behind it move up.
            Assert.NotNull(solarJobQueue.SolarJob(ids[0])?.BuildingModel);
            Assert.IsType<NoContentResult>(SolarController_Controller(scriptedWebApi, null, solarJobQueue).DeleteJob(ids[0]));
            Assert.Equal(1, solarJobQueue.QueuePosition(ids[1]));
            SolarJob solarJob_Cancelled = Assert.IsType<SolarJob>(solarJobQueue.SolarJob(ids[0]));
            Assert.Null(solarJob_Cancelled.BuildingModel);
            Assert.Null(solarJob_Cancelled.BuildingModels_Surrounding);
            Assert.IsType<AcceptedResult>(await SolarController_Controller(scriptedWebApi, null, solarJobQueue).PostJobAsync(7, 1465, null));
        }
    }
}
