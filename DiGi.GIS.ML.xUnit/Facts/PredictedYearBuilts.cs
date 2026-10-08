using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.IO;
using DiGi.GIS.ML;
using System;
using System.Collections.Generic;
using System.IO;

namespace DiGi.GIS.ML.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the deployed scoring path bounds every score by the imagery that first saw the building: a row confidently detected in 2008 is never predicted later than 2008, a row only detected in 2008 is capped by that detection, and a row whose confident detection is later keeps the bound without being dragged below it.
        /// <para>The regression of ZiolkowskiJakub/DiGi.GIS.ML#14 predicted 2013-2014 for buildings the 2008 orthophoto shows, because the regressor extrapolates on absolute coordinates. The cap is what makes such a score harmless: whatever the model returns, the row's first confident detection year is the ceiling. On unmodified code these assertions fail - the confident-2008 rows come back at the raw score - which is the reproduce for that issue.</para>
        /// <para>Medium test (measured 0.06 s, plus up to 1 s of model load): runs when DIGI_TEST_MAX_DURATION is Medium (the default) or Long.</para>
        /// </summary>
        [MediumFact]
        public void PredictedYearBuilts_CapsConfidentDetection()
        {
            Table table = new();
            table.AddColumn(IO.Constants.Column.Reference);
            table.AddColumn(IO.Constants.Column.FloorArea);
            table.AddColumn(IO.Constants.Column.Storeys);
            table.AddColumn(IO.Create.Column_PredictionYearBuit(IO.Constants.ColumnNamePrefix.PredictionConfidence, 2008));
            table.AddColumn(IO.Create.Column_PredictionYearBuit(IO.Constants.ColumnNamePrefix.PredictionConfidence, 2019));
            table.AddColumn(IO.Create.Column_PredictionYearBuit(IO.Constants.ColumnNamePrefix.PredictionConfidence, 2025));

            // Two confident-2008 rows with different features: whichever one the raw regressor scores later
            // than 2008, the cap has to pull back. The third row is detected but not confident, the fourth
            // is confidently seen only in 2019, the last is never detected by any year.
            table.AddRow(["REF-CONFIDENT-2008", 120.5F, (ushort)3, 0.9F, 0F, 0F]);
            table.AddRow(["REF-CONFIDENT-2008-ALT", 25.4F, (ushort)1, 0.9F, 0F, 0F]);
            table.AddRow(["REF-DETECTED-2008", 64.0F, (ushort)1, 0.3F, 0F, 0F]);
            table.AddRow(["REF-CONFIDENT-2019", 88.0F, (ushort)2, 0F, 0.9F, 0F]);
            table.AddRow(["REF-NO-DETECTION", 10.0F, (ushort)1, 0F, 0F, 0F]);

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

            // The guard passes: the capped share later than a first confident detection is 0, so the run
            // returns the table instead of refusing it.
            Assert.NotNull(table_Predictions);
            Assert.Equal(table.RowCount, table_Predictions!.RowCount);

            int index_Reference = table_Predictions.GetColumnIndex(IO.Constants.Column.Reference.Name);
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

            Assert.Equal(5, years.Count);

            // The acceptance criterion of ZiolkowskiJakub/DiGi.GIS.ML#14: nothing confidently seen in 2008
            // may be predicted later than 2009. The cap is stricter still - it binds to 2008 or earlier.
            Assert.True(years["REF-CONFIDENT-2008"] <= 2008, $"REF-CONFIDENT-2008 predicted {years["REF-CONFIDENT-2008"]}, later than its confident 2008 detection.");
            Assert.True(years["REF-CONFIDENT-2008-ALT"] <= 2008, $"REF-CONFIDENT-2008-ALT predicted {years["REF-CONFIDENT-2008-ALT"]}, later than its confident 2008 detection.");

            // Detected at 0.3 (below the confident threshold): the cap falls back to the first detection year.
            Assert.True(years["REF-DETECTED-2008"] <= 2008, $"REF-DETECTED-2008 predicted {years["REF-DETECTED-2008"]}, later than its first detection year.");

            // A confident detection in 2019 bounds its row without constraining a plausible score below it.
            Assert.True(years["REF-CONFIDENT-2019"] <= 2019, $"REF-CONFIDENT-2019 predicted {years["REF-CONFIDENT-2019"]}, later than its confident 2019 detection.");

            // Never detected: nothing to bound it by, so it keeps the raw score - the same range the
            // shipped sample fact pins for scored rows.
            Assert.InRange(years["REF-NO-DETECTION"], (ushort)1900, (ushort)2100);

            // The run reported the share loudly, before and after the cap - the reporting half of the
            // plausibility check the issue asks for.
            string output = stringWriter.ToString();
            Assert.Contains("year built plausibility", output);
            Assert.Contains("rows with a confident detection", output);
            Assert.Contains("before the plausibility cap", output);
            Assert.Contains("rows capped", output);

            // Same input, same answers: the cap is a bound, not a perturbation.
            Table? table_Repeat = table.PredictedYearBuilts();
            Assert.NotNull(table_Repeat);
            Assert.Equal(table.RowCount, table_Repeat!.RowCount);
            for (int i = 0; i < table_Repeat.RowCount; i++)
            {
                Assert.True(table_Repeat.TryGetValue(i, index_Reference, out string? reference));
                Assert.True(table_Repeat.TryGetValue(i, index_Year, out ushort year));
                Assert.Equal(years[reference ?? string.Empty], year);
            }
        }

        /// <summary>
        /// Verifies that a scored table carrying no confident detection at all still returns its predictions, with the plausibility share reported as not evaluable instead of guarded.
        /// <para>There is nothing to bound those rows by, and refusing them would fail open on the wrong axis - a diagnostic table with only a reference column must stay usable (this is the stripped-row differential the sample fact scores). The loud report is what keeps the case visible: the run says the share was not evaluable rather than implying it was acceptable.</para>
        /// <para>Medium test (measured 1 s, including the model load): runs when DIGI_TEST_MAX_DURATION is Medium (the default) or Long.</para>
        /// </summary>
        [MediumFact]
        public void PredictedYearBuilts_NotEvaluableWithoutConfidentDetection()
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
            Assert.Equal(3, table_Predictions!.RowCount);

            int index_Reference = table_Predictions.GetColumnIndex(IO.Constants.Column.Reference.Name);
            int index_Year = table_Predictions.GetColumnIndex(IO.Constants.Column.PredictedYearBuilt.Name);
            Assert.True(index_Reference >= 0);
            Assert.True(index_Year >= 0);

            List<ushort> years = [];
            for (int i = 0; i < table_Predictions.RowCount; i++)
            {
                Assert.False(string.IsNullOrWhiteSpace(table_Predictions.GetValue<string>(i, index_Reference)));
                Assert.True(table_Predictions.TryGetValue(i, index_Year, out ushort year));
                Assert.InRange(year, (ushort)1900, (ushort)2100);
                years.Add(year);
            }

            // Identical rows score identically.
            Assert.Single([.. new HashSet<ushort>(years)]);

            string output = stringWriter.ToString();
            Assert.Contains("not evaluable", output);
        }
    }
}
