using DiGi.Core.Classes;
using DiGi.Core.Test.Enums;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.Test.Classes
{
    /// <summary>
    /// Represents a test object that inherits from <see cref="GuidObject"/>, containing various data types to verify serialization and data handling.
    /// </summary>
    public class TestObject : GuidObject
    {
        [JsonInclude, JsonPropertyName("Name")]
        private string name;

        [JsonInclude, JsonPropertyName("UniqueReference")]
        private UniqueReference uniqueReference;

        [JsonInclude, JsonPropertyName("Range")]
        private Range<double> range;

        [JsonInclude, JsonPropertyName("DateTime")]
        private DateTime dateTime = DateTime.MinValue;

        [JsonInclude, JsonPropertyName("Color")]
        private System.Drawing.Color color = System.Drawing.Color.Empty;

        [JsonInclude, JsonPropertyName("IndexedDoubles")]
        private IndexedObjects<double> indexedDoubles = null;

        [JsonInclude, JsonPropertyName("TestEnum")]
        private TestEnum? testEnum = null;

        [JsonInclude, JsonPropertyName("KeyValuePair")]
        private KeyValuePair<int, string> KeyValuePair = new KeyValuePair<int, string>(1, "AAA");

        /// <summary>
        /// A dictionary that maps integer keys to string values.
        /// </summary>
        [JsonInclude, JsonPropertyName("Dictionary")]
        public Dictionary<int, string> Dictionary = new Dictionary<int, string>();

        /// <summary>
        /// A sorted dictionary that maps integer keys to string values.
        /// </summary>
        [JsonInclude, JsonPropertyName("SortedDictionary")]
        public SortedDictionary<int, string> SortedDictionary = new SortedDictionary<int, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TestObject"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name of the test object.</param>
        public TestObject(string name)
            : base()
        {
            this.name = name;
            uniqueReference = new GuidReference(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestObject"/> class with the specified name and range values.
        /// </summary>
        /// <param name="name">The name of the test object.</param>
        /// <param name="min">The minimum value for the range.</param>
        /// <param name="max">The maximum value for the range.</param>
        public TestObject(string name, double min, double max)
            : base()
        {
            this.name = name;
            uniqueReference = new GuidReference(this);
            range = new Range<double>(min, max);
            dateTime = DateTime.Now;
            color = System.Drawing.Color.Blue;

            indexedDoubles = new IndexedObjects<double>();
            indexedDoubles.Add(0, 10);
            indexedDoubles.Add(1, 20);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestObject"/> class with the specified identifier and name.
        /// </summary>
        /// <param name="guid">The unique identifier for the test object.</param>
        /// <param name="name">The name of the test object.</param>
        public TestObject(Guid guid, string name)
            : base(guid)
        {
            this.name = name;
            uniqueReference = new GuidReference(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestObject"/> class using the provided JSON object.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the data used to initialize the test object.</param>
        public TestObject(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the name of the test object.
        /// </summary>
        [JsonIgnore]
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// Gets the unique reference associated with the test object.
        /// </summary>
        [JsonIgnore]
        public UniqueReference UniqueReference
        {
            get
            {
                return uniqueReference;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TestEnum"/> value for the test object.
        /// </summary>
        [JsonIgnore]
        public TestEnum? TestEnum
        {
            get
            {
                return testEnum;
            }

            set
            {
                testEnum = value;
            }
        }
    }
}
