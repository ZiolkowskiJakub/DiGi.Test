using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.CityGML.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;
using DiGi.GIS.Classes;
using DiGi.GIS.Enums;
using System;
using System.Collections.Generic;

namespace DiGi.GIS.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the null handling of the overload taking a CityGML building together with the matching 2D building.
        /// <para>Verifies that two null inputs return null and that a null 2D building falls back to the model created from the CityGML building alone, which is identified by the space named after the building.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_BuildingAndBuilding2D_NullInputs()
        {
            Building? building_Null = null;
            Building2D? building2D_Null = null;

            Assert.Null(Create.BuildingModel(building_Null, building2D_Null));

            Building building = CityGML_Building(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 9)), "Building 1");

            BuildingModel? buildingModel = Create.BuildingModel(building, building2D_Null);

            Assert.NotNull(buildingModel);

            List<Space>? spaces = buildingModel.GetSpaces<Space>();
            Assert.NotNull(spaces);
            Assert.Single(spaces);
            Assert.Equal(building.UniqueId, spaces[0].Name);
        }

        /// <summary>
        /// Tests that a null CityGML building falls back to the model extruded from the footprint of the 2D building.
        /// <para>Guards the overload resolution of the fallback - the extruded overload takes the storey height ahead of the tolerance, so passing the tolerance positionally would produce a model of a micrometre in height instead of one storey height per storey.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_BuildingAndBuilding2D_NullBuilding()
        {
            Building? building = null;

            Building2D building2D = Building2D_Rectangle(10, 10, 2, BuildingGeneralFunction.residential_buildings);

            BuildingModel? buildingModel = Create.BuildingModel(building, building2D);

            Assert.NotNull(buildingModel);

            List<Space>? spaces = buildingModel.GetSpaces<Space>();
            Assert.NotNull(spaces);
            Assert.Equal(2, spaces.Count);

            BoundingBox3D? boundingBox3D = buildingModel.GetBoundingBox();
            Assert.NotNull(boundingBox3D);
            Assert.True(Math.Abs(boundingBox3D.Height - (2 * Constants.StoreyHeight.Default)) < Core.Constants.Tolerance.Distance, $"Extruded fallback is {boundingBox3D.Height} high instead of {2 * Constants.StoreyHeight.Default}.");
        }

        /// <summary>
        /// Tests that a residential building of a plausible storey height is cut into storeys.
        /// <para>Verifies that a nine metre high building of three storeys is cut at six and three metres, that the separators created on the cutting planes are converted into floors and that the walls are split accordingly.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_BuildingAndBuilding2D_Residential()
        {
            Building building = CityGML_Building(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 9)), "Building 1");

            Building2D building2D = Building2D_Rectangle(10, 10, 3, BuildingGeneralFunction.residential_buildings);

            BuildingModel? buildingModel = Create.BuildingModel(building, building2D);

            Assert.NotNull(buildingModel);

            List<Space>? spaces = buildingModel.GetSpaces<Space>();
            Assert.NotNull(spaces);
            Assert.Equal(3, spaces.Count);

            // The separators created on the cutting planes carry no construction, so they are created as airs and converted into floors afterwards.
            List<IAir>? airs = buildingModel.GetComponents<IAir>();
            Assert.True(airs is null || airs.Count == 0);

            List<double> elevations = HorizontalElevations(buildingModel);
            Assert.Contains(elevations, x => Math.Abs(x - 6) < Core.Constants.Tolerance.Distance);
            Assert.Contains(elevations, x => Math.Abs(x - 3) < Core.Constants.Tolerance.Distance);

            List<IWall>? walls = buildingModel.GetComponents<IWall>();
            Assert.NotNull(walls);
            Assert.Equal(12, walls.Count);
        }

        /// <summary>
        /// Tests that a building of a single storey is returned unsplit.
        /// </summary>
        [Fact]
        public void BuildingModel_BuildingAndBuilding2D_SingleStorey()
        {
            Building building = CityGML_Building(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 9)), "Building 1");

            Building2D building2D = Building2D_Rectangle(10, 10, 1, BuildingGeneralFunction.residential_buildings);

            BuildingModel? buildingModel = Create.BuildingModel(building, building2D);

            Assert.NotNull(buildingModel);

            List<Space>? spaces = buildingModel.GetSpaces<Space>();
            Assert.NotNull(spaces);
            Assert.Single(spaces);
        }

        /// <summary>
        /// Tests that the storey count of a residential building whose derived storey height exceeds the maximal storey height is recalculated from the extents of the model.
        /// <para>A fifteen metre high building of three storeys gives five metres per storey, which cannot be reconciled with a residential building, so the storey count is replaced by the five storeys the model holds at the default storey height and the cutting planes follow at every three metres.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_BuildingAndBuilding2D_ResidentialAboveMax()
        {
            Building building = CityGML_Building(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 15)), "Building 1");

            Building2D building2D = Building2D_Rectangle(10, 10, 3, BuildingGeneralFunction.residential_buildings);

            BuildingModel? buildingModel = Create.BuildingModel(building, building2D);

            Assert.NotNull(buildingModel);

            // The recalculated storey count wins over the three storeys carried by the 2D building.
            List<Space>? spaces = buildingModel.GetSpaces<Space>();
            Assert.NotNull(spaces);
            Assert.Equal(5, spaces.Count);

            List<double> elevations = HorizontalElevations(buildingModel);
            Assert.Contains(elevations, x => Math.Abs(x - 12) < Core.Constants.Tolerance.Distance);
            Assert.Contains(elevations, x => Math.Abs(x - 9) < Core.Constants.Tolerance.Distance);
            Assert.Contains(elevations, x => Math.Abs(x - 6) < Core.Constants.Tolerance.Distance);
            Assert.Contains(elevations, x => Math.Abs(x - 3) < Core.Constants.Tolerance.Distance);
        }

        /// <summary>
        /// Tests that a residential building keeps its storey count when the recalculation cannot improve it.
        /// <para>An eight and a half metre high building of two storeys gives four and a quarter metres per storey, and the recalculation at the default storey height gives the very same two storeys, so the storey height stays above the maximal storey height and the model is returned unsplit.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_BuildingAndBuilding2D_ResidentialAboveMax_NotRecoverable()
        {
            Building building = CityGML_Building(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 8.5)), "Building 1");

            Building2D building2D = Building2D_Rectangle(10, 10, 2, BuildingGeneralFunction.residential_buildings);

            BuildingModel? buildingModel = Create.BuildingModel(building, building2D);

            Assert.NotNull(buildingModel);

            List<Space>? spaces = buildingModel.GetSpaces<Space>();
            Assert.NotNull(spaces);
            Assert.Single(spaces);
        }

        /// <summary>
        /// Tests the recalculation of the storey count of a residential building against the rounding of the storey height.
        /// <para>A twenty metre high building of four storeys gives five metres per storey, so the storey count is recalculated to the six storeys the model holds at the default storey height, which gives a storey height of three metres and thirty centimetres after rounding down. The cutting planes are measured downwards from the top of the model, so the rounding remainder is left to the lowest storey, which ends up half a metre higher than the others.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_BuildingAndBuilding2D_ResidentialAboveMax_Rounding()
        {
            Building building = CityGML_Building(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 20)), "Building 1");

            Building2D building2D = Building2D_Rectangle(10, 10, 4, BuildingGeneralFunction.residential_buildings);

            BuildingModel? buildingModel = Create.BuildingModel(building, building2D);

            Assert.NotNull(buildingModel);

            List<Space>? spaces = buildingModel.GetSpaces<Space>();
            Assert.NotNull(spaces);
            Assert.Equal(6, spaces.Count);

            List<double> elevations = HorizontalElevations(buildingModel);

            double[] elevations_Expected = [16.7, 13.4, 10.1, 6.8, 3.5];
            for (int i = 0; i < elevations_Expected.Length; i++)
            {
                double elevation = elevations_Expected[i];
                Assert.Contains(elevations, x => Math.Abs(x - elevation) < Core.Constants.Tolerance.MacroDistance);
            }
        }

        /// <summary>
        /// Tests that the storey count of a residential building is only recalculated once the rounded storey height passes the maximal storey height.
        /// <para>The storey height is rounded down to <see cref="Constants.StoreyHeight.Precision"/>, so a twelve metre high building of three storeys still gives exactly the maximal storey height and keeps its storey count, whereas the same building of two storeys is recalculated to four storeys.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_BuildingAndBuilding2D_ResidentialAtMax()
        {
            Building building = CityGML_Building(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 12)), "Building 1");

            BuildingModel? buildingModel_3 = Create.BuildingModel(building, Building2D_Rectangle(10, 10, 3, BuildingGeneralFunction.residential_buildings));
            Assert.NotNull(buildingModel_3);

            List<Space>? spaces_3 = buildingModel_3.GetSpaces<Space>();
            Assert.NotNull(spaces_3);
            Assert.Equal(3, spaces_3.Count);

            List<double> elevations_3 = HorizontalElevations(buildingModel_3);
            Assert.Contains(elevations_3, x => Math.Abs(x - 8) < Core.Constants.Tolerance.Distance);
            Assert.Contains(elevations_3, x => Math.Abs(x - 4) < Core.Constants.Tolerance.Distance);

            BuildingModel? buildingModel_2 = Create.BuildingModel(building, Building2D_Rectangle(10, 10, 2, BuildingGeneralFunction.residential_buildings));
            Assert.NotNull(buildingModel_2);

            List<Space>? spaces_2 = buildingModel_2.GetSpaces<Space>();
            Assert.NotNull(spaces_2);
            Assert.Equal(4, spaces_2.Count);
        }

        /// <summary>
        /// Tests that a residential building is classified by its specific function as well.
        /// <para>A building carrying a non residential general function together with a multi family specific function takes the recalculation branch instead of the clamp taken by a non residential building of the same extents.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_BuildingAndBuilding2D_ResidentialBySpecificFunction()
        {
            Building building = CityGML_Building(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 15)), "Building 1");

            Building2D building2D = Building2D_Rectangle(10, 10, 3, BuildingGeneralFunction.other_non_residential_buildings, BuildingSpecificFunction.multi_family_building);

            Assert.True(GIS.Query.IsResidential(building2D));

            BuildingModel? buildingModel = Create.BuildingModel(building, building2D);

            Assert.NotNull(buildingModel);

            List<Space>? spaces = buildingModel.GetSpaces<Space>();
            Assert.NotNull(spaces);
            Assert.Equal(5, spaces.Count);
        }

        /// <summary>
        /// Tests the storey height at the boundaries of the minimal storey height.
        /// <para>The storey height is rounded down to <see cref="Constants.StoreyHeight.Precision"/>, so a building giving exactly the minimal storey height is cut, whereas a building ten centimetres lower is left unsplit.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_BuildingAndBuilding2D_MinBoundaries()
        {
            Building building_Inside = CityGML_Building(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 7.2)), "Building 1");

            BuildingModel? buildingModel_Inside = Create.BuildingModel(building_Inside, Building2D_Rectangle(10, 10, 3, BuildingGeneralFunction.residential_buildings));
            Assert.NotNull(buildingModel_Inside);

            List<Space>? spaces_Inside = buildingModel_Inside.GetSpaces<Space>();
            Assert.NotNull(spaces_Inside);
            Assert.Equal(3, spaces_Inside.Count);

            Building building_Outside = CityGML_Building(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 7.1)), "Building 2");

            BuildingModel? buildingModel_Outside = Create.BuildingModel(building_Outside, Building2D_Rectangle(10, 10, 3, BuildingGeneralFunction.residential_buildings));
            Assert.NotNull(buildingModel_Outside);

            List<Space>? spaces_Outside = buildingModel_Outside.GetSpaces<Space>();
            Assert.NotNull(spaces_Outside);
            Assert.Single(spaces_Outside);
        }

        /// <summary>
        /// Tests that the derived storey height of a non residential building is clamped to the maximal storey height.
        /// <para>A fifteen metre high building of three storeys gives five metres per storey, which is clamped to four metres, so the cutting planes are measured downwards from the top of the model at eleven and seven metres.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_BuildingAndBuilding2D_NonResidentialClamped()
        {
            Building building = CityGML_Building(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 15)), "Building 1");

            Building2D building2D = Building2D_Rectangle(10, 10, 3, BuildingGeneralFunction.industrial_buildings);

            BuildingModel? buildingModel = Create.BuildingModel(building, building2D);

            Assert.NotNull(buildingModel);

            List<Space>? spaces = buildingModel.GetSpaces<Space>();
            Assert.NotNull(spaces);
            Assert.Equal(3, spaces.Count);

            List<double> elevations = HorizontalElevations(buildingModel);
            Assert.Contains(elevations, x => Math.Abs(x - 11) < Core.Constants.Tolerance.Distance);
            Assert.Contains(elevations, x => Math.Abs(x - 7) < Core.Constants.Tolerance.Distance);
        }

        /// <summary>
        /// Tests that a building whose derived storey height is below the minimal storey height is returned unsplit.
        /// <para>A six metre high building of three storeys gives two metres per storey, which is below the minimal plausible storey height.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_BuildingAndBuilding2D_ShortStorey()
        {
            Building building = CityGML_Building(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 6)), "Building 1");

            Building2D building2D = Building2D_Rectangle(10, 10, 3, BuildingGeneralFunction.residential_buildings);

            BuildingModel? buildingModel = Create.BuildingModel(building, building2D);

            Assert.NotNull(buildingModel);

            List<Space>? spaces = buildingModel.GetSpaces<Space>();
            Assert.NotNull(spaces);
            Assert.Single(spaces);
        }

        /// <summary>
        /// Tests that the storey count of a non residential building is kept and the whole remainder is left to the lowest storey.
        /// <para>A thirteen metre high building of two storeys gives six and a half metres per storey, which is clamped to the maximal storey height, so the single cutting plane sits four metres below the top of the model and the lowest storey ends up nine metres high. The very same building read as residential is recalculated into four storeys instead, which is the asymmetry between the two branches.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_BuildingAndBuilding2D_NonResidentialClamped_Remainder()
        {
            Building building = CityGML_Building(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 13)), "Building 1");

            BuildingModel? buildingModel = Create.BuildingModel(building, Building2D_Rectangle(10, 10, 2, BuildingGeneralFunction.industrial_buildings));
            Assert.NotNull(buildingModel);

            List<Space>? spaces = buildingModel.GetSpaces<Space>();
            Assert.NotNull(spaces);
            Assert.Equal(2, spaces.Count);

            List<double> elevations = HorizontalElevations(buildingModel);
            Assert.Contains(elevations, x => Math.Abs(x - 9) < Core.Constants.Tolerance.Distance);

            // The same extents read as residential give four storeys of three metres and twenty centimetres.
            BuildingModel? buildingModel_Residential = Create.BuildingModel(building, Building2D_Rectangle(10, 10, 2, BuildingGeneralFunction.residential_buildings));
            Assert.NotNull(buildingModel_Residential);

            List<Space>? spaces_Residential = buildingModel_Residential.GetSpaces<Space>();
            Assert.NotNull(spaces_Residential);
            Assert.Equal(4, spaces_Residential.Count);
        }

        /// <summary>
        /// Tests the storey split against the real CityGML buildings of the "2862_CityGML.zip" fixture.
        /// <para>Every building of the fixture is paired with a plausible storey count and function, and the resulting model is checked against the invariants of the split: every cutting plane lies strictly inside the extents of the model, the number of spaces matches the number of cutting planes and no storey created by a cut is lower than <see cref="Constants.StoreyHeight.Min"/>.</para>
        /// <para>The buildings of the fixture are between three and a half and six metres high, so the branch recalculating the storey count of a residential building - which needs a derived storey height above <see cref="Constants.StoreyHeight.Max"/> - is not reached by this data.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_BuildingAndBuilding2D_FromCityGMLFile()
        {
            List<Building> buildings = CityGML_Buildings();
            Assert.NotEmpty(buildings);

            ushort[] storeyCounts = [1, 2, 3, 4];
            BuildingGeneralFunction[] buildingGeneralFunctions = [BuildingGeneralFunction.residential_buildings, BuildingGeneralFunction.industrial_buildings];

            foreach (Building building in buildings)
            {
                PolygonalFace2D? polygonalFace2D = Footprint(building);
                Assert.NotNull(polygonalFace2D);

                for (int i = 0; i < storeyCounts.Length; i++)
                {
                    for (int j = 0; j < buildingGeneralFunctions.Length; j++)
                    {
                        Building2D building2D = new(Guid.NewGuid(), Guid.NewGuid().ToString(), polygonalFace2D, storeyCounts[i], null, buildingGeneralFunctions[j], []);

                        BuildingModel? buildingModel = Create.BuildingModel(building, building2D);
                        Assert.NotNull(buildingModel);

                        BoundingBox3D? boundingBox3D = buildingModel.GetBoundingBox();
                        Assert.NotNull(boundingBox3D);

                        List<Space>? spaces = buildingModel.GetSpaces<Space>();
                        Assert.NotNull(spaces);
                        Assert.NotEmpty(spaces);

                        List<double> elevations = HorizontalElevations(buildingModel);
                        elevations = elevations.FindAll(x => x - boundingBox3D.MinZ > Core.Constants.Tolerance.MacroDistance && boundingBox3D.MaxZ - x > Core.Constants.Tolerance.MacroDistance);
                        elevations.Sort();

                        string message = $"Building {building.UniqueId} of {storeyCounts[i]} storeys and function {buildingGeneralFunctions[j]}.";

                        // One space per cutting plane plus the storey below the lowest one.
                        Assert.True(spaces.Count == elevations.Count + 1, $"{message} {spaces.Count} spaces against {elevations.Count} cutting planes.");

                        // The topmost storey and every storey between two cutting planes clears the minimal storey height.
                        for (int k = 0; k < elevations.Count; k++)
                        {
                            double height = (k == elevations.Count - 1 ? boundingBox3D.MaxZ : elevations[k + 1]) - elevations[k];
                            Assert.True(height >= Constants.StoreyHeight.Min - Core.Constants.Tolerance.MacroDistance, $"{message} Storey of {height} is lower than {Constants.StoreyHeight.Min}.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tests the parameters of a model created from a CityGML building together with the matching 2D building.
        /// <para>Verifies that the reference of the 2D building is carried over, that the level of detail of the CityGML geometry survives the split and that the model serializes.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_BuildingAndBuilding2D_Reference()
        {
            Building building = CityGML_Building(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 9)), "Building 1");

            Building2D building2D = Building2D_Rectangle(10, 10, 3, BuildingGeneralFunction.residential_buildings);

            BuildingModel? buildingModel = Create.BuildingModel(building, building2D);

            Assert.NotNull(buildingModel);

            Assert.True(buildingModel.TryGetValue(Enums.BuildingModelParameter.Reference, out string? reference));
            Assert.Equal(building2D.Reference, reference);

            Assert.True(buildingModel.TryGetValue(Enums.BuildingModelParameter.LOD, out CityGML.Enums.LOD lOD));
            Assert.Equal(CityGML.Enums.LOD.LOD2, lOD);

            Core.xUnit.Query.SerializationCheck(buildingModel);
        }

        /// <summary>
        /// Creates a CityGML building shaped as a box, the bottom face becoming a ground surface, the top face a roof surface and the vertical faces wall surfaces.
        /// </summary>
        /// <param name="boundingBox3D">The extents of the box.</param>
        /// <param name="uniqueId">The unique identifier of the created building.</param>
        /// <returns>A <see cref="Building"/> carrying the typed boundary surfaces of the box.</returns>
        private static Building CityGML_Building(BoundingBox3D boundingBox3D, string uniqueId)
        {
            Polyhedron? polyhedron = Geometry.Spatial.Create.Polyhedron(boundingBox3D);
            Assert.NotNull(polyhedron);

            List<IPolygonalFace3D>? polygonalFace3Ds = polyhedron.PolygonalFaces;
            Assert.NotNull(polygonalFace3Ds);

            List<CityGML.Interfaces.ISurface> surfaces = [];

            for (int i = 0; i < polygonalFace3Ds.Count; i++)
            {
                IPolygonalFace3D polygonalFace3D = polygonalFace3Ds[i];

                BoundingBox3D? boundingBox3D_Face = polygonalFace3D.GetBoundingBox();
                Assert.NotNull(boundingBox3D_Face);

                string uniqueId_Surface = $"{uniqueId} Surface {i + 1}";

                if (boundingBox3D_Face.MaxZ - boundingBox3D_Face.MinZ >= Core.Constants.Tolerance.Distance)
                {
                    surfaces.Add(new WallSurface(uniqueId_Surface, polygonalFace3D));
                }
                else if (boundingBox3D_Face.MinZ - boundingBox3D.MinZ < Core.Constants.Tolerance.Distance)
                {
                    surfaces.Add(new GroundSurface(uniqueId_Surface, polygonalFace3D));
                }
                else
                {
                    surfaces.Add(new RoofSurface(uniqueId_Surface, polygonalFace3D));
                }
            }

            return new Building(uniqueId, -1, surfaces);
        }

        /// <summary>
        /// Creates a 2D building whose footprint is a rectangle anchored at the origin.
        /// </summary>
        /// <param name="width">The size of the footprint along the X axis.</param>
        /// <param name="depth">The size of the footprint along the Y axis.</param>
        /// <param name="storeys">The number of storeys carried by the created building.</param>
        /// <param name="buildingGeneralFunction">The general function of the created building.</param>
        /// <param name="buildingSpecificFunctions">The specific functions of the created building.</param>
        /// <returns>A <see cref="Building2D"/> carrying the rectangular footprint.</returns>
        private static Building2D Building2D_Rectangle(double width, double depth, ushort storeys, BuildingGeneralFunction buildingGeneralFunction, params BuildingSpecificFunction[] buildingSpecificFunctions)
        {
            PolygonalFace2D? polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(width, 0), new Point2D(width, depth), new Point2D(0, depth));
            Assert.NotNull(polygonalFace2D);

            return new Building2D(Guid.NewGuid(), Guid.NewGuid().ToString(), polygonalFace2D, storeys, null, buildingGeneralFunction, buildingSpecificFunctions);
        }

        /// <summary>
        /// Collects the elevations of the horizontal components of the given building model.
        /// <para>The separators created on the cutting planes have an upwards facing normal, so they are classified as roofs rather than floors - the query is kept on the component level to stay independent of that classification.</para>
        /// </summary>
        /// <param name="buildingModel">The building model to be queried.</param>
        /// <returns>A list holding the elevation of every horizontal component.</returns>
        private static List<double> HorizontalElevations(BuildingModel buildingModel)
        {
            List<double> result = [];

            List<IComponent>? components = buildingModel.GetComponents<IComponent>();
            if (components is null)
            {
                return result;
            }

            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].GetBoundingBox() is BoundingBox3D boundingBox3D && Math.Abs(boundingBox3D.MaxZ - boundingBox3D.MinZ) < Core.Constants.Tolerance.Distance)
                {
                    result.Add(boundingBox3D.MinZ);
                }
            }

            return result;
        }
    }
}
