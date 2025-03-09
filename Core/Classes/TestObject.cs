using DiGi.Core.Classes;
using DiGi.Core.Test.Enums;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.Test.Classes
{
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

        [JsonInclude, JsonPropertyName("Dictionary")]
        public Dictionary<int, string> Dictionary = new Dictionary<int, string>();

        [JsonInclude, JsonPropertyName("SortedDictionary")]
        public SortedDictionary<int, string> SortedDictionary = new SortedDictionary<int, string>();


        public TestObject(string name)
            : base()
        {
            this.name = name;
            uniqueReference = new GuidReference(this);
        }


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

        public TestObject(Guid guid, string name)
            : base(guid)
        {
            this.name = name;
            uniqueReference = new GuidReference(this);
        }

        public TestObject(JsonObject jsonObject)
            :base(jsonObject)
        {

        }

        [JsonIgnore]
        public string Name
        {
            get
            {
                return name;
            }
        }

        [JsonIgnore]
        public UniqueReference UniqueReference
        {
            get
            {
                return uniqueReference;
            }
        }

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
