using DiGi.GIS.YOLO.UI.Classes;
using DiGi.GIS.YOLO.UI.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the training options the tray task writes are the options the headless runner's <c>--train</c> mode reads back, nested dataset options included.
        /// <para>The file is written the way the task writes it - the serialized form, <c>_type</c> keys and all - and read the way the runner reads it, through <c>DiGi.GIS.YOLO.UI.Query.YOLOTrainingRunOptions</c>, which takes the nested <c>DatasetOptions</c> out and builds it separately. Nothing links the two at compile time and the reader drops any key it does not know in silence, so a member lost on the way is a run that trains with a default nobody chose.</para>
        /// <para>Every member is set to a non-default value, and <c>Steps</c> to a subset in which <c>Dataset</c> - the value an unreadable step falls back to - is absent, so a step that did not survive would show up as one that did not belong. <c>Resume</c> and <c>CountOnly</c> are handed over the opposite way round from their defaults: those two decide whether an existing dataset is appended to and whether anything is built at all.</para>
        /// </summary>
        [Fact]
        public void YOLOTrainingRunOptions_Handover()
        {
            YOLOTrainingRunOptions yOLOTrainingRunOptions = new()
            {
                StartWeightsPath = @"C:\YOLO\models\base\yolo26x.pt",
                RunName = "train_2026_09",
                ProjectDirectory = @"C:\YOLO\runs",
                Epochs = 300,
                Patience = 40,
                ImageSize = 800,
                Batch = 8,
                Seed = 7,
                Device = "0",
                PythonPath = @"C:\Python\python.exe",
                WorkingDirectory = @"C:\YOLO\working",
                ResumeTraining = true,
                Steps = [YOLOTrainingStep.LabelCheck, YOLOTrainingStep.Train, YOLOTrainingStep.Evaluate],
                DatasetOptions = new YOLOTrainingDatasetOptions()
                {
                    CountyIds = [22138, 22139],
                    OutputDirectory = @"C:\YOLO\dataset",
                    Resume = false,
                    CountOnly = true,
                    PythonPath = @"C:\Python\python.exe",
                    WorkingDirectory = @"C:\YOLO\working",
                    ModelPath = @"C:\YOLO\models\model.pt",
                    LegacyReferencesFilePath = @"C:\YOLO\Data.tsv",
                    ReportsDirectory = @"C:\YOLO\reports",
                    WeightsPaths = [@"C:\YOLO\models\model.pt", @"C:\YOLO\runs\train8\train8.pt"],
                    MaxConcurrentRequests = 3,
                    Seed = 5
                }
            };

            string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

            try
            {
                Directory.CreateDirectory(directory);

                JsonObject? jsonObject = yOLOTrainingRunOptions.ToJsonObject();
                Assert.NotNull(jsonObject);

                string path = Path.Combine(directory, yOLOTrainingRunOptions.RunName + Constants.FileName.YOLOTrainingRunOptionsSuffix);
                File.WriteAllText(path, jsonObject!.ToString());

                YOLOTrainingRunOptions? yOLOTrainingRunOptions_Read = YOLO.UI.Query.YOLOTrainingRunOptions(path);
                Assert.NotNull(yOLOTrainingRunOptions_Read);

                Assert.Equal(@"C:\YOLO\models\base\yolo26x.pt", yOLOTrainingRunOptions_Read!.StartWeightsPath);
                Assert.Equal("train_2026_09", yOLOTrainingRunOptions_Read.RunName);
                Assert.Equal(@"C:\YOLO\runs", yOLOTrainingRunOptions_Read.ProjectDirectory);
                Assert.Equal(300, yOLOTrainingRunOptions_Read.Epochs);
                Assert.Equal(40, yOLOTrainingRunOptions_Read.Patience);
                Assert.Equal(800, yOLOTrainingRunOptions_Read.ImageSize);
                Assert.Equal(8, yOLOTrainingRunOptions_Read.Batch);
                Assert.Equal(7, yOLOTrainingRunOptions_Read.Seed);
                Assert.Equal("0", yOLOTrainingRunOptions_Read.Device);
                Assert.Equal(@"C:\Python\python.exe", yOLOTrainingRunOptions_Read.PythonPath);
                Assert.Equal(@"C:\YOLO\working", yOLOTrainingRunOptions_Read.WorkingDirectory);
                Assert.True(yOLOTrainingRunOptions_Read.ResumeTraining);
                Assert.Equal<IEnumerable<YOLOTrainingStep>>([YOLOTrainingStep.LabelCheck, YOLOTrainingStep.Train, YOLOTrainingStep.Evaluate], yOLOTrainingRunOptions_Read.Steps!);

                YOLOTrainingDatasetOptions? yOLOTrainingDatasetOptions_Read = yOLOTrainingRunOptions_Read.DatasetOptions;
                Assert.NotNull(yOLOTrainingDatasetOptions_Read);
                Assert.Equal<IEnumerable<int>>([22138, 22139], yOLOTrainingDatasetOptions_Read!.CountyIds!);
                Assert.Equal(@"C:\YOLO\dataset", yOLOTrainingDatasetOptions_Read.OutputDirectory);
                Assert.False(yOLOTrainingDatasetOptions_Read.Resume);
                Assert.True(yOLOTrainingDatasetOptions_Read.CountOnly);
                Assert.Equal(@"C:\Python\python.exe", yOLOTrainingDatasetOptions_Read.PythonPath);
                Assert.Equal(@"C:\YOLO\working", yOLOTrainingDatasetOptions_Read.WorkingDirectory);
                Assert.Equal(@"C:\YOLO\models\model.pt", yOLOTrainingDatasetOptions_Read.ModelPath);
                Assert.Equal(@"C:\YOLO\Data.tsv", yOLOTrainingDatasetOptions_Read.LegacyReferencesFilePath);
                Assert.Equal(@"C:\YOLO\reports", yOLOTrainingDatasetOptions_Read.ReportsDirectory);
                Assert.Equal<IEnumerable<string>>([@"C:\YOLO\models\model.pt", @"C:\YOLO\runs\train8\train8.pt"], yOLOTrainingDatasetOptions_Read.WeightsPaths!);
                Assert.Equal(3, yOLOTrainingDatasetOptions_Read.MaxConcurrentRequests);
                Assert.Equal(5, yOLOTrainingDatasetOptions_Read.Seed);

                Core.xUnit.Query.SerializationCheck(yOLOTrainingRunOptions);
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
