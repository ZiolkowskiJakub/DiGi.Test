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
        /// Verifies that the exit code of a training run follows the step that failed: a cancellation wins over everything, an unusable option is a configuration error, a missing interpreter an environment error, the training and the validation have codes of their own, and a clean run succeeds.
        /// <para>The dataset steps list their splits as <c>Train</c> and <c>Test</c>, and the training step is listed with its enumeration prefix so that the two are never confused.</para>
        /// </summary>
        [Fact]
        public void YOLOTrainingRunExitCode()
        {
            static Classes.YOLOTrainingRunResult Result(bool cancelled, params string[] failedStepNames)
            {
                return new Classes.YOLOTrainingRunResult("run", null, null, null, null, null, null, null, cancelled, failedStepNames, null, null, null);
            }

            Assert.Equal(Enums.YearBuiltPredictionExitCode.Failed, Query.YOLOTrainingRunExitCode(null));
            Assert.Equal(Enums.YearBuiltPredictionExitCode.Succeeded, Query.YOLOTrainingRunExitCode(Result(false)));
            Assert.Equal(Enums.YearBuiltPredictionExitCode.Cancelled, Query.YOLOTrainingRunExitCode(Result(true, Query.YOLOTrainingStepName(YOLOTrainingStep.Train))));
            Assert.Equal(Enums.YearBuiltPredictionExitCode.Configuration, Query.YOLOTrainingRunExitCode(Result(false, nameof(Classes.YOLOTrainingRunOptions.StartWeightsPath))));
            Assert.Equal(Enums.YearBuiltPredictionExitCode.Environment, Query.YOLOTrainingRunExitCode(Result(false, nameof(Classes.YOLOTrainingRunOptions.PythonPath))));
            Assert.Equal(Enums.YearBuiltPredictionExitCode.Training, Query.YOLOTrainingRunExitCode(Result(false, Query.YOLOTrainingStepName(YOLOTrainingStep.Train))));
            Assert.Equal(Enums.YearBuiltPredictionExitCode.Validation, Query.YOLOTrainingRunExitCode(Result(false, Query.YOLOTrainingStepName(YOLOTrainingStep.Validate))));
            Assert.Equal(Enums.YearBuiltPredictionExitCode.Failed, Query.YOLOTrainingRunExitCode(Result(false, Query.YOLOTrainingStepName(YOLOTrainingStep.Evaluate))));

            // A dataset split named in a failure is a dataset problem, not a failed training.
            Assert.Equal(Enums.YearBuiltPredictionExitCode.Configuration, Query.YOLOTrainingRunExitCode(Result(false, nameof(DiGi.YOLO.Enums.Category.Train), Query.YOLOTrainingStepName(YOLOTrainingStep.Evaluate))));
        }

        /// <summary>
        /// Verifies that the run refuses what it can know before the first step - no options, no dataset root, a missing start file, an unusable interpreter, a run name that is taken or is <c>model</c>, a project folder inside a YOLO\models folder - and that it does so without starting a process.
        /// <para>It also verifies that the steps are selected: an evaluation-only run never reaches the training, and a token that is already cancelled ends the run as cancelled. Relative paths are made absolute, so the start weights are reported under their full path.</para>
        /// </summary>
        [Fact]
        public async Task RunYOLOTrainingAsync_Preflight()
        {
            Assert.Null(await Modify.RunYOLOTrainingAsync(null, null));

            string directory = Path.Combine(Path.GetTempPath(), "DiGi.GIS.YOLO.UI.xUnit." + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);

            try
            {
                string path_Start = Path.Combine(directory, "start.pt");
                File.WriteAllText(path_Start, "not weights");
                string path_Python = Path.Combine(directory, "python.exe");
                File.WriteAllText(path_Python, "not an interpreter");

                Classes.YOLOTrainingRunOptions Options(string? runName, string? projectDirectory, string? pythonPath, string? startWeightsPath)
                {
                    return new Classes.YOLOTrainingRunOptions()
                    {
                        DatasetOptions = new Classes.YOLOTrainingDatasetOptions() { OutputDirectory = Path.Combine(directory, "dataset") },
                        ProjectDirectory = projectDirectory,
                        PythonPath = pythonPath,
                        RunName = runName,
                        StartWeightsPath = startWeightsPath,
                        Steps = [YOLOTrainingStep.Train]
                    };
                }

                async Task<Classes.YOLOTrainingRunResult> RunAsync(Classes.YOLOTrainingRunOptions yOLOTrainingRunOptions)
                {
                    Classes.YOLOTrainingRunResult? yOLOTrainingRunResult = await Modify.RunYOLOTrainingAsync(null, yOLOTrainingRunOptions);
                    Assert.NotNull(yOLOTrainingRunResult);
                    return yOLOTrainingRunResult!;
                }

                string directory_Runs = Path.Combine(directory, "runs");

                // No dataset root.
                Classes.YOLOTrainingRunResult yOLOTrainingRunResult_NoDataset = await RunAsync(new Classes.YOLOTrainingRunOptions() { Steps = [YOLOTrainingStep.Train] });
                Assert.Contains(nameof(Classes.YOLOTrainingRunOptions.DatasetOptions), yOLOTrainingRunResult_NoDataset.FailedStepNames);
                Assert.Equal(Enums.YearBuiltPredictionExitCode.Configuration, Query.YOLOTrainingRunExitCode(yOLOTrainingRunResult_NoDataset));

                // A missing start file, reported before anything starts.
                Classes.YOLOTrainingRunResult yOLOTrainingRunResult_NoStart = await RunAsync(Options("train9", directory_Runs, path_Python, Path.Combine(directory, "missing.pt")));
                Assert.Contains(nameof(Classes.YOLOTrainingRunOptions.StartWeightsPath), yOLOTrainingRunResult_NoStart.FailedStepNames);
                Assert.Null(yOLOTrainingRunResult_NoStart.WeightsPath);
                Assert.Equal(Enums.YearBuiltPredictionExitCode.Configuration, Query.YOLOTrainingRunExitCode(yOLOTrainingRunResult_NoStart));

                // An interpreter that does not exist is an environment problem.
                Classes.YOLOTrainingRunResult yOLOTrainingRunResult_NoPython = await RunAsync(Options("train9", directory_Runs, Path.Combine(directory, "nope", "python.exe"), path_Start));
                Assert.Equal([nameof(Classes.YOLOTrainingRunOptions.PythonPath)], yOLOTrainingRunResult_NoPython.FailedStepNames);
                Assert.Equal(Enums.YearBuiltPredictionExitCode.Environment, Query.YOLOTrainingRunExitCode(yOLOTrainingRunResult_NoPython));

                // The production weights are never a target, and a run name is a plain file name.
                Assert.Contains(nameof(Classes.YOLOTrainingRunOptions.RunName), (await RunAsync(Options("model", directory_Runs, path_Python, path_Start))).FailedStepNames);
                Assert.Contains(nameof(Classes.YOLOTrainingRunOptions.RunName), (await RunAsync(Options("a" + Path.DirectorySeparatorChar + "b", directory_Runs, path_Python, path_Start))).FailedStepNames);
                Assert.Contains(nameof(Classes.YOLOTrainingRunOptions.RunName), (await RunAsync(Options(null, directory_Runs, path_Python, path_Start))).FailedStepNames);

                // A project folder has to be absolute and may not be inside a YOLO\models folder.
                Assert.Contains(nameof(Classes.YOLOTrainingRunOptions.ProjectDirectory), (await RunAsync(Options("train9", "runs", path_Python, path_Start))).FailedStepNames);
                Classes.YOLOTrainingRunResult yOLOTrainingRunResult_Models = await RunAsync(Options("train9", Path.Combine(directory, "YOLO", "models"), path_Python, path_Start));
                Assert.Contains(nameof(Classes.YOLOTrainingRunOptions.ProjectDirectory), yOLOTrainingRunResult_Models.FailedStepNames);
                Assert.Equal(Enums.YearBuiltPredictionExitCode.Configuration, Query.YOLOTrainingRunExitCode(yOLOTrainingRunResult_Models));

                // A run that already exists is never overwritten.
                Directory.CreateDirectory(Path.Combine(directory_Runs, "train9"));
                Assert.Contains(nameof(Classes.YOLOTrainingRunOptions.RunName), (await RunAsync(Options("train9", directory_Runs, path_Python, path_Start))).FailedStepNames);

                // The start weights are hashed and reported under their full path.
                Classes.YOLOTrainingRunResult yOLOTrainingRunResult_Identity = await RunAsync(Options("train9", directory_Runs, Path.Combine(directory, "nope", "python.exe"), path_Start));
                Assert.Equal(path_Start, yOLOTrainingRunResult_Identity.StartWeightsPath);
                Assert.Equal(DiGi.YOLO.Query.FileSHA256(path_Start), yOLOTrainingRunResult_Identity.StartWeightsSHA256);

                // The dataset step needs a Web API client.
                Classes.YOLOTrainingRunOptions yOLOTrainingRunOptions_Dataset = Options("train9", directory_Runs, path_Python, path_Start);
                yOLOTrainingRunOptions_Dataset.Steps = [YOLOTrainingStep.Dataset];
                Assert.Contains("GISWebAPIManager", (await RunAsync(yOLOTrainingRunOptions_Dataset)).FailedStepNames);

                // An evaluation-only run never reaches the training, and a cancelled token ends it as cancelled.
                Classes.YOLOTrainingRunOptions yOLOTrainingRunOptions_Evaluate = Options(null, null, null, null);
                yOLOTrainingRunOptions_Evaluate.Steps = [YOLOTrainingStep.Evaluate];
                Classes.YOLOTrainingRunResult yOLOTrainingRunResult_Evaluate = await RunAsync(yOLOTrainingRunOptions_Evaluate);
                Assert.Contains(Query.YOLOTrainingStepName(YOLOTrainingStep.Evaluate), yOLOTrainingRunResult_Evaluate.FailedStepNames);
                Assert.DoesNotContain(Query.YOLOTrainingStepName(YOLOTrainingStep.Train), yOLOTrainingRunResult_Evaluate.FailedStepNames);

                using CancellationTokenSource cancellationTokenSource = new();
                cancellationTokenSource.Cancel();
                Classes.YOLOTrainingRunResult? yOLOTrainingRunResult_Cancelled = await Modify.RunYOLOTrainingAsync(null, yOLOTrainingRunOptions_Evaluate, cancellationToken: cancellationTokenSource.Token);
                Assert.NotNull(yOLOTrainingRunResult_Cancelled);
                Assert.True(yOLOTrainingRunResult_Cancelled!.Cancelled);
                Assert.Equal(Enums.YearBuiltPredictionExitCode.Cancelled, Query.YOLOTrainingRunExitCode(yOLOTrainingRunResult_Cancelled));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }
    }
}
