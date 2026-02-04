using System.Text.Json;
using System.Text.Json.Nodes;

namespace DiGi.Core.xUnit
{
    public partial class Tests
    {
        [Fact]
        public void UniqueId()
        {
            string json = JsonSerializer.Serialize(new Classes.UniqueIdObject());
            Assert.False(string.IsNullOrWhiteSpace(json));
            Assert.DoesNotContain("UniqueId", json);

            TestObject testObject = new("AAAA");

            JsonObject? jsonObject = testObject.ToJsonObject();

            Assert.NotNull(jsonObject);

            Assert.False(jsonObject.ContainsKey(nameof(testObject.UniqueId)));
        }
    }
}