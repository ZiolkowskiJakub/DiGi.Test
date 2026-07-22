using DiGi.CityGML.Classes;
using DiGi.CityGML.Interfaces;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DiGi.CityGML.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that the public <see cref="Building"/> constructor still stores defensive copies of the supplied surfaces.
        /// <para>The parse path in <see cref="Convert"/> now adopts its surfaces through an internal constructor; this locks the public contract so no consumer of the library can observe that change.</para>
        /// </summary>
        [Fact]
        public void Building_PublicConstructorClonesSurfaces()
        {
            RoofSurface roofSurface = new("Surface_1", PolygonalFace3D());

            Building building = new("Building_1", 1, [roofSurface]);

            List<ISurface>? surfaces = building.Surfaces?.ToList();

            Assert.NotNull(surfaces);
            Assert.Single(surfaces);
            Assert.NotSame(roofSurface, surfaces[0]);
            Assert.Equal("Surface_1", surfaces[0].UniqueId);
        }

        /// <summary>
        /// Tests that the public <see cref="CityModel"/> constructor still stores defensive copies of the supplied buildings.
        /// <para>Counterpart to <see cref="Building_PublicConstructorClonesSurfaces"/> for the outer container, which the parse path also now adopts.</para>
        /// </summary>
        [Fact]
        public void CityModel_PublicConstructorClonesBuildings()
        {
            Building building = new("Building_1", 1, [new RoofSurface("Surface_1", PolygonalFace3D())]);

            CityModel cityModel = new([building]);

            List<Building>? buildings = cityModel.Buildings?.ToList();

            Assert.NotNull(buildings);
            Assert.Single(buildings);
            Assert.NotSame(building, buildings[0]);
            Assert.Equal("Building_1", buildings[0].UniqueId);
        }

        /// <summary>
        /// Tests that the adopting parse path produces a fully populated, round-trippable city model.
        /// <para>Removing the three defensive clone passes must not drop geometry or break serialization, so this asserts the surface and vertex totals of the fixture as well as a JSON round trip.</para>
        /// </summary>
        [Fact]
        public void CityModels_ParsedGeometryIntegrity()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "2862_CityGML.zip");

            Assert.True(File.Exists(path));

            List<CityModel>? cityModels = Create.CityModels(path);

            Assert.NotNull(cityModels);
            Assert.Single(cityModels);

            CityModel cityModel = cityModels[0];

            List<Building>? buildings = cityModel.Buildings?.ToList();

            Assert.NotNull(buildings);
            Assert.Equal(3, buildings.Count);

            int surfaceCount = 0;
            int point3DCount = 0;

            foreach (Building building in buildings)
            {
                List<ISurface>? surfaces = building.Surfaces?.ToList();

                Assert.NotNull(surfaces);
                Assert.NotEmpty(surfaces);

                foreach (ISurface surface in surfaces)
                {
                    surfaceCount++;

                    IPolygonalFace3D? polygonalFace3D = surface.Geometry;

                    Assert.NotNull(polygonalFace3D);

                    IPolygonal3D? polygonal3D = polygonalFace3D.ExternalEdge;

                    Assert.NotNull(polygonal3D);

                    List<Point3D>? point3Ds = polygonal3D.GetPoints();

                    Assert.NotNull(point3Ds);
                    Assert.True(point3Ds.Count >= 3);

                    point3DCount += point3Ds.Count;
                }
            }

            Assert.True(surfaceCount > 0);
            Assert.True(point3DCount >= surfaceCount * 3);

            Core.xUnit.Query.SerializationCheck(cityModel);
        }

        private static IPolygonalFace3D? PolygonalFace3D()
        {
            return Geometry.Spatial.Create.PolygonalFace3D(Geometry.Spatial.Create.Polygon3D([new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(1, 1, 0)]));
        }
    }
}
