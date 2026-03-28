using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.Core.xUnit
{
    public class TimeSpanObject : SerializableObject, ISerializableObject
    {
        [JsonInclude, JsonPropertyName("TimeSpan")]
        public TimeSpan TimeSpan { get; set; }

        public TimeSpanObject()
        {
        }

        public TimeSpanObject(TimeSpanObject timeSpanObject)
            : base(timeSpanObject)
        {
            if (timeSpanObject is not null)
            {
                TimeSpan = timeSpanObject.TimeSpan;
            }
        }

        public TimeSpanObject(JsonObject jsonObject)
            : base(jsonObject)
        {
        }
    }
}