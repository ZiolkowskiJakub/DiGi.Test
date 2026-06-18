using System.Text.Json.Serialization;

namespace DiGi.Core.xUnit
{
    public partial class Classes
    {
        /// <summary>
        /// Provides a base class for objects that require a unique identifier.
        /// </summary>
        public abstract class UniqueIdBaseObject
        {
            /// <summary>
            /// Gets the unique identifier associated with the object.
            /// </summary>
            [JsonInclude]
            public abstract string? UniqueId { get; }
        }
    }
}
