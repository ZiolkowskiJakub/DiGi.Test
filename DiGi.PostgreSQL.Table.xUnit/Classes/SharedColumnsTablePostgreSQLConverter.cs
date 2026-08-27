using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Table.Classes;

namespace DiGi.PostgreSQL.Table.xUnit.Classes
{
    /// <summary>
    /// Test converter that exposes shared static column definitions for identity, unique, and primary key options.
    /// </summary>
    public class SharedColumnsTablePostgreSQLConverter : TablePostgreSQLConverter<Core.IO.Table.Classes.Column>
    {
        /// <summary>
        /// Shared static column definition used as an identity column.
        /// </summary>
        public static readonly Core.IO.Table.Classes.Column Column_Identity = new("col_identity", typeof(int));

        /// <summary>
        /// Shared static column definition used as a unique column.
        /// </summary>
        public static readonly Core.IO.Table.Classes.Column Column_Unique = new("col_unique", typeof(string));

        /// <summary>
        /// Shared static column definition used as a primary key column.
        /// </summary>
        public static readonly Core.IO.Table.Classes.Column Column_PrimaryKey = new("col_primary_key", typeof(int));

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedColumnsTablePostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The connection data used to establish a database connection.</param>
        public SharedColumnsTablePostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the name of the database table used by this converter.
        /// </summary>
        public override string TableName => "shared_columns_table";

        /// <summary>
        /// Gets the conversion options with shared static column instances.
        /// </summary>
        protected override TableConversionOptions<Core.IO.Table.Classes.Column>? TableConversionOptions => new()
        {
            IdentityColumn = Column_Identity,
            UniqueColumns = [Column_Unique],
            PrimaryKeyColumns = [Column_PrimaryKey]
        };
    }
}
