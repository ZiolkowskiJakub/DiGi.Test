using DiGi.Core.Classes;
using DiGi.Core.IO.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.Test.Classes
{
    public class TestMetadata : SerializableObject, IMetadata
    {
        [JsonInclude, JsonPropertyName("Name")]
        private string name;


        public TestMetadata(string name)
            : base()
        {
            this.name = name;
        }

        public TestMetadata(JsonObject jsonObject)
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
    }
}
