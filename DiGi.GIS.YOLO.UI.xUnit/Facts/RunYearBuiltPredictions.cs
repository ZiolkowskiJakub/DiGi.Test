using DiGi.GIS.IO.Interfaces;
using DiGi.GIS.YOLO.UI.Classes;
using DiGi.GIS.WebAPI.Classes;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that Modify.RunYearBuiltPredictionsAsync refuses a run it cannot scope, rather than starting one and discovering it halfway through.
        /// <para>The county scope has no run-everything default on purpose: the pipeline writes deployed data.</para>
        /// </summary>
        [Fact]
        public async Task RunYearBuiltPredictions_Validation()
        {
            GISWebAPIManager gisWebAPIManager = new(null);

            Classes.YearBuiltPredictionPipelineOptions yearBuiltPredictionPipelineOptions = new()
            {
                CountyIds = [73485],
                ScratchDirectory = Path.GetTempPath(),
                ExportImages = false,
                RunPrediction = false,
                Score = false,
                UpdateDetections = false,
                UpdatePredictedYearBuilt = false,
                UpdateYearBuiltData = false
            };

            Classes.YearBuiltPredictionResult? result_NullManager = await Modify.RunYearBuiltPredictionsAsync(null, null, yearBuiltPredictionPipelineOptions);
            Assert.Null(result_NullManager);

            Classes.YearBuiltPredictionResult? result_NullOptions = await gisWebAPIManager.RunYearBuiltPredictionsAsync(null, null);
            Assert.Null(result_NullOptions);

            Classes.YearBuiltPredictionPipelineOptions yearBuiltPredictionPipelineOptions_NoCounty = new(yearBuiltPredictionPipelineOptions) { CountyIds = [] };
            Classes.YearBuiltPredictionResult? result_NoCounty = await gisWebAPIManager.RunYearBuiltPredictionsAsync(null, yearBuiltPredictionPipelineOptions_NoCounty);
            Assert.Null(result_NoCounty);

            //A county identifier is a database row identifier, so a non-positive one names nothing
            Classes.YearBuiltPredictionPipelineOptions yearBuiltPredictionPipelineOptions_BadCounty = new(yearBuiltPredictionPipelineOptions) { CountyIds = [0, -1] };
            Classes.YearBuiltPredictionResult? result_BadCounty = await gisWebAPIManager.RunYearBuiltPredictionsAsync(null, yearBuiltPredictionPipelineOptions_BadCounty);
            Assert.Null(result_BadCounty);

            Classes.YearBuiltPredictionPipelineOptions yearBuiltPredictionPipelineOptions_NoScratch = new(yearBuiltPredictionPipelineOptions) { ScratchDirectory = null };
            Classes.YearBuiltPredictionResult? result_NoScratch = await gisWebAPIManager.RunYearBuiltPredictionsAsync(null, yearBuiltPredictionPipelineOptions_NoScratch);
            Assert.Null(result_NoScratch);
        }

        /// <summary>
        /// Verifies that the orchestrator turns the detections a previous run left on disk into objects and counts them, without contacting Python, ML.NET or a database.
        /// <para>Every step but the translation is turned off, so what this pins is the sequence itself: the detections are read from the deterministic scratch path the run derives from the county identifier, which is what makes a resumed run find what it left behind.</para>
        /// </summary>
        [Fact]
        public async Task RunYearBuiltPredictions_Detections()
        {
            int countyId = 73485;

            string? path_Fixture = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "YOLO_Prediction.bbrf");
            Assert.False(string.IsNullOrWhiteSpace(path_Fixture));
            Assert.True(File.Exists(path_Fixture));

            string? directory_Reports = Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly());
            Assert.False(string.IsNullOrWhiteSpace(directory_Reports));

            string directory_Scratch = Path.Combine(directory_Reports!, nameof(RunYearBuiltPredictions_Detections));
            string directory_County = Path.Combine(directory_Scratch, countyId.ToString());
            Directory.CreateDirectory(Path.Combine(directory_County, Constants.DirectoryName.PredictionImages));
            File.Copy(path_Fixture!, Path.Combine(directory_County, Constants.FileName.PredictionResults), true);

            GISWebAPIManager gisWebAPIManager = new(null);

            Classes.YearBuiltPredictionPipelineOptions yearBuiltPredictionPipelineOptions = new()
            {
                CountyIds = [countyId],
                ScratchDirectory = directory_Scratch,
                ExportImages = false,
                RunPrediction = false,
                Score = false,
                UpdateDetections = false,
                UpdatePredictedYearBuilt = false,
                UpdateYearBuiltData = false
            };

            Classes.YearBuiltPredictionResult? yearBuiltPredictionResult = await gisWebAPIManager.RunYearBuiltPredictionsAsync(null, yearBuiltPredictionPipelineOptions);

            Assert.NotNull(yearBuiltPredictionResult);
            Assert.Equal([countyId], yearBuiltPredictionResult!.CountyIds);
            Assert.False(yearBuiltPredictionResult.Cancelled);
            Assert.NotNull(yearBuiltPredictionResult.RunTimestamp);

            //The fixture holds three images, two of which carry a detection and one of which carries none
            Assert.Equal(2, yearBuiltPredictionResult.BuildingCount);
            Assert.Equal(2, yearBuiltPredictionResult.DetectionCount);

            //Nothing was written, so nothing is reported written
            Assert.Equal(0, yearBuiltPredictionResult.BuildingDataUpdatedCount);
            Assert.Equal(0, yearBuiltPredictionResult.YearBuiltDataUpdatedCount);
            Assert.Equal(0, yearBuiltPredictionResult.PredictionCount);

            Core.xUnit.Query.SerializationCheck(yearBuiltPredictionResult);
        }

        /// <summary>
        /// Verifies that the scoring step is refused when no predictor was supplied, and that the refusal is reported rather than thrown.
        /// <para>The seam is optional by design - a detection-only pass needs no regressor - so asking for the scoring step without one is a stated failure of that step and not of the run.</para>
        /// </summary>
        [Fact]
        public async Task RunYearBuiltPredictions_MissingPredictor()
        {
            int countyId = 73485;

            string? path_Fixture = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "YOLO_Prediction.bbrf");
            string? directory_Reports = Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly());

            string directory_Scratch = Path.Combine(directory_Reports!, nameof(RunYearBuiltPredictions_MissingPredictor));
            string directory_County = Path.Combine(directory_Scratch, countyId.ToString());
            Directory.CreateDirectory(Path.Combine(directory_County, Constants.DirectoryName.PredictionImages));
            File.Copy(path_Fixture!, Path.Combine(directory_County, Constants.FileName.PredictionResults), true);

            GISWebAPIManager gisWebAPIManager = new(null);

            Classes.YearBuiltPredictionPipelineOptions yearBuiltPredictionPipelineOptions = new()
            {
                CountyIds = [countyId],
                ScratchDirectory = directory_Scratch,
                ExportImages = false,
                RunPrediction = false,
                Score = true,
                UpdateDetections = false,
                UpdatePredictedYearBuilt = false,
                UpdateYearBuiltData = false
            };

            Classes.YearBuiltPredictionResult? yearBuiltPredictionResult = await gisWebAPIManager.RunYearBuiltPredictionsAsync(null, yearBuiltPredictionPipelineOptions);

            Assert.NotNull(yearBuiltPredictionResult);
            Assert.Equal(2, yearBuiltPredictionResult!.BuildingCount);
            Assert.Contains(nameof(IYearBuiltPredictor), yearBuiltPredictionResult.FailedStepNames);
            Assert.Equal(0, yearBuiltPredictionResult.PredictionCount);
        }

        /// <summary>
        /// Verifies that a predictor is accepted and the run then reaches the feature read, which is the first step that cannot be satisfied without a server.
        /// <para>What this pins is that the predictor guard passes and the failure moves on to the read, so a stub predictor is enough to exercise the sequence up to the point a database is genuinely needed.</para>
        /// </summary>
        [Fact]
        public async Task RunYearBuiltPredictions_StubPredictor()
        {
            int countyId = 73485;

            string? path_Fixture = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "YOLO_Prediction.bbrf");
            string? directory_Reports = Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly());

            string directory_Scratch = Path.Combine(directory_Reports!, nameof(RunYearBuiltPredictions_StubPredictor));
            string directory_County = Path.Combine(directory_Scratch, countyId.ToString());
            Directory.CreateDirectory(Path.Combine(directory_County, Constants.DirectoryName.PredictionImages));
            File.Copy(path_Fixture!, Path.Combine(directory_County, Constants.FileName.PredictionResults), true);

            GISWebAPIManager gisWebAPIManager = new(null);
            YearBuiltPredictorStub yearBuiltPredictorStub = new(1965);

            Classes.YearBuiltPredictionPipelineOptions yearBuiltPredictionPipelineOptions = new()
            {
                CountyIds = [countyId],
                ScratchDirectory = directory_Scratch,
                ExportImages = false,
                RunPrediction = false,
                Score = true,
                UpdateDetections = false,
                UpdatePredictedYearBuilt = false,
                UpdateYearBuiltData = false
            };

            Classes.YearBuiltPredictionResult? yearBuiltPredictionResult = await gisWebAPIManager.RunYearBuiltPredictionsAsync(yearBuiltPredictorStub, yearBuiltPredictionPipelineOptions);

            Assert.NotNull(yearBuiltPredictionResult);
            Assert.Equal(2, yearBuiltPredictionResult!.BuildingCount);

            //The predictor was accepted, so the run got past it and stopped at the read instead
            Assert.DoesNotContain(nameof(IYearBuiltPredictor), yearBuiltPredictionResult.FailedStepNames);
            Assert.Contains(nameof(Query.BuildingDataTableAsync), yearBuiltPredictionResult.FailedStepNames);
            Assert.Equal(0, yearBuiltPredictorStub.CallCount);
            Assert.Equal(0, yearBuiltPredictionResult.FeatureRowCount);
        }

        /// <summary>
        /// Verifies that a predictor reporting itself unable to score is refused before any county work starts, and that the refusal carries the reason.
        /// <para>The readiness preflight sits beside the Python one: a runner without its model otherwise exports a county of imagery, fails on the first scoring batch, and the cached engine failure repeats for every county behind it. What this pins is that the refusal comes first, with the diagnostic, and that the predictor is never asked to score.</para>
        /// </summary>
        [Fact]
        public async Task RunYearBuiltPredictions_UnrunnablePredictor()
        {
            int countyId = 73485;

            string? path_Fixture = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "YOLO_Prediction.bbrf");
            string? directory_Reports = Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly());

            string directory_Scratch = Path.Combine(directory_Reports!, nameof(RunYearBuiltPredictions_UnrunnablePredictor));
            string directory_County = Path.Combine(directory_Scratch, countyId.ToString());
            Directory.CreateDirectory(Path.Combine(directory_County, Constants.DirectoryName.PredictionImages));
            File.Copy(path_Fixture!, Path.Combine(directory_County, Constants.FileName.PredictionResults), true);

            GISWebAPIManager gisWebAPIManager = new(null);
            YearBuiltPredictorStub yearBuiltPredictorStub = new(1965, runnable: false);

            Classes.YearBuiltPredictionPipelineOptions yearBuiltPredictionPipelineOptions = new()
            {
                CountyIds = [countyId],
                ScratchDirectory = directory_Scratch,
                ExportImages = false,
                RunPrediction = false,
                Score = true,
                UpdateDetections = false,
                UpdatePredictedYearBuilt = false,
                UpdateYearBuiltData = false
            };

            Classes.YearBuiltPredictionResult? yearBuiltPredictionResult = await gisWebAPIManager.RunYearBuiltPredictionsAsync(yearBuiltPredictorStub, yearBuiltPredictionPipelineOptions);

            Assert.NotNull(yearBuiltPredictionResult);

            //The refusal is a preflight: nothing of the county was carried through, and the predictor was never asked to score
            Assert.Equal(0, yearBuiltPredictorStub.CallCount);
            Assert.Equal(0, yearBuiltPredictionResult!.BuildingCount);
            Assert.Equal(0, yearBuiltPredictionResult.FeatureRowCount);

            //The refusal is named after the readiness surface, and the reason it refused travels with the result
            Assert.Contains(nameof(DiGi.GIS.IO.Classes.YearBuiltPredictorReadiness), yearBuiltPredictionResult.FailedStepNames);
            Assert.Contains("model", string.Join(" ", yearBuiltPredictionResult.Messages));
        }
    }
}
