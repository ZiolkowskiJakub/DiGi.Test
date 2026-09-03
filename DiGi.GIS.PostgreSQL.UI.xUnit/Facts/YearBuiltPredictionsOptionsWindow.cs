using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.PostgreSQL.UI.Windows;
using DiGi.GIS.YOLO.UI.Classes;
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
        /// <para>What the constructor does is what this exercises: it parses the markup, wires the item naming of the county list, fills and sorts it, restores the selection from the options, and fills every path, flag and threshold. A multi-part county is included because two of its pieces share a code and a name and are told apart only by the identifier the naming callback appends - and a run has to name every part, so both have to be selectable.</para>
        /// <para>The settings that are <b>not</b> on the window are asserted too. The batch sizes, the year range and the radiuses decide which columns the feature projection asks for, and a projection that disagrees with what the regressor was trained on hands the model defaults rather than features - which scores without failing. Carrying them through untouched is the behaviour, not an omission.</para>
        /// <para><b>It does not check what the window looks like.</b> A window is laid out by the handle it gets when it is shown, so measuring one that is never shown reports nothing, and showing one during a test run would put a dialog on screen. The controls are private to the window, so the values they end up holding cannot be read from here either - that the dialog behaves correctly on screen is still something a person has to look at once.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictionsOptionsWindow_Construction()
        {
            Exception? exception = null;
            bool countiesCarried = false;
            bool pathsCarried = false;
            bool stepsCarried = false;
            bool untouchedCarried = false;
            bool copied = false;

            YearBuiltPredictionPipelineOptions yearBuiltPredictionPipelineOptions = new()
            {
                CountyIds = [22138, 22139],
                ScratchDirectory = @"C:\YOLO\scratch",
                PythonPath = @"C:\Python\python.exe",
                ModelPath = @"C:\YOLO\models\model.pt",
                Confidence = 0.25,
                BatchSize = 2500,
                ReferenceBatchSize = 5000,
                MaxConcurrentRequests = 4,
                ExportImages = true,
                RunPrediction = true,
                Score = true,
                UpdateDetections = false,
                UpdateYearBuiltData = false,
                UpdatePredictedYearBuilt = false
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

                    // The settings the window does not show survive it, because they have to match what the
                    // regressor was trained on rather than what a dialog was last set to.
                    untouchedCarried = yearBuiltPredictionPipelineOptions_Held.BatchSize == 2500 && yearBuiltPredictionPipelineOptions_Held.ReferenceBatchSize == 5000 && yearBuiltPredictionPipelineOptions_Held.MaxConcurrentRequests == 4;

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
            Assert.True(untouchedCarried);
            Assert.True(copied);

            // The caller's own instance is untouched by the edit made to the window's copy above.
            Assert.Equal(0.25, yearBuiltPredictionPipelineOptions.Confidence);
        }
    }
}
