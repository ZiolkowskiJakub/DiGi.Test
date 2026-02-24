using DiGi.Core;
using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.UniqueReference.xUnit.Classes;

namespace DiGi.PostgreSQL.UniqueReference.xUnit
{
    public partial class Tests
    {
        [SkippableFact]
        public async Task ArchiveUpdateAsyncCheck()
        {
            ConnectionData connectionData = PostgreSQL.xUnit.Create.ConnectionData(Enums.StorageMethod.UniqueReference);

            ArchivePostgreSQLConverter archivePostgreSQLConverter = new(connectionData);

            string? name = Core.Query.FullTypeName(typeof(Address));
            Assert.NotNull(name);

            await archivePostgreSQLConverter.RemoveAsync<Address>();

            Address address_1 = new("123 Main St", "Anytown", "CA", Core.Enums.CountryCode.Undefined);

            IUniqueReference? uniqueReference_1 = await archivePostgreSQLConverter.UpdateAsync(address_1);
            Assert.NotNull(uniqueReference_1);

            Address? address_2 = await archivePostgreSQLConverter.GetSerializableObjectAsync<Address>(uniqueReference_1);
            Assert.NotNull(uniqueReference_1);

            Assert.Equal(address_1.ToSystem_String(), address_2.ToSystem_String());

            IUniqueReference? uniqueReference_2 = await archivePostgreSQLConverter.RemoveAsync(uniqueReference_1);

            Assert.Equal(uniqueReference_1.ToSystem_String(), uniqueReference_2.ToSystem_String());
        }
    }
}