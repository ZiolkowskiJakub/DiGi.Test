using DiGi.Core.Classes;
using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.UniqueReference.Classes;

namespace DiGi.PostgreSQL.UniqueReference.xUnit
{
    public partial class Tests
    {
        [SkippableFact]
        public async Task MultipleUpdateAsyncCheck()
        {
            ConnectionData connectionData = PostgreSQL.xUnit.Create.ConnectionData(Enums.StorageMethod.UniqueReference);

            PostgreSQLConverter postgreSQLConverter = new(connectionData);

            List<Address> addresses = [];

            int count = 1000;

            for (int i = 0; i < count; i++)
            {
                addresses.Add(new($"{i} Main St", "Anytown", "CA", Core.Enums.CountryCode.GB));
            }

            addresses.Add(new("123 Main St", "Anytown", "CA", Core.Enums.CountryCode.PL));
            addresses.Add(new("123 Main St", "Anytown", "CA", Core.Enums.CountryCode.PL));

            count++;

            HashSet<Core.Classes.UniqueReference>? uniqueReferences_1 = await postgreSQLConverter.UpdateAsync(addresses);
            Assert.NotNull(uniqueReferences_1);

            long count_Temp = await postgreSQLConverter.CountAsync<Address>();
            Assert.Equal(count, count_Temp);

            bool removed = await postgreSQLConverter.RemoveAsync<Address>(false);

            Assert.True(removed);

            List<Address>? addresses_Temp = await postgreSQLConverter.GetSerializableObjects<Address>();
            Assert.True(addresses_Temp is not null && addresses_Temp.Count == 0);
        }
    }
}