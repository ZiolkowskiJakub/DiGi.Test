using DiGi.PostgreSQL.Classes;
using Npgsql;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a pre-cancelled CancellationToken aborts both collection overloads of PartitionsAsync at ExecuteReaderAsync instead of being silently dropped.
        /// <para>This is a pin, not a reproducer: ExecuteReaderAsync already receives the token before the fix, so this fact is green both before and after the fix. It pins the token contract of both overloads, including the names overload whose loop stage the reproducer below does not cover. The reader-loop defect of ZiolkowskiJakub/DiGi.PostgreSQL#3 is reproduced by <see cref="PartitionsAsync_Cancellation_ReaderLoop"/>.</para>
        /// <para>Figures describe the development database resolved from <c>user files/PostgreSQL_PartitionReference.conf</c> (localhost), never production.</para>
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task PartitionsAsync_Cancellation()
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

            // The connection is open and each request is non-empty, so every call reaches ExecuteReaderAsync, where the pre-cancelled token must abort it.
            CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Query.PartitionsAsync(npgsqlConnection, [1], cancellationToken: cancellationTokenSource.Token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Query.PartitionsAsync(npgsqlConnection, ["x"], cancellationToken: cancellationTokenSource.Token));
        }

        /// <summary>
        /// Reproduces ZiolkowskiJakub/DiGi.PostgreSQL#3: with a token cancelled partway through a long read, PartitionsAsync must abort in the row-reading loop.
        /// <para>Before the fix the loop calls ReadAsync() with no token, so the call returns all 32 767 seeded rows instead of throwing and this fact is red; after the fix ReadAsync(cancellationToken) throws on the next row and the fact is green. The control leg with a live token proves the same request completes and returns exactly the seeded rows.</para>
        /// <para>The seed gives every name a 4096 character payload, which makes the client-side row loop dominate the read - measured warm on the development database: 126-171 ms of loop in a 132-181 ms total, the server-side reader phase only 6-14 ms - with about 10% variance between runs. The cancellation at 40% of the measured warm control total then lands inside the loop, which keeps running for well over 40% of the total, so the throw can only come from the loop honouring the token. At this cancellation point the unmodified code shape completed with all 32 767 rows on 12 of 12 runs and the fixed shape threw OperationCanceledException on 12 of 12.</para>
        /// <para>If the warm control total is under 100 ms the cancellation window is too small to attribute a throw to the loop, and the fact skips; the pin above still carries the token contract.</para>
        /// <para>Figures describe the development database resolved from <c>user files/PostgreSQL_PartitionReference.conf</c> (localhost), never production.</para>
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task PartitionsAsync_Cancellation_ReaderLoop()
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

            const int firstId = 1;
            const int lastId = 32767; // partitions.id is the primary key (smallint); the seed occupies its full range
            const int rows = lastId - firstId + 1; // 32 767
            const int namePadding = 4096; // a heavy name payload makes the client-side row loop dominate the read, so the cancellation lands inside it

            List<short> partitionIds = [];
            for (int i = firstId; i <= lastId; i++)
            {
                partitionIds.Add((short)i);
            }

            // Drop leftovers of an interrupted earlier run, then seed the full range with the padded names.
            await using NpgsqlCommand npgsqlCommand_Setup = new(
                $@"DELETE FROM partitions WHERE name LIKE 'xunit_scratch_%';
                    INSERT INTO partitions (id, name, data_type, created_at) OVERRIDING SYSTEM VALUE
                    SELECT g, 'xunit_scratch_' || g || repeat('x', {namePadding}), 1, now()
                    FROM generate_series({firstId}, {lastId}) g;", npgsqlConnection);
            await npgsqlCommand_Setup.ExecuteNonQueryAsync();

            try
            {
                // Warm-up pass: trigger JIT and warm the server-side plan and buffer caches so the measured control total represents the cancellation leg's path.
                _ = await Query.PartitionsAsync(npgsqlConnection, partitionIds);

                // Control leg: the live token reads to completion and returns exactly the seeded rows, and the measured total anchors the cancellation delay.
                Stopwatch stopwatch = Stopwatch.StartNew();
                List<Partition>? partitions_Control = await Query.PartitionsAsync(npgsqlConnection, partitionIds);
                long totalMilliseconds = stopwatch.ElapsedMilliseconds;

                Assert.NotNull(partitions_Control);
                Assert.Equal(rows, partitions_Control.Count);

                // Below this total the read is too fast for a mid-loop cancellation to be observable, and the pin above carries the token contract.
                if (totalMilliseconds < 100)
                {
                    throw new SkipException($"Warm control total {totalMilliseconds} ms is under 100 ms; the mid-loop cancellation window is not observable on this machine.");
                }

                CancellationTokenSource cancellationTokenSource = new();
                Task<List<Partition>?> attempt = Query.PartitionsAsync(npgsqlConnection, partitionIds, cancellationToken: cancellationTokenSource.Token);

                // Cancel at 40% of the warm control total: inside the row loop (the reader phase is a few ms), with the loop still running for well over 40% of the total.
                await Task.Delay(TimeSpan.FromMilliseconds(totalMilliseconds * 4 / 10));
                cancellationTokenSource.Cancel();

                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => attempt);
            }
            finally
            {
                await using NpgsqlCommand npgsqlCommand_Cleanup = new("DELETE FROM partitions WHERE name LIKE 'xunit_scratch_%';", npgsqlConnection);
                await npgsqlCommand_Cleanup.ExecuteNonQueryAsync();
            }
        }
    }
}
