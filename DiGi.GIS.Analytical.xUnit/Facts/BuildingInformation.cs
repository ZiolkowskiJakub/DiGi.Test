using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Solar;
using DiGi.CityGML.Classes;
using DiGi.Core.Classes;
using DiGi.Core.Enums;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.GIS.Classes;
using DiGi.Solar.Classes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DiGi.GIS.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Latitude [deg] of the WGS 84 position of the bounding-box centre of the 55417 fixture building in Warsaw, as returned by the EPSG:2180 to WGS 84 conversion.
        /// </summary>
        private const double latitude_55417 = 52.25432670046006;

        /// <summary>
        /// Longitude [deg] of the WGS 84 position of the bounding-box centre of the 55417 fixture building in Warsaw, as returned by the EPSG:2180 to WGS 84 conversion.
        /// </summary>
        private const double longitude_55417 = 20.91081434062408;

        /// <summary>
        /// Tests that UpdateBuildingInformation places a model built from the 55417 fixture at its Warsaw location with the Polish standard time zone, and never leaves the unlocated (0, 0) default.
        /// </summary>
        [Fact]
        public void UpdateBuildingInformation_SetsPolishCoordinates()
        {
            BuildingModel? buildingModel = Create.BuildingModel(Building2D_55417());
            Assert.NotNull(buildingModel);

            bool stamped = Modify.UpdateBuildingInformation(buildingModel);
            Assert.True(stamped);

            Coordinates? coordinates = buildingModel.BuildingInformation.Coordinates;
            Assert.NotNull(coordinates);
            Assert.True(System.Math.Abs(coordinates.Latitude - latitude_55417) < 0.01, $"Latitude is {coordinates.Latitude} instead of {latitude_55417}.");
            Assert.True(System.Math.Abs(coordinates.Longitude - longitude_55417) < 0.01, $"Longitude is {coordinates.Longitude} instead of {longitude_55417}.");
            Assert.False((coordinates.Latitude == 0) && (coordinates.Longitude == 0));
            Assert.Equal(UTC.Plus0100, buildingModel.BuildingInformation.UTC);
        }

        /// <summary>
        /// Tests that a building model created from a 2D building carries located BuildingInformation, so every consumer of the created model - not only the solar endpoint - gets the coordinates and the UTC offset.
        /// </summary>
        [Fact]
        public void Create_BuildingModel_FromBuilding2D_HasCoordinates()
        {
            BuildingModel? buildingModel = Create.BuildingModel(Building2D_55417());
            Assert.NotNull(buildingModel);

            Coordinates? coordinates = buildingModel.BuildingInformation.Coordinates;
            Assert.NotNull(coordinates);
            Assert.True(System.Math.Abs(coordinates.Latitude - latitude_55417) < 0.01, $"Latitude is {coordinates.Latitude} instead of {latitude_55417}.");
            Assert.True(System.Math.Abs(coordinates.Longitude - longitude_55417) < 0.01, $"Longitude is {coordinates.Longitude} instead of {longitude_55417}.");
            Assert.Equal(UTC.Plus0100, buildingModel.BuildingInformation.UTC);
        }

        /// <summary>
        /// Tests that ToSolar carries the stamped coordinates and UTC offset into the shading model, and that the stamped model solves while the unlocated default throws.
        /// <para>The unlocated model still holds UTC.Undefined, whose TimeOffset is NaN - Convert.ToInt32 rejects it with an OverflowException. At the solstice noon of a Warsaw building the sun sits at about sixty one degrees elevation, so the direction vector points down and its negative Z component is about 0.87.</para>
        /// </summary>
        [Fact]
        public void ToSolar_CarriesCoordinates()
        {
            BuildingModel? buildingModel = Create.BuildingModel(Building2D_55417());
            Assert.NotNull(buildingModel);

            ShadingModel? shadingModel = buildingModel.ToSolar();
            Assert.NotNull(shadingModel);

            Assert.Equal(UTC.Plus0100, shadingModel.UTC);
            Coordinates? coordinates = shadingModel.Coordinates;
            Assert.NotNull(coordinates);
            Assert.True(System.Math.Abs(coordinates.Latitude - latitude_55417) < 0.01, $"Latitude is {coordinates.Latitude} instead of {latitude_55417}.");

            DateTime dateTime = new(2026, 6, 21, 12, 0, 0);
            Vector3D? vector3D = Solar.Query.SunDirection(shadingModel, dateTime);
            Assert.NotNull(vector3D);
            Assert.True(-vector3D.Z > 0.85, $"The negative Z component is {-vector3D.Z} below 0.85.");

            ShadingModel? shadingModel_Unstamped = new BuildingModel().ToSolar();
            Assert.NotNull(shadingModel_Unstamped);
            Assert.Throws<OverflowException>(() => { Solar.Query.SunDirection(shadingModel_Unstamped, dateTime); });
        }

        /// <summary>
        /// Tests that UpdateBuildingInformation refuses geometry that is not in EPSG:2180 over Poland and leaves the model with the unlocated defaults.
        /// <para>A model extruded from a square at the origin has its bounding-box centre near (0, 0), which the EPSG:2180 conversion places in Austria - far outside the Polish range.</para>
        /// </summary>
        [Fact]
        public void UpdateBuildingInformation_RejectsNonProjectedGeometry()
        {
            Plane plane = Geometry.Spatial.Create.Plane(0)!;
            PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(
                plane,
                [
                    new Point2D(0, 0),
                    new Point2D(10, 0),
                    new Point2D(10, 10),
                    new Point2D(0, 10)
                ]);
            Assert.NotNull(polygonalFace3D);

            BuildingModel? buildingModel = Create.BuildingModel(polygonalFace3D, 1, 3.0);
            Assert.NotNull(buildingModel);

            bool stamped = Modify.UpdateBuildingInformation(buildingModel);
            Assert.False(stamped);

            Coordinates? coordinates = buildingModel.BuildingInformation.Coordinates;
            Assert.NotNull(coordinates);
            Assert.Equal(0, coordinates.Latitude);
            Assert.Equal(0, coordinates.Longitude);
            Assert.Equal(UTC.Undefined, buildingModel.BuildingInformation.UTC);
        }

        /// <summary>
        /// Tests that UpdateBuildingInformation preserves the address of the BuildingInformation it rewrites.
        /// </summary>
        [Fact]
        public void UpdateBuildingInformation_KeepsAddress()
        {
            BuildingModel? buildingModel = Create.BuildingModel(Building2D_55417());
            Assert.NotNull(buildingModel);

            Address address = new("ul. Testowa 1", "Warszawa", "00-001", CountryCode.PL);
            buildingModel.BuildingInformation.Address = address;

            bool stamped = Modify.UpdateBuildingInformation(buildingModel);
            Assert.True(stamped);

            Address? address_Result = buildingModel.BuildingInformation.Address;
            Assert.NotNull(address_Result);
            Assert.Equal("ul. Testowa 1", address_Result.Street);
            Assert.Equal("Warszawa", address_Result.City);
        }

        /// <summary>
        /// Tests that the footprint extrusion fallback of the batch creation stamps the WGS 84 coordinates and the Polish standard time zone, so the models of the directory import path are located rather than computing their sun path at (0, 0).
        /// <para>The sliver fixture of the <c>BuildingModelSliver</c> facts reaches the fallback: the LOD1 model converts to fewer than four surfaces, so the single-creation path refuses it and the batch creation extrudes the footprint through the CRS-agnostic <c>Create.BuildingModel(Polyhedron, double)</c> overload. Before DiGi.GIS.PostgreSQL.UI#17 that branch stamped nothing.</para>
        /// </summary>
        [Fact]
        public void BuildingModels_Unidentified_HasCoordinates()
        {
            Building building = CityGML_Building_Sliver_3024();
            Assert.Null(Create.BuildingModel(building));

            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "3024_e189b2a2_CityGML.gml");
            List<CityModel>? cityModels = CityGML.Create.CityModels(path);
            Assert.NotNull(cityModels);

            PolygonalFace2D? polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D(new Point2D(331032.97, 539122.27), new Point2D(331030.36, 539122.98), new Point2D(331030.79, 539124.54), new Point2D(331033.39, 539123.84));
            Assert.NotNull(polygonalFace2D);

            Building2D building2D = new(Guid.NewGuid(), reference_Sliver_3024, polygonalFace2D, 1, null, null, []);

            List<BuildingModel>? buildingModels = Create.BuildingModels([building2D], cityModels, Constants.Tolerance.Coordinate);
            Assert.NotNull(buildingModels);

            BuildingModel buildingModel = Assert.Single(buildingModels);

            // The extrusion names its space "Building"; a model joined to a CityGML building carries that building's UniqueId instead -
            // so this asserts the model came out of the fallback the fact exists for, not of a stamped branch.
            List<Space>? spaces = buildingModel.GetSpaces<Space>();
            Assert.NotNull(spaces);
            Assert.Equal("Building", Assert.Single(spaces).Name);

            Assert.True(Query.IsLocated(buildingModel), "The extruded fallback model carries no located BuildingInformation.");
            Assert.Equal(UTC.Plus0100, buildingModel.BuildingInformation.UTC);
        }

        /// <summary>
        /// Tests that IsLocated reports each unlocated state - the (0, 0) coordinate default and an undefined UTC offset - and a stamped model as located.
        /// </summary>
        [Fact]
        public void IsLocated()
        {
            // The unlocated default: (0, 0) coordinates and an undefined UTC offset.
            BuildingModel buildingModel_Default = new();
            Assert.False(Query.IsLocated(buildingModel_Default));

            BuildingModel? buildingModel = Create.BuildingModel(Building2D_55417());
            Assert.NotNull(buildingModel);
            Assert.True(Query.IsLocated(buildingModel));

            // Located coordinates but the UTC offset still undefined - the sun path would carry a not-a-number offset.
            buildingModel.BuildingInformation.UTC = UTC.Undefined;
            Assert.False(Query.IsLocated(buildingModel));

            // The UTC offset defined but the coordinates still at the unlocated origin - the sun path would sit at (0, 0).
            BuildingModel buildingModel_Origin = new();
            buildingModel_Origin.BuildingInformation.UTC = UTC.Plus0100;
            Assert.False(Query.IsLocated(buildingModel_Origin));
        }
    }
}
