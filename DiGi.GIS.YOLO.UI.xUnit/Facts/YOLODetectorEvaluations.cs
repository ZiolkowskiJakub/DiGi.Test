using DiGi.Core.Classes;
using DiGi.YOLO.Classes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the regressor-free detector score over a fixture detection file: the first detection years read back through the pipeline&apos;s own columns, scored against the labels for all Test buildings and for the clean subset.
        /// <para>The fixture detects <c>0207</c> and <c>0209</c> in 2021 and <c>0208</c> in no year. Labelled 2021, 2015 and 2018, the errors are 0, -7 (a building never detected reads as the first year of the range, 2008, as the regressor sees it) and +3. <c>0209</c> is Legacy, so the clean subset is the first two. A Train building and a Test building with no image are not scored at all. The expected numbers are worked out by hand, not with the code under test.</para>
        /// </summary>
        [Fact]
        public void YOLODetectorEvaluations()
        {
            string? path_Fixture = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "YOLO_Prediction.bbrf");
            Assert.False(string.IsNullOrWhiteSpace(path_Fixture));

            BoundingBoxResultFile? boundingBoxResultFile = DiGi.YOLO.Create.BoundingBoxResultFile(path_Fixture);
            Assert.NotNull(boundingBoxResultFile);

            List<Classes.DatasetReference> datasetReferences =
            [
                new("0207", 73485, DiGi.YOLO.Enums.Category.Test, 2021, Enums.LegacySource.None),
                new("0208", 73485, DiGi.YOLO.Enums.Category.Test, 2015, Enums.LegacySource.None),
                new("0209", 73482, DiGi.YOLO.Enums.Category.Test, 2018, Enums.LegacySource.Tsv),
                new("0210", 73485, DiGi.YOLO.Enums.Category.Train, 1950, Enums.LegacySource.None),
                new("0211", 73485, DiGi.YOLO.Enums.Category.Test, 1950, Enums.LegacySource.None)
            ];

            List<string> references_Imaged = ["0207", "0208", "0209", "0210"];

            List<Classes.YOLODetectorEvaluation> yOLODetectorEvaluations = Query.YOLODetectorEvaluations(datasetReferences, boundingBoxResultFile, references_Imaged, "model.pt", "abc", new Range<int>(2008, 2025));

            Assert.Equal(2, yOLODetectorEvaluations.Count);

            Classes.YOLODetectorEvaluation yOLODetectorEvaluation_All = yOLODetectorEvaluations[0];
            Assert.Equal(Enums.YOLODetectorEvaluationSubset.All, yOLODetectorEvaluation_All.Subset);
            Assert.Equal("model.pt", yOLODetectorEvaluation_All.WeightsPath);
            Assert.Equal("abc", yOLODetectorEvaluation_All.SHA256);
            Assert.Equal(3, yOLODetectorEvaluation_All.Count);
            Assert.Equal(10.0 / 3.0, yOLODetectorEvaluation_All.MeanAbsoluteError, 9);
            Assert.Equal(Math.Sqrt(58.0 / 3.0), yOLODetectorEvaluation_All.RootMeanSquareError, 9);
            Assert.Equal(1.0 / 3.0, yOLODetectorEvaluation_All.ExactShare, 9);

            Classes.YOLODetectorEvaluation yOLODetectorEvaluation_Clean = yOLODetectorEvaluations[1];
            Assert.Equal(Enums.YOLODetectorEvaluationSubset.Clean, yOLODetectorEvaluation_Clean.Subset);
            Assert.Equal(2, yOLODetectorEvaluation_Clean.Count);
            Assert.Equal(3.5, yOLODetectorEvaluation_Clean.MeanAbsoluteError, 9);
            Assert.Equal(Math.Sqrt(24.5), yOLODetectorEvaluation_Clean.RootMeanSquareError, 9);
            Assert.Equal(0.5, yOLODetectorEvaluation_Clean.ExactShare, 9);

            // An unknown Legacy decision is not clean.
            datasetReferences[1] = new Classes.DatasetReference("0208", 73485, DiGi.YOLO.Enums.Category.Test, 2015, Enums.LegacySource.Unknown);
            Classes.YOLODetectorEvaluation yOLODetectorEvaluation_Unknown = Query.YOLODetectorEvaluations(datasetReferences, boundingBoxResultFile, references_Imaged, "model.pt", "abc", null)[1];
            Assert.Equal(1, yOLODetectorEvaluation_Unknown.Count);
            Assert.Equal(0, yOLODetectorEvaluation_Unknown.MeanAbsoluteError, 9);

            // The same detections from two weights files are two identical row pairs - one per file, the files told apart by their identity.
            List<Classes.YOLODetectorEvaluation> yOLODetectorEvaluations_Weights = [];
            List<string> weightsPaths = ["model.pt", "model_train9_continue.pt", "model_train9_fresh.pt"];
            foreach (string weightsPath in weightsPaths)
            {
                yOLODetectorEvaluations_Weights.AddRange(Query.YOLODetectorEvaluations(datasetReferences, boundingBoxResultFile, references_Imaged, weightsPath, weightsPath, null));
            }

            Assert.Equal(6, yOLODetectorEvaluations_Weights.Count);
            Assert.Equal(3, yOLODetectorEvaluations_Weights.FindAll(x => x.Subset == Enums.YOLODetectorEvaluationSubset.Clean).Count);

            // No detections at all: every imaged Test building reads as first detected at the start of the range.
            Classes.YOLODetectorEvaluation yOLODetectorEvaluation_None = Query.YOLODetectorEvaluations(datasetReferences, [], references_Imaged, "model.pt", "abc", null)[0];
            Assert.Equal(3, yOLODetectorEvaluation_None.Count);
            Assert.Equal((13.0 + 7.0 + 10.0) / 3.0, yOLODetectorEvaluation_None.MeanAbsoluteError, 9);

            Assert.Empty(Query.YOLODetectorEvaluations(null, boundingBoxResultFile, references_Imaged, "model.pt", "abc", null));
        }
    }
}
