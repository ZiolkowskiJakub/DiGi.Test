using DiGi.Geometry.Planar.Classes;
using System.Reflection;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that triangulating a polygonal face with an internal hole (a courtyard footprint) conserves the whole face area.
        /// <para>Regression guard for the triangulation that used to filter the conforming Delaunay triangles by a strict whole-triangle containment test: triangles that shared an edge with the face boundary were dropped, punching holes in the cap of extruded courtyard buildings. The triangulated area must equal the face area (external area minus the hole area).</para>
        /// </summary>
        [Fact]
        public void Triangulate_PolygonalFace2DWithHole()
        {
            // External boundary 30 x 20 (area 600) with a 10 x 10 internal hole (area 100): face area 500.
            Polygon2D polygon2D_External = new(
            [
                new Point2D(0, 0),
                new Point2D(30, 0),
                new Point2D(30, 20),
                new Point2D(0, 20)
            ]);

            Polygon2D polygon2D_Hole = new(
            [
                new Point2D(10, 5),
                new Point2D(20, 5),
                new Point2D(20, 15),
                new Point2D(10, 15)
            ]);

            PolygonalFace2D? polygonalFace2D = Planar.Create.PolygonalFace2D(polygon2D_External, [polygon2D_Hole]);
            Assert.NotNull(polygonalFace2D);

            double faceArea = polygonalFace2D.GetArea();
            Assert.Equal(500, faceArea, 6);

            // Triangulate at the distance tolerance used by the GLTF rendering pipeline (the path that
            // produced the reported missing roof triangles).
            List<Triangle2D>? triangle2Ds = polygonalFace2D.Triangulate(DiGi.Core.Constants.Tolerance.Distance);
            Assert.NotNull(triangle2Ds);
            Assert.NotEmpty(triangle2Ds);

            double triangulatedArea = 0;
            foreach (Triangle2D triangle2D in triangle2Ds)
            {
                triangulatedArea += triangle2D.GetArea();
            }

            // The triangles must tile the whole face without gaps: a dropped triangle shows up as a
            // deficit in the summed area. A relative tolerance of 0.1% catches the historical losses
            // (35% and 9.7%) with a wide margin while tolerating floating point rounding.
            Assert.True(System.Math.Abs(faceArea - triangulatedArea) <= faceArea * 1e-3, $"Triangulated area {triangulatedArea} does not match the face area {faceArea}; triangles were dropped.");
        }

        /// <summary>
        /// Tests that triangulating the real courtyard building footprint (an offset, non axis aligned polygonal face with a hole, expressed in GIS coordinates) conserves the whole face area.
        /// <para>This fixture reproduced the missing roof triangles reported on the 3D view: at GIS scale the conforming Delaunay vertices snap onto the face boundary, so the previous strict whole-triangle containment filter dropped roughly a third of the cap. The triangulated area must equal the face area within a tight tolerance.</para>
        /// </summary>
        [Fact]
        public void Triangulate_PolygonalFace2DWithHole_RealBuilding()
        {
            string? path = DiGi.Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "PolygonalFace2D_CourtyardBuilding.json");
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }

            PolygonalFace2D? polygonalFace2D = DiGi.Core.Convert.ToDiGi<PolygonalFace2D>((DiGi.Core.Classes.Path)path)?.FirstOrDefault();
            Assert.NotNull(polygonalFace2D);

            double faceArea = polygonalFace2D.GetArea();
            Assert.True(faceArea > 0);

            // Triangulate at the distance tolerance used by the GLTF rendering pipeline (the path that
            // produced the reported missing roof triangles).
            List<Triangle2D>? triangle2Ds = polygonalFace2D.Triangulate(DiGi.Core.Constants.Tolerance.Distance);
            Assert.NotNull(triangle2Ds);
            Assert.NotEmpty(triangle2Ds);

            double triangulatedArea = 0;
            foreach (Triangle2D triangle2D in triangle2Ds)
            {
                triangulatedArea += triangle2D.GetArea();
            }

            Assert.True(System.Math.Abs(faceArea - triangulatedArea) <= faceArea * 1e-3, $"Triangulated area {triangulatedArea} does not match the face area {faceArea}; roof triangles were dropped.");
        }

        /// <summary>
        /// Tests that a ring carrying corners closer together than the tolerance is triangulated rather than taking the process down.
        /// <para>Regression guard for a stack overflow, which is not a catchable exception: the triangulation snaps to a grid of the tolerance it is given, so a ring whose corners sit a fraction of that apart came back out of the overlay unchanged and the routine recursed on it until the stack gave out. The fixture is a real remainder, in PL-1992 coordinates, left by cutting the outlines of several neighbouring buildings out of one terrain triangle - it carries three such clusters, the closest pair 3e-7 apart.</para>
        /// </summary>
        [Fact]
        public void Triangulate_SubToleranceCorners()
        {
            Polygon2D polygon2D = new(
            [
                new Point2D(629111.237625244, 489322.475250488),
                new Point2D(629129.5951417107, 489359.19028342154),
                new Point2D(629131.455, 489359.36),
                new Point2D(629135.6199999795, 489359.7399999981),
                new Point2D(629135.6200003031, 489359.73999682313),
                new Point2D(629139.77, 489360.135),
                new Point2D(629143.92, 489360.53),
                new Point2D(629143.525, 489364.705),
                new Point2D(629143.1300003032, 489368.87999682315),
                new Point2D(629143.1300003006, 489368.8799968229),
                new Point2D(629143.13, 489368.88),
                new Point2D(629138.95, 489368.48),
                new Point2D(629134.7699996969, 489368.0800031769),
                new Point2D(629134.7700000205, 489368.080000002),
                new Point2D(629134.77, 489368.08),
                new Point2D(629134.0002658227, 489368.0005316456),
                new Point2D(629150, 489400),
                new Point2D(629150, 489350),
                new Point2D(629131.5082684177, 489331.5082684176),
                new Point2D(629130.0298241151, 489331.3615930516),
                new Point2D(629130.0299859374, 489331.35999883636),
                new Point2D(629130.0296130513, 489331.35996133345),
                new Point2D(629130.1511853237, 489330.1511853237),
                new Point2D(629115.9224593076, 489315.9224593076),
                new Point2D(629115.6646376164, 489318.66385319375),
                new Point2D(629115.27, 489322.86)
            ]);

            List<NetTopologySuite.Geometries.Polygon>? polygons = Planar.Query.Triangulate(Planar.Convert.ToNTS_Polygon(polygon2D), DiGi.Core.Constants.Tolerance.Distance);
            Assert.NotNull(polygons);
            Assert.NotEmpty(polygons);

            double area = 0;
            foreach (NetTopologySuite.Geometries.Polygon polygon in polygons)
            {
                Assert.Equal(4, polygon.Coordinates.Length);
                area += polygon.Area;
            }

            double area_Expected = Planar.Convert.ToNTS_Polygon(polygon2D)!.Area;
            Assert.True(area > area_Expected * 0.99, $"Triangulated area {area} lost too much of the {area_Expected} the ring encloses.");
        }
    }
}
