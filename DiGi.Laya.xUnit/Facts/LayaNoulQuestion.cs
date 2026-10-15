using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaNoulQuestion"/> keeps its name and statement, reports <see cref="Enums.LayaQuestionType.Noul"/>, survives a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies both members, for a populated question and for one with both members <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaNoulQuestion()
        {
            Classes.LayaNoulQuestion layaNoulQuestion = new("urgent", "The request needs an answer today.");

            static void AssertPopulated(Classes.LayaNoulQuestion? layaNoulQuestion_Actual)
            {
                Assert.NotNull(layaNoulQuestion_Actual);
                Assert.Equal("urgent", layaNoulQuestion_Actual!.Name);
                Assert.Equal("The request needs an answer today.", layaNoulQuestion_Actual.Instructions);
                Assert.Equal(Enums.LayaQuestionType.Noul, layaNoulQuestion_Actual.QuestionType);
            }

            AssertPopulated(layaNoulQuestion);

            string? json = Core.Convert.ToSystem_String(layaNoulQuestion);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaNoulQuestion>? layaNoulQuestions = Core.Convert.ToDiGi<Classes.LayaNoulQuestion>(json);
            AssertPopulated(layaNoulQuestions?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaNoulQuestion);

            AssertPopulated(new Classes.LayaNoulQuestion(layaNoulQuestion));

            Classes.LayaNoulQuestion layaNoulQuestion_Empty = new(null, null);
            Core.xUnit.Query.SerializationCheck(layaNoulQuestion_Empty);

            Classes.LayaNoulQuestion layaNoulQuestion_EmptyCopy = new(layaNoulQuestion_Empty);
            Assert.Null(layaNoulQuestion_EmptyCopy.Name);
            Assert.Null(layaNoulQuestion_EmptyCopy.Instructions);
        }
    }
}
