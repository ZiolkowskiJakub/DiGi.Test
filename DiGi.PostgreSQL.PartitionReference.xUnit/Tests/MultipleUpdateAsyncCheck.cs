using DiGi.Core.Classes;
using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.PartitionReference.Classes;

namespace DiGi.PostgreSQL.PartitionReference.xUnit
{
    public partial class Tests
    {
        /// <summary>
        /// Verifies that multiple address records can be updated asynchronously via the <see cref="PartitionReferencePostgreSQLConverter"/>,
        /// ensuring that unique partition references are generated and the record count is correctly maintained in the database.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task MultipleUpdateAsyncCheck()
        {
            ConnectionData connectionData = PostgreSQL.xUnit.Create.ConnectionData(Enums.StorageMethod.PartitionReference);

            PartitionReferencePostgreSQLConverter partitionReferencePostgreSQLConverter = new(connectionData);
            partitionReferencePostgreSQLConverter.PartitionReferenceGenerating += PartitionReferencePostgreSQLConverter_PartitionReferenceGenerating;

            List<Address> addresses = [];

            int count = 1000;

            for (int i = 0; i < count; i++)
            {
                addresses.Add(new($"{i} Main St", "Anytown", "CA", Core.Enums.CountryCode.GB));
            }

            addresses.Add(new("123 Main St", "Anytown", "CA", Core.Enums.CountryCode.PL));
            addresses.Add(new("123 Main St", "Anytown", "CA", Core.Enums.CountryCode.PL));

            count++;

            HashSet<PartitionReference.Classes.PartitionReference>? uniqueReferences_1 = await partitionReferencePostgreSQLConverter.UpdateAsync(addresses);
            Assert.NotNull(uniqueReferences_1);

            Assert.NotEmpty(uniqueReferences_1);

            string? name = uniqueReferences_1.ElementAt(0).Name;
            Assert.False(string.IsNullOrEmpty(name));

            long count_Temp = await partitionReferencePostgreSQLConverter.CountAsync(name);
            Assert.Equal(count, count_Temp);

            bool removed = await partitionReferencePostgreSQLConverter.RemoveAsync(name);

            Assert.True(removed);

            List<Address>? addresses_Temp = await partitionReferencePostgreSQLConverter.GetSerializableObjectsAsync<Address>(name);
            Assert.True(addresses_Temp is not null && addresses_Temp.Count == 0);
        }

        private void PartitionReferencePostgreSQLConverter_PartitionReferenceGenerating(object sender, PartitionReferenceGeneratingEventArgs e)
        {
            if (e.Item is not Core.Interfaces.ISerializableObject serializableObject)
            {
                e.PartitionReference = null;
                return;
            }

            e.PartitionReference = new PartitionReference.Classes.PartitionReference(serializableObject.GetType().Name, Core.Query.UniqueId(serializableObject));
        }
    }
}
