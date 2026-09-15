using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Table.Classes;

namespace DiGi.PostgreSQL.Table.xUnit.Classes
{
    /// <summary>
    /// Partitioned scratch-table converter for the GetHistogramSummaryAsync partition-scope facts: a LIST partition key <c>county</c>, a <c>row</c> primary-key discriminator, and a nullable <c>value</c> double column to histogram.
    /// </summary>
    public class HistogramPartitionTablePostgreSQLConverter : TablePostgreSQLConverter<Core.IO.Table.Classes.Column>
    {
        /// <summary>
        /// The LIST partition key column; also part of the primary key.
        /// </summary>
        public static readonly Core.IO.Table.Classes.Column Column_County = new("county", typeof(int));

        /// <summary>
        /// The primary-key discriminator column.
        /// </summary>
        public static readonly Core.IO.Table.Classes.Column Column_Row = new("row", typeof(int));

        /// <summary>
        /// The nullable double column under test.
        /// </summary>
        public static readonly Core.IO.Table.Classes.Column Column_Value = new("value", typeof(double));

        /// <summary>
        /// Initializes a new instance of the <see cref="HistogramPartitionTablePostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The connection data used to establish a database connection.</param>
        public HistogramPartitionTablePostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the name of the database table used by this converter.
        /// </summary>
        public override string TableName => "histogram_partition_table";

        /// <summary>
        /// Gets the conversion options: LIST partitioning on <c>county</c> and a <c>(county, row)</c> primary key.
        /// </summary>
        protected override TableConversionOptions<Core.IO.Table.Classes.Column>? TableConversionOptions => new()
        {
            PrimaryKeyColumns = [Column_County, Column_Row],
            PartitioningOptions = new PartitioningOptions<Core.IO.Table.Classes.Column>()
            {
                Column = Column_County,
                PartitioningRule = new ValuePartitioningRule()
            }
        };
    }
}
