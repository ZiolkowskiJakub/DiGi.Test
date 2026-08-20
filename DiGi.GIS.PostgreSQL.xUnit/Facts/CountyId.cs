using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using DiGi.Geometry.Spatial.Classes;
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
            Assert.Null(Query.CountyId([administrativeAreal2D], null));

            // A bare null cannot pick between the two overloads, so each null case names the one it is testing.
            Assert.Null(Query.CountyId((IEnumerable<AdministrativeAreal2D>?)null, polygon2D));
            Assert.Null(Query.CountyId((IDictionary<int, IPolygonal2D>?)null, polygon2D));
            Assert.Null(Query.CountyId(new Dictionary<int, IPolygonal2D>(), polygon2D));
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
        /// Verifies that deciding from pre-derived polygons gives the same answer as deciding from the county rows themselves.
        /// <para>The pre-derived overload exists only so a caller testing many buildings against the same parts does not deserialize a county-sized geometry per building. It is a performance path, so the answer has to be identical - including where the footprint lies outside every part and the nearest one wins.</para>
        /// </summary>
        [Fact]
        public void CountyId_PolygonalsMatchAdministrativeAreal2Ds()
        {
            AdministrativeAreal2D administrativeAreal2D_A = AdministrativeAreal2D_Square(10, "2405", 0, 0, 100);
            AdministrativeAreal2D administrativeAreal2D_B = AdministrativeAreal2D_Square(20, "2405", 1000, 0, 100);

            List<AdministrativeAreal2D> administrativeAreal2Ds = [administrativeAreal2D_A, administrativeAreal2D_B];

            Dictionary<int, IPolygonal2D> polygonal2Ds_ByCountyId = administrativeAreal2Ds.Polygonal2DsByCountyId();

            Assert.Equal(2, polygonal2Ds_ByCountyId.Count);

            // Inside part B, inside part A, and outside both.
            double[][] coordinates = [[1040, 40], [40, 40], [500, 5000]];

            foreach (double[] coordinate in coordinates)
            {
                IPolygonal2D_Square(coordinate[0], coordinate[1], 10, out Polygon2D polygon2D);

                Assert.Equal(Query.CountyId(administrativeAreal2Ds, polygon2D), Query.CountyId(polygonal2Ds_ByCountyId, polygon2D));
            }
        }

        /// <summary>
        /// Verifies that a footprint straddling two county parts is assigned to the part with the larger overlap area, regardless of candidate order.
        /// </summary>
        [Fact]
        public void CountyId_FootprintStraddlingParts_SelectsLargerOverlap()
        {
            AdministrativeAreal2D administrativeAreal2D_A = AdministrativeAreal2D_Square(10, "2405", 0, 0, 100);
            AdministrativeAreal2D administrativeAreal2D_B = AdministrativeAreal2D_Square(20, "2405", 100, 0, 100);

            // Footprint spanning [60, 110] in X: 80% in Part A (id 10), 20% in Part B (id 20).
            List<Point2D> point2Ds_MajorA =
            [
                new Point2D(60, 40),
                new Point2D(110, 40),
                new Point2D(110, 50),
                new Point2D(60, 50)
            ];
            Polygon2D polygon2D_MajorA = new(point2Ds_MajorA);

            Assert.Equal(10, Query.CountyId([administrativeAreal2D_A, administrativeAreal2D_B], polygon2D_MajorA));
            Assert.Equal(10, Query.CountyId([administrativeAreal2D_B, administrativeAreal2D_A], polygon2D_MajorA));

            // Footprint spanning [90, 140] in X: 20% in Part A (id 10), 80% in Part B (id 20).
            List<Point2D> point2Ds_MajorB =
            [
                new Point2D(90, 40),
                new Point2D(140, 40),
                new Point2D(140, 50),
                new Point2D(90, 50)
            ];
            Polygon2D polygon2D_MajorB = new(point2Ds_MajorB);

            Assert.Equal(20, Query.CountyId([administrativeAreal2D_A, administrativeAreal2D_B], polygon2D_MajorB));
            Assert.Equal(20, Query.CountyId([administrativeAreal2D_B, administrativeAreal2D_A], polygon2D_MajorB));
        }

        /// <summary>
        /// Verifies that a footprint covered whole by more than one candidate is assigned to the smallest of them.
        /// <para>Overlap cannot separate candidates that each hold every square metre of the building - all of their overlaps equal the building - so without this rule the answer falls to the lowest identifier, which is a property of import order rather than of geography. The identifiers are therefore assigned both ways round here: a pass that came from the identifier tie-break would fail one of the two.</para>
        /// <para>Nesting reaches this method for real. A candidate polygon is a stored area's external edge, so an area with a hole punched in it arrives solid and contains whatever sits in that hole, and the bounding box search feeding the converters answers with areas that overlap rather than sibling parts of one code.</para>
        /// </summary>
        [Fact]
        public void CountyId_FootprintCoveredBySeveralParts_SelectsSmallestPart()
        {
            // Well inside both the large square and the small one nested in it.
            IPolygonal2D_Square(60, 60, 10, out Polygon2D polygon2D);

            AdministrativeAreal2D administrativeAreal2D_Large = AdministrativeAreal2D_Square(10, "2412", 0, 0, 200);
            AdministrativeAreal2D administrativeAreal2D_Small = AdministrativeAreal2D_Square(20, "2412", 50, 50, 50);

            Assert.Equal(20, Query.CountyId([administrativeAreal2D_Large, administrativeAreal2D_Small], polygon2D));
            Assert.Equal(20, Query.CountyId([administrativeAreal2D_Small, administrativeAreal2D_Large], polygon2D));

            // The same geometry with the identifiers swapped, so the smallest still answers when it is also
            // the lowest identifier - and the previous pair cannot have come from the identifier alone.
            AdministrativeAreal2D administrativeAreal2D_Large_Swapped = AdministrativeAreal2D_Square(20, "2412", 0, 0, 200);
            AdministrativeAreal2D administrativeAreal2D_Small_Swapped = AdministrativeAreal2D_Square(10, "2412", 50, 50, 50);

            Assert.Equal(10, Query.CountyId([administrativeAreal2D_Large_Swapped, administrativeAreal2D_Small_Swapped], polygon2D));
            Assert.Equal(10, Query.CountyId([administrativeAreal2D_Small_Swapped, administrativeAreal2D_Large_Swapped], polygon2D));

            // Only the large square covers this one whole - the small square holds half of it - so the rule
            // does not apply and the larger overlap decides, which is the large square.
            IPolygonal2D_Square(95, 60, 10, out Polygon2D polygon2D_Straddling);

            Assert.Equal(10, Query.CountyId([administrativeAreal2D_Large, administrativeAreal2D_Small], polygon2D_Straddling));
            Assert.Equal(10, Query.CountyId([administrativeAreal2D_Small, administrativeAreal2D_Large], polygon2D_Straddling));
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

        /// <summary>
        /// Verifies that a building tested as the rectangle of its 3D bounding box is assigned the same part its footprint would be.
        /// <para>A <see cref="CityGML.Classes.Building"/> carries no footprint, so <see cref="BuildingPostgreSQLConverter"/> tests it as the X and Y extent of the <see cref="BoundingBox3D"/> it already stores - the shape this fact builds. County parts lie kilometres apart, so the rectangle and a true footprint can only disagree for a building sitting on a part boundary, and even then the larger overlap still decides.</para>
        /// </summary>
        [Fact]
        public void CountyId_BuildingBoundingBoxSelectsPart()
        {
            AdministrativeAreal2D administrativeAreal2D_A = AdministrativeAreal2D_Square(10, "2412", 0, 0, 100);
            AdministrativeAreal2D administrativeAreal2D_B = AdministrativeAreal2D_Square(20, "2412", 100, 0, 100);

            Dictionary<int, IPolygonal2D> polygonal2Ds_ByCountyId = new List<AdministrativeAreal2D>([administrativeAreal2D_A, administrativeAreal2D_B]).Polygonal2DsByCountyId();

            // Well inside part B, at an elevation that must not reach the decision.
            Assert.Equal(20, polygonal2Ds_ByCountyId.CountyId(Polygonal2D_BoundingBox3D(140, 40, 150, 50, 120, 135)));

            // Spanning [60, 110] in X: four fifths of it lies in part A.
            Assert.Equal(10, polygonal2Ds_ByCountyId.CountyId(Polygonal2D_BoundingBox3D(60, 40, 110, 50, 120, 135)));

            // Spanning [90, 140] in X: four fifths of it lies in part B.
            Assert.Equal(20, polygonal2Ds_ByCountyId.CountyId(Polygonal2D_BoundingBox3D(90, 40, 140, 50, 120, 135)));

            // Beyond the right edge of part B and far from part A - the nearest part answers rather than nothing.
            Assert.Equal(20, polygonal2Ds_ByCountyId.CountyId(Polygonal2D_BoundingBox3D(300, 40, 310, 50, 120, 135)));
        }

        /// <summary>
        /// Builds the shape a building is tested as: the X and Y extent of its 3D bounding box.
        /// </summary>
        /// <param name="minX">The lower X coordinate.</param>
        /// <param name="minY">The lower Y coordinate.</param>
        /// <param name="maxX">The upper X coordinate.</param>
        /// <param name="maxY">The upper Y coordinate.</param>
        /// <param name="minZ">The lower Z coordinate, which the decision must ignore.</param>
        /// <param name="maxZ">The upper Z coordinate, which the decision must ignore.</param>
        /// <returns>The rectangle of the bounding box in X and Y.</returns>
        private static IPolygonal2D? Polygonal2D_BoundingBox3D(double minX, double minY, double maxX, double maxY, double minZ, double maxZ)
        {
            BoundingBox3D boundingBox3D = new(new Point3D(minX, minY, minZ), new Point3D(maxX, maxY, maxZ));

            BoundingBox2D boundingBox2D = new(new Point2D(boundingBox3D.MinX, boundingBox3D.MinY), new Point2D(boundingBox3D.MaxX, boundingBox3D.MaxY));

            return (Polygon2D?)boundingBox2D;
        }
    }
}
