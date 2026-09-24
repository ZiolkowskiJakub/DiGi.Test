using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Analytical.Classes;
using DiGi.CityGML.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.GIS.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DiGi.GIS.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Reference of the building of county 3024 reported by DiGi.GIS.Analytical#1, whose LOD1 model was built from a sliver footprint BDOT10k has since corrected.
        /// </summary>
        private const string reference_Sliver_3024 = "e189b2a2-9f5e-46f6-a540-235c03e5afcb";

        /// <summary>
        /// Tests that a footprint too small to carry a floor produces no building model rather than walls around nothing.
        /// <para>The three BDOT10k footprints reported by DiGi.GIS.Analytical#1 (counties 2007, 2807 and 3005) are triangles of 0.0050, 0.0011 and 0.0026 square metres. At <see cref="Constants.Tolerance.Coordinate"/> their floor and roof were rejected by area while every edge was still long enough for a wall, so each came out as three walls with no envelope and failed the external components run.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_SliverFootprint_ReturnsNull()
        {
            List<Point2D[]> point2Ds_Slivers =
            [
                [new(700343.96, 578938.75), new(700344.02, 578938.62), new(700344.06, 578938.70)],
                [new(524871.99, 645200.83), new(524881.74, 645200.61), new(524881.73, 645200.61)],
                [new(322130.84, 489265.13), new(322130.98, 489265.08), new(322130.96, 489265.05)],
            ];

            foreach (Point2D[] point2Ds in point2Ds_Slivers)
            {
                Building2D building2D = Building2D_Polygon(point2Ds);

                Assert.Null(Create.BuildingModel(building2D, 100.0, Constants.StoreyHeight.Default, Constants.Tolerance.Coordinate));
                Assert.Null(Create.BuildingModel(null, building2D, 100.0));
            }
        }

        /// <summary>
        /// Tests the floor area boundary of the footprint extrusion at <see cref="Constants.Tolerance.Coordinate"/>.
        /// <para>A right triangle of 0.0098 square metres is rejected, while one of 0.0102 square metres still builds a model holding one floor, one roof and three walls.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_SliverFootprint_AreaBoundary()
        {
            Building2D building2D_Below = Building2D_Polygon(new Point2D(0, 0), new Point2D(0.14, 0), new Point2D(0, 0.14));
            Assert.Null(Create.BuildingModel(building2D_Below, 100.0, Constants.StoreyHeight.Default, Constants.Tolerance.Coordinate));

            Building2D building2D_Above = Building2D_Polygon(new Point2D(0, 0), new Point2D(0.1429, 0), new Point2D(0, 0.1429));
            BuildingModel? buildingModel = Create.BuildingModel(building2D_Above, 100.0, Constants.StoreyHeight.Default, Constants.Tolerance.Coordinate);
            Assert.NotNull(buildingModel);

            Assert.Single(buildingModel.GetComponents<IFaceFloor>() ?? []);
            Assert.Single(buildingModel.GetComponents<ISurfaceRoof>() ?? []);
            Assert.Equal(3, (buildingModel.GetComponents<CurveWall>() ?? []).Count);
        }

        /// <summary>
        /// Tests that a CityGML building converting to fewer than four surfaces is replaced by the extruded footprint.
        /// <para>The LOD1 model of building 3024 reported by DiGi.GIS.Analytical#1 carries a ground and a roof ring that are sliver triangles with a corner of 0.34 degrees, below the angle tolerance of the plane fit, so the conversion drops both and keeps the three walls. The model used to be built from those walls alone. The current BDOT10k footprint of the building is a valid quadrilateral of about 4.3 square metres, and the model is now extruded from it.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_CityGML_3024_FallsBackToFootprint()
        {
            Building building = CityGML_Building_Sliver_3024();

            int count = 0;
            foreach (CityGML.Interfaces.ISurface surface in building.Surfaces ?? [])
            {
                count++;
            }

            Assert.Equal(3, count);
            Assert.Null(CityGML.Query.Polyhedron(building));

            Building2D building2D = Building2D_Polygon(new Point2D(331032.97, 539122.27), new Point2D(331030.36, 539122.98), new Point2D(331030.79, 539124.54), new Point2D(331033.39, 539123.84));

            BuildingModel? buildingModel = Create.BuildingModel(building, building2D, 61.3928755941663);
            Assert.NotNull(buildingModel);

            Assert.Empty(buildingModel.GetComponents<ISurfaceWall>() ?? []);
            Assert.Single(buildingModel.GetComponents<IFaceFloor>() ?? []);
            Assert.Single(buildingModel.GetComponents<ISurfaceRoof>() ?? []);
            Assert.Equal(4, (buildingModel.GetComponents<CurveWall>() ?? []).Count);

            Shell? shell = buildingModel.GetExternalShell(tolerance: Constants.Tolerance.Coordinate);
            Assert.NotNull(shell);
            Assert.Equal(6, shell.Count);
            Assert.True(shell.IsClosed(Constants.Tolerance.Coordinate));

            // Without an elevation the fallback refuses, so the caller queries the terrain service as for any building without 3D geometry
            Assert.Null(Create.BuildingModel(building, building2D, double.NaN));

            // The 2021 footprint the LOD1 model was built from is a sliver of 0.0136 square metres - above the floor
            // threshold of the extrusion, so it still yields a model, and one that is enclosed rather than walls alone
            Building2D building2D_Sliver = Building2D_Polygon(new Point2D(331031.13, 539125.71), new Point2D(331030.36, 539122.98), new Point2D(331030.79, 539124.54));
            BuildingModel? buildingModel_Sliver = Create.BuildingModel(building, building2D_Sliver, 61.3928755941663);
            Assert.NotNull(buildingModel_Sliver);
            Assert.Single(buildingModel_Sliver.GetComponents<IFaceFloor>() ?? []);
            Assert.Single(buildingModel_Sliver.GetComponents<ISurfaceRoof>() ?? []);
        }

        /// <summary>
        /// Tests that the batch creation used by the directory import extrudes the footprint of a CityGML building converting to fewer than four surfaces.
        /// <para><see cref="Create.BuildingModel(Building, double)"/> returns null for such a building instead of a model of walls alone, so <see cref="Create.BuildingModels(IEnumerable{Building2D}, IEnumerable{CityModel}, double)"/> hands the 2D building on to the footprint extrusion, which takes its base elevation from the nearest CityGML geometry - here the neighbouring building the fixture carries for that purpose.</para>
        /// </summary>
        [Fact]
        public void BuildingModels_CityGML_3024_FallsBackToFootprint()
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

            // The extrusion names its space "Building"; a model joined to a CityGML building carries that building's UniqueId instead
            List<Space>? spaces = buildingModel.GetSpaces<Space>();
            Assert.NotNull(spaces);
            Assert.Equal("Building", Assert.Single(spaces).Name);

            Assert.Single(buildingModel.GetComponents<IFaceFloor>() ?? []);
            Assert.Single(buildingModel.GetComponents<ISurfaceRoof>() ?? []);
            Assert.Equal(4, (buildingModel.GetComponents<ISurfaceWall>() ?? []).Count);

            BoundingBox3D? boundingBox3D = buildingModel.GetBoundingBox();
            Assert.NotNull(boundingBox3D);
            // The walls of the refused building project onto no area, so the base elevation comes from the neighbour (61.81 m) rather than from them (61.39 m)
            Assert.True(System.Math.Abs(boundingBox3D.Min.Z - 61.8128755941663) < Constants.Tolerance.Coordinate, $"The footprint was extruded from {boundingBox3D.Min.Z} instead of the base of the neighbouring CityGML building.");
        }

        /// <summary>
        /// Tests that a footprint no storey can be built from produces no building model rather than an empty one.
        /// <para>A vertical face projects onto no horizontal storey plane, so every storey is skipped. The extrusion used to return a model holding no space and no component.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_UnprojectableFootprint_ReturnsNull()
        {
            Polygon3D? polygon3D = Geometry.Spatial.Create.Polygon3D([new Point3D(0, 0, 0), new Point3D(10, 0, 0), new Point3D(10, 0, 3), new Point3D(0, 0, 3)]);
            Assert.NotNull(polygon3D);

            PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(polygon3D);
            Assert.NotNull(polygonalFace3D);

            Assert.Null(Create.BuildingModel(polygonalFace3D, 2, Constants.StoreyHeight.Default, Constants.Tolerance.Coordinate));
        }

        /// <summary>
        /// Loads the LOD1 building of county 3024 reported by DiGi.GIS.Analytical#1.
        /// </summary>
        /// <returns>The building of the "3024_e189b2a2_CityGML.gml" fixture.</returns>
        private static Building CityGML_Building_Sliver_3024()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "3024_e189b2a2_CityGML.gml");

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
                    if (CityGML.Query.Reference(building) == reference_Sliver_3024)
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
        /// Creates a single storey 2D building with the given footprint.
        /// </summary>
        /// <param name="point2Ds">The corners of the footprint.</param>
        /// <returns>A <see cref="Building2D"/> carrying the footprint.</returns>
        private static Building2D Building2D_Polygon(params Point2D[] point2Ds)
        {
            PolygonalFace2D? polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D(point2Ds);
            Assert.NotNull(polygonalFace2D);

            return new Building2D(Guid.NewGuid(), Guid.NewGuid().ToString(), polygonalFace2D, 1, null, null, []);
        }
    }
}
