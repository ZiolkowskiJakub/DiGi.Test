using DiGi.GIS.PostgreSQL.Classes;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Creates a referenced-object table from the current DDL and checks the index set it ends up with.
        /// <para>Skipped by default: it creates and drops a table, so it needs <c>GIS_PostgreSQL_Main.conf</c> beside the test assembly pointing at a scratch database. The table name below is a scratch one and belongs to no converter, but the connection is whatever the configuration file names - never point it at the deployed database.</para>
        /// <para><c>(county_id, reference)</c> is the primary access path of these tables and every read filters on it, while <c>(county_id, unique_id)</c> is already indexed by its own <c>UNIQUE</c> constraint.</para>
        /// </summary>
        [Fact(Skip = "Creates and drops a table. Point GIS_PostgreSQL_Main.conf at a scratch database before running.")]
        public async Task TableAsync_Building2DReferencedObject_Integration()
        {
            const string tableName = "xunit_scratch_referenced_object";
            const string indexName_Reference = "idx_xunit_scratch_referenced_object_county_id_reference";
            const string indexName_Legacy = "idx_xunit_scratch_referenced_object_unique_id_county";

            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            Building2DOccupancyDataPostgreSQLConverter? building2DOccupancyDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DOccupancyDataPostgreSQLConverter>();
            Assert.NotNull(building2DOccupancyDataPostgreSQLConverter);

            ConnectionData? connectionData = building2DOccupancyDataPostgreSQLConverter.ConnectionData;
            Assert.NotNull(connectionData);

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(connectionData);
            Assert.NotNull(npgsqlConnection);

            await npgsqlConnection.OpenAsync();

            try
            {
                await ExecuteAsync(npgsqlConnection, $"DROP TABLE IF EXISTS {tableName};");

                //A table created from the current DDL carries the reference index and nothing of its own on (county_id, unique_id)

                Assert.True(await npgsqlConnection.TableAsync_Building2DReferencedObject(tableName));

                HashSet<string> indexNames = await IndexNamesAsync(npgsqlConnection, tableName);
                Assert.Contains(indexName_Reference, indexNames);
                Assert.DoesNotContain(indexName_Legacy, indexNames);

                //Running it again leaves the index set exactly as it was - both statements are no-ops on a table already in shape

                Assert.True(await npgsqlConnection.TableAsync_Building2DReferencedObject(tableName));
                Assert.Equal(indexNames, await IndexNamesAsync(npgsqlConnection, tableName));
            }
            finally
            {
                await ExecuteAsync(npgsqlConnection, $"DROP TABLE IF EXISTS {tableName};");
            }
        }

        /// <summary>
        /// Returns the names of the indexes PostgreSQL currently holds for one table.
        /// <para>Read from <c>pg_indexes</c> rather than from what the DDL asked for, because the point of every assertion using it is what the server ended up with. Indexes on a partitioned parent are listed here as well, so a partitioned table answers with the parent's set rather than with nothing.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The open connection to read through.</param>
        /// <param name="tableName">The table whose indexes are wanted.</param>
        /// <returns>The index names held for the table.</returns>
        private static async Task<HashSet<string>> IndexNamesAsync(NpgsqlConnection npgsqlConnection, string tableName)
        {
            string commandText = @"
                SELECT indexname
                FROM pg_indexes
                WHERE schemaname = 'public'
                  AND tablename = @tableName;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.Parameters.AddWithValue("tableName", tableName);

            HashSet<string> result = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync();
            while (await npgsqlDataReader.ReadAsync())
            {
                result.Add(npgsqlDataReader.GetString(0));
            }

            return result;
        }

        /// <summary>
        /// Executes one statement and discards its result.
        /// </summary>
        /// <param name="npgsqlConnection">The open connection to execute through.</param>
        /// <param name="commandText">The statement to execute.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private static async Task ExecuteAsync(NpgsqlConnection npgsqlConnection, string commandText)
        {
            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);

            await npgsqlCommand.ExecuteNonQueryAsync();
        }
    }
}
