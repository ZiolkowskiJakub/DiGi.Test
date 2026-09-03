using DiGi.Core.Classes;
using DiGi.GIS.YOLO.UI.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that YearBuiltPredictionPipelineOptions keeps the values it is given, survives the round trip through its string form, and clones identically.
        /// </summary>
        [Fact]
        public void YearBuiltPredictionPipelineOptions()
        {
            Classes.YearBuiltPredictionPipelineOptions yearBuiltPredictionPipelineOptions = new()
            {
                BatchSize = 2500,
                Confidence = 0.35,
                CountyIds = [73485, 73482],
                ExportImages = false,
                MaxConcurrentRequests = 4,
                ModelPath = @"C:\YOLO\models\model.pt",
                PythonPath = @"C:\Python\python.exe",
                ReferenceBatchSize = 5000,
                Resume = false,
                RunPrediction = false,
                ScratchDirectory = @"C:\YOLO\scratch",
                Score = false,
                UpdateDetections = false,
                UpdatePredictedYearBuilt = false,
                UpdateYearBuiltData = false,
                WorkingDirectory = null,
                Years = new Range<int>(2010, 2024),
                Radiuses = [250, 500]
            };

            Assert.Equal(2500, yearBuiltPredictionPipelineOptions.BatchSize);
            Assert.Equal(0.35, yearBuiltPredictionPipelineOptions.Confidence);
            Assert.NotNull(yearBuiltPredictionPipelineOptions.CountyIds);
            Assert.Equal(2, yearBuiltPredictionPipelineOptions.CountyIds!.Count);
            Assert.False(yearBuiltPredictionPipelineOptions.ExportImages);
            Assert.Null(yearBuiltPredictionPipelineOptions.WorkingDirectory);
            Assert.NotNull(yearBuiltPredictionPipelineOptions.Years);
            Assert.Equal(2010, yearBuiltPredictionPipelineOptions.Years!.Min);
            Assert.Equal(2024, yearBuiltPredictionPipelineOptions.Years.Max);

            string? json = Core.Convert.ToSystem_String(yearBuiltPredictionPipelineOptions);
            Assert.False(string.IsNullOrWhiteSpace(json));

            Classes.YearBuiltPredictionPipelineOptions? yearBuiltPredictionPipelineOptions_Actual = Core.Convert.ToDiGi<Classes.YearBuiltPredictionPipelineOptions>(json)?.FirstOrDefault();
            Assert.NotNull(yearBuiltPredictionPipelineOptions_Actual);
            Assert.Equal(yearBuiltPredictionPipelineOptions.BatchSize, yearBuiltPredictionPipelineOptions_Actual!.BatchSize);
            Assert.Equal(yearBuiltPredictionPipelineOptions.Confidence, yearBuiltPredictionPipelineOptions_Actual.Confidence);
            Assert.Equal(yearBuiltPredictionPipelineOptions.ScratchDirectory, yearBuiltPredictionPipelineOptions_Actual.ScratchDirectory);
            Assert.Null(yearBuiltPredictionPipelineOptions_Actual.WorkingDirectory);
            Assert.NotNull(yearBuiltPredictionPipelineOptions_Actual.CountyIds);
            Assert.Contains(73485, yearBuiltPredictionPipelineOptions_Actual.CountyIds!);
            Assert.Contains(73482, yearBuiltPredictionPipelineOptions_Actual.CountyIds!);
            Assert.NotNull(yearBuiltPredictionPipelineOptions_Actual.Years);
            Assert.Equal(2010, yearBuiltPredictionPipelineOptions_Actual.Years!.Min);
            Assert.Equal(2024, yearBuiltPredictionPipelineOptions_Actual.Years.Max);

            Assert.NotNull(yearBuiltPredictionPipelineOptions_Actual.Radiuses);
            Assert.Equal<IEnumerable<double>>([250, 500], yearBuiltPredictionPipelineOptions_Actual.Radiuses!);

            //The county set, the year range and the radiuses are the three members a shallow copy would share
            //with the source - and a copy constructor that forgot one of them outright is what the options
            //window works on, so the run would be scoped from a projection the operator never chose.
            Classes.YearBuiltPredictionPipelineOptions yearBuiltPredictionPipelineOptions_Clone = new(yearBuiltPredictionPipelineOptions);
            Assert.NotNull(yearBuiltPredictionPipelineOptions_Clone.CountyIds);
            Assert.NotSame(yearBuiltPredictionPipelineOptions.CountyIds, yearBuiltPredictionPipelineOptions_Clone.CountyIds);
            Assert.NotNull(yearBuiltPredictionPipelineOptions_Clone.Years);
            Assert.NotSame(yearBuiltPredictionPipelineOptions.Years, yearBuiltPredictionPipelineOptions_Clone.Years);
            Assert.NotNull(yearBuiltPredictionPipelineOptions_Clone.Radiuses);
            Assert.NotSame(yearBuiltPredictionPipelineOptions.Radiuses, yearBuiltPredictionPipelineOptions_Clone.Radiuses);
            Assert.Equal<IEnumerable<double>>([250, 500], yearBuiltPredictionPipelineOptions_Clone.Radiuses!);

            Core.xUnit.Query.SerializationCheck(yearBuiltPredictionPipelineOptions);
        }

        /// <summary>
        /// Verifies the defaults of YearBuiltPredictionPipelineOptions.
        /// <para>The confidence matches the prediction script's own default, the reference page matches the cap the building data endpoint enforces, and no county is named - the pipeline writes deployed data, so a run that states no scope does nothing rather than everything.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictionPipelineOptions_Defaults()
        {
            Classes.YearBuiltPredictionPipelineOptions yearBuiltPredictionPipelineOptions = new();

            Assert.Equal(0.1, yearBuiltPredictionPipelineOptions.Confidence);
            Assert.Equal(5000, yearBuiltPredictionPipelineOptions.BatchSize);
            Assert.Equal(Constants.Count.BuildingDataReference_Maximum, yearBuiltPredictionPipelineOptions.ReferenceBatchSize);
            Assert.Equal(8, yearBuiltPredictionPipelineOptions.MaxConcurrentRequests);
            Assert.Null(yearBuiltPredictionPipelineOptions.CountyIds);
            Assert.Null(yearBuiltPredictionPipelineOptions.ScratchDirectory);
            Assert.Null(yearBuiltPredictionPipelineOptions.Years);

            Assert.True(yearBuiltPredictionPipelineOptions.ExportImages);
            Assert.True(yearBuiltPredictionPipelineOptions.Resume);
            Assert.True(yearBuiltPredictionPipelineOptions.RunPrediction);
            Assert.True(yearBuiltPredictionPipelineOptions.Score);
            Assert.True(yearBuiltPredictionPipelineOptions.UpdateDetections);
            Assert.True(yearBuiltPredictionPipelineOptions.UpdatePredictedYearBuilt);
            Assert.True(yearBuiltPredictionPipelineOptions.UpdateYearBuiltData);

            //The default year range has to be the one the column allow-list applies, or the projection asks for columns the regressor was not trained on
            List<Core.IO.Table.Classes.Column> columns_Default = GIS.IO.Query.YearBuiltPredictionInputColumns(yearBuiltPredictionPipelineOptions.Years);
            List<Core.IO.Table.Classes.Column> columns_Stated = GIS.IO.Query.YearBuiltPredictionInputColumns(new Range<int>(2008, 2025));
            Assert.Equal(columns_Stated.Count, columns_Default.Count);
        }
    }
}
