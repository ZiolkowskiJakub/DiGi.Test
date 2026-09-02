using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.ML;
using System.Collections.Generic;

namespace DiGi.GIS.ML.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the assembled training table carries the fixed allow-list schema, in allow-list order, with the reference first and the label last.
        /// <para>The schema has to be fixed rather than inherited from whatever the read returned, because the detection columns are created per county only for the years that county's orthophoto series actually covers. Two counties therefore answer with different column sets, and a table built from whatever arrived would line different features up under the same position.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictionTrainingTable_Schema()
        {
            List<Column> columns_Input = GIS.IO.Query.YearBuiltPredictionInputColumns();

            // 31 base + 25 grid cell coverage + 90 detection + 18 population + 8 radial ratio. Stated rather than
            // derived so that widening the feature set - the 2026 year range in DiGi.GIS.IO#10 is the next one -
            // has to come here and to the regenerated ModelInput together, instead of one moving without the other.
            Assert.Equal(172, columns_Input.Count);

            // A deliberately narrow source: the reference and two features, standing in for a county whose
            // detection and population columns have not been written yet.
            Table table_Source = new();
            table_Source.AddColumn(GIS.IO.Constants.Column.Reference);
            table_Source.AddColumn(GIS.IO.Constants.Column.FloorArea);
            table_Source.AddColumn(GIS.IO.Constants.Column.Storeys);
            table_Source.AddRow(["REF-1", 120.5F, (ushort)2]);
            table_Source.AddRow(["REF-2", 64.0F, (ushort)1]);
            table_Source.AddRow(["REF-UNLABELLED", 10.0F, (ushort)1]);

            Dictionary<string, short> labels = new() { ["REF-1"] = 1975, ["REF-2"] = 2011 };

            Table? table = table_Source.YearBuiltPredictionTrainingTable(labels);
            Assert.NotNull(table);

            // Reference + every allow-list column + the label.
            Assert.Equal(columns_Input.Count + 2, table!.ColumnCount);

            List<Column> columns = [.. table.Columns];
            Assert.Equal(GIS.IO.Constants.Column.Reference.Name, columns[0].Name);
            Assert.Equal(Constants.Column.YearBuilt.Name, columns[columns.Count - 1].Name);

            for (int i = 0; i < columns_Input.Count; i++)
            {
                Assert.Equal(columns_Input[i].Name, columns[i + 1].Name);
            }

            // Only the labelled buildings become rows.
            Assert.Equal(2, table.RowCount);

            int index_Label = table.GetColumnIndex(Constants.Column.YearBuilt.Name);
            int index_Reference = table.GetColumnIndex(GIS.IO.Constants.Column.Reference.Name);
            Assert.True(index_Label > 0);
            Assert.True(index_Reference == 0);

            Dictionary<string, short> years_ByReference = [];
            for (int i = 0; i < table.RowCount; i++)
            {
                Assert.True(table.TryGetValue(i, index_Label, out short year));
                years_ByReference[table.GetValue<string>(i, index_Reference) ?? string.Empty] = year;
            }

            Assert.Equal((short)1975, years_ByReference["REF-1"]);
            Assert.Equal((short)2011, years_ByReference["REF-2"]);
            Assert.False(years_ByReference.ContainsKey("REF-UNLABELLED"));

            // The values the source did carry survive the projection.
            int index_FloorArea = table.GetColumnIndex(GIS.IO.Constants.Column.FloorArea.Name);
            Assert.True(index_FloorArea > 0);
            Assert.True(table.TryGetValue(0, index_FloorArea, out float floorArea));
            Assert.Equal(120.5F, floorArea, 3);
        }

        /// <summary>
        /// Verifies that a column the source did not carry is materialised at the same default the inference path reads for an absent feature, and that the resulting constant columns are reportable.
        /// <para>Query.PredictedYearBuilts reads an absent feature as 0F. Training on an absent feature written as anything else would show the model one distribution and the deployed pipeline another, so the two defaults are asserted against each other rather than left to agree by coincidence.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictionTrainingTable_MissingColumnsDefaulted()
        {
            Table table_Source = new();
            table_Source.AddColumn(GIS.IO.Constants.Column.Reference);
            table_Source.AddColumn(GIS.IO.Constants.Column.FloorArea);
            table_Source.AddRow(["REF-1", 120.5F]);
            table_Source.AddRow(["REF-2", 64.0F]);

            Dictionary<string, short> labels = new() { ["REF-1"] = 1975, ["REF-2"] = 2011 };

            Table? table = table_Source.YearBuiltPredictionTrainingTable(labels);
            Assert.NotNull(table);

            // A detection column no county has yet been written: present in the schema, zero in every row.
            Column column_Detection = GIS.IO.Create.Column_PredictionYearBuit(GIS.IO.Constants.ColumnNamePrefix.PredictionConfidence, 2019);
            int index_Detection = table!.GetColumnIndex(column_Detection.Name);
            Assert.True(index_Detection > 0);

            for (int i = 0; i < table.RowCount; i++)
            {
                Assert.True(table.TryGetValue(i, index_Detection, out double confidence));
                Assert.Equal(0D, confidence);
            }

            // A population column, likewise.
            int index_Population = table.GetColumnIndex(GIS.IO.Create.Column_Population(2019).Name);
            Assert.True(index_Population > 0);
            Assert.True(table.TryGetValue(0, index_Population, out int population));
            Assert.Equal(0, population);

            // The signature of an assembly run before the data-population runs: whole feature groups constant.
            List<string> names_Constant = table.DefaultOnlyColumnNames();
            Assert.Contains(column_Detection.Name, names_Constant);
            Assert.Contains(GIS.IO.Create.Column_Population(2019).Name, names_Constant);

            // A column that genuinely varies is not reported.
            Assert.DoesNotContain(GIS.IO.Constants.Column.FloorArea.Name, names_Constant);
        }

        /// <summary>
        /// Verifies that the training projection never carries a column the pipeline writes as its own output, so the regressor cannot be trained on its own answer.
        /// <para>The disjointness of the two column lists is already covered in DiGi.GIS.xUnit. This asserts the same property one step later, on the materialised table, which is what actually reaches the trainer.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictionTrainingTable_NoOutputColumnLeaks()
        {
            Table table_Source = new();
            table_Source.AddColumn(GIS.IO.Constants.Column.Reference);
            table_Source.AddColumn(GIS.IO.Constants.Column.PredictedYearBuilt);
            table_Source.AddRow(["REF-1", (ushort)1999]);

            Table? table = table_Source.YearBuiltPredictionTrainingTable(new Dictionary<string, short> { ["REF-1"] = 1975 });
            Assert.NotNull(table);

            // Even when the source hands it over, the pipeline's own output does not reach the training table.
            foreach (Column column in GIS.IO.Query.YearBuiltPredictionOutputColumns())
            {
                Assert.True(table!.GetColumnIndex(column.Name) < 0, $"Output column '{column.Name}' leaked into the training table.");
            }

            Assert.True(table!.GetColumnIndex(Constants.Column.YearBuilt.Name) > 0);
        }
    }
}
