using DiGi.PostgreSQL.Classes;

namespace DiGi.PostgreSQL.xUnit
{
    public partial class Tests
    {
        [Fact]
        public async Task CreateDatabase()
        {
            ConnectionData connectionData = Create.ConnectionData();

            Assert.True(await PostgreSQL.Create.Database(connectionData));
        }
    }
}