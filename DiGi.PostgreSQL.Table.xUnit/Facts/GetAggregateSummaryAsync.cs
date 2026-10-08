using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Table.xUnit.Classes;
using Npgsql;

namespace DiGi.PostgreSQL.Table.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that SinglevalueAggregateFunction.Count answers the number of non-null cells of the requested column instead of the scope's row count, on a partially-NULL column where the two answers differ.
        /// <para>Reproduces ZiolkowskiJakub/DiGi.PostgreSQL#9: the singlevalue switch built COUNT(*), so a sparse column reported the row count of the scope and every requested column answered the same number.</para>
        /// <para>Figures describe the development database resolved from <c>user files/PostgreSQL_Table.conf</c> (localhost), never production.</para>
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task GetAggregateSummaryAsync_Count_SparseColumn_CountsNonNullCells()
        {
            if (!PostgreSQL.xUnit.Create.IsAvailable(PostgreSQL.Enums.StorageMethod.Table, out ConnectionData? connectionData))
            {
                return;
            }

            BaseTablePostgreSQLConverter testConverter = new(connectionData);

            // Three non-NULL values interleaved with two NULLs: COUNT(column) must answer 3 while the scope holds 5 rows.
            await SeedHistogramTableAsync(connectionData, testConverter, [10.0, null, 20.0, null, 30.0]);
            try
            {
                await using NpgsqlConnection? npgsqlConnection = PostgreSQL.Create.NpgsqlConnection(connectionData);
                if (npgsqlConnection is null)
                {
                    return;
                }

                await npgsqlConnection.OpenAsync();

                await using NpgsqlCommand npgsqlCommand_RowCount = new($"SELECT COUNT(*) FROM \"{testConverter.TableName}\"", npgsqlConnection);
                object? resultValue_RowCount = await npgsqlCommand_RowCount.ExecuteScalarAsync();
                Assert.NotNull(resultValue_RowCount);

                long rowCount = System.Convert.ToInt64(resultValue_RowCount);

                System.Text.Json.Nodes.JsonNode? jsonNode_Count = await testConverter.GetAggregateSummaryAsync<Core.IO.Table.Classes.Column>(npgsqlConnection, "value", PostgreSQL.Table.Enums.SinglevalueAggregateFunction.Count);

                Assert.NotNull(jsonNode_Count);
                Assert.Equal(5L, rowCount);
                Assert.Equal(3L, jsonNode_Count.GetValue<long>());
                Assert.NotEqual(rowCount, jsonNode_Count.GetValue<long>());
            }
            finally
            {
                await UnseedHistogramTableAsync(connectionData, testConverter);
            }
        }
    }
}
