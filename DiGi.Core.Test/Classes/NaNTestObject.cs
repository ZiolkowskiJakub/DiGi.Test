using DiGi.Core.Classes;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.Test.Classes
{
    /// <summary>
    /// Represents a test object used to verify the serialization and deserialization of special floating-point values such as Not-a-Number (NaN), positive infinity, and negative infinity.
    /// </summary>
    public class NaNTestObject : SerializableObject
    {
        [JsonInclude, JsonPropertyName("NaN")]
        private readonly double NaN = double.NaN;

        [JsonInclude, JsonPropertyName("NegativeInfinity")]
        private readonly double NegativeInfinity = double.NegativeInfinity;

        [JsonInclude, JsonPropertyName("PositiveInfinity")]
        private readonly double PositiveInfinity = double.PositiveInfinity;

        /// <summary>
        /// Initializes a new instance of the <see cref="NaNTestObject"/> class.
        /// </summary>
        public NaNTestObject()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NaNTestObject"/> class using the provided <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the data used to initialize the instance.</param>
        public NaNTestObject(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NaNTestObject"/> class using the values from an existing <see cref="NaNTestObject"/> instance.
        /// </summary>
        /// <param name="naNTestObject">The source <see cref="NaNTestObject"/> instance to copy data from.</param>
        public NaNTestObject(NaNTestObject naNTestObject)
            : base(naNTestObject)
        {
        }
    }
}
