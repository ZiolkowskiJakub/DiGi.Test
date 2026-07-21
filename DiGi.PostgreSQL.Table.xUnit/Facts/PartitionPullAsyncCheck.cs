using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Table.xUnit.Classes;

namespace DiGi.PostgreSQL.Table.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that data can be asynchronously pulled from a PostgreSQL partitioned table using the <see cref="PartitionTablePostgreSQLConverter"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task PartitionPullAsyncCheck()
        {
            if (!PostgreSQL.xUnit.Create.IsAvailable(PostgreSQL.Enums.StorageMethod.Table, out ConnectionData? connectionData))
            {
                return;
            }

            PartitionTablePostgreSQLConverter partitionTablePostgreSQLConverter = new(connectionData);

            Core.IO.Table.Classes.Table table = new();

            table.AddColumn(PartitionTablePostgreSQLConverter.Column_1);
            table.AddColumn(PartitionTablePostgreSQLConverter.Column_2);
            table.AddColumn(PartitionTablePostgreSQLConverter.Column_3);
            table.AddColumn(PartitionTablePostgreSQLConverter.Column_4);

            table.AddRow([1, "AAA", "A", false]);
            table.AddRow([2, "BBB", "B"]);
            table.AddRow([3, "AAA", "C"]);
            table.AddRow([3, "CCC", "D", true]);
            table.AddRow([4, "AAA", "E", true]);
            table.AddRow([5, "CCC", "F", true]);
            table.AddRow([6, "CCC", "G", false]);

            bool updated;

            updated = await partitionTablePostgreSQLConverter.PushAsync(table);
            Assert.True(updated);

            table.Clear();
            table.AddColumn(PartitionTablePostgreSQLConverter.Column_1);
            table.AddColumn(PartitionTablePostgreSQLConverter.Column_2);
            table.AddColumn(PartitionTablePostgreSQLConverter.Column_4);

            updated = await partitionTablePostgreSQLConverter.PullAsync(table);
            Assert.True(updated);

            Assert.Equal(false, table[0, 2]);

            table.ClearRows();
            table.AddRow([6, "CCC"]);

            Assert.Equal(false, table[0, 2]);

            bool removed_Table = await PostgreSQL.Modify.RemoveTableAsync(connectionData, partitionTablePostgreSQLConverter.TableName);
            Assert.True(removed_Table);

            bool removed_Columns = await PostgreSQL.Modify.RemoveTableAsync(connectionData, Constants.TableName.Columns);
            Assert.True(removed_Columns);
        }
    }
}