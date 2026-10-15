using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaLabelSetEvaluation"/> keeps every member through a string round trip and <c>SerializationCheck</c> with the nested label evaluations and confusion rows, and that its copy constructor copies every member, for a populated instance and for one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaLabelSetEvaluation()
        {
            Classes.LayaLabelSetEvaluation layaLabelSetEvaluation = new(
                "category",
                Enums.LayaLabelSetKind.SingleChoice,
                4,
                0.5,
                1.0,
                0.375,
                0.5,
                0.55,
                0.71,
                [new Classes.LayaLabelEvaluation("soup", 2, 1, 1, 1, 0.5, 0.5, 0.5)],
                [new Classes.LayaConfusionRow("soup", "soup", 1), new Classes.LayaConfusionRow("soup", "dessert", 1)]);

            static void AssertPopulated(Classes.LayaLabelSetEvaluation? layaLabelSetEvaluation_Actual)
            {
                Assert.NotNull(layaLabelSetEvaluation_Actual);
                Assert.Equal("category", layaLabelSetEvaluation_Actual!.LabelSetId);
                Assert.Equal(Enums.LayaLabelSetKind.SingleChoice, layaLabelSetEvaluation_Actual.Kind);
                Assert.Equal(4, layaLabelSetEvaluation_Actual.ItemCount);
                Assert.Equal(0.5, layaLabelSetEvaluation_Actual.Top1Accuracy);
                Assert.Equal(1.0, layaLabelSetEvaluation_Actual.Top3Accuracy);
                Assert.Equal(0.375, layaLabelSetEvaluation_Actual.MicroF1);
                Assert.Equal(0.5, layaLabelSetEvaluation_Actual.MacroF1);
                Assert.Equal(0.55, layaLabelSetEvaluation_Actual.ExpectedCalibrationError);
                Assert.Equal(0.71, layaLabelSetEvaluation_Actual.BrierScore);
                Assert.Equal("soup", Assert.Single(layaLabelSetEvaluation_Actual.LabelEvaluations!).LabelId);
                Assert.Equal(["soup", "dessert"], layaLabelSetEvaluation_Actual.ConfusionRows!.Select(x => x.PredictedLabelId));
            }

            AssertPopulated(layaLabelSetEvaluation);

            string? json = Core.Convert.ToSystem_String(layaLabelSetEvaluation);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaLabelSetEvaluation>? layaLabelSetEvaluations = Core.Convert.ToDiGi<Classes.LayaLabelSetEvaluation>(json);
            AssertPopulated(layaLabelSetEvaluations?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaLabelSetEvaluation);

            Classes.LayaLabelSetEvaluation layaLabelSetEvaluation_Copy = new(layaLabelSetEvaluation);
            AssertPopulated(layaLabelSetEvaluation_Copy);
            Assert.NotSame(layaLabelSetEvaluation.LabelEvaluations![0], layaLabelSetEvaluation_Copy.LabelEvaluations![0]);
            Assert.NotSame(layaLabelSetEvaluation.ConfusionRows![0], layaLabelSetEvaluation_Copy.ConfusionRows![0]);

            Classes.LayaLabelSetEvaluation layaLabelSetEvaluation_Empty = new(null, Enums.LayaLabelSetKind.MultiLabel, 0, null, null, null, null, null, null, null, null);
            Core.xUnit.Query.SerializationCheck(layaLabelSetEvaluation_Empty);

            Classes.LayaLabelSetEvaluation layaLabelSetEvaluation_EmptyCopy = new(layaLabelSetEvaluation_Empty);
            Assert.Null(layaLabelSetEvaluation_EmptyCopy.LabelSetId);
            Assert.Equal(0, layaLabelSetEvaluation_EmptyCopy.ItemCount);
            Assert.Null(layaLabelSetEvaluation_EmptyCopy.Top1Accuracy);
            Assert.Null(layaLabelSetEvaluation_EmptyCopy.Top3Accuracy);
            Assert.Null(layaLabelSetEvaluation_EmptyCopy.MicroF1);
            Assert.Null(layaLabelSetEvaluation_EmptyCopy.MacroF1);
            Assert.Null(layaLabelSetEvaluation_EmptyCopy.ExpectedCalibrationError);
            Assert.Null(layaLabelSetEvaluation_EmptyCopy.BrierScore);
            Assert.Null(layaLabelSetEvaluation_EmptyCopy.LabelEvaluations);
            Assert.Null(layaLabelSetEvaluation_EmptyCopy.ConfusionRows);
        }
    }
}
