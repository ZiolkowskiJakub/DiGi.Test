using DiGi.Core.IO.Table.Classes;
using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Table.xUnit.Classes;
using Npgsql;

namespace DiGi.PostgreSQL.Table.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a caller-supplied commandTimeout reaches the DetectSeparatorAsync command: while a second connection holds an ACCESS EXCLUSIVE lock on the basetable, a one second commandTimeout must abort the blocked separator query (Npgsql 10 surfaces it as an NpgsqlException wrapping a TimeoutException) instead of waiting out the 30 second Npgsql default.
        /// <para>Reproduces ZiolkowskiJakub/DiGi.PostgreSQL#4: the unmodified DetectSeparatorAsync never assigns npgsqlCommand.CommandTimeout, so with the lock released after 3 seconds the query completes normally instead of throwing at one second. The control leg then shows a generous commandTimeout completes the same query and returns a detected separator.</para>
        /// <para>Figures describe the development database resolved from <c>user files/PostgreSQL_Table.conf</c> (localhost), never production.</para>
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task DetectSeparatorAsync_CommandTimeout()
        {
            if (!PostgreSQL.xUnit.Create.IsAvailable(PostgreSQL.Enums.StorageMethod.Table, out ConnectionData? connectionData))
            {
                return;
            }

            BaseTablePostgreSQLConverter testTablePostgreSQLConverter = new(connectionData);

            await SeedSeparatorTableAsync(connectionData, testTablePostgreSQLConverter, "a,b,c", "d,e", "g,h");
            try
            {
                await using NpgsqlConnection? lockConnection = PostgreSQL.Create.NpgsqlConnection(connectionData);
                if (lockConnection is null)
                {
                    return;
                }

                await lockConnection.OpenAsync();

                await using NpgsqlTransaction? lockTransaction = await lockConnection.BeginTransactionAsync();

                await using (NpgsqlCommand lockCommand = new($"LOCK TABLE \"{testTablePostgreSQLConverter.TableName}\" IN ACCESS EXCLUSIVE MODE", lockConnection, lockTransaction))
                {
                    await lockCommand.ExecuteNonQueryAsync();
                }

                await using NpgsqlConnection? workConnection = PostgreSQL.Create.NpgsqlConnection(connectionData);
                if (workConnection is null)
                {
                    await lockTransaction.RollbackAsync();
                    return;
                }

                await workConnection.OpenAsync();

                Task attempt = testTablePostgreSQLConverter.DetectSeparatorAsync(workConnection, "column_2", commandTimeout: 1);

                // Release the lock unconditionally after 3 seconds: with the fix in place the one second statement timeout has already fired while the lock was held; on the unmodified code (30 second Npgsql default) the query then completes normally once the lock is gone.
                const int lockHoldMilliseconds = 3000;
                await Task.Delay(lockHoldMilliseconds);

                await lockTransaction.RollbackAsync();

                // Npgsql 10 enforces CommandTimeout client-side: the call surfaces as an NpgsqlException wrapping a TimeoutException.
                NpgsqlException exception = await Assert.ThrowsAnyAsync<NpgsqlException>(() => attempt);
                Assert.IsAssignableFrom<TimeoutException>(exception.InnerException);

                // Control leg: with the lock released and a generous timeout the same query completes and returns a detected separator.
                string separator = await testTablePostgreSQLConverter.DetectSeparatorAsync(workConnection, "column_2", commandTimeout: 30);
                Assert.False(string.IsNullOrEmpty(separator));
            }
            finally
            {
                await UnseedSeparatorTableAsync(connectionData, testTablePostgreSQLConverter);
            }
        }

        /// <summary>
        /// Verifies that DetectSeparatorAsync returns the dominant separator for comma-, semicolon- and pipe-heavy data, proving the commandTimeout/cancellationToken refactor did not change the detection logic.
        /// <para>Figures describe the development database resolved from <c>user files/PostgreSQL_Table.conf</c> (localhost), never production.</para>
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task DetectSeparatorAsync_Detection()
        {
            if (!PostgreSQL.xUnit.Create.IsAvailable(PostgreSQL.Enums.StorageMethod.Table, out ConnectionData? connectionData))
            {
                return;
            }

            BaseTablePostgreSQLConverter testTablePostgreSQLConverter = new(connectionData);

            await using NpgsqlConnection? npgsqlConnection = PostgreSQL.Create.NpgsqlConnection(connectionData);
            if (npgsqlConnection is null)
            {
                return;
            }

            await npgsqlConnection.OpenAsync();

            try
            {
                await SeedSeparatorTableAsync(connectionData, testTablePostgreSQLConverter, "a,b,c", "d,e", "g,h,i");
                string separator_Comma = await testTablePostgreSQLConverter.DetectSeparatorAsync(npgsqlConnection, "column_2");
                Assert.Equal(",", separator_Comma);

                await SeedSeparatorTableAsync(connectionData, testTablePostgreSQLConverter, "a;b;c", "d;e", "f;g;h");
                string separator_Semi = await testTablePostgreSQLConverter.DetectSeparatorAsync(npgsqlConnection, "column_2");
                Assert.Equal(";", separator_Semi);

                await SeedSeparatorTableAsync(connectionData, testTablePostgreSQLConverter, "a|b|c", "d|e", "f|g|h");
                string separator_Pipe = await testTablePostgreSQLConverter.DetectSeparatorAsync(npgsqlConnection, "column_2");
                Assert.Equal("|", separator_Pipe);
            }
            finally
            {
                await UnseedSeparatorTableAsync(connectionData, testTablePostgreSQLConverter);
            }
        }

        /// <summary>
        /// Creates the basetable scratch table (an int primary key plus a string multi-value column) and seeds the supplied separator values, dropping any leftovers first so every fact starts from the same state.
        /// </summary>
        /// <param name="connectionData">The connection data for the development database.</param>
        /// <param name="testTablePostgreSQLConverter">The scratch-table converter to seed through.</param>
        /// <param name="row1">The value for the first row's multi-value column.</param>
        /// <param name="row2">The value for the second row's multi-value column.</param>
        /// <param name="row3">The value for the third row's multi-value column.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static async Task SeedSeparatorTableAsync(ConnectionData connectionData, BaseTablePostgreSQLConverter testTablePostgreSQLConverter, string row1, string row2, string row3)
        {
            await PostgreSQL.Modify.RemoveTableAsync(connectionData, testTablePostgreSQLConverter.TableName);
            await PostgreSQL.Modify.RemoveTableAsync(connectionData, Constants.TableName.Columns);

            Core.IO.Table.Classes.Table table = new();
            table.AddColumn("Column_1", typeof(int));
            table.AddColumn(new ExtendedColumn("Column_2", typeof(string), "Separator Column", "Multi-value column for separator detection"));
            table.AddRow([1, row1]);
            table.AddRow([2, row2]);
            table.AddRow([3, row3]);

            Assert.True(await testTablePostgreSQLConverter.PushAsync(table));
        }

        /// <summary>
        /// Removes the basetable scratch table and its columns metadata after a DetectSeparatorAsync fact, restoring the development database state.
        /// </summary>
        /// <param name="connectionData">The connection data for the development database.</param>
        /// <param name="testTablePostgreSQLConverter">The scratch-table converter whose table is removed.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static async Task UnseedSeparatorTableAsync(ConnectionData connectionData, BaseTablePostgreSQLConverter testTablePostgreSQLConverter)
        {
            await PostgreSQL.Modify.RemoveTableAsync(connectionData, testTablePostgreSQLConverter.TableName);
            await PostgreSQL.Modify.RemoveTableAsync(connectionData, Constants.TableName.Columns);
        }
    }
}
