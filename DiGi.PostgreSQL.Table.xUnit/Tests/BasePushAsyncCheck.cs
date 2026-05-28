using DiGi.Core.IO.Table.Classes;
using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Table.xUnit.Classes;

namespace DiGi.PostgreSQL.Table.xUnit
{
    public partial class Tests
    {
        [SkippableFact]
        public async Task BasePushAsyncCheck()
        {
            ConnectionData connectionData = PostgreSQL.xUnit.Create.ConnectionData(Enums.StorageMethod.Table);

            BaseTablePostgreSQLConverter testTablePostgreSQLConverter = new (connectionData);

            Core.IO.Table.Classes.Table table = new ();

            table.AddColumn("Column_1", typeof(int));
            table.AddColumn(new ExtendedColumn("Column_2", typeof(string), "Additional Column", "This is additional Column"));

            table.AddRow([1, "AAA"]);
            table.AddRow([2, "BBB"]);
            table.AddRow([3, null]);
            table.AddRow([3, "CCC"]);

            bool updated = await testTablePostgreSQLConverter.PushAsync(table);
            Assert.True(updated);

            HashSet<string>? categories = await testTablePostgreSQLConverter.GetCategories();
            Assert.True(categories is not null && categories.Count != 0);
            Assert.Contains("Additional Column", categories);

            List<Column>? columns = await testTablePostgreSQLConverter.GetColumns();
            Assert.NotNull(columns);
            Assert.Equal(2, columns.Count);

            columns = await testTablePostgreSQLConverter.GetColumnsByCategories(["Additional Column"]);
            Assert.NotNull(columns);
            Assert.Single(columns);

            bool removed_Table = await PostgreSQL.Modify.RemoveTableAsync(connectionData, testTablePostgreSQLConverter.TableName);
            Assert.True(removed_Table);
            
            bool removed_Columns = await PostgreSQL.Modify.RemoveTableAsync(connectionData, Constants.TableName.Columns);
            Assert.True(removed_Columns);
        }
    }
}