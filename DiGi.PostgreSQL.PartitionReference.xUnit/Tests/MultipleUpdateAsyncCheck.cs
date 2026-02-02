using DiGi.Core.Classes;
using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.PartitionReference.Classes;

namespace DiGi.PostgreSQL.PartitionReference.xUnit
{
    public partial class Tests
    {
        [SkippableFact]
        public async Task MultipleUpdateAsyncCheck()
        {
            ConnectionData connectionData = PostgreSQL.xUnit.Create.ConnectionData(Enums.StorageMethod.PartitionReference);

            PostgreSQLConverter postgreSQLConverter = new(connectionData);
            postgreSQLConverter.PartitionReferenceGenerating += PostgreSQLConverter_PartitionReferenceGenerating;

            List<Address> addresses = [];

            int count = 1000;

            for (int i = 0; i < count; i++)
            {
                addresses.Add(new($"{i} Main St", "Anytown", "CA", Core.Enums.CountryCode.GB));
            }

            addresses.Add(new("123 Main St", "Anytown", "CA", Core.Enums.CountryCode.PL));
            addresses.Add(new("123 Main St", "Anytown", "CA", Core.Enums.CountryCode.PL));

            count++;

            HashSet<Classes.PartitionReference>? uniqueReferences_1 = await postgreSQLConverter.UpdateAsync(addresses);
            Assert.NotNull(uniqueReferences_1);

            Assert.NotEmpty(uniqueReferences_1);

            string? name = uniqueReferences_1.ElementAt(0).Name;
            Assert.False(string.IsNullOrEmpty(name));

            long count_Temp = await postgreSQLConverter.CountAsync(name);
            Assert.Equal(count, count_Temp);

            bool removed = await postgreSQLConverter.RemoveAsync(name);

            Assert.True(removed);

            List<Address>? addresses_Temp = await postgreSQLConverter.GetSerializableObjects<Address>(name);
            Assert.True(addresses_Temp is not null && addresses_Temp.Count == 0);
        }

        private void PostgreSQLConverter_PartitionReferenceGenerating(object sender, PartitionReferenceGeneratingEventArgs e)
        {
            if(e.Item is not Core.Interfaces.ISerializableObject serializableObject)
            {
                e.PartitionReference = null;
                return;
            }

            e.PartitionReference = new Classes.PartitionReference(serializableObject.GetType().Name, Core.Query.UniqueId(serializableObject));
        }
    }
}