using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.ML;
using DiGi_GIS_ML;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.ML.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the generated ModelInput and the feature allow-list describe exactly the same features.
        /// <para>Since ZiolkowskiJakub/DiGi.GIS.ML#15 the regressor no longer predicts in production - the first-detection heuristic does - but it stays in the repository as the baseline a feature redesign is measured from, scored by the evaluation app. Its contract with the allow-list still has to hold for that measurement to mean anything: a feature dropped from one side is read as its type default by the other, which produces plausible scores from a model shown a distribution it was never fitted on.</para>
        /// <para>The label is a member of ModelInput but is not a feature, so it is excluded from the comparison rather than expected in the allow-list - the allow-list carrying it would be the leak the pipeline exists to prevent.</para>
        /// </summary>
        [Fact]
        public void OrtoBuildingDetectionModel_FeatureContract()
        {
            // Resolved the way ML.NET itself resolves them, so the comparison is against the names the
            // trainer bound rather than against a re-derivation of the member-name mangling.
            Microsoft.ML.MLContext mLContext = new();
            Microsoft.ML.IDataView dataView = mLContext.Data.LoadFromEnumerable<OrtoBuildingDetectionModel.ModelInput>([]);

            HashSet<string> names_Model = [];
            foreach (Microsoft.ML.DataViewSchema.Column column_Model in dataView.Schema)
            {
                if (column_Model.Name != Constants.Column.YearBuilt.Name)
                {
                    names_Model.Add(column_Model.Name);
                }
            }

            HashSet<string> names_AllowList = [];
            foreach (Column column in IO.Query.YearBuiltPredictionInputColumns())
            {
                if (column.Name is string name)
                {
                    names_AllowList.Add(name);
                }
            }

            List<string> missing_FromModel = [.. names_AllowList.Except(names_Model).OrderBy(x => x)];
            List<string> missing_FromAllowList = [.. names_Model.Except(names_AllowList).OrderBy(x => x)];

            Assert.True(missing_FromModel.Count == 0, $"Allow-list columns absent from ModelInput: {string.Join(", ", missing_FromModel)}");
            Assert.True(missing_FromAllowList.Count == 0, $"ModelInput members absent from the allow-list: {string.Join(", ", missing_FromAllowList)}");
            Assert.Equal(172, names_Model.Count);

            // The trained contract must name exactly the features the generated model binds: a retrain that moves the
            // range but forgets the contract would still compile, and this is the check that catches it.
            HashSet<string> names_Contract = IO.Query.YearBuiltPredictionInputColumnNames(OrtoBuildingDetectionModel.TrainedYears, OrtoBuildingDetectionModel.TrainedRadiuses);
            Assert.Empty(names_Model.Except(names_Contract));
            Assert.Empty(names_Contract.Except(names_Model));

            // The pipeline's own output must not be readable as a feature from either side.
            foreach (Column column in IO.Query.YearBuiltPredictionOutputColumns())
            {
                Assert.DoesNotContain(column.Name ?? string.Empty, names_Model);
            }
        }

        /// <summary>
        /// Verifies that the predictor behind the runner's seam is always runnable, identifies itself as the first-detection heuristic, and states the detection years it reads as its contract.
        /// <para>The runner stamps the identity on every stored prediction (ZiolkowskiJakub/DiGi.GIS.YOLO.UI#26). It names the rule and its threshold rather than a model file's SHA-256, because since ZiolkowskiJakub/DiGi.GIS.ML#15 there is no model file in the scoring path - so readiness must not depend on one being present either.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictor_Readiness()
        {
            IO.Classes.YearBuiltPredictorReadiness yearBuiltPredictorReadiness = new Classes.YearBuiltPredictor().YearBuiltPredictorReadiness();

            Assert.True(yearBuiltPredictorReadiness.Runnable);
            Assert.Empty(yearBuiltPredictorReadiness.Messages);

            Assert.Equal(Constants.Heuristic.Id, yearBuiltPredictorReadiness.ModelId);
            Assert.Equal("first-confident-detection@0.5", yearBuiltPredictorReadiness.ModelId);

            // The detection years the heuristic reads: a run narrowing them would hide a building's first detection.
            Assert.NotNull(yearBuiltPredictorReadiness.Years);
            Assert.Equal(2008, yearBuiltPredictorReadiness.Years!.Min);
            Assert.Equal(2025, yearBuiltPredictorReadiness.Years.Max);
            Assert.Null(yearBuiltPredictorReadiness.Radiuses);
        }
    }
}
