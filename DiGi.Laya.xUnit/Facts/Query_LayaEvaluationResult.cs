using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies <see cref="Query.LayaEvaluationResult(IEnumerable{Classes.LayaLabelAssignment}?, IEnumerable{Classes.LayaClassificationResult}?, IEnumerable{Classes.LayaLabelSet}?)"/> against metrics computed by hand.
        /// <para>Single-choice set a/b/c over four items (truths a, b, c, a; ranks a-b-c, a-b-c, c-b-a, b-a-c, a tie on 0.1 going to the earlier label): top-1 accuracy 0.5, top-3 accuracy 1; a precision/recall/F1 0.5/0.5/0.5, b 0/0/0, c 1/1/1, macro F1 0.5; confusion rows a-a, a-b, b-a, c-c; ECE (|1-0.7| + |0-0.6| + |1-0.5| + |0-0.8|) / 4 = 0.55; Brier (0.14 + 0.86 + 0.38 + 1.46) / 4 = 0.71. An item with a truth but no result and one with a result but no truth are not counted.</para>
        /// <para>Multi-label set x/y over three items, closed-world per item, the decision being <c>IsSuggested</c> (a forbidden 0.7 counts as not suggested): x TP 2, y FP 1 and FN 2; micro F1 4/7, macro F1 0.5; ECE 2.35 / 6; Brier 1.1125 / 6.</para>
        /// </summary>
        [Fact]
        public void Query_LayaEvaluationResult()
        {
            Classes.LayaLabelSet layaLabelSet_Category = new("category", "Which?", Enums.LayaLabelSetKind.SingleChoice, [new Classes.LayaLabel("a", null, null), new Classes.LayaLabel("b", null, null), new Classes.LayaLabel("c", null, null)]);
            Classes.LayaLabelSet layaLabelSet_Tags = new("tags", "It is {label}.", Enums.LayaLabelSetKind.MultiLabel, [new Classes.LayaLabel("x", null, null), new Classes.LayaLabel("y", null, null)]);

            static Classes.LayaLabelAssignment Assignment(string itemId, string labelSetId, string labelId, bool truth)
            {
                return new Classes.LayaLabelAssignment(itemId, labelSetId, labelId, truth, 1.0, null);
            }

            static Classes.LayaClassificationResult Result(string itemId, params Classes.LayaLabelScore[] layaLabelScores)
            {
                return new Classes.LayaClassificationResult(itemId, layaLabelScores, null, null, null);
            }

            static Classes.LayaLabelScore Score(string labelSetId, string labelId, double probability, bool isSuggested = false, bool isForbidden = false)
            {
                return new Classes.LayaLabelScore(labelSetId, labelId, probability, isSuggested, isForbidden, null);
            }

            List<Classes.LayaLabelAssignment> layaLabelAssignments =
            [
                Assignment("i1", "category", "a", true),
                Assignment("i2", "category", "b", true),
                Assignment("i3", "category", "c", true),
                Assignment("i4", "category", "a", true),
                Assignment("i5", "category", "b", true),
                Assignment("j1", "tags", "x", true),
                Assignment("j2", "tags", "y", true),
                Assignment("j2", "tags", "x", false),
                Assignment("j3", "tags", "x", true),
                Assignment("j3", "tags", "y", true)
            ];

            List<Classes.LayaClassificationResult> layaClassificationResults =
            [
                Result("i1", Score("category", "a", 0.7), Score("category", "b", 0.2), Score("category", "c", 0.1)),
                Result("i2", Score("category", "a", 0.6), Score("category", "b", 0.3), Score("category", "c", 0.1)),
                Result("i3", Score("category", "a", 0.2), Score("category", "b", 0.3), Score("category", "c", 0.5)),
                Result("i4", Score("category", "a", 0.1), Score("category", "b", 0.8), Score("category", "c", 0.1)),
                Result("i6", Score("category", "a", 0.9), Score("category", "b", 0.05), Score("category", "c", 0.05)),
                Result("j1", Score("tags", "x", 0.9, true), Score("tags", "y", 0.6, true)),
                Result("j2", Score("tags", "x", 0.3), Score("tags", "y", 0.4)),
                Result("j3", Score("tags", "x", 0.55, true), Score("tags", "y", 0.7, false, true))
            ];

            Classes.LayaEvaluationResult? layaEvaluationResult = Query.LayaEvaluationResult(layaLabelAssignments, layaClassificationResults, [layaLabelSet_Category, layaLabelSet_Tags]);
            Assert.NotNull(layaEvaluationResult);
            Assert.Equal(["category", "tags"], layaEvaluationResult!.LabelSetEvaluations!.Select(x => x.LabelSetId));

            const double tolerance = 1e-12;

            Classes.LayaLabelSetEvaluation layaLabelSetEvaluation_Category = layaEvaluationResult.LabelSetEvaluations![0];
            Assert.Equal(Enums.LayaLabelSetKind.SingleChoice, layaLabelSetEvaluation_Category.Kind);
            Assert.Equal(4, layaLabelSetEvaluation_Category.ItemCount);
            Assert.Equal(0.5, layaLabelSetEvaluation_Category.Top1Accuracy!.Value, tolerance);
            Assert.Equal(1.0, layaLabelSetEvaluation_Category.Top3Accuracy!.Value, tolerance);
            Assert.Null(layaLabelSetEvaluation_Category.MicroF1);
            Assert.Equal(0.5, layaLabelSetEvaluation_Category.MacroF1!.Value, tolerance);
            Assert.Equal(0.55, layaLabelSetEvaluation_Category.ExpectedCalibrationError!.Value, tolerance);
            Assert.Equal(0.71, layaLabelSetEvaluation_Category.BrierScore!.Value, tolerance);

            List<Classes.LayaLabelEvaluation> layaLabelEvaluations_Category = layaLabelSetEvaluation_Category.LabelEvaluations!;
            Assert.Equal(["a", "b", "c"], layaLabelEvaluations_Category.Select(x => x.LabelId));
            Assert.Equal([2, 1, 1], layaLabelEvaluations_Category.Select(x => x.Support));
            Assert.Equal([1, 0, 1], layaLabelEvaluations_Category.Select(x => x.TruePositiveCount));
            Assert.Equal([1, 1, 0], layaLabelEvaluations_Category.Select(x => x.FalsePositiveCount));
            Assert.Equal([1, 1, 0], layaLabelEvaluations_Category.Select(x => x.FalseNegativeCount));
            Assert.Equal([0.5, 0.0, 1.0], layaLabelEvaluations_Category.Select(x => x.Precision!.Value));
            Assert.Equal([0.5, 0.0, 1.0], layaLabelEvaluations_Category.Select(x => x.Recall!.Value));
            Assert.Equal([0.5, 0.0, 1.0], layaLabelEvaluations_Category.Select(x => x.F1!.Value));

            Assert.Equal(["a>a:1", "a>b:1", "b>a:1", "c>c:1"], layaLabelSetEvaluation_Category.ConfusionRows!.Select(x => string.Concat(x.ActualLabelId, ">", x.PredictedLabelId, ":", x.Count.ToString(System.Globalization.CultureInfo.InvariantCulture))));

            Classes.LayaLabelSetEvaluation layaLabelSetEvaluation_Tags = layaEvaluationResult.LabelSetEvaluations![1];
            Assert.Equal(Enums.LayaLabelSetKind.MultiLabel, layaLabelSetEvaluation_Tags.Kind);
            Assert.Equal(3, layaLabelSetEvaluation_Tags.ItemCount);
            Assert.Null(layaLabelSetEvaluation_Tags.Top1Accuracy);
            Assert.Null(layaLabelSetEvaluation_Tags.Top3Accuracy);
            Assert.Equal(4.0 / 7.0, layaLabelSetEvaluation_Tags.MicroF1!.Value, tolerance);
            Assert.Equal(0.5, layaLabelSetEvaluation_Tags.MacroF1!.Value, tolerance);
            Assert.Equal(2.35 / 6.0, layaLabelSetEvaluation_Tags.ExpectedCalibrationError!.Value, tolerance);
            Assert.Equal(1.1125 / 6.0, layaLabelSetEvaluation_Tags.BrierScore!.Value, tolerance);
            Assert.Empty(layaLabelSetEvaluation_Tags.ConfusionRows!);

            List<Classes.LayaLabelEvaluation> layaLabelEvaluations_Tags = layaLabelSetEvaluation_Tags.LabelEvaluations!;
            Assert.Equal([2, 2], layaLabelEvaluations_Tags.Select(x => x.Support));
            Assert.Equal([2, 0], layaLabelEvaluations_Tags.Select(x => x.TruePositiveCount));
            Assert.Equal([0, 1], layaLabelEvaluations_Tags.Select(x => x.FalsePositiveCount));
            Assert.Equal([0, 2], layaLabelEvaluations_Tags.Select(x => x.FalseNegativeCount));
            Assert.Equal(1.0, layaLabelEvaluations_Tags[0].F1);
            Assert.Equal(0.0, layaLabelEvaluations_Tags[1].F1);
            Assert.Equal(0.0, layaLabelEvaluations_Tags[1].Precision);

            //Nothing to evaluate: counts of zero, metrics null
            Classes.LayaLabelSetEvaluation layaLabelSetEvaluation_None = Query.LayaEvaluationResult([], layaClassificationResults, [layaLabelSet_Category])!.LabelSetEvaluations![0];
            Assert.Equal(0, layaLabelSetEvaluation_None.ItemCount);
            Assert.Null(layaLabelSetEvaluation_None.Top1Accuracy);
            Assert.Null(layaLabelSetEvaluation_None.MacroF1);
            Assert.Null(layaLabelSetEvaluation_None.ExpectedCalibrationError);
            Assert.Null(layaLabelSetEvaluation_None.BrierScore);
            Assert.All(layaLabelSetEvaluation_None.LabelEvaluations!, x => Assert.Null(x.F1));

            Assert.Null(Query.LayaEvaluationResult(null, layaClassificationResults, [layaLabelSet_Category]));
        }
    }
}
