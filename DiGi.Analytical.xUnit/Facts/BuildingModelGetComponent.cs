using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        private class TestOpening : BuildingGeometry3DObject<Plane>, IOpening
        {
            public TestOpening(Plane? plane = null) : base(plane) { }
            public TestOpening(TestOpening? source) : base(source) { }
            public TestOpening(System.Text.Json.Nodes.JsonObject? jsonObject) : base(jsonObject) { }
        }

        /// <summary>
        /// Tests that <see cref="BuildingModel.GetComponent{TComponent}(IOpening?)"/> returns the cloned component associated with the given opening and returns null when the opening is null or has no relation in the model.
        /// </summary>
        [Fact]
        public void BuildingModel_GetComponent_Opening()
        {
            BuildingModel buildingModel = new();

            PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(
                Geometry.Spatial.Constants.Plane.WorldZ,
                [
                    new Geometry.Planar.Classes.Point2D(0, 0),
                    new Geometry.Planar.Classes.Point2D(10, 0),
                    new Geometry.Planar.Classes.Point2D(10, 10),
                    new Geometry.Planar.Classes.Point2D(0, 10)
                ]);
            Assert.NotNull(polygonalFace3D);

            SurfaceWall surfaceWall = new(polygonalFace3D);
            TestOpening opening = new(Geometry.Spatial.Constants.Plane.WorldZ);

            // Returns null before relation is created
            Assert.Null(buildingModel.GetComponent<IComponent>(opening));
            Assert.Null(buildingModel.GetComponent<IComponent>((IOpening?)null));

            // Store component and opening in the model
            buildingModel.Update(surfaceWall);
            buildingModel.Update(opening);

            // Query component before building relationship
            Assert.Null(buildingModel.GetComponent<IComponent>(opening));

            Core.xUnit.Query.SerializationCheck(buildingModel);
        }
    }
}
