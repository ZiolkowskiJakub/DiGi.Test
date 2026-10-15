using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaProbability"/> keeps its label and value, survives a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies both members, for a populated probability and for one with a <c>null</c> label.
        /// </summary>
        [Fact]
        public void LayaProbability()
        {
            Classes.LayaProbability layaProbability = new("billing", 0.8731);

            static void AssertPopulated(Classes.LayaProbability? layaProbability_Actual)
            {
                Assert.NotNull(layaProbability_Actual);
                Assert.Equal("billing", layaProbability_Actual!.Label);
                Assert.Equal(0.8731, layaProbability_Actual.Value);
            }

            AssertPopulated(layaProbability);

            string? json = Core.Convert.ToSystem_String(layaProbability);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaProbability>? layaProbabilities = Core.Convert.ToDiGi<Classes.LayaProbability>(json);
            AssertPopulated(layaProbabilities?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaProbability);

            AssertPopulated(new Classes.LayaProbability(layaProbability));

            Classes.LayaProbability layaProbability_Empty = new(null, 0.0);
            Core.xUnit.Query.SerializationCheck(layaProbability_Empty);

            Classes.LayaProbability layaProbability_EmptyCopy = new(layaProbability_Empty);
            Assert.Null(layaProbability_EmptyCopy.Label);
            Assert.Equal(0.0, layaProbability_EmptyCopy.Value);
        }
    }
}
