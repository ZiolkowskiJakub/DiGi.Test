using DiGi.GIS.PostgreSQL.UI.Classes;
using DiGi.GIS.WebAPI.Classes;
using DiGi.GIS.YOLO.UI.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies a refusal before launch names its reason on the task row instead of the generic "reported failure without an exception" text - the symptom of ZiolkowskiJakub/DiGi.GIS.PostgreSQL.UI#26.
        /// <para>The fact drives the real task rather than a helper, because the check it exercises runs first: the runner is resolved before the window, the counties or anything else, so no WPF application, no HTTP request and no database is needed to reach it. The precondition pins that under a test run no runner candidate resolves, so the refusal it reaches is always the runner one; on unmodified code the same run ends in a bare <c>false</c> and the row carries the generic text, which is exactly what this fact fails on.</para>
        /// <para>Logging is a no-op in the test host (no logger manager is configured), so nothing is written anywhere by the run.</para>
        /// </summary>
        [Fact]
        public async Task UIYearBuiltPredictionsTask_Refusal_NamesReasonOnTaskRow()
        {
            // The candidates the resolver probes are all absent under a test run - the same premise the resolver
            // facts state - so this task cannot find a runner and its first check is the one that refuses.
            Assert.Null(Query.YearBuiltPredictionConsoleAppPath());

            // Any non-empty key builds a manager; nothing here reaches the network, because the refusal happens
            // long before the county query.
            GISWebAPIManager? gISWebAPIManager = WebAPI.Create.GISWebAPIManager("00000000-0000-0000-0000-000000000000");
            Assert.NotNull(gISWebAPIManager);

            UIYearBuiltPredictionsTask uIYearBuiltPredictionsTask = new(gISWebAPIManager!)
            {
                ConsoleAppPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), Constants.FileName.YearBuiltPredictionConsoleApp)
            };

            uIYearBuiltPredictionsTask.Start();

            int timeoutMs = 1000;
            while (!uIYearBuiltPredictionsTask.IsCompleted && timeoutMs > 0)
            {
                await Task.Delay(10);
                timeoutMs -= 10;
            }

            Assert.True(uIYearBuiltPredictionsTask.IsCompleted);

            string expected = Constants.FileName.YearBuiltPredictionConsoleApp + " was not found beside this application or in the workspace - the Year Built prediction run cannot be started";

            Assert.IsType<Core.Classes.BackgroundTaskFailureException>(uIYearBuiltPredictionsTask.Exception);
            Assert.Equal(expected, uIYearBuiltPredictionsTask.Exception!.Message);

            // The row reads the same reason: the wrapper is built only after the task has finished, because it
            // refreshes through the WPF dispatcher on the task's events and a test host runs no WPF application.
            DiGi.UI.WPF.Classes.VisualBackgroundTask visualBackgroundTask = new(uIYearBuiltPredictionsTask, "Predict year built", "Runs the Year Built prediction pipeline over the chosen counties");

            Assert.NotNull(visualBackgroundTask.ExceptionText);
            Assert.Contains(expected, visualBackgroundTask.ExceptionText);
            Assert.DoesNotContain("The task reported failure without an exception", visualBackgroundTask.ExceptionText);
        }

        /// <summary>
        /// Verifies <see cref="UIYearBuiltPredictionsTask.FailureMessage(YearBuiltPredictionExitCode, IEnumerable{string})"/> names what actually ended the run, in the order the row needs it: the failed step under its header, else the last <c>[ERROR]</c>/<c>[FATAL]</c> line, else the exit code alone - and never a narration line the log already carries.
        /// </summary>
        [Fact]
        public void UIYearBuiltPredictionsTask_FailureMessage()
        {
            // The step that died is named with the header it was listed under; the earlier error, the progress
            // counts and the note are narration and never win over it.
            Assert.Equal(
                "The Year Built prediction run did not finish - Pipeline completed with 1 failed step(s): Score the buildings with the detector - Pipeline execution failure. See the log beside this application.",
                UIYearBuiltPredictionsTask.FailureMessage(
                    YearBuiltPredictionExitCode.Failed,
                    [
                        "[ERROR] An earlier step could not read its input",
                        "[PROGRESS] Processed 10 items...",
                        "[ERROR] Pipeline completed with 1 failed step(s):",
                        "  - Score the buildings with the detector",
                        "[NOTE] Tallies are not a record of what was stored"
                    ]));

            // Without a failed-step header the last cause alone is named, with the exit code that carried it.
            Assert.Equal(
                "The Year Built prediction run did not finish - Unrecognized request - WebAPI key or client configuration missing. See the log beside this application.",
                UIYearBuiltPredictionsTask.FailureMessage(
                    YearBuiltPredictionExitCode.Authorization,
                    [
                        "[PROGRESS] Processed 10 items...",
                        "[ERROR] Unrecognized request",
                        "[INFO] Execution cancelled."
                    ]));

            // A fatal crash printed last is the cause, whatever failed before it.
            Assert.Equal(
                "The Year Built prediction run did not finish - Unhandled pipeline error: CUDA out of memory - Pipeline execution failure. See the log beside this application.",
                UIYearBuiltPredictionsTask.FailureMessage(
                    YearBuiltPredictionExitCode.Failed,
                    [
                        "[ERROR] An earlier step failed",
                        "[FATAL] Unhandled pipeline error: CUDA out of memory"
                    ]));

            // A step entry listed under no header qualifies nothing - the cause is named without it.
            Assert.Equal(
                "The Year Built prediction run did not finish - An earlier step failed - Pipeline execution failure. See the log beside this application.",
                UIYearBuiltPredictionsTask.FailureMessage(
                    YearBuiltPredictionExitCode.Failed,
                    [
                        "[ERROR] An earlier step failed",
                        "  - Score the buildings with the detector"
                    ]));

            // Nothing quotable at all: only progress counts and narration, so the exit code is the whole reason.
            Assert.Equal(
                "The Year Built prediction run did not finish - Pipeline execution failure. See the log beside this application.",
                UIYearBuiltPredictionsTask.FailureMessage(
                    YearBuiltPredictionExitCode.Failed,
                    [
                        "[PROGRESS] Processed 10 items...",
                        "[INFO] Dataset: C:\\scratch"
                    ]));
        }
    }
}
