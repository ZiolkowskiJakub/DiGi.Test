using DiGi.Core.Classes;
using DiGi.Core.Test.Enums;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.Test.Classes
{
    /// <summary>
    /// Represents an object used for testing the handling of enumeration values, inheriting from <see cref="GuidObject"/>.
    /// </summary>
    public class EnumObject : GuidObject
    {
        /// <summary>
        /// Gets or sets the nullable <see cref="TestEnum"/> value.
        /// </summary>
        [JsonInclude, JsonPropertyName("TestEnum")]
        public TestEnum? TestEnum { get; set; } = null;

        /// <summary>
        /// Gets or sets a list of nullable <see cref="TestEnum"/> values.
        /// </summary>
        [JsonInclude, JsonPropertyName("TestEnums_1")]
        public List<TestEnum?> TestEnums_1 { get; set; } = null;

        /// <summary>
        /// Gets or sets a set of nullable <see cref="TestEnum"/> values.
        /// </summary>
        [JsonInclude, JsonPropertyName("TestEnums_2")]
        public HashSet<TestEnum?> TestEnums_2 { get; set; } = null;

        /// <summary>
        /// Gets or sets a set of <see cref="TestEnum"/> values.
        /// </summary>
        [JsonInclude, JsonPropertyName("TestEnums_3")]
        public HashSet<TestEnum>? TestEnums_3 { get; set; } = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumObject"/> class.
        /// </summary>
        public EnumObject()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumObject"/> class using the specified <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> used to initialize the current instance.</param>
        public EnumObject(JsonObject jsonObject)
            : base(jsonObject)
        {
        }
    }
}
