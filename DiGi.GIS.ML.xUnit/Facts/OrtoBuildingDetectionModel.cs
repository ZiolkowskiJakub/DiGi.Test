using DiGi.Core.IO.DelimitedData.Enums;
using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.ML;
using DiGi_GIS_ML;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DiGi.GIS.ML.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the generated ModelInput and the feature allow-list describe exactly the same features.
        /// <para>These two are regenerated and maintained apart - ModelInput by the trainer, the allow-list by hand in DiGi.GIS.IO - and nothing at runtime notices when they disagree. A feature dropped from one side is read as its type default by the other, which produces plausible predictions from a model that is being shown a distribution it was never fitted on. Comparing the sets is the only cheap way to catch that.</para>
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
            foreach (Column column in GIS.IO.Query.YearBuiltPredictionInputColumns())
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

            // The pipeline's own output must not be readable as a feature from either side.
            foreach (Column column in GIS.IO.Query.YearBuiltPredictionOutputColumns())
            {
                Assert.DoesNotContain(column.Name ?? string.Empty, names_Model);
            }
        }

        /// <summary>
        /// Verifies that the deployed scoring path reads a real feature table and returns one plausible year per building, deterministically.
        /// <para>Runs against a committed sample of the assembled training table, so it exercises the binding by column slug and by display name against the shapes those columns actually have - which is what caught the model being handed content hashes instead of stored column identifiers.</para>
        /// </summary>
        [Fact]
        public void OrtoBuildingDetectionModel_Predict()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "YearBuiltPrediction_Sample.tsv");
            Assert.False(string.IsNullOrWhiteSpace(path));

            Table? table = Core.IO.DelimitedData.Create.Table(path, DelimitedDataSeparator.Tab);
            Assert.NotNull(table);
            Assert.True(table!.RowCount > 0);

            Table? table_Predictions = table.PredictedYearBuilts();
            Assert.NotNull(table_Predictions);
            Assert.Equal(table.RowCount, table_Predictions!.RowCount);

            int index_Reference = table_Predictions.GetColumnIndex(GIS.IO.Constants.Column.Reference.Name);
            int index_Year = table_Predictions.GetColumnIndex(GIS.IO.Constants.Column.PredictedYearBuilt.Name);
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

            // Same rows, same answers. A prediction engine that drifted between calls would make every
            // stored year depend on when the pipeline happened to run.
            Table? table_Repeat = table.PredictedYearBuilts();
            Assert.NotNull(table_Repeat);
            for (int i = 0; i < table_Repeat!.RowCount; i++)
            {
                Assert.True(table_Repeat.TryGetValue(i, index_Year, out ushort year));
                Assert.Equal(years[i], year);
            }

            // The features have to be reaching the model, and a range check does not establish that: a row
            // carrying nothing but a reference still scores 2012, comfortably inside any plausible range.
            // Scoring the same references stripped of every feature has to give a different answer, or the
            // binding is silently reading defaults - which is exactly how the deployed path came to score
            // an RSquared of -1.771 while failing at nothing.
            Table table_Stripped = new();
            table_Stripped.AddColumn(GIS.IO.Constants.Column.Reference);
            for (int i = 0; i < table.RowCount; i++)
            {
                table_Stripped.AddRow([table.GetValue<string>(i, table.GetColumnIndex(GIS.IO.Constants.Column.Reference.Name))]);
            }

            Table? table_StrippedPredictions = table_Stripped.PredictedYearBuilts();
            Assert.NotNull(table_StrippedPredictions);

            bool differs = false;
            for (int i = 0; i < table_StrippedPredictions!.RowCount; i++)
            {
                if (table_StrippedPredictions.TryGetValue(i, index_Year, out ushort year_Stripped) && year_Stripped != years[i])
                {
                    differs = true;
                    break;
                }
            }

            Assert.True(differs, "Stripping every feature changed no prediction - the scorer is not reading features.");
        }
    }
}
