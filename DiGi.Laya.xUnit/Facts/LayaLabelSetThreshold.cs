using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaLabelSetThreshold"/> keeps every member through a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies every member, for a populated instance and for one with every optional member <c>null</c>.
        /// <para>Also verifies that a document written before <see cref="Classes.LayaLabelSetThreshold.LabelId"/> existed still reads, as a rule of the whole set.</para>
        /// </summary>
        [Fact]
        public void LayaLabelSetThreshold()
        {
            Classes.LayaLabelSetThreshold layaLabelSetThreshold = new("tags", "vegan", 0.65, 3);

            static void AssertPopulated(Classes.LayaLabelSetThreshold? layaLabelSetThreshold_Actual)
            {
                Assert.NotNull(layaLabelSetThreshold_Actual);
                Assert.Equal("tags", layaLabelSetThreshold_Actual!.LabelSetId);
                Assert.Equal("vegan", layaLabelSetThreshold_Actual.LabelId);
                Assert.Equal(0.65, layaLabelSetThreshold_Actual.Threshold);
                Assert.Equal(3, layaLabelSetThreshold_Actual.TopCount);
            }

            AssertPopulated(layaLabelSetThreshold);

            string? json = Core.Convert.ToSystem_String(layaLabelSetThreshold);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaLabelSetThreshold>? layaLabelSetThresholds = Core.Convert.ToDiGi<Classes.LayaLabelSetThreshold>(json);
            AssertPopulated(layaLabelSetThresholds?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaLabelSetThreshold);

            Classes.LayaLabelSetThreshold layaLabelSetThreshold_Copy = new(layaLabelSetThreshold);
            AssertPopulated(layaLabelSetThreshold_Copy);

            Classes.LayaLabelSetThreshold layaLabelSetThreshold_Empty = new(null, null, null, null);
            Core.xUnit.Query.SerializationCheck(layaLabelSetThreshold_Empty);

            Classes.LayaLabelSetThreshold layaLabelSetThreshold_EmptyCopy = new(layaLabelSetThreshold_Empty);
            Assert.Null(layaLabelSetThreshold_EmptyCopy.LabelSetId);
            Assert.Null(layaLabelSetThreshold_EmptyCopy.LabelId);
            Assert.Null(layaLabelSetThreshold_EmptyCopy.Threshold);
            Assert.Null(layaLabelSetThreshold_EmptyCopy.TopCount);

            //A document without LabelId is a rule of the whole set
            Classes.LayaLabelSetThreshold? layaLabelSetThreshold_Legacy = Core.Convert.ToDiGi<Classes.LayaLabelSetThreshold>(json!.Replace("\"LabelId\":\"vegan\",", string.Empty))?.FirstOrDefault();
            Assert.NotNull(layaLabelSetThreshold_Legacy);
            Assert.Equal("tags", layaLabelSetThreshold_Legacy!.LabelSetId);
            Assert.Null(layaLabelSetThreshold_Legacy.LabelId);
            Assert.Equal(0.65, layaLabelSetThreshold_Legacy.Threshold);
        }
    }
}
