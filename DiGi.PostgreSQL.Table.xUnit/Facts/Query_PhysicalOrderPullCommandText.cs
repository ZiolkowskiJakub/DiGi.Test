using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Table.xUnit.Classes;
using Npgsql;
using System;
using System.Linq;
using System.Text;

namespace DiGi.PostgreSQL.Table.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies <see cref="Query.PhysicalOrderPullCommandText"/> (DiGi.PostgreSQL#8). It answers <see langword="null"/> without a table name, a partitioning column or a column id. On the local test server, the plan of a window bounded on both sides is a <b>Tid Range Scan</b>, which reads only the window's blocks.
        /// <para>This is the property the physical-order read exists for. A lower bound alone is planned as a sequential scan of the rest of the partition plus a top-N sort, on every page. Measured on PostgreSQL 18 while designing the read: 7 143 buffers per page, against 150 for a bounded window.</para>
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task Query_PhysicalOrderPullCommandText()
        {
            Assert.Null(Query.PhysicalOrderPullCommandText(null, ["column_1"], "column_2"));
            Assert.Null(Query.PhysicalOrderPullCommandText("partitiontable", ["column_1"], null));
            Assert.Null(Query.PhysicalOrderPullCommandText("partitiontable", null, "column_2"));
            Assert.Null(Query.PhysicalOrderPullCommandText("partitiontable", [" ", ""], "column_2"));

            string? commandText_Duplicates = Query.PhysicalOrderPullCommandText("partitiontable", ["column_1", "column_1"], "column_2");
            Assert.NotNull(commandText_Duplicates);
            Assert.Contains($"\"column_1\", ctid::text AS \"{Constants.ColumnName.PhysicalPosition}\"", commandText_Duplicates);
            Assert.DoesNotContain("\"column_1\", \"column_1\"", commandText_Duplicates);

            if (!PostgreSQL.xUnit.Create.IsAvailable(PostgreSQL.Enums.StorageMethod.Table, out ConnectionData? connectionData))
            {
                return;
            }

            PartitionTablePostgreSQLConverter partitionTablePostgreSQLConverter = new(connectionData);

            // 400 rows of about 1.8 kB: a partition of about a hundred blocks, so a 10-block window is a small part of it.
            Core.IO.Table.Classes.Table table = new();
            table.AddColumn(PartitionTablePostgreSQLConverter.Column_1);
            table.AddColumn(PartitionTablePostgreSQLConverter.Column_2);
            table.AddColumn(PartitionTablePostgreSQLConverter.Column_3);
            for (int i = 0; i < 400; i++)
            {
                table.AddRow([i, "AAA", string.Concat(Enumerable.Range(0, 56).Select(_ => Guid.NewGuid().ToString("N")))]);
            }

            try
            {
                Assert.True(await partitionTablePostgreSQLConverter.PushAsync(table));

                await using NpgsqlConnection? npgsqlConnection = PostgreSQL.Create.NpgsqlConnection(connectionData);
                Assert.NotNull(npgsqlConnection);
                await npgsqlConnection.OpenAsync();
                Skip.IfNot(npgsqlConnection.IsPhysicalOrderSupported(), $"PostgreSQL {npgsqlConnection.PostgreSqlVersion} has no TID Range Scan.");

                await using (NpgsqlCommand npgsqlCommand_Analyze = new($"ANALYZE \"{partitionTablePostgreSQLConverter.TableName}\"", npgsqlConnection))
                {
                    await npgsqlCommand_Analyze.ExecuteNonQueryAsync();
                }

                string? commandText = Query.PhysicalOrderPullCommandText(partitionTablePostgreSQLConverter.TableName, [PartitionTablePostgreSQLConverter.Column_1.UniqueId()!, PartitionTablePostgreSQLConverter.Column_2.UniqueId()!], PartitionTablePostgreSQLConverter.Column_2.UniqueId());
                Assert.NotNull(commandText);

                foreach ((string lowerPosition, string upperPosition) in new[] { ("(0,0)", "(10,0)"), ("(40,3)", "(50,0)") })
                {
                    await using NpgsqlCommand npgsqlCommand_Explain = new($"EXPLAIN (ANALYZE, BUFFERS) {commandText}", npgsqlConnection);
                    npgsqlCommand_Explain.Parameters.AddWithValue("partitionValue", "AAA");
                    npgsqlCommand_Explain.Parameters.AddWithValue("lowerPosition", lowerPosition);
                    npgsqlCommand_Explain.Parameters.AddWithValue("upperPosition", upperPosition);
                    npgsqlCommand_Explain.Parameters.AddWithValue("pageSize", 1000);

                    StringBuilder stringBuilder = new();
                    await using (NpgsqlDataReader npgsqlDataReader = await npgsqlCommand_Explain.ExecuteReaderAsync())
                    {
                        while (await npgsqlDataReader.ReadAsync())
                        {
                            stringBuilder.AppendLine(npgsqlDataReader.GetString(0));
                        }
                    }

                    string plan = stringBuilder.ToString();
                    Assert.True(plan.Contains("Tid Range Scan"), $"Window {lowerPosition}..{upperPosition} was not planned as a TID Range Scan:{Environment.NewLine}{plan}");
                }
            }
            finally
            {
                await PostgreSQL.Modify.RemoveTableAsync(connectionData, partitionTablePostgreSQLConverter.TableName);
                await PostgreSQL.Modify.RemoveTableAsync(connectionData, Constants.TableName.Columns);
            }
        }
    }
}
