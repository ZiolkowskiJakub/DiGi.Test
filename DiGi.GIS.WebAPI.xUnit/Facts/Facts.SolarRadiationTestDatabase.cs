using DiGi.PostgreSQL.Classes;
using Npgsql;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// The scratch county part of the stored solar radiation controller facts. Only a partition key in the throwaway test databases.
        /// </summary>
        private const int countyId_SolarRadiation = 990311;

        /// <summary>
        /// The configuration names of the throwaway test databases and of the host databases they must never name, all read from <c>DiGi.Test/user files/</c>.
        /// </summary>
        private static readonly string[] fileNames_SolarRadiation = ["GIS_PostgreSQL_Main_Test.conf", "GIS_PostgreSQL_Storage_Test.conf", "GIS_PostgreSQL_Main.conf", "GIS_PostgreSQL_Storage.conf"];

        /// <summary>
        /// Verifies the guard that keeps the writing solar radiation facts off a host's database: a test configuration in <c>DiGi.Test/user files/</c> must not name the database of a host configuration there (<c>Coding - PostgreSQL.md</c> §6).
        /// <para>A refused configuration only skips the facts, and a suite of skips reads as green, so this plain fact is the visible failure. It passes when the files are absent.</para>
        /// </summary>
        [Fact]
        public void SolarRadiationTestDatabase()
        {
            ConnectionData? connectionData_Main = SolarRadiationConnectionData(fileNames_SolarRadiation[0]);
            ConnectionData? connectionData_Storage = SolarRadiationConnectionData(fileNames_SolarRadiation[1]);
            foreach (string fileName in new string[] { fileNames_SolarRadiation[2], fileNames_SolarRadiation[3] })
            {
                ConnectionData? connectionData_Host = SolarRadiationConnectionData(fileName);
                Assert.False(IsSameSolarRadiationDatabase(connectionData_Main, connectionData_Host), $"{fileNames_SolarRadiation[0]} names the database of {fileName}.");
                Assert.False(IsSameSolarRadiationDatabase(connectionData_Storage, connectionData_Host), $"{fileNames_SolarRadiation[1]} names the database of {fileName}.");
            }
        }

        /// <summary>
        /// Returns the connection data of the throwaway main and storage test databases, skipping the calling fact when either configuration is missing, names a host's database or does not answer.
        /// </summary>
        /// <returns>The main and the storage test connection data.</returns>
        private static (ConnectionData Main, ConnectionData Storage) SolarRadiationTestConnectionDatas()
        {
            ConnectionData? connectionData_Main = SolarRadiationConnectionData(fileNames_SolarRadiation[0]);
            ConnectionData? connectionData_Storage = SolarRadiationConnectionData(fileNames_SolarRadiation[1]);
            Skip.If(connectionData_Main is null || connectionData_Storage is null, $"{fileNames_SolarRadiation[0]} and {fileNames_SolarRadiation[1]} are needed in DiGi.Test/user files. Point them at throwaway databases - never a host's.");

            foreach (string fileName in new string[] { fileNames_SolarRadiation[2], fileNames_SolarRadiation[3] })
            {
                ConnectionData? connectionData_Host = SolarRadiationConnectionData(fileName);
                Skip.If(IsSameSolarRadiationDatabase(connectionData_Main, connectionData_Host) || IsSameSolarRadiationDatabase(connectionData_Storage, connectionData_Host), $"A test configuration names the database of {fileName}; the facts write what they test and refuse to run there. SolarRadiationTestDatabase fails on this.");
            }

            Skip.IfNot(DiGi.PostgreSQL.Query.IsAvailable(connectionData_Main!) && DiGi.PostgreSQL.Query.IsAvailable(connectionData_Storage!), "The PostgreSQL test databases are not reachable.");

            return (connectionData_Main!, connectionData_Storage!);
        }

        /// <summary>
        /// Reads the connection data of a configuration file in <c>DiGi.Test/user files/</c>.
        /// </summary>
        /// <param name="fileName">The name of the configuration file.</param>
        /// <returns>The connection data, or null when the file is missing or incomplete.</returns>
        private static ConnectionData? SolarRadiationConnectionData(string fileName)
        {
            string? directory = Core.xUnit.Query.UserFilesDirectory(Assembly.GetExecutingAssembly());
            if (directory is null)
            {
                return null;
            }

            string path = Path.Combine(directory, fileName);
            if (!File.Exists(path))
            {
                return null;
            }

            return DiGi.PostgreSQL.Create.ConnectionData(DiGi.PostgreSQL.Create.PostgreSQLConfigurationFile(path));
        }

        /// <summary>
        /// Answers whether two connection configurations name the same database on the same server: host and database compared case-insensitively, the port exactly with a missing one read as 5432. Credentials are not compared.
        /// </summary>
        /// <param name="connectionData_1">The first connection configuration.</param>
        /// <param name="connectionData_2">The second connection configuration.</param>
        /// <returns>True when both are present and name the same host, port and database; otherwise, false.</returns>
        private static bool IsSameSolarRadiationDatabase(ConnectionData? connectionData_1, ConnectionData? connectionData_2)
        {
            if (connectionData_1 is null || connectionData_2 is null)
            {
                return false;
            }

            return string.Equals(connectionData_1.Host?.Trim(), connectionData_2.Host?.Trim(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(connectionData_1.Database?.Trim(), connectionData_2.Database?.Trim(), StringComparison.OrdinalIgnoreCase)
                && (connectionData_1.Port ?? 5432) == (connectionData_2.Port ?? 5432);
        }

        /// <summary>
        /// Drops the scratch partitions of the stored solar radiation controller facts: the <c>building_2d</c> partition on the main test database and the <c>building_solar_radiation</c> partition on the storage test database. Called before writing and again in <c>finally</c>.
        /// </summary>
        /// <param name="connectionData_Main">The main test database.</param>
        /// <param name="connectionData_Storage">The storage test database.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private static async Task CleanupSolarRadiationScratchAsync(ConnectionData connectionData_Main, ConnectionData connectionData_Storage)
        {
            await using (NpgsqlConnection? npgsqlConnection_Main = DiGi.PostgreSQL.Create.NpgsqlConnection(connectionData_Main))
            {
                Assert.NotNull(npgsqlConnection_Main);
                await npgsqlConnection_Main.OpenAsync();
                await ExecuteSolarRadiationAsync(npgsqlConnection_Main, $"DROP TABLE IF EXISTS {PostgreSQL.Constants.TableName.Building2D}_{countyId_SolarRadiation};");
            }

            await using NpgsqlConnection? npgsqlConnection_Storage = DiGi.PostgreSQL.Create.NpgsqlConnection(connectionData_Storage);
            Assert.NotNull(npgsqlConnection_Storage);
            await npgsqlConnection_Storage.OpenAsync();
            await ExecuteSolarRadiationAsync(npgsqlConnection_Storage, $"DROP TABLE IF EXISTS {PostgreSQL.Constants.TableName.BuildingSolarRadiation}_{countyId_SolarRadiation};");
        }

        /// <summary>
        /// Executes one statement and discards its result.
        /// </summary>
        /// <param name="npgsqlConnection">The open connection to execute through.</param>
        /// <param name="commandText">The statement to execute.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private static async Task ExecuteSolarRadiationAsync(NpgsqlConnection npgsqlConnection, string commandText)
        {
            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            await npgsqlCommand.ExecuteNonQueryAsync();
        }
    }
}
