using DiGi.Core.IO.Table.Classes;
using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Table.xUnit.Classes;
using System.Threading.Tasks;

namespace DiGi.PostgreSQL.Table.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Validates that PushAsync does not mutate the Index property of shared column definitions supplied via TableConversionOptions.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task PushAsync_SharedColumnImmutability()
        {
            if (!PostgreSQL.xUnit.Create.IsAvailable(PostgreSQL.Enums.StorageMethod.Table, out ConnectionData? connectionData))
            {
                return;
            }

            // 1. Non-partitioned table converter test
            SharedColumnsTablePostgreSQLConverter.Column_Identity.Index = -1;
            SharedColumnsTablePostgreSQLConverter.Column_Unique.Index = -1;
            SharedColumnsTablePostgreSQLConverter.Column_PrimaryKey.Index = -1;

            SharedColumnsTablePostgreSQLConverter converter = new(connectionData);

            Core.IO.Table.Classes.Table table_Standard = new();
            table_Standard.AddColumn("col_unrelated", typeof(string));
            table_Standard.AddColumn("col_primary_key", typeof(int));
            table_Standard.AddColumn("col_unique", typeof(string));
            table_Standard.AddColumn("col_identity", typeof(int));

            table_Standard.AddRow(["extra_1", 1, "u_1", 101]);
            table_Standard.AddRow(["extra_2", 2, "u_2", 102]);

            try
            {
                bool updated = await converter.PushAsync(table_Standard);
                Assert.True(updated);

                Assert.Equal(-1, SharedColumnsTablePostgreSQLConverter.Column_Identity.Index);
                Assert.Equal(-1, SharedColumnsTablePostgreSQLConverter.Column_Unique.Index);
                Assert.Equal(-1, SharedColumnsTablePostgreSQLConverter.Column_PrimaryKey.Index);
            }
            finally
            {
                await PostgreSQL.Modify.RemoveTableAsync(connectionData, converter.TableName);
                await PostgreSQL.Modify.RemoveTableAsync(connectionData, Constants.TableName.Columns);
            }

            // 2. Partitioned table converter test
            SharedColumnsPartitionTablePostgreSQLConverter.Column_PrimaryKey.Index = -1;
            SharedColumnsPartitionTablePostgreSQLConverter.Column_Partition.Index = -1;

            SharedColumnsPartitionTablePostgreSQLConverter converter_Partition = new(connectionData);

            Core.IO.Table.Classes.Table table_Partition = new();
            table_Partition.AddColumn("col_unrelated", typeof(string));
            table_Partition.AddColumn("col_primary_key", typeof(int));
            table_Partition.AddColumn("col_partition", typeof(string));

            table_Partition.AddRow(["extra_1", 1, "part_A"]);
            table_Partition.AddRow(["extra_2", 2, "part_A"]);

            try
            {
                bool updated = await converter_Partition.PushAsync(table_Partition);
                Assert.True(updated);

                Assert.Equal(-1, SharedColumnsPartitionTablePostgreSQLConverter.Column_PrimaryKey.Index);
                Assert.Equal(-1, SharedColumnsPartitionTablePostgreSQLConverter.Column_Partition.Index);
            }
            finally
            {
                await PostgreSQL.Modify.RemoveTableAsync(connectionData, converter_Partition.TableName);
                await PostgreSQL.Modify.RemoveTableAsync(connectionData, Constants.TableName.Columns);
            }
        }
    }
}
