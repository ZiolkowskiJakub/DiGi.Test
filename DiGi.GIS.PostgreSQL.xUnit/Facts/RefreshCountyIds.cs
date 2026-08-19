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
        /// Stores an occupancy object under the wrong county row and checks that a refresh moves it onto the right one, then checks that a row which cannot move is left alone rather than deleted.
        /// <para>Skipped by default: it writes to a database, so it needs <c>GIS_PostgreSQL_Main.conf</c> beside the test assembly pointing at a scratch database. Never run it against the deployed one - the converters address fixed table names, so there is no scratch table to fall back on and the clean-up at the end has no undo.</para>
        /// <para><c>county_id</c> is the partition key, so the refresh moves the row between partitions. The two assertions that matter are that the identifier survives that move - anything holding an <c>id</c> from before the call has to keep addressing the same record - and that a row blocked by <c>UNIQUE (county_id, unique_id)</c> stays where it is and is not reported, which is what makes the method safe to run over a whole county unattended.</para>
        /// </summary>
        [Fact(Skip = "Writes to a database. Point GIS_PostgreSQL_Main.conf at a scratch database before running.")]
        public async Task RefreshCountyIds_Integration()
        {
            const int countyId_Target = 5;
            const int countyId_Wrong = 6;
            const string reference = "272D6AAF-9D86-9B0E-E053-CC2BA8C0B5EA";

            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = GIS.PostgreSQL.Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            Building2DOccupancyDataPostgreSQLConverter? building2DOccupancyDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DOccupancyDataPostgreSQLConverter>();
            Assert.NotNull(building2DOccupancyDataPostgreSQLConverter);

            //A row filed under the wrong county part moves onto the right one

            OccupancyData occupancyData_Stray = new(reference, 120.5, 3);

            Building2DOccupancyData? building2DOccupancyData_Stray = occupancyData_Stray.ToPostgreSQL(countyId_Wrong);
            Assert.NotNull(building2DOccupancyData_Stray);

            HashSet<long>? ids_Stored = await building2DOccupancyDataPostgreSQLConverter.UpdateAsync([building2DOccupancyData_Stray]);
            Assert.NotNull(ids_Stored);
            Assert.Single(ids_Stored);

            long id_Stored = ids_Stored.First();

            HashSet<string>? references_Refreshed = await building2DOccupancyDataPostgreSQLConverter.RefreshCountyIdsAsync([reference], countyId_Target);
            Assert.NotNull(references_Refreshed);
            Assert.Single(references_Refreshed);
            Assert.Contains(reference, references_Refreshed);

            List<Building2DOccupancyData>? building2DOccupancyDatas_Target = await building2DOccupancyDataPostgreSQLConverter.GetItemsByReferenceAsync(reference, countyId_Target);
            Assert.NotNull(building2DOccupancyDatas_Target);
            Assert.Single(building2DOccupancyDatas_Target);
            Assert.Equal(id_Stored, building2DOccupancyDatas_Target.First().Id);
            Assert.Equal(countyId_Target, building2DOccupancyDatas_Target.First().CountyId);

            List<Building2DOccupancyData>? building2DOccupancyDatas_Wrong = await building2DOccupancyDataPostgreSQLConverter.GetItemsByReferenceAsync(reference, countyId_Wrong);
            Assert.NotNull(building2DOccupancyDatas_Wrong);
            Assert.Empty(building2DOccupancyDatas_Wrong);

            //A second call has nothing left to move

            HashSet<string>? references_Refreshed_Again = await building2DOccupancyDataPostgreSQLConverter.RefreshCountyIdsAsync([reference], countyId_Target);
            Assert.NotNull(references_Refreshed_Again);
            Assert.Empty(references_Refreshed_Again);

            //A row whose object is already held under the target county cannot move, and is left where it is

            OccupancyData occupancyData_Collision = new(reference, 98.0, 2);

            Building2DOccupancyData? building2DOccupancyData_Collision_Target = occupancyData_Collision.ToPostgreSQL(countyId_Target);
            Building2DOccupancyData? building2DOccupancyData_Collision_Wrong = occupancyData_Collision.ToPostgreSQL(countyId_Wrong);
            Assert.NotNull(building2DOccupancyData_Collision_Target);
            Assert.NotNull(building2DOccupancyData_Collision_Wrong);
            Assert.Equal(building2DOccupancyData_Collision_Target.UniqueId, building2DOccupancyData_Collision_Wrong.UniqueId);

            HashSet<long>? ids_Stored_Collision = await building2DOccupancyDataPostgreSQLConverter.UpdateAsync([building2DOccupancyData_Collision_Target, building2DOccupancyData_Collision_Wrong]);
            Assert.NotNull(ids_Stored_Collision);
            Assert.Equal(2, ids_Stored_Collision.Count);

            HashSet<string>? references_Refreshed_Collision = await building2DOccupancyDataPostgreSQLConverter.RefreshCountyIdsAsync([reference], countyId_Target);
            Assert.NotNull(references_Refreshed_Collision);
            Assert.Empty(references_Refreshed_Collision);

            List<Building2DOccupancyData>? building2DOccupancyDatas_Wrong_Kept = await building2DOccupancyDataPostgreSQLConverter.GetItemsByReferenceAsync(reference, countyId_Wrong);
            Assert.NotNull(building2DOccupancyDatas_Wrong_Kept);
            Assert.Single(building2DOccupancyDatas_Wrong_Kept);

            //Clean up

            HashSet<long>? ids_Cleared_Target = await building2DOccupancyDataPostgreSQLConverter.RemoveAsync([reference], countyId_Target);
            Assert.NotNull(ids_Cleared_Target);
            Assert.Equal(2, ids_Cleared_Target.Count);

            HashSet<long>? ids_Cleared_Wrong = await building2DOccupancyDataPostgreSQLConverter.RemoveAsync([reference], countyId_Wrong);
            Assert.NotNull(ids_Cleared_Wrong);
            Assert.Single(ids_Cleared_Wrong);
        }
    }
}
