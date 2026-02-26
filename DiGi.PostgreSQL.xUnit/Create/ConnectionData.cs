using DiGi.PostgreSQL.Classes;
using System.Reflection;

namespace DiGi.PostgreSQL.xUnit
{
    public static partial class Create
    {
        public static ConnectionData ConnectionData(Enums.StorageMethod storageMethod)
        {
            Assert.True(storageMethod != Enums.StorageMethod.Undefined);

            string? directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Assert.NotNull(directory);

            string? fileName = null;
            switch (storageMethod)
            {
                case Enums.StorageMethod.UniqueReference:
                    fileName = "PostgreSQL_UniqueReference.conf";
                    break;

                case Enums.StorageMethod.PartitionReference:
                    fileName = "PostgreSQL_PartitionReference.conf";
                    break;

                case Enums.StorageMethod.PartitionUniqueReference:
                    fileName = "PostgreSQL_PartitionUniqueReference.conf";
                    break;
            }

            Assert.NotNull(fileName);

            string path = Path.Combine(directory, fileName);

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