using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaTrainingSample"/> keeps every member, survives a string round trip and <c>SerializationCheck</c> with its polymorphic question coming back as its own derived type, and that its copy constructor copies every member and clones the question, for a populated sample and for one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaTrainingSample()
        {
            Classes.LayaTrainingSample layaTrainingSample = new(
                "item-7:refund",
                "[{\"role\":\"user\",\"content\":\"Please return my money.\"}]",
                true,
                new Classes.LayaChoiceQuestion("department", "Which team should handle this?", [new Classes.LayaCriterion("billing", "invoices, refunds"), new Classes.LayaCriterion("technical", null)]),
                "billing",
                1.5,
                true,
                0.75,
                "customer-42");

            static void AssertPopulated(Classes.LayaTrainingSample? layaTrainingSample_Actual)
            {
                Assert.NotNull(layaTrainingSample_Actual);
                Assert.Equal("item-7:refund", layaTrainingSample_Actual!.Id);
                Assert.Equal("[{\"role\":\"user\",\"content\":\"Please return my money.\"}]", layaTrainingSample_Actual.State);
                Assert.True(layaTrainingSample_Actual.StateIsJson);
                Assert.Equal("billing", layaTrainingSample_Actual.Label);
                Assert.Equal(1.5, layaTrainingSample_Actual.Level);
                Assert.True(layaTrainingSample_Actual.Truth);
                Assert.Equal(0.75, layaTrainingSample_Actual.Probability);
                Assert.Equal("customer-42", layaTrainingSample_Actual.Group);

                Classes.LayaChoiceQuestion layaChoiceQuestion = Assert.IsType<Classes.LayaChoiceQuestion>(layaTrainingSample_Actual.Question);
                Assert.Equal("department", layaChoiceQuestion.Name);
                Assert.Equal(["billing", "technical"], layaChoiceQuestion.Criteria!.Select(x => x.Label));
            }

            AssertPopulated(layaTrainingSample);

            string? json = Core.Convert.ToSystem_String(layaTrainingSample);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaTrainingSample>? layaTrainingSamples = Core.Convert.ToDiGi<Classes.LayaTrainingSample>(json);
            AssertPopulated(layaTrainingSamples?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaTrainingSample);

            Classes.LayaTrainingSample layaTrainingSample_Copy = new(layaTrainingSample);
            AssertPopulated(layaTrainingSample_Copy);
            Assert.NotSame(layaTrainingSample.Question, layaTrainingSample_Copy.Question);

            Classes.LayaTrainingSample layaTrainingSample_Empty = new(null, null, false, null, null, null, null, null, null);
            Core.xUnit.Query.SerializationCheck(layaTrainingSample_Empty);

            Classes.LayaTrainingSample layaTrainingSample_EmptyCopy = new(layaTrainingSample_Empty);
            Assert.Null(layaTrainingSample_EmptyCopy.Id);
            Assert.Null(layaTrainingSample_EmptyCopy.State);
            Assert.False(layaTrainingSample_EmptyCopy.StateIsJson);
            Assert.Null(layaTrainingSample_EmptyCopy.Question);
            Assert.Null(layaTrainingSample_EmptyCopy.Label);
            Assert.Null(layaTrainingSample_EmptyCopy.Level);
            Assert.Null(layaTrainingSample_EmptyCopy.Truth);
            Assert.Null(layaTrainingSample_EmptyCopy.Probability);
            Assert.Null(layaTrainingSample_EmptyCopy.Group);
        }
    }
}
