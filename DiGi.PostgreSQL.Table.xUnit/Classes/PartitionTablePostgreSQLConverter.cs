
using DiGi.Core.IO.Table.Classes;
using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Table.Classes;

namespace DiGi.PostgreSQL.Table.xUnit.Classes
{
    public class PartitionTablePostgreSQLConverter : TablePostgreSQLConverter<Column>
    {
        public static readonly Column Column_1 = new ("Column_1", typeof(int));
        public static readonly Column Column_2 = new ("Column_2", typeof(string));
        public static readonly Column Column_3 = new("Column_3", typeof(string));

        public PartitionTablePostgreSQLConverter(ConnectionData? connectionData) 
            : base(connectionData)
        {

        }

        public override string TableName => "partitiontable";

        protected override TableConversionOptions<Column>? TableConversionOptions => new () 
        {
            PrimaryKeyColumns = 
            [ 
                Column_1,
                Column_2,
            ],

            PartitioningOptions = new PartitioningOptions<Column>()
            {
                Column = Column_2,
                PartitioningRule = new ValuePartitioningRule()
            }
        };
    }
}
