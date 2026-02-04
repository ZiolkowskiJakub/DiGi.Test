using DiGi.Core.Classes;
using DiGi.Core.xUnit.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.xUnit
{
    public class TestObject : GuidObject
    {
        [JsonInclude, JsonPropertyName("Name")]
        private string? name;

        [JsonInclude, JsonPropertyName("UniqueReference")]
        private UniqueReference? uniqueReference;

        [JsonInclude, JsonPropertyName("Range")]
        private Range<double>? range;

        [JsonInclude, JsonPropertyName("DateTime")]
        private DateTime dateTime = DateTime.MinValue;

        [JsonInclude, JsonPropertyName("Color")]
        private System.Drawing.Color color = System.Drawing.Color.Empty;

        [JsonInclude, JsonPropertyName("IndexedDoubles")]
        private IndexedObjects<double>? indexedDoubles = null;

        [JsonInclude, JsonPropertyName("TestEnum")]
        private TestEnum? testEnum = null;

        [JsonInclude, JsonPropertyName("KeyValuePair")]
        private KeyValuePair<int, string>? KeyValuePair = new KeyValuePair<int, string>(1, "AAA");

        [JsonInclude, JsonPropertyName("Dictionary")]
        public Dictionary<int, string>? Dictionary = [];

        [JsonInclude, JsonPropertyName("SortedDictionary")]
        public SortedDictionary<int, string>? SortedDictionary = [];

        public TestObject()
        {
            Random random = new();

            name = Guid.NewGuid().ToString();
            uniqueReference = new GuidReference(new TypeReference(typeof(TestObject)), Guid.NewGuid());
            range = new Range<double>(random.NextDouble(), random.NextDouble());
            dateTime = DateTime.Now;

            var colors = typeof(System.Drawing.Color)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.PropertyType == typeof(System.Drawing.Color))
                .Select(p => (System.Drawing.Color)p.GetValue(null)!)
                .ToArray();

            color = colors[random.Next(colors.Length)];

            indexedDoubles = new IndexedObjects<double>
            {
                { 0, random.NextDouble() },
                { 1, random.NextDouble() }
            };

            testEnum = (TestEnum)random.Next(Enum.GetValues(typeof(TestEnum)).Length);

            KeyValuePair = new KeyValuePair<int, string>(random.Next(), Guid.NewGuid().ToString());

            Dictionary[random.Next()] = Guid.NewGuid().ToString();

            SortedDictionary[random.Next()] = Guid.NewGuid().ToString();
        }

        public TestObject(string? name)
            : base()
        {
            this.name = name;
            uniqueReference = new GuidReference(this);
        }

        public TestObject(string? name, double min, double max)
            : base()
        {
            this.name = name;
            uniqueReference = new GuidReference(this);
            range = new Range<double>(min, max);
            dateTime = DateTime.Now;
            color = System.Drawing.Color.Blue;

            indexedDoubles = new IndexedObjects<double>
            {
                { 0, 10 },
                { 1, 20 }
            };
        }

        public TestObject(Guid guid, string? name)
            : base(guid)
        {
            this.name = name;
            uniqueReference = new GuidReference(this);
        }

        public TestObject(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        [JsonIgnore]
        public string? Name
        {
            get
            {
                return name;
            }
        }

        [JsonIgnore]
        public UniqueReference? UniqueReference
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