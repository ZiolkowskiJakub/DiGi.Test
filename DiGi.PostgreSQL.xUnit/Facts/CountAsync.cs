using DiGi.PostgreSQL.Classes;
using Npgsql;
using System.Threading.Tasks;

namespace DiGi.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Pins the guard contract of <see cref="Query.CountAsync(NpgsqlConnection, string, int, System.Threading.CancellationToken)"/> that is decided before any statement is built.
        /// <para>None of these cases needs a database or an open connection: a blank table name answers -1 before the existence check runs. Naming the <c>commandTimeout</c> argument is itself the point - the defect this change fixes was that no caller could set it at all, so this fact does not compile against the pre-fix signature.</para>
        /// </summary>
        [Fact]
        public async Task CountAsync_Guards()
        {
            await using NpgsqlConnection npgsqlConnection = new();

            // The connection is never opened below: each assertion returns before any command is created.
            Assert.Equal(-1, await Query.CountAsync(npgsqlConnection, string.Empty, commandTimeout: 30));
            Assert.Equal(-1, await Query.CountAsync(npgsqlConnection, "   ", commandTimeout: 30));
        }

        /// <summary>
        /// Verifies the <c>commandTimeout</c> parameter is accepted and threaded to the count statement end to end, answering the exact row count under both a disabled timeout (0) and a normal one.
        /// <para>Skipped by default: it creates and drops a scratch table, so it needs <c>PostgreSQL_Table.conf</c> beside the test assembly pointing at a scratch database - never the deployed one.</para>
        /// <para>The authoritative check that the timeout actually cancels a long count is the large-partition request against the deployed API: a county partition of millions of rows is a production-scale figure, and asserting a machine-dependent cancellation here would be flaky.</para>
        /// </summary>
        [Fact(Skip = "Creates and drops a table. Point PostgreSQL_Table.conf at a scratch database before running.")]
        public async Task CountAsync_CommandTimeout()
        {
            if (!Create.IsAvailable(Enums.StorageMethod.Table, out ConnectionData? connectionData) || connectionData is null)
            {
                return;
            }

            await using NpgsqlConnection? npgsqlConnection = PostgreSQL.Create.NpgsqlConnection(connectionData);
            if (npgsqlConnection is null)
            {
                return;
            }

            await npgsqlConnection.OpenAsync();

            const string tableName = "xunit_scratch_countasync";
            const int rows = 1000;

            await using NpgsqlCommand npgsqlCommand_Setup = new($"DROP TABLE IF EXISTS {tableName}; CREATE TABLE {tableName} (id INT); INSERT INTO {tableName} SELECT generate_series(1, {rows});", npgsqlConnection);
            await npgsqlCommand_Setup.ExecuteNonQueryAsync();

            try
            {
                // A disabled timeout (0) and a normal timeout both must answer the exact row count: the parameter is accepted and the statement still runs to completion.
                long count_Disabled = await Query.CountAsync(npgsqlConnection, tableName, commandTimeout: 0);
                Assert.Equal(rows, count_Disabled);

                long count_Normal = await Query.CountAsync(npgsqlConnection, tableName, commandTimeout: 30);
                Assert.Equal(rows, count_Normal);
            }
            finally
            {
                await using NpgsqlCommand npgsqlCommand_Cleanup = new($"DROP TABLE IF EXISTS {tableName};", npgsqlConnection);
                await npgsqlCommand_Cleanup.ExecuteNonQueryAsync();
            }
        }
    }
}
