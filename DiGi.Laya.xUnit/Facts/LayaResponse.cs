using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaResponse"/> keeps its request id, answers, token count and truncation flag, survives a string round trip and <c>SerializationCheck</c> with a polymorphic answer list holding all three answer types (each coming back as its own derived type), and that its copy constructor copies every member and clones each answer as its own type, for a populated response and for one with every member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaResponse()
        {
            Classes.LayaResponse layaResponse = new(
                "item-1",
                [
                    new Classes.LayaChoiceAnswer("department", 0.7412, 0.9135, "billing", [new Classes.LayaProbability("billing", 0.9135), new Classes.LayaProbability("technical", 0.0865)]),
                    new Classes.LayaScoreAnswer("urgency", 0.4127, 0.6021, 1.3874, [0.0105, 0.5916, 0.3979]),
                    new Classes.LayaNoulAnswer("refund", 0.8312, 0.8312, 0.1688)
                ],
                187,
                true);

            static void AssertPopulated(Classes.LayaResponse? layaResponse_Actual)
            {
                Assert.NotNull(layaResponse_Actual);
                Assert.Equal("item-1", layaResponse_Actual!.RequestId);
                Assert.Equal(187, layaResponse_Actual.InputTokenCount);
                Assert.True(layaResponse_Actual.Truncated);

                List<Classes.LayaAnswer> layaAnswers = layaResponse_Actual.Answers!;
                Assert.Equal(3, layaAnswers.Count);

                Classes.LayaChoiceAnswer layaChoiceAnswer = Assert.IsType<Classes.LayaChoiceAnswer>(layaAnswers[0]);
                Assert.Equal("billing", layaChoiceAnswer.Label);
                Assert.Equal([0.9135, 0.0865], layaChoiceAnswer.Probabilities!.Select(x => x.Value));

                Classes.LayaScoreAnswer layaScoreAnswer = Assert.IsType<Classes.LayaScoreAnswer>(layaAnswers[1]);
                Assert.Equal(1.3874, layaScoreAnswer.Level);
                Assert.Equal([0.0105, 0.5916, 0.3979], layaScoreAnswer.Distribution!);

                Classes.LayaNoulAnswer layaNoulAnswer = Assert.IsType<Classes.LayaNoulAnswer>(layaAnswers[2]);
                Assert.Equal("refund", layaNoulAnswer.Name);
                Assert.Equal(0.1688, layaNoulAnswer.Probability);
            }

            AssertPopulated(layaResponse);

            string? json = Core.Convert.ToSystem_String(layaResponse);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaResponse>? layaResponses = Core.Convert.ToDiGi<Classes.LayaResponse>(json);
            AssertPopulated(layaResponses?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaResponse);

            Classes.LayaResponse layaResponse_Copy = new(layaResponse);
            AssertPopulated(layaResponse_Copy);
            Assert.NotSame(layaResponse.Answers, layaResponse_Copy.Answers);
            Assert.NotSame(layaResponse.Answers![0], layaResponse_Copy.Answers![0]);

            Classes.LayaResponse layaResponse_Empty = new(null, null, null, null);
            Core.xUnit.Query.SerializationCheck(layaResponse_Empty);

            Classes.LayaResponse layaResponse_EmptyCopy = new(layaResponse_Empty);
            Assert.Null(layaResponse_EmptyCopy.RequestId);
            Assert.Null(layaResponse_EmptyCopy.Answers);
            Assert.Null(layaResponse_EmptyCopy.InputTokenCount);
            Assert.Null(layaResponse_EmptyCopy.Truncated);
        }
    }
}
