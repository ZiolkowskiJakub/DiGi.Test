using DiGi.GIS.YOLO.UI.Enums;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the automatic-resume loop of <c>Modify.RunYOLOTrainingAsync</c>: a stalled attempt whose checkpoint is unfinished is resumed from <c>weights\last.pt</c> after the checkpoint is copied aside, up to <c>AutoResumeCount</c> times, and the run then completes its unchanged tail; a stall with no retries left fails, a finished checkpoint after a crash is never resumed, and a cancellation is never resumed.
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
        /// Writes a stand-in <c>ultralytics</c> package whose <c>YOLO</c> leaves a resumable checkpoint behind and goes silent on a fresh attempt, and completes or stalls on a resume according to <c>mock_mode.txt</c>: <c>stall_once</c> resumes to completion, <c>stall_always</c> never does, and <c>crash</c> exits non-zero on the fresh attempt.
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
                "        print('mock training started', flush=True)",
                "        time.sleep(120)",
            ]);
        }
    }
}
