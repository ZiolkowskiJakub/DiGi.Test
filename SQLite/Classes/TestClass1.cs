using DiGi.Core.Classes;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.SQLite.Test.Classes
{
    public class TestClass1 : SerializableObject
    {
        [JsonInclude, JsonPropertyName("Parameter1")]
        public string Parameter1 { get; set; }

        public TestClass1()
        {

        }

        public TestClass1(JsonObject jsonObject)
            : base(jsonObject)
        {

        }
    }
}
