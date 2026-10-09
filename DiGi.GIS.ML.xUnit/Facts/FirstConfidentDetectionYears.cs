using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.ML;
using System.Collections.Generic;

namespace DiGi.GIS.ML.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the first confident detection year is the first year at or above the confidence threshold, that a detection below it does not count, that a row with no confident detection is null rather than a default year, and that a threshold of 0 finds the first detection at all.
        /// <para>This is the year the first-detection heuristic predicts and the bound the evaluation app judges a prediction against (ZiolkowskiJakub/DiGi.GIS.ML#14, #15). A default year would count a never-confident row as evidence, so the null is asserted explicitly.</para>
        /// </summary>
        [Fact]
        public void FirstConfidentDetectionYears()
        {
            Table table = new();
            table.AddColumn(IO.Constants.Column.Reference);
            table.AddColumn(IO.Create.Column_PredictionYearBuit(IO.Constants.ColumnNamePrefix.PredictionConfidence, 2008));
            table.AddColumn(IO.Create.Column_PredictionYearBuit(IO.Constants.ColumnNamePrefix.PredictionConfidence, 2012));
            table.AddColumn(IO.Create.Column_PredictionYearBuit(IO.Constants.ColumnNamePrefix.PredictionConfidence, 2019));

            table.AddRow(["REF-2008", 0.9F, 0.9F, 0.9F]);
            table.AddRow(["REF-THRESHOLD-2012", 0.49F, 0.5F, 0.9F]);
            table.AddRow(["REF-2019", 0F, 0.3F, 0.7F]);
            table.AddRow(["REF-NEVER-CONFIDENT", 0.2F, 0.4F, 0.1F]);

            List<int?> years = table.FirstConfidentDetectionYears();

            Assert.Equal(4, years.Count);
            Assert.Equal(2008, years[0]);

            // 0.49 is just below the threshold and 0.5 is exactly on it.
            Assert.Equal(2012, years[1]);
            Assert.Equal(2019, years[2]);
            Assert.Null(years[3]);

            // A threshold of 0 finds the first detection at all - the fallback of the heuristic.
            List<int?> years_Detected = table.FirstConfidentDetectionYears(0F);
            Assert.Equal(2008, years_Detected[0]);
            Assert.Equal(2008, years_Detected[1]);
            Assert.Equal(2012, years_Detected[2]);
            Assert.Equal(2008, years_Detected[3]);

            Assert.Empty(((Table?)null).FirstConfidentDetectionYears());
            Assert.Empty(new Table().FirstConfidentDetectionYears());
        }
    }
}
