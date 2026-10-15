using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaLabelScore"/> keeps every member through a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies every member, for a populated instance and for one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaLabelScore()
        {
            Classes.LayaLabelScore layaLabelScore = new("tags", "vegan", 0.8125, false, true, "contains kiełbasa");

            static void AssertPopulated(Classes.LayaLabelScore? layaLabelScore_Actual)
            {
                Assert.NotNull(layaLabelScore_Actual);
                Assert.Equal("tags", layaLabelScore_Actual!.LabelSetId);
                Assert.Equal("vegan", layaLabelScore_Actual.LabelId);
                Assert.Equal(0.8125, layaLabelScore_Actual.Probability);
                Assert.False(layaLabelScore_Actual.IsSuggested);
                Assert.True(layaLabelScore_Actual.IsForbidden);
                Assert.Equal("contains kiełbasa", layaLabelScore_Actual.Reason);
            }

            AssertPopulated(layaLabelScore);

            string? json = Core.Convert.ToSystem_String(layaLabelScore);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaLabelScore>? layaLabelScores = Core.Convert.ToDiGi<Classes.LayaLabelScore>(json);
            AssertPopulated(layaLabelScores?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaLabelScore);

            Classes.LayaLabelScore layaLabelScore_Copy = new(layaLabelScore);
            AssertPopulated(layaLabelScore_Copy);

            Classes.LayaLabelScore layaLabelScore_Empty = new(null, null, 0.0, true, false, null);
            Core.xUnit.Query.SerializationCheck(layaLabelScore_Empty);

            Classes.LayaLabelScore layaLabelScore_EmptyCopy = new(layaLabelScore_Empty);
            Assert.Null(layaLabelScore_EmptyCopy.LabelSetId);
            Assert.Null(layaLabelScore_EmptyCopy.LabelId);
            Assert.Equal(0.0, layaLabelScore_EmptyCopy.Probability);
            Assert.True(layaLabelScore_EmptyCopy.IsSuggested);
            Assert.False(layaLabelScore_EmptyCopy.IsForbidden);
            Assert.Null(layaLabelScore_EmptyCopy.Reason);
        }
    }
}
