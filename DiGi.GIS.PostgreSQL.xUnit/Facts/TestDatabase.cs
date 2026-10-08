using DiGi.PostgreSQL.Classes;
using System;
using System.IO;
using System.Reflection;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// The configuration of the throwaway main test database, kept in the git-ignored <c>user files/</c> and copied beside the test assembly.
        /// </summary>
        private const string fileName_PostgreSQL_Main_Test = "GIS_PostgreSQL_Main_Test.conf";

        /// <summary>
        /// The configuration of the throwaway storage test database, kept in the git-ignored <c>user files/</c> and copied beside the test assembly.
        /// </summary>
        private const string fileName_PostgreSQL_Storage_Test = "GIS_PostgreSQL_Storage_Test.conf";

        /// <summary>
        /// Verifies the guard that keeps the writing database facts off a host's database: they connect through their own configuration files, never the ones the Web API, the tray and the background tasks read, and a test configuration naming the same database as the host configuration is refused.
        /// <para>The host configurations reach this assembly through the same copy of <c>user files/</c>, and on a deployed host they name production (<c>Coding - PostgreSQL.md</c> §6). The comparison is pinned on synthetic configurations - case folds on the host and the database, the port and every name counts - and then the files beside this assembly are checked: when both of a pair are present they must name different databases. That half is the visible failure, because a refused configuration only skips the facts, and a suite of skips reads as green.</para>
        /// <para>The fact needs no server.</para>
        /// </summary>
        [Fact]
        public void TestDatabase()
        {
            Assert.NotEqual(Constants.FileName.PostgreSQL_Main, fileName_PostgreSQL_Main_Test);
            Assert.NotEqual(Constants.FileName.PostgreSQL_Storage, fileName_PostgreSQL_Storage_Test);

            ConnectionData connectionData = new("localhost", "postgres", "x", "gis_main", 5432);

            Assert.True(IsSameDatabase(connectionData, new ConnectionData("LOCALHOST", "other_user", "y", "GIS_Main", 5432)));
            Assert.True(IsSameDatabase(connectionData, new ConnectionData("localhost", "postgres", "x", "gis_main", null)));
            Assert.False(IsSameDatabase(connectionData, new ConnectionData("localhost", "postgres", "x", "gis_main_test", 5432)));
            Assert.False(IsSameDatabase(connectionData, new ConnectionData("db.example", "postgres", "x", "gis_main", 5432)));
            Assert.False(IsSameDatabase(connectionData, new ConnectionData("localhost", "postgres", "x", "gis_main", 5433)));
            Assert.False(IsSameDatabase(connectionData, null));
            Assert.False(IsSameDatabase(null, null));

            Assert.False(IsSameDatabase(TestConnectionData(fileName_PostgreSQL_Main_Test), TestConnectionData(Constants.FileName.PostgreSQL_Main)), $"{fileName_PostgreSQL_Main_Test} names the database of {Constants.FileName.PostgreSQL_Main}.");
            Assert.False(IsSameDatabase(TestConnectionData(fileName_PostgreSQL_Storage_Test), TestConnectionData(Constants.FileName.PostgreSQL_Storage)), $"{fileName_PostgreSQL_Storage_Test} names the database of {Constants.FileName.PostgreSQL_Storage}.");
            Assert.False(IsSameDatabase(TestConnectionData(fileName_PostgreSQL_Main_Test), TestConnectionData(Constants.FileName.PostgreSQL_Storage)), $"{fileName_PostgreSQL_Main_Test} names the database of {Constants.FileName.PostgreSQL_Storage}.");
            Assert.False(IsSameDatabase(TestConnectionData(fileName_PostgreSQL_Storage_Test), TestConnectionData(Constants.FileName.PostgreSQL_Main)), $"{fileName_PostgreSQL_Storage_Test} names the database of {Constants.FileName.PostgreSQL_Main}.");
        }

        /// <summary>
        /// Reads the connection data of a configuration file copied beside the test assembly.
        /// </summary>
        /// <param name="fileName">The name of the configuration file.</param>
        /// <returns>The connection data, or null when the file is missing or incomplete.</returns>
        private static ConnectionData? TestConnectionData(string fileName)
        {
            string? directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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
        private static bool IsSameDatabase(ConnectionData? connectionData_1, ConnectionData? connectionData_2)
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
        /// Returns the connection data of the throwaway main and storage test databases, skipping the calling fact when either configuration is missing, names a host's database or does not answer.
        /// <para>This is the only way the writing year built facts reach a database.</para>
        /// </summary>
        /// <returns>The main and the storage test connection data.</returns>
        private static (ConnectionData Main, ConnectionData Storage) TestConnectionDatas()
        {
            ConnectionData? connectionData_Main = TestConnectionData(fileName_PostgreSQL_Main_Test);
            ConnectionData? connectionData_Storage = TestConnectionData(fileName_PostgreSQL_Storage_Test);
            Skip.If(connectionData_Main is null || connectionData_Storage is null, $"{fileName_PostgreSQL_Main_Test} and {fileName_PostgreSQL_Storage_Test} are needed beside the test assembly. Point them at throwaway databases - never a host's.");

            foreach (string fileName in new string[] { Constants.FileName.PostgreSQL_Main, Constants.FileName.PostgreSQL_Storage })
            {
                ConnectionData? connectionData_Host = TestConnectionData(fileName);
                Skip.If(IsSameDatabase(connectionData_Main, connectionData_Host) || IsSameDatabase(connectionData_Storage, connectionData_Host), $"A test configuration names the database of {fileName}; the facts write what they test and refuse to run there. TestDatabase fails on this.");
            }

            Skip.IfNot(DiGi.PostgreSQL.Query.IsAvailable(connectionData_Main!) && DiGi.PostgreSQL.Query.IsAvailable(connectionData_Storage!), "The PostgreSQL test databases are not reachable.");

            return (connectionData_Main!, connectionData_Storage!);
        }
    }
}
