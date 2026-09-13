using DiGi.Core.IO.Table.Classes;
using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Table.xUnit.Classes;
using Npgsql;

namespace DiGi.PostgreSQL.Table.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a pre-cancelled CancellationToken aborts the connection-owning GetColumnsByUniqueIdsAsync at OpenAsync instead of being silently dropped.
        /// <para>Reproduces ZiolkowskiJakub/DiGi.GIS.WebAPI#33: the unmodified converter opens the connection and executes the query with no token, so the call completes instead of throwing.</para>
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task GetColumnsByUniqueIdsAsync_Cancellation_OwningConnection()
        {
            if (!PostgreSQL.xUnit.Create.IsAvailable(PostgreSQL.Enums.StorageMethod.Table, out ConnectionData? connectionData))
            {
                return;
            }

            BaseTablePostgreSQLConverter testTablePostgreSQLConverter = new(connectionData);

            await SeedBasetableAsync(connectionData, testTablePostgreSQLConverter);
            try
            {
                CancellationTokenSource cancellationTokenSource = new();
                cancellationTokenSource.Cancel();

                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => testTablePostgreSQLConverter.GetColumnsByUniqueIdsAsync(cancellationToken: cancellationTokenSource.Token));
            }
            finally
            {
                await UnseedBasetableAsync(connectionData, testTablePostgreSQLConverter);
            }
        }

        /// <summary>
        /// Verifies that a pre-cancelled CancellationToken aborts the NpgsqlConnection overload of GetColumnsByUniqueIdsAsync at ExecuteReaderAsync instead of being silently dropped.
        /// <para>Reproduces ZiolkowskiJakub/DiGi.GIS.WebAPI#33: the unmodified chain reads with ExecuteReaderAsync() and ReadAsync() with no token, so the call returns rows instead of throwing.</para>
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task GetColumnsByUniqueIdsAsync_Cancellation_ProvidedConnection()
        {
            if (!PostgreSQL.xUnit.Create.IsAvailable(PostgreSQL.Enums.StorageMethod.Table, out ConnectionData? connectionData))
            {
                return;
            }

            BaseTablePostgreSQLConverter testTablePostgreSQLConverter = new(connectionData);

            await SeedBasetableAsync(connectionData, testTablePostgreSQLConverter);
            try
            {
                await using NpgsqlConnection? npgsqlConnection = PostgreSQL.Create.NpgsqlConnection(connectionData);
                if (npgsqlConnection is null)
                {
                    return;
                }

                await npgsqlConnection.OpenAsync();

                CancellationTokenSource cancellationTokenSource = new();
                cancellationTokenSource.Cancel();

                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => testTablePostgreSQLConverter.GetColumnsByUniqueIdsAsync(npgsqlConnection, cancellationToken: cancellationTokenSource.Token));
            }
            finally
            {
                await UnseedBasetableAsync(connectionData, testTablePostgreSQLConverter);
            }
        }

        /// <summary>
        /// Verifies that a caller-supplied commandTimeout reaches the command: while a second connection holds an ACCESS EXCLUSIVE lock on the columns table, a one second commandTimeout must abort the blocked query (Npgsql 10 surfaces it as an NpgsqlException wrapping a TimeoutException) instead of waiting out the 30 second Npgsql default.
        /// <para>Reproduces ZiolkowskiJakub/DiGi.GIS.WebAPI#33: the unmodified chain never assigns npgsqlCommand.CommandTimeout, so the query completes normally once the lock is released.</para>
        /// <para>Afterwards a generous commandTimeout completes the same query and returns the seeded columns, proving that a longer timeout lets the operation finish.</para>
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task GetColumnsByUniqueIdsAsync_CommandTimeout()
        {
            if (!PostgreSQL.xUnit.Create.IsAvailable(PostgreSQL.Enums.StorageMethod.Table, out ConnectionData? connectionData))
            {
                return;
            }

            BaseTablePostgreSQLConverter testTablePostgreSQLConverter = new(connectionData);

            await SeedBasetableAsync(connectionData, testTablePostgreSQLConverter);
            try
            {
                await using NpgsqlConnection? lockConnection = PostgreSQL.Create.NpgsqlConnection(connectionData);
                if (lockConnection is null)
                {
                    return;
                }

                await lockConnection.OpenAsync();

                await using NpgsqlTransaction? lockTransaction = await lockConnection.BeginTransactionAsync();

                await using (NpgsqlCommand lockCommand = new($"LOCK TABLE \"{Constants.TableName.Columns}\" IN ACCESS EXCLUSIVE MODE", lockConnection, lockTransaction))
                {
                    await lockCommand.ExecuteNonQueryAsync();
                }

                Task attempt = testTablePostgreSQLConverter.GetColumnsByUniqueIdsAsync(commandTimeout: 1);

                // Release the lock unconditionally after 3 seconds: with the fix in place the one second statement timeout has already fired while the lock was held; on the unmodified code (30 second Npgsql default) the query then completes normally once the lock is gone.
                const int lockHoldMilliseconds = 3000;
                await Task.Delay(lockHoldMilliseconds);

                await lockTransaction.RollbackAsync();

                // Npgsql 10 enforces CommandTimeout client-side: the call surfaces as an NpgsqlException wrapping a TimeoutException.
                NpgsqlException exception = await Assert.ThrowsAnyAsync<NpgsqlException>(() => attempt);
                Assert.IsAssignableFrom<TimeoutException>(exception.InnerException);

                // Control leg: with the lock released and a generous timeout the same query completes and returns the seeded columns.
                List<Column>? columns = await testTablePostgreSQLConverter.GetColumnsByUniqueIdsAsync(commandTimeout: 30);
                Assert.NotNull(columns);
                Assert.NotEmpty(columns);
            }
            finally
            {
                await UnseedBasetableAsync(connectionData, testTablePostgreSQLConverter);
            }
        }

        /// <summary>
        /// Seeds the basetable scratch table and its columns metadata for the GetColumnsByUniqueIdsAsync facts, dropping any leftovers first so every fact starts from the same state.
        /// </summary>
        /// <param name="connectionData">The connection data for the development database.</param>
        /// <param name="testTablePostgreSQLConverter">The scratch-table converter to seed through.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static async Task SeedBasetableAsync(ConnectionData connectionData, BaseTablePostgreSQLConverter testTablePostgreSQLConverter)
        {
            await PostgreSQL.Modify.RemoveTableAsync(connectionData, testTablePostgreSQLConverter.TableName);
            await PostgreSQL.Modify.RemoveTableAsync(connectionData, Constants.TableName.Columns);

            Core.IO.Table.Classes.Table table = new();
            table.AddColumn("Column_1", typeof(int));
            table.AddRow([1]);

            Assert.True(await testTablePostgreSQLConverter.PushAsync(table));
        }

        /// <summary>
        /// Removes the basetable scratch table and its columns metadata after a GetColumnsByUniqueIdsAsync fact, restoring the development database state.
        /// </summary>
        /// <param name="connectionData">The connection data for the development database.</param>
        /// <param name="testTablePostgreSQLConverter">The scratch-table converter whose table is removed.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static async Task UnseedBasetableAsync(ConnectionData connectionData, BaseTablePostgreSQLConverter testTablePostgreSQLConverter)
        {
            await PostgreSQL.Modify.RemoveTableAsync(connectionData, testTablePostgreSQLConverter.TableName);
            await PostgreSQL.Modify.RemoveTableAsync(connectionData, Constants.TableName.Columns);
        }
    }
}
