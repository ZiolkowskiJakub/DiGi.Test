using DiGi.GIS.Classes;
using DiGi.Geometry.Planar.Classes;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that when two detections share the same building-year, the higher-confidence prediction is
        /// retained regardless of file order. Reproduces #10 — the constructor previously kept whichever
        /// detection appeared last in the file.
        /// </summary>
        [Fact]
        public void Building2DYearBuiltPredictions()
        {
            BoundingBox2D boundingBox2D_A = new(0.0, 0.0, 10.0, 10.0);
            BoundingBox2D boundingBox2D_B = new(5.0, 5.0, 8.0, 8.0);

            YearBuiltPrediction yearBuiltPrediction_Higher = new(2020, boundingBox2D_A, 0.85);
            YearBuiltPrediction yearBuiltPrediction_Lower = new(2020, boundingBox2D_B, 0.40);

            // Higher confidence first in file order — last-write-wins would keep the lower one.
            Building2DYearBuiltPredictions building2DYearBuiltPredictions_1 = new("ref_1001", [yearBuiltPrediction_Higher, yearBuiltPrediction_Lower]);

            Assert.NotNull(building2DYearBuiltPredictions_1.Years);
            Assert.Single(building2DYearBuiltPredictions_1.Years!);
            Assert.Equal(2020, building2DYearBuiltPredictions_1.Years![0]);

            YearBuiltPrediction? yearBuiltPrediction_Retained_1 = building2DYearBuiltPredictions_1[2020];
            Assert.NotNull(yearBuiltPrediction_Retained_1);
            Assert.Equal(0.85, yearBuiltPrediction_Retained_1.Confidence, precision: 3);
            Assert.Equal(0.0, yearBuiltPrediction_Retained_1.BoundingBox!.Min.X, precision: 3);

            // Lower confidence first in file order — the fix must still keep the higher one,
            // proving the rule is highest-confidence, not first-in-file.
            Building2DYearBuiltPredictions building2DYearBuiltPredictions_2 = new("ref_1002", [yearBuiltPrediction_Lower, yearBuiltPrediction_Higher]);

            YearBuiltPrediction? yearBuiltPrediction_Retained_2 = building2DYearBuiltPredictions_2[2020];
            Assert.NotNull(yearBuiltPrediction_Retained_2);
            Assert.Equal(0.85, yearBuiltPrediction_Retained_2.Confidence, precision: 3);
            Assert.Equal(0.0, yearBuiltPrediction_Retained_2.BoundingBox!.Min.X, precision: 3);

            Core.xUnit.Query.SerializationCheck(building2DYearBuiltPredictions_1);
        }
    }
}