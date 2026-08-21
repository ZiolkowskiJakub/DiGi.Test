using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.xUnit
{
    /// <summary>
    /// Represents a test serializable object containing <see cref="DateTimeOffset"/> scalar, nullable, and array properties.
    /// </summary>
    public class DateTimeOffsetObject : SerializableObject, ISerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(NullableTimestamp))]
        private DateTimeOffset? nullableTimestamp;

        [JsonInclude, JsonPropertyName(nameof(Timestamp))]
        private DateTimeOffset timestamp;

        [JsonInclude, JsonPropertyName(nameof(Timestamps))]
        private DateTimeOffset[]? timestamps;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeOffsetObject"/> class.
        /// </summary>
        public DateTimeOffsetObject()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeOffsetObject"/> class with the specified timestamps.
        /// </summary>
        /// <param name="timestamp">The primary timestamp value.</param>
        /// <param name="nullableTimestamp">The optional nullable timestamp value.</param>
        /// <param name="timestamps">The array of timestamp values.</param>
        public DateTimeOffsetObject(DateTimeOffset timestamp, DateTimeOffset? nullableTimestamp = null, DateTimeOffset[]? timestamps = null)
        {
            this.timestamp = timestamp;
            this.nullableTimestamp = nullableTimestamp;
            this.timestamps = timestamps;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeOffsetObject"/> class by copying another instance.
        /// </summary>
        /// <param name="dateTimeOffsetObject">The instance to copy from.</param>
        public DateTimeOffsetObject(DateTimeOffsetObject? dateTimeOffsetObject)
            : base(dateTimeOffsetObject)
        {
            if (dateTimeOffsetObject != null)
            {
                timestamp = dateTimeOffsetObject.timestamp;
                nullableTimestamp = dateTimeOffsetObject.nullableTimestamp;
                timestamps = dateTimeOffsetObject.timestamps == null ? null : (DateTimeOffset[])dateTimeOffsetObject.timestamps.Clone();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeOffsetObject"/> class from a JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object containing serialized data.</param>
        public DateTimeOffsetObject(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the optional nullable timestamp value.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? NullableTimestamp
        {
            get
            {
                return nullableTimestamp;
            }
        }

        /// <summary>
        /// Gets the primary timestamp value.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset Timestamp
        {
            get
            {
                return timestamp;
            }
        }

        /// <summary>
        /// Gets the array of timestamp values.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset[]? Timestamps
        {
            get
            {
                return timestamps;
            }
        }
    }
}
