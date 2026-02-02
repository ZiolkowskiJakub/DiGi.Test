using System.Text.Json.Serialization;

namespace DiGi.Core.xUnit
{
    public partial class Classes
    {
        public class UniqueIdObject : UniqueIdBaseObject
        {
            [JsonInclude]
            public Guid Guid { get; set; } = Guid.NewGuid();

            [JsonIgnore]
            public override string? UniqueId
            {
                get
                {
                    return Guid.ToString();
                }
            }
        }
    }

}
