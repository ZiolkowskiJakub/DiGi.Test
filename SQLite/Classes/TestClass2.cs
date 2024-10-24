using DiGi.Core.Classes;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.SQLite.Test.Classes
{
    public class TestClass2 : SerializableObject
    {
        [JsonInclude, JsonPropertyName("Parameter1")]
        public double Parameter1 { get; set; }

        [JsonInclude, JsonPropertyName("TestClass1")]
        public TestClass1 TestClass1 { get; set; }

        [JsonInclude, JsonPropertyName("Parent")]
        public TestClass2 Parent { get; set; }

        public TestClass2()
        {

        }

        public TestClass2(JsonObject jsonObject)
            : base(jsonObject)
        {

        }
    }
}
