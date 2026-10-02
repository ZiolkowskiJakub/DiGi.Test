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
        /// Verifies that the YOLO training options window carries the automatic-resume count and the stall limit: it opens with the previous run's values, the two inputs are enabled only while the Training step is ticked, and OK writes what was typed into the options.
        /// <para>The stall limit is typed in minutes and written as a <see cref="TimeSpan"/>; an empty box leaves it null, which the runner reads as its own default.</para>
        /// </summary>
        [Fact]
        public void YOLOTrainingOptionsWindow_AutomaticResumes()
        {
            Exception? exception = null;
            bool shownValues = false;
            bool enabledWithTrain = false;
            bool disabledWithoutTrain = false;
            int? autoResumeCount_Written = null;
            TimeSpan? inactivityTimeout_Written = null;

            YOLOTrainingRunOptions yOLOTrainingRunOptions = new()
            {
                AutoResumeCount = 7,
                InactivityTimeout = TimeSpan.FromMinutes(5),
                DatasetOptions = new YOLOTrainingDatasetOptions() { CountyIds = [22138], OutputDirectory = @"C:\YOLO\dataset" },
                ProjectDirectory = @"C:\YOLO\runs",
                RunName = "train9",
                Steps = [YOLOTrainingStep.Train]
            };

            Thread thread = new(() =>
            {
                try
                {
                    List<AdministrativeAreal2DReference> administrativeAreal2DReferences =
                    [
                        new() { Id = 22138, Code = "2412", Name = "rybnicki", AdministrativeArealType = AdministrativeArealType.County }
                    ];

                    YOLOTrainingOptionsWindow yOLOTrainingOptionsWindow = new(YOLOTrainingScenario.Retrain, yOLOTrainingRunOptions, administrativeAreal2DReferences, null);

                    TextBoxControl textBoxControl_AutoResumeCount = (TextBoxControl)yOLOTrainingOptionsWindow.FindName("TextBoxControl_AutoResumeCount")!;
                    TextBoxControl textBoxControl_InactivityTimeout = (TextBoxControl)yOLOTrainingOptionsWindow.FindName("TextBoxControl_InactivityTimeout")!;
                    System.Windows.Controls.CheckBox checkBox_Train = (System.Windows.Controls.CheckBox)yOLOTrainingOptionsWindow.FindName("CheckBox_Train")!;

                    shownValues = textBoxControl_AutoResumeCount.Value == "7" && textBoxControl_InactivityTimeout.Value == "5";
                    enabledWithTrain = textBoxControl_AutoResumeCount.IsEnabled && textBoxControl_InactivityTimeout.IsEnabled;

                    checkBox_Train.IsChecked = false;
                    disabledWithoutTrain = !textBoxControl_AutoResumeCount.IsEnabled && !textBoxControl_InactivityTimeout.IsEnabled;

                    checkBox_Train.IsChecked = true;
                    textBoxControl_AutoResumeCount.Value = "4";
                    textBoxControl_InactivityTimeout.Value = "10";

                    ((TextBoxControl)yOLOTrainingOptionsWindow.FindName("TextBoxControl_StartWeightsPath")!).Value = @"C:\YOLO\typed.pt";

                    try
                    {
                        ((System.Windows.Controls.Button)yOLOTrainingOptionsWindow.FindName("Button_OK")!).RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                    }
                    catch (InvalidOperationException)
                    {
                    }

                    autoResumeCount_Written = yOLOTrainingOptionsWindow.YOLOTrainingRunOptions.AutoResumeCount;
                    inactivityTimeout_Written = yOLOTrainingOptionsWindow.YOLOTrainingRunOptions.InactivityTimeout;
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
            Assert.True(shownValues);
            Assert.True(enabledWithTrain);
            Assert.True(disabledWithoutTrain);
            Assert.Equal(4, autoResumeCount_Written);
            Assert.Equal(TimeSpan.FromMinutes(10), inactivityTimeout_Written);
        }
    }
}
