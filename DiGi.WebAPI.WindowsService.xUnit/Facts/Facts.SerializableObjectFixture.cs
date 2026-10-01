using DiGi.Core.Classes;
using DiGi.WebAPI.Classes;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.WebAPI.WindowsService.xUnit
{
    public partial class Facts
    {
        // A DiGi payload with the two kinds of nullable member the serializer writes as an explicit null: a nullable value
        // type (Count) and a member typed by another payload component (Health), which the schema references by $ref.
        private sealed class SerializableObjectFixture : SerializableObject
        {
            [JsonInclude, JsonPropertyName(nameof(Count))]
            private readonly int? count;

            [JsonInclude, JsonPropertyName(nameof(Health))]
            private readonly ServiceHealthInformation? health;

            public SerializableObjectFixture(int? count, ServiceHealthInformation? health)
                : base()
            {
                this.count = count;
                this.health = health;
            }

            public SerializableObjectFixture(SerializableObjectFixture? serializableObjectFixture)
                : base(serializableObjectFixture)
            {
                if (serializableObjectFixture is not null)
                {
                    count = serializableObjectFixture.count;
                    health = Core.Query.Clone(serializableObjectFixture.health);
                }
            }

            public SerializableObjectFixture(JsonObject? jsonObject)
                : base(jsonObject)
            {
            }

            [JsonIgnore]
            public int? Count
            {
                get
                {
                    return count;
                }
            }

            [JsonIgnore]
            public ServiceHealthInformation? Health
            {
                get
                {
                    return health;
                }
            }
        }
    }
}
