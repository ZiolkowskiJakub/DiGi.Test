using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Table.Classes;

namespace DiGi.PostgreSQL.Table.xUnit.Classes
{
    /// <summary>
    /// Provides functionality to convert and manage data for partition tables in a PostgreSQL database, utilizing <see cref="Core.IO.Table.Classes.Column"/> for column definitions.
    /// </summary>
    public class PartitionTablePostgreSQLConverter : TablePostgreSQLConverter<Core.IO.Table.Classes.Column>
    {
        /// <summary>
        /// Represents the first column of the partition table, defined with an integer data type.
        /// </summary>
        public static readonly Core.IO.Table.Classes.Column Column_1 = new("Column_1", typeof(int));

        /// <summary>
        /// Represents the second column of the partition table, defined with a string data type.
        /// </summary>
        public static readonly Core.IO.Table.Classes.Column Column_2 = new("Column_2", typeof(string));

        /// <summary>
        /// Represents the third column of the partition table, defined with a string data type.
        /// </summary>
        public static readonly Core.IO.Table.Classes.Column Column_3 = new("Column_3", typeof(string));

        /// <summary>
        /// Represents the fourth column of the partition table, defined with a boolean data type.
        /// </summary>
        public static readonly Core.IO.Table.Classes.Column Column_4 = new("Column_4", typeof(bool));

        /// <summary>
        /// Initializes a new instance of the <see cref="PartitionTablePostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The connection data used to establish a connection to the PostgreSQL database.</param>
        public PartitionTablePostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the name of the database table used by the partition table PostgreSQL converter.
        /// </summary>
        public override string TableName => "partitiontable";

        protected override TableConversionOptions<Core.IO.Table.Classes.Column>? TableConversionOptions => new()
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