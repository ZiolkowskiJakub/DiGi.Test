using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaLabelSet"/> keeps every member through a string round trip and <c>SerializationCheck</c> with the nested label list, and that its copy constructor copies every member, for a populated instance and for one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaLabelSet()
        {
            Classes.LayaLabelSet layaLabelSet = new(
                "category",
                "Which course is this recipe?",
                Enums.LayaLabelSetKind.SingleChoice,
                [new Classes.LayaLabel("soup", "a soup or broth", null), new Classes.LayaLabel("dessert", "a sweet course", "The recipe is {label}.")]);

            static void AssertPopulated(Classes.LayaLabelSet? layaLabelSet_Actual)
            {
                Assert.NotNull(layaLabelSet_Actual);
                Assert.Equal("category", layaLabelSet_Actual!.Id);
                Assert.Equal("Which course is this recipe?", layaLabelSet_Actual.Instructions);
                Assert.Equal(Enums.LayaLabelSetKind.SingleChoice, layaLabelSet_Actual.Kind);
                Assert.Equal(["soup", "dessert"], layaLabelSet_Actual.Labels!.Select(x => x.Id));
                Assert.Equal("The recipe is {label}.", layaLabelSet_Actual.Labels![1].Instructions);
            }

            AssertPopulated(layaLabelSet);

            string? json = Core.Convert.ToSystem_String(layaLabelSet);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaLabelSet>? layaLabelSets = Core.Convert.ToDiGi<Classes.LayaLabelSet>(json);
            AssertPopulated(layaLabelSets?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaLabelSet);

            Classes.LayaLabelSet layaLabelSet_Copy = new(layaLabelSet);
            AssertPopulated(layaLabelSet_Copy);
            Assert.NotSame(layaLabelSet.Labels, layaLabelSet_Copy.Labels);
            Assert.NotSame(layaLabelSet.Labels![0], layaLabelSet_Copy.Labels![0]);

            Classes.LayaLabelSet layaLabelSet_Empty = new(null, null, Enums.LayaLabelSetKind.MultiLabel, null);
            Core.xUnit.Query.SerializationCheck(layaLabelSet_Empty);

            Classes.LayaLabelSet layaLabelSet_EmptyCopy = new(layaLabelSet_Empty);
            Assert.Null(layaLabelSet_EmptyCopy.Id);
            Assert.Null(layaLabelSet_EmptyCopy.Instructions);
            Assert.Equal(Enums.LayaLabelSetKind.MultiLabel, layaLabelSet_EmptyCopy.Kind);
            Assert.Null(layaLabelSet_EmptyCopy.Labels);
        }
    }
}
