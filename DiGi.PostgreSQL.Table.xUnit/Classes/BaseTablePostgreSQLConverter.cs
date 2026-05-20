
using DiGi.Core.IO.Table.Classes;
using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Table.Classes;

namespace DiGi.PostgreSQL.Table.xUnit.Classes
{
    internal class BaseTablePostgreSQLConverter : TablePostgreSQLConverter<Column>
    {
        public BaseTablePostgreSQLConverter(ConnectionData? connectionData) 
            : base(connectionData)
        {

        }

        public override string TableName => "basetable";

        protected override TableConversionOptions<Column>? TableConversionOptions => new () 
        {
            PrimaryKeyColumns = [ new ExtendedColumn("Column_1", typeof(int), "Main", "Main Column")]
        };
    }
}
