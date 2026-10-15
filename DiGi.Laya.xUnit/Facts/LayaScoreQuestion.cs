using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaScoreQuestion"/> keeps its name, instructions and ordered levels, reports <see cref="Enums.LayaQuestionType.Score"/>, survives a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies every member into a new level list, for a populated question and for one with every member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaScoreQuestion()
        {
            Classes.LayaScoreQuestion layaScoreQuestion = new("urgency", "How urgent is this request?", ["low", "mid", "high"]);

            static void AssertPopulated(Classes.LayaScoreQuestion? layaScoreQuestion_Actual)
            {
                Assert.NotNull(layaScoreQuestion_Actual);
                Assert.Equal("urgency", layaScoreQuestion_Actual!.Name);
                Assert.Equal("How urgent is this request?", layaScoreQuestion_Actual.Instructions);
                Assert.Equal(Enums.LayaQuestionType.Score, layaScoreQuestion_Actual.QuestionType);
                Assert.Equal(["low", "mid", "high"], layaScoreQuestion_Actual.Levels!);
            }

            AssertPopulated(layaScoreQuestion);

            string? json = Core.Convert.ToSystem_String(layaScoreQuestion);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaScoreQuestion>? layaScoreQuestions = Core.Convert.ToDiGi<Classes.LayaScoreQuestion>(json);
            AssertPopulated(layaScoreQuestions?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaScoreQuestion);

            Classes.LayaScoreQuestion layaScoreQuestion_Copy = new(layaScoreQuestion);
            AssertPopulated(layaScoreQuestion_Copy);
            Assert.NotSame(layaScoreQuestion.Levels, layaScoreQuestion_Copy.Levels);

            Classes.LayaScoreQuestion layaScoreQuestion_Empty = new(null, null, null);
            Core.xUnit.Query.SerializationCheck(layaScoreQuestion_Empty);

            Classes.LayaScoreQuestion layaScoreQuestion_EmptyCopy = new(layaScoreQuestion_Empty);
            Assert.Null(layaScoreQuestion_EmptyCopy.Name);
            Assert.Null(layaScoreQuestion_EmptyCopy.Instructions);
            Assert.Null(layaScoreQuestion_EmptyCopy.Levels);
        }
    }
}
