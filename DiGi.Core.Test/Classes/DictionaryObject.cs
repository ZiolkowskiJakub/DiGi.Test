using DiGi.Core.Classes;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.Test.Classes
{
    public class DictionaryObject : GuidObject
    {
        [JsonInclude, JsonPropertyName("Dictionary")]
        private Dictionary<string, TestObject> dictionary;


        public DictionaryObject()
            : base()
        {

        }

        public DictionaryObject(JsonObject jsonObject)
            :base(jsonObject)
        {

        }

        public bool Add(string name)
        {
            if(name == null)
            {
                return false;
            }

            if(dictionary == null)
            {
                dictionary = new Dictionary<string, TestObject>();
            }

            dictionary[name] = new TestObject(name);
            return true;
        }

        [JsonIgnore]
        public IEnumerable<string> Values
        {
            get
            {
                return dictionary.Keys.ToList();
            }
        }
    }
}
