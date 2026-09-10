using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Geometry.Spatial.Classes;
using System.Text.Json.Nodes;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a serialized <see cref="BuildingModel"/> carrying an explicit null in place of its building relation cluster deserializes into a model that still answers queries, rather than one holding a null cluster.
        /// </summary>
        [Fact]
        public void BuildingModelDeserialization_NullRelationCluster()
        {
            FaceFloor faceFloor = new(BuildingModelUnassign_PolygonalFace3D());

            Space space = new(new Point3D(5, 5, 0), "Space 1");

            BuildingModel buildingModel = new();
            Assert.True(buildingModel.Assign(faceFloor, space));

            JsonObject? jsonObject = buildingModel.ToJsonObject();
            Assert.NotNull(jsonObject);

            jsonObject!.Remove("BuildingRelationCluster");
            jsonObject.Add("BuildingRelationCluster", null);

            BuildingModel buildingModel_Deserialized = new(jsonObject);

            List<IFloor>? floors = buildingModel_Deserialized.GetComponents<IFloor>();
            Assert.NotNull(floors);
            Assert.Empty(floors!);
        }
    }
}
