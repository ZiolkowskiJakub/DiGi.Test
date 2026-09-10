using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Creates a horizontal square <see cref="PolygonalFace3D"/> used as the geometry of the components under test.
        /// </summary>
        /// <returns>The created <see cref="PolygonalFace3D"/>.</returns>
        private static PolygonalFace3D BuildingModelUnassign_PolygonalFace3D()
        {
            Plane? plane = Geometry.Spatial.Create.Plane(0.0);

            PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(plane,
            [
                new Geometry.Planar.Classes.Point2D(0, 0),
                new Geometry.Planar.Classes.Point2D(0, 10),
                new Geometry.Planar.Classes.Point2D(10, 10),
                new Geometry.Planar.Classes.Point2D(10, 0)
            ]);

            Assert.NotNull(polygonalFace3D);

            return polygonalFace3D;
        }

        /// <summary>
        /// Tests that <see cref="BuildingModel.Unassign(IComponent, ISpace)"/> removes the space relation of a component bound by a single space, leaving the component stored but unassigned.
        /// </summary>
        [Fact]
        public void BuildingModelUnassign_Component_Space_LastSpace()
        {
            FaceFloor faceFloor = new(BuildingModelUnassign_PolygonalFace3D());

            Space space_1 = new(new Point3D(5, 5, 0), "Space 1");

            BuildingModel buildingModel = new();
            Assert.True(buildingModel.Assign(faceFloor, space_1));

            Assert.True(buildingModel.Unassign(faceFloor, space_1));

            List<ISpace>? spaces = buildingModel.GetSpaces(faceFloor);
            Assert.True(spaces == null || spaces.Count == 0);

            Assert.Null(buildingModel.GetRelation<SpaceRelation>(faceFloor));

            List<IFloor>? floors = buildingModel.GetComponents<IFloor>();
            Assert.NotNull(floors);
            Assert.Single(floors!);
        }

        /// <summary>
        /// Tests that <see cref="BuildingModel.Unassign(IComponent, ISpace)"/> removes only the requested space of a component bound by two spaces, keeping the other one.
        /// </summary>
        [Fact]
        public void BuildingModelUnassign_Component_Space_SecondOfTwo()
        {
            FaceFloor faceFloor = new(BuildingModelUnassign_PolygonalFace3D());

            Space space_1 = new(new Point3D(5, 5, 1), "Space 1");
            Space space_2 = new(new Point3D(5, 5, -1), "Space 2");

            BuildingModel buildingModel = new();
            Assert.True(buildingModel.Assign(faceFloor, space_1, space_2));

            Assert.True(buildingModel.Unassign(faceFloor, space_2));

            List<ISpace>? spaces = buildingModel.GetSpaces(faceFloor);
            Assert.NotNull(spaces);
            Assert.Single(spaces!);
            Assert.Equal(space_1.Guid, spaces![0].Guid);

            SpaceRelation? spaceRelation = buildingModel.GetRelation<SpaceRelation>(faceFloor);
            Assert.NotNull(spaceRelation);
        }

        /// <summary>
        /// Tests that <see cref="BuildingModel.Unassign(IComponent, ISpace)"/> returns false and keeps the relation unchanged when the space was never assigned to the component.
        /// </summary>
        [Fact]
        public void BuildingModelUnassign_Component_Space_Unrelated()
        {
            FaceFloor faceFloor = new(BuildingModelUnassign_PolygonalFace3D());

            Space space_1 = new(new Point3D(5, 5, 0), "Space 1");
            Space space_2 = new(new Point3D(50, 50, 0), "Space 2");

            BuildingModel buildingModel = new();
            Assert.True(buildingModel.Assign(faceFloor, space_1));

            Assert.False(buildingModel.Unassign(faceFloor, space_2));

            List<ISpace>? spaces = buildingModel.GetSpaces(faceFloor);
            Assert.NotNull(spaces);
            Assert.Single(spaces!);
            Assert.Equal(space_1.Guid, spaces![0].Guid);
        }

        /// <summary>
        /// Tests that <see cref="BuildingModel.Unassign(IZone, ISpace)"/> removes the zone relation holding a single space, leaving the zone and the space stored.
        /// </summary>
        [Fact]
        public void BuildingModelUnassign_Zone_Space_LastSpace()
        {
            Zone zone = new("Zone 1");

            Space space_1 = new(new Point3D(5, 5, 0), "Space 1");

            BuildingModel buildingModel = new();
            List<ISpace> spaces_Assign = [space_1];
            Assert.True(buildingModel.Assign(zone, spaces_Assign));

            Assert.True(buildingModel.Unassign(zone, space_1));

            List<ISpace>? spaces = buildingModel.GetSpaces(zone);
            Assert.True(spaces == null || spaces.Count == 0);

            Assert.Null(buildingModel.GetRelation<ZoneRelation>(zone));

            List<ISpace>? spaces_Model = buildingModel.GetSpaces<ISpace>();
            Assert.NotNull(spaces_Model);
            Assert.Single(spaces_Model!);
        }

        /// <summary>
        /// Tests that <see cref="BuildingModel.Unassign(IZone, ISpace)"/> removes only the requested space of a zone holding two spaces, keeping the other one.
        /// </summary>
        [Fact]
        public void BuildingModelUnassign_Zone_Space_SecondOfTwo()
        {
            Zone zone = new("Zone 1");

            Space space_1 = new(new Point3D(5, 5, 0), "Space 1");
            Space space_2 = new(new Point3D(50, 50, 0), "Space 2");

            BuildingModel buildingModel = new();
            List<ISpace> spaces_Assign = [space_1, space_2];
            Assert.True(buildingModel.Assign(zone, spaces_Assign));

            Assert.True(buildingModel.Unassign(zone, space_2));

            List<ISpace>? spaces = buildingModel.GetSpaces(zone);
            Assert.NotNull(spaces);
            Assert.Single(spaces!);
            Assert.Equal(space_1.Guid, spaces![0].Guid);
        }

        /// <summary>
        /// Tests that <see cref="BuildingModel.Unassign(IZone, ISpace)"/> returns false and keeps the relation unchanged when the space was never assigned to the zone.
        /// </summary>
        [Fact]
        public void BuildingModelUnassign_Zone_Space_Unrelated()
        {
            Zone zone = new("Zone 1");

            Space space_1 = new(new Point3D(5, 5, 0), "Space 1");
            Space space_2 = new(new Point3D(50, 50, 0), "Space 2");

            BuildingModel buildingModel = new();
            List<ISpace> spaces_Assign = [space_1];
            Assert.True(buildingModel.Assign(zone, spaces_Assign));

            Assert.False(buildingModel.Unassign(zone, space_2));

            List<ISpace>? spaces = buildingModel.GetSpaces(zone);
            Assert.NotNull(spaces);
            Assert.Single(spaces!);
            Assert.Equal(space_1.Guid, spaces![0].Guid);
        }
    }
}
