using DiGi.PostgreSQL.Classes;
using Npgsql;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a pre-cancelled CancellationToken aborts ColumnNamesAsync at ExecuteReaderAsync instead of being silently dropped.
        /// <para>This is a pin, not a reproducer: ExecuteReaderAsync already receives the token before the fix, so this fact is green both before and after the fix. The tokenless ReadAsync() in the loop of ZiolkowskiJakub/DiGi.PostgreSQL#3 is the same defect class that <see cref="PartitionsAsync_Cancellation_ReaderLoop"/> reproduces for PartitionsAsync.</para>
        /// <para>Figures describe the development database resolved from <c>user files/PostgreSQL_PartitionReference.conf</c> (localhost), never production.</para>
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task ColumnNamesAsync_Cancellation()
        {
            if (!Create.IsAvailable(Enums.StorageMethod.PartitionReference, out ConnectionData? connectionData) || connectionData is null)
            {
                return;
            }

            await using NpgsqlConnection? npgsqlConnection = PostgreSQL.Create.NpgsqlConnection(connectionData);
            if (npgsqlConnection is null)
            {
                return;
            }

            await npgsqlConnection.OpenAsync();

            // The connection is open and the table name is non-blank, so the call reaches ExecuteReaderAsync, where the pre-cancelled token must abort it.
            CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Query.ColumnNamesAsync(npgsqlConnection, "partitions", cancellationToken: cancellationTokenSource.Token));
        }
    }
}
