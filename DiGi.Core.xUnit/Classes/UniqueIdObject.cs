using System;
using System.Text.Json.Serialization;

namespace DiGi.Core.xUnit
{
    public partial class Classes
    {
        /// <summary>
        /// Represents an object that possesses a globally unique identifier (GUID) as its unique identification mechanism.
        /// </summary>
        public class UniqueIdObject : UniqueIdBaseObject
        {
            /// <summary>
            /// Gets or sets the globally unique identifier (GUID) for the object.
            /// </summary>
            [JsonInclude]
            public Guid Guid { get; set; } = Guid.NewGuid();

            /// <summary>
            /// Gets the unique identifier of the object as a string representation of its GUID.
            /// </summary>
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
