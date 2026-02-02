using DiGi.Core;
using DiGi.Core.Classes;
using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.UniqueReference.Classes;

namespace DiGi.PostgreSQL.UniqueReference.xUnit
{
    public partial class Tests
    {
        [SkippableFact]
        public async Task UpdateAsyncCheck()
        {
            ConnectionData connectionData = PostgreSQL.xUnit.Create.ConnectionData(Enums.StorageMethod.UniqueReference);

            PostgreSQLConverter postgreSQLConverter = new(connectionData);

            Address address_1 = new("123 Main St", "Anytown", "CA", Core.Enums.CountryCode.Undefined);

            Core.Classes.UniqueReference? uniqueReference_1 = await postgreSQLConverter.UpdateAsync(address_1);
            Assert.NotNull(uniqueReference_1);

            Address? address_2 = await postgreSQLConverter.GetSerializableObjects<Address>(uniqueReference_1);
            Assert.NotNull(uniqueReference_1);

            Assert.Equal(address_1.ToSystem_String(), address_2.ToSystem_String());

            Core.Classes.UniqueReference? uniqueReference_2 = await postgreSQLConverter.RemoveAsync(uniqueReference_1);

            Assert.Equal(uniqueReference_1.ToSystem_String(), uniqueReference_2.ToSystem_String());
        }
    }
}