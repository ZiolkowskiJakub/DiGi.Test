// TODO [BuildingModelRowIdentity]: these facts cover the one-off unique_id migration of issue
// ZiolkowskiJakub/DiGi.GIS.PostgreSQL#5 and are temporary with it. Delete this file once every
// deployed database has run PostgreSQLBuildingModelUniqueIdMigrationTask and it reports zero
// pending rows nationally, together with the migration members the facts exercise.

using DiGi.GIS.PostgreSQL.Classes;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Checks the migration's classification against two tables that never needed migrating, and asserts that it finds nothing to do in either.
        /// <para>Skipped by default: it reads a database, so it needs <c>GIS_PostgreSQL_Main.conf</c> beside the test assembly. Unlike the other integration facts here it writes nothing at all, so the deployed database is a legitimate target - and the more useful one, since the point is to check the classification against rows the production write path really produced.</para>
        /// <para><c>occupancy_data_building_2d</c> and <c>year_built_data</c> hold objects deriving from the same <c>GuidObject</c> as a building model, and their converters have always written <c>unique_id</c> from the stored object's own identifier - which is what <c>building_model</c> is being changed to do. Every one of their rows must therefore come back already done. The migration reaches that identifier a different way, out of the serialized JSON in the <c>object</c> column, so agreement here is the evidence that the two routes land on the same value before the migration is ever pointed at the table it is meant to repair. A non-zero pending count means they do not, and nothing should be migrated until that is understood.</para>
        /// </summary>
        [Fact(Skip = "Reads a database. Needs GIS_PostgreSQL_Main.conf beside the test assembly. Writes nothing, so the deployed database is a valid target.")]
        public async Task GetUniqueIdMigrationResult_TablesThatNeverNeededMigrating()
        {
            const int countyId = 5;

            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = GIS.PostgreSQL.Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            Building2DOccupancyDataPostgreSQLConverter? building2DOccupancyDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DOccupancyDataPostgreSQLConverter>();
            Assert.NotNull(building2DOccupancyDataPostgreSQLConverter);

            YearBuiltDataPostgreSQLConverter? yearBuiltDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<YearBuiltDataPostgreSQLConverter>();
            Assert.NotNull(yearBuiltDataPostgreSQLConverter);

            static void AssertNothingToDo(UniqueIdMigrationResult? uniqueIdMigrationResult, string tableName)
            {
                Assert.NotNull(uniqueIdMigrationResult);

                // A county row holding nothing would pass every check below without having checked anything.
                Assert.True(uniqueIdMigrationResult.Total > 0, $"{tableName} holds no rows under this county row - point the test at one that does");

                Assert.True(uniqueIdMigrationResult.Done == uniqueIdMigrationResult.Total, $"{tableName}: only {uniqueIdMigrationResult.Done} of {uniqueIdMigrationResult.Total} rows are keyed on the identifier of the object they store");
                Assert.True(uniqueIdMigrationResult.Pending == 0, $"{tableName}: {uniqueIdMigrationResult.Pending} rows would be migrated in a table that has always been written correctly - the expression computing the target identifier does not reproduce what the converter emits");
                Assert.True(uniqueIdMigrationResult.Blocked == 0, $"{tableName}: {uniqueIdMigrationResult.Blocked} rows carry an identifier another row already holds");
                Assert.True(uniqueIdMigrationResult.Missing == 0, $"{tableName}: {uniqueIdMigrationResult.Missing} rows store an object carrying no identifier");
            }

            AssertNothingToDo(await building2DOccupancyDataPostgreSQLConverter.GetUniqueIdMigrationResultAsync(countyId), "occupancy_data_building_2d");
            AssertNothingToDo(await yearBuiltDataPostgreSQLConverter.GetUniqueIdMigrationResultAsync(countyId), "year_built_data");
        }
    }
}
