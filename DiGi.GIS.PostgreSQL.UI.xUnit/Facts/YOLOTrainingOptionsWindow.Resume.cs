using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.PostgreSQL.UI.Enums;
using DiGi.GIS.PostgreSQL.UI.Windows;
using DiGi.GIS.YOLO.UI.Classes;
using DiGi.GIS.YOLO.UI.Enums;
using DiGi.UI.WPF.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the YOLO training options window offers to resume an interrupted run: with a run folder holding an unfinished checkpoint it names the epoch and ceiling, ticking the box locks the hyperparameters and the dataset build and shows the checkpoint's values, and OK sets <c>ResumeTraining</c> with <c>Train</c> kept and <c>Dataset</c> dropped. A folder that is missing, holds a completed run, or holds a finished checkpoint offers nothing.
        /// <para>The checkpoint is read through a stand-in torch module written into the working directory, so no real ultralytics or GPU is needed. The interpreter is machine specific, so the fact returns without asserting when none is installed.</para>
        /// <para>Medium test (0.7 s): runs when DIGI_TEST_MAX_DURATION is Medium (the default) or Long.</para>
        /// </summary>
        [MediumFact]
        public void YOLOTrainingOptionsWindow_Resume()
        {
            string? pythonPath = PythonPath_Runnable();
            if (string.IsNullOrWhiteSpace(pythonPath))
            {
                return;
            }

            string directory = Path.Combine(Path.GetTempPath(), "DiGi.GIS.PostgreSQL.UI.xUnit.Resume." + Guid.NewGuid().ToString("N"));

            try
            {
                Directory.CreateDirectory(directory);

                string path_Configuration = Path.Combine(directory, "conf.yaml");
                File.WriteAllText(path_Configuration, "path: .\nnames:\n  0: Building");

                string projectDirectory = Path.Combine(directory, "runs", "detect");
                string directory_Run = Path.Combine(projectDirectory, "train9");
                Directory.CreateDirectory(Path.Combine(directory_Run, "weights"));
                string path_Last = Path.Combine(directory_Run, "weights", "last.pt");
                File.WriteAllText(path_Last, "dummy checkpoint");

                WriteMockTorch(directory);

                Exception? exception = null;
                bool offered = false;
                bool locked = false;
                bool values = false;
                bool ok = false;
                bool refusedWithoutFolder = false;
                bool refusedCompleted = false;
                bool refusedFinished = false;

                Thread thread = new(() =>
                {
                    try
                    {
                        List<AdministrativeAreal2DReference> administrativeAreal2DReferences =
                        [
                            new() { Id = 22138, Code = "2412", Name = "rybnicki", AdministrativeArealType = AdministrativeArealType.County }
                        ];

                        YOLOTrainingRunOptions Options()
                        {
                            return new YOLOTrainingRunOptions()
                            {
                                DatasetOptions = new YOLOTrainingDatasetOptions() { CountyIds = [22138], OutputDirectory = directory },
                                ProjectDirectory = projectDirectory,
                                PythonPath = pythonPath,
                                RunName = "train9",
                                StartWeightsPath = @"C:\YOLO\previous.pt",
                                Steps = [YOLOTrainingStep.Train, YOLOTrainingStep.Validate],
                                WorkingDirectory = directory
                            };
                        }

                        // An unfinished checkpoint is offered, with its epoch and ceiling.
                        WriteMockCheckpoints(directory, MockCheckpoint(0, 100, path_Configuration, projectDirectory, "train9", true));
                        YOLOTrainingOptionsWindow yOLOTrainingOptionsWindow = new(YOLOTrainingScenario.Retrain, Options(), administrativeAreal2DReferences, null);

                        System.Windows.Controls.CheckBox checkBox_Resume = (System.Windows.Controls.CheckBox)yOLOTrainingOptionsWindow.FindName("CheckBox_ResumeInterruptedRun")!;
                        System.Windows.Controls.TextBlock textBlock_Interrupted = (System.Windows.Controls.TextBlock)yOLOTrainingOptionsWindow.FindName("TextBlock_InterruptedRun")!;
                        offered = checkBox_Resume.Visibility == System.Windows.Visibility.Visible
                            && textBlock_Interrupted.Text.Contains("epoch 1 of 100", StringComparison.Ordinal);

                        // Ticking the box locks what a resume cannot change and shows the checkpoint's values.
                        checkBox_Resume.IsChecked = true;

                        System.Windows.Controls.CheckBox checkBox_Dataset = (System.Windows.Controls.CheckBox)yOLOTrainingOptionsWindow.FindName("CheckBox_Dataset")!;
                        TextBoxControl textBoxControl_Epochs = (TextBoxControl)yOLOTrainingOptionsWindow.FindName("TextBoxControl_Epochs")!;
                        TextBoxControl textBoxControl_Patience = (TextBoxControl)yOLOTrainingOptionsWindow.FindName("TextBoxControl_Patience")!;
                        TextBoxControl textBoxControl_ImageSize = (TextBoxControl)yOLOTrainingOptionsWindow.FindName("TextBoxControl_ImageSize")!;
                        TextBoxControl textBoxControl_Batch = (TextBoxControl)yOLOTrainingOptionsWindow.FindName("TextBoxControl_Batch")!;
                        TextBoxControl textBoxControl_Seed = (TextBoxControl)yOLOTrainingOptionsWindow.FindName("TextBoxControl_Seed")!;

                        locked = !textBoxControl_Epochs.IsEnabled
                            && !textBoxControl_Patience.IsEnabled
                            && !textBoxControl_ImageSize.IsEnabled
                            && !textBoxControl_Batch.IsEnabled
                            && !textBoxControl_Seed.IsEnabled
                            && !checkBox_Dataset.IsEnabled
                            && checkBox_Dataset.IsChecked == false;

                        values = textBoxControl_Epochs.Value == "100"
                            && textBoxControl_Patience.Value == "40"
                            && textBoxControl_ImageSize.Value == "800"
                            && textBoxControl_Batch.Value == "8"
                            && textBoxControl_Seed.Value == "3";

                        try
                        {
                            ((System.Windows.Controls.Button)yOLOTrainingOptionsWindow.FindName("Button_OK")!).RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                        }
                        catch (InvalidOperationException)
                        {
                        }

                        YOLOTrainingRunOptions yOLOTrainingRunOptions_Written = yOLOTrainingOptionsWindow.YOLOTrainingRunOptions;
                        ok = yOLOTrainingRunOptions_Written.ResumeTraining
                            && yOLOTrainingRunOptions_Written.Steps is List<YOLOTrainingStep> steps_Train
                            && steps_Train.Contains(YOLOTrainingStep.Train)
                            && !steps_Train.Contains(YOLOTrainingStep.Dataset);

                        // No folder: nothing is offered.
                        Directory.Delete(directory_Run, true);
                        YOLOTrainingOptionsWindow yOLOTrainingOptionsWindow_NoFolder = new(YOLOTrainingScenario.Retrain, Options(), administrativeAreal2DReferences, null);
                        refusedWithoutFolder = ((System.Windows.Controls.CheckBox)yOLOTrainingOptionsWindow_NoFolder.FindName("CheckBox_ResumeInterruptedRun")!).Visibility == System.Windows.Visibility.Collapsed;

                        // A completed run is not offered.
                        Directory.CreateDirectory(Path.Combine(directory_Run, "weights"));
                        File.WriteAllText(path_Last, "dummy checkpoint");
                        File.WriteAllText(Path.Combine(directory_Run, "train9.pt"), "completed");
                        YOLOTrainingOptionsWindow yOLOTrainingOptionsWindow_Completed = new(YOLOTrainingScenario.Retrain, Options(), administrativeAreal2DReferences, null);
                        refusedCompleted = ((System.Windows.Controls.CheckBox)yOLOTrainingOptionsWindow_Completed.FindName("CheckBox_ResumeInterruptedRun")!).Visibility == System.Windows.Visibility.Collapsed;
                        File.Delete(Path.Combine(directory_Run, "train9.pt"));

                        // A finished checkpoint is not offered.
                        WriteMockCheckpoints(directory, MockCheckpoint(-1, 100, path_Configuration, projectDirectory, "train9", false));
                        YOLOTrainingOptionsWindow yOLOTrainingOptionsWindow_Finished = new(YOLOTrainingScenario.Retrain, Options(), administrativeAreal2DReferences, null);
                        refusedFinished = ((System.Windows.Controls.CheckBox)yOLOTrainingOptionsWindow_Finished.FindName("CheckBox_ResumeInterruptedRun")!).Visibility == System.Windows.Visibility.Collapsed;
                    }
                    catch (Exception exception_Temp)
                    {
                        exception = exception_Temp;
                    }
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();

                Assert.Null(exception);
                Assert.True(offered);
                Assert.True(locked);
                Assert.True(values);
                Assert.True(ok);
                Assert.True(refusedWithoutFolder);
                Assert.True(refusedCompleted);
                Assert.True(refusedFinished);
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
        /// Returns the first interpreter on PATH that actually starts, or <c>null</c> when none does.
        /// <para>A path that exists is not enough: with no Python installed, Windows still puts the App Installer redirector (a Microsoft Store app execution alias) on PATH as <c>python.exe</c>, which exits non-zero without running anything.</para>
        /// </summary>
        /// <returns>The path of a working interpreter, or <c>null</c>.</returns>
        private static string? PythonPath_Runnable()
        {
            foreach (string pythonPath in DiGi.YOLO.Query.PythonPaths())
            {
                (int exitCode, _, _) = DiGi.YOLO.Query.ExecuteProcess(pythonPath, "-c \"import sys\"", Path.GetTempPath());
                if (exitCode == 0)
                {
                    return pythonPath;
                }
            }

            return null;
        }

        /// <summary>
        /// Writes a stand-in <c>torch</c> module into the working directory, so <c>checkpoint.py</c> reads the checkpoint the accompanying <c>checkpoints.json</c> describes without real torch.
        /// </summary>
        /// <param name="directory">The working directory the scripts run in.</param>
        private static void WriteMockTorch(string directory)
        {
            File.WriteAllLines(Path.Combine(directory, "torch.py"),
            [
                "import json, os",
                "",
                "__version__ = '0.0.0'",
                "",
                "class _Cuda:",
                "    @staticmethod",
                "    def is_available():",
                "        return False",
                "",
                "cuda = _Cuda()",
                "",
                "with open(os.path.join(os.path.dirname(os.path.abspath(__file__)), 'checkpoints.json'), 'r', encoding='utf-8') as file:",
                "    _CHECKPOINTS = json.load(file)",
                "",
                "def load(path, map_location=None, weights_only=False):",
                "    return _CHECKPOINTS.get(os.path.basename(path), {})",
            ]);
        }

        /// <summary>
        /// Writes the single checkpoint the stand-in torch module answers for <c>last.pt</c>, so a case can be swapped without touching the module.
        /// </summary>
        /// <param name="directory">The working directory the scripts run in.</param>
        /// <param name="checkpoint">The checkpoint dictionary, or <c>null</c> for one torch answers as unreadable.</param>
        private static void WriteMockCheckpoints(string directory, JsonNode? checkpoint)
        {
            JsonObject checkpoints = new()
            {
                ["last.pt"] = checkpoint
            };

            File.WriteAllText(Path.Combine(directory, "checkpoints.json"), checkpoints.ToJsonString());
        }

        /// <summary>
        /// Builds one stand-in checkpoint dictionary in the shape ultralytics stores, with the arguments a resume restores so the dialog can show them.
        /// </summary>
        /// <param name="epoch">The stored 0-based epoch, or -1 for a finished checkpoint.</param>
        /// <param name="epochs">The epoch ceiling recorded in train_args.</param>
        /// <param name="data">The dataset path recorded in train_args.</param>
        /// <param name="project">The run directory parent recorded in train_args.</param>
        /// <param name="name">The run directory name recorded in train_args.</param>
        /// <param name="optimizer">Whether the checkpoint carries optimizer state.</param>
        /// <returns>The checkpoint dictionary.</returns>
        private static JsonObject MockCheckpoint(int epoch, int epochs, string data, string project, string name, bool optimizer)
        {
            return new JsonObject()
            {
                ["epoch"] = epoch,
                ["optimizer"] = optimizer ? new JsonObject() { ["state"] = new JsonObject() } : null,
                ["best_fitness"] = 0.5,
                ["train_args"] = new JsonObject()
                {
                    ["data"] = data,
                    ["epochs"] = epochs,
                    ["patience"] = 40,
                    ["imgsz"] = 800,
                    ["batch"] = 8,
                    ["seed"] = 3,
                    ["project"] = project,
                    ["name"] = name
                }
            };
        }
    }
}
