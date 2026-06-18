using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Enums;
using DiGi.PostgreSQL.UniqueReference.Classes;

namespace DiGi.PostgreSQL.UniqueReference.xUnit.Classes
{
    /// <summary>
    /// Provides functionality to convert and map data types specifically for archive records within a PostgreSQL database, extending the base <see cref="UniqueReferencePostgreSQLConverter"/>.
    /// </summary>
    public class ArchivePostgreSQLConverter : UniqueReferencePostgreSQLConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArchivePostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The connection data used to initialize the converter.</param>
        public ArchivePostgreSQLConverter(ConnectionData connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the corresponding database data type for the specified .NET type.
        /// </summary>
        /// <param name="type">The type to be converted to a database data type.</param>
        /// <returns>The <see cref="DataType"/> that represents the PostgreSQL archive data type, or <see cref="DataType.Undefined"/> if the provided type is null.</returns>
        public override DataType GetDataType(Type? type)
        {
            if (type == null)
            {
                return DataType.Undefined;
            }

            return DataType.Archive;
        }
    }
}
