using DiGi.Core;
using DiGi.Core.Classes;
using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.PartitionReference.Classes;

namespace DiGi.PostgreSQL.PartitionReference.xUnit
{
    public partial class Tests
    {
        [SkippableFact]
        public async Task UpdateAsyncCheck()
        {
            ConnectionData connectionData = PostgreSQL.xUnit.Create.ConnectionData(Enums.StorageMethod.UniqueReference);

            PostgreSQLConverter postgreSQLConverter = new(connectionData);

            Address address_1 = new("123 Main St", "Anytown", "CA", Core.Enums.CountryCode.Undefined);

            Classes.PartitionReference? partitionReference_1 = await postgreSQLConverter.UpdateAsync(address_1);
            Assert.NotNull(partitionReference_1);

            Address? address_2 = await postgreSQLConverter.GetSerializableObjects<Address>(partitionReference_1);
            Assert.NotNull(partitionReference_1);

            Assert.Equal(address_1.ToSystem_String(), address_2.ToSystem_String());

            Classes.PartitionReference? partitionReference_2 = await postgreSQLConverter.RemoveAsync(partitionReference_1);

            Assert.Equal(partitionReference_1.ToSystem_String(), partitionReference_2.ToSystem_String());
        }
    }
}