using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Table.xUnit.Classes;

namespace DiGi.PostgreSQL.Table.xUnit
{
    public partial class Tests
    {
        [SkippableFact]
        public async Task TablePostgreSQLConverterUpdateAsyncCheck()
        {
            ConnectionData connectionData = PostgreSQL.xUnit.Create.ConnectionData(Enums.StorageMethod.Table);

            TestTablePostgreSQLConverter testTablePostgreSQLConverter = new (connectionData);

            Core.IO.Table.Classes.Table table = new ();

            table.AddColumn("Column_1", typeof(int));
            table.AddColumn("Column_2", typeof(string));

            table.AddRow([1, "AAA"]);
            table.AddRow([2, "BBB"]);
            table.AddRow([3, null]);
            table.AddRow([3, "CCC"]);

            bool updated = await testTablePostgreSQLConverter.UpdateAsync(table);
            Assert.True(updated);

            bool removed_Table = await PostgreSQL.Modify.RemoveTableAsync(connectionData, testTablePostgreSQLConverter.TableName);
            Assert.True(removed_Table);
            
            bool removed_Columns = await PostgreSQL.Modify.RemoveTableAsync(connectionData, Constants.TableName.Columns);
            Assert.True(removed_Columns);
        }
    }
}