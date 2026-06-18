using DiGi.Core.Classes;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.SQLite.Test.Classes
{
    /// <summary>
    /// Represents a test class that inherits from <see cref="SerializableObject"/> to verify serialization and persistence functionality.
    /// </summary>
    public class TestClass1 : SerializableObject
    {
        /// <summary>
        /// Gets or sets the value of Parameter1.
        /// </summary>
        [JsonInclude, JsonPropertyName("Parameter1")]
        public string Parameter1 { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestClass1"/> class.
        /// </summary>
        public TestClass1()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestClass1"/> class using the specified JSON object.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> used to initialize the current instance.</param>
        public TestClass1(JsonObject jsonObject)
            : base(jsonObject)
        {
        }
    }
}
