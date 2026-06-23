using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.xUnit
{
    /// <summary>Represents a serializable object that wraps a <see cref="TimeSpan"/> value.</summary>
    public class TimeSpanObject : SerializableObject, ISerializableObject
    {
        /// <summary>Gets or sets the <see cref="TimeSpan"/> value.</summary>
        [JsonInclude, JsonPropertyName("TimeSpan")]
        public TimeSpan TimeSpan { get; set; }

        /// <summary>Initializes a new instance of the <see cref="TimeSpanObject"/> class.</summary>
        public TimeSpanObject()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="TimeSpanObject"/> class by copying the values from another <see cref="TimeSpanObject"/> instance.</summary>
        /// <param name="timeSpanObject">The <see cref="TimeSpanObject"/> instance to copy from.</param>
        public TimeSpanObject(TimeSpanObject timeSpanObject)
            : base(timeSpanObject)
        {
            if (timeSpanObject is not null)
            {
                TimeSpan = timeSpanObject.TimeSpan;
            }
        }

        /// <summary>Initializes a new instance of the <see cref="TimeSpanObject"/> class using the specified JSON object.</summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> used to initialize the current instance.</param>
        public TimeSpanObject(JsonObject jsonObject)
            : base(jsonObject)
        {
        }
    }
}