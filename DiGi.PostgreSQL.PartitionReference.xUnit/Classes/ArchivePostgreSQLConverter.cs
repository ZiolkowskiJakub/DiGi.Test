using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Enums;
using DiGi.PostgreSQL.PartitionReference.Classes;

namespace DiGi.PostgreSQL.PartitionReference.xUnit.Classes
{
    public class ArchivePostgreSQLConverter : PartitionReferencePostgreSQLConverter
    {
        public ArchivePostgreSQLConverter(ConnectionData connectionData)
            : base(connectionData)
        {

        }

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
