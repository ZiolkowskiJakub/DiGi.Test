using System;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaPredictionResult"/> keeps every member through a string round trip and <c>SerializationCheck</c>, nested responses with their polymorphic answers included, and that its copy constructor copies every member and clones the responses, for a completed run and for a refused one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaPredictionResult()
        {
            Classes.LayaPredictionResult layaPredictionResult = new(
                [
                    new Classes.LayaResponse("item-1", [new Classes.LayaNoulAnswer("refund", 0.9991, 0.9991, 0.0009)], 142, false),
                    new Classes.LayaResponse("item-2", [new Classes.LayaChoiceAnswer("department", 0.9987, 0.9999, "billing", [new Classes.LayaProbability("billing", 0.9999), new Classes.LayaProbability("technical", 0.0001)])], 50, true)
                ],
                "0d3f1e2a5b6c7d8e9f00112233445566778899aabbccddeeff00112233445566",
                "aa11bb22cc33dd44ee55ff6600778899aabbccddeeff00112233445566778899",
                TimeSpan.FromSeconds(19.912),
                0,
                false,
                false,
                false,
                ["The request 'item-2' was truncated to fit the token budget and was answered from part of its input."]);

            static void AssertPopulated(Classes.LayaPredictionResult? layaPredictionResult_Actual)
            {
                Assert.NotNull(layaPredictionResult_Actual);
                Assert.Equal(["item-1", "item-2"], layaPredictionResult_Actual!.Responses!.Select(x => x.RequestId));
                Assert.IsType<Classes.LayaNoulAnswer>(layaPredictionResult_Actual.Responses![0].Answers![0]);
                Assert.Equal("billing", Assert.IsType<Classes.LayaChoiceAnswer>(layaPredictionResult_Actual.Responses![1].Answers![0]).Label);
                Assert.True(layaPredictionResult_Actual.Responses![1].Truncated);
                Assert.Equal("0d3f1e2a5b6c7d8e9f00112233445566778899aabbccddeeff00112233445566", layaPredictionResult_Actual.ModelSHA256);
                Assert.Equal("aa11bb22cc33dd44ee55ff6600778899aabbccddeeff00112233445566778899", layaPredictionResult_Actual.CalibrationSHA256);
                Assert.Equal(TimeSpan.FromSeconds(19.912), layaPredictionResult_Actual.Duration);
                Assert.Equal(0, layaPredictionResult_Actual.ExitCode);
                Assert.False(layaPredictionResult_Actual.Stalled);
                Assert.False(layaPredictionResult_Actual.TimedOut);
                Assert.False(layaPredictionResult_Actual.Cancelled);
                Assert.Single(layaPredictionResult_Actual.Messages!);
            }

            AssertPopulated(layaPredictionResult);

            string? json = Core.Convert.ToSystem_String(layaPredictionResult);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaPredictionResult>? layaPredictionResults = Core.Convert.ToDiGi<Classes.LayaPredictionResult>(json);
            AssertPopulated(layaPredictionResults?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaPredictionResult);

            Classes.LayaPredictionResult layaPredictionResult_Copy = new(layaPredictionResult);
            AssertPopulated(layaPredictionResult_Copy);
            Assert.NotSame(layaPredictionResult.Responses![0], layaPredictionResult_Copy.Responses![0]);
            Assert.NotSame(layaPredictionResult.Messages, layaPredictionResult_Copy.Messages);

            //A run that ended on its time limit, with the flags set and no responses
            Classes.LayaPredictionResult layaPredictionResult_Ended = new(null, null, null, null, -1, true, true, true, null);
            Core.xUnit.Query.SerializationCheck(layaPredictionResult_Ended);

            Classes.LayaPredictionResult layaPredictionResult_EndedCopy = new(layaPredictionResult_Ended);
            Assert.Null(layaPredictionResult_EndedCopy.Responses);
            Assert.Null(layaPredictionResult_EndedCopy.ModelSHA256);
            Assert.Null(layaPredictionResult_EndedCopy.CalibrationSHA256);
            Assert.Null(layaPredictionResult_EndedCopy.Duration);
            Assert.Equal(-1, layaPredictionResult_EndedCopy.ExitCode);
            Assert.True(layaPredictionResult_EndedCopy.Stalled);
            Assert.True(layaPredictionResult_EndedCopy.TimedOut);
            Assert.True(layaPredictionResult_EndedCopy.Cancelled);
            Assert.Null(layaPredictionResult_EndedCopy.Messages);
        }
    }
}
