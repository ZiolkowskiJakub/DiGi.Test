using DiGi.PostgreSQL.Classes;
using System.Reflection;

namespace DiGi.PostgreSQL.xUnit
{
    public static partial class Create
    {
        /// <summary>
        /// Creates and returns a <see cref="ConnectionData"/> instance based on the specified storage method by loading the corresponding configuration file from the executing assembly's directory.
        /// </summary>
        /// <param name="storageMethod">The <see cref="Enums.StorageMethod"/> used to determine which PostgreSQL configuration file to load.</param>
        /// <returns>A <see cref="ConnectionData"/> object containing the connection details retrieved from the configuration file.</returns>
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

                case Enums.StorageMethod.Table:
                    fileName = "PostgreSQL_Table.conf";
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
