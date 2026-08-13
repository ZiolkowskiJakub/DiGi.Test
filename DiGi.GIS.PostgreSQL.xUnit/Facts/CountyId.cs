using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a footprint lying inside the second polygon part of a county is assigned to that part.
        /// <para>A county code names one row per polygon part, so a code can only narrow the candidates - geometry has to decide between them. Picking a part any other way is what filed a whole county's data onto one part and left its siblings reading back empty.</para>
        /// </summary>
        [Fact]
        public void CountyId_FootprintInsideSecondPart()
        {
            AdministrativeAreal2D administrativeAreal2D_A = AdministrativeAreal2D_Square(10, "2405", 0, 0, 100);
            AdministrativeAreal2D administrativeAreal2D_B = AdministrativeAreal2D_Square(20, "2405", 1000, 0, 100);

            // Well inside part B.
            IPolygonal2D_Square(1040, 40, 10, out Polygon2D polygon2D);

            int? countyId = Query.CountyId([administrativeAreal2D_A, administrativeAreal2D_B], polygon2D);

            Assert.Equal(20, countyId);
        }

        /// <summary>
        /// Verifies that the order of the candidates does not change the answer.
        /// </summary>
        [Fact]
        public void CountyId_IndependentOfCandidateOrder()
        {
            AdministrativeAreal2D administrativeAreal2D_A = AdministrativeAreal2D_Square(10, "2405", 0, 0, 100);
            AdministrativeAreal2D administrativeAreal2D_B = AdministrativeAreal2D_Square(20, "2405", 1000, 0, 100);

            IPolygonal2D_Square(1040, 40, 10, out Polygon2D polygon2D);

            Assert.Equal(Query.CountyId([administrativeAreal2D_A, administrativeAreal2D_B], polygon2D), Query.CountyId([administrativeAreal2D_B, administrativeAreal2D_A], polygon2D));
        }

        /// <summary>
        /// Verifies that a footprint outside every part falls back to the nearest one rather than being left unassigned.
        /// </summary>
        [Fact]
        public void CountyId_FootprintOutsideEveryPart()
        {
            AdministrativeAreal2D administrativeAreal2D_A = AdministrativeAreal2D_Square(10, "2405", 0, 0, 100);
            AdministrativeAreal2D administrativeAreal2D_B = AdministrativeAreal2D_Square(20, "2405", 1000, 0, 100);

            // Beyond the right edge of part B, and far from part A.
            IPolygonal2D_Square(1200, 40, 10, out Polygon2D polygon2D);

            int? countyId = Query.CountyId([administrativeAreal2D_A, administrativeAreal2D_B], polygon2D);

            Assert.Equal(20, countyId);
        }

        /// <summary>
        /// Verifies that a single candidate is returned without consulting geometry, and that nothing to decide between yields no answer rather than a guess.
        /// </summary>
        [Fact]
        public void CountyId_DegenerateCandidateSets()
        {
            AdministrativeAreal2D administrativeAreal2D = AdministrativeAreal2D_Square(10, "2405", 0, 0, 100);

            IPolygonal2D_Square(40, 40, 10, out Polygon2D polygon2D);

            Assert.Equal(10, Query.CountyId([administrativeAreal2D], polygon2D));

            Assert.Null(Query.CountyId([], polygon2D));
            Assert.Null(Query.CountyId(null, polygon2D));
            Assert.Null(Query.CountyId([administrativeAreal2D], null));
        }

        /// <summary>
        /// Builds a county row holding one square polygon part.
        /// </summary>
        /// <param name="id">The identifier of the row.</param>
        /// <param name="code">The county code shared by every part of the county.</param>
        /// <param name="x">The X coordinate of the lower left corner.</param>
        /// <param name="y">The Y coordinate of the lower left corner.</param>
        /// <param name="size">The edge length of the square.</param>
        /// <returns>The county row.</returns>
        private static AdministrativeAreal2D AdministrativeAreal2D_Square(int id, string code, double x, double y, double size)
        {
            IPolygonal2D_Square(x, y, size, out Polygon2D polygon2D);

            PolygonalFace2D? polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D(polygon2D);
            Assert.NotNull(polygonalFace2D);

            GIS.Classes.AdministrativeDivision administrativeAreal2D_GIS = new(Guid.NewGuid(), $"REF_{id}", code, polygonalFace2D, GIS.Enums.AdministrativeDivisionType.county, $"part {id}");

            return new AdministrativeAreal2D()
            {
                Id = id,
                Code = code,
                Reference = $"REF_{id}",
                UniqueId = administrativeAreal2D_GIS.UniqueId,
                Object = administrativeAreal2D_GIS.ToJsonObject()
            };
        }

        /// <summary>
        /// Builds a square polygon.
        /// </summary>
        /// <param name="x">The X coordinate of the lower left corner.</param>
        /// <param name="y">The Y coordinate of the lower left corner.</param>
        /// <param name="size">The edge length of the square.</param>
        /// <param name="polygon2D">The resulting polygon.</param>
        private static void IPolygonal2D_Square(double x, double y, double size, out Polygon2D polygon2D)
        {
            List<Point2D> point2Ds =
            [
                new Point2D(x, y),
                new Point2D(x + size, y),
                new Point2D(x + size, y + size),
                new Point2D(x, y + size)
            ];

            polygon2D = new Polygon2D(point2Ds);
        }
    }
}
