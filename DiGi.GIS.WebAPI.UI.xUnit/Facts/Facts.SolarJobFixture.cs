using DiGi.GIS.WebAPI.UI.Classes;
using DiGi.GIS.WebAPI.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// A clock the facts move by hand, so that the expiry of background solar radiation jobs is tested on both sides of <see cref="Constants.Default.SolarJobResultRetentionMinutes"/> without waiting for it.
        /// </summary>
        private sealed class ManualTimeProvider : TimeProvider
        {
            private DateTimeOffset dateTimeOffset = new(2026, 9, 26, 12, 0, 0, TimeSpan.Zero);

            /// <summary>
            /// Moves the clock forward.
            /// </summary>
            /// <param name="timeSpan">The time to move by.</param>
            public void Advance(TimeSpan timeSpan)
            {
                dateTimeOffset += timeSpan;
            }

            /// <summary>
            /// Gets the time the clock was moved to.
            /// </summary>
            /// <returns>The current time of the clock.</returns>
            public override DateTimeOffset GetUtcNow()
            {
                return dateTimeOffset;
            }
        }

        // A background job with a fake calculation, so the queue, the gate and the states are tested without geometry.
        private static SolarJob SolarJobFixture_Job(Func<List<SurfaceSolarRadiationResult>?> calculation, long buildingModelId = 7)
        {
            return new SolarJob(Guid.NewGuid(), buildingModelId, 1465, Constants.Default.SolarSurroundingRadius, 5, null, null, calculation);
        }

        // Polls a job until its view satisfies the condition, failing the fact after the timeout.
        private static async Task<SolarJobViewModel> SolarJobFixture_WaitAsync(SolarJobQueue solarJobQueue, Guid id, Func<SolarJobViewModel, bool> condition, int timeoutMilliseconds = 30_000)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                SolarJobViewModel? solarJobViewModel = solarJobQueue.SolarJobViewModel(id);
                Assert.NotNull(solarJobViewModel);
                if (condition(solarJobViewModel))
                {
                    return solarJobViewModel;
                }

                Assert.True(stopwatch.ElapsedMilliseconds < timeoutMilliseconds, $"Job {id} stayed {solarJobViewModel.Status}.");
                await Task.Delay(20);
            }
        }
    }
}
