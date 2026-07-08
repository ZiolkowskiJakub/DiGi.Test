using DiGi.Core.Classes;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.xUnit
{
    public class ComplexObject : SerializableObject
    {
        [JsonInclude, JsonPropertyName("Complex")]
        private System.Numerics.Complex? complex;

        public ComplexObject()
        {
        }

        public ComplexObject(System.Numerics.Complex complex)
            : base()
        {
            this.complex = complex;
        }

        public ComplexObject(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        [JsonIgnore]
        public System.Numerics.Complex? Complex
        {
            get
            {
                return complex;
            }
        }
    }
}