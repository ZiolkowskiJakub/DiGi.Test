using DiGi.Analytical.Building.Classes;
using DiGi.CityGML.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.GIS.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that the BuildingModels extension method returns an empty list when the input Building2D collection is empty.
        /// </summary>
        [Fact]
        public void BuildingModels_EmptyInput()
        {
            List<Building2D> building2Ds = [];
            List<CityModel> cityModels = [];

            List<BuildingModel>? buildingModels = Create.BuildingModels(building2Ds, cityModels);

            Assert.NotNull(buildingModels);
            Assert.Empty(buildingModels);
        }

        /// <summary>
        /// Tests that the BuildingModels extension method produces models for a non-empty Building2D collection.
        /// <para>The city model is built in memory by lifting the Building2D footprint onto the world XY plane, so the CityGML geometry matches the 2D building exactly and a model must be produced.</para>
        /// <para>Guards the input check at the top of the method - an inverted guard there short-circuits every non-empty input to an empty result.</para>
        /// </summary>
        [Fact]
        public void BuildingModels_NonEmptyInput()
        {
            Building2D? building2D = Core.Convert.ToDiGi<Building2D>(Building2D_Json())?.FirstOrDefault();

            Assert.NotNull(building2D);
            Assert.NotNull(building2D.PolygonalFace2D);

            PolygonalFace3D polygonalFace3D = new(Geometry.Spatial.Constants.Plane.WorldZ, building2D.PolygonalFace2D);

            GroundSurface groundSurface = new("GroundSurface_1", polygonalFace3D);

            Building building = new("Building_1", -1, [groundSurface]);

            CityModel cityModel = new([building]);

            List<Building2D> building2Ds = [building2D];
            List<CityModel> cityModels = [cityModel];

            List<BuildingModel>? buildingModels = Create.BuildingModels(building2Ds, cityModels);

            Assert.NotNull(buildingModels);
            Assert.NotEmpty(buildingModels);
        }

        /// <summary>
        /// Tests that the BuildingModels overload taking CityGML buildings joins them to the 2D buildings on their shared reference.
        /// <para>The CityGML building carries the buildingId parameter of the Building2D but its footprint is offset by 100m, so only the reference join can select it - the spatial stage of the fallback cannot reach it.</para>
        /// <para>The space name distinguishes a reference-joined model from an extruded one.</para>
        /// </summary>
        [Fact]
        public void BuildingModels_ByReference_Matching()
        {
            Building2D? building2D = Core.Convert.ToDiGi<Building2D>(Building2D_Json())?.FirstOrDefault();

            Assert.NotNull(building2D);
            Assert.NotNull(building2D.PolygonalFace2D);
            Assert.False(string.IsNullOrWhiteSpace(building2D.Reference));

            Building building = Building_Offset(building2D.Reference);

            Assert.Equal(building2D.Reference, CityGML.Query.Reference(building));

            List<Building2D> building2Ds = [building2D];
            List<Building> buildings = [building];

            List<BuildingModel>? buildingModels = Create.BuildingModels(building2Ds, buildings);

            Assert.NotNull(buildingModels);
            Assert.Single(buildingModels);

            List<Space>? spaces = buildingModels[0].GetSpaces<Space>();
            Assert.NotNull(spaces);
            Assert.NotEmpty(spaces);

            // Create.BuildingModel(Building) names the space after the building UniqueId, whereas the extruded fallback names it "Building" - so the name identifies which path produced the model.
            Assert.Equal("Building_1", spaces[0].Name);
        }

        /// <summary>
        /// Tests that a 2D building whose reference matches no CityGML building still yields a model through the extruded footprint fallback.
        /// <para>The CityGML building is given both a different reference and a footprint offset far enough that the spatial stage of the fallback cannot match it either, so only the extrusion stage can produce the model.</para>
        /// <para>The resulting model is therefore expected at the Building2D location rather than the offset CityGML location.</para>
        /// </summary>
        [Fact]
        public void BuildingModels_ByReference_Unmatched()
        {
            Building2D? building2D = Core.Convert.ToDiGi<Building2D>(Building2D_Json())?.FirstOrDefault();

            Assert.NotNull(building2D);
            Assert.NotNull(building2D.PolygonalFace2D);

            Building building = Building_Offset("not-the-building2D-reference");

            Assert.NotEqual(building2D.Reference, CityGML.Query.Reference(building));

            List<Building2D> building2Ds = [building2D];
            List<Building> buildings = [building];

            List<BuildingModel>? buildingModels = Create.BuildingModels(building2Ds, buildings);

            Assert.NotNull(buildingModels);
            Assert.Single(buildingModels);

            List<Space>? spaces = buildingModels[0].GetSpaces<Space>();
            Assert.NotNull(spaces);
            Assert.NotEmpty(spaces);

            // The extruded fallback names the space "Building"; a reference-joined model would carry the CityGML building UniqueId instead.
            Assert.Equal("Building", spaces[0].Name);
        }

        private static Building Building_Offset(string? reference)
        {
            // Offset by 100m in both X and Y from the Building2D footprint so neither the bounding box nor the face covers the Building2D internal point.
            List<Point2D> point2Ds =
            [
                new(482530.74, 559148.76),
                new(482525.19, 559150.29),
                new(482527.49, 559158.66),
                new(482533.15, 559157.01)
            ];

            Polygon3D polygon3D = new(Geometry.Spatial.Constants.Plane.WorldZ, point2Ds);

            PolygonalFace3D polygonalFace3D = new(polygon3D);

            GroundSurface groundSurface = new("GroundSurface_1", polygonalFace3D);

            Building result = new("Building_1", -1, [groundSurface]);
            result.SetValue(CityGML.Enums.BuildingParameter.buildingId, reference, new Core.Parameter.Classes.SetValueSettings(true, false));

            return result;
        }

        private static string Building2D_Json()
        {
            return "{\"_type\":\"DiGi.GIS.Classes.Building2D,DiGi.GIS\",\"Guid\":\"8531176b-ae1f-4dbc-b864-e71ab24a6af2\",\"Reference\":\"1fc24a0d-8d0c-4e15-b6d2-ea52124f30b7\",\"PolygonalFace2D\":{\"_type\":\"DiGi.Geometry.Planar.Classes.PolygonalFace2D,DiGi.Geometry\",\"ExternalEdge\":{\"_type\":\"DiGi.Geometry.Planar.Classes.Polygon2D,DiGi.Geometry\",\"Points\":[{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":482430.74,\"Y\":559048.76},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":482425.19,\"Y\":559050.29},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":482427.49,\"Y\":559058.66},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":482433.15,\"Y\":559057.01}]},\"InternalEdges\":null},\"Storeys\":2,\"BuildingGeneralFunction\":4,\"BuildingSpecificFunctions\":[7],\"BuildingPhase\":0}";
        }
    }
}
