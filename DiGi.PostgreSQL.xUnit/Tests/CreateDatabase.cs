using DiGi.PostgreSQL.Classes;

namespace DiGi.PostgreSQL.xUnit
{
    public partial class Tests
    {
        [Fact]
        public async Task CreateDatabase()
        {
            ConnectionData connectionData;

            connectionData = Create.ConnectionData(Enums.StorageMethod.PartitionReference);

            Assert.True(await PostgreSQL.Create.Database(connectionData));

            connectionData = Create.ConnectionData(Enums.StorageMethod.UniqueReference);

            Assert.True(await PostgreSQL.Create.Database(connectionData));
        }
    }
}