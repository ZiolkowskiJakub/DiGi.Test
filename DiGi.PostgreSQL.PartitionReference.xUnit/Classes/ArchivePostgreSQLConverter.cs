using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Enums;
using DiGi.PostgreSQL.PartitionReference.Classes;

namespace DiGi.PostgreSQL.PartitionReference.xUnit.Classes
{
    /// <summary>
    /// Provides functionality to convert and manage PostgreSQL partition references specifically for archive data.
    /// </summary>
    public class ArchivePostgreSQLConverter : PartitionReferencePostgreSQLConverter
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
        /// Gets the data type associated with the specified name.
        /// </summary>
        /// <param name="name">The name of the member for which to retrieve the data type.</param>
        /// <returns>The <see cref="DataType"/> corresponding to the provided name, or <see cref="DataType.Undefined"/> if the name is null or whitespace.</returns>
        public override DataType GetDataType(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return DataType.Undefined;
            }

            return DataType.Archive;
        }
    }
}