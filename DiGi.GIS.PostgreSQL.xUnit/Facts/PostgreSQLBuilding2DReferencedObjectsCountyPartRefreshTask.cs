using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a referenced object filed under the wrong county part is reported by a dry run without being written, moved onto the part holding its building by a live run with its identifier preserved, and reported nothing on a re-run.
        /// <para>Skipped by default: it writes to a database, so it needs <c>GIS_PostgreSQL_Main.conf</c> beside the test assembly pointing at a scratch database. Never run it against the deployed one - the converters address fixed table names, so there is no scratch table to fall back on and the clean-up at the end has no undo.</para>
        /// <para>The dry run is the default the tray offers and has to be provably read-only: it names the stray, and the row it would move keeps its identifier and its part. The live run is the repair the mover exists for, and the re-run is the idempotence that makes the run safe to repeat - a mover that still reports what it already moved is one to be afraid to run.</para>
        /// </summary>
        [Fact(Skip = "Writes to a database. Point GIS_PostgreSQL_Main.conf at a scratch database before running.")]
        public async Task PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshTask_Integration()
        {
            const string code = "9001";
            const string reference = "REFERENCED_OBJECTS_COUNTY_PART_INTEGRATION";

            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = GIS.PostgreSQL.Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            Building2DOccupancyDataPostgreSQLConverter? building2DOccupancyDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DOccupancyDataPostgreSQLConverter>();
            Assert.NotNull(building2DOccupancyDataPostgreSQLConverter);

            //Two parts of one county code, one building sitting in the part that does not hold its occupancy object

            AdministrativeAreal2D administrativeAreal2D_A = AdministrativeAreal2D_SquareWithBoundingBox2D(0, code, 0, 0, 100);
            AdministrativeAreal2D administrativeAreal2D_B = AdministrativeAreal2D_SquareWithBoundingBox2D(0, code, 1000, 0, 100);

            HashSet<int>? ids = await administrativeAreal2DPostgreSQLConverter.UpdateAsync([administrativeAreal2D_A, administrativeAreal2D_B]);
            Assert.NotNull(ids);
            Assert.Equal(2, ids.Count);

            List<AdministrativeAreal2D>? administrativeAreal2Ds = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByCodeAsync(code, Enums.AdministrativeArealType.County);
            Assert.NotNull(administrativeAreal2Ds);

            List<AdministrativeAreal2D> administrativeAreal2Ds_Ours = administrativeAreal2Ds.FindAll(x => ids.Contains(x.Id));
            Assert.Equal(2, administrativeAreal2Ds_Ours.Count);

            //The part holding the square the building sits in, and the one that does not

            AdministrativeAreal2D? administrativeAreal2D_Right = administrativeAreal2Ds_Ours.Find(x => x.BoundingBox2D is not null && x.BoundingBox2D.Min is not null && x.BoundingBox2D.Min.X > 500);
            Assert.NotNull(administrativeAreal2D_Right);

            AdministrativeAreal2D? administrativeAreal2D_Wrong = administrativeAreal2Ds_Ours.Find(x => x.Id != administrativeAreal2D_Right.Id);
            Assert.NotNull(administrativeAreal2D_Wrong);

            try
            {
                IPolygonal2D_Square(1040, 40, 10, out Polygon2D polygon2D);

                PolygonalFace2D? polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D(polygon2D);
                Assert.NotNull(polygonalFace2D);

                GIS.Classes.Building2D building2D_GIS = new(Guid.NewGuid(), reference, polygonalFace2D, 1, null, null, []);

                Building2D? building2D = building2D_GIS.ToPostgreSQL(code);
                Assert.NotNull(building2D);

                building2D.CountyId = administrativeAreal2D_Right.Id;

                PostgreSQLUpdateResult? updateResult_Building = await building2DPostgreSQLConverter.UpdateAsync([building2D]);
                Assert.NotNull(updateResult_Building);
                Assert.Single(updateResult_Building.Ids);

                //The building's occupancy object sits under the other part of the county, so a read for the part holding the building finds nothing

                GIS.Classes.OccupancyData occupancyData_Stray = new(reference, 120.5, 3);

                Building2DOccupancyData? building2DOccupancyData_Stray = occupancyData_Stray.ToPostgreSQL(administrativeAreal2D_Wrong.Id);
                Assert.NotNull(building2DOccupancyData_Stray);

                PostgreSQLUpdateResult? updateResult_Stray = await building2DOccupancyDataPostgreSQLConverter.UpdateAsync([building2DOccupancyData_Stray]);
                Assert.NotNull(updateResult_Stray);
                Assert.Single(updateResult_Stray.Ids);

                long id_Stored = updateResult_Stray.Ids.First();

                //A dry run reports the stray and writes nothing

                PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult? result_DryRun = await RunPostgreSQLBuilding2DReferencedObjectsCountyPartRefreshTaskAsync(gISPostgreSQLConverterManager, code, true);
                Assert.NotNull(result_DryRun);
                Assert.Equal(1, result_DryRun.CodeCount);
                Assert.Equal(1, result_DryRun.StrayReferenceCount);
                Assert.Equal(1, result_DryRun.MovableReferenceCount);
                Assert.Equal(0, result_DryRun.BlockedReferenceCount);
                Assert.Equal(0, result_DryRun.MovedReferenceCount);
                Assert.Equal(0, result_DryRun.FailedCodeCount);
                Assert.False(result_DryRun.Cancelled);

                List<Building2DOccupancyData>? building2DOccupancyDatas_Wrong = await building2DOccupancyDataPostgreSQLConverter.GetItemsByReferenceAsync(reference, administrativeAreal2D_Wrong.Id);
                Assert.NotNull(building2DOccupancyDatas_Wrong);
                Assert.Single(building2DOccupancyDatas_Wrong);
                Assert.Equal(id_Stored, building2DOccupancyDatas_Wrong.First().Id);
                Assert.Equal(administrativeAreal2D_Wrong.Id, building2DOccupancyDatas_Wrong.First().CountyId);

                List<Building2DOccupancyData>? building2DOccupancyDatas_Right = await building2DOccupancyDataPostgreSQLConverter.GetItemsByReferenceAsync(reference, administrativeAreal2D_Right.Id);
                Assert.NotNull(building2DOccupancyDatas_Right);
                Assert.Empty(building2DOccupancyDatas_Right);

                //A live run moves the stray onto the part holding the building, preserving the identifier

                PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult? result_Live = await RunPostgreSQLBuilding2DReferencedObjectsCountyPartRefreshTaskAsync(gISPostgreSQLConverterManager, code, false);
                Assert.NotNull(result_Live);
                Assert.Equal(1, result_Live.MovableReferenceCount);
                Assert.Equal(0, result_Live.BlockedReferenceCount);
                Assert.Equal(1, result_Live.MovedReferenceCount);
                Assert.Equal(0, result_Live.FailedCodeCount);
                Assert.False(result_Live.Cancelled);

                List<Building2DOccupancyData>? building2DOccupancyDatas_Moved = await building2DOccupancyDataPostgreSQLConverter.GetItemsByReferenceAsync(reference, administrativeAreal2D_Right.Id);
                Assert.NotNull(building2DOccupancyDatas_Moved);
                Assert.Single(building2DOccupancyDatas_Moved);
                Assert.Equal(id_Stored, building2DOccupancyDatas_Moved.First().Id);
                Assert.Equal(administrativeAreal2D_Right.Id, building2DOccupancyDatas_Moved.First().CountyId);

                List<Building2DOccupancyData>? building2DOccupancyDatas_Wrong_Empty = await building2DOccupancyDataPostgreSQLConverter.GetItemsByReferenceAsync(reference, administrativeAreal2D_Wrong.Id);
                Assert.NotNull(building2DOccupancyDatas_Wrong_Empty);
                Assert.Empty(building2DOccupancyDatas_Wrong_Empty);

                //A re-run has nothing left to move, and leaves the row where the live run put it

                PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult? result_Again = await RunPostgreSQLBuilding2DReferencedObjectsCountyPartRefreshTaskAsync(gISPostgreSQLConverterManager, code, false);
                Assert.NotNull(result_Again);
                Assert.Equal(0, result_Again.StrayReferenceCount);
                Assert.Equal(0, result_Again.MovableReferenceCount);
                Assert.Equal(0, result_Again.BlockedReferenceCount);
                Assert.Equal(0, result_Again.MovedReferenceCount);
                Assert.Equal(0, result_Again.FailedCodeCount);
                Assert.False(result_Again.Cancelled);

                List<Building2DOccupancyData>? building2DOccupancyDatas_StillMoved = await building2DOccupancyDataPostgreSQLConverter.GetItemsByReferenceAsync(reference, administrativeAreal2D_Right.Id);
                Assert.NotNull(building2DOccupancyDatas_StillMoved);
                Assert.Single(building2DOccupancyDatas_StillMoved);
                Assert.Equal(id_Stored, building2DOccupancyDatas_StillMoved.First().Id);
            }
            finally
            {
                //Clean up. The county parts stay, as the county part named integration test leaves them - a scratch database is rebuilt, not repaired

                await building2DOccupancyDataPostgreSQLConverter.RemoveAsync([reference], administrativeAreal2D_Right.Id);
                await building2DOccupancyDataPostgreSQLConverter.RemoveAsync([reference], administrativeAreal2D_Wrong.Id);
                await building2DPostgreSQLConverter.RemoveAsync([reference], administrativeAreal2D_Right.Id);
                await building2DPostgreSQLConverter.RemoveAsync([reference], administrativeAreal2D_Wrong.Id);
            }
        }

        /// <summary>
        /// Runs the task once over one county code and returns its result.
        /// </summary>
        /// <param name="gISPostgreSQLConverterManager">The converter manager the run draws its converters from.</param>
        /// <param name="code">The county code the run examines.</param>
        /// <param name="dryRun">Whether the run only reports what it would do.</param>
        /// <returns>The result of the run.</returns>
        private static async Task<PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult> RunPostgreSQLBuilding2DReferencedObjectsCountyPartRefreshTaskAsync(GISPostgreSQLConverterManager gISPostgreSQLConverterManager, string code, bool dryRun)
        {
            PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshTask postgreSQLBuilding2DReferencedObjectsCountyPartRefreshTask = new(gISPostgreSQLConverterManager)
            {
                PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions = new PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions
                {
                    Codes = [code],
                    DryRun = dryRun,
                    ReportDirectory = Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly())
                }
            };

            TaskCompletionSource<bool> taskCompletionSource = new();
            postgreSQLBuilding2DReferencedObjectsCountyPartRefreshTask.Stopped += (object? sender, EventArgs e) => taskCompletionSource.TrySetResult(true);

            postgreSQLBuilding2DReferencedObjectsCountyPartRefreshTask.Start();

            await taskCompletionSource.Task;

            Assert.Null(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshTask.Exception);
            Assert.True(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshTask.IsSucceeded);

            PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult? result = postgreSQLBuilding2DReferencedObjectsCountyPartRefreshTask.PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult;
            Assert.NotNull(result);

            return result;
        }
    }
}
