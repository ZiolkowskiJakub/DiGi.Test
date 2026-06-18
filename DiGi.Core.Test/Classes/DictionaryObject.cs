using DiGi.Core.Classes;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.Test.Classes
{
    /// <summary>Represents an object that maintains a collection of <see cref="TestObject"/> instances associated with unique string keys.</summary>
    public class DictionaryObject : GuidObject
    {
        [JsonInclude, JsonPropertyName("Dictionary")]
        private Dictionary<string, TestObject> dictionary;

        /// <summary>Initializes a new instance of the <see cref="DictionaryObject"/> class.</summary>
        public DictionaryObject()
            : base()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DictionaryObject"/> class using the specified JSON object.</summary>
        /// <param name="jsonObject">The JSON object used to initialize the current instance.</param>
        public DictionaryObject(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>Gets the collection of string values stored in the internal dictionary.</summary>
        [JsonIgnore]
        public IEnumerable<string> Values
        {
            get
            {
                return dictionary.Keys.ToList();
            }
        }

        /// <summary>Adds a new object with the specified name to the internal dictionary.</summary>
        /// <param name="name">The name of the object to be added.</param>
        /// <returns>True if the object was successfully added; otherwise, false if the provided name is null.</returns>
        public bool Add(string name)
        {
            if (name == null)
            {
                return false;
            }

            if (dictionary == null)
            {
                dictionary = new Dictionary<string, TestObject>();
            }

            dictionary[name] = new TestObject(name);
            return true;
        }
    }
}
