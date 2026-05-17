
using DiGi.Core.IO.Table.Classes;
using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Table.Classes;

namespace DiGi.PostgreSQL.Table.xUnit.Classes
{
    internal class TestTablePostgreSQLConverter : TablePostgreSQLConverter<Column>
    {
        public TestTablePostgreSQLConverter(ConnectionData? connectionData) 
            : base(connectionData)
        {

        }

        public override string TableName => "testtable";

        protected override TableConversionOptions<Column>? TableConversionOptions => new () 
        {
            PrimaryKeyColumns = [ new Column("Column_1", typeof(int))]
        };
    }
}
