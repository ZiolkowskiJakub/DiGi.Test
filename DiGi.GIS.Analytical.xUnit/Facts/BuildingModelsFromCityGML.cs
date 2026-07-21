using DiGi.Analytical.Building.Classes;
using DiGi.CityGML.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.GIS.Classes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DiGi.GIS.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that the shared CityGML fixture parses into the expected content.
        /// <para>Guards the archive itself - an entry that is stored rather than deflated is silently skipped by the CityGML walker, which would leave every other fact in this file asserting against an empty model.</para>
        /// </summary>
        [Fact]
        public void CityGMLFile_Parses()
        {
            List<Building> buildings = CityGML_Buildings();

            Assert.Equal(3, buildings.Count);

            int count_GroundSurface = 0;
            int count_WallSurface = 0;
            int count_RoofSurface = 0;

            foreach (Building building in buildings)
            {
                Assert.NotNull(building.Surfaces);
                foreach (CityGML.Interfaces.ISurface surface in building.Surfaces)
                {
                    Assert.NotNull(surface.Geometry);

                    if (surface is GroundSurface)
                    {
                        count_GroundSurface++;
                    }
                    else if (surface is WallSurface)
                    {
                        count_WallSurface++;
                    }
                    else if (surface is RoofSurface)
                    {
                        count_RoofSurface++;
                    }
                }
            }

            // Every building clears the four-face threshold required to form a polyhedron.
            Assert.Equal(3, count_GroundSurface);
            Assert.Equal(12, count_WallSurface);
            Assert.Equal(5, count_RoofSurface);
        }

        /// <summary>
        /// Tests that the building reference resolves identically from the buildingId attribute and from the gml:id carried as UniqueId.
        /// <para>The fixture stores gml:id as "ID-&lt;county&gt;-&lt;buildingId&gt;", so the prefix-stripping branch of the reference query must reproduce the attribute value exactly.</para>
        /// </summary>
        [Fact]
        public void Reference_FromCityGMLFile()
        {
            List<Building> buildings = CityGML_Buildings();

            foreach (Building building in buildings)
            {
                string? reference = CityGML.Query.Reference(building);
                Assert.False(string.IsNullOrWhiteSpace(reference));

                string? buildingId = building.GetValue<string>(CityGML.Enums.BuildingParameter.buildingId);
                Assert.Equal(buildingId, reference);

                string? uniqueId = building.UniqueId;
                Assert.NotNull(uniqueId);
                Assert.StartsWith("ID-", uniqueId);

                string[] values = uniqueId.Split('-');
                string uniqueId_Stripped = string.Join("-", values, 2, values.Length - 2);
                Assert.Equal(uniqueId_Stripped, reference);
            }
        }

        /// <summary>
        /// Tests that a BuildingModel is created from a real multi-surface CityGML building.
        /// <para>Every building in the fixture carries more than four boundary surfaces, so a polyhedron is formed and the space receives an internal point - the case a single-surface synthetic building cannot reach.</para>
        /// <para>The fixture is LOD2 data whose boundary surfaces are typed, so each one is expected to convert directly into a component.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_FromCityGMLFile()
        {
            List<Building> buildings = CityGML_Buildings();

            foreach (Building building in buildings)
            {
                BuildingModel? buildingModel = Create.BuildingModel(building);

                Assert.NotNull(buildingModel);

                List<Space>? spaces = buildingModel.GetSpaces<Space>();
                Assert.NotNull(spaces);
                Assert.Single(spaces);

                // The space is named after the CityGML building, which is what separates this path from the extruded fallback.
                Assert.Equal(building.UniqueId, spaces[0].Name);

                Point3D? point3D = spaces[0].Geometry;
                Assert.NotNull(point3D);
                Assert.False(double.IsNaN(point3D.X) || double.IsNaN(point3D.Y) || double.IsNaN(point3D.Z));

                Assert.True(buildingModel.TryGetValue(Enums.BuildingModelParameter.LOD, out CityGML.Enums.LOD lOD));
                Assert.Equal(CityGML.Enums.LOD.LOD2, lOD);

                List<FaceFloor>? faceFloors = buildingModel.GetComponents<FaceFloor>();
                Assert.NotNull(faceFloors);
                Assert.NotEmpty(faceFloors);

                List<SurfaceWall>? surfaceWalls = buildingModel.GetComponents<SurfaceWall>();
                Assert.NotNull(surfaceWalls);
                Assert.NotEmpty(surfaceWalls);

                List<SurfaceRoof>? surfaceRoofs = buildingModel.GetComponents<SurfaceRoof>();
                Assert.NotNull(surfaceRoofs);
                Assert.NotEmpty(surfaceRoofs);
            }
        }

        /// <summary>
        /// Tests that the reference join resolves real CityGML buildings for 2D buildings carrying the matching references.
        /// <para>Each 2D building is given the footprint of its CityGML counterpart, so a failed join would still produce a model through the extruded fallback; the space name is what proves the join was used.</para>
        /// </summary>
        [Fact]
        public void BuildingModels_ByReference_FromCityGMLFile()
        {
            List<Building> buildings = CityGML_Buildings();

            List<Building2D> building2Ds = [];
            Dictionary<string, string> dictionary = [];

            foreach (Building building in buildings)
            {
                string? reference = CityGML.Query.Reference(building);
                Assert.False(string.IsNullOrWhiteSpace(reference));

                PolygonalFace2D? polygonalFace2D = Footprint(building);
                Assert.NotNull(polygonalFace2D);

                building2Ds.Add(new Building2D(Guid.NewGuid(), reference, polygonalFace2D, 1, null, null, []));

                Assert.NotNull(building.UniqueId);
                dictionary[reference!] = building.UniqueId;
            }

            Assert.Equal(3, building2Ds.Count);

            List<BuildingModel>? buildingModels = Create.BuildingModels(building2Ds, buildings);

            Assert.NotNull(buildingModels);
            Assert.Equal(building2Ds.Count, buildingModels.Count);

            foreach (BuildingModel buildingModel in buildingModels)
            {
                Assert.True(buildingModel.TryGetValue(Enums.BuildingModelParameter.Reference, out string? reference));
                Assert.False(string.IsNullOrWhiteSpace(reference));
                Assert.True(dictionary.ContainsKey(reference!));

                List<Space>? spaces = buildingModel.GetSpaces<Space>();
                Assert.NotNull(spaces);
                Assert.NotEmpty(spaces);

                // The extruded fallback would name the space "Building"; the CityGML UniqueId proves the reference join resolved it.
                Assert.Equal(dictionary[reference!], spaces[0].Name);
            }
        }

        private static List<Building> CityGML_Buildings()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "2862_CityGML.zip");

            Assert.False(string.IsNullOrWhiteSpace(path));
            Assert.True(System.IO.File.Exists(path));

            List<CityModel>? cityModels = CityGML.Create.CityModels(path);

            Assert.NotNull(cityModels);
            Assert.Single(cityModels);

            List<Building> result = [];
            foreach (CityModel cityModel in cityModels)
            {
                IEnumerable<Building>? buildings = cityModel?.Buildings;
                if (buildings is null)
                {
                    continue;
                }

                result.AddRange(buildings);
            }

            return result;
        }

        private static PolygonalFace2D? Footprint(Building? building)
        {
            IEnumerable<CityGML.Interfaces.ISurface>? surfaces = building?.Surfaces;
            if (surfaces is null)
            {
                return null;
            }

            Plane plane = Geometry.Spatial.Constants.Plane.WorldZ;

            foreach (CityGML.Interfaces.ISurface surface in surfaces)
            {
                if (surface is not GroundSurface)
                {
                    continue;
                }

                PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Query.Project<PolygonalFace3D>(plane, surface.Geometry);
                if (polygonalFace3D is null)
                {
                    continue;
                }

                if (plane.Convert(polygonalFace3D) is PolygonalFace2D polygonalFace2D)
                {
                    return polygonalFace2D;
                }
            }

            return null;
        }
    }
}
