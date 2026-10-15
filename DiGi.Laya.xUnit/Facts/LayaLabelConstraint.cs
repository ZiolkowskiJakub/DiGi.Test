using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaLabelConstraint"/> keeps every member through a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies every member, for a populated instance and for one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaLabelConstraint()
        {
            Classes.LayaLabelConstraint layaLabelConstraint = new("recipe-17", "tags", "vegan", Enums.LayaConstraintEffect.Forbid, "contains kiełbasa");

            static void AssertPopulated(Classes.LayaLabelConstraint? layaLabelConstraint_Actual)
            {
                Assert.NotNull(layaLabelConstraint_Actual);
                Assert.Equal("recipe-17", layaLabelConstraint_Actual!.ItemId);
                Assert.Equal("tags", layaLabelConstraint_Actual.LabelSetId);
                Assert.Equal("vegan", layaLabelConstraint_Actual.LabelId);
                Assert.Equal(Enums.LayaConstraintEffect.Forbid, layaLabelConstraint_Actual.Effect);
                Assert.Equal("contains kiełbasa", layaLabelConstraint_Actual.Reason);
            }

            AssertPopulated(layaLabelConstraint);

            string? json = Core.Convert.ToSystem_String(layaLabelConstraint);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaLabelConstraint>? layaLabelConstraints = Core.Convert.ToDiGi<Classes.LayaLabelConstraint>(json);
            AssertPopulated(layaLabelConstraints?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaLabelConstraint);

            Classes.LayaLabelConstraint layaLabelConstraint_Copy = new(layaLabelConstraint);
            AssertPopulated(layaLabelConstraint_Copy);

            Classes.LayaLabelConstraint layaLabelConstraint_Empty = new(null, null, null, Enums.LayaConstraintEffect.Forbid, null);
            Core.xUnit.Query.SerializationCheck(layaLabelConstraint_Empty);

            Classes.LayaLabelConstraint layaLabelConstraint_EmptyCopy = new(layaLabelConstraint_Empty);
            Assert.Null(layaLabelConstraint_EmptyCopy.ItemId);
            Assert.Null(layaLabelConstraint_EmptyCopy.LabelSetId);
            Assert.Null(layaLabelConstraint_EmptyCopy.LabelId);
            Assert.Equal(Enums.LayaConstraintEffect.Forbid, layaLabelConstraint_EmptyCopy.Effect);
            Assert.Null(layaLabelConstraint_EmptyCopy.Reason);
        }
    }
}
