using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Table.xUnit.Classes;

namespace DiGi.PostgreSQL.Table.xUnit
{
    public partial class Tests
    {
        [SkippableFact]
        public async Task PartitionPushAsyncCheck()
        {
            ConnectionData connectionData = PostgreSQL.xUnit.Create.ConnectionData(Enums.StorageMethod.Table);

            PartitionTablePostgreSQLConverter partitionTablePostgreSQLConverter = new(connectionData);

            Core.IO.Table.Classes.Table table = new();

            table.AddColumn(PartitionTablePostgreSQLConverter.Column_1);
            table.AddColumn(PartitionTablePostgreSQLConverter.Column_2);
            table.AddColumn(PartitionTablePostgreSQLConverter.Column_3);

            table.AddRow([1, "AAA", "A"]);
            table.AddRow([2, "BBB", "B"]);
            table.AddRow([3, "AAA", "C"]);
            table.AddRow([3, "CCC", "D"]);
            table.AddRow([4, "AAA", "E"]);

            bool updated;

            updated = await partitionTablePostgreSQLConverter.PushAsync(table);
            Assert.True(updated);

            table.ClearRows();
            table.AddColumn(PartitionTablePostgreSQLConverter.Column_4);
            table.AddRow([5, "CCC", "F", true]);
            table.AddRow([6, "CCC", "G", false]);

            updated = await partitionTablePostgreSQLConverter.PushAsync(table);
            Assert.True(updated);

            bool removed_Table = await PostgreSQL.Modify.RemoveTableAsync(connectionData, partitionTablePostgreSQLConverter.TableName);
            Assert.True(removed_Table);

            bool removed_Columns = await PostgreSQL.Modify.RemoveTableAsync(connectionData, Constants.TableName.Columns);
            Assert.True(removed_Columns);
        }
    }
}