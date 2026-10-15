using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaChoiceQuestion"/> keeps its name, instructions and ordered criteria, reports <see cref="Enums.LayaQuestionType.Choice"/>, survives a string round trip and <c>SerializationCheck</c> with the nested criterion list, and that its copy constructor copies every member and clones the criteria, for a populated question and for one with every member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaChoiceQuestion()
        {
            Classes.LayaChoiceQuestion layaChoiceQuestion = new(
                "department",
                "Which team should handle this?",
                [new Classes.LayaCriterion("billing", "invoices, refunds"), new Classes.LayaCriterion("technical", "bugs, outages"), new Classes.LayaCriterion("sales", null)]);

            static void AssertPopulated(Classes.LayaChoiceQuestion? layaChoiceQuestion_Actual)
            {
                Assert.NotNull(layaChoiceQuestion_Actual);
                Assert.Equal("department", layaChoiceQuestion_Actual!.Name);
                Assert.Equal("Which team should handle this?", layaChoiceQuestion_Actual.Instructions);
                Assert.Equal(Enums.LayaQuestionType.Choice, layaChoiceQuestion_Actual.QuestionType);
                Assert.Equal(["billing", "technical", "sales"], layaChoiceQuestion_Actual.Criteria!.Select(x => x.Label));
                Assert.Equal("invoices, refunds", layaChoiceQuestion_Actual.Criteria![0].Description);
                Assert.Null(layaChoiceQuestion_Actual.Criteria![2].Description);
            }

            AssertPopulated(layaChoiceQuestion);

            string? json = Core.Convert.ToSystem_String(layaChoiceQuestion);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaChoiceQuestion>? layaChoiceQuestions = Core.Convert.ToDiGi<Classes.LayaChoiceQuestion>(json);
            AssertPopulated(layaChoiceQuestions?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaChoiceQuestion);

            Classes.LayaChoiceQuestion layaChoiceQuestion_Copy = new(layaChoiceQuestion);
            AssertPopulated(layaChoiceQuestion_Copy);
            Assert.NotSame(layaChoiceQuestion.Criteria, layaChoiceQuestion_Copy.Criteria);
            Assert.NotSame(layaChoiceQuestion.Criteria![0], layaChoiceQuestion_Copy.Criteria![0]);

            Classes.LayaChoiceQuestion layaChoiceQuestion_Empty = new(null, null, null);
            Core.xUnit.Query.SerializationCheck(layaChoiceQuestion_Empty);

            Classes.LayaChoiceQuestion layaChoiceQuestion_EmptyCopy = new(layaChoiceQuestion_Empty);
            Assert.Null(layaChoiceQuestion_EmptyCopy.Name);
            Assert.Null(layaChoiceQuestion_EmptyCopy.Instructions);
            Assert.Null(layaChoiceQuestion_EmptyCopy.Criteria);
        }
    }
}
