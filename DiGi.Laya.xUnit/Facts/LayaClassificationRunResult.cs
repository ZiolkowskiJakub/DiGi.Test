using System;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaClassificationRunResult"/> keeps every member through a string round trip and <c>SerializationCheck</c>, nested item results with their scores included, and that its copy constructor copies every member and clones the item results, for an answered run and for a refused one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaClassificationRunResult()
        {
            Classes.LayaClassificationRunResult layaClassificationRunResult = new(
                [
                    new Classes.LayaClassificationResult("recipe-17", [new Classes.LayaLabelScore("category", "soup", 0.91, true, false, null)], "0d3f1e2a5b6c7d8e9f00112233445566778899aabbccddeeff00112233445566", new DateTimeOffset(2026, 10, 15, 7, 30, 0, TimeSpan.FromHours(2)), []),
                    new Classes.LayaClassificationResult("recipe-18", [], "0d3f1e2a5b6c7d8e9f00112233445566778899aabbccddeeff00112233445566", new DateTimeOffset(2026, 10, 15, 7, 30, 0, TimeSpan.FromHours(2)), ["The request 'recipe-18' got no response."])
                ],
                "0d3f1e2a5b6c7d8e9f00112233445566778899aabbccddeeff00112233445566",
                "aa11bb22cc33dd44ee55ff6600778899aabbccddeeff00112233445566778899",
                TimeSpan.FromSeconds(7.25),
                0,
                false,
                false,
                false,
                ["The single-choice label set 'category' has 21 labels; Laya's accuracy drops sharply past 20 options in one question."]);

            static void AssertPopulated(Classes.LayaClassificationRunResult? layaClassificationRunResult_Actual)
            {
                Assert.NotNull(layaClassificationRunResult_Actual);
                Assert.Equal(["recipe-17", "recipe-18"], layaClassificationRunResult_Actual!.Results!.Select(x => x.ItemId));
                Assert.Equal("soup", Assert.Single(layaClassificationRunResult_Actual.Results![0].Scores!).LabelId);
                Assert.Single(layaClassificationRunResult_Actual.Results![1].Messages!);
                Assert.Equal("0d3f1e2a5b6c7d8e9f00112233445566778899aabbccddeeff00112233445566", layaClassificationRunResult_Actual.ModelSHA256);
                Assert.Equal("aa11bb22cc33dd44ee55ff6600778899aabbccddeeff00112233445566778899", layaClassificationRunResult_Actual.CalibrationSHA256);
                Assert.Equal(TimeSpan.FromSeconds(7.25), layaClassificationRunResult_Actual.Duration);
                Assert.Equal(0, layaClassificationRunResult_Actual.ExitCode);
                Assert.False(layaClassificationRunResult_Actual.Stalled);
                Assert.False(layaClassificationRunResult_Actual.TimedOut);
                Assert.False(layaClassificationRunResult_Actual.Cancelled);
                Assert.Contains("has 21 labels", Assert.Single(layaClassificationRunResult_Actual.Messages!));
            }

            AssertPopulated(layaClassificationRunResult);

            string? json = Core.Convert.ToSystem_String(layaClassificationRunResult);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaClassificationRunResult>? layaClassificationRunResults = Core.Convert.ToDiGi<Classes.LayaClassificationRunResult>(json);
            AssertPopulated(layaClassificationRunResults?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaClassificationRunResult);

            Classes.LayaClassificationRunResult layaClassificationRunResult_Copy = new(layaClassificationRunResult);
            AssertPopulated(layaClassificationRunResult_Copy);
            Assert.NotSame(layaClassificationRunResult.Results![0], layaClassificationRunResult_Copy.Results![0]);
            Assert.NotSame(layaClassificationRunResult.Messages, layaClassificationRunResult_Copy.Messages);

            //A run whose process was ended by every limit at once, with no optional member set
            Classes.LayaClassificationRunResult layaClassificationRunResult_Ended = new(null, null, null, null, -1, true, true, true, null);
            Core.xUnit.Query.SerializationCheck(layaClassificationRunResult_Ended);

            Classes.LayaClassificationRunResult layaClassificationRunResult_EndedCopy = new(layaClassificationRunResult_Ended);
            Assert.Null(layaClassificationRunResult_EndedCopy.Results);
            Assert.Null(layaClassificationRunResult_EndedCopy.ModelSHA256);
            Assert.Null(layaClassificationRunResult_EndedCopy.CalibrationSHA256);
            Assert.Null(layaClassificationRunResult_EndedCopy.Duration);
            Assert.Equal(-1, layaClassificationRunResult_EndedCopy.ExitCode);
            Assert.True(layaClassificationRunResult_EndedCopy.Stalled);
            Assert.True(layaClassificationRunResult_EndedCopy.TimedOut);
            Assert.True(layaClassificationRunResult_EndedCopy.Cancelled);
            Assert.Null(layaClassificationRunResult_EndedCopy.Messages);
        }
    }
}
