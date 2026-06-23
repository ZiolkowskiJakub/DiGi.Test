using DiGi.PostgreSQL.Classes;
using System.Reflection;

namespace DiGi.PostgreSQL.xUnit
{
    public static partial class Create
    {
        /// <summary>
        /// Checks if the PostgreSQL configuration file exists and if the PostgreSQL database service is available/accessible.
        /// Prints a warning if the database is not available.
        /// </summary>
        /// <param name="storageMethod">The storage method to check.</param>
        /// <param name="connectionData">The loaded connection data, or null if not available.</param>
        /// <returns>True if PostgreSQL is available; otherwise, false.</returns>
        public static bool IsAvailable(Enums.StorageMethod storageMethod, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ConnectionData? connectionData)
        {
            connectionData = null;
            try
            {
                string? directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (directory == null)
                {
                    Console.WriteLine("[WARNING] Could not determine executing assembly directory.");
                    return false;
                }

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

                if (fileName == null)
                {
                    Console.WriteLine("[WARNING] Storage method is undefined or invalid.");
                    return false;
                }

                string path = Path.Combine(directory, fileName);
                if (!File.Exists(path))
                {
                    Console.WriteLine($"[WARNING] PostgreSQL configuration file '{fileName}' does not exist at '{path}'. PostgreSQL is not available.");
                    return false;
                }

                PostgreSQLConfigurationFile? postgreSQLConfigurationFile = PostgreSQL.Create.PostgreSQLConfigurationFile(path);
                if (postgreSQLConfigurationFile == null)
                {
                    Console.WriteLine($"[WARNING] Failed to load PostgreSQL configuration file '{fileName}'.");
                    return false;
                }

                connectionData = PostgreSQL.Create.ConnectionData(postgreSQLConfigurationFile);
                if (connectionData == null)
                {
                    Console.WriteLine($"[WARNING] Failed to create ConnectionData from configuration file '{fileName}'.");
                    return false;
                }

                if (!Query.IsAvailable(connectionData.GetDefault()))
                {
                    Console.WriteLine("[WARNING] PostgreSQL database server/service is not available or could not be reached.");
                    connectionData = null;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Exception occurred while checking PostgreSQL availability: {ex.Message}");
                connectionData = null;
                return false;
            }
        }
    }
}