using DiGi.Core;
using DiGi.Core.Classes;
using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.PartitionReference.Classes;

namespace DiGi.PostgreSQL.PartitionReference.xUnit
{
    public partial class Tests
    {
        /// <summary>
        /// Verifies that the update process for a partition reference in PostgreSQL correctly persists the data and allows for subsequent retrieval and verification of the serialized object.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task UpdateAsyncCheck()
        {
            ConnectionData connectionData = PostgreSQL.xUnit.Create.ConnectionData(Enums.StorageMethod.PartitionReference);

            PartitionReferencePostgreSQLConverter partitionReferencePostgreSQLConverter = new(connectionData);

            Address address_1 = new("123 Main St", "Anytown", "CA", Core.Enums.CountryCode.Undefined);

            PartitionReference.Classes.PartitionReference? partitionReference_1 = await partitionReferencePostgreSQLConverter.UpdateAsync(address_1);
            Assert.NotNull(partitionReference_1);

            Address? address_2 = await partitionReferencePostgreSQLConverter.GetSerializableObjectAsync<Address>(partitionReference_1);
            Assert.NotNull(partitionReference_1);

            Assert.Equal(address_1.ToSystem_String(), address_2.ToSystem_String());

            PartitionReference.Classes.PartitionReference? partitionReference_2 = await partitionReferencePostgreSQLConverter.RemoveAsync(partitionReference_1);

            Assert.Equal(partitionReference_1.ToSystem_String(), partitionReference_2.ToSystem_String());
        }
    }
}
