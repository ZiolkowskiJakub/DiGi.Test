using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaLabel"/> keeps every member through a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies every member, for a populated instance and for one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaLabel()
        {
            Classes.LayaLabel layaLabel = new("soup", "a soup or broth", "The recipe is {label}.");

            static void AssertPopulated(Classes.LayaLabel? layaLabel_Actual)
            {
                Assert.NotNull(layaLabel_Actual);
                Assert.Equal("soup", layaLabel_Actual!.Id);
                Assert.Equal("a soup or broth", layaLabel_Actual.Text);
                Assert.Equal("The recipe is {label}.", layaLabel_Actual.Instructions);
            }

            AssertPopulated(layaLabel);

            string? json = Core.Convert.ToSystem_String(layaLabel);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaLabel>? layaLabels = Core.Convert.ToDiGi<Classes.LayaLabel>(json);
            AssertPopulated(layaLabels?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaLabel);

            Classes.LayaLabel layaLabel_Copy = new(layaLabel);
            AssertPopulated(layaLabel_Copy);

            Classes.LayaLabel layaLabel_Empty = new(null, null, null);
            Core.xUnit.Query.SerializationCheck(layaLabel_Empty);

            Classes.LayaLabel layaLabel_EmptyCopy = new(layaLabel_Empty);
            Assert.Null(layaLabel_EmptyCopy.Id);
            Assert.Null(layaLabel_EmptyCopy.Text);
            Assert.Null(layaLabel_EmptyCopy.Instructions);
        }
    }
}
