using DiGi.Analytical.Building.Classes;
using DiGi.CityGML.Classes;
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
        /// Reference of a building of the "2476_CityGML.zip" fixture whose boundary surfaces are not planar within <see cref="Core.Constants.Tolerance.Distance"/>.
        /// </summary>
        private const string reference_NonPlanar_1 = "2A522112-FC32-62E0-E053-CA2BA8C0E403";

        /// <summary>
        /// Reference of a second building of the "2476_CityGML.zip" fixture whose boundary surfaces are not planar within <see cref="Core.Constants.Tolerance.Distance"/>.
        /// </summary>
        private const string reference_NonPlanar_2 = "2A522113-0066-62E0-E053-CA2BA8C0E403";

        /// <summary>
        /// Reference of a third building of the "2476_CityGML.zip" fixture whose boundary surfaces are not planar within <see cref="Core.Constants.Tolerance.Distance"/>.
        /// </summary>
        private const string reference_NonPlanar_3 = "2A522112-F975-62E0-E053-CA2BA8C0E403";

        /// <summary>
        /// Reference of a three storey residential building of the "2476_CityGML.zip" fixture, cut into three storeys at the coordinate tolerance but not at the distance tolerance.
        /// </summary>
        private const string reference_Residential_1 = "2A522112-F816-62E0-E053-CA2BA8C0E403";

        /// <summary>
        /// Reference of a second three storey residential building of the "2476_CityGML.zip" fixture, cut into three storeys at the coordinate tolerance but not at the distance tolerance.
        /// </summary>
        private const string reference_Residential_2 = "2A522112-F8DE-62E0-E053-CA2BA8C0E403";

        /// <summary>
        /// Reference of a two storey non residential building of the "2476_CityGML.zip" fixture, cut into two storeys at either tolerance.
        /// </summary>
        private const string reference_NonResidential = "2A522112-F96B-62E0-E053-CA2BA8C0E403";

        /// <summary>
        /// Tests that the "2476_CityGML.zip" fixture holds the six real LOD2 buildings the facts of this file are built on.
        /// <para>The buildings were taken from the county 2476 of the national 3D building model and are paired with the 2D buildings of the "2476_GML.gmf" fixture of the same county by their reference.</para>
        /// </summary>
        [Fact]
        public void CityGMLFile_2476_Parses()
        {
            Dictionary<string, Building> buildings = CityGML_Buildings_2476();

            Assert.Equal(6, buildings.Count);

            string[] references = [reference_NonPlanar_1, reference_NonPlanar_2, reference_NonPlanar_3, reference_Residential_1, reference_Residential_2, reference_NonResidential];
            for (int i = 0; i < references.Length; i++)
            {
                Assert.True(buildings.ContainsKey(references[i]), $"Building {references[i]} is missing from the fixture.");
            }

            Dictionary<string, Building2D> building2Ds = Building2Ds_2476(references);
            Assert.Equal(references.Length, building2Ds.Count);
        }

        /// <summary>
        /// Tests the creation of a building model for real LOD2 buildings whose boundary surfaces are not planar within the distance tolerance.
        /// <para>Survey geometry is never planar to a micrometre, so the normal of such a surface used to come back as a zero vector, which turned the plane of the surface and every point placed on it into NaN and threw inside NetTopologySuite. This fact guards the conversion of the three buildings of the fixture which used to be lost this way - about one percent of the buildings of a county - together with the geometry they produce.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_FromCityGMLFile_2476_NonPlanarGeometry()
        {
            Dictionary<string, Building> buildings = CityGML_Buildings_2476();

            string[] references = [reference_NonPlanar_1, reference_NonPlanar_2, reference_NonPlanar_3];

            Dictionary<string, Building2D> building2Ds = Building2Ds_2476(references);

            for (int i = 0; i < references.Length; i++)
            {
                Building building = buildings[references[i]];
                Building2D building2D = building2Ds[references[i]];

                BuildingModel? buildingModel = Create.BuildingModel(building, building2D);
                Assert.NotNull(buildingModel);

                BoundingBox3D? boundingBox3D = buildingModel.GetBoundingBox();
                Assert.NotNull(boundingBox3D);
                Assert.False(double.IsNaN(boundingBox3D.MinZ) || double.IsNaN(boundingBox3D.MaxZ), $"Building {references[i]} has a NaN bounding box.");
                Assert.True(boundingBox3D.Height > 0);

                List<Space>? spaces = buildingModel.GetSpaces<Space>();
                Assert.NotNull(spaces);
                Assert.NotEmpty(spaces);

                for (int j = 0; j < spaces.Count; j++)
                {
                    Point3D? point3D = spaces[j].Geometry;
                    Assert.NotNull(point3D);
                    Assert.False(double.IsNaN(point3D.X) || double.IsNaN(point3D.Y) || double.IsNaN(point3D.Z), $"Building {references[i]} has a space without a valid internal point.");
                }

                // The single argument overload is where the geometry is reached, so it has to hold on its own as well.
                Assert.NotNull(Create.BuildingModel(building));
            }
        }

        /// <summary>
        /// Tests the storey split of real LOD2 buildings against the tolerance it is given.
        /// <para>Both buildings are three storey residential buildings of about eleven metres, so the derived storey height passes both gates and the model is cut into three storeys. The coordinates of the source data carry two decimal places, so the boundary surfaces of a building meet only within <see cref="Constants.Tolerance.Coordinate"/>, which is the default of the overload and cuts the model correctly. Passing the finer <see cref="Core.Constants.Tolerance.Distance"/> leaves the ring the cutting plane assembles open at the corners, so no cut is made and the model is returned whole - the reason the overload does not default to that value.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_FromCityGMLFile_2476_SplitTolerance()
        {
            Dictionary<string, Building> buildings = CityGML_Buildings_2476();

            string[] references = [reference_Residential_1, reference_Residential_2];

            Dictionary<string, Building2D> building2Ds = Building2Ds_2476(references);

            for (int i = 0; i < references.Length; i++)
            {
                Building building = buildings[references[i]];
                Building2D building2D = building2Ds[references[i]];

                Assert.Equal(3, building2D.Storeys);
                Assert.True(GIS.Query.IsResidential(building2D));

                BuildingModel? buildingModel = Create.BuildingModel(building, building2D);
                Assert.NotNull(buildingModel);

                BoundingBox3D? boundingBox3D = buildingModel.GetBoundingBox();
                Assert.NotNull(boundingBox3D);

                // The storey height passes both gates, so the model is expected to be cut.
                double height = Core.Query.Round(boundingBox3D.Height / building2D.Storeys, Constants.StoreyHeight.Precision, Core.Enums.RoundingMethod.Floor);
                Assert.True(height >= Constants.StoreyHeight.Min);
                Assert.True(height <= Constants.StoreyHeight.Max);

                // The default tolerance matches the coordinate precision of the source data, so the cut succeeds.
                List<Space>? spaces = buildingModel.GetSpaces<Space>();
                Assert.NotNull(spaces);
                Assert.Equal(building2D.Storeys, spaces.Count);

                // Passing the finer distance tolerance leaves the ring open at the corners, so no cut is made - the reason the overload does not default to it.
                BuildingModel? buildingModel_Fine = Create.BuildingModel(building, building2D, Core.Constants.Tolerance.Distance);
                Assert.NotNull(buildingModel_Fine);

                List<Space>? spaces_Fine = buildingModel_Fine.GetSpaces<Space>();
                Assert.NotNull(spaces_Fine);
                Assert.Single(spaces_Fine);
            }
        }

        /// <summary>
        /// Tests the storey split for a real LOD2 building the split can handle.
        /// <para>The building is a two storey non residential building of about eight and a half metres, so the derived storey height is clamped to the maximal storey height and the single cutting plane sits four metres below the top of the model, leaving the remainder to the lowest storey.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_FromCityGMLFile_2476_SplitSucceeds()
        {
            Dictionary<string, Building> buildings = CityGML_Buildings_2476();

            Building building = buildings[reference_NonResidential];
            Building2D building2D = Building2Ds_2476([reference_NonResidential])[reference_NonResidential];

            Assert.Equal(2, building2D.Storeys);
            Assert.False(GIS.Query.IsResidential(building2D));

            BuildingModel? buildingModel = Create.BuildingModel(building, building2D);
            Assert.NotNull(buildingModel);

            BoundingBox3D? boundingBox3D = buildingModel.GetBoundingBox();
            Assert.NotNull(boundingBox3D);

            List<Space>? spaces = buildingModel.GetSpaces<Space>();
            Assert.NotNull(spaces);
            Assert.Equal(2, spaces.Count);

            // The clamp keeps the storey count, so the whole remainder is left to the lowest storey.
            List<double> elevations = HorizontalElevations(buildingModel);
            elevations = elevations.FindAll(x => x - boundingBox3D.MinZ > Core.Constants.Tolerance.MacroDistance && boundingBox3D.MaxZ - x > Core.Constants.Tolerance.MacroDistance);
            Assert.Single(elevations);
            Assert.True(Math.Abs(boundingBox3D.MaxZ - elevations[0] - Constants.StoreyHeight.Max) < Core.Constants.Tolerance.MacroDistance, $"The topmost storey is {boundingBox3D.MaxZ - elevations[0]} instead of {Constants.StoreyHeight.Max}.");
            Assert.True(elevations[0] - boundingBox3D.MinZ > Constants.StoreyHeight.Max, "The lowest storey is expected to carry the remainder.");

            Core.xUnit.Query.SerializationCheck(buildingModel);
        }

        /// <summary>
        /// Tests that every real LOD2 building of the "2476_CityGML.zip" fixture converts at the default tolerance of the overload.
        /// <para>Guards the two defects found on this data together - no building throws and no model carries a NaN extent (the non planar normal defect) and every space of every model has a valid internal point. The fixture holds a building of each behaviour a county produces, so a regression in either defect fails here.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_FromCityGMLFile_2476_AllConvert()
        {
            Dictionary<string, Building> buildings = CityGML_Buildings_2476();

            string[] references = [reference_NonPlanar_1, reference_NonPlanar_2, reference_NonPlanar_3, reference_Residential_1, reference_Residential_2, reference_NonResidential];

            Dictionary<string, Building2D> building2Ds = Building2Ds_2476(references);

            for (int i = 0; i < references.Length; i++)
            {
                Building building = buildings[references[i]];
                Building2D building2D = building2Ds[references[i]];

                BuildingModel? buildingModel = Create.BuildingModel(building, building2D);
                Assert.NotNull(buildingModel);

                BoundingBox3D? boundingBox3D = buildingModel.GetBoundingBox();
                Assert.NotNull(boundingBox3D);
                Assert.False(double.IsNaN(boundingBox3D.MinZ) || double.IsNaN(boundingBox3D.MaxZ), $"Building {references[i]} has a NaN bounding box.");

                List<Space>? spaces = buildingModel.GetSpaces<Space>();
                Assert.NotNull(spaces);
                Assert.NotEmpty(spaces);

                for (int j = 0; j < spaces.Count; j++)
                {
                    Point3D? point3D = spaces[j].Geometry;
                    Assert.NotNull(point3D);
                    Assert.False(double.IsNaN(point3D.X) || double.IsNaN(point3D.Y) || double.IsNaN(point3D.Z), $"Building {references[i]} has a space without a valid internal point.");
                }

                Core.xUnit.Query.SerializationCheck(buildingModel);
            }
        }

        /// <summary>
        /// Tests that the default tolerance of the overload cuts the real LOD2 buildings of the "2476_CityGML.zip" fixture into the expected number of storeys.
        /// <para>Locks in the outcome of matching the tolerance to the coordinate precision of the source data - the two three storey residential buildings become three storeys and the two storey non residential building becomes two storeys, all at the default tolerance. The same cuts were left undone while the overload defaulted to <see cref="Core.Constants.Tolerance.Distance"/>.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_FromCityGMLFile_2476_DefaultToleranceSplits()
        {
            Dictionary<string, Building> buildings = CityGML_Buildings_2476();

            Dictionary<string, int> expected = new()
            {
                { reference_Residential_1, 3 },
                { reference_Residential_2, 3 },
                { reference_NonResidential, 2 },
            };

            Dictionary<string, Building2D> building2Ds = Building2Ds_2476(expected.Keys);

            foreach (KeyValuePair<string, int> keyValuePair in expected)
            {
                Building building = buildings[keyValuePair.Key];
                Building2D building2D = building2Ds[keyValuePair.Key];

                BuildingModel? buildingModel = Create.BuildingModel(building, building2D);
                Assert.NotNull(buildingModel);

                List<Space>? spaces = buildingModel.GetSpaces<Space>();
                Assert.NotNull(spaces);
                Assert.Equal(keyValuePair.Value, spaces.Count);
            }
        }

        /// <summary>
        /// Loads the real LOD2 buildings of the "2476_CityGML.zip" fixture indexed by their reference.
        /// </summary>
        /// <returns>A dictionary holding the buildings of the fixture keyed by their reference.</returns>
        private static Dictionary<string, Building> CityGML_Buildings_2476()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "2476_CityGML.zip");

            Assert.False(string.IsNullOrWhiteSpace(path));
            Assert.True(System.IO.File.Exists(path));

            List<CityModel>? cityModels = CityGML.Create.CityModels(path);
            Assert.NotNull(cityModels);

            Dictionary<string, Building> result = [];
            foreach (CityModel cityModel in cityModels)
            {
                IEnumerable<Building>? buildings = cityModel?.Buildings;
                if (buildings is null)
                {
                    continue;
                }

                foreach (Building building in buildings)
                {
                    if (CityGML.Query.Reference(building) is string reference)
                    {
                        result[reference] = building;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Loads the 2D buildings of the "2476_GML.gmf" fixture carrying the given references.
        /// </summary>
        /// <param name="references">The references of the 2D buildings to be taken.</param>
        /// <returns>A dictionary holding the matching 2D buildings keyed by their reference.</returns>
        private static Dictionary<string, Building2D> Building2Ds_2476(IEnumerable<string> references)
        {
            HashSet<string> references_Temp = [.. references];

            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "2476_GML.gmf");

            Assert.False(string.IsNullOrWhiteSpace(path));

            using GISModelFile gISModelFile = new(path);
            Assert.True(gISModelFile.Open());

            List<Building2D>? building2Ds = gISModelFile.Value?.GetObjects<Building2D>();
            Assert.NotNull(building2Ds);

            Dictionary<string, Building2D> result = [];
            foreach (Building2D building2D in building2Ds)
            {
                if (building2D?.Reference is string reference && references_Temp.Contains(reference))
                {
                    result[reference] = building2D;
                }
            }

            return result;
        }
    }
}
