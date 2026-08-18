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
        /// Creates a referenced-object table from the current DDL and from the DDL that preceded it, and checks the index set both end up with.
        /// <para>Skipped by default: it creates and drops a table, so it needs <c>GIS_PostgreSQL_Main.conf</c> beside the test assembly pointing at a scratch database. The table name below is a scratch one and belongs to no converter, but the connection is whatever the configuration file names - never point it at the deployed database.</para>
        /// <para><c>(county_id, reference)</c> is the primary access path of these tables and every read filters on it, while <c>(county_id, unique_id)</c> is already indexed by its own <c>UNIQUE</c> constraint. The third part is the one that matters most: <c>CREATE TABLE IF NOT EXISTS</c> leaves an existing table with the index set it was created with, so a table left over from the previous DDL is the state every deployed table is in, and the migration has to reach it without touching its rows.</para>
        /// </summary>
        [Fact(Skip = "Creates and drops a table. Point GIS_PostgreSQL_Main.conf at a scratch database before running.")]
        public async Task TableAsync_Building2DReferencedObject_Integration()
        {
            const string tableName = "xunit_scratch_referenced_object";
            const string indexName_Reference = "idx_xunit_scratch_referenced_object_county_id_reference";
            const string indexName_Legacy = "idx_xunit_scratch_referenced_object_unique_id_county";
            const int countyId = 5;

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

                //A table left over from the previous DDL is migrated in place, and the rows it holds survive it

                await ExecuteAsync(npgsqlConnection, $"DROP TABLE IF EXISTS {tableName};");
                await ExecuteAsync(npgsqlConnection, CommandText_Legacy(tableName));
                Assert.True(await npgsqlConnection.TableAsync_Building2DReferencedObject_Partition(tableName, countyId));
                await ExecuteAsync(npgsqlConnection, $"INSERT INTO {tableName} (county_id, unique_id, reference) VALUES ({countyId}, 'unique_id_1', 'reference_1');");

                HashSet<string> indexNames_Legacy = await IndexNamesAsync(npgsqlConnection, tableName);
                Assert.Contains(indexName_Legacy, indexNames_Legacy);
                Assert.DoesNotContain(indexName_Reference, indexNames_Legacy);

                Assert.True(await npgsqlConnection.TableAsync_Building2DReferencedObject(tableName));

                HashSet<string> indexNames_Migrated = await IndexNamesAsync(npgsqlConnection, tableName);
                Assert.Contains(indexName_Reference, indexNames_Migrated);
                Assert.DoesNotContain(indexName_Legacy, indexNames_Migrated);

                Assert.Equal(1, await DiGi.PostgreSQL.Query.CountAsync(npgsqlConnection, tableName));
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

        /// <summary>
        /// Builds the referenced-object DDL as it stood before the index fix, so a table in the state every deployed one is in can be created to migrate.
        /// <para>It is a copy rather than a call, deliberately: the shipped helper no longer produces this shape, and the whole point of the third part of the fact is to reach a table it did not produce.</para>
        /// </summary>
        /// <param name="tableName">The name of the table to create.</param>
        /// <returns>The statements creating the table in its pre-fix shape.</returns>
        private static string CommandText_Legacy(string tableName)
        {
            return $@"
                CREATE TABLE IF NOT EXISTS {tableName} (
                    id BIGINT GENERATED ALWAYS AS IDENTITY,
                    unique_id TEXT NOT NULL,
                    county_id INT NOT NULL,
                    reference TEXT NOT NULL,
                    object JSONB,
                    created_at timestamptz DEFAULT now(),
                    PRIMARY KEY (id, county_id),
                    UNIQUE (county_id, unique_id)
                ) PARTITION BY LIST (county_id);

                CREATE INDEX IF NOT EXISTS idx_{tableName}_unique_id_county
                ON {tableName} (county_id, unique_id);";
        }
    }
}
