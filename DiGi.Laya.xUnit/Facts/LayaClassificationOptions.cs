using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaClassificationOptions"/> starts with no thresholds and no confidence floor, keeps both through a string round trip and <c>SerializationCheck</c> with the nested threshold list, and that its copy constructor copies the floor and clones the thresholds.
        /// </summary>
        [Fact]
        public void LayaClassificationOptions()
        {
            Classes.LayaClassificationOptions layaClassificationOptions_Default = new();
            Assert.Null(layaClassificationOptions_Default.Thresholds);
            Assert.Null(layaClassificationOptions_Default.MinimumConfidence);
            Core.xUnit.Query.SerializationCheck(layaClassificationOptions_Default);

            Classes.LayaClassificationOptions layaClassificationOptions = new()
            {
                MinimumConfidence = 0.8,
                Thresholds = [new Classes.LayaLabelSetThreshold("tags", 0.65, null), new Classes.LayaLabelSetThreshold("category", null, 3)]
            };

            static void AssertPopulated(Classes.LayaClassificationOptions? layaClassificationOptions_Actual)
            {
                Assert.NotNull(layaClassificationOptions_Actual);
                Assert.Equal(0.8, layaClassificationOptions_Actual!.MinimumConfidence);
                Assert.Equal(["tags", "category"], layaClassificationOptions_Actual.Thresholds!.Select(x => x.LabelSetId));
                Assert.Equal(0.65, layaClassificationOptions_Actual.Thresholds![0].Threshold);
                Assert.Equal(3, layaClassificationOptions_Actual.Thresholds![1].TopCount);
            }

            AssertPopulated(layaClassificationOptions);

            string? json = Core.Convert.ToSystem_String(layaClassificationOptions);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaClassificationOptions>? layaClassificationOptionsList = Core.Convert.ToDiGi<Classes.LayaClassificationOptions>(json);
            AssertPopulated(layaClassificationOptionsList?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaClassificationOptions);

            Classes.LayaClassificationOptions layaClassificationOptions_Copy = new(layaClassificationOptions);
            AssertPopulated(layaClassificationOptions_Copy);
            Assert.NotSame(layaClassificationOptions.Thresholds, layaClassificationOptions_Copy.Thresholds);
            Assert.NotSame(layaClassificationOptions.Thresholds![0], layaClassificationOptions_Copy.Thresholds![0]);
        }
    }
}
