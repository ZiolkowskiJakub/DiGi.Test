using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Table.Classes;

namespace DiGi.PostgreSQL.Table.xUnit.Classes
{
    public class PartitionTablePostgreSQLConverter : TablePostgreSQLConverter<Core.IO.Table.Classes.Column>
    {
        public static readonly Core.IO.Table.Classes.Column Column_1 = new ("Column_1", typeof(int));
        public static readonly Core.IO.Table.Classes.Column Column_2 = new ("Column_2", typeof(string));
        public static readonly Core.IO.Table.Classes.Column Column_3 = new("Column_3", typeof(string));
        public static readonly Core.IO.Table.Classes.Column Column_4 = new("Column_4", typeof(bool));

        public PartitionTablePostgreSQLConverter(ConnectionData? connectionData) 
            : base(connectionData)
        {

        }

        public override string TableName => "partitiontable";

        protected override TableConversionOptions<Core.IO.Table.Classes.Column>? TableConversionOptions => new () 
        {
            PrimaryKeyColumns = 
            [ 
                Column_1,
                Column_2,
            ],

            PartitioningOptions = new PartitioningOptions<Core.IO.Table.Classes.Column>()
            {
                Column = Column_2,
                PartitioningRule = new ValuePartitioningRule()
            }
        };
    }
}
