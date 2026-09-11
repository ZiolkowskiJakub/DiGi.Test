using DiGi.GIS.PostgreSQL.Classes;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Creates a county partition through the shared partition statement and checks that PostgreSQL holds statistics for it straight away.
        /// <para>Skipped by default: it creates and drops a table, so it needs <c>GIS_PostgreSQL_Main.conf</c> beside the test assembly pointing at a scratch database - never the deployed one.</para>
        /// <para>A partition that has never been analysed reports <c>reltuples = -1</c>, and an empty one never gets analysed on its own, because autovacuum reacts to modifications and nothing modifies it. Every estimated count then reads it as "not measured" instead of zero, and one such partition voided the coverage figure of a whole voivodeship. The statement is also what every write runs before inserting, so the second half checks that re-running it on a partition that already has statistics leaves them alone - the analyse is guarded so that it does not run on every batch.</para>
        /// </summary>
        [Fact(Skip = "Creates and drops a table. Point GIS_PostgreSQL_Main.conf at a scratch database before running.")]
        public async Task TableAsync_Partition_Analysed()
        {
            const string tableName = "xunit_scratch_partition";
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
                await ExecuteAsync(npgsqlConnection, $"CREATE TABLE {tableName} (county_id integer NOT NULL, value integer) PARTITION BY LIST (county_id);");

                //A partition created through the shared statement is analysed at once, so an empty one reads as 0 rather than as -1

                Assert.True(await npgsqlConnection.TableAsync_Building2DReferencedObject_Partition(tableName, countyId));
                Assert.Equal(0, await ReltuplesAsync(npgsqlConnection, $"{tableName}_{countyId}"));

                //Re-running it on a partition that already has statistics keeps them as they are - rows written since are not counted in, which shows the analyse did not run again

                await ExecuteAsync(npgsqlConnection, $"INSERT INTO {tableName} (county_id, value) VALUES ({countyId}, 1), ({countyId}, 2);");
                Assert.True(await npgsqlConnection.TableAsync_Building2DReferencedObject_Partition(tableName, countyId));
                Assert.Equal(0, await ReltuplesAsync(npgsqlConnection, $"{tableName}_{countyId}"));
            }
            finally
            {
                await ExecuteAsync(npgsqlConnection, $"DROP TABLE IF EXISTS {tableName};");
            }
        }

        /// <summary>
        /// Reads the row estimate PostgreSQL holds for one relation.
        /// </summary>
        /// <param name="npgsqlConnection">The open connection to read through.</param>
        /// <param name="relationName">The relation whose estimate is wanted.</param>
        /// <returns>The value of <c>reltuples</c>, which is -1 for a relation that has never been analysed.</returns>
        private static async Task<long> ReltuplesAsync(NpgsqlConnection npgsqlConnection, string relationName)
        {
            await using NpgsqlCommand npgsqlCommand = new("SELECT reltuples::bigint FROM pg_class WHERE oid = to_regclass(@relationName)", npgsqlConnection);
            npgsqlCommand.Parameters.AddWithValue("relationName", relationName);

            object? result = await npgsqlCommand.ExecuteScalarAsync();
            Assert.NotNull(result);

            return (long)result;
        }
    }
}
