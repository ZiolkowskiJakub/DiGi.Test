using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaConfusionRow"/> keeps every member through a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies every member, for a populated instance and for one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaConfusionRow()
        {
            Classes.LayaConfusionRow layaConfusionRow = new("soup", "dessert", 4);

            static void AssertPopulated(Classes.LayaConfusionRow? layaConfusionRow_Actual)
            {
                Assert.NotNull(layaConfusionRow_Actual);
                Assert.Equal("soup", layaConfusionRow_Actual!.ActualLabelId);
                Assert.Equal("dessert", layaConfusionRow_Actual.PredictedLabelId);
                Assert.Equal(4, layaConfusionRow_Actual.Count);
            }

            AssertPopulated(layaConfusionRow);

            string? json = Core.Convert.ToSystem_String(layaConfusionRow);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaConfusionRow>? layaConfusionRows = Core.Convert.ToDiGi<Classes.LayaConfusionRow>(json);
            AssertPopulated(layaConfusionRows?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaConfusionRow);

            Classes.LayaConfusionRow layaConfusionRow_Copy = new(layaConfusionRow);
            AssertPopulated(layaConfusionRow_Copy);

            Classes.LayaConfusionRow layaConfusionRow_Empty = new(null, null, 0);
            Core.xUnit.Query.SerializationCheck(layaConfusionRow_Empty);

            Classes.LayaConfusionRow layaConfusionRow_EmptyCopy = new(layaConfusionRow_Empty);
            Assert.Null(layaConfusionRow_EmptyCopy.ActualLabelId);
            Assert.Null(layaConfusionRow_EmptyCopy.PredictedLabelId);
            Assert.Equal(0, layaConfusionRow_EmptyCopy.Count);
        }
    }
}
