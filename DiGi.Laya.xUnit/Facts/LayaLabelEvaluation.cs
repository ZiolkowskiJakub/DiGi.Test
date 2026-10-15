using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaLabelEvaluation"/> keeps every member through a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies every member, for a populated instance and for one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaLabelEvaluation()
        {
            Classes.LayaLabelEvaluation layaLabelEvaluation = new(
                "soup",
                5,
                3,
                1,
                2,
                0.75,
                0.6,
                0.6666666666666666);

            static void AssertPopulated(Classes.LayaLabelEvaluation? layaLabelEvaluation_Actual)
            {
                Assert.NotNull(layaLabelEvaluation_Actual);
                Assert.Equal("soup", layaLabelEvaluation_Actual!.LabelId);
                Assert.Equal(5, layaLabelEvaluation_Actual.Support);
                Assert.Equal(3, layaLabelEvaluation_Actual.TruePositiveCount);
                Assert.Equal(1, layaLabelEvaluation_Actual.FalsePositiveCount);
                Assert.Equal(2, layaLabelEvaluation_Actual.FalseNegativeCount);
                Assert.Equal(0.75, layaLabelEvaluation_Actual.Precision);
                Assert.Equal(0.6, layaLabelEvaluation_Actual.Recall);
                Assert.Equal(0.6666666666666666, layaLabelEvaluation_Actual.F1);
            }

            AssertPopulated(layaLabelEvaluation);

            string? json = Core.Convert.ToSystem_String(layaLabelEvaluation);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaLabelEvaluation>? layaLabelEvaluations = Core.Convert.ToDiGi<Classes.LayaLabelEvaluation>(json);
            AssertPopulated(layaLabelEvaluations?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaLabelEvaluation);

            Classes.LayaLabelEvaluation layaLabelEvaluation_Copy = new(layaLabelEvaluation);
            AssertPopulated(layaLabelEvaluation_Copy);

            Classes.LayaLabelEvaluation layaLabelEvaluation_Empty = new(null, 0, 0, 0, 0, null, null, null);
            Core.xUnit.Query.SerializationCheck(layaLabelEvaluation_Empty);

            Classes.LayaLabelEvaluation layaLabelEvaluation_EmptyCopy = new(layaLabelEvaluation_Empty);
            Assert.Null(layaLabelEvaluation_EmptyCopy.LabelId);
            Assert.Equal(0, layaLabelEvaluation_EmptyCopy.Support);
            Assert.Null(layaLabelEvaluation_EmptyCopy.Precision);
            Assert.Null(layaLabelEvaluation_EmptyCopy.Recall);
            Assert.Null(layaLabelEvaluation_EmptyCopy.F1);
        }
    }
}
