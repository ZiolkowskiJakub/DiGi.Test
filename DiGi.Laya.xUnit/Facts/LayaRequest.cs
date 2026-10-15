using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaRequest"/> keeps every member, survives a string round trip and <c>SerializationCheck</c> with a polymorphic question list holding all three question types (each coming back as its own derived type), and that its copy constructor copies every member and clones each question as its own type, for a populated request and for one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaRequest()
        {
            Classes.LayaRequest layaRequest = new(
                "item-1",
                "{\"subject\":\"Double charge\",\"body\":\"We were billed twice for March.\"}",
                true,
                [
                    new Classes.LayaChoiceQuestion("department", "Which team should handle this?", [new Classes.LayaCriterion("billing", "invoices, refunds"), new Classes.LayaCriterion("technical", "bugs, outages")]),
                    new Classes.LayaScoreQuestion("urgency", "How urgent is this request?", ["low", "mid", "high"]),
                    new Classes.LayaNoulQuestion("refund", "The customer asks for a refund.")
                ],
                1024,
                256);

            static void AssertPopulated(Classes.LayaRequest? layaRequest_Actual)
            {
                Assert.NotNull(layaRequest_Actual);
                Assert.Equal("item-1", layaRequest_Actual!.Id);
                Assert.Equal("{\"subject\":\"Double charge\",\"body\":\"We were billed twice for March.\"}", layaRequest_Actual.State);
                Assert.True(layaRequest_Actual.StateIsJson);
                Assert.Equal(1024, layaRequest_Actual.MaxLength);
                Assert.Equal(256, layaRequest_Actual.HeadMaxLength);

                List<Classes.LayaQuestion> layaQuestions = layaRequest_Actual.Questions!;
                Assert.Equal(3, layaQuestions.Count);

                Classes.LayaChoiceQuestion layaChoiceQuestion = Assert.IsType<Classes.LayaChoiceQuestion>(layaQuestions[0]);
                Assert.Equal("department", layaChoiceQuestion.Name);
                Assert.Equal(["billing", "technical"], layaChoiceQuestion.Criteria!.Select(x => x.Label));

                Classes.LayaScoreQuestion layaScoreQuestion = Assert.IsType<Classes.LayaScoreQuestion>(layaQuestions[1]);
                Assert.Equal("urgency", layaScoreQuestion.Name);
                Assert.Equal(["low", "mid", "high"], layaScoreQuestion.Levels!);

                Classes.LayaNoulQuestion layaNoulQuestion = Assert.IsType<Classes.LayaNoulQuestion>(layaQuestions[2]);
                Assert.Equal("refund", layaNoulQuestion.Name);
                Assert.Equal("The customer asks for a refund.", layaNoulQuestion.Instructions);
            }

            AssertPopulated(layaRequest);

            string? json = Core.Convert.ToSystem_String(layaRequest);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaRequest>? layaRequests = Core.Convert.ToDiGi<Classes.LayaRequest>(json);
            AssertPopulated(layaRequests?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaRequest);

            Classes.LayaRequest layaRequest_Copy = new(layaRequest);
            AssertPopulated(layaRequest_Copy);
            Assert.NotSame(layaRequest.Questions, layaRequest_Copy.Questions);
            Assert.NotSame(layaRequest.Questions![0], layaRequest_Copy.Questions![0]);

            //A plain text state with the checkpoint's own token budgets
            Classes.LayaRequest layaRequest_Empty = new(null, null, false, null, null, null);
            Core.xUnit.Query.SerializationCheck(layaRequest_Empty);

            Classes.LayaRequest layaRequest_EmptyCopy = new(layaRequest_Empty);
            Assert.Null(layaRequest_EmptyCopy.Id);
            Assert.Null(layaRequest_EmptyCopy.State);
            Assert.False(layaRequest_EmptyCopy.StateIsJson);
            Assert.Null(layaRequest_EmptyCopy.Questions);
            Assert.Null(layaRequest_EmptyCopy.MaxLength);
            Assert.Null(layaRequest_EmptyCopy.HeadMaxLength);
        }
    }
}
