using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaItem"/> keeps every member through a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies every member, for a populated instance and for one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaItem()
        {
            Classes.LayaItem layaItem = new("recipe-17", "{\"title\":\"Żurek\",\"ingredients\":[\"żur\",\"kiełbasa\"]}", true);

            static void AssertPopulated(Classes.LayaItem? layaItem_Actual)
            {
                Assert.NotNull(layaItem_Actual);
                Assert.Equal("recipe-17", layaItem_Actual!.Id);
                Assert.Equal("{\"title\":\"Żurek\",\"ingredients\":[\"żur\",\"kiełbasa\"]}", layaItem_Actual.State);
                Assert.True(layaItem_Actual.StateIsJson);
            }

            AssertPopulated(layaItem);

            string? json = Core.Convert.ToSystem_String(layaItem);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaItem>? layaItems = Core.Convert.ToDiGi<Classes.LayaItem>(json);
            AssertPopulated(layaItems?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaItem);

            Classes.LayaItem layaItem_Copy = new(layaItem);
            AssertPopulated(layaItem_Copy);

            Classes.LayaItem layaItem_Empty = new(null, null, false);
            Core.xUnit.Query.SerializationCheck(layaItem_Empty);

            Classes.LayaItem layaItem_EmptyCopy = new(layaItem_Empty);
            Assert.Null(layaItem_EmptyCopy.Id);
            Assert.Null(layaItem_EmptyCopy.State);
            Assert.False(layaItem_EmptyCopy.StateIsJson);
        }
    }
}
