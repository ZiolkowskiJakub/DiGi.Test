using DiGi.PostgreSQL.Classes;
using Npgsql;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.PostgreSQL.UniqueReference.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a pre-cancelled CancellationToken aborts PartitionIdAsync at ExecuteReaderAsync instead of the commandTimeout/cancellationToken parameters added by ZiolkowskiJakub/DiGi.PostgreSQL#5 being accepted and silently dropped.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task PartitionIdAsync_Cancellation()
        {
            if (!DiGi.PostgreSQL.xUnit.Create.IsAvailable(DiGi.PostgreSQL.Enums.StorageMethod.UniqueReference, out ConnectionData? connectionData) || connectionData is null)
            {
                return;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(connectionData);
            if (npgsqlConnection is null)
            {
                return;
            }

            await npgsqlConnection.OpenAsync();

            // The connection is open and the type is non-null, so the call reaches ExecuteReaderAsync, where the pre-cancelled token must abort it.
            CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Query.PartitionIdAsync(npgsqlConnection, typeof(string), cancellationToken: cancellationTokenSource.Token));
        }

        /// <summary>
        /// Verifies that a pre-cancelled CancellationToken aborts PartitionsAsync at ExecuteReaderAsync instead of the commandTimeout/cancellationToken parameters added by ZiolkowskiJakub/DiGi.PostgreSQL#5 being accepted and silently dropped.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task PartitionsAsync_Cancellation()
        {
            if (!DiGi.PostgreSQL.xUnit.Create.IsAvailable(DiGi.PostgreSQL.Enums.StorageMethod.UniqueReference, out ConnectionData? connectionData) || connectionData is null)
            {
                return;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(connectionData);
            if (npgsqlConnection is null)
            {
                return;
            }

            await npgsqlConnection.OpenAsync();

            // The connection is open and the type is non-null, so the call reaches ExecuteReaderAsync, where the pre-cancelled token must abort it.
            CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Query.PartitionsAsync(npgsqlConnection, typeof(string), cancellationToken: cancellationTokenSource.Token));
        }
    }
}
