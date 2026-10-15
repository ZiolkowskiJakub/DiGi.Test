using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaLabelAssignment"/> keeps every member through a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies every member, for a populated instance and for one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaLabelAssignment()
        {
            Classes.LayaLabelAssignment layaLabelAssignment = new("recipe-17", "tags", "vegetarian", false, 0.25, "family-3");

            static void AssertPopulated(Classes.LayaLabelAssignment? layaLabelAssignment_Actual)
            {
                Assert.NotNull(layaLabelAssignment_Actual);
                Assert.Equal("recipe-17", layaLabelAssignment_Actual!.ItemId);
                Assert.Equal("tags", layaLabelAssignment_Actual.LabelSetId);
                Assert.Equal("vegetarian", layaLabelAssignment_Actual.LabelId);
                Assert.False(layaLabelAssignment_Actual.Truth);
                Assert.Equal(0.25, layaLabelAssignment_Actual.Weight);
                Assert.Equal("family-3", layaLabelAssignment_Actual.Group);
            }

            AssertPopulated(layaLabelAssignment);

            string? json = Core.Convert.ToSystem_String(layaLabelAssignment);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaLabelAssignment>? layaLabelAssignments = Core.Convert.ToDiGi<Classes.LayaLabelAssignment>(json);
            AssertPopulated(layaLabelAssignments?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaLabelAssignment);

            Classes.LayaLabelAssignment layaLabelAssignment_Copy = new(layaLabelAssignment);
            AssertPopulated(layaLabelAssignment_Copy);

            Classes.LayaLabelAssignment layaLabelAssignment_Empty = new(null, null, null, true, 1.0, null);
            Core.xUnit.Query.SerializationCheck(layaLabelAssignment_Empty);

            Classes.LayaLabelAssignment layaLabelAssignment_EmptyCopy = new(layaLabelAssignment_Empty);
            Assert.Null(layaLabelAssignment_EmptyCopy.ItemId);
            Assert.Null(layaLabelAssignment_EmptyCopy.LabelSetId);
            Assert.Null(layaLabelAssignment_EmptyCopy.LabelId);
            Assert.True(layaLabelAssignment_EmptyCopy.Truth);
            Assert.Equal(1.0, layaLabelAssignment_EmptyCopy.Weight);
            Assert.Null(layaLabelAssignment_EmptyCopy.Group);
        }
    }
}
