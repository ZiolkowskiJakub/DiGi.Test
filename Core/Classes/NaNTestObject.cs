using DiGi.Core.Classes;
using DiGi.Core.Test.Enums;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.Test.Classes
{
    public class NaNTestObject : SerializableObject
    {
        [JsonInclude, JsonPropertyName("NaN")]
        private double NaN = double.NaN;

        [JsonInclude, JsonPropertyName("NegativeInfinity")]
        private double NegativeInfinity = double.NegativeInfinity;

        [JsonInclude, JsonPropertyName("PositiveInfinity")]
        private double PositiveInfinity = double.PositiveInfinity;

        public NaNTestObject()
            : base()
        {
        }
    }
}
