using DiGi.Core.IO.DelimitedData.Enums;
using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.ML;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace DiGi.GIS.ML.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the deployed scoring path does not move a single prediction when a building's absolute location and administrative identity are replaced by another county's.
        /// <para>ZiolkowskiJakub/DiGi.GIS.ML#14 found the regressor keyed on location: re-scoring county 80328 with county-8956-like coordinates moved the buildings first confidently detected in 2008 and predicted after 2009 from 1.7 % to 100 %. On the 2026-10-08 model (SHA-256 2e120f49...) this probe moved 12 of 24 raw predictions of the committed sample. ZiolkowskiJakub/DiGi.GIS.ML#15 replaced the regressor by the first-detection heuristic; this keeps any future predictor from keying on where a building is again.</para>
        /// </summary>
        [Fact]
        public void PredictedYearBuilts_LocationInvariance()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "YearBuiltPrediction_Sample.tsv");
            Assert.False(string.IsNullOrWhiteSpace(path));

            Table? table = Core.IO.DelimitedData.Create.Table(path, DelimitedDataSeparator.Tab);
            Assert.NotNull(table);
            Assert.True(table!.RowCount > 0);

            // Values of a far-away county no labelled row comes from: a PUWG-1992 position at the other end of Poland,
            // the identifiers of county part 8956, and names that occur in no training row.
            Dictionary<string, object> values_ByName = new()
            {
                [IO.Constants.Column.InternalPointX.Name!] = 745000.0,
                [IO.Constants.Column.InternalPointY.Name!] = 290000.0,
                [IO.Constants.Column.BoundingBoxX.Name!] = 745000.0,
                [IO.Constants.Column.BoundingBoxY.Name!] = 290000.0,
                [IO.Constants.Column.CountyId.Name!] = 8956,
                [IO.Constants.Column.SubdivisionId.Name!] = 8956,
                [IO.Constants.Column.VoivodeshipName.Name!] = "Perturbed",
                [IO.Constants.Column.CountyName.Name!] = "Perturbed",
                [IO.Constants.Column.MunicipalityName.Name!] = "Perturbed",
                [IO.Constants.Column.SubdivisionName.Name!] = "Perturbed",
            };

            // The sample carries every one of them, so the probe really changes something.
            foreach (string name in values_ByName.Keys)
            {
                Assert.True(table.GetColumnIndex(name) >= 0, $"The sample lacks the column '{name}', so the probe would not change it.");
            }

            List<Column> columns = [.. table.Columns];
            Table table_Perturbed = new();
            foreach (Column column in columns)
            {
                table_Perturbed.AddColumn(column);
            }

            for (int i = 0; i < table.RowCount; i++)
            {
                List<object?> values = [];
                for (int j = 0; j < columns.Count; j++)
                {
                    object? value = table.GetValue(i, j);
                    if (columns[j].Name is string name && values_ByName.TryGetValue(name, out object? value_Perturbed))
                    {
                        Type type = columns[j].Type ?? typeof(string);
                        value = type == typeof(string) ? value_Perturbed.ToString() : System.Convert.ChangeType(value_Perturbed, Nullable.GetUnderlyingType(type) ?? type, CultureInfo.InvariantCulture);
                    }

                    values.Add(value);
                }

                table_Perturbed.AddRow(values);
            }

            Table? table_Predictions = table.PredictedYearBuilts();
            Table? table_PredictionsPerturbed = table_Perturbed.PredictedYearBuilts();
            Assert.NotNull(table_Predictions);
            Assert.NotNull(table_PredictionsPerturbed);
            Assert.True(table_Predictions!.RowCount > 0);
            Assert.Equal(table_Predictions.RowCount, table_PredictionsPerturbed!.RowCount);

            int index_Year = table_Predictions.GetColumnIndex(IO.Constants.Column.PredictedYearBuilt.Name);
            List<string> references_Moved = [];
            for (int i = 0; i < table_Predictions.RowCount; i++)
            {
                Assert.True(table_Predictions.TryGetValue(i, index_Year, out ushort year));
                Assert.True(table_PredictionsPerturbed.TryGetValue(i, index_Year, out ushort year_Perturbed));
                if (year != year_Perturbed)
                {
                    references_Moved.Add($"{table_Predictions.GetValue<string>(i, 0)}: {year} -> {year_Perturbed}");
                }
            }

            Assert.True(references_Moved.Count == 0, $"Replacing location and identity moved {references_Moved.Count} of {table_Predictions.RowCount} predictions: {string.Join("; ", references_Moved)}");
        }
    }
}
