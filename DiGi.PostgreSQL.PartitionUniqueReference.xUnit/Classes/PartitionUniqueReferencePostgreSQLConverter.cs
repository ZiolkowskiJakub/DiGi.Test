using DiGi.Core;
using DiGi.Core.Interfaces;
using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Enums;
using DiGi.PostgreSQL.PartitionUniqueReference.Classes;

namespace DiGi.PostgreSQL.PartitionUniqueReference.xUnit.Classes
{
    /// <summary>
    /// Represents a test implementation of the <see cref="PartitionUniqueReferencePostgreSQLConverter"/> class used for testing purposes.
    /// </summary>
    public class TestPartitionUniqueReferencePostgreSQLConverter : PartitionUniqueReferencePostgreSQLConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestPartitionUniqueReferencePostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The connection data used to initialize the converter.</param>
        public TestPartitionUniqueReferencePostgreSQLConverter(ConnectionData connectionData)
            : base(connectionData)
        {
            PartitionUniqueReferenceReferenceGenerating += TestPartitionUniqueReferencePostgreSQLConverter_PartitionUniqueReferenceReferenceGenerating;
        }

        private void TestPartitionUniqueReferencePostgreSQLConverter_PartitionUniqueReferenceReferenceGenerating(object sender, PartitionUniqueReferenceGeneratingEventArgs e)
        {
            e.PartitionReference = new PartitionReference.Classes.PartitionReference("AAA", (e.Item as ISerializableObject).UniqueId());
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
