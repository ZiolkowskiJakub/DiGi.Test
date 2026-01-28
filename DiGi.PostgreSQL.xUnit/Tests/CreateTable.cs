using DiGi.PostgreSQL.Classes;
using System.Reflection;
using Xunit.Sdk;

namespace DiGi.PostgreSQL.xUnit
{
    public partial class Tests
    {
        [Fact]
        public async Task CreateTable()
        {
            string? directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Assert.NotNull(directory);

            string path = Path.Combine(directory, "PostgreSQL.conf");

            Assert.True(File.Exists(path));

            PostgreSQLConfigurationFile? postgreSQLConfigurationFile = Create.PostgreSQLConfigurationFile(path);
            Assert.NotNull(postgreSQLConfigurationFile);

            ConnectionData? connectionData = Create.ConnectionData(postgreSQLConfigurationFile);
            Assert.NotNull(connectionData);

            if (!Query.IsAvailable(connectionData.GetDefault()))
            {
                throw SkipException.ForSkip("PostgreSQL service is not available");
            }

            bool result = false;

            result = await Create.Database(connectionData);

            Assert.True(result);
        }
    }
}