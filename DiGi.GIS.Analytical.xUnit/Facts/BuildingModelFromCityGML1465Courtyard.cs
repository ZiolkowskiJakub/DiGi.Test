using DiGi.Analytical.Building;
using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.CityGML.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.Classes;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DiGi.GIS.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Reference of the courtyard building of county 1465 (Warsaw) reported by DiGi.GIS.WebAPI.UI#45.
        /// </summary>
        private const string reference_Courtyard_1465 = "38F62224-C903-F520-E053-CA2BA8C0BE14";

        /// <summary>
        /// Tests the model built from the real LOD2 courtyard building of DiGi.GIS.WebAPI.UI#45 - the 5-storey block at the centre of the reported scene.
        /// <para>The CityGML source carries the courtyard as the interior ring of its single ground surface (10 002 square metres outside, 4 664 inside) and no roof surface covers it. The storey split used to assemble the faces on every cutting plane from each section loop separately, so the model gained a solid floor over the courtyard on every storey, the outline lost its internal edge and the terrain of the scene was cut away under the courtyard. The outline of the split model has to be a single face keeping the courtyard as its internal edge, and the scene centre (629671.3, 489136.8) has to stay outside it.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_FromCityGML_1465_Courtyard()
        {
            Building building = CityGML_Building_1465_Courtyard();
            Building2D building2D = Building2D_1465_Courtyard();

            Assert.Equal(5, building2D.Storeys);

            BuildingModel? buildingModel = Create.BuildingModel(building, building2D);
            Assert.NotNull(buildingModel);

            // The storey count of the 2D building cut the model
            List<ISpace>? spaces = buildingModel.GetSpaces<ISpace>();
            Assert.NotNull(spaces);
            Assert.True(spaces.Count > 1, "The model was not cut into storeys.");

            List<PolygonalFace2D>? polygonalFace2Ds = buildingModel.Footprints(Constants.Tolerance.Coordinate);
            Assert.NotNull(polygonalFace2Ds);
            Assert.Single(polygonalFace2Ds);

            PolygonalFace2D polygonalFace2D = polygonalFace2Ds[0];

            Assert.NotNull(polygonalFace2D.InternalEdges);
            Assert.True(polygonalFace2D.InternalEdges.Count == 1, $"The outline carries {polygonalFace2D.InternalEdges.Count} internal edges instead of the courtyard.");

            double area_Courtyard = polygonalFace2D.InternalEdges[0].GetArea();
            Assert.True(System.Math.Abs(area_Courtyard - 4664) < 50, $"The courtyard covers {area_Courtyard} square metres, the source ring covers 4664.");

            double area = polygonalFace2D.GetArea();
            Assert.True(System.Math.Abs(area - (10002 - 4664)) < 100, $"The outline covers {area} square metres, the source ground surface covers 10002 minus the 4664 of its courtyard.");

            // The centre of the reported scene lies in the courtyard
            Assert.False(polygonalFace2D.InRange(new Point2D(629671.3, 489136.8), Constants.Tolerance.Coordinate), "The scene centre is covered by the outline.");
        }

        /// <summary>
        /// Loads the LOD2 courtyard building of county 1465 reported by DiGi.GIS.WebAPI.UI#45.
        /// </summary>
        /// <returns>The building of the "1465_38F62224-C903_CityGML.gml" fixture.</returns>
        private static Building CityGML_Building_1465_Courtyard()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "1465_38F62224-C903_CityGML.gml");

            Assert.False(string.IsNullOrWhiteSpace(path));
            Assert.True(File.Exists(path));

            List<CityModel>? cityModels = CityGML.Create.CityModels(path);
            Assert.NotNull(cityModels);
            Assert.Single(cityModels);

            Building? result = null;
            if (cityModels[0].Buildings is IEnumerable<Building> buildings)
            {
                foreach (Building building in buildings)
                {
                    if (CityGML.Query.Reference(building) == reference_Courtyard_1465)
                    {
                        result = building;
                        break;
                    }
                }
            }

            Assert.NotNull(result);

            return result;
        }

        /// <summary>
        /// Loads the 2D building pairing with the LOD2 courtyard building of county 1465 reported by DiGi.GIS.WebAPI.UI#45, as served by the deployed WebAPI on 2026-09-18.
        /// </summary>
        /// <returns>The 2D building of the "1465_38F62224-C903_Building2D.json" fixture.</returns>
        private static Building2D Building2D_1465_Courtyard()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "1465_38F62224-C903_Building2D.json");

            Assert.False(string.IsNullOrWhiteSpace(path));
            Assert.True(File.Exists(path));

            List<Building2D>? building2Ds = Core.Convert.ToDiGi<Building2D>(File.ReadAllText(path!));

            Assert.NotNull(building2Ds);
            Assert.Single(building2Ds);
            Assert.Equal(reference_Courtyard_1465, building2Ds[0].Reference);

            return building2Ds[0];
        }
    }
}
