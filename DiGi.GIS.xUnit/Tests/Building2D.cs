using DiGi.GIS.Classes;
using System.Linq;

namespace DiGi.GIS.xUnit
{
    public partial class Tests
    {
        [Fact]
        public void Building2D()
        {
            string json = "{\"_type\":\"DiGi.GIS.Classes.Building2D,DiGi.GIS\",\"Guid\":\"8531176b-ae1f-4dbc-b864-e71ab24a6af2\",\"Reference\":\"1fc24a0d-8d0c-4e15-b6d2-ea52124f30b7\",\"PolygonalFace2D\":{\"_type\":\"DiGi.Geometry.Planar.Classes.PolygonalFace2D,DiGi.Geometry\",\"ExternalEdge\":{\"_type\":\"DiGi.Geometry.Planar.Classes.Polygon2D,DiGi.Geometry\",\"Points\":[{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":482430.74,\"Y\":559048.76},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":482425.19,\"Y\":559050.29},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":482427.49,\"Y\":559058.66},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":482433.15,\"Y\":559057.01}]},\"InternalEdges\":null},\"Storeys\":1,\"BuildingGeneralFunction\":4,\"BuildingSpecificFunctions\":[7],\"BuildingPhase\":0}";

            Building2D? building2D = Core.Convert.ToDiGi<Building2D>(json)?.FirstOrDefault();

            Assert.NotNull(building2D);

            Assert.True(building2D.BuildingSpecificFunctions is not null && building2D.BuildingSpecificFunctions.Count > 0);

            string? json_Temp = Core.Convert.ToSystem_String(building2D);

            Assert.NotNull(json_Temp);

            Assert.Equal(json, json_Temp);
        }
    }
}