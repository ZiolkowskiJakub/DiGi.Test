using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using DiGi.GIS.PostgreSQL.Classes;
using System;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the face of each row is derived once and keyed by the identifier of the row it came from.
        /// </summary>
        [Fact]
        public void PolygonalFace2DsById_KeyedByRowId()
        {
            AdministrativeAreal2D administrativeAreal2D_A = AdministrativeAreal2D_Square(10, "2405", 0, 0, 100);
            AdministrativeAreal2D administrativeAreal2D_B = AdministrativeAreal2D_Square(20, "2405", 1000, 0, 100);

            Dictionary<int, PolygonalFace2D> polygonalFace2Ds_ById = new List<AdministrativeAreal2D> { administrativeAreal2D_A, administrativeAreal2D_B }.PolygonalFace2DsById();

            Assert.Equal(2, polygonalFace2Ds_ById.Count);
            Assert.True(polygonalFace2Ds_ById.ContainsKey(10));
            Assert.True(polygonalFace2Ds_ById.ContainsKey(20));

            Assert.True(polygonalFace2Ds_ById[10].InRange(new Point2D(50, 50)));
            Assert.True(polygonalFace2Ds_ById[20].InRange(new Point2D(1050, 50)));
        }

        /// <summary>
        /// Verifies that the hole of a face survives the derivation, which is what separates this from the overload that keeps only the outer ring.
        /// </summary>
        [Fact]
        public void PolygonalFace2DsById_KeepsHoles()
        {
            AdministrativeAreal2D administrativeAreal2D = AdministrativeAreal2D_SquareWithHole(30, "2405", 0, 0, 100, 40, 20);

            Dictionary<int, PolygonalFace2D> polygonalFace2Ds_ById = new List<AdministrativeAreal2D> { administrativeAreal2D }.PolygonalFace2DsById();

            Assert.Single(polygonalFace2Ds_ById);

            PolygonalFace2D polygonalFace2D = polygonalFace2Ds_ById[30];
            Assert.NotNull(polygonalFace2D.InternalEdges);
            Assert.Single(polygonalFace2D.InternalEdges);

            Assert.True(polygonalFace2D.InRange(new Point2D(10, 10)));
            Assert.False(polygonalFace2D.InRange(new Point2D(50, 50)));
        }

        /// <summary>
        /// Verifies that a row whose geometry cannot be read is left out rather than keyed to nothing.
        /// </summary>
        [Fact]
        public void PolygonalFace2DsById_SkipsUnreadableRows()
        {
            AdministrativeAreal2D administrativeAreal2D_Valid = AdministrativeAreal2D_Square(10, "2405", 0, 0, 100);
            AdministrativeAreal2D administrativeAreal2D_Empty = new() { Id = 99, Code = "2405", Reference = "REF_99" };

            Dictionary<int, PolygonalFace2D> polygonalFace2Ds_ById = new List<AdministrativeAreal2D> { administrativeAreal2D_Valid, administrativeAreal2D_Empty }.PolygonalFace2DsById();

            Assert.Single(polygonalFace2Ds_ById);
            Assert.True(polygonalFace2Ds_ById.ContainsKey(10));

            Assert.Empty(((IEnumerable<AdministrativeAreal2D>?)null).PolygonalFace2DsById());
        }

        /// <summary>
        /// Builds a row holding one square polygon with a square hole cut out of its middle.
        /// </summary>
        /// <param name="id">The identifier of the row.</param>
        /// <param name="code">The code shared by every part of the unit.</param>
        /// <param name="x">The X coordinate of the lower left corner of the outer square.</param>
        /// <param name="y">The Y coordinate of the lower left corner of the outer square.</param>
        /// <param name="size">The edge length of the outer square.</param>
        /// <param name="offset_Hole">The distance from the lower left corner to the lower left corner of the hole.</param>
        /// <param name="size_Hole">The edge length of the hole.</param>
        /// <returns>The row.</returns>
        private static AdministrativeAreal2D AdministrativeAreal2D_SquareWithHole(int id, string code, double x, double y, double size, double offset_Hole, double size_Hole)
        {
            IPolygonal2D_Square(x, y, size, out Polygon2D polygon2D_External);
            IPolygonal2D_Square(x + offset_Hole, y + offset_Hole, size_Hole, out Polygon2D polygon2D_Internal);

            List<IPolygonal2D> polygonal2Ds_Internal = [polygon2D_Internal];

            PolygonalFace2D? polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D(polygon2D_External, polygonal2Ds_Internal);
            Assert.NotNull(polygonalFace2D);

            GIS.Classes.AdministrativeDivision administrativeDivision = new(Guid.NewGuid(), $"REF_{id}", code, polygonalFace2D, GIS.Enums.AdministrativeDivisionType.county, $"part {id}");

            return new AdministrativeAreal2D()
            {
                Id = id,
                Code = code,
                Reference = $"REF_{id}",
                UniqueId = administrativeDivision.UniqueId,
                Object = administrativeDivision.ToJsonObject()
            };
        }
    }
}
