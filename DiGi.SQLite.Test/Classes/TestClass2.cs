using DiGi.Core.Classes;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.SQLite.Test.Classes
{
    /// <summary>
    /// Represents a serializable object used for testing purposes that contains basic data types and references to other objects.
    /// </summary>
    public class TestClass2 : SerializableObject
    {
        /// <summary>
        /// Gets or sets the value of Parameter1.
        /// </summary>
        [JsonInclude, JsonPropertyName("Parameter1")]
        public double Parameter1 { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TestClass1"/> instance.
        /// </summary>
        [JsonInclude, JsonPropertyName("TestClass1")]
        public TestClass1 TestClass1 { get; set; }

        /// <summary>
        /// Gets or sets the parent instance of the <see cref="TestClass2"/> class.
        /// </summary>
        [JsonInclude, JsonPropertyName("Parent")]
        public TestClass2 Parent { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestClass2"/> class.
        /// </summary>
        public TestClass2()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestClass2"/> class using the specified <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> used to initialize the current instance.</param>
        public TestClass2(JsonObject jsonObject)
            : base(jsonObject)
        {
        }
    }
}
