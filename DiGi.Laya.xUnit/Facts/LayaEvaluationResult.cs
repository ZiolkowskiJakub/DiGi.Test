using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaEvaluationResult"/> keeps every member through a string round trip and <c>SerializationCheck</c> with the nested label set evaluations, and that its copy constructor copies every member, for a populated instance and for one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaEvaluationResult()
        {
            Classes.LayaEvaluationResult layaEvaluationResult = new(
                [new Classes.LayaLabelSetEvaluation("tags", Enums.LayaLabelSetKind.MultiLabel, 3, null, null, 0.5714285714285714, 0.5, 0.3916666666666667, 0.18541666666666667, [new Classes.LayaLabelEvaluation("vegan", 2, 2, 0, 0, 1.0, 1.0, 1.0)], [])]);

            static void AssertPopulated(Classes.LayaEvaluationResult? layaEvaluationResult_Actual)
            {
                Assert.NotNull(layaEvaluationResult_Actual);
                Classes.LayaLabelSetEvaluation layaLabelSetEvaluation = Assert.Single(layaEvaluationResult_Actual!.LabelSetEvaluations!);
                Assert.Equal("tags", layaLabelSetEvaluation.LabelSetId);
                Assert.Equal(0.5714285714285714, layaLabelSetEvaluation.MicroF1);
                Assert.Null(layaLabelSetEvaluation.Top1Accuracy);
                Assert.Equal("vegan", Assert.Single(layaLabelSetEvaluation.LabelEvaluations!).LabelId);
                Assert.Empty(layaLabelSetEvaluation.ConfusionRows!);
            }

            AssertPopulated(layaEvaluationResult);

            string? json = Core.Convert.ToSystem_String(layaEvaluationResult);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaEvaluationResult>? layaEvaluationResults = Core.Convert.ToDiGi<Classes.LayaEvaluationResult>(json);
            AssertPopulated(layaEvaluationResults?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaEvaluationResult);

            Classes.LayaEvaluationResult layaEvaluationResult_Copy = new(layaEvaluationResult);
            AssertPopulated(layaEvaluationResult_Copy);
            Assert.NotSame(layaEvaluationResult.LabelSetEvaluations![0], layaEvaluationResult_Copy.LabelSetEvaluations![0]);

            Classes.LayaEvaluationResult layaEvaluationResult_Empty = new((IEnumerable<Classes.LayaLabelSetEvaluation>?)null);
            Core.xUnit.Query.SerializationCheck(layaEvaluationResult_Empty);

            Classes.LayaEvaluationResult layaEvaluationResult_EmptyCopy = new(layaEvaluationResult_Empty);
            Assert.Null(layaEvaluationResult_EmptyCopy.LabelSetEvaluations);
        }
    }
}
