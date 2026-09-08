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

        /// <summary>
        /// Builds a county polygon part as a square, with the stored extent a row of the table carries.
        /// </summary>
        /// <param name="id">The identifier of the part.</param>
        /// <param name="code">The county code the part belongs to.</param>
        /// <param name="x">The lower X coordinate of the square.</param>
        /// <param name="y">The lower Y coordinate of the square.</param>
        /// <param name="size">The edge length of the square.</param>
        /// <returns>The county row.</returns>
        private static AdministrativeAreal2D AdministrativeAreal2D_SquareWithBoundingBox2D(int id, string code, double x, double y, double size)
        {
            AdministrativeAreal2D administrativeAreal2D = AdministrativeAreal2D_Square(id, code, x, y, size);
            administrativeAreal2D.BoundingBox2D = BoundingBox2D_Square(x, y, size);

            return administrativeAreal2D;
        }

        /// <summary>
        /// Builds the bounding box of a square.
        /// </summary>
        /// <param name="x">The lower X coordinate of the square.</param>
        /// <param name="y">The lower Y coordinate of the square.</param>
        /// <param name="size">The edge length of the square.</param>
        /// <returns>The bounding box.</returns>
        private static BoundingBox2D BoundingBox2D_Square(double x, double y, double size)
        {
            return new BoundingBox2D(new Point2D(x, y), new Point2D(x + size, y + size));
        }
    }
}
