using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.PostgreSQL.UI.Windows;
using DiGi.GIS.YOLO.UI.Classes;
using DiGi.UI.WPF.Controls;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the Year Built prediction options window can be built and that it works on a copy of the options it is given, without a database or a running application.
        /// <para>The window is the only way a run started from the tray application can be scoped, and a run writes stored production data. A window whose markup failed to parse, or whose constructor threw on the county list, would not be found until someone opened it to scope a run and could not.</para>
        /// <para>What the constructor does is what this exercises: it parses the markup, wires the item naming of the county list, fills and sorts it, restores the selection from the options, and fills every path, flag, threshold, concurrency and batch size. A multi-part county is included because two of its pieces share a code and a name and are told apart only by the identifier the naming callback appends - and a run has to name every part, so both have to be selectable.</para>
        /// <para>The settings that are <b>not</b> on the window are asserted too. The year range and the radiuses decide which columns the feature projection asks for, and a projection that disagrees with what the regressor was trained on hands the model defaults rather than features - which scores without failing. Carrying them through untouched is the behaviour, not an omission.</para>
        /// <para><b>It does not check what the window looks like.</b> A window is laid out by the handle it gets when it is shown, so measuring one that is never shown reports nothing, and showing one during a test run would put a dialog on screen. The controls are private to the window, so the values they end up holding cannot be read from here either - that the dialog behaves correctly on screen is still something a person has to look at once.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictionsOptionsWindow_Construction()
        {
            Exception? exception = null;
            bool countiesCarried = false;
            bool pathsCarried = false;
            bool stepsCarried = false;
            bool settingsCarried = false;
            bool untouchedCarried = false;
            bool copied = false;

            YearBuiltPredictionPipelineOptions yearBuiltPredictionPipelineOptions = new()
            {
                CountyIds = [22138, 22139],
                ScratchDirectory = @"C:\YOLO\scratch",
                PythonPath = @"C:\Python\python.exe",
                ModelPath = @"C:\YOLO\models\model.pt",
                Confidence = 0.25,
                MaxConcurrentRequests = 4,
                BatchSize = 2500,
                ReferenceBatchSize = 5000,
                CleanScratchDirectory = false,
                ExportImages = true,
                RunPrediction = true,
                Score = true,
                UpdateDetections = false,
                UpdateYearBuiltData = false,
                UpdatePredictedYearBuilt = false,
                Years = new DiGi.Core.Classes.Range<int>(1900, 2000),
                Radiuses = [1.0, 2.0]
            };

            Thread thread = new(() =>
            {
                try
                {
                    List<AdministrativeAreal2DReference> administrativeAreal2DReferences =
                    [
                        new() { Id = 55417, Code = "1465", Name = "m. St. Warszawa", AdministrativeArealType = AdministrativeArealType.County },
                        new() { Id = 4816, Code = "0201", Name = "boleslawiecki", AdministrativeArealType = AdministrativeArealType.County },
                        // Two pieces of one multi-part county: same code and name, told apart only by the identifier.
                        new() { Id = 22138, Code = "2412", Name = "rybnicki", AdministrativeArealType = AdministrativeArealType.County },
                        new() { Id = 22139, Code = "2412", Name = "rybnicki", AdministrativeArealType = AdministrativeArealType.County }
                    ];

                    YearBuiltPredictionsOptionsWindow yearBuiltPredictionsOptionsWindow = new(yearBuiltPredictionPipelineOptions, administrativeAreal2DReferences);

                    YearBuiltPredictionPipelineOptions yearBuiltPredictionPipelineOptions_Held = yearBuiltPredictionsOptionsWindow.YearBuiltPredictionPipelineOptions;

                    // Until OK is pressed the window holds what it was given.
                    countiesCarried = yearBuiltPredictionPipelineOptions_Held.CountyIds is HashSet<int> countyIds && countyIds.Count == 2 && countyIds.Contains(22138) && countyIds.Contains(22139);
                    pathsCarried = yearBuiltPredictionPipelineOptions_Held.ScratchDirectory == @"C:\YOLO\scratch" && yearBuiltPredictionPipelineOptions_Held.PythonPath == @"C:\Python\python.exe" && yearBuiltPredictionPipelineOptions_Held.ModelPath == @"C:\YOLO\models\model.pt" && yearBuiltPredictionPipelineOptions_Held.WorkingDirectory is null;
                    stepsCarried = yearBuiltPredictionPipelineOptions_Held.ExportImages && yearBuiltPredictionPipelineOptions_Held.RunPrediction && yearBuiltPredictionPipelineOptions_Held.Score && !yearBuiltPredictionPipelineOptions_Held.UpdateDetections && !yearBuiltPredictionPipelineOptions_Held.UpdateYearBuiltData && !yearBuiltPredictionPipelineOptions_Held.UpdatePredictedYearBuilt;

                    // The concurrency and the two batch sizes are on the window now; before OK they are whatever
                    // the run was given, which is what the dialog pre-fills the boxes with.
                    settingsCarried = yearBuiltPredictionPipelineOptions_Held.MaxConcurrentRequests == 4 && yearBuiltPredictionPipelineOptions_Held.BatchSize == 2500 && yearBuiltPredictionPipelineOptions_Held.ReferenceBatchSize == 5000 && !yearBuiltPredictionPipelineOptions_Held.CleanScratchDirectory;

                    // The settings the window does not show survive it, because they have to match what the
                    // regressor was trained on rather than what a dialog was last set to.
                    untouchedCarried = yearBuiltPredictionPipelineOptions_Held.Years is DiGi.Core.Classes.Range<int> years && years.Min == 1900 && years.Max == 2000 && yearBuiltPredictionPipelineOptions_Held.Radiuses is not null && yearBuiltPredictionPipelineOptions_Held.Radiuses.Count == 2 && yearBuiltPredictionPipelineOptions_Held.Radiuses[0] == 1.0 && yearBuiltPredictionPipelineOptions_Held.Radiuses[1] == 2.0;

                    // A cancelled dialog has to leave the caller's options alone, which only holds if the window
                    // took a copy rather than a reference.
                    copied = !ReferenceEquals(yearBuiltPredictionPipelineOptions, yearBuiltPredictionPipelineOptions_Held);
                    yearBuiltPredictionPipelineOptions_Held.Confidence = 0.9;
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
            Assert.True(countiesCarried);
            Assert.True(pathsCarried);
            Assert.True(stepsCarried);
            Assert.True(settingsCarried);
            Assert.True(untouchedCarried);
            Assert.True(copied);

            // The caller's own instance is untouched by the edit made to the window's copy above.
            Assert.Equal(0.25, yearBuiltPredictionPipelineOptions.Confidence);
        }

        /// <summary>
        /// Verifies that closing the Year Built prediction options window with OK writes every control into the options the window holds, and that the members the window has no control for survive it unchanged.
        /// <para>The round trip is the whole contract the run depends on: whatever the operator chose in the dialog is what the options file written beside the scratch directory carries, and a member the dialog deliberately does not settle - the year range and the radiuses, which have to match what the regressor was trained on (ZiolkowskiJakub/DiGi.GIS.ML#6) - must come through the copy exactly as it was handed in.</para>
        /// <para>The click is raised on a window that was never shown, so the handler runs to its final DialogResult assignment, which throws on a window that was never shown as a dialog - everything the handler writes has already been written by then, which is what makes the thrown half harmless here. The inputs are all valid on purpose: a validation failure would put a message box on screen and block the run rather than fail it.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictionsOptionsWindow_Ok()
        {
            Exception? exception = null;
            bool pathsWritten = false;
            bool numericWritten = false;
            bool stepsWritten = false;
            bool countiesCarried = false;
            bool untouchedSurvived = false;

            YearBuiltPredictionPipelineOptions yearBuiltPredictionPipelineOptions = new()
            {
                CountyIds = [22138, 22139],
                Years = new DiGi.Core.Classes.Range<int>(1900, 2000),
                Radiuses = [1.0, 2.0]
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

                    YearBuiltPredictionsOptionsWindow yearBuiltPredictionsOptionsWindow = new(yearBuiltPredictionPipelineOptions, administrativeAreal2DReferences);

                    ((TextBoxControl)yearBuiltPredictionsOptionsWindow.FindName("TextBoxControl_ScratchDirectory")!).Value = @"C:\YOLO\scratch_ok";
                    ((TextBoxControl)yearBuiltPredictionsOptionsWindow.FindName("TextBoxControl_PythonPath")!).Value = @"C:\Python\python.exe";
                    ((TextBoxControl)yearBuiltPredictionsOptionsWindow.FindName("TextBoxControl_ModelPath")!).Value = @"C:\YOLO\models\model_ok.pt";

                    // Whitespace only, because an empty path is not a path: the handler is meant to turn this into null.
                    ((TextBoxControl)yearBuiltPredictionsOptionsWindow.FindName("TextBoxControl_WorkingDirectory")!).Value = " ";
                    ((TextBoxControl)yearBuiltPredictionsOptionsWindow.FindName("TextBoxControl_Confidence")!).Value = "0.75";
                    ((TextBoxControl)yearBuiltPredictionsOptionsWindow.FindName("TextBoxControl_MaxConcurrentRequests")!).Value = "3";
                    ((TextBoxControl)yearBuiltPredictionsOptionsWindow.FindName("TextBoxControl_BatchSize")!).Value = "1250";
                    ((TextBoxControl)yearBuiltPredictionsOptionsWindow.FindName("TextBoxControl_ReferenceBatchSize")!).Value = "9000";

                    ((System.Windows.Controls.CheckBox)yearBuiltPredictionsOptionsWindow.FindName("CheckBox_ExportImages")!).IsChecked = false;
                    ((System.Windows.Controls.CheckBox)yearBuiltPredictionsOptionsWindow.FindName("CheckBox_RunPrediction")!).IsChecked = true;
                    ((System.Windows.Controls.CheckBox)yearBuiltPredictionsOptionsWindow.FindName("CheckBox_Score")!).IsChecked = false;
                    ((System.Windows.Controls.CheckBox)yearBuiltPredictionsOptionsWindow.FindName("CheckBox_Resume")!).IsChecked = false;
                    ((System.Windows.Controls.CheckBox)yearBuiltPredictionsOptionsWindow.FindName("CheckBox_CleanScratchDirectory")!).IsChecked = false;
                    ((System.Windows.Controls.CheckBox)yearBuiltPredictionsOptionsWindow.FindName("CheckBox_UpdateDetections")!).IsChecked = true;
                    ((System.Windows.Controls.CheckBox)yearBuiltPredictionsOptionsWindow.FindName("CheckBox_UpdateYearBuiltData")!).IsChecked = false;
                    ((System.Windows.Controls.CheckBox)yearBuiltPredictionsOptionsWindow.FindName("CheckBox_UpdatePredictedYearBuilt")!).IsChecked = true;

                    try
                    {
                        ((System.Windows.Controls.Button)yearBuiltPredictionsOptionsWindow.FindName("Button_OK")!).RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                    }
                    catch (InvalidOperationException)
                    {
                        // DialogResult refuses a window that was never shown as a dialog - by then every
                        // assignment the handler makes has already been made.
                    }

                    YearBuiltPredictionPipelineOptions yearBuiltPredictionPipelineOptions_Written = yearBuiltPredictionsOptionsWindow.YearBuiltPredictionPipelineOptions;

                    pathsWritten = yearBuiltPredictionPipelineOptions_Written.ScratchDirectory == @"C:\YOLO\scratch_ok" && yearBuiltPredictionPipelineOptions_Written.PythonPath == @"C:\Python\python.exe" && yearBuiltPredictionPipelineOptions_Written.ModelPath == @"C:\YOLO\models\model_ok.pt" && yearBuiltPredictionPipelineOptions_Written.WorkingDirectory is null;
                    numericWritten = yearBuiltPredictionPipelineOptions_Written.Confidence == 0.75 && yearBuiltPredictionPipelineOptions_Written.MaxConcurrentRequests == 3 && yearBuiltPredictionPipelineOptions_Written.BatchSize == 1250 && yearBuiltPredictionPipelineOptions_Written.ReferenceBatchSize == 9000;
                    stepsWritten = !yearBuiltPredictionPipelineOptions_Written.ExportImages && yearBuiltPredictionPipelineOptions_Written.RunPrediction && !yearBuiltPredictionPipelineOptions_Written.Score && !yearBuiltPredictionPipelineOptions_Written.Resume && !yearBuiltPredictionPipelineOptions_Written.CleanScratchDirectory && yearBuiltPredictionPipelineOptions_Written.UpdateDetections && !yearBuiltPredictionPipelineOptions_Written.UpdateYearBuiltData && yearBuiltPredictionPipelineOptions_Written.UpdatePredictedYearBuilt;
                    countiesCarried = yearBuiltPredictionPipelineOptions_Written.CountyIds is HashSet<int> countyIds && countyIds.Count == 2 && countyIds.Contains(22138) && countyIds.Contains(22139);

                    // The members the window deliberately has no control for survive the click unchanged.
                    untouchedSurvived = yearBuiltPredictionPipelineOptions_Written.Years is DiGi.Core.Classes.Range<int> years && years.Min == 1900 && years.Max == 2000 && yearBuiltPredictionPipelineOptions_Written.Radiuses is not null && yearBuiltPredictionPipelineOptions_Written.Radiuses.Count == 2 && yearBuiltPredictionPipelineOptions_Written.Radiuses[0] == 1.0 && yearBuiltPredictionPipelineOptions_Written.Radiuses[1] == 2.0;
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
            Assert.True(pathsWritten);
            Assert.True(numericWritten);
            Assert.True(stepsWritten);
            Assert.True(countiesCarried);
            Assert.True(untouchedSurvived);
        }
    }
}
