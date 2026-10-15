using System;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaClassificationResult"/> keeps every member through a string round trip and <c>SerializationCheck</c> with the nested score list and a <c>DateTimeOffset</c> timestamp, and that its copy constructor copies every member, for a populated instance and for one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaClassificationResult()
        {
            Classes.LayaClassificationResult layaClassificationResult = new(
                "recipe-17",
                [new Classes.LayaLabelScore("category", "soup", 0.91, true, false, null), new Classes.LayaLabelScore("tags", "vegan", 0.8125, false, true, "contains kiełbasa")],
                "0d3f1e2a5b6c7d8e9f00112233445566778899aabbccddeeff00112233445566",
                new DateTimeOffset(2026, 10, 15, 7, 30, 0, TimeSpan.FromHours(2)),
                ["No answer for the label 'spicy' of the label set 'tags'."]);

            static void AssertPopulated(Classes.LayaClassificationResult? layaClassificationResult_Actual)
            {
                Assert.NotNull(layaClassificationResult_Actual);
                Assert.Equal("recipe-17", layaClassificationResult_Actual!.ItemId);
                Assert.Equal(["soup", "vegan"], layaClassificationResult_Actual.Scores!.Select(x => x.LabelId));
                Assert.True(layaClassificationResult_Actual.Scores![1].IsForbidden);
                Assert.Equal("0d3f1e2a5b6c7d8e9f00112233445566778899aabbccddeeff00112233445566", layaClassificationResult_Actual.ModelSHA256);
                Assert.Equal(new DateTimeOffset(2026, 10, 15, 7, 30, 0, TimeSpan.FromHours(2)), layaClassificationResult_Actual.CreatedAt);
                Assert.Single(layaClassificationResult_Actual.Messages!);
            }

            AssertPopulated(layaClassificationResult);

            string? json = Core.Convert.ToSystem_String(layaClassificationResult);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaClassificationResult>? layaClassificationResults = Core.Convert.ToDiGi<Classes.LayaClassificationResult>(json);
            AssertPopulated(layaClassificationResults?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaClassificationResult);

            Classes.LayaClassificationResult layaClassificationResult_Copy = new(layaClassificationResult);
            AssertPopulated(layaClassificationResult_Copy);
            Assert.NotSame(layaClassificationResult.Scores, layaClassificationResult_Copy.Scores);
            Assert.NotSame(layaClassificationResult.Scores![0], layaClassificationResult_Copy.Scores![0]);
            Assert.NotSame(layaClassificationResult.Messages, layaClassificationResult_Copy.Messages);

            Classes.LayaClassificationResult layaClassificationResult_Empty = new(null, null, null, null, null);
            Core.xUnit.Query.SerializationCheck(layaClassificationResult_Empty);

            Classes.LayaClassificationResult layaClassificationResult_EmptyCopy = new(layaClassificationResult_Empty);
            Assert.Null(layaClassificationResult_EmptyCopy.ItemId);
            Assert.Null(layaClassificationResult_EmptyCopy.Scores);
            Assert.Null(layaClassificationResult_EmptyCopy.ModelSHA256);
            Assert.Null(layaClassificationResult_EmptyCopy.CreatedAt);
            Assert.Null(layaClassificationResult_EmptyCopy.Messages);
        }
    }
}
