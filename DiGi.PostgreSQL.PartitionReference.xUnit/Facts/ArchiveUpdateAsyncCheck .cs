using DiGi.Core;
using DiGi.Core.Classes;
using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.PartitionReference.xUnit.Classes;

namespace DiGi.PostgreSQL.PartitionReference.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the asynchronous update and removal operations of the archive PostgreSQL converter function correctly when utilizing partition references.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task ArchiveUpdateAsyncCheck()
        {
            if (!PostgreSQL.xUnit.Create.IsAvailable(Enums.StorageMethod.PartitionReference, out ConnectionData? connectionData))
            {
                return;
            }

            ArchivePostgreSQLConverter archivePostgreSQLConverter = new(connectionData);

            string? name = Core.Query.FullTypeName(typeof(Address));
            Assert.NotNull(name);

            await archivePostgreSQLConverter.RemoveAsync(name);

            Address address_1 = new("123 Main St", "Anytown", "CA", Core.Enums.CountryCode.Undefined);

            PartitionReference.Classes.PartitionReference? partitionReference_1 = await archivePostgreSQLConverter.UpdateAsync(address_1);
            Assert.NotNull(partitionReference_1);

            Address? address_2 = await archivePostgreSQLConverter.GetSerializableObjectAsync<Address>(partitionReference_1);
            Assert.NotNull(partitionReference_1);

            Assert.Equal(address_1.ToSystem_String(), address_2.ToSystem_String());

            PartitionReference.Classes.PartitionReference? partitionReference_2 = await archivePostgreSQLConverter.RemoveAsync(partitionReference_1);

            Assert.Equal(partitionReference_1.ToSystem_String(), partitionReference_2.ToSystem_String());
        }
    }
}