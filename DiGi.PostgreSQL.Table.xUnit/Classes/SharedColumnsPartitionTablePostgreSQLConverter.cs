using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Table.Classes;

namespace DiGi.PostgreSQL.Table.xUnit.Classes
{
    /// <summary>
    /// Test converter that exposes shared static column definitions for partition and primary key options.
    /// </summary>
    public class SharedColumnsPartitionTablePostgreSQLConverter : TablePostgreSQLConverter<Core.IO.Table.Classes.Column>
    {
        /// <summary>
        /// Shared static column definition used as a primary key column.
        /// </summary>
        public static readonly Core.IO.Table.Classes.Column Column_PrimaryKey = new("col_primary_key", typeof(int));

        /// <summary>
        /// Shared static column definition used as a partition column.
        /// </summary>
        public static readonly Core.IO.Table.Classes.Column Column_Partition = new("col_partition", typeof(string));

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedColumnsPartitionTablePostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The connection data used to establish a database connection.</param>
        public SharedColumnsPartitionTablePostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the name of the database table used by this converter.
        /// </summary>
        public override string TableName => "shared_columns_partition_table";

        /// <summary>
        /// Gets the conversion options with shared static column instances.
        /// </summary>
        protected override TableConversionOptions<Core.IO.Table.Classes.Column>? TableConversionOptions => new()
        {
            PartitioningOptions = new PartitioningOptions<Core.IO.Table.Classes.Column>()
            {
                Column = Column_Partition,
                PartitioningRule = new ValuePartitioningRule()
            },
            PrimaryKeyColumns = [Column_PrimaryKey, Column_Partition]
        };
    }
}
