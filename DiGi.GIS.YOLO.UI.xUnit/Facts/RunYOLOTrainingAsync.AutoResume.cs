using DiGi.GIS.YOLO.UI.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the automatic-resume loop of <c>Modify.RunYOLOTrainingAsync</c>: a stalled attempt whose checkpoint is unfinished is resumed from <c>weights\last.pt</c> after the checkpoint is copied aside, up to <c>AutoResumeCount</c> times, and the run then completes its unchanged tail; a stall with no retries left fails, a finished checkpoint after a crash is never resumed, and a cancellation is never resumed. A crash with an unfinished checkpoint is resumed too, its log line names the epoch it interrupted, and the attempt's traceback is reported before the resume; a crash that wrote nothing says so instead of listing nothing. The attempt that ends a run, with no resume left or with automatic resume off, names its own cause and epoch in the result's messages.
        /// <para>The training is stand-in torch and ultralytics modules written into the working directory: the fresh attempt leaves a resumable checkpoint behind and goes silent, ended by the two-second inactivity limit, and the resumed attempt writes <c>best.pt</c>. No real ultralytics or GPU is needed. The interpreter is machine specific, so the fact returns without asserting when none is installed.</para>
        /// <para>Medium test (17 s): runs when DIGI_TEST_MAX_DURATION is Medium (the default) or Long.</para>
        /// </summary>
        [MediumFact]
        public async Task RunYOLOTrainingAsync_AutoResume()
        {
            string? pythonPath = PythonPath_Runnable();
            if (string.IsNullOrWhiteSpace(pythonPath))
            {
                return;
            }

            string directory = Path.Combine(Path.GetTempPath(), "DiGi.GIS.YOLO.UI.xUnit.AutoResume." + Guid.NewGuid().ToString("N"));

            try
            {
                Directory.CreateDirectory(directory);

                string path_Configuration = Path.Combine(directory, "conf.yaml");
                File.WriteAllText(path_Configuration, "path: .\nnames:\n  0: Building");

                string path_Start = Path.Combine(directory, "start.pt");
                File.WriteAllText(path_Start, "dummy start weights");

                WriteMockTorch(directory);
                WriteMockUltralyticsAutoResume(directory);

                TimeSpan inactivityTimeout = TimeSpan.FromSeconds(2);

                void SetMode(string mode)
                {
                    File.WriteAllText(Path.Combine(directory, "mock_mode.txt"), mode);
                }

                int Attempts()
                {
                    string path_Attempts = Path.Combine(directory, "attempts.log");
                    return File.Exists(path_Attempts) ? File.ReadAllLines(path_Attempts).Length : 0;
                }

                void ResetAttempts()
                {
                    string path_Attempts = Path.Combine(directory, "attempts.log");
                    if (File.Exists(path_Attempts))
                    {
                        File.Delete(path_Attempts);
                    }
                }

                Classes.YOLOTrainingRunOptions Options(string projectDirectory, int autoResumeCount, TimeSpan? timeout)
                {
                    return new Classes.YOLOTrainingRunOptions()
                    {
                        AutoResumeCount = autoResumeCount,
                        DatasetOptions = new Classes.YOLOTrainingDatasetOptions() { OutputDirectory = directory },
                        Device = "cpu",
                        InactivityTimeout = timeout,
                        ProjectDirectory = projectDirectory,
                        PythonPath = pythonPath,
                        RunName = "train9",
                        StartWeightsPath = path_Start,
                        Steps = [YOLOTrainingStep.Train],
                        WorkingDirectory = directory
                    };
                }

                // 1. A stall is resumed once, the checkpoint is backed up first, and the run then completes its tail.
                {
                    string projectDirectory = Path.Combine(directory, "scenario", "stall_once");
                    WriteMockCheckpoints(directory, MockCheckpoint(0, 6, path_Configuration, projectDirectory, "train9", true));
                    SetMode("stall_once");
                    ResetAttempts();

                    Classes.YOLOTrainingRunResult? yOLOTrainingRunResult = await Modify.RunYOLOTrainingAsync(null, Options(projectDirectory, 2, inactivityTimeout));
                    Assert.NotNull(yOLOTrainingRunResult);
                    Assert.Empty(yOLOTrainingRunResult!.FailedStepNames);
                    Assert.Equal(Enums.YearBuiltPredictionExitCode.Succeeded, Query.YOLOTrainingRunExitCode(yOLOTrainingRunResult));
                    Assert.Single(yOLOTrainingRunResult.AutoResumes);
                    Assert.Equal("Stalled", yOLOTrainingRunResult.AutoResumes[0].Reason);
                    Assert.Equal(1, yOLOTrainingRunResult.AutoResumes[0].Epoch);
                    Assert.False(string.IsNullOrWhiteSpace(yOLOTrainingRunResult.AutoResumes[0].BackupFileName));

                    string directory_Weights = Path.Combine(projectDirectory, "train9", "weights");
                    Assert.True(File.Exists(Path.Combine(directory_Weights, yOLOTrainingRunResult.AutoResumes[0].BackupFileName!)));
                    Assert.Single(Directory.GetFiles(directory_Weights, "last_autoresume1_*.pt"));
                    Assert.True(File.Exists(Path.Combine(projectDirectory, "train9", "train9.pt")));
                    Assert.Equal(2, Attempts());
                }

                // 2. With every attempt stalling, the run makes exactly AutoResumeCount resumes and then fails.
                {
                    string projectDirectory = Path.Combine(directory, "scenario", "stall_always");
                    WriteMockCheckpoints(directory, MockCheckpoint(0, 6, path_Configuration, projectDirectory, "train9", true));
                    SetMode("stall_always");
                    ResetAttempts();

                    Classes.YOLOTrainingRunResult? yOLOTrainingRunResult = await Modify.RunYOLOTrainingAsync(null, Options(projectDirectory, 2, inactivityTimeout));
                    Assert.NotNull(yOLOTrainingRunResult);
                    Assert.Contains(Query.YOLOTrainingStepName(YOLOTrainingStep.Train), yOLOTrainingRunResult!.FailedStepNames);
                    Assert.Equal(2, yOLOTrainingRunResult.AutoResumes.Count);
                    Assert.Equal(3, Attempts());

                    // The attempt that ended the run names its own cause and epoch, not only the resumes before it.
                    Assert.Contains(yOLOTrainingRunResult.Messages, x => x.StartsWith("Training stalled at epoch 2", StringComparison.Ordinal) && x.EndsWith("- no automatic resume left (2 of 2 used)", StringComparison.Ordinal));
                }

                // 3. AutoResumeCount = 0 keeps today's behaviour: the stall fails at once.
                {
                    string projectDirectory = Path.Combine(directory, "scenario", "no_resume");
                    WriteMockCheckpoints(directory, MockCheckpoint(0, 6, path_Configuration, projectDirectory, "train9", true));
                    SetMode("stall_always");
                    ResetAttempts();

                    Classes.YOLOTrainingRunResult? yOLOTrainingRunResult = await Modify.RunYOLOTrainingAsync(null, Options(projectDirectory, 0, inactivityTimeout));
                    Assert.NotNull(yOLOTrainingRunResult);
                    Assert.Empty(yOLOTrainingRunResult!.AutoResumes);
                    Assert.Contains(Query.YOLOTrainingStepName(YOLOTrainingStep.Train), yOLOTrainingRunResult.FailedStepNames);
                    Assert.Equal(1, Attempts());
                    Assert.Contains(yOLOTrainingRunResult.Messages, x => x.StartsWith("Training stalled at epoch 2", StringComparison.Ordinal) && x.EndsWith("- automatic resume is off (AutoResumeCount 0)", StringComparison.Ordinal));
                }

                // 4. A crash that leaves a finished checkpoint is not resumed.
                {
                    string projectDirectory = Path.Combine(directory, "scenario", "crash_finished");
                    WriteMockCheckpoints(directory, MockCheckpoint(-1, 6, path_Configuration, projectDirectory, "train9", false));
                    SetMode("crash");
                    ResetAttempts();

                    Classes.YOLOTrainingRunResult? yOLOTrainingRunResult = await Modify.RunYOLOTrainingAsync(null, Options(projectDirectory, 2, inactivityTimeout));
                    Assert.NotNull(yOLOTrainingRunResult);
                    Assert.Empty(yOLOTrainingRunResult!.AutoResumes);
                    Assert.Contains(Query.YOLOTrainingStepName(YOLOTrainingStep.Train), yOLOTrainingRunResult.FailedStepNames);
                }

                // 5. A cancellation is never undone by an automatic resume.
                {
                    string projectDirectory = Path.Combine(directory, "scenario", "cancelled");
                    WriteMockCheckpoints(directory, MockCheckpoint(0, 6, path_Configuration, projectDirectory, "train9", true));
                    SetMode("stall_always");
                    ResetAttempts();

                    using CancellationTokenSource cancellationTokenSource = new();
                    cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(3));

                    Classes.YOLOTrainingRunResult? yOLOTrainingRunResult = await Modify.RunYOLOTrainingAsync(null, Options(projectDirectory, 2, TimeSpan.FromSeconds(30)), cancellationToken: cancellationTokenSource.Token);
                    Assert.NotNull(yOLOTrainingRunResult);
                    Assert.Empty(yOLOTrainingRunResult!.AutoResumes);
                }

                // 6. A crash with an unfinished checkpoint is resumed; its log line names the epoch it interrupted, and the
                // attempt's error output - the traceback, its only record - is reported before the resume.
                {
                    string projectDirectory = Path.Combine(directory, "scenario", "crash_once");
                    WriteMockCheckpoints(directory, MockCheckpoint(0, 6, path_Configuration, projectDirectory, "train9", true));
                    SetMode("crash_once");
                    ResetAttempts();

                    ReportedLines reportedLines = new();
                    Classes.YOLOTrainingRunResult? yOLOTrainingRunResult = await Modify.RunYOLOTrainingAsync(null, Options(projectDirectory, 2, TimeSpan.FromSeconds(30)), information: reportedLines);
                    Assert.NotNull(yOLOTrainingRunResult);
                    Assert.Empty(yOLOTrainingRunResult!.FailedStepNames);
                    Assert.Single(yOLOTrainingRunResult.AutoResumes);
                    Assert.Equal("Exited with code 1", yOLOTrainingRunResult.AutoResumes[0].Reason);
                    Assert.Equal(2, Attempts());

                    List<string> values = reportedLines.Values;
                    int index_Resume = values.FindIndex(x => x == "Training exited with code 1 at epoch 2 - automatic resume 1 of 2");
                    Assert.True(index_Resume > 0, string.Join(Environment.NewLine, values));

                    int index_Header = values.FindIndex(x => x.StartsWith("Error output of the attempt that exited with code 1 (last ", StringComparison.Ordinal));
                    Assert.True(index_Header >= 0 && index_Header < index_Resume, string.Join(Environment.NewLine, values));

                    List<string> values_ErrorOutput = values.GetRange(index_Header + 1, index_Resume - index_Header - 1);
                    Assert.All(values_ErrorOutput, x => Assert.StartsWith("  | ", x));
                    Assert.Contains(values_ErrorOutput, x => x.Contains("Traceback", StringComparison.Ordinal));
                    Assert.Contains(values_ErrorOutput, x => x.Contains("RuntimeError: mock DataLoader worker exited unexpectedly", StringComparison.Ordinal));
                }

                // 7. A crash that wrote no error output (a process ended from outside writes none) says so instead of
                // listing nothing, and with automatic resume off the failure still names the cause and the epoch.
                {
                    string projectDirectory = Path.Combine(directory, "scenario", "crash_silent");
                    WriteMockCheckpoints(directory, MockCheckpoint(0, 6, path_Configuration, projectDirectory, "train9", true));
                    SetMode("crash");
                    ResetAttempts();

                    ReportedLines reportedLines = new();
                    Classes.YOLOTrainingRunResult? yOLOTrainingRunResult = await Modify.RunYOLOTrainingAsync(null, Options(projectDirectory, 0, TimeSpan.FromSeconds(30)), information: reportedLines);
                    Assert.NotNull(yOLOTrainingRunResult);
                    Assert.Empty(yOLOTrainingRunResult!.AutoResumes);
                    Assert.Contains(Query.YOLOTrainingStepName(YOLOTrainingStep.Train), yOLOTrainingRunResult.FailedStepNames);
                    Assert.Contains("Training exited with code 3 at epoch 2 - automatic resume is off (AutoResumeCount 0)", yOLOTrainingRunResult.Messages);

                    List<string> values = reportedLines.Values;
                    Assert.Contains("The attempt that exited with code 3 wrote no error output - it was ended from outside, or exited without a message.", values);
                    Assert.DoesNotContain(values, x => x.StartsWith("Error output of the attempt", StringComparison.Ordinal));
                }
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        /// <summary>
        /// Writes a stand-in <c>ultralytics</c> package whose <c>YOLO</c> leaves a resumable checkpoint behind and goes silent on a fresh attempt, and completes or stalls on a resume according to <c>mock_mode.txt</c>: <c>stall_once</c> resumes to completion, <c>stall_always</c> never does, <c>crash</c> exits non-zero on the fresh attempt, and <c>crash_once</c> raises an uncaught exception on the fresh attempt - exit code 1 with a traceback on standard error, as a torch data-loader worker crash does - and resumes to completion.
        /// <para>Each attempt appends one line to <c>attempts.log</c> beside the package, so a fact can count how many times the runner tried.</para>
        /// </summary>
        /// <param name="directory">The working directory the scripts run in.</param>
        private static void WriteMockUltralyticsAutoResume(string directory)
        {
            string directory_Mock = Path.Combine(directory, "ultralytics");
            Directory.CreateDirectory(directory_Mock);

            File.WriteAllLines(Path.Combine(directory_Mock, "__init__.py"),
            [
                "import json, os, time, torch",
                "",
                "__version__ = '0.0.0'",
                "",
                "class _Trainer:",
                "    pass",
                "",
                "class YOLO:",
                "    def __init__(self, model):",
                "        self.model = model",
                "        self.trainer = None",
                "",
                "    def train(self, **kwargs):",
                "        directory = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))",
                "        with open(os.path.join(directory, 'attempts.log'), 'a') as file:",
                "            file.write('resume\\n' if kwargs.get('resume') else 'fresh\\n')",
                "        path_mode = os.path.join(directory, 'mock_mode.txt')",
                "        mode = 'stall_once'",
                "        if os.path.isfile(path_mode):",
                "            with open(path_mode) as file:",
                "                mode = file.read().strip()",
                "        resume = bool(kwargs.get('resume'))",
                "        checkpoint = torch.load(self.model) if resume else {}",
                "        train_args = checkpoint.get('train_args', {}) if isinstance(checkpoint, dict) else {}",
                "        project = kwargs.get('project') or train_args.get('project')",
                "        name = kwargs.get('name') or train_args.get('name') or 'train'",
                "        weights = os.path.join(project, name, 'weights')",
                "        os.makedirs(weights, exist_ok=True)",
                "        if not resume:",
                "            with open(os.path.join(weights, 'last.pt'), 'w') as file:",
                "                file.write('interrupted checkpoint')",
                "        if resume and mode != 'stall_always':",
                "            path = os.path.join(weights, 'best.pt')",
                "            with open(path, 'w') as file:",
                "                file.write(json.dumps(kwargs, sort_keys=True))",
                "            self.trainer = _Trainer()",
                "            self.trainer.best = path",
                "            self.trainer.amp = kwargs.get('amp', False)",
                "            return",
                "        if mode == 'crash':",
                "            raise SystemExit(3)",
                "        if mode == 'crash_once':",
                "            raise RuntimeError('mock DataLoader worker exited unexpectedly')",
                "        print('mock training started', flush=True)",
                "        time.sleep(120)",
            ]);
        }
    }
}
