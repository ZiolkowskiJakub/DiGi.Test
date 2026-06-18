using DiGi.Core.Classes;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.Test.Classes
{
    /// <summary>
    /// Represents an object that encapsulates a byte array and inherits from <see cref="GuidObject"/>.
    /// </summary>
    public class BytesObject : GuidObject
    {
        [JsonInclude, JsonPropertyName("Bytes")]
        private byte[] bytes;

        /// <summary>
        /// Initializes a new instance of the <see cref="BytesObject"/> class using the specified byte array.
        /// </summary>
        /// <param name="bytes">The byte array used to initialize the current instance.</param>
        public BytesObject(byte[] bytes)
            : base()
        {
            this.bytes = bytes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BytesObject"/> class using the specified JSON object.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> used to initialize the current instance.</param>
        public BytesObject(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the raw byte array representation of the object.
        /// </summary>
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
