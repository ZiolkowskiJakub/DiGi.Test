using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.PostgreSQL.UI.Enums;
using DiGi.GIS.PostgreSQL.UI.Windows;
using DiGi.GIS.YOLO.UI.Classes;
using DiGi.GIS.YOLO.UI.Enums;
using DiGi.UI.WPF.Controls;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the YOLO training options window can be built, that it holds a copy of the options it is given rather than the options themselves, and that it opens with the previous run's values rather than a scenario's defaults.
        /// <para>The dialog is handed the previous run's options every time the task is started, so a window that applied the scenario's defaults on opening would throw away the start weights and epochs chosen last time - and one that changed the instance it was given would do it even when the dialog is cancelled.</para>
        /// <para>Opened with no options, it must hold the scenario's defaults instead, including every step ticked: the runner reads an empty step list as every step. It does not check what the window looks like - a window is laid out when it is shown, and showing one would put a dialog on screen.</para>
        /// </summary>
        [Fact]
        public void YOLOTrainingOptionsWindow_Construction()
        {
            Exception? exception = null;
            bool copied = false;
            bool previousKept = false;
            bool scenarioKept = false;
            bool defaultsApplied = false;

            YOLOTrainingRunOptions yOLOTrainingRunOptions = new()
            {
                StartWeightsPath = @"C:\YOLO\previous.pt",
                Epochs = 12,
                RunName = "train9",
                ProjectDirectory = @"C:\YOLO\runs",
                Steps = [YOLOTrainingStep.Train],
                DatasetOptions = new YOLOTrainingDatasetOptions()
                {
                    CountyIds = [22138],
                    OutputDirectory = @"C:\YOLO\dataset",
                    Confidence = 0.2
                }
            };

            Thread thread = new(() =>
            {
                try
                {
                    List<AdministrativeAreal2DReference> administrativeAreal2DReferences =
                    [
                        new() { Id = 22138, Code = "2412", Name = "rybnicki", AdministrativeArealType = AdministrativeArealType.County },
                        new() { Id = 22139, Code = "2412", Name = "rybnicki", AdministrativeArealType = AdministrativeArealType.County }
                    ];

                    YOLOTrainingOptionsWindow yOLOTrainingOptionsWindow = new(YOLOTrainingScenario.Fresh, yOLOTrainingRunOptions, administrativeAreal2DReferences, null);

                    YOLOTrainingRunOptions yOLOTrainingRunOptions_Held = yOLOTrainingOptionsWindow.YOLOTrainingRunOptions;

                    copied = !ReferenceEquals(yOLOTrainingRunOptions, yOLOTrainingRunOptions_Held);
                    previousKept = yOLOTrainingRunOptions_Held.StartWeightsPath == @"C:\YOLO\previous.pt"
                        && yOLOTrainingRunOptions_Held.Epochs == 12
                        && yOLOTrainingRunOptions_Held.RunName == "train9"
                        && yOLOTrainingRunOptions_Held.DatasetOptions?.Confidence == 0.2
                        && yOLOTrainingRunOptions_Held.DatasetOptions.CountyIds is HashSet<int> countyIds && countyIds.Count == 1 && countyIds.Contains(22138);
                    scenarioKept = yOLOTrainingOptionsWindow.YOLOTrainingScenario == YOLOTrainingScenario.Fresh
                        && ((System.Windows.Controls.ComboBox)yOLOTrainingOptionsWindow.FindName("ComboBox_Scenario")!).SelectedIndex == 1;

                    yOLOTrainingRunOptions_Held.Epochs = 99;

                    YOLOTrainingOptionsWindow yOLOTrainingOptionsWindow_Default = new(YOLOTrainingScenario.Retrain, null, administrativeAreal2DReferences, null);
                    YOLOTrainingRunOptions yOLOTrainingRunOptions_Default = yOLOTrainingOptionsWindow_Default.YOLOTrainingRunOptions;
                    defaultsApplied = yOLOTrainingRunOptions_Default.Epochs == 150
                        && yOLOTrainingRunOptions_Default.DatasetOptions?.Resume == true
                        && yOLOTrainingRunOptions_Default.Steps?.Count == 5
                        && ((System.Windows.Controls.CheckBox)yOLOTrainingOptionsWindow_Default.FindName("CheckBox_Dataset")!).IsChecked == true
                        && ((System.Windows.Controls.CheckBox)yOLOTrainingOptionsWindow_Default.FindName("CheckBox_Evaluate")!).IsChecked == true;
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
            Assert.True(copied);
            Assert.True(previousKept);
            Assert.True(scenarioKept);
            Assert.True(defaultsApplied);

            Assert.Equal(12, yOLOTrainingRunOptions.Epochs);
        }

        /// <summary>
        /// Verifies that closing the YOLO training options window with OK writes every control into the options it holds, applies the chosen scenario, and leaves the members it has no control for unchanged.
        /// <para>The scenario is switched from Re-train to Start from yolo26x.pt before OK, and the start weights are then typed over: the switch has to decide the dataset folder's resume flag - off, so an existing dataset is refused - while the typed start weights, not the scenario's default, are what runs. <c>CountOnly</c> is handed in on and must come back off, and the unticked steps must be left out of a list the runner would otherwise read as every step.</para>
        /// <para>The click is raised on a window that was never shown, so the handler runs to its final DialogResult assignment, which throws on a window that was never shown as a dialog - by then every assignment has been made. The inputs are valid on purpose: a refusal would put a message box on screen and block the run rather than fail it, so the refusals are left to the person who opens the dialog once.</para>
        /// </summary>
        [Fact]
        public void YOLOTrainingOptionsWindow_Ok()
        {
            Exception? exception = null;
            YOLOTrainingRunOptions? yOLOTrainingRunOptions_Written = null;
            YOLOTrainingScenario yOLOTrainingScenario_Written = YOLOTrainingScenario.Retrain;

            YOLOTrainingRunOptions yOLOTrainingRunOptions = new()
            {
                Steps = [],
                DatasetOptions = new YOLOTrainingDatasetOptions()
                {
                    CountyIds = [22138, 22139],
                    CountOnly = true,
                    Resume = true,
                    Confidence = 0.2,
                    HoldoutDenominator = 7
                }
            };

            Thread thread = new(() =>
            {
                try
                {
                    List<AdministrativeAreal2DReference> administrativeAreal2DReferences =
                    [
                        new() { Id = 22138, Code = "2412", Name = "rybnicki", AdministrativeArealType = AdministrativeArealType.County },
                        new() { Id = 22139, Code = "2412", Name = "rybnicki", AdministrativeArealType = AdministrativeArealType.County }
                    ];

                    YOLOTrainingOptionsWindow yOLOTrainingOptionsWindow = new(YOLOTrainingScenario.Retrain, yOLOTrainingRunOptions, administrativeAreal2DReferences, null);

                    ((System.Windows.Controls.ComboBox)yOLOTrainingOptionsWindow.FindName("ComboBox_Scenario")!).SelectedIndex = 1;

                    void Set(string name, string value)
                    {
                        ((TextBoxControl)yOLOTrainingOptionsWindow.FindName(name)!).Value = value;
                    }

                    Set("TextBoxControl_DatasetDirectory", @"C:\YOLO\dataset_new");
                    Set("TextBoxControl_StartWeightsPath", @"C:\YOLO\typed.pt");
                    Set("TextBoxControl_Epochs", "250");
                    Set("TextBoxControl_Patience", "30");
                    Set("TextBoxControl_ImageSize", "800");
                    Set("TextBoxControl_Batch", "8");
                    Set("TextBoxControl_Seed", "3");
                    Set("TextBoxControl_Device", "0");
                    Set("TextBoxControl_RunName", "train10");
                    Set("TextBoxControl_ProjectDirectory", @"C:\YOLO\runs");
                    Set("TextBoxControl_PythonPath", @"C:\Python\python.exe");
                    Set("TextBoxControl_WorkingDirectory", " ");
                    Set("TextBoxControl_WeightsPaths", @"C:\YOLO\a.pt; ; C:\YOLO\b.pt");

                    ((System.Windows.Controls.CheckBox)yOLOTrainingOptionsWindow.FindName("CheckBox_Dataset")!).IsChecked = true;
                    ((System.Windows.Controls.CheckBox)yOLOTrainingOptionsWindow.FindName("CheckBox_LabelCheck")!).IsChecked = false;
                    ((System.Windows.Controls.CheckBox)yOLOTrainingOptionsWindow.FindName("CheckBox_Train")!).IsChecked = true;
                    ((System.Windows.Controls.CheckBox)yOLOTrainingOptionsWindow.FindName("CheckBox_Validate")!).IsChecked = false;
                    ((System.Windows.Controls.CheckBox)yOLOTrainingOptionsWindow.FindName("CheckBox_Evaluate")!).IsChecked = true;

                    try
                    {
                        ((System.Windows.Controls.Button)yOLOTrainingOptionsWindow.FindName("Button_OK")!).RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                    }
                    catch (InvalidOperationException)
                    {
                        // DialogResult refuses a window that was never shown as a dialog - by then every
                        // assignment the handler makes has already been made.
                    }

                    yOLOTrainingRunOptions_Written = yOLOTrainingOptionsWindow.YOLOTrainingRunOptions;
                    yOLOTrainingScenario_Written = yOLOTrainingOptionsWindow.YOLOTrainingScenario;
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
            Assert.NotNull(yOLOTrainingRunOptions_Written);

            Assert.Equal(YOLOTrainingScenario.Fresh, yOLOTrainingScenario_Written);
            Assert.Equal(@"C:\YOLO\typed.pt", yOLOTrainingRunOptions_Written!.StartWeightsPath);
            Assert.Equal(250, yOLOTrainingRunOptions_Written.Epochs);
            Assert.Equal(30, yOLOTrainingRunOptions_Written.Patience);
            Assert.Equal(800, yOLOTrainingRunOptions_Written.ImageSize);
            Assert.Equal(8, yOLOTrainingRunOptions_Written.Batch);
            Assert.Equal(3, yOLOTrainingRunOptions_Written.Seed);
            Assert.Equal("0", yOLOTrainingRunOptions_Written.Device);
            Assert.Equal("train10", yOLOTrainingRunOptions_Written.RunName);
            Assert.Equal(@"C:\YOLO\runs", yOLOTrainingRunOptions_Written.ProjectDirectory);
            Assert.Equal(@"C:\Python\python.exe", yOLOTrainingRunOptions_Written.PythonPath);
            Assert.Null(yOLOTrainingRunOptions_Written.WorkingDirectory);
            Assert.Equal<IEnumerable<YOLOTrainingStep>>([YOLOTrainingStep.Dataset, YOLOTrainingStep.Train, YOLOTrainingStep.Evaluate], yOLOTrainingRunOptions_Written.Steps!);

            YOLOTrainingDatasetOptions? yOLOTrainingDatasetOptions_Written = yOLOTrainingRunOptions_Written.DatasetOptions;
            Assert.NotNull(yOLOTrainingDatasetOptions_Written);
            Assert.Equal(@"C:\YOLO\dataset_new", yOLOTrainingDatasetOptions_Written!.OutputDirectory);
            Assert.Equal<IEnumerable<int>>([22138, 22139], yOLOTrainingDatasetOptions_Written.CountyIds!);
            Assert.Equal(@"C:\Python\python.exe", yOLOTrainingDatasetOptions_Written.PythonPath);
            Assert.Null(yOLOTrainingDatasetOptions_Written.WorkingDirectory);
            Assert.Equal<IEnumerable<string>>([@"C:\YOLO\a.pt", @"C:\YOLO\b.pt"], yOLOTrainingDatasetOptions_Written.WeightsPaths!);
            Assert.False(yOLOTrainingDatasetOptions_Written.Resume);
            Assert.False(yOLOTrainingDatasetOptions_Written.CountOnly);

            // No control for these: they survive the click unchanged.
            Assert.Equal(0.2, yOLOTrainingDatasetOptions_Written.Confidence);
            Assert.Equal(7, yOLOTrainingDatasetOptions_Written.HoldoutDenominator);

            // The caller's own instance is untouched.
            Assert.True(yOLOTrainingRunOptions.DatasetOptions!.CountOnly);
            Assert.Empty(yOLOTrainingRunOptions.Steps!);
        }
    }
}
