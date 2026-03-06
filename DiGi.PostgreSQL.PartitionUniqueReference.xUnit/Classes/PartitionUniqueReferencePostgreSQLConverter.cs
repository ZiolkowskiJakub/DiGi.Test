using DiGi.Core;
using DiGi.Core.Interfaces;
using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Enums;
using DiGi.PostgreSQL.PartitionUniqueReference.Classes;

namespace DiGi.PostgreSQL.PartitionUniqueReference.xUnit.Classes
{
    public class TestPartitionUniqueReferencePostgreSQLConverter : PartitionUniqueReferencePostgreSQLConverter
    {
        public TestPartitionUniqueReferencePostgreSQLConverter(ConnectionData connectionData)
            : base(connectionData)
        {
            PartitionUniqueReferenceReferenceGenerating += TestPartitionUniqueReferencePostgreSQLConverter_PartitionUniqueReferenceReferenceGenerating;
        }

        private void TestPartitionUniqueReferencePostgreSQLConverter_PartitionUniqueReferenceReferenceGenerating(object sender, PartitionUniqueReferenceGeneratingEventArgs e)
        {
            e.PartitionReference = new PartitionReference.Classes.PartitionReference("AAA", (e.Item as ISerializableObject).UniqueId());
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