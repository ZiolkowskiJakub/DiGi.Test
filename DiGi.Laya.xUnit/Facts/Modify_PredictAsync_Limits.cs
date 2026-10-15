using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the three ways <see cref="Modify.PredictAsync(IEnumerable{Classes.LayaRequest}?, Classes.LayaPredictionOptions?, CancellationToken)"/> ends a run that does not finish are reported apart, with a fake interpreter (a .cmd that waits a minute) standing in for CPython: the overall <see cref="Classes.LayaPredictionOptions.Timeout"/> sets only <see cref="Classes.LayaPredictionResult.TimedOut"/>, the <see cref="Classes.LayaPredictionOptions.InactivityTimeout"/> sets <see cref="Classes.LayaPredictionResult.Stalled"/>, and the caller's token sets only <see cref="Classes.LayaPredictionResult.Cancelled"/>; each returns a result rather than throwing, with exit code -1 and no responses, within seconds of the limit.
        /// <para>Medium test (3 s, 2026-10-15): runs when DIGI_TEST_MAX_DURATION is Medium (the default) or Long.</para>
        /// </summary>
        [MediumFact]
        public async Task Modify_PredictAsync_Limits()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                return;
            }

            string directory = Path.Combine(Path.GetTempPath(), string.Concat("Laya_PredictAsync_", Guid.NewGuid().ToString("N")));

            try
            {
                string directory_Checkpoint = Path.Combine(directory, "checkpoint");
                Directory.CreateDirectory(directory_Checkpoint);
                File.WriteAllText(Path.Combine(directory_Checkpoint, "rl_agent_config.json"), "{}");

                string path_Hang = Path.Combine(directory, "hang.cmd");
                File.WriteAllText(path_Hang, "@ping -n 60 127.0.0.1 >nul\r\n");

                Classes.LayaRequest[] layaRequests = [new Classes.LayaRequest("item-1", "Please refund me.", false, [new Classes.LayaNoulQuestion("refund", "The customer asks for a refund.")], null, null)];

                Classes.LayaPredictionOptions Options(string name)
                {
                    return new Classes.LayaPredictionOptions() { PythonPath = path_Hang, ModelPath = directory_Checkpoint, WorkingDirectory = Path.Combine(directory, name) };
                }

                Stopwatch stopwatch = Stopwatch.StartNew();
                Classes.LayaPredictionOptions layaPredictionOptions_Timeout = Options("timeout");
                layaPredictionOptions_Timeout.Timeout = TimeSpan.FromSeconds(1);
                Classes.LayaPredictionResult? layaPredictionResult_Timeout = await layaRequests.PredictAsync(layaPredictionOptions_Timeout);
                Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(20));
                Assert.True(layaPredictionResult_Timeout!.TimedOut);
                Assert.False(layaPredictionResult_Timeout.Cancelled);
                Assert.False(layaPredictionResult_Timeout.Stalled);
                Assert.Equal(-1, layaPredictionResult_Timeout.ExitCode);
                Assert.Empty(layaPredictionResult_Timeout.Responses!);
                Assert.Contains(layaPredictionResult_Timeout.Messages!, x => x.Contains("time limit"));

                stopwatch.Restart();
                Classes.LayaPredictionOptions layaPredictionOptions_Stalled = Options("stalled");
                layaPredictionOptions_Stalled.InactivityTimeout = TimeSpan.FromSeconds(1);
                Classes.LayaPredictionResult? layaPredictionResult_Stalled = await layaRequests.PredictAsync(layaPredictionOptions_Stalled);
                Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(20));
                Assert.True(layaPredictionResult_Stalled!.Stalled);
                Assert.False(layaPredictionResult_Stalled.TimedOut);
                Assert.False(layaPredictionResult_Stalled.Cancelled);
                Assert.Equal(-1, layaPredictionResult_Stalled.ExitCode);
                Assert.Contains(layaPredictionResult_Stalled.Messages!, x => x.Contains("printed nothing"));

                stopwatch.Restart();
                using CancellationTokenSource cancellationTokenSource = new();
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));
                Classes.LayaPredictionResult? layaPredictionResult_Cancelled = await layaRequests.PredictAsync(Options("cancelled"), cancellationTokenSource.Token);
                Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(20));
                Assert.True(layaPredictionResult_Cancelled!.Cancelled);
                Assert.False(layaPredictionResult_Cancelled.TimedOut);
                Assert.False(layaPredictionResult_Cancelled.Stalled);
                Assert.Equal(-1, layaPredictionResult_Cancelled.ExitCode);
                Assert.Contains(layaPredictionResult_Cancelled.Messages!, x => x.Contains("cancelled"));
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }
    }
}
