using DiGi.Core.IO.Table.Classes;
using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Table.xUnit.Classes;
using Npgsql;

namespace DiGi.PostgreSQL.Table.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that GetHistogramSummaryAsync answers null (the empty-result contract the controller maps to 404 NotFound) when the column is entirely NULL in the scope, instead of throwing on the NULL bucket.
        /// <para>Reproduces ZiolkowskiJakub/DiGi.PostgreSQL#6: the unmodified reader read the NULL bucket with an unguarded GetInt32(0), which the endpoint surfaced as a 500. A fully-populated control proves the column is actually resolved, so this null is the empty-scope answer and not a whitelist rejection.</para>
        /// <para>Figures describe the development database resolved from <c>user files/PostgreSQL_Table.conf</c> (localhost), never production.</para>
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task GetHistogramSummaryAsync_AllNullScope_ReturnsNull()
        {
            if (!PostgreSQL.xUnit.Create.IsAvailable(PostgreSQL.Enums.StorageMethod.Table, out ConnectionData? connectionData))
            {
                return;
            }

            BaseTablePostgreSQLConverter testConverter = new(connectionData);

            await SeedHistogramTableAsync(connectionData, testConverter, [(double?)null, null, null]);
            try
            {
                await using NpgsqlConnection? npgsqlConnection = PostgreSQL.Create.NpgsqlConnection(connectionData);
                if (npgsqlConnection is null)
                {
                    return;
                }

                await npgsqlConnection.OpenAsync();

                System.Text.Json.Nodes.JsonArray? histogramArray = await testConverter.GetHistogramSummaryAsync<Core.IO.Table.Classes.Column>(npgsqlConnection, "value", bucketCount: 10);
                Assert.Null(histogramArray);
            }
            finally
            {
                await UnseedHistogramTableAsync(connectionData, testConverter);
            }
        }

        /// <summary>
        /// Verifies that a partially-NULL scope returns the non-NULL values' buckets without throwing, and that the per-bucket counts sum to the non-NULL row count (NULLs excluded, not counted).
        /// <para>The unmodified code threw on the NULL-bucket group here as well, not only for an entirely-NULL scope, because the NULL group sorts first and is the first row read.</para>
        /// <para>Figures describe the development database resolved from <c>user files/PostgreSQL_Table.conf</c> (localhost), never production.</para>
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task GetHistogramSummaryAsync_PartialNullScope_ReturnsNonNullBuckets()
        {
            if (!PostgreSQL.xUnit.Create.IsAvailable(PostgreSQL.Enums.StorageMethod.Table, out ConnectionData? connectionData))
            {
                return;
            }

            BaseTablePostgreSQLConverter testConverter = new(connectionData);

            // Three non-NULL values interleaved with two NULLs: the histogram must cover exactly the three non-NULL rows.
            await SeedHistogramTableAsync(connectionData, testConverter, [10.0, null, 20.0, null, 30.0]);
            try
            {
                await using NpgsqlConnection? npgsqlConnection = PostgreSQL.Create.NpgsqlConnection(connectionData);
                if (npgsqlConnection is null)
                {
                    return;
                }

                await npgsqlConnection.OpenAsync();

                System.Text.Json.Nodes.JsonArray? histogramArray = await testConverter.GetHistogramSummaryAsync<Core.IO.Table.Classes.Column>(npgsqlConnection, "value", bucketCount: 10);
                Assert.NotNull(histogramArray);
                Assert.True(histogramArray.Count > 0);
                Assert.Equal(3, SumBucketCounts(histogramArray));
            }
            finally
            {
                await UnseedHistogramTableAsync(connectionData, testConverter);
            }
        }

        /// <summary>
        /// Control for the empty-scope fix: a fully-populated scope still returns a non-empty histogram whose bucket counts sum to the row count, proving the data path is intact and that the null in the all-NULL fact is the empty-scope answer rather than a whitelist rejection.
        /// <para>Figures describe the development database resolved from <c>user files/PostgreSQL_Table.conf</c> (localhost), never production.</para>
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task GetHistogramSummaryAsync_FullyPopulatedScope_ReturnsBuckets()
        {
            if (!PostgreSQL.xUnit.Create.IsAvailable(PostgreSQL.Enums.StorageMethod.Table, out ConnectionData? connectionData))
            {
                return;
            }

            BaseTablePostgreSQLConverter testConverter = new(connectionData);

            await SeedHistogramTableAsync(connectionData, testConverter, [1.0, 2.0, 3.0]);
            try
            {
                await using NpgsqlConnection? npgsqlConnection = PostgreSQL.Create.NpgsqlConnection(connectionData);
                if (npgsqlConnection is null)
                {
                    return;
                }

                await npgsqlConnection.OpenAsync();

                System.Text.Json.Nodes.JsonArray? histogramArray = await testConverter.GetHistogramSummaryAsync<Core.IO.Table.Classes.Column>(npgsqlConnection, "value", bucketCount: 10);
                Assert.NotNull(histogramArray);
                Assert.True(histogramArray.Count > 0);
                Assert.Equal(3, SumBucketCounts(histogramArray));
            }
            finally
            {
                await UnseedHistogramTableAsync(connectionData, testConverter);
            }
        }

        /// <summary>
        /// Verifies the partition-scope path: a partition whose column is entirely NULL answers null (the 404 contract) while a sibling partition with data answers buckets, proving the county filter and the empty-scope answer work together.
        /// <para>Mirrors ZiolkowskiJakub/DiGi.PostgreSQL#6, whose reproduction is a county part (partition) whose <c>predicted_year_built</c> is all NULL.</para>
        /// <para>Figures describe the development database resolved from <c>user files/PostgreSQL_Table.conf</c> (localhost), never production.</para>
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task GetHistogramSummaryAsync_Partitioned_AllNullPartition_ReturnsNull()
        {
            if (!PostgreSQL.xUnit.Create.IsAvailable(PostgreSQL.Enums.StorageMethod.Table, out ConnectionData? connectionData))
            {
                return;
            }

            HistogramPartitionTablePostgreSQLConverter testConverter = new(connectionData);

            // Partition 5 is entirely NULL; partition 7 carries data and acts as the control.
            await SeedHistogramPartitionTableAsync(connectionData, testConverter, [(5, (double?)null), (5, null), (5, null), (7, 1.0), (7, 2.0), (7, 3.0)]);
            try
            {
                await using NpgsqlConnection? npgsqlConnection = PostgreSQL.Create.NpgsqlConnection(connectionData);
                if (npgsqlConnection is null)
                {
                    return;
                }

                await npgsqlConnection.OpenAsync();

                System.Text.Json.Nodes.JsonArray? allNullPartition = await testConverter.GetHistogramSummaryAsync<Core.IO.Table.Classes.Column>(npgsqlConnection, "value", bucketCount: 10, partitionValue: 5);
                Assert.Null(allNullPartition);

                System.Text.Json.Nodes.JsonArray? dataPartition = await testConverter.GetHistogramSummaryAsync<Core.IO.Table.Classes.Column>(npgsqlConnection, "value", bucketCount: 10, partitionValue: 7);
                Assert.NotNull(dataPartition);
                Assert.Equal(3, SumBucketCounts(dataPartition));
            }
            finally
            {
                await UnseedHistogramPartitionTableAsync(connectionData, testConverter);
            }
        }

        /// <summary>
        /// Creates the basetable scratch table (an int primary key plus the <c>value</c> double column under test) and seeds the supplied values, dropping any leftovers first so every fact starts from the same state.
        /// </summary>
        /// <param name="connectionData">The connection data for the development database.</param>
        /// <param name="testConverter">The scratch-table converter to seed through.</param>
        /// <param name="values">The <c>value</c> column entries; a null entry is stored as a database NULL.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static async Task SeedHistogramTableAsync(ConnectionData connectionData, BaseTablePostgreSQLConverter testConverter, double?[] values)
        {
            await PostgreSQL.Modify.RemoveTableAsync(connectionData, testConverter.TableName);
            await PostgreSQL.Modify.RemoveTableAsync(connectionData, Constants.TableName.Columns);

            Core.IO.Table.Classes.Table table = new();
            table.AddColumn("Column_1", typeof(int));
            table.AddColumn("value", typeof(double));

            int key = 1;
            foreach (double? value in values)
            {
                table.AddRow([key, value]);
                key++;
            }

            Assert.True(await testConverter.PushAsync(table));
        }

        /// <summary>
        /// Removes the basetable scratch table and its columns metadata after a GetHistogramSummaryAsync fact, restoring the development database state.
        /// </summary>
        /// <param name="connectionData">The connection data for the development database.</param>
        /// <param name="testConverter">The scratch-table converter whose table is removed.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static async Task UnseedHistogramTableAsync(ConnectionData connectionData, BaseTablePostgreSQLConverter testConverter)
        {
            await PostgreSQL.Modify.RemoveTableAsync(connectionData, testConverter.TableName);
            await PostgreSQL.Modify.RemoveTableAsync(connectionData, Constants.TableName.Columns);
        }

        /// <summary>
        /// Creates the partitioned scratch table (LIST partitioned on <c>county</c>) and seeds the supplied (county, value) rows, dropping any leftovers first so every fact starts from the same state.
        /// </summary>
        /// <param name="connectionData">The connection data for the development database.</param>
        /// <param name="testConverter">The partitioned scratch-table converter to seed through.</param>
        /// <param name="rows">The (county, value) entries; a null value is stored as a database NULL.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static async Task SeedHistogramPartitionTableAsync(ConnectionData connectionData, HistogramPartitionTablePostgreSQLConverter testConverter, (int county, double? value)[] rows)
        {
            await PostgreSQL.Modify.RemoveTableAsync(connectionData, testConverter.TableName);
            await PostgreSQL.Modify.RemoveTableAsync(connectionData, Constants.TableName.Columns);

            Core.IO.Table.Classes.Table table = new();
            table.AddColumn(HistogramPartitionTablePostgreSQLConverter.Column_County);
            table.AddColumn(HistogramPartitionTablePostgreSQLConverter.Column_Row);
            table.AddColumn(HistogramPartitionTablePostgreSQLConverter.Column_Value);

            int rowKey = 1;
            foreach ((int county, double? value) row in rows)
            {
                table.AddRow([row.county, rowKey, row.value]);
                rowKey++;
            }

            Assert.True(await testConverter.PushAsync(table));
        }

        /// <summary>
        /// Removes the partitioned scratch table (and its partitions) and its columns metadata after a partitioned GetHistogramSummaryAsync fact.
        /// </summary>
        /// <param name="connectionData">The connection data for the development database.</param>
        /// <param name="testConverter">The partitioned scratch-table converter whose table is removed.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static async Task UnseedHistogramPartitionTableAsync(ConnectionData connectionData, HistogramPartitionTablePostgreSQLConverter testConverter)
        {
            await PostgreSQL.Modify.RemoveTableAsync(connectionData, testConverter.TableName);
            await PostgreSQL.Modify.RemoveTableAsync(connectionData, Constants.TableName.Columns);
        }

        /// <summary>
        /// Sums the per-bucket <c>count</c> values of a histogram result.
        /// </summary>
        /// <param name="histogramArray">The histogram result to sum, or null.</param>
        /// <returns>The total of the bucket counts, or 0 when the result is null.</returns>
        private static long SumBucketCounts(System.Text.Json.Nodes.JsonArray? histogramArray)
        {
            if (histogramArray is null)
            {
                return 0;
            }

            long total = 0;
            foreach (System.Text.Json.Nodes.JsonNode? jsonNode_Bucket in histogramArray)
            {
                if (jsonNode_Bucket is not System.Text.Json.Nodes.JsonObject jsonObject_Bucket)
                {
                    continue;
                }

                if (jsonObject_Bucket["count"] is System.Text.Json.Nodes.JsonNode countNode)
                {
                    total += countNode.GetValue<long>();
                }
            }

            return total;
        }
    }
}
