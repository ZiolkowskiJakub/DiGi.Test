using DiGi.Core.Classes;
using System.Text.Json.Serialization;

namespace DiGi.Core.xUnit
{
    public partial class Classes
    {
        /// <summary>
        /// Minimal serializable type mixing <see cref="JsonPropertyOrderAttribute"/>-ordered members with
        /// unordered members, used to exercise <see cref="Create.SerializationMethodCollection(System.Type)"/>'s
        /// member-ordering logic (ordered members sorted ascending, then placed before unordered members).
        /// </summary>
        public class TestOrderedSerializationObject : GuidObject
        {
            [JsonInclude, JsonPropertyName("Beta"), JsonPropertyOrder(2)]
            public string? Beta = "B";

            [JsonInclude, JsonPropertyName("Alpha"), JsonPropertyOrder(1)]
            public string? Alpha = "A";

            [JsonInclude, JsonPropertyName("Zulu")]
            public string? Zulu = "Z";

            [JsonInclude, JsonPropertyName("Yankee")]
            public string? Yankee = "Y";
        }
    }
}