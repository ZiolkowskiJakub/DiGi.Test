using DiGi.Core;
using DiGi.Core.Interfaces;
using DiGi.Core.IO;
using DiGi.Core.IO.Table.Classes;
using DiGi.Core.IO.Table.Interfaces;
using DiGi.GIS.ML;
using DiGi.GIS.ML.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that PredictedYearBuilts returns null when provided with null, an empty table, or a table missing the reference column.
        /// </summary>
        [Fact]
        public void PredictedYearBuilts_NullOrMissingReference()
        {
            Table? table_Null = null;
            Assert.Null(table_Null.PredictedYearBuilts());

            YearBuiltPredictor predictor = new();
            Assert.Null(predictor.Predict(table_Null));

            Table table_WithoutReference = new();
            table_WithoutReference.AddColumn("Storeys", typeof(ushort));
            table_WithoutReference.AddRow([2]);

            Assert.Null(table_WithoutReference.PredictedYearBuilts());
            Assert.Null(predictor.Predict(table_WithoutReference));
        }

        /// <summary>
        /// Verifies that PredictedYearBuilts correctly scores rows from an input table carrying canonical features and returns predicted construction years.
        /// </summary>
        [Fact]
        public void PredictedYearBuilts_Scoring()
        {
            Table table = new();
            table.AddColumn(GIS.IO.Constants.Column.Reference);
            table.AddColumn(GIS.IO.Constants.Column.Storeys);
            table.AddColumn(GIS.IO.Constants.Column.FloorArea);
            table.AddColumn(GIS.IO.Constants.Column.InternalPointX);
            table.AddColumn(GIS.IO.Constants.Column.InternalPointY);

            table.AddRow(["PL.PZGiK.338.2415.B1", (ushort)2, 120.5, 500000.0, 600000.0]);
            table.AddRow(["PL.PZGiK.338.2415.B2", (ushort)1, 85.0, 500100.0, 600100.0]);

            Table? table_Predictions = table.PredictedYearBuilts();
            Assert.NotNull(table_Predictions);
            Assert.Equal(2, table_Predictions.RowCount);
            Assert.Equal(2, table_Predictions.ColumnCount);

            int index_Reference = table_Predictions.GetColumnIndex(GIS.IO.Constants.Column.Reference.Name);
            int index_PredictedYear = table_Predictions.GetColumnIndex(GIS.IO.Constants.Column.PredictedYearBuilt.Name);

            Assert.True(index_Reference >= 0);
            Assert.True(index_PredictedYear >= 0);

            string? ref1 = table_Predictions.GetValue<string>(0, index_Reference);
            Assert.Equal("PL.PZGiK.338.2415.B1", ref1);

            Assert.True(table_Predictions.TryGetValue(0, index_PredictedYear, out ushort year1));
            Assert.True(year1 >= 1900 && year1 <= 2030);

            string? ref2 = table_Predictions.GetValue<string>(1, index_Reference);
            Assert.Equal("PL.PZGiK.338.2415.B2", ref2);

            Assert.True(table_Predictions.TryGetValue(1, index_PredictedYear, out ushort year2));
            Assert.True(year2 >= 1900 && year2 <= 2030);

            // Verify YearBuiltPredictor yields identical results
            YearBuiltPredictor predictor = new();
            Table? table_PredictorResult = predictor.Predict(table);
            Assert.NotNull(table_PredictorResult);
            Assert.Equal(2, table_PredictorResult.RowCount);
            Assert.True(table_PredictorResult.TryGetValue(0, index_PredictedYear, out ushort yearPredictor1));
            Assert.Equal(year1, yearPredictor1);
        }

        /// <summary>
        /// Verifies that presence of the PredictedYearBuilt output column in the input table does not cause target leakage or fail the prediction.
        /// </summary>
        [Fact]
        public void PredictedYearBuilts_TargetLeakageProtection()
        {
            Table table = new();
            table.AddColumn(GIS.IO.Constants.Column.Reference);
            table.AddColumn(GIS.IO.Constants.Column.Storeys);
            table.AddColumn(GIS.IO.Constants.Column.FloorArea);
            table.AddColumn(GIS.IO.Constants.Column.PredictedYearBuilt);

            table.AddRow(["PL.PZGiK.338.2415.B1", (ushort)2, 120.5, (ushort)1950]);

            Table? table_Predictions = table.PredictedYearBuilts();
            Assert.NotNull(table_Predictions);
            Assert.Equal(1, table_Predictions.RowCount);

            int index_PredictedYear = table_Predictions.GetColumnIndex(GIS.IO.Constants.Column.PredictedYearBuilt.Name);
            Assert.True(table_Predictions.TryGetValue(0, index_PredictedYear, out ushort predictedYear));
            Assert.True(predictedYear >= 1900);
        }

        /// <summary>
        /// Verifies that legacy alternative display names (such as Location X and Polpulation) are correctly resolved as fallbacks.
        /// </summary>
        [Fact]
        public void PredictedYearBuilts_AlternativeNameFallback()
        {
            Table table = new();
            table.AddColumn("Reference", typeof(string));
            table.AddColumn("Location X", typeof(double));
            table.AddColumn("Location Y", typeof(double));
            table.AddColumn("Polpulation 2020", typeof(float));

            table.AddRow(["PL.PZGiK.338.2415.B1", 500000.0, 600000.0, 15000.0F]);

            Table? table_Predictions = table.PredictedYearBuilts();
            Assert.NotNull(table_Predictions);
            Assert.Equal(1, table_Predictions.RowCount);

            int index_PredictedYear = table_Predictions.GetColumnIndex(GIS.IO.Constants.Column.PredictedYearBuilt.Name);
            Assert.True(table_Predictions.TryGetValue(0, index_PredictedYear, out ushort year));
            Assert.True(year >= 1900 && year <= 2030);
        }

        /// <summary>
        /// Verifies that YearBuiltPredictor helper methods expose the S5 input allow-list columns and distinct unique identifiers as normalized slugs.
        /// </summary>
        [Fact]
        public void YearBuiltPredictor_Helpers()
        {
            List<Column> columns = YearBuiltPredictor.InputColumns();
            Assert.NotNull(columns);
            Assert.Equal(172, columns.Count);

            List<string> uniqueIds = YearBuiltPredictor.InputColumnUniqueIds();
            Assert.NotNull(uniqueIds);
            Assert.Equal(172, uniqueIds.Count);

            HashSet<string> set_UniqueIds = [.. uniqueIds];
            Assert.Equal(172, set_UniqueIds.Count);

            Assert.Contains("floor_area", set_UniqueIds);
            Assert.Contains("storeys", set_UniqueIds);
            Assert.Contains("internal_point_x", set_UniqueIds);
            Assert.Contains("internal_point_y", set_UniqueIds);
            Assert.Contains("radial_floor_area_ratio_200m", set_UniqueIds);
            Assert.Contains("grid_cell_coverage_0_0", set_UniqueIds);
        }

        /// <summary>
        /// Verifies that <see cref="YearBuiltPredictor.InputColumnUniqueIds"/> returns normalized column slugs matching <see cref="Core.IO.Query.UniqueId(IColumn?)"/> rather than <see cref="Core.Query.UniqueId(ISerializableObject?)"/> content hashes.
        /// </summary>
        [Fact]
        public void YearBuiltPredictor_InputColumnUniqueIds_SlugVersusContentHash()
        {
            Column column_FloorArea = GIS.IO.Constants.Column.FloorArea;
            string? slug_FloorArea = column_FloorArea.UniqueId();
            string? hash_FloorArea = Core.Query.UniqueId((ISerializableObject)column_FloorArea);

            Assert.Equal("floor_area", slug_FloorArea);
            Assert.NotEqual(slug_FloorArea, hash_FloorArea);

            List<string> uniqueIds = YearBuiltPredictor.InputColumnUniqueIds();
            Assert.Contains("floor_area", uniqueIds);
            Assert.Contains("storeys", uniqueIds);
            Assert.Contains("internal_point_x", uniqueIds);
            Assert.Contains("radial_floor_area_ratio_200m", uniqueIds);
            Assert.DoesNotContain(hash_FloorArea, uniqueIds);
        }
    }
}
