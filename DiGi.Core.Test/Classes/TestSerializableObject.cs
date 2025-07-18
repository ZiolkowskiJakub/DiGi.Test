using DiGi.Core.Classes;
using DiGi.Core.Test.Enums;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.Test.Classes
{
    public class TestSerializableObject : SerializableObject
    {
        [JsonInclude, JsonPropertyName("TestEnum")]
        public TestEnum? TestEnum { get; set; } = null;

        [JsonInclude, JsonPropertyName("Value")]
        public double? Value { get; set; } = null;

        public TestSerializableObject()
            : base()
        {
        }

        public TestSerializableObject(JsonObject jsonObject)
            :base(jsonObject)
        {

        }
    }
}
