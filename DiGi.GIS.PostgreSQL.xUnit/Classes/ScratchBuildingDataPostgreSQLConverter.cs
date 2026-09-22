using DiGi.GIS.PostgreSQL.Classes;
using DiGi.PostgreSQL.Classes;

namespace DiGi.GIS.PostgreSQL.xUnit.Classes
{
    /// <summary>
    /// A <see cref="BuildingDataPostgreSQLConverter"/> writing to a scratch table, so a fact can push its own building data into the local test database and read it back through the real converter code: the same primary key, partitioning and column handling, under a table name nothing else uses.
    /// </summary>
    public class ScratchBuildingDataPostgreSQLConverter : BuildingDataPostgreSQLConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScratchBuildingDataPostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The connection settings of the local test database. This value can be null.</param>
        public ScratchBuildingDataPostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the name of the scratch table.
        /// </summary>
        public override string TableName => "zz_building_data_scratch";
    }
}
