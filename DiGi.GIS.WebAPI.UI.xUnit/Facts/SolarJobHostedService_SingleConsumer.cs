using DiGi.GIS.WebAPI.UI.Classes;
using DiGi.GIS.WebAPI.UI.HostedServices;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that the background solar radiation jobs run one after the other, in the order they were queued: three jobs queued at once, each calculating for 200 ms, never overlap - each starts after the previous one finished, and at most one calculation is ever active.
        /// </summary>
        [Fact]
        public async Task SolarJobHostedService_SingleConsumer()
        {
            SolarJobQueue solarJobQueue = new(TimeProvider.System);
            SolarJobHostedService solarJobHostedService = new(solarJobQueue, new SemaphoreSlim(1, 1), NullLogger<SolarJobHostedService>.Instance);

            Stopwatch stopwatch = Stopwatch.StartNew();
            Lock @lock = new();
            List<(long Id, long Start, long Finish)> intervals = [];
            int active = 0;
            int activeMax = 0;

            List<SolarJob> solarJobs = [];
            for (int i = 1; i <= Constants.Default.SolarJobQueueLengthMax; i++)
            {
                long buildingModelId = i;
                solarJobs.Add(SolarJobFixture_Job(() =>
                {
                    long start = stopwatch.ElapsedTicks;
                    int active_Current = Interlocked.Increment(ref active);
                    lock (@lock)
                    {
                        activeMax = Math.Max(activeMax, active_Current);
                    }

                    Thread.Sleep(200);

                    Interlocked.Decrement(ref active);
                    lock (@lock)
                    {
                        intervals.Add((buildingModelId, start, stopwatch.ElapsedTicks));
                    }

                    return [];
                }, buildingModelId));
            }

            foreach (SolarJob solarJob in solarJobs)
            {
                Assert.True(solarJobQueue.TryEnqueue(solarJob));
            }

            await solarJobHostedService.StartAsync(CancellationToken.None);
            try
            {
                foreach (SolarJob solarJob in solarJobs)
                {
                    await SolarJobFixture_WaitAsync(solarJobQueue, solarJob.Id, x => x.Status == nameof(Enums.SolarJobStatus.Completed));
                }
            }
            finally
            {
                await solarJobHostedService.StopAsync(CancellationToken.None);
            }

            Assert.Equal(1, activeMax);
            Assert.Equal(solarJobs.Count, intervals.Count);
            for (int i = 0; i < intervals.Count; i++)
            {
                Assert.Equal(i + 1, intervals[i].Id);
                if (i > 0)
                {
                    Assert.True(intervals[i].Start >= intervals[i - 1].Finish, $"Job {i + 1} started before job {i} finished.");
                }
            }

            for (int i = 1; i < solarJobs.Count; i++)
            {
                Assert.True(solarJobs[i].StartedAt >= solarJobs[i - 1].FinishedAt);
            }
        }
    }
}
