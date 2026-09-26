using DiGi.Analytical.Building.Classes;
using DiGi.GIS.WebAPI.UI.Classes;
using DiGi.GIS.WebAPI.UI.Enums;
using DiGi.GIS.WebAPI.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the states of a background solar radiation job: Queued when queued, Running while its calculation runs, Completed with the calculated results; Failed with the exception type and message when the calculation throws, and Failed with a message when it gives no result. A finished job has released its calculation, and a failed one its building and neighbours too, which only a completed job's scene needs.
        /// </summary>
        [Fact]
        public async Task Modify_SolveAsync_Status()
        {
            SolarJobQueue solarJobQueue = new(TimeProvider.System);
            SemaphoreSlim semaphoreSlim = new(1, 1);

            // Queued -> Running -> Completed.
            List<SurfaceSolarRadiationResult> surfaceSolarRadiationResults = [];
            SolarJobStatus? solarJobStatus_During = null;
            SolarJob solarJob = null!;
            solarJob = SolarJobFixture_Job(() =>
            {
                solarJobStatus_During = solarJob.Status;
                return surfaceSolarRadiationResults;
            });

            Assert.True(solarJobQueue.TryEnqueue(solarJob));
            Assert.Equal(nameof(SolarJobStatus.Queued), solarJobQueue.SolarJobViewModel(solarJob.Id)?.Status);

            await solarJobQueue.SolveAsync(solarJob, semaphoreSlim);

            Assert.Equal(SolarJobStatus.Running, solarJobStatus_During);
            SolarJobViewModel solarJobViewModel = Assert.IsType<SolarJobViewModel>(solarJobQueue.SolarJobViewModel(solarJob.Id));
            Assert.Equal(nameof(SolarJobStatus.Completed), solarJobViewModel.Status);
            Assert.Null(solarJobViewModel.QueuePosition);
            Assert.Null(solarJobViewModel.Error);
            Assert.Same(surfaceSolarRadiationResults, solarJob.SurfaceSolarRadiationResults);
            Assert.Null(solarJob.Calculation);
            Assert.NotNull(solarJob.StartedAt);
            Assert.NotNull(solarJob.FinishedAt);
            Assert.Equal(1, semaphoreSlim.CurrentCount);

            // A throwing calculation: Failed, with the exception, and the gate released.
            BuildingModel buildingModel = SolarFixture_Box();
            SolarJob solarJob_Throw = new(Guid.NewGuid(), 7, 1465, Constants.Default.SolarSurroundingRadius, 5, buildingModel, [buildingModel], () => throw new InvalidOperationException("Shading model has no receivers."));
            Assert.True(solarJobQueue.TryEnqueue(solarJob_Throw));
            await solarJobQueue.SolveAsync(solarJob_Throw, semaphoreSlim);

            SolarJobViewModel solarJobViewModel_Throw = Assert.IsType<SolarJobViewModel>(solarJobQueue.SolarJobViewModel(solarJob_Throw.Id));
            Assert.Equal(nameof(SolarJobStatus.Failed), solarJobViewModel_Throw.Status);
            Assert.Equal("InvalidOperationException: Shading model has no receivers.", solarJobViewModel_Throw.Error);
            Assert.Null(solarJob_Throw.SurfaceSolarRadiationResults);
            Assert.Null(solarJob_Throw.Calculation);
            Assert.Null(solarJob_Throw.BuildingModel);
            Assert.Null(solarJob_Throw.BuildingModels_Surrounding);
            Assert.Equal(1, semaphoreSlim.CurrentCount);

            // No result: Failed, with a message naming the building.
            SolarJob solarJob_Null = SolarJobFixture_Job(() => null, 11126205);
            Assert.True(solarJobQueue.TryEnqueue(solarJob_Null));
            await solarJobQueue.SolveAsync(solarJob_Null, semaphoreSlim);

            SolarJobViewModel solarJobViewModel_Null = Assert.IsType<SolarJobViewModel>(solarJobQueue.SolarJobViewModel(solarJob_Null.Id));
            Assert.Equal(nameof(SolarJobStatus.Failed), solarJobViewModel_Null.Status);
            Assert.Contains("11126205", solarJobViewModel_Null.Error);
        }
    }
}
