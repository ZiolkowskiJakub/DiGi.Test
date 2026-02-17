using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Enums;
using DiGi.PostgreSQL.UniqueReference.Classes;

namespace DiGi.PostgreSQL.UniqueReference.xUnit.Classes
{
    public class ArchivePostgreSQLConverter : UniqueReferencePostgreSQLConverter
    {
        public ArchivePostgreSQLConverter(ConnectionData connectionData)
            : base(connectionData)
        {

        }

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
