// TODO [BuildingModelRowIdentity]: these facts cover the one-off unique_id migration of issue
// ZiolkowskiJakub/DiGi.GIS.PostgreSQL#5 and are temporary with it. Delete this file once every
// deployed database has run PostgreSQLBuildingModelUniqueIdMigrationTask and it reports zero
// pending rows nationally, together with the migration members the facts exercise.

using DiGi.Core.Parameter.Classes;
using DiGi.GIS.Analytical.Enums;
using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Stores a building model row keyed the old way, on the reference of the building, and checks that the migration re-keys it on the model's own identifier without disturbing the rest of the row.
        /// <para>Skipped by default: it writes to a database, so it needs <c>GIS_PostgreSQL_Main.conf</c> beside the test assembly pointing at a scratch database. Never run it against the deployed one - the converters address fixed table names, so there is no scratch table to fall back on.</para>
        /// <para>The legacy row cannot be produced through the converter any more, which is the point of the change, so it is built by hand exactly as the converter used to build it. What the migration has to do is read the identifier out of the serialized model in the <c>object</c> column and write it into <c>unique_id</c>, leaving <c>reference</c> - and therefore the row's link to its building - alone.</para>
        /// </summary>
        [Fact(Skip = "Writes to a database. Point GIS_PostgreSQL_Main.conf at a scratch database before running.")]
        public async Task MigrateUniqueIds_Integration()
        {
            const int countyId = 5;
            const string reference = "272D6AAF-9D86-9B0E-E053-CC2BA8C0B5EA";

            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = GIS.PostgreSQL.Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            BuildingModelPostgreSQLConverter? buildingModelPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingModelPostgreSQLConverter>();
            Assert.NotNull(buildingModelPostgreSQLConverter);

            DiGi.Analytical.Building.Classes.BuildingModel buildingModel = new();
            Assert.True(buildingModel.SetValue(BuildingModelParameter.Reference, reference, new SetValueSettings(true, false)));

            string? uniqueId = buildingModel.UniqueId;
            Assert.False(string.IsNullOrWhiteSpace(uniqueId));
            Assert.NotEqual(reference, uniqueId);

            // Built by hand rather than through Convert.ToPostgreSQL, which now emits the model's own
            // identifier - this is the row shape the migration exists to correct.
            BuildingModel buildingModel_PostgreSQL = new()
            {
                Reference = reference,
                Object = buildingModel.ToJsonObject(),
                UniqueId = reference,
                CountyId = countyId
            };

            HashSet<long>? ids_Stored = await buildingModelPostgreSQLConverter.UpdateAsync([buildingModel_PostgreSQL]);
            Assert.NotNull(ids_Stored);
            Assert.Single(ids_Stored);

            //The county row now holds at least this one row to migrate

            UniqueIdMigrationResult? uniqueIdMigrationResult = await buildingModelPostgreSQLConverter.GetUniqueIdMigrationResultAsync(countyId);
            Assert.NotNull(uniqueIdMigrationResult);
            Assert.True(uniqueIdMigrationResult.Pending > 0);
            Assert.Equal(uniqueIdMigrationResult.Total, uniqueIdMigrationResult.Done + uniqueIdMigrationResult.Pending + uniqueIdMigrationResult.Blocked + uniqueIdMigrationResult.Missing);

            //Migrating moves exactly the rows that were counted

            HashSet<long>? ids_Migrated = await buildingModelPostgreSQLConverter.MigrateUniqueIdsAsync(countyId);
            Assert.NotNull(ids_Migrated);
            Assert.Equal(uniqueIdMigrationResult.Pending, ids_Migrated.Count);
            Assert.Contains(ids_Stored.First(), ids_Migrated);

            //The row is now keyed on the model and still names its building

            List<BuildingModel>? buildingModels_Stored = await buildingModelPostgreSQLConverter.GetItemsByReferenceAsync(reference, countyId);
            Assert.NotNull(buildingModels_Stored);
            Assert.Single(buildingModels_Stored);
            Assert.Equal(uniqueId, buildingModels_Stored[0].UniqueId);
            Assert.Equal(reference, buildingModels_Stored[0].Reference);

            //Running it again finds nothing left to do, so the migration is repeatable

            UniqueIdMigrationResult? uniqueIdMigrationResult_Migrated = await buildingModelPostgreSQLConverter.GetUniqueIdMigrationResultAsync(countyId);
            Assert.NotNull(uniqueIdMigrationResult_Migrated);
            Assert.Equal(0, uniqueIdMigrationResult_Migrated.Pending);

            //Clean up

            HashSet<long>? ids_Cleared = await buildingModelPostgreSQLConverter.RemoveAsync([reference], countyId);
            Assert.NotNull(ids_Cleared);
            Assert.Single(ids_Cleared);
        }
    }
}
