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
        /// Stores an occupancy object under the wrong county row and checks that the probe reports it movable, that the mover then reports exactly the set the probe found movable, and that the probe reports nothing once the move has happened.
        /// <para>A row the destination part refuses - it already holds the stored object - is reported blocked and left where it is, and the mover does not report it either.</para>
        /// <para>Skipped by default: it writes to a database, so it needs <c>GIS_PostgreSQL_Main.conf</c> beside the test assembly pointing at a scratch database. Never run it against the deployed one - the converters address fixed table names, so there is no scratch table to fall back on and the clean-up at the end has no undo.</para>
        /// <para>The probe and the mover express the same rule in two statements, and the assertion that matters is the differential one: on the same data, the set the probe classifies as movable and the set the mover reports moved must be the same set. A change to one of the statements without the other fails this, not a person.</para>
        /// </summary>
        [Fact(Skip = "Writes to a database. Point GIS_PostgreSQL_Main.conf at a scratch database before running.")]
        public async Task GetStrayReferences_Integration()
        {
            const int countyId_Target = 5;
            const int countyId_Wrong = 6;
            const string reference = "272D6AAF-9D86-9B0E-E053-CC2BA8C0B5EA";

            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = GIS.PostgreSQL.Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            Building2DOccupancyDataPostgreSQLConverter? building2DOccupancyDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DOccupancyDataPostgreSQLConverter>();
            Assert.NotNull(building2DOccupancyDataPostgreSQLConverter);

            try
            {
                //A row filed under the wrong county part is reported movable

                OccupancyData occupancyData_Stray = new(reference, 120.5, 3);

                Building2DOccupancyData? building2DOccupancyData_Stray = occupancyData_Stray.ToPostgreSQL(countyId_Wrong);
                Assert.NotNull(building2DOccupancyData_Stray);

                PostgreSQLUpdateResult? updateResult_Stored = await building2DOccupancyDataPostgreSQLConverter.UpdateAsync([building2DOccupancyData_Stray]);
                Assert.NotNull(updateResult_Stored);
                Assert.Single(updateResult_Stored.Ids);

                Building2DReferencedObjectStrayResult? strayReferences = await building2DOccupancyDataPostgreSQLConverter.GetStrayReferencesAsync([reference], countyId_Target);
                Assert.NotNull(strayReferences);
                Assert.Equal(countyId_Target, strayReferences.CountyId);
                Assert.Single(strayReferences.MovableReferences);
                Assert.Contains(reference, strayReferences.MovableReferences);
                Assert.Empty(strayReferences.BlockedReferences);

                //The mover reports exactly the set the probe classified as movable

                HashSet<string>? references_Moved = await building2DOccupancyDataPostgreSQLConverter.RefreshCountyIdsAsync([reference], countyId_Target);
                Assert.NotNull(references_Moved);
                Assert.Equal(strayReferences.MovableReferences, references_Moved);

                //Once the move has happened, the probe reports nothing

                Building2DReferencedObjectStrayResult? strayReferences_Again = await building2DOccupancyDataPostgreSQLConverter.GetStrayReferencesAsync([reference], countyId_Target);
                Assert.NotNull(strayReferences_Again);
                Assert.Empty(strayReferences_Again.MovableReferences);
                Assert.Empty(strayReferences_Again.BlockedReferences);

                //A row whose stored object is already held under the target county is reported blocked, left where it is, and not reported by the mover

                OccupancyData occupancyData_Collision = new(reference, 98.0, 2);

                Building2DOccupancyData? building2DOccupancyData_Collision_Target = occupancyData_Collision.ToPostgreSQL(countyId_Target);
                Building2DOccupancyData? building2DOccupancyData_Collision_Wrong = occupancyData_Collision.ToPostgreSQL(countyId_Wrong);
                Assert.NotNull(building2DOccupancyData_Collision_Target);
                Assert.NotNull(building2DOccupancyData_Collision_Wrong);

                PostgreSQLUpdateResult? updateResult_Stored_Collision = await building2DOccupancyDataPostgreSQLConverter.UpdateAsync([building2DOccupancyData_Collision_Target, building2DOccupancyData_Collision_Wrong]);
                Assert.NotNull(updateResult_Stored_Collision);
                Assert.Equal(2, updateResult_Stored_Collision.Ids.Count);

                Building2DReferencedObjectStrayResult? strayReferences_Collision = await building2DOccupancyDataPostgreSQLConverter.GetStrayReferencesAsync([reference], countyId_Target);
                Assert.NotNull(strayReferences_Collision);
                Assert.Empty(strayReferences_Collision.MovableReferences);
                Assert.Single(strayReferences_Collision.BlockedReferences);
                Assert.Contains(reference, strayReferences_Collision.BlockedReferences);

                HashSet<string>? references_Moved_Collision = await building2DOccupancyDataPostgreSQLConverter.RefreshCountyIdsAsync([reference], countyId_Target);
                Assert.NotNull(references_Moved_Collision);
                Assert.Empty(references_Moved_Collision);

                List<Building2DOccupancyData>? building2DOccupancyDatas_Wrong_Kept = await building2DOccupancyDataPostgreSQLConverter.GetItemsByReferenceAsync(reference, countyId_Wrong);
                Assert.NotNull(building2DOccupancyDatas_Wrong_Kept);
                Assert.Single(building2DOccupancyDatas_Wrong_Kept);
            }
            finally
            {
                //Clean up

                await building2DOccupancyDataPostgreSQLConverter.RemoveAsync([reference], countyId_Target);
                await building2DOccupancyDataPostgreSQLConverter.RemoveAsync([reference], countyId_Wrong);
            }
        }
    }
}
