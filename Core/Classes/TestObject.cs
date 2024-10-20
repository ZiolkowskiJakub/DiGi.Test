using DiGi.Core.Classes;
using DiGi.Core.Test.Enums;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.Test.Classes
{
    public class TestObject : UniqueObject
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


        public TestObject(string name)
            : base()
        {
            this.name = name;
            uniqueReference = new UniqueReference(this);
        }


        public TestObject(string name, double min, double max)
            : base()
        {
            this.name = name;
            uniqueReference = new UniqueReference(this);
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
            uniqueReference = new UniqueReference(this);
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
