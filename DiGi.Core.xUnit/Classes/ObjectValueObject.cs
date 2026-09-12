using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.xUnit
{
    /// <summary>
    /// A test <see cref="SerializableObject"/> whose single serializable member is declared as a bare
    /// <see cref="object"/>, so it can hold any CLR value. It exists to pin how a numeric <c>object</c> member
    /// survives the clone leg (<see cref="SerializableObject.Clone"/>, no text parse) versus the text leg
    /// (<c>Convert.ToSystem_String</c> then <c>Convert.ToDiGi</c>), which is the surface of
    /// ZiolkowskiJakub/DiGi.Core#6.
    /// </summary>
    public class ObjectValueObject : SerializableObject, ISerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(Value))]
        private object? value;

        /// <summary>
        /// Initializes a new, empty instance of the <see cref="ObjectValueObject"/> class.
        /// </summary>
        public ObjectValueObject()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectValueObject"/> class wrapping a value.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        public ObjectValueObject(object? value)
        {
            this.value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectValueObject"/> class by copying another instance.
        /// </summary>
        /// <param name="objectValueObject">The instance to copy from.</param>
        public ObjectValueObject(ObjectValueObject? objectValueObject)
            : base(objectValueObject)
        {
            if (objectValueObject != null)
            {
                value = objectValueObject.value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectValueObject"/> class from a JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object containing the serialized value.</param>
        public ObjectValueObject(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the wrapped value.
        /// </summary>
        [JsonIgnore]
        public object? Value
        {
            get
            {
                return value;
            }
        }
    }
}
