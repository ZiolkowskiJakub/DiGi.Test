using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the extents narrow the parts of a county to the one that can hold a building, and that the answer is the same one the containment test gives.
        /// <para>This is what lets a repair decide a county of a hundred thousand buildings without deserializing a footprint: a part whose bounding box does not reach a building cannot contain it, so where one part is left the extents have already answered. The fact that matters is not the narrowing itself but that it agrees with <see cref="Query.CountyId(IDictionary{int, IPolygonal2D}, IPolygonal2D, double)"/> - a cheaper decision that disagreed would be a different decision.</para>
        /// </summary>
        [Fact]
        public void CountyIds_NarrowsToTheReachablePart()
        {
            AdministrativeAreal2D administrativeAreal2D_A = AdministrativeAreal2D_SquareWithBoundingBox2D(10, "3020", 0, 0, 100);
            AdministrativeAreal2D administrativeAreal2D_B = AdministrativeAreal2D_SquareWithBoundingBox2D(20, "3020", 1000, 0, 100);

            List<AdministrativeAreal2D> administrativeAreal2Ds = [administrativeAreal2D_A, administrativeAreal2D_B];

            // Well inside part B: only part B is left, and no footprint is needed to say so.
            List<int> countyIds = administrativeAreal2Ds.CountyIds(BoundingBox2D_Square(1040, 40, 10));
            Assert.Single(countyIds);
            Assert.Equal(20, countyIds[0]);

            // The same building through the full containment test, which reads its footprint.
            IPolygonal2D_Square(1040, 40, 10, out Polygon2D polygon2D);
            Assert.Equal(20, administrativeAreal2Ds.Polygonal2DsByCountyId().CountyId(polygon2D));
        }

        /// <summary>
        /// Verifies that a building reaching both parts leaves both of them standing, so that the footprint decides.
        /// </summary>
        [Fact]
        public void CountyIds_KeepsEveryPartTheBuildingReaches()
        {
            AdministrativeAreal2D administrativeAreal2D_A = AdministrativeAreal2D_SquareWithBoundingBox2D(10, "2412", 0, 0, 100);
            AdministrativeAreal2D administrativeAreal2D_B = AdministrativeAreal2D_SquareWithBoundingBox2D(20, "2412", 100, 0, 100);

            List<AdministrativeAreal2D> administrativeAreal2Ds = [administrativeAreal2D_A, administrativeAreal2D_B];

            List<int> countyIds = administrativeAreal2Ds.CountyIds(BoundingBox2D_Square(90, 40, 20));

            Assert.Equal(2, countyIds.Count);
            Assert.Contains(10, countyIds);
            Assert.Contains(20, countyIds);
        }

        /// <summary>
        /// Verifies that a building no part reaches narrows to nothing, rather than to an arbitrary part.
        /// <para>An empty answer is what tells the caller to hand every part to the containment test, so that the nearest one is found. Narrowing to one part here would file a building on the far side of the country under whichever part happened to be listed first.</para>
        /// </summary>
        [Fact]
        public void CountyIds_KeepsNoPartWhenNoneReachesTheBuilding()
        {
            AdministrativeAreal2D administrativeAreal2D_A = AdministrativeAreal2D_SquareWithBoundingBox2D(10, "0418", 0, 0, 100);
            AdministrativeAreal2D administrativeAreal2D_B = AdministrativeAreal2D_SquareWithBoundingBox2D(20, "0418", 1000, 0, 100);

            List<AdministrativeAreal2D> administrativeAreal2Ds = [administrativeAreal2D_A, administrativeAreal2D_B];

            Assert.Empty(administrativeAreal2Ds.CountyIds(BoundingBox2D_Square(500_000, 500_000, 10)));
        }

        /// <summary>
        /// Verifies that a part storing no extent, and a building storing none, both leave every part standing.
        /// <para>Nothing is known in either case, so nothing can be deduced. Ruling a part out here would be a guess, and the guess would move rows.</para>
        /// </summary>
        [Fact]
        public void CountyIds_KeepsEveryPartWhenAnExtentIsMissing()
        {
            AdministrativeAreal2D administrativeAreal2D_A = AdministrativeAreal2D_SquareWithBoundingBox2D(10, "2401", 0, 0, 100);
            AdministrativeAreal2D administrativeAreal2D_B = AdministrativeAreal2D_SquareWithBoundingBox2D(20, "2401", 1000, 0, 100);
            administrativeAreal2D_B.BoundingBox2D = null;

            List<AdministrativeAreal2D> administrativeAreal2Ds = [administrativeAreal2D_A, administrativeAreal2D_B];

            // The building sits in part A, but part B stores no extent and cannot be ruled out.
            List<int> countyIds = administrativeAreal2Ds.CountyIds(BoundingBox2D_Square(40, 40, 10));
            Assert.Equal(2, countyIds.Count);

            // A building storing no extent leaves both standing for the same reason.
            Assert.Equal(2, administrativeAreal2Ds.CountyIds(null).Count);
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
