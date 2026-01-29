using DiGi.PostgreSQL.Classes;
using System.Reflection;

namespace DiGi.PostgreSQL.xUnit
{
    public static partial class Create
    {
        public static ConnectionData ConnectionData()
        {
            string? directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Assert.NotNull(directory);

            string path = Path.Combine(directory, "PostgreSQL.conf");

            Assert.True(File.Exists(path));

            PostgreSQLConfigurationFile? postgreSQLConfigurationFile = PostgreSQL.Create.PostgreSQLConfigurationFile(path);
            Assert.NotNull(postgreSQLConfigurationFile);

            ConnectionData? connectionData = PostgreSQL.Create.ConnectionData(postgreSQLConfigurationFile);
            Assert.NotNull(connectionData);

            Skip.IfNot(Query.IsAvailable(connectionData.GetDefault()), "PostgreSQL service is not available");

            return connectionData;
        }
    }
}