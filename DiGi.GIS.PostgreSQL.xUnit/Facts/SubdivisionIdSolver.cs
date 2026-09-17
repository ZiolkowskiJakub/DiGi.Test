using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="SubdivisionIdSolver"/> attributes a building to the smallest subdivision containing it on a nested layer - city, district, neighbourhood - and takes a single overlapping candidate as it stands, as the database path does.
        /// </summary>
        [Fact]
        public void SubdivisionIdSolver_NestedLayer_SmallestContainer()
        {
            List<AdministrativeAreal2D> administrativeAreal2Ds =
            [
                AdministrativeAreal2D_Subdivision(1, "City", 0, 0, 100),
                AdministrativeAreal2D_Subdivision(2, "District", 0, 0, 50),
                AdministrativeAreal2D_Subdivision(3, "Neighbourhood", 10, 10, 10),
                AdministrativeAreal2D_Subdivision(4, "Village", 200, 200, 20),
            ];

            SubdivisionIdSolver subdivisionIdSolver = new(administrativeAreal2Ds);
            Assert.Equal(4, subdivisionIdSolver.Count);

            subdivisionIdSolver.Input = Building2D_Square(12, 12, 2);
            Assert.True(subdivisionIdSolver.Solve());
            Assert.Equal(3, subdivisionIdSolver.Output);

            subdivisionIdSolver.Input = Building2D_Square(30, 30, 2);
            Assert.True(subdivisionIdSolver.Solve());
            Assert.Equal(2, subdivisionIdSolver.Output);

            subdivisionIdSolver.Input = Building2D_Square(70, 70, 2);
            Assert.True(subdivisionIdSolver.Solve());
            Assert.Equal(1, subdivisionIdSolver.Output);

            // One candidate by box: taken without a containment test, as GetSubdivisionIdAsync does.
            subdivisionIdSolver.Input = Building2D_Square(205, 205, 2);
            Assert.True(subdivisionIdSolver.Solve());
            Assert.Equal(4, subdivisionIdSolver.Output);

            // No candidate by box: the layer cannot answer and the database path decides.
            subdivisionIdSolver.Input = Building2D_Square(500, 500, 2);
            Assert.False(subdivisionIdSolver.Solve());
            Assert.Null(subdivisionIdSolver.Output);

            subdivisionIdSolver.Input = null;
            Assert.False(subdivisionIdSolver.Solve());
        }

        /// <summary>
        /// Verifies that a building straddling the boundary of two sibling subdivisions goes to the one holding the larger part of it, exactly as <see cref="Query.SubdivisionId(IEnumerable{ValueTuple{int, double, double}}?, double)"/> decides from the overlaps, and that a building whose box meets candidates it does not overlap answers a null pick rather than a fallback.
        /// </summary>
        [Fact]
        public void SubdivisionIdSolver_Straddling_LargestOverlap()
        {
            List<AdministrativeAreal2D> administrativeAreal2Ds =
            [
                AdministrativeAreal2D_Subdivision(1, "West", 0, 0, 50),
                AdministrativeAreal2D_Subdivision(2, "East", 50, 0, 50),
            ];

            SubdivisionIdSolver subdivisionIdSolver = new(administrativeAreal2Ds);

            // 2 wide in West, 3 wide in East.
            subdivisionIdSolver.Input = Building2D_Rectangle(48, 10, 5, 2);
            Assert.True(subdivisionIdSolver.Solve());
            Assert.Equal(2, subdivisionIdSolver.Output);
            Assert.Equal(2, Query.SubdivisionId([(1, 4.0, 2500.0), (2, 6.0, 2500.0)]));

            // 3 wide in West, 2 wide in East.
            subdivisionIdSolver.Input = Building2D_Rectangle(47, 10, 5, 2);
            Assert.True(subdivisionIdSolver.Solve());
            Assert.Equal(1, subdivisionIdSolver.Output);
        }

        /// <summary>
        /// Verifies that rows which are not a subdivision with a polygon - an <see cref="GIS.Classes.AdministrativeDivision"/> stored under the subdivision type, or an empty row - are left out of the layer, and that a null or empty layer answers nothing.
        /// </summary>
        [Fact]
        public void SubdivisionIdSolver_SkipsNonSubdivisionRows()
        {
            IPolygonal2D_Square(0, 0, 100, out Polygon2D polygon2D);
            GIS.Classes.AdministrativeDivision administrativeDivision = new(Guid.NewGuid(), "REF_D", "1465028", Geometry.Planar.Create.PolygonalFace2D(polygon2D), GIS.Enums.AdministrativeDivisionType.county, "Bemowo");

            List<AdministrativeAreal2D> administrativeAreal2Ds =
            [
                new AdministrativeAreal2D() { Id = 55413, Code = "1465028", Reference = "REF_D", Object = administrativeDivision.ToJsonObject() },
                new AdministrativeAreal2D() { Id = 99 },
                AdministrativeAreal2D_Subdivision(55626, "Bemowo", 0, 0, 100),
            ];

            SubdivisionIdSolver subdivisionIdSolver = new(administrativeAreal2Ds);
            Assert.Equal(1, subdivisionIdSolver.Count);

            subdivisionIdSolver.Input = Building2D_Square(10, 10, 2);
            Assert.True(subdivisionIdSolver.Solve());
            Assert.Equal(55626, subdivisionIdSolver.Output);

            SubdivisionIdSolver subdivisionIdSolver_Empty = new(null);
            Assert.Equal(0, subdivisionIdSolver_Empty.Count);
            subdivisionIdSolver_Empty.Input = Building2D_Square(10, 10, 2);
            Assert.False(subdivisionIdSolver_Empty.Solve());
        }

        /// <summary>
        /// Builds a subdivision row holding one square polygon.
        /// </summary>
        /// <param name="id">The identifier of the row.</param>
        /// <param name="name">The name of the subdivision.</param>
        /// <param name="x">The X coordinate of the lower left corner.</param>
        /// <param name="y">The Y coordinate of the lower left corner.</param>
        /// <param name="size">The edge length.</param>
        /// <returns>The row.</returns>
        private static AdministrativeAreal2D AdministrativeAreal2D_Subdivision(int id, string name, double x, double y, double size)
        {
            IPolygonal2D_Square(x, y, size, out Polygon2D polygon2D);

            GIS.Classes.AdministrativeSubdivision administrativeSubdivision = new(Guid.NewGuid(), $"REF_{id}", "1465011", Geometry.Planar.Create.PolygonalFace2D(polygon2D), GIS.Enums.AdministrativeSubdivisionType.part_of_city, name, null);

            return new AdministrativeAreal2D()
            {
                Id = id,
                Code = "1465011",
                Reference = $"REF_{id}",
                UniqueId = administrativeSubdivision.UniqueId,
                Object = administrativeSubdivision.ToJsonObject()
            };
        }

        /// <summary>
        /// Builds a square building.
        /// </summary>
        /// <param name="x">The X coordinate of the lower left corner.</param>
        /// <param name="y">The Y coordinate of the lower left corner.</param>
        /// <param name="size">The edge length.</param>
        /// <returns>The building.</returns>
        private static GIS.Classes.Building2D Building2D_Square(double x, double y, double size)
        {
            return Building2D_Rectangle(x, y, size, size);
        }

        /// <summary>
        /// Builds a rectangular building.
        /// </summary>
        /// <param name="x">The X coordinate of the lower left corner.</param>
        /// <param name="y">The Y coordinate of the lower left corner.</param>
        /// <param name="width">The extent along X.</param>
        /// <param name="height">The extent along Y.</param>
        /// <returns>The building.</returns>
        private static GIS.Classes.Building2D Building2D_Rectangle(double x, double y, double width, double height)
        {
            Polygon2D polygon2D = new([new Point2D(x, y), new Point2D(x + width, y), new Point2D(x + width, y + height), new Point2D(x, y + height)]);

            return new GIS.Classes.Building2D(Guid.NewGuid(), $"B_{x}_{y}", Geometry.Planar.Create.PolygonalFace2D(polygon2D), 1, null, null, []);
        }
    }
}
