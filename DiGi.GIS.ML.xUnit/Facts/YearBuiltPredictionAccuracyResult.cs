using DiGi.GIS.ML;
using DiGi.GIS.ML.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.ML.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests YearBuiltPredictionAccuracyResult construction, string conversion and serialization round-trip.
        /// </summary>
        [Fact]
        public void YearBuiltPredictionAccuracyResult()
        {
            YearBuiltPredictionAccuracyResult yearBuiltPredictionAccuracyResult = new("first detection year", "random 20% holdout", 4048, 0.449, 1.750, 0.7830);

            Assert.Equal("first detection year", yearBuiltPredictionAccuracyResult.Name);
            Assert.Equal("random 20% holdout", yearBuiltPredictionAccuracyResult.SplitName);
            Assert.Equal(4048, yearBuiltPredictionAccuracyResult.Count);
            Assert.Equal(0.449, yearBuiltPredictionAccuracyResult.MeanAbsoluteError, 3);
            Assert.Equal(1.750, yearBuiltPredictionAccuracyResult.RootMeanSquaredError, 3);
            Assert.Equal(0.7830, yearBuiltPredictionAccuracyResult.RSquared, 4);

            Core.xUnit.Query.SerializationCheck(yearBuiltPredictionAccuracyResult);
        }

        /// <summary>
        /// Verifies that the accuracy measures are computed correctly, that a pair is skipped when either side is missing, and that R squared is not invented when the holdout has no variance.
        /// <para>The perfect-predictor case pins the sign convention: a model reproducing the label exactly scores zero error and an R squared of one.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictionAccuracyResult_Measures()
        {
            List<double?> years = [2000, 2010, 2020, 2030];

            // Errors of -2, +2, -2, +2: mean absolute 2, root mean squared 2.
            List<double?> years_Predicted = [2002, 2008, 2022, 2028];
            YearBuiltPredictionAccuracyResult? result = Create.YearBuiltPredictionAccuracyResult("test", "split", years, years_Predicted);
            Assert.NotNull(result);
            Assert.Equal(4, result!.Count);
            Assert.Equal(2D, result.MeanAbsoluteError, 6);
            Assert.Equal(2D, result.RootMeanSquaredError, 6);

            // A predictor reproducing the label exactly.
            YearBuiltPredictionAccuracyResult? result_Perfect = Create.YearBuiltPredictionAccuracyResult("perfect", "split", years, [2000, 2010, 2020, 2030]);
            Assert.NotNull(result_Perfect);
            Assert.Equal(0D, result_Perfect!.MeanAbsoluteError, 6);
            Assert.Equal(1D, result_Perfect.RSquared, 6);

            // A building the predictor declined to answer for is not charged a default - it is left out.
            YearBuiltPredictionAccuracyResult? result_Partial = Create.YearBuiltPredictionAccuracyResult("partial", "split", years, [2000, null, 2020, null]);
            Assert.NotNull(result_Partial);
            Assert.Equal(2, result_Partial!.Count);
            Assert.Equal(0D, result_Partial.MeanAbsoluteError, 6);

            // No variance in the holdout means no R squared to report, rather than a number that reads as a score.
            YearBuiltPredictionAccuracyResult? result_Constant = Create.YearBuiltPredictionAccuracyResult("constant", "split", [2008, 2008, 2008], [2008, 2009, 2007]);
            Assert.NotNull(result_Constant);
            Assert.True(double.IsNaN(result_Constant!.RSquared));

            Assert.Null(Create.YearBuiltPredictionAccuracyResult("empty", "split", [], []));
            Assert.Null(Create.YearBuiltPredictionAccuracyResult("null", "split", null, null));
        }
    }
}
