using DiGi.Core.IO.DelimitedData.Enums;
using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.IO;
using DiGi.GIS.ML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DiGi.GIS.ML.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the deployed scoring path dates every building by the first year the imagery confidently saw it, falls back to the first weaker detection, never dates a building after its first confident detection, and leaves out a building that was never detected.
        /// <para>This is the first-detection heuristic that replaced the regressor (ZiolkowskiJakub/DiGi.GIS.ML#15). The regressor dated buildings the 2008 orthophoto shows to 2013-2014 (ZiolkowskiJakub/DiGi.GIS.ML#14); here a confident 2008 detection is 2008 exactly. A weak earlier detection does not date a building that is confidently seen later, which is what the confidence threshold is for.</para>
        /// <para>Short test: no model is loaded.</para>
        /// </summary>
        [Fact]
        public void PredictedYearBuilts_FirstDetection()
        {
            Table table = new();
            table.AddColumn(IO.Constants.Column.Reference);
            table.AddColumn(IO.Constants.Column.FloorArea);
            table.AddColumn(IO.Create.Column_PredictionYearBuit(IO.Constants.ColumnNamePrefix.PredictionConfidence, 2008));
            table.AddColumn(IO.Create.Column_PredictionYearBuit(IO.Constants.ColumnNamePrefix.PredictionConfidence, 2019));
            table.AddColumn(IO.Create.Column_PredictionYearBuit(IO.Constants.ColumnNamePrefix.PredictionConfidence, 2025));

            table.AddRow(["REF-CONFIDENT-2008", 120.5F, 0.9F, 0.9F, 0.9F]);
            table.AddRow(["REF-DETECTED-2008", 64.0F, 0.3F, 0F, 0F]);
            table.AddRow(["REF-WEAK-2008-CONFIDENT-2019", 88.0F, 0.3F, 0.9F, 0.9F]);
            table.AddRow(["REF-THRESHOLD-2025", 12.0F, 0F, 0.49F, 0.5F]);
            table.AddRow(["REF-NO-DETECTION", 10.0F, 0F, 0F, 0F]);

            StringWriter stringWriter = new();
            TextWriter textWriter_Base = Console.Out;
            Table? table_Predictions;
            try
            {
                Console.SetOut(stringWriter);
                table_Predictions = table.PredictedYearBuilts();
            }
            finally
            {
                Console.SetOut(textWriter_Base);
            }

            Assert.NotNull(table_Predictions);

            int index_Reference = table_Predictions!.GetColumnIndex(IO.Constants.Column.Reference.Name);
            int index_Year = table_Predictions.GetColumnIndex(IO.Constants.Column.PredictedYearBuilt.Name);
            Assert.True(index_Reference >= 0);
            Assert.True(index_Year >= 0);

            Dictionary<string, ushort> years = [];
            for (int i = 0; i < table_Predictions.RowCount; i++)
            {
                Assert.True(table_Predictions.TryGetValue(i, index_Reference, out string? reference));
                Assert.True(table_Predictions.TryGetValue(i, index_Year, out ushort year));
                years[reference ?? string.Empty] = year;
            }

            // The building that was never detected is left out rather than dated by a default.
            Assert.Equal(4, years.Count);
            Assert.False(years.ContainsKey("REF-NO-DETECTION"));

            Assert.Equal((ushort)2008, years["REF-CONFIDENT-2008"]);

            // Detected only below the confident threshold: the first detection dates it.
            Assert.Equal((ushort)2008, years["REF-DETECTED-2008"]);

            // A weak 2008 detection does not date a building first confidently seen in 2019.
            Assert.Equal((ushort)2019, years["REF-WEAK-2008-CONFIDENT-2019"]);

            // 0.49 is just below the threshold and 0.5 is exactly on it.
            Assert.Equal((ushort)2025, years["REF-THRESHOLD-2025"]);

            string output = stringWriter.ToString();
            Assert.Contains("3 buildings dated by their first confident detection", output);
            Assert.Contains("1 by a weaker detection only", output);
            Assert.Contains("1 never detected and left out", output);

            // The predictor behind the runner's seam answers the same.
            Table? table_Predictor = new Classes.YearBuiltPredictor().Predict(table);
            Assert.NotNull(table_Predictor);
            Assert.Equal(table_Predictions.RowCount, table_Predictor!.RowCount);
            for (int i = 0; i < table_Predictor.RowCount; i++)
            {
                Assert.True(table_Predictor.TryGetValue(i, index_Reference, out string? reference));
                Assert.True(table_Predictor.TryGetValue(i, index_Year, out ushort year));
                Assert.Equal(years[reference ?? string.Empty], year);
            }
        }

        /// <summary>
        /// Verifies that a table carrying no detection at all returns an empty prediction table rather than null, and says why.
        /// <para>A table with only a reference column is a valid input with nothing to date - not a failure - so the runner records no prediction instead of failing the county. The report is what keeps the case visible.</para>
        /// </summary>
        [Fact]
        public void PredictedYearBuilts_NoDetection()
        {
            Table table = new();
            table.AddColumn(IO.Constants.Column.Reference);
            table.AddRow(["REF-1"]);
            table.AddRow(["REF-2"]);
            table.AddRow(["REF-3"]);

            StringWriter stringWriter = new();
            TextWriter textWriter_Base = Console.Out;
            Table? table_Predictions;
            try
            {
                Console.SetOut(stringWriter);
                table_Predictions = table.PredictedYearBuilts();
            }
            finally
            {
                Console.SetOut(textWriter_Base);
            }

            Assert.NotNull(table_Predictions);
            Assert.Equal(0, table_Predictions!.RowCount);
            Assert.True(table_Predictions.GetColumnIndex(IO.Constants.Column.PredictedYearBuilt.Name) >= 0);
            Assert.Contains("3 never detected and left out", stringWriter.ToString());
        }

        /// <summary>
        /// Verifies that the deployed scoring path dates every building of a real feature table by its first confident detection, falling back to its first detection, deterministically.
        /// <para>Runs against a committed sample of the assembled training table, so it exercises the column binding against the shapes those columns actually have. The expected years come from <see cref="Query.FirstConfidentDetectionYears"/> rather than a second copy of the rule.</para>
        /// </summary>
        [Fact]
        public void PredictedYearBuilts_Sample()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "YearBuiltPrediction_Sample.tsv");
            Assert.False(string.IsNullOrWhiteSpace(path));

            Table? table = Core.IO.DelimitedData.Create.Table(path, DelimitedDataSeparator.Tab);
            Assert.NotNull(table);
            Assert.True(table!.RowCount > 0);

            List<int?> years_Confident = table.FirstConfidentDetectionYears();
            List<int?> years_Detected = table.FirstConfidentDetectionYears(0F);
            int index_Reference_Source = table.GetColumnIndex(IO.Constants.Column.Reference.Name);

            Dictionary<string, int> years_Expected = [];
            for (int i = 0; i < table.RowCount; i++)
            {
                if ((years_Confident[i] ?? years_Detected[i]) is int year && table.GetValue<string>(i, index_Reference_Source) is string reference)
                {
                    years_Expected[reference] = year;
                }
            }

            // The sample is a real slice of the training table: its buildings were detected.
            Assert.NotEmpty(years_Expected);

            Table? table_Predictions = table.PredictedYearBuilts();
            Assert.NotNull(table_Predictions);
            Assert.Equal(years_Expected.Count, table_Predictions!.RowCount);

            int index_Reference = table_Predictions.GetColumnIndex(IO.Constants.Column.Reference.Name);
            int index_Year = table_Predictions.GetColumnIndex(IO.Constants.Column.PredictedYearBuilt.Name);
            for (int i = 0; i < table_Predictions.RowCount; i++)
            {
                Assert.True(table_Predictions.TryGetValue(i, index_Reference, out string? reference));
                Assert.True(table_Predictions.TryGetValue(i, index_Year, out ushort year));
                Assert.Equal(years_Expected[reference ?? string.Empty], year);
            }

            // Same rows, same answers.
            Table? table_Repeat = table.PredictedYearBuilts();
            Assert.NotNull(table_Repeat);
            Assert.Equal(table_Predictions.RowCount, table_Repeat!.RowCount);
            for (int i = 0; i < table_Repeat.RowCount; i++)
            {
                Assert.True(table_Repeat.TryGetValue(i, index_Year, out ushort year_Repeat));
                Assert.True(table_Predictions.TryGetValue(i, index_Year, out ushort year));
                Assert.Equal(year, year_Repeat);
            }
        }
    }
}
