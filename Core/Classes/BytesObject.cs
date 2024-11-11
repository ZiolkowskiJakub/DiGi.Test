using DiGi.Core.Classes;
using DiGi.Core.Test.Enums;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.Test.Classes
{
    public class BytesObject : UniqueObject
    {
        [JsonInclude, JsonPropertyName("Bytes")]
        private byte[] bytes;


        public BytesObject(byte[] bytes)
            : base()
        {
            this.bytes = bytes;
        }

        public BytesObject(JsonObject jsonObject)
            :base(jsonObject)
        {

        }

        [JsonIgnore]
        public byte[] Bytes
        {
            get
            {
                return bytes;
            }
        }
    }
}
