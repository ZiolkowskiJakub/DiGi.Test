using DiGi.Core.Classes;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.SQLite.Test.Classes
{
    /// <summary>
    /// Represents a test class that inherits from <see cref="GuidObject"/> and maintains relationships with <see cref="TestClass2"/> and <see cref="TestClass1"/>.
    /// </summary>
    public class TestClass3 : GuidObject
    {
        /// <summary>
        /// Gets or sets the <see cref="TestClass2"/> instance that serves as the parent of this object.
        /// </summary>
        [JsonInclude, JsonPropertyName("Parent")]
        public TestClass2 Parent { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="TestClass1"/> instances.
        /// </summary>
        [JsonInclude, JsonPropertyName("TestClasses")]
        public List<TestClass1> TestClasses { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestClass3"/> class.
        /// </summary>
        public TestClass3()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestClass3"/> class using the specified unique identifier.
        /// </summary>
        /// <param name="guid">The <see cref="Guid"/> used to initialize the instance.</param>
        public TestClass3(Guid guid)
            : base(guid)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestClass3"/> class using the specified JSON object.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> used to initialize the instance.</param>
        public TestClass3(JsonObject jsonObject)
            : base(jsonObject)
        {
        }
    }
}
