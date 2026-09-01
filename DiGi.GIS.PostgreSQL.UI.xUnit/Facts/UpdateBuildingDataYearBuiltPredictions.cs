using DiGi.Core.IO.Table.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.Classes;
using DiGi.GIS.WebAPI.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that Modify.UpdateBuildingDataYearBuiltPredictionsAsync rejects arguments it cannot act on, and answers true without sending anything when there are no detections to write.
        /// </summary>
        [Fact]
        public async Task UpdateBuildingDataYearBuiltPredictions_Validation()
        {
            GISWebAPIManager gisWebAPIManager = new(null);

            List<Building2DYearBuiltPredictions> building2DYearBuiltPredictions = [new("b_ref_001", [new YearBuiltPrediction(2020, new BoundingBox2D(10, 20, 30, 40), 0.9)])];

            bool result_NullManager = await Modify.UpdateBuildingDataYearBuiltPredictionsAsync(null, 2212, building2DYearBuiltPredictions);
            Assert.False(result_NullManager);

            bool result_NoCounty = await gisWebAPIManager.UpdateBuildingDataYearBuiltPredictionsAsync(0, building2DYearBuiltPredictions);
            Assert.False(result_NoCounty);

            bool result_NullPredictions = await gisWebAPIManager.UpdateBuildingDataYearBuiltPredictionsAsync(2212, null);
            Assert.False(result_NullPredictions);

            //Nothing to send is not a failure - the run simply found no detections for these buildings
            bool result_Empty = await gisWebAPIManager.UpdateBuildingDataYearBuiltPredictionsAsync(2212, []);
            Assert.True(result_Empty);
        }

        /// <summary>
        /// Verifies that the table the detection write posts carries the reference, the county and the per-year detection columns, so a narrow upsert reaches exactly those columns and leaves the rest of a building's row alone.
        /// </summary>
        [Fact]
        public void UpdateBuildingDataYearBuiltPredictions_Table()
        {
            int countyId = 2212;

            Building2DYearBuiltPredictions building2DYearBuiltPredictions = new("b_ref_001",
            [
                new YearBuiltPrediction(2020, new BoundingBox2D(10, 20, 30, 40), 0.9),
                new YearBuiltPrediction(2021, new BoundingBox2D(11, 21, 31, 41), 0.8)
            ]);

            Table table = new();
            GIS.IO.Modify.Update_Building2D_YearBuiltPredictions(table, countyId, [building2DYearBuiltPredictions]);

            Assert.Equal(1, table.RowCount);

            List<string?> columnNames = [.. (table.Columns ?? []).Select(x => x.Name)];

            Assert.Contains(GIS.IO.Constants.Column.Reference.Name, columnNames);
            Assert.Contains(GIS.IO.Constants.Column.CountyId.Name, columnNames);
            Assert.Contains("Prediction Confidence 2020", columnNames);
            Assert.Contains("Prediction BoundingBox X 2020", columnNames);
            Assert.Contains("Prediction BoundingBox Y 2020", columnNames);
            Assert.Contains("Prediction BoundingBox Width 2020", columnNames);
            Assert.Contains("Prediction BoundingBox Height 2020", columnNames);
            Assert.Contains("Prediction Confidence 2021", columnNames);

            //The pipeline's own output must never travel with its inputs
            Assert.DoesNotContain(GIS.IO.Constants.Column.PredictedYearBuilt.Name, columnNames);
        }
    }
}
