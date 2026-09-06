using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Stores a building under the wrong polygon part of a county and checks that the repair decides the right one and moves the row onto it.
        /// <para>Skipped by default: it writes to a database, so it needs <c>GIS_PostgreSQL_Main.conf</c> beside the test assembly pointing at a scratch database. Never run it against the deployed one - the converters address fixed table names, so there is no scratch table to fall back on and the clean-up at the end has no undo.</para>
        /// <para>The parts are handed to the decision as objects rather than read from <c>administrative_areal_2d</c>, which is what the repair does as well, so nothing outside <c>building_2d</c> is written here.</para>
        /// <para>Three things are asserted, and each of them is a way the repair could go wrong quietly. The building is decided into the part its footprint lies in rather than the part it was filed under. Its identifier survives the move, because <c>county_id</c> is the partition key and a move between partitions is a delete and an insert underneath - anything holding an <c>id</c> from before has to keep addressing the same record. And a second run reports nothing to move, because a repair that is not idempotent cannot be resumed after an interruption.</para>
        /// </summary>
        [Fact(Skip = "Writes to a database. Point GIS_PostgreSQL_Main.conf at a scratch database before running.")]
        public async Task Building2DCountyPartRefresh_Integration()
        {
            const int countyId_Wrong = 900001;
            const int countyId_Right = 900002;
            const string reference = "COUNTY_PART_REFRESH_INTEGRATION";

            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = GIS.PostgreSQL.Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            // Two parts of one county, a kilometre apart. Only the second contains the building.
            AdministrativeAreal2D administrativeAreal2D_Wrong = AdministrativeAreal2D_SquareWithBoundingBox2D(countyId_Wrong, "9000", 0, 0, 100);
            AdministrativeAreal2D administrativeAreal2D_Right = AdministrativeAreal2D_SquareWithBoundingBox2D(countyId_Right, "9000", 1000, 0, 100);

            List<AdministrativeAreal2D> administrativeAreal2Ds = [administrativeAreal2D_Wrong, administrativeAreal2D_Right];

            try
            {
                IPolygonal2D_Square(1040, 40, 10, out Polygon2D polygon2D);

                PolygonalFace2D? polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D(polygon2D);
                Assert.NotNull(polygonalFace2D);

                GIS.Classes.Building2D building2D_GIS = new(Guid.NewGuid(), reference, polygonalFace2D, 1, null, null, []);

                Building2D? building2D = building2D_GIS.ToPostgreSQL("9000");
                Assert.NotNull(building2D);

                // Filed under the part it does not lie in, which is the defect being repaired.
                building2D.CountyId = countyId_Wrong;

                PostgreSQLUpdateResult? postgreSQLUpdateResult = await building2DPostgreSQLConverter.UpdateAsync([building2D]);
                Assert.NotNull(postgreSQLUpdateResult);
                Assert.Single(postgreSQLUpdateResult.Ids);

                long id_Stored = postgreSQLUpdateResult.Ids.First();

                List<Building2DCountyPartMoveResult>? building2DCountyPartMoveResults = await building2DPostgreSQLConverter.GetCountyPartMovesAsync(administrativeAreal2Ds);
                Assert.NotNull(building2DCountyPartMoveResults);

                Building2DCountyPartMoveResult? building2DCountyPartMoveResult = building2DCountyPartMoveResults.Find(x => x.Reference == reference);
                Assert.NotNull(building2DCountyPartMoveResult);
                Assert.Equal(countyId_Wrong, building2DCountyPartMoveResult.CountyId);
                Assert.Equal(countyId_Right, building2DCountyPartMoveResult.CountyIdResolved);

                HashSet<string>? references_Moved = await building2DPostgreSQLConverter.RefreshCountyIdsAsync([reference], countyId_Right, [countyId_Wrong]);
                Assert.NotNull(references_Moved);
                Assert.Single(references_Moved);
                Assert.Contains(reference, references_Moved);

                List<Building2D>? building2Ds_Right = await building2DPostgreSQLConverter.GetBuilding2DsByCountyIdAsync(countyId_Right);
                Assert.NotNull(building2Ds_Right);

                Building2D? building2D_Right = building2Ds_Right.Find(x => x.Reference == reference);
                Assert.NotNull(building2D_Right);

                // The identifier survives a move between partitions.
                Assert.Equal(id_Stored, building2D_Right.Id);

                // Nothing is left behind under the part it came from.
                List<Building2D>? building2Ds_Wrong = await building2DPostgreSQLConverter.GetBuilding2DsByCountyIdAsync(countyId_Wrong);
                Assert.True(building2Ds_Wrong is null || building2Ds_Wrong.Find(x => x.Reference == reference) is null);

                // A repaired county has nothing left to move, so a second run is a no-op and an interrupted
                // run can simply be run again.
                List<Building2DCountyPartMoveResult>? building2DCountyPartMoveResults_Second = await building2DPostgreSQLConverter.GetCountyPartMovesAsync(administrativeAreal2Ds);
                Assert.NotNull(building2DCountyPartMoveResults_Second);
                Assert.Null(building2DCountyPartMoveResults_Second.Find(x => x.Reference == reference));
            }
            finally
            {
                await building2DPostgreSQLConverter.RemoveAsync([reference], countyId_Wrong);
                await building2DPostgreSQLConverter.RemoveAsync([reference], countyId_Right);
            }
        }

        /// <summary>
        /// Stores a building naming the wrong polygon part of a multi-part county and checks that the write path files it under the part its footprint lies in anyway.
        /// <para>Skipped by default: it writes to a database, and it also writes the two county parts into <c>administrative_areal_2d</c>, because the check is driven by the code the named part belongs to. Point <c>GIS_PostgreSQL_Main.conf</c> at a scratch database before running it.</para>
        /// <para>This is the half of the repair that keeps it repaired. A caller resolves the part of a building by reading the row it is about to overwrite, so for as long as a named part was taken on trust, a row filed under the wrong part rewrote itself there every time it was touched.</para>
        /// </summary>
        [Fact(Skip = "Writes to a database. Point GIS_PostgreSQL_Main.conf at a scratch database before running.")]
        public async Task Building2DCountyPartRefresh_UpdateAsyncChecksNamedPart()
        {
            const string code = "9001";
            const string reference = "COUNTY_PART_NAMED_INTEGRATION";

            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = GIS.PostgreSQL.Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            AdministrativeAreal2D administrativeAreal2D_A = AdministrativeAreal2D_SquareWithBoundingBox2D(0, code, 0, 0, 100);
            AdministrativeAreal2D administrativeAreal2D_B = AdministrativeAreal2D_SquareWithBoundingBox2D(0, code, 1000, 0, 100);

            HashSet<int>? ids = await administrativeAreal2DPostgreSQLConverter.UpdateAsync([administrativeAreal2D_A, administrativeAreal2D_B]);
            Assert.NotNull(ids);
            Assert.Equal(2, ids.Count);

            List<AdministrativeAreal2D>? administrativeAreal2Ds = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByCodeAsync(code, Enums.AdministrativeArealType.County);
            Assert.NotNull(administrativeAreal2Ds);
            Assert.Equal(2, administrativeAreal2Ds.Count);

            // The part holding the square the building sits in, and the one that does not.
            AdministrativeAreal2D? administrativeAreal2D_Right = administrativeAreal2Ds.Find(x => x.BoundingBox2D is not null && x.BoundingBox2D.Min is not null && x.BoundingBox2D.Min.X > 500);
            Assert.NotNull(administrativeAreal2D_Right);

            AdministrativeAreal2D? administrativeAreal2D_Wrong = administrativeAreal2Ds.Find(x => x.Id != administrativeAreal2D_Right.Id);
            Assert.NotNull(administrativeAreal2D_Wrong);

            try
            {
                IPolygonal2D_Square(1040, 40, 10, out Polygon2D polygon2D);

                PolygonalFace2D? polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D(polygon2D);
                Assert.NotNull(polygonalFace2D);

                GIS.Classes.Building2D building2D_GIS = new(Guid.NewGuid(), reference, polygonalFace2D, 1, null, null, []);

                Building2D? building2D = building2D_GIS.ToPostgreSQL(code);
                Assert.NotNull(building2D);

                // The caller names the wrong part, as a client reading a wrongly filed row would.
                building2D.CountyId = administrativeAreal2D_Wrong.Id;

                PostgreSQLUpdateResult? postgreSQLUpdateResult = await building2DPostgreSQLConverter.UpdateAsync([building2D]);
                Assert.NotNull(postgreSQLUpdateResult);
                Assert.Single(postgreSQLUpdateResult.Ids);

                List<Building2D>? building2Ds_Right = await building2DPostgreSQLConverter.GetBuilding2DsByCountyIdAsync(administrativeAreal2D_Right.Id);
                Assert.NotNull(building2Ds_Right);
                Assert.NotNull(building2Ds_Right.Find(x => x.Reference == reference));

                List<Building2D>? building2Ds_Wrong = await building2DPostgreSQLConverter.GetBuilding2DsByCountyIdAsync(administrativeAreal2D_Wrong.Id);
                Assert.True(building2Ds_Wrong is null || building2Ds_Wrong.Find(x => x.Reference == reference) is null);
            }
            finally
            {
                await building2DPostgreSQLConverter.RemoveAsync([reference], administrativeAreal2D_Right.Id);
                await building2DPostgreSQLConverter.RemoveAsync([reference], administrativeAreal2D_Wrong.Id);
            }
        }
    }
}
