using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaScoreAnswer"/> keeps its name, both confidences, expected level and distribution, reports <see cref="Enums.LayaQuestionType.Score"/>, survives a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies every member into a new distribution list, for a populated answer and for one with every member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaScoreAnswer()
        {
            Classes.LayaScoreAnswer layaScoreAnswer = new("urgency", 0.4127, 0.6021, 1.3874, [0.0105, 0.5916, 0.3979]);

            static void AssertPopulated(Classes.LayaScoreAnswer? layaScoreAnswer_Actual)
            {
                Assert.NotNull(layaScoreAnswer_Actual);
                Assert.Equal("urgency", layaScoreAnswer_Actual!.Name);
                Assert.Equal(0.4127, layaScoreAnswer_Actual.Confidence);
                Assert.Equal(0.6021, layaScoreAnswer_Actual.AnswerConfidence);
                Assert.Equal(1.3874, layaScoreAnswer_Actual.Level);
                Assert.Equal(Enums.LayaQuestionType.Score, layaScoreAnswer_Actual.QuestionType);
                Assert.Equal([0.0105, 0.5916, 0.3979], layaScoreAnswer_Actual.Distribution!);
            }

            AssertPopulated(layaScoreAnswer);

            string? json = Core.Convert.ToSystem_String(layaScoreAnswer);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaScoreAnswer>? layaScoreAnswers = Core.Convert.ToDiGi<Classes.LayaScoreAnswer>(json);
            AssertPopulated(layaScoreAnswers?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaScoreAnswer);

            Classes.LayaScoreAnswer layaScoreAnswer_Copy = new(layaScoreAnswer);
            AssertPopulated(layaScoreAnswer_Copy);
            Assert.NotSame(layaScoreAnswer.Distribution, layaScoreAnswer_Copy.Distribution);

            Classes.LayaScoreAnswer layaScoreAnswer_Empty = new(null, null, null, null, null);
            Core.xUnit.Query.SerializationCheck(layaScoreAnswer_Empty);

            Classes.LayaScoreAnswer layaScoreAnswer_EmptyCopy = new(layaScoreAnswer_Empty);
            Assert.Null(layaScoreAnswer_EmptyCopy.Name);
            Assert.Null(layaScoreAnswer_EmptyCopy.Confidence);
            Assert.Null(layaScoreAnswer_EmptyCopy.AnswerConfidence);
            Assert.Null(layaScoreAnswer_EmptyCopy.Level);
            Assert.Null(layaScoreAnswer_EmptyCopy.Distribution);
        }
    }
}
