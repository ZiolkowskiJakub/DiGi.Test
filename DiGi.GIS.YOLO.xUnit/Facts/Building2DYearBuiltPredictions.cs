using DiGi.GIS.Classes;
using DiGi.YOLO.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DiGi.GIS.YOLO.xUnit
{
    using YOLO = DiGi.YOLO;

    public partial class Facts
    {
        /// <summary>
        /// Verifies that Create.Building2DYearBuiltPredictions parses a sample bounding box result file, carrying reference, year, confidence, and bounding box data accurately.
        /// </summary>
        [Fact]
        public void Building2DYearBuiltPredictions_SampleFile()
        {
            string? filePath = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "YOLO_Prediction.bbrf");
            Assert.NotNull(filePath);

            BoundingBoxResultFile? boundingBoxResultFile = DiGi.YOLO.Create.BoundingBoxResultFile(filePath);
            Assert.NotNull(boundingBoxResultFile);

            List<Building2DYearBuiltPredictions>? predictions = Create.Building2DYearBuiltPredictions(boundingBoxResultFile);
            Assert.NotNull(predictions);
            Assert.Equal(2, predictions!.Count);

            Building2DYearBuiltPredictions? prediction_0207 = predictions.FirstOrDefault(x => x.Reference == "0207");
            Assert.NotNull(prediction_0207);
            Assert.NotNull(prediction_0207!.Years);
            Assert.Single(prediction_0207.Years!);
            Assert.Equal((ushort)2021, prediction_0207.Years![0]);

            YearBuiltPrediction? yearPrediction_0207 = prediction_0207[2021];
            Assert.NotNull(yearPrediction_0207);
            Assert.Equal((ushort)2021, yearPrediction_0207!.Year);
            Assert.True(yearPrediction_0207.Confidence > 0.4);
            Assert.NotNull(yearPrediction_0207.BoundingBox);

            Building2DYearBuiltPredictions? prediction_0209 = predictions.FirstOrDefault(x => x.Reference == "0209");
            Assert.NotNull(prediction_0209);
            Assert.NotNull(prediction_0209!.Years);
            Assert.Single(prediction_0209.Years!);

            YearBuiltPrediction? yearPrediction_0209 = prediction_0209[2021];
            Assert.NotNull(yearPrediction_0209);
            Assert.Equal((ushort)2021, yearPrediction_0209!.Year);
        }

        /// <summary>
        /// Verifies that building references containing underscores are correctly parsed from the last underscore delimiter rather than truncated or dropped.
        /// </summary>
        [Fact]
        public void Building2DYearBuiltPredictions_UnderscoredReference()
        {
            BoundingBoxResultFile boundingBoxResultFile =
            [
                new BoundingBoxResult("PL_12345_6789_1990.jpeg", 0, 100.0, 200.0, 50.0, 60.0, 0.85)
            ];

            List<Building2DYearBuiltPredictions>? predictions = Create.Building2DYearBuiltPredictions(boundingBoxResultFile);
            Assert.NotNull(predictions);
            Assert.Single(predictions!);
            Assert.Equal("PL_12345_6789", predictions![0].Reference);

            YearBuiltPrediction? yearPrediction = predictions[0][1990];
            Assert.NotNull(yearPrediction);
            Assert.Equal((ushort)1990, yearPrediction!.Year);
            Assert.Equal(0.85, yearPrediction.Confidence);
        }

        /// <summary>
        /// Verifies that null inputs or empty bounding box files return null or empty prediction collections safely without generating malformed entries.
        /// </summary>
        [Fact]
        public void Building2DYearBuiltPredictions_EmptyDetection()
        {
            List<Building2DYearBuiltPredictions>? predictions_Null = Create.Building2DYearBuiltPredictions((BoundingBoxResultFile?)null);
            Assert.Null(predictions_Null);

            BoundingBoxResultFile boundingBoxResultFile_Empty = [];
            List<Building2DYearBuiltPredictions>? predictions_Empty = Create.Building2DYearBuiltPredictions(boundingBoxResultFile_Empty);
            Assert.NotNull(predictions_Empty);
            Assert.Empty(predictions_Empty!);
        }
    }
}
