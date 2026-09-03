using DiGi.Core.Classes;
using DiGi.GIS.YOLO.UI.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the options this application writes are the options the headless runner reads back.
        /// <para>The run happens in another process, so the whole of what the operator chose travels as one JSON file. Nothing links the writing to the reading at compile time - the runner parses whatever it finds, keeps the class default for any member the file does not name, and drops any key the class does not declare, all in silence.</para>
        /// <para><b>That silence is why this is worth a fact.</b> The defaults of every write step are <c>true</c>, so a handover that failed to carry them would not produce an empty run - it would produce a run that writes stored production data over counties nobody scoped, reporting success while doing it. The three write flags are asserted individually below for that reason.</para>
        /// <para>The two calls here are exactly the two the task makes: the serialized form on this side, <c>DiGi.GIS.YOLO.UI.Query.YearBuiltPredictionPipelineOptions</c> on the runner's.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictionPipelineOptions_Handover()
        {
            YearBuiltPredictionPipelineOptions yearBuiltPredictionPipelineOptions = new()
            {
                CountyIds = [22138, 22139],
                ScratchDirectory = @"C:\YOLO\scratch",
                PythonPath = @"C:\Python\python.exe",
                ModelPath = @"C:\YOLO\models\model.pt",
                WorkingDirectory = null,
                Confidence = 0.25,
                BatchSize = 2500,
                ReferenceBatchSize = 5000,
                MaxConcurrentRequests = 4,
                Resume = false,
                ExportImages = true,
                RunPrediction = true,
                Score = true,
                // Every one of these defaults to true, so carrying them across is the difference between a
                // harmless first pass and a run that writes deployed data.
                UpdateDetections = false,
                UpdateYearBuiltData = false,
                UpdatePredictedYearBuilt = false,
                Years = new Range<int>(2010, 2024),
                Radiuses = [250, 500]
            };

            string directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));

            try
            {
                Directory.CreateDirectory(directory);

                JsonObject? jsonObject = yearBuiltPredictionPipelineOptions.ToJsonObject();
                Assert.NotNull(jsonObject);

                string path = System.IO.Path.Combine(directory, DiGi.GIS.YOLO.UI.Constants.FileName.YearBuiltPredictionPipelineOptions);
                File.WriteAllText(path, jsonObject!.ToString());

                YearBuiltPredictionPipelineOptions? yearBuiltPredictionPipelineOptions_Read = DiGi.GIS.YOLO.UI.Query.YearBuiltPredictionPipelineOptions(path);
                Assert.NotNull(yearBuiltPredictionPipelineOptions_Read);

                Assert.NotNull(yearBuiltPredictionPipelineOptions_Read!.CountyIds);
                Assert.Equal(2, yearBuiltPredictionPipelineOptions_Read.CountyIds!.Count);
                Assert.Contains(22138, yearBuiltPredictionPipelineOptions_Read.CountyIds);
                Assert.Contains(22139, yearBuiltPredictionPipelineOptions_Read.CountyIds);

                Assert.Equal(@"C:\YOLO\scratch", yearBuiltPredictionPipelineOptions_Read.ScratchDirectory);
                Assert.Equal(@"C:\Python\python.exe", yearBuiltPredictionPipelineOptions_Read.PythonPath);
                Assert.Equal(@"C:\YOLO\models\model.pt", yearBuiltPredictionPipelineOptions_Read.ModelPath);
                Assert.Null(yearBuiltPredictionPipelineOptions_Read.WorkingDirectory);
                Assert.Equal(0.25, yearBuiltPredictionPipelineOptions_Read.Confidence);

                Assert.Equal(2500, yearBuiltPredictionPipelineOptions_Read.BatchSize);
                Assert.Equal(5000, yearBuiltPredictionPipelineOptions_Read.ReferenceBatchSize);
                Assert.Equal(4, yearBuiltPredictionPipelineOptions_Read.MaxConcurrentRequests);

                Assert.False(yearBuiltPredictionPipelineOptions_Read.Resume);
                Assert.True(yearBuiltPredictionPipelineOptions_Read.ExportImages);
                Assert.True(yearBuiltPredictionPipelineOptions_Read.RunPrediction);
                Assert.True(yearBuiltPredictionPipelineOptions_Read.Score);

                Assert.False(yearBuiltPredictionPipelineOptions_Read.UpdateDetections);
                Assert.False(yearBuiltPredictionPipelineOptions_Read.UpdateYearBuiltData);
                Assert.False(yearBuiltPredictionPipelineOptions_Read.UpdatePredictedYearBuilt);

                // The feature projection is built from these, and one that disagrees with what the regressor was
                // trained on hands the model defaults rather than features - which scores without failing.
                Assert.NotNull(yearBuiltPredictionPipelineOptions_Read.Years);
                Assert.Equal(2010, yearBuiltPredictionPipelineOptions_Read.Years!.Min);
                Assert.Equal(2024, yearBuiltPredictionPipelineOptions_Read.Years.Max);

                Assert.NotNull(yearBuiltPredictionPipelineOptions_Read.Radiuses);
                Assert.Equal<IEnumerable<double>>([250, 500], yearBuiltPredictionPipelineOptions_Read.Radiuses!);
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
