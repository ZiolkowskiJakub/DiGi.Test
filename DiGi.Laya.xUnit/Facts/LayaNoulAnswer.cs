using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaNoulAnswer"/> keeps its name, both confidences and probability, reports <see cref="Enums.LayaQuestionType.Noul"/>, survives a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies every member, for a populated answer and for one with every member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaNoulAnswer()
        {
            Classes.LayaNoulAnswer layaNoulAnswer = new("refund", 0.8312, 0.8312, 0.1688);

            static void AssertPopulated(Classes.LayaNoulAnswer? layaNoulAnswer_Actual)
            {
                Assert.NotNull(layaNoulAnswer_Actual);
                Assert.Equal("refund", layaNoulAnswer_Actual!.Name);
                Assert.Equal(0.8312, layaNoulAnswer_Actual.Confidence);
                Assert.Equal(0.8312, layaNoulAnswer_Actual.AnswerConfidence);
                Assert.Equal(0.1688, layaNoulAnswer_Actual.Probability);
                Assert.Equal(Enums.LayaQuestionType.Noul, layaNoulAnswer_Actual.QuestionType);
            }

            AssertPopulated(layaNoulAnswer);

            string? json = Core.Convert.ToSystem_String(layaNoulAnswer);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaNoulAnswer>? layaNoulAnswers = Core.Convert.ToDiGi<Classes.LayaNoulAnswer>(json);
            AssertPopulated(layaNoulAnswers?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaNoulAnswer);

            AssertPopulated(new Classes.LayaNoulAnswer(layaNoulAnswer));

            Classes.LayaNoulAnswer layaNoulAnswer_Empty = new(null, null, null, null);
            Core.xUnit.Query.SerializationCheck(layaNoulAnswer_Empty);

            Classes.LayaNoulAnswer layaNoulAnswer_EmptyCopy = new(layaNoulAnswer_Empty);
            Assert.Null(layaNoulAnswer_EmptyCopy.Name);
            Assert.Null(layaNoulAnswer_EmptyCopy.Confidence);
            Assert.Null(layaNoulAnswer_EmptyCopy.AnswerConfidence);
            Assert.Null(layaNoulAnswer_EmptyCopy.Probability);
        }
    }
}
