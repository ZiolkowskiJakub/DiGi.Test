using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaChoiceAnswer"/> keeps its name, both confidences, label and ordered probabilities, reports <see cref="Enums.LayaQuestionType.Choice"/>, survives a string round trip and <c>SerializationCheck</c> with the nested probability list, and that its copy constructor copies every member and clones the probabilities, for a populated answer and for one with every member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaChoiceAnswer()
        {
            Classes.LayaChoiceAnswer layaChoiceAnswer = new(
                "department",
                0.7412,
                0.9135,
                "billing",
                [new Classes.LayaProbability("billing", 0.9135), new Classes.LayaProbability("technical", 0.0865)]);

            static void AssertPopulated(Classes.LayaChoiceAnswer? layaChoiceAnswer_Actual)
            {
                Assert.NotNull(layaChoiceAnswer_Actual);
                Assert.Equal("department", layaChoiceAnswer_Actual!.Name);
                Assert.Equal(0.7412, layaChoiceAnswer_Actual.Confidence);
                Assert.Equal(0.9135, layaChoiceAnswer_Actual.AnswerConfidence);
                Assert.Equal("billing", layaChoiceAnswer_Actual.Label);
                Assert.Equal(Enums.LayaQuestionType.Choice, layaChoiceAnswer_Actual.QuestionType);
                Assert.Equal(["billing", "technical"], layaChoiceAnswer_Actual.Probabilities!.Select(x => x.Label));
                Assert.Equal([0.9135, 0.0865], layaChoiceAnswer_Actual.Probabilities!.Select(x => x.Value));
            }

            AssertPopulated(layaChoiceAnswer);

            string? json = Core.Convert.ToSystem_String(layaChoiceAnswer);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaChoiceAnswer>? layaChoiceAnswers = Core.Convert.ToDiGi<Classes.LayaChoiceAnswer>(json);
            AssertPopulated(layaChoiceAnswers?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaChoiceAnswer);

            Classes.LayaChoiceAnswer layaChoiceAnswer_Copy = new(layaChoiceAnswer);
            AssertPopulated(layaChoiceAnswer_Copy);
            Assert.NotSame(layaChoiceAnswer.Probabilities, layaChoiceAnswer_Copy.Probabilities);
            Assert.NotSame(layaChoiceAnswer.Probabilities![0], layaChoiceAnswer_Copy.Probabilities![0]);

            Classes.LayaChoiceAnswer layaChoiceAnswer_Empty = new(null, null, null, null, null);
            Core.xUnit.Query.SerializationCheck(layaChoiceAnswer_Empty);

            Classes.LayaChoiceAnswer layaChoiceAnswer_EmptyCopy = new(layaChoiceAnswer_Empty);
            Assert.Null(layaChoiceAnswer_EmptyCopy.Name);
            Assert.Null(layaChoiceAnswer_EmptyCopy.Confidence);
            Assert.Null(layaChoiceAnswer_EmptyCopy.AnswerConfidence);
            Assert.Null(layaChoiceAnswer_EmptyCopy.Label);
            Assert.Null(layaChoiceAnswer_EmptyCopy.Probabilities);
        }
    }
}
