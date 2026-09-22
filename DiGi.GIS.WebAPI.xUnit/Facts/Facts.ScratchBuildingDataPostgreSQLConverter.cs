using DiGi.GIS.PostgreSQL.Classes;
using DiGi.PostgreSQL.Classes;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// A <see cref="BuildingDataPostgreSQLConverter"/> writing to a scratch table, so a controller fact can push its own building data into the local test database and page it back through the real endpoint code: the same primary key, partitioning and column handling, under a table name nothing else uses.
        /// <para>The name differs from the scratch table of <c>DiGi.GIS.PostgreSQL.xUnit</c>, so the two projects can run at the same time. Nested in <see cref="Facts"/> rather than declared in a <c>Classes</c> namespace: such a namespace would capture the existing <c>Classes.Parameter</c> references of this project, which mean <c>DiGi.GIS.WebAPI.Classes</c>.</para>
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
            public override string TableName => "zz_building_data_scratch_webapi";
        }
    }
}
