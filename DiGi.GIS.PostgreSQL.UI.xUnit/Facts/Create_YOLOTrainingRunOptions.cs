using DiGi.GIS.PostgreSQL.UI.Enums;
using DiGi.GIS.YOLO.UI.Classes;
using DiGi.GIS.YOLO.UI.Enums;
using System;
using System.Collections.Generic;
using System.IO;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the defaults each training scenario applies, and that switching scenario keeps what the operator chose.
        /// <para>A runner folder is laid out in temp the way the build deploys one - the git-ignored "user files" flattened into the output root - holding a model.pt but no yolo26x.pt. Re-train must then start from that model.pt, resolved to its absolute path, and resume the dataset; Start from yolo26x.pt must start from the absolute place the base checkpoint would be, so a preflight can name it, and must not resume. Both epoch ceilings are asserted, as is the incumbent the evaluation gates against.</para>
        /// <para>Two defaults are asserted against the class and the template rather than for themselves: <c>CountOnly</c> is handed in on, as the committed runner template ships it, and must come back off, because a counting run trains nothing; and an empty step list, which the runner reads as every step, must come back as the five steps written out.</para>
        /// <para>The switch is the other half: counties, folders, device and interpreter handed in must survive both scenarios, and the options handed in must not be changed, because the dialog is given the previous run's options and a cancelled dialog has to leave them alone.</para>
        /// </summary>
        [Fact]
        public void Create_YOLOTrainingRunOptions()
        {
            string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            string path_ConsoleApp = Path.Combine(directory, Constants.FileName.YearBuiltPredictionConsoleApp);
            string path_Model = Path.Combine(directory, "YOLO", "models", Constants.FileName.Model);
            string path_BaseModel = Path.Combine(directory, "YOLO", "models", Constants.DirectoryName.BaseModels, Constants.FileName.BaseModel);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path_Model)!);
                File.WriteAllText(path_ConsoleApp, string.Empty);
                File.WriteAllText(path_Model, string.Empty);

                YOLOTrainingRunOptions yOLOTrainingRunOptions_Retrain = Create.YOLOTrainingRunOptions(YOLOTrainingScenario.Retrain, path_ConsoleApp);
                Assert.Equal(path_Model, yOLOTrainingRunOptions_Retrain.StartWeightsPath);
                Assert.Equal(150, yOLOTrainingRunOptions_Retrain.Epochs);
                Assert.Equal(50, yOLOTrainingRunOptions_Retrain.Patience);
                Assert.Equal(640, yOLOTrainingRunOptions_Retrain.ImageSize);
                Assert.Equal(16, yOLOTrainingRunOptions_Retrain.Batch);
                Assert.Equal(0, yOLOTrainingRunOptions_Retrain.Seed);
                Assert.Equal(3, yOLOTrainingRunOptions_Retrain.AutoResumeCount);
                Assert.Null(yOLOTrainingRunOptions_Retrain.InactivityTimeout);
                Assert.NotNull(yOLOTrainingRunOptions_Retrain.DatasetOptions);
                Assert.True(yOLOTrainingRunOptions_Retrain.DatasetOptions!.Resume);
                Assert.False(yOLOTrainingRunOptions_Retrain.DatasetOptions.CountOnly);
                Assert.Equal<IEnumerable<string>>([path_Model], yOLOTrainingRunOptions_Retrain.DatasetOptions.WeightsPaths!);
                Assert.Equal<IEnumerable<YOLOTrainingStep>>([YOLOTrainingStep.Dataset, YOLOTrainingStep.LabelCheck, YOLOTrainingStep.Train, YOLOTrainingStep.Validate, YOLOTrainingStep.Evaluate], yOLOTrainingRunOptions_Retrain.Steps!);

                // The base checkpoint is absent from the runner folder: the default is still the absolute place it
                // would be, so that the refusal names it.
                YOLOTrainingRunOptions yOLOTrainingRunOptions_Fresh = Create.YOLOTrainingRunOptions(YOLOTrainingScenario.Fresh, path_ConsoleApp);
                Assert.Equal(path_BaseModel, yOLOTrainingRunOptions_Fresh.StartWeightsPath);
                Assert.Equal(300, yOLOTrainingRunOptions_Fresh.Epochs);
                Assert.False(yOLOTrainingRunOptions_Fresh.DatasetOptions!.Resume);
                Assert.False(yOLOTrainingRunOptions_Fresh.DatasetOptions.CountOnly);
                Assert.Equal<IEnumerable<string>>([path_Model], yOLOTrainingRunOptions_Fresh.DatasetOptions.WeightsPaths!);

                // Present, it is found where the deployed layout puts it.
                Directory.CreateDirectory(Path.GetDirectoryName(path_BaseModel)!);
                File.WriteAllText(path_BaseModel, string.Empty);
                Assert.Equal(path_BaseModel, Create.YOLOTrainingRunOptions(YOLOTrainingScenario.Fresh, path_ConsoleApp).StartWeightsPath);

                YOLOTrainingRunOptions yOLOTrainingRunOptions_Previous = new()
                {
                    ProjectDirectory = @"C:\YOLO\runs",
                    RunName = "train9",
                    Device = "cpu",
                    PythonPath = @"C:\Python\python.exe",
                    Epochs = 12,
                    StartWeightsPath = @"C:\YOLO\previous.pt",
                    AutoResumeCount = 7,
                    InactivityTimeout = TimeSpan.FromMinutes(5),
                    Steps = [],
                    DatasetOptions = new YOLOTrainingDatasetOptions()
                    {
                        CountyIds = [4816],
                        OutputDirectory = @"C:\YOLO\dataset",
                        CountOnly = true,
                        Resume = true,
                        WeightsPaths = [@"C:\YOLO\gate.pt"]
                    }
                };

                YOLOTrainingScenario[] yOLOTrainingScenarios = [YOLOTrainingScenario.Retrain, YOLOTrainingScenario.Fresh];
                foreach (YOLOTrainingScenario yOLOTrainingScenario in yOLOTrainingScenarios)
                {
                    YOLOTrainingRunOptions yOLOTrainingRunOptions_Switched = Create.YOLOTrainingRunOptions(yOLOTrainingScenario, path_ConsoleApp, yOLOTrainingRunOptions_Previous);

                    Assert.NotSame(yOLOTrainingRunOptions_Previous, yOLOTrainingRunOptions_Switched);
                    Assert.Equal(@"C:\YOLO\runs", yOLOTrainingRunOptions_Switched.ProjectDirectory);
                    Assert.Equal("train9", yOLOTrainingRunOptions_Switched.RunName);
                    Assert.Equal("cpu", yOLOTrainingRunOptions_Switched.Device);
                    Assert.Equal(@"C:\Python\python.exe", yOLOTrainingRunOptions_Switched.PythonPath);
                    Assert.Equal(7, yOLOTrainingRunOptions_Switched.AutoResumeCount);
                    Assert.Equal(TimeSpan.FromMinutes(5), yOLOTrainingRunOptions_Switched.InactivityTimeout);
                    Assert.Equal<IEnumerable<int>>([4816], yOLOTrainingRunOptions_Switched.DatasetOptions!.CountyIds!);
                    Assert.Equal(@"C:\YOLO\dataset", yOLOTrainingRunOptions_Switched.DatasetOptions.OutputDirectory);
                    Assert.Equal<IEnumerable<string>>([@"C:\YOLO\gate.pt"], yOLOTrainingRunOptions_Switched.DatasetOptions.WeightsPaths!);
                    Assert.False(yOLOTrainingRunOptions_Switched.DatasetOptions.CountOnly);
                    Assert.Equal(5, yOLOTrainingRunOptions_Switched.Steps!.Count);
                    Assert.Equal(yOLOTrainingScenario == YOLOTrainingScenario.Retrain, yOLOTrainingRunOptions_Switched.DatasetOptions.Resume);
                    Assert.Equal(yOLOTrainingScenario == YOLOTrainingScenario.Retrain ? path_Model : path_BaseModel, yOLOTrainingRunOptions_Switched.StartWeightsPath);
                }

                // The options handed in are the previous run's, and a cancelled dialog must leave them as they were.
                Assert.Equal(12, yOLOTrainingRunOptions_Previous.Epochs);
                Assert.Equal(@"C:\YOLO\previous.pt", yOLOTrainingRunOptions_Previous.StartWeightsPath);
                Assert.Equal(7, yOLOTrainingRunOptions_Previous.AutoResumeCount);
                Assert.Equal(TimeSpan.FromMinutes(5), yOLOTrainingRunOptions_Previous.InactivityTimeout);
                Assert.Empty(yOLOTrainingRunOptions_Previous.Steps!);
                Assert.True(yOLOTrainingRunOptions_Previous.DatasetOptions!.CountOnly);
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
