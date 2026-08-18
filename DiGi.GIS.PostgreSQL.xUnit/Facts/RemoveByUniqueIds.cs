using DiGi.GIS.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Stores two occupancy objects against one building and removes one of them by its unique identifier, checking that the other survives.
        /// <para>Skipped by default: it writes to a database, so it needs <c>GIS_PostgreSQL_Main.conf</c> beside the test assembly pointing at a scratch database. Never run it against the deployed one - the converters address fixed table names, so there is no scratch table to fall back on and the delete has no undo.</para>
        /// <para>This is the operation the row layout depends on. A building holds one row per stored object, keyed <c>UNIQUE (county_id, unique_id)</c>, so correcting a single object means taking that one row out and writing its replacement. Removing by reference alone would take out everything held for the building, which is why the count returned here has to be exactly one.</para>
        /// </summary>
        [Fact(Skip = "Writes to a database. Point GIS_PostgreSQL_Main.conf at a scratch database before running.")]
        public async Task RemoveByUniqueIds_Integration()
        {
            const int countyId = 5;
            const string reference = "272D6AAF-9D86-9B0E-E053-CC2BA8C0B5EA";

            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = GIS.PostgreSQL.Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            Building2DOccupancyDataPostgreSQLConverter? building2DOccupancyDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DOccupancyDataPostgreSQLConverter>();
            Assert.NotNull(building2DOccupancyDataPostgreSQLConverter);

            OccupancyData occupancyData_First = new(reference, 120.5, 3);
            OccupancyData occupancyData_Second = new(reference, 120.5, 4);

            string? uniqueId_First = occupancyData_First.UniqueId;
            string? uniqueId_Second = occupancyData_Second.UniqueId;

            Assert.False(string.IsNullOrWhiteSpace(uniqueId_First));
            Assert.False(string.IsNullOrWhiteSpace(uniqueId_Second));
            Assert.NotEqual(uniqueId_First, uniqueId_Second);

            List<Building2DOccupancyData> building2DOccupancyDatas = [];
            foreach (OccupancyData occupancyData in new OccupancyData[] { occupancyData_First, occupancyData_Second })
            {
                Building2DOccupancyData? building2DOccupancyData = occupancyData.ToPostgreSQL(countyId);
                Assert.NotNull(building2DOccupancyData);
                building2DOccupancyDatas.Add(building2DOccupancyData);
            }

            HashSet<long>? ids_Stored = await building2DOccupancyDataPostgreSQLConverter.UpdateAsync(building2DOccupancyDatas);
            Assert.NotNull(ids_Stored);
            Assert.Equal(2, ids_Stored.Count);

            List<Building2DOccupancyData>? building2DOccupancyDatas_Stored = await building2DOccupancyDataPostgreSQLConverter.GetItemsByReferenceAsync(reference, countyId);
            Assert.NotNull(building2DOccupancyDatas_Stored);
            Assert.Equal(2, building2DOccupancyDatas_Stored.Count);

            //Remove one of the two

            HashSet<long>? ids_Removed = await building2DOccupancyDataPostgreSQLConverter.RemoveByUniqueIdsAsync([uniqueId_First!], reference, countyId);
            Assert.NotNull(ids_Removed);
            Assert.Single(ids_Removed);

            List<Building2DOccupancyData>? building2DOccupancyDatas_Remaining = await building2DOccupancyDataPostgreSQLConverter.GetItemsByReferenceAsync(reference, countyId);
            Assert.NotNull(building2DOccupancyDatas_Remaining);
            Assert.Single(building2DOccupancyDatas_Remaining);
            Assert.Equal(uniqueId_Second, building2DOccupancyDatas_Remaining.First().UniqueId);

            //A unique identifier belonging to another building must not match

            HashSet<long>? ids_Guarded = await building2DOccupancyDataPostgreSQLConverter.RemoveByUniqueIdsAsync([uniqueId_Second!], "00000000-0000-0000-0000-000000000000", countyId);
            Assert.NotNull(ids_Guarded);
            Assert.Empty(ids_Guarded);

            //Clean up

            HashSet<long>? ids_Cleared = await building2DOccupancyDataPostgreSQLConverter.RemoveAsync([reference], countyId);
            Assert.NotNull(ids_Cleared);
            Assert.Single(ids_Cleared);
        }
    }
}
