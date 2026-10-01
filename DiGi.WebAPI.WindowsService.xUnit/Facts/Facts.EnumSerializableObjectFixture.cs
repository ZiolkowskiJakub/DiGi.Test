using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.WebAPI.WindowsService.xUnit
{
    public partial class Facts
    {
        // A DiGi payload with one member per enum shape the DiGi serializer writes as integers: a nullable enum, a list and
        // a dictionary of enums, a [Flags] enum whose combined values no member list contains, and a long-backed enum.
        // Field-backed, so the member descriptions come from the public properties the fields are named after.
        private sealed class EnumSerializableObjectFixture : SerializableObject
        {
            [JsonInclude, JsonPropertyName(nameof(Level))]
            private readonly AdministrativeArealType? level;

            [JsonInclude, JsonPropertyName(nameof(Levels))]
            private readonly List<AdministrativeArealType>? levels;

            [JsonInclude, JsonPropertyName(nameof(LevelsByCode))]
            private readonly Dictionary<string, AdministrativeArealType>? levelsByCode;

            [JsonInclude, JsonPropertyName(nameof(Flags))]
            private readonly FlagsEnumFixture flags;

            [JsonInclude, JsonPropertyName(nameof(Long))]
            private readonly LongEnumFixture @long;

            public EnumSerializableObjectFixture(AdministrativeArealType? level, IEnumerable<AdministrativeArealType>? levels, IDictionary<string, AdministrativeArealType>? levelsByCode, FlagsEnumFixture flags, LongEnumFixture @long)
                : base()
            {
                this.level = level;
                this.levels = levels is null ? null : [.. levels];
                this.levelsByCode = levelsByCode is null ? null : new Dictionary<string, AdministrativeArealType>(levelsByCode);
                this.flags = flags;
                this.@long = @long;
            }

            public EnumSerializableObjectFixture(EnumSerializableObjectFixture? enumSerializableObjectFixture)
                : base(enumSerializableObjectFixture)
            {
                if (enumSerializableObjectFixture is not null)
                {
                    level = enumSerializableObjectFixture.level;
                    levels = enumSerializableObjectFixture.levels is null ? null : [.. enumSerializableObjectFixture.levels];
                    levelsByCode = enumSerializableObjectFixture.levelsByCode is null ? null : new Dictionary<string, AdministrativeArealType>(enumSerializableObjectFixture.levelsByCode);
                    flags = enumSerializableObjectFixture.flags;
                    @long = enumSerializableObjectFixture.@long;
                }
            }

            public EnumSerializableObjectFixture(JsonObject? jsonObject)
                : base(jsonObject)
            {
            }

            /// <summary>
            /// Gets the administrative level of the fixture, or <c>null</c> when it is not known.
            /// </summary>
            [JsonIgnore]
            public AdministrativeArealType? Level
            {
                get
                {
                    return level;
                }
            }

            /// <summary>
            /// Gets the administrative levels the fixture spans.
            /// </summary>
            [JsonIgnore]
            public List<AdministrativeArealType>? Levels
            {
                get
                {
                    return levels;
                }
            }

            /// <summary>
            /// Gets the administrative level of each unit code.
            /// </summary>
            [JsonIgnore]
            public Dictionary<string, AdministrativeArealType>? LevelsByCode
            {
                get
                {
                    return levelsByCode;
                }
            }

            /// <summary>
            /// Gets the access flags of the fixture.
            /// </summary>
            [JsonIgnore]
            public FlagsEnumFixture Flags
            {
                get
                {
                    return flags;
                }
            }

            /// <summary>
            /// Gets the long-backed value of the fixture.
            /// </summary>
            [JsonIgnore]
            public LongEnumFixture Long
            {
                get
                {
                    return @long;
                }
            }
        }
    }
}
