using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaCriterion"/> keeps its label and description, survives a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies both members, for a populated criterion and for one with both members <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaCriterion()
        {
            Classes.LayaCriterion layaCriterion = new("billing", "invoices, refunds and payment problems");

            static void AssertPopulated(Classes.LayaCriterion? layaCriterion_Actual)
            {
                Assert.NotNull(layaCriterion_Actual);
                Assert.Equal("billing", layaCriterion_Actual!.Label);
                Assert.Equal("invoices, refunds and payment problems", layaCriterion_Actual.Description);
            }

            AssertPopulated(layaCriterion);

            string? json = Core.Convert.ToSystem_String(layaCriterion);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaCriterion>? layaCriteria = Core.Convert.ToDiGi<Classes.LayaCriterion>(json);
            AssertPopulated(layaCriteria?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaCriterion);

            AssertPopulated(new Classes.LayaCriterion(layaCriterion));

            Classes.LayaCriterion layaCriterion_Empty = new(null, null);
            Core.xUnit.Query.SerializationCheck(layaCriterion_Empty);

            Classes.LayaCriterion layaCriterion_EmptyCopy = new(layaCriterion_Empty);
            Assert.Null(layaCriterion_EmptyCopy.Label);
            Assert.Null(layaCriterion_EmptyCopy.Description);
        }
    }
}
