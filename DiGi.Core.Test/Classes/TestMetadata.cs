using DiGi.Core.Classes;
using DiGi.Core.IO.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.Test.Classes
{
    /// <summary>
    /// Represents the metadata associated with a test, providing information such as the test name and ensuring serializability through the <see cref="SerializableObject"/> base class and <see cref="IMetadata"/> interface.
    /// </summary>
    public class TestMetadata : SerializableObject, IMetadata
    {
        [JsonInclude, JsonPropertyName("Name")]
        private string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestMetadata"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name of the test metadata.</param>
        public TestMetadata(string name)
            : base()
        {
            this.name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestMetadata"/> class using the provided JSON object.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the metadata for the test.</param>
        public TestMetadata(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the name of the test metadata.
        /// </summary>
        [JsonIgnore]
        public string Name
        {
            get
            {
                return name;
            }
        }
    }
}
