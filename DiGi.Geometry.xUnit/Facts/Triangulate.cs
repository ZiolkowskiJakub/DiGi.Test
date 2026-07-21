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
    }
}