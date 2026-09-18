using DiGi.PostgreSQL.Classes;
using Npgsql;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.PostgreSQL.PartitionUniqueReference.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a pre-cancelled CancellationToken aborts TypeAsync at ExecuteReaderAsync instead of the commandTimeout/cancellationToken parameters added by ZiolkowskiJakub/DiGi.PostgreSQL#5 being accepted and silently dropped.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task TypeAsync_Cancellation()
        {
            if (!DiGi.PostgreSQL.xUnit.Create.IsAvailable(DiGi.PostgreSQL.Enums.StorageMethod.PartitionUniqueReference, out ConnectionData? connectionData) || connectionData is null)
            {
                return;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(connectionData);
            if (npgsqlConnection is null)
            {
                return;
            }

            await npgsqlConnection.OpenAsync();

            // The connection is open and the name is non-blank, so the call reaches ExecuteReaderAsync, where the pre-cancelled token must abort it.
            CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Query.TypeAsync(npgsqlConnection, "xunit_scratch", cancellationToken: cancellationTokenSource.Token));
        }

        /// <summary>
        /// Verifies that a pre-cancelled CancellationToken aborts TypeIdAsync at ExecuteReaderAsync instead of the commandTimeout/cancellationToken parameters added by ZiolkowskiJakub/DiGi.PostgreSQL#5 being accepted and silently dropped.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task TypeIdAsync_Cancellation()
        {
            if (!DiGi.PostgreSQL.xUnit.Create.IsAvailable(DiGi.PostgreSQL.Enums.StorageMethod.PartitionUniqueReference, out ConnectionData? connectionData) || connectionData is null)
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

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Query.TypeIdAsync(npgsqlConnection, typeof(string), cancellationToken: cancellationTokenSource.Token));
        }
    }
}
