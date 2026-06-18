using System.Text.Json.Nodes;

namespace DiGi.Core.xUnit
{
    public partial class Classes
    {
        /// <summary>
        /// Tests the functionality of the <see cref="Core.Classes.Size"/> class, verifying its property initialization and JSON serialization behavior.
        /// </summary>
        [Fact]
        public void Size()
        {
            Core.Classes.Size size = new (10, 20);

            Assert.Equal(10, size.Width);
            Assert.Equal(20, size.Height);

            string? jsonString = size.ToJsonObject()?.ToJsonString();
            Assert.NotNull(jsonString);

            Assert.Contains("Width", jsonString);
            Assert.Contains("Height", jsonString);

            JsonObject? jsonObject = JsonNode.Parse(jsonString) as JsonObject;
            Assert.NotNull(jsonObject);

            int count = jsonObject.Count;
            Assert.Equal(3, count);

            Query.SerializationCheck(size);
        }
    }
}
