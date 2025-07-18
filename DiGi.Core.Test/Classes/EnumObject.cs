using DiGi.Core.Classes;
using DiGi.Core.Test.Enums;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.Test.Classes
{
    public class EnumObject : GuidObject
    {
        [JsonInclude, JsonPropertyName("TestEnum")]
        public TestEnum? TestEnum { get; set; } = null;

        [JsonInclude, JsonPropertyName("TestEnums_1")]
        public List<TestEnum?> TestEnums_1 { get; set; } = null;

        [JsonInclude, JsonPropertyName("TestEnums_2")]
        public HashSet<TestEnum?> TestEnums_2 { get; set; } = null;

        [JsonInclude, JsonPropertyName("TestEnums_3")]
        public HashSet<TestEnum>? TestEnums_3 { get; set; } = null;

        public EnumObject()
            : base()
        {
        }

        public EnumObject(JsonObject jsonObject)
            :base(jsonObject)
        {

        }
    }
}
