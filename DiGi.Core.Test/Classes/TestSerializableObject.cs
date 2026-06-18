using DiGi.Core.Classes;
using DiGi.Core.Test.Enums;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.Test.Classes
{
    /// <summary>
    /// Represents a test object used to verify the serialization and deserialization functionality of classes inheriting from <see cref="SerializableObject"/>.
    /// </summary>
    public class TestSerializableObject : SerializableObject
    {
        /// <summary>
        /// Gets or sets the enumeration value of the test serializable object.
        /// </summary>
        [JsonInclude, JsonPropertyName("TestEnum")]
        public TestEnum? TestEnum { get; set; } = null;

        /// <summary>
        /// Gets or sets the numeric value of the test serializable object.
        /// </summary>
        [JsonInclude, JsonPropertyName("Value")]
        public double? Value { get; set; } = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestSerializableObject"/> class.
        /// </summary>
        public TestSerializableObject()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestSerializableObject"/> class using the specified JSON object.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> used to initialize the current instance.</param>
        public TestSerializableObject(JsonObject jsonObject)
            : base(jsonObject)
        {
        }
    }
}
