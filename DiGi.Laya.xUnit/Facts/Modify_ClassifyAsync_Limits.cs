using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Modify.ClassifyAsync(IEnumerable{Classes.LayaItem}?, IEnumerable{Classes.LayaLabelSet}?, Classes.LayaClassificationOptions?, Classes.LayaPredictionOptions?, IEnumerable{Classes.LayaLabelConstraint}?, System.Threading.CancellationToken)"/> reports a prediction run ended by <see cref="Classes.LayaPredictionOptions.Timeout"/> on the run result, with a fake interpreter (a .cmd that waits a minute) standing in for CPython: <see cref="Classes.LayaClassificationRunResult.TimedOut"/> is set, the exit code is -1, the time limit is a run message, and every item still gets an unscored result that does not repeat it - within seconds of the limit.
        /// <para>Medium test (2 s, 2026-10-15): runs when DIGI_TEST_MAX_DURATION is Medium (the default) or Long.</para>
        /// </summary>
        [MediumFact]
        public async Task Modify_ClassifyAsync_Limits()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                return;
            }

            string directory = Path.Combine(Path.GetTempPath(), string.Concat("Laya_ClassifyAsync_Limits_", Guid.NewGuid().ToString("N")));

            try
            {
                string directory_Checkpoint = Path.Combine(directory, "checkpoint");
                Directory.CreateDirectory(directory_Checkpoint);
                File.WriteAllText(Path.Combine(directory_Checkpoint, "rl_agent_config.json"), "{}");

                string path_Hang = Path.Combine(directory, "hang.cmd");
                File.WriteAllText(path_Hang, "@ping -n 60 127.0.0.1 >nul\r\n");

                List<Classes.LayaLabelSet> layaLabelSets = [new Classes.LayaLabelSet("tags", "The recipe is {label}.", Enums.LayaLabelSetKind.MultiLabel, [new Classes.LayaLabel("vegan", "vegan", null)])];
                List<Classes.LayaItem> layaItems = [new Classes.LayaItem("r1", "Lentil soup.", false), new Classes.LayaItem("r2", "Cheesecake.", false)];

                Classes.LayaPredictionOptions layaPredictionOptions = new() { PythonPath = path_Hang, ModelPath = directory_Checkpoint, WorkingDirectory = Path.Combine(directory, "timeout"), Timeout = TimeSpan.FromSeconds(1) };

                Stopwatch stopwatch = Stopwatch.StartNew();
                Classes.LayaClassificationRunResult? layaClassificationRunResult = await layaItems.ClassifyAsync(layaLabelSets, null, layaPredictionOptions);
                Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(20));
                Assert.NotNull(layaClassificationRunResult);
                Assert.True(layaClassificationRunResult!.TimedOut);
                Assert.False(layaClassificationRunResult.Stalled);
                Assert.False(layaClassificationRunResult.Cancelled);
                Assert.Equal(-1, layaClassificationRunResult.ExitCode);
                Assert.NotNull(layaClassificationRunResult.Duration);
                Assert.Contains(layaClassificationRunResult.Messages!, x => x.Contains("time limit"));
                Assert.Equal(["r1", "r2"], layaClassificationRunResult.Results!.Select(x => x.ItemId));
                Assert.All(layaClassificationRunResult.Results!, x => Assert.Empty(x.Scores!));
                Assert.All(layaClassificationRunResult.Results!, x => Assert.DoesNotContain(x.Messages!, y => y.Contains("time limit")));
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
