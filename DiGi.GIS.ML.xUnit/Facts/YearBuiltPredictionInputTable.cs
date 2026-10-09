using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.ML;
using System.Collections.Generic;

namespace DiGi.GIS.ML.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the input table carries every building - labelled or not - under the fixed allow-list schema without a label, and that the training table is exactly its labelled rows with the label appended.
        /// <para>A county with no labels is scored and judged from this table (ZiolkowskiJakub/DiGi.GIS.ML#15), so it must show the model exactly what a training row of the same building carries. Comparing the two cell by cell is what pins that, rather than trusting two projections to stay alike.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictionInputTable()
        {
            Table table_Source = new();
            table_Source.AddColumn(IO.Constants.Column.Reference);
            table_Source.AddColumn(IO.Constants.Column.FloorArea);
            table_Source.AddColumn(IO.Create.Column_PredictionYearBuit(IO.Constants.ColumnNamePrefix.PredictionConfidence, 2008));
            table_Source.AddRow(["REF-1", 120.5F, 0.9F]);
            table_Source.AddRow(["REF-UNLABELLED", 64.0F, 0.2F]);
            table_Source.AddRow(["REF-1", 999.0F, 0.1F]);

            Table? table_Input = new List<Table?>() { table_Source, null }.YearBuiltPredictionInputTable();
            Assert.NotNull(table_Input);

            // Reference + every allow-list column, and no label.
            List<Column> columns_Input = IO.Query.YearBuiltPredictionInputColumns();
            Assert.Equal(columns_Input.Count + 1, table_Input!.ColumnCount);
            Assert.True(table_Input.GetColumnIndex(Constants.Column.YearBuilt.Name) < 0);
            Assert.Equal(IO.Constants.Column.Reference.Name, table_Input.GetColumn(0)?.Name);
            for (int i = 0; i < columns_Input.Count; i++)
            {
                Assert.Equal(columns_Input[i].Name, table_Input.GetColumn(i + 1)?.Name);
            }

            // Every building, the unlabelled one included; a reference read twice is one row, the first read.
            Assert.Equal(2, table_Input.RowCount);
            int index_FloorArea = table_Input.GetColumnIndex(IO.Constants.Column.FloorArea.Name);
            Assert.Equal("REF-1", table_Input.GetValue<string>(0, 0));
            Assert.True(table_Input.TryGetValue(0, index_FloorArea, out float floorArea));
            Assert.Equal(120.5F, floorArea, 3);
            Assert.Equal("REF-UNLABELLED", table_Input.GetValue<string>(1, 0));

            // The training table is the labelled input rows, cell for cell, with the label appended.
            Table? table_Training = table_Source.YearBuiltPredictionTrainingTable(new Dictionary<string, short> { ["REF-1"] = 1975 });
            Assert.NotNull(table_Training);
            Assert.Equal(1, table_Training!.RowCount);
            Assert.Equal(table_Input.ColumnCount + 1, table_Training.ColumnCount);
            for (int j = 0; j < table_Input.ColumnCount; j++)
            {
                Assert.Equal(table_Input.GetColumn(j)?.Name, table_Training.GetColumn(j)?.Name);
                Assert.Equal(table_Input.GetValue(0, j), table_Training.GetValue(0, j));
            }

            Assert.Null(((IEnumerable<Table?>?)null).YearBuiltPredictionInputTable());
        }
    }
}
