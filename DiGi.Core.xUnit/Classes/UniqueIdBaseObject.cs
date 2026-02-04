using System.Text.Json.Serialization;

namespace DiGi.Core.xUnit
{
    public partial class Classes
    {
        public abstract class UniqueIdBaseObject
        {
            [JsonInclude]
            public abstract string? UniqueId { get; }
        }
    }
}