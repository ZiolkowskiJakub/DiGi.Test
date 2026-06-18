using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Enums;
using DiGi.PostgreSQL.UniqueReference.Classes;

namespace DiGi.PostgreSQL.UniqueReference.xUnit.Classes
{
    /// <summary>
    /// Provides a PostgreSQL converter implementation for handling byte arrays as binary data within the database.
    /// </summary>
    public class BytesPostgreSQLConverter : UniqueReferencePostgreSQLConverter
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
        /// Gets the corresponding database data type for the specified .NET type.
        /// </summary>
        /// <param name="type">The .NET type to evaluate.</param>
        /// <returns>The <see cref="DataType"/> representing the binary format if a type is provided; otherwise, <see cref="DataType.Undefined"/>.</returns>
        public override DataType GetDataType(Type? type)
        {
            if (type == null)
            {
                return DataType.Undefined;
            }

            return DataType.Binary;
        }
    }
}
