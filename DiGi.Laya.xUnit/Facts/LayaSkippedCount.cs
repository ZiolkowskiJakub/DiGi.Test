using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaSkippedCount"/> keeps every member through a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies every member, for a populated instance and for one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaSkippedCount()
        {
            Classes.LayaSkippedCount layaSkippedCount = new("options_collapsed", 3);

            static void AssertPopulated(Classes.LayaSkippedCount? layaSkippedCount_Actual)
            {
                Assert.NotNull(layaSkippedCount_Actual);
                Assert.Equal("options_collapsed", layaSkippedCount_Actual!.Reason);
                Assert.Equal(3, layaSkippedCount_Actual.Count);
            }

            AssertPopulated(layaSkippedCount);

            string? json = Core.Convert.ToSystem_String(layaSkippedCount);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaSkippedCount>? layaSkippedCounts = Core.Convert.ToDiGi<Classes.LayaSkippedCount>(json);
            AssertPopulated(layaSkippedCounts?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaSkippedCount);

            Classes.LayaSkippedCount layaSkippedCount_Copy = new(layaSkippedCount);
            AssertPopulated(layaSkippedCount_Copy);

            Classes.LayaSkippedCount layaSkippedCount_Empty = new(null, 0);
            Core.xUnit.Query.SerializationCheck(layaSkippedCount_Empty);

            Classes.LayaSkippedCount layaSkippedCount_EmptyCopy = new(layaSkippedCount_Empty);
            Assert.Null(layaSkippedCount_EmptyCopy.Reason);
            Assert.Equal(0, layaSkippedCount_EmptyCopy.Count);
        }
    }
}
