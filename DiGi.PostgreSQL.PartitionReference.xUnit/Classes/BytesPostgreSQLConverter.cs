using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Enums;
using DiGi.PostgreSQL.PartitionReference.Classes;

namespace DiGi.PostgreSQL.PartitionReference.xUnit.Classes
{
    /// <summary>
    /// Provides a PostgreSQL converter implementation for handling byte arrays as partition references.
    /// </summary>
    public class BytesPostgreSQLConverter : PartitionReferencePostgreSQLConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BytesPostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The connection data used to initialize the converter.</param>
        public BytesPostgreSQLConverter(ConnectionData connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the PostgreSQL data type associated with the specified name.
        /// </summary>
        /// <param name="name">The name of the column or parameter for which to retrieve the data type.</param>
        /// <returns>A <see cref="DataType"/> representing binary data if a valid name is provided; otherwise, an undefined data type.</returns>
        public override DataType GetDataType(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return DataType.Undefined;
            }

            return DataType.Binary;
        }
    }
}
