using DiGi.PostgreSQL.Classes;

namespace DiGi.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the PostgreSQL database can be successfully created for all supported storage methods, including partition references, unique references, partition unique references, and standard tables.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation of the test.</returns>
        [Fact]
        public async Task CreateDatabase()
        {
            if (!Create.IsAvailable(Enums.StorageMethod.PartitionReference, out ConnectionData? connectionData))
            {
                return;
            }

            Assert.True(await PostgreSQL.Create.DatabaseAsync(connectionData));

            connectionData = Create.ConnectionData(Enums.StorageMethod.UniqueReference);

            Assert.True(await PostgreSQL.Create.DatabaseAsync(connectionData));

            connectionData = Create.ConnectionData(Enums.StorageMethod.PartitionUniqueReference);

            Assert.True(await PostgreSQL.Create.DatabaseAsync(connectionData));

            connectionData = Create.ConnectionData(Enums.StorageMethod.Table);

            Assert.True(await PostgreSQL.Create.DatabaseAsync(connectionData));
        }
    }
}