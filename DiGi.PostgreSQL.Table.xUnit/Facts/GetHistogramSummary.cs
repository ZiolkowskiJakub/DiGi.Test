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
        /// Verifies the equal-count bucketing (ZiolkowskiJakub/DiGi.PostgreSQL#7): 25 skewed values asked for 5 buckets answer exactly 5 buckets of 5 rows each, numbered 1 to 5, ascending and non-overlapping, with the actual minimum and maximum of each bucket's rows - and the equal-width control on the same seed files 21 of the 25 rows into its first bucket, proving the two rules differ where a skewed column needs them to.
        /// <para>Figures describe the development database resolved from <c>user files/PostgreSQL_Table.conf</c> (localhost), never production.</para>
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task GetHistogramSummaryAsync_EqualCount_SkewedValues_EqualRowsPerBucket()
        {
            if (!PostgreSQL.xUnit.Create.IsAvailable(PostgreSQL.Enums.StorageMethod.Table, out ConnectionData? connectionData))
            {
                return;
            }

            BaseTablePostgreSQLConverter testConverter = new(connectionData);

            // 1 .. 20 then 100, 200, 300, 400, 500: the body sits in the first fifth of the span, the tail spreads over the rest.
            double?[] values = new double?[25];
            for (int i = 0; i < 20; i++)
            {
                values[i] = i + 1;
            }

            for (int i = 0; i < 5; i++)
            {
                values[20 + i] = 100 * (i + 1);
            }

            await SeedHistogramTableAsync(connectionData, testConverter, values);
            try
            {
                await using NpgsqlConnection? npgsqlConnection = PostgreSQL.Create.NpgsqlConnection(connectionData);
                if (npgsqlConnection is null)
                {
                    return;
                }

                await npgsqlConnection.OpenAsync();

                System.Text.Json.Nodes.JsonArray? equalCount = await testConverter.GetHistogramSummaryAsync<Core.IO.Table.Classes.Column>(npgsqlConnection, "value", bucketCount: 5, histogramBucketing: Enums.HistogramBucketing.EqualCount);
                Assert.NotNull(equalCount);
                Assert.Equal(5, equalCount.Count);
                Assert.Equal(25, SumBucketCounts(equalCount));

                (double rangeStart, double rangeEnd)[] expectedBounds = [(1, 5), (6, 10), (11, 15), (16, 20), (100, 500)];
                for (int i = 0; i < 5; i++)
                {
                    (int bucket, double rangeStart, double rangeEnd, long count) row = BucketRow(equalCount[i]);
                    Assert.Equal(i + 1, row.bucket);
                    Assert.Equal(5, row.count);
                    Assert.Equal(expectedBounds[i].rangeStart, row.rangeStart);
                    Assert.Equal(expectedBounds[i].rangeEnd, row.rangeEnd);
                }

                // The control: equal width over [1, 500] puts 1 .. 20 and 100 into the first fifth of the span.
                System.Text.Json.Nodes.JsonArray? equalWidth = await testConverter.GetHistogramSummaryAsync<Core.IO.Table.Classes.Column>(npgsqlConnection, "value", bucketCount: 5);
                Assert.NotNull(equalWidth);
                Assert.Equal(25, SumBucketCounts(equalWidth));
                Assert.Equal(21, BucketRow(equalWidth[0]).count);
            }
            finally
            {
                await UnseedHistogramTableAsync(connectionData, testConverter);
            }
        }

        /// <summary>
        /// Verifies that equal-count bucketing with fewer rows than buckets answers one bucket per row (ntile numbers 1 .. rows), every bucket a single value with its start equal to its end, and that an all-NULL scope still answers null under equal count (the #6 contract holds for both rules).
        /// <para>Figures describe the development database resolved from <c>user files/PostgreSQL_Table.conf</c> (localhost), never production.</para>
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task GetHistogramSummaryAsync_EqualCount_FewerRowsThanBuckets_OneRowPerBucket()
        {
            if (!PostgreSQL.xUnit.Create.IsAvailable(PostgreSQL.Enums.StorageMethod.Table, out ConnectionData? connectionData))
            {
                return;
            }

            BaseTablePostgreSQLConverter testConverter = new(connectionData);

            await SeedHistogramTableAsync(connectionData, testConverter, [30.0, null, 10.0, 20.0]);
            try
            {
                await using NpgsqlConnection? npgsqlConnection = PostgreSQL.Create.NpgsqlConnection(connectionData);
                if (npgsqlConnection is null)
                {
                    return;
                }

                await npgsqlConnection.OpenAsync();

                System.Text.Json.Nodes.JsonArray? histogramArray = await testConverter.GetHistogramSummaryAsync<Core.IO.Table.Classes.Column>(npgsqlConnection, "value", bucketCount: 10, histogramBucketing: Enums.HistogramBucketing.EqualCount);
                Assert.NotNull(histogramArray);
                Assert.Equal(3, histogramArray.Count);
                Assert.Equal(3, SumBucketCounts(histogramArray));

                double[] expectedValues = [10, 20, 30];
                for (int i = 0; i < 3; i++)
                {
                    (int bucket, double rangeStart, double rangeEnd, long count) row = BucketRow(histogramArray[i]);
                    Assert.Equal(i + 1, row.bucket);
                    Assert.Equal(1, row.count);
                    Assert.Equal(expectedValues[i], row.rangeStart);
                    Assert.Equal(expectedValues[i], row.rangeEnd);
                }
            }
            finally
            {
                await UnseedHistogramTableAsync(connectionData, testConverter);
            }

            await SeedHistogramTableAsync(connectionData, testConverter, [(double?)null, null]);
            try
            {
                await using NpgsqlConnection? npgsqlConnection = PostgreSQL.Create.NpgsqlConnection(connectionData);
                if (npgsqlConnection is null)
                {
                    return;
                }

                await npgsqlConnection.OpenAsync();

                System.Text.Json.Nodes.JsonArray? histogramArray = await testConverter.GetHistogramSummaryAsync<Core.IO.Table.Classes.Column>(npgsqlConnection, "value", bucketCount: 10, histogramBucketing: Enums.HistogramBucketing.EqualCount);
                Assert.Null(histogramArray);
            }
            finally
            {
                await UnseedHistogramTableAsync(connectionData, testConverter);
            }
        }

        /// <summary>
        /// Verifies the partition-scope path under equal-count bucketing: the partition filter is honoured (the sibling partition's rows are not ranked into the buckets), a tie spanning two buckets is answered with the same value as one bucket's end and the next bucket's start, and an all-NULL partition answers null.
        /// <para>Figures describe the development database resolved from <c>user files/PostgreSQL_Table.conf</c> (localhost), never production.</para>
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task GetHistogramSummaryAsync_EqualCount_Partitioned_HonoursPartitionAndTies()
        {
            if (!PostgreSQL.xUnit.Create.IsAvailable(PostgreSQL.Enums.StorageMethod.Table, out ConnectionData? connectionData))
            {
                return;
            }

            HistogramPartitionTablePostgreSQLConverter testConverter = new(connectionData);

            // Partition 7: four rows, three of them the value 1 - two buckets split the tie. Partition 5 is entirely NULL; partition 9 carries values that must not leak into 7.
            await SeedHistogramPartitionTableAsync(connectionData, testConverter, [(7, 1.0), (7, 1.0), (7, 1.0), (7, 2.0), (5, (double?)null), (5, null), (9, 500.0), (9, 600.0)]);
            try
            {
                await using NpgsqlConnection? npgsqlConnection = PostgreSQL.Create.NpgsqlConnection(connectionData);
                if (npgsqlConnection is null)
                {
                    return;
                }

                await npgsqlConnection.OpenAsync();

                System.Text.Json.Nodes.JsonArray? dataPartition = await testConverter.GetHistogramSummaryAsync<Core.IO.Table.Classes.Column>(npgsqlConnection, "value", bucketCount: 2, partitionValue: 7, histogramBucketing: Enums.HistogramBucketing.EqualCount);
                Assert.NotNull(dataPartition);
                Assert.Equal(2, dataPartition.Count);
                Assert.Equal(4, SumBucketCounts(dataPartition));

                (int bucket, double rangeStart, double rangeEnd, long count) row_1 = BucketRow(dataPartition[0]);
                (int bucket, double rangeStart, double rangeEnd, long count) row_2 = BucketRow(dataPartition[1]);
                Assert.Equal(2, row_1.count);
                Assert.Equal(1, row_1.rangeStart);
                Assert.Equal(1, row_1.rangeEnd);
                Assert.Equal(2, row_2.count);
                Assert.Equal(1, row_2.rangeStart);
                Assert.Equal(2, row_2.rangeEnd);

                System.Text.Json.Nodes.JsonArray? allNullPartition = await testConverter.GetHistogramSummaryAsync<Core.IO.Table.Classes.Column>(npgsqlConnection, "value", bucketCount: 2, partitionValue: 5, histogramBucketing: Enums.HistogramBucketing.EqualCount);
                Assert.Null(allNullPartition);
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

        /// <summary>
        /// Reads the four fields of one histogram bucket row.
        /// </summary>
        /// <param name="jsonNode_Bucket">The bucket row, a <c>{bucket, rangeStart, rangeEnd, count}</c> object.</param>
        /// <returns>The bucket ordinal, its actual value bounds and its row count.</returns>
        private static (int bucket, double rangeStart, double rangeEnd, long count) BucketRow(System.Text.Json.Nodes.JsonNode? jsonNode_Bucket)
        {
            System.Text.Json.Nodes.JsonObject jsonObject_Bucket = Assert.IsType<System.Text.Json.Nodes.JsonObject>(jsonNode_Bucket);

            return (
                jsonObject_Bucket["bucket"]!.GetValue<int>(),
                jsonObject_Bucket["rangeStart"]!.GetValue<double>(),
                jsonObject_Bucket["rangeEnd"]!.GetValue<double>(),
                jsonObject_Bucket["count"]!.GetValue<long>());
        }
    }
}
