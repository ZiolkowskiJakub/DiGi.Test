using DiGi.Analytical.Building;
using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="Building.Query.Surface3D(IOpening, IOpeningConstruction)"/> maps a rectangular construction outline onto the vertical placement plane of the opening (origin = insertion point) and returns null for null or geometry-less inputs.
        /// </summary>
        [Fact]
        public void Query_Surface3D_Opening()
        {
            Window window = new(OpeningFixture_Plane(2, 0, 0.5));
            WindowConstruction windowConstruction = OpeningFixture_WindowConstruction(1.2, 1.5);

            ISurface3D? surface3D = window.Surface3D(windowConstruction);
            IPolygonalFace3D? polygonalFace3D = surface3D as IPolygonalFace3D;
            Assert.NotNull(polygonalFace3D);

            List<Point3D>? point3Ds = polygonalFace3D.ExternalEdge?.GetPoints();
            Assert.NotNull(point3Ds);
            Assert.Equal(4, point3Ds.Count);

            List<Point3D> point3Ds_Expected = [new(2, 0, 0.5), new(3.2, 0, 0.5), new(3.2, 0, 2), new(2, 0, 2)];
            foreach (Point3D point3D_Expected in point3Ds_Expected)
            {
                Assert.Contains(point3Ds, x => x.Distance(point3D_Expected) < Core.Constants.Tolerance.Distance);
            }

            Assert.Null(((IOpening?)null).Surface3D(windowConstruction));
            Assert.Null(window.Surface3D(null));
            Assert.Null(new Window((Plane?)null).Surface3D(windowConstruction));
            Assert.Null(window.Surface3D(new WindowConstruction("Empty", null)));
        }

        /// <summary>
        /// Tests <see cref="Building.Query.IsValidHost(IPhysicalComponent, IOpening, IOpeningConstruction, double)"/> on a 10 x 3 <see cref="SurfaceWall"/>: an opening inside the face or touching its edge is valid, while one partially outside, overlapping a hole, enclosing a hole (centred on it or not) or exactly covering a hole is not.
        /// <para>The out-of-plane offset is checked just inside (0.5 x tolerance) and just outside (2 x tolerance) the tolerance.</para>
        /// </summary>
        [Fact]
        public void Query_IsValidHost()
        {
            double tolerance = Core.Constants.Tolerance.Distance;

            SurfaceWall surfaceWall = OpeningFixture_SurfaceWall();
            WindowConstruction windowConstruction = OpeningFixture_WindowConstruction(1.2, 1.5);

            Assert.True(surfaceWall.IsValidHost(new Window(OpeningFixture_Plane(2, 0, 0.5)), windowConstruction, tolerance));

            DoorConstruction doorConstruction = new("Door Construction", OpeningFixture_Rectangle(0, 0, 1, 2.1));
            Assert.True(surfaceWall.IsValidHost(new Door(OpeningFixture_Plane(5, 0, 0)), doorConstruction, tolerance));
            Assert.True(surfaceWall.IsValidHost(new Window(OpeningFixture_Plane(8.8, 0, 1.5)), windowConstruction, tolerance));

            Assert.False(surfaceWall.IsValidHost(new Window(OpeningFixture_Plane(9.5, 0, 1)), windowConstruction, tolerance));
            Assert.False(surfaceWall.IsValidHost(new Window(OpeningFixture_Plane(2, 0, 2)), windowConstruction, tolerance));
            Assert.False(surfaceWall.IsValidHost(new Window(OpeningFixture_Plane(-0.5, 0, 0.5)), windowConstruction, tolerance));

            Assert.True(surfaceWall.IsValidHost(new Window(OpeningFixture_Plane(2, 0.5 * tolerance, 0.5)), windowConstruction, tolerance));
            Assert.False(surfaceWall.IsValidHost(new Window(OpeningFixture_Plane(2, 2 * tolerance, 0.5)), windowConstruction, tolerance));

            SurfaceWall surfaceWall_Hole = OpeningFixture_SurfaceWall((6, 1, 7, 2));
            Assert.True(surfaceWall_Hole.IsValidHost(new Window(OpeningFixture_Plane(2, 0, 0.5)), windowConstruction, tolerance));
            Assert.False(surfaceWall_Hole.IsValidHost(new Window(OpeningFixture_Plane(5.5, 0, 1.2)), new WindowConstruction("Overlapping", OpeningFixture_Rectangle(0, 0, 1, 0.6)), tolerance));
            Assert.False(surfaceWall_Hole.IsValidHost(new Window(OpeningFixture_Plane(5.5, 0, 0.5)), new WindowConstruction("Enclosing", OpeningFixture_Rectangle(0, 0, 2, 2)), tolerance));
            Assert.False(surfaceWall_Hole.IsValidHost(new Window(OpeningFixture_Plane(3, 0, 0.2)), new WindowConstruction("Enclosing Off-Centre", OpeningFixture_Rectangle(0, 0, 5.5, 2.6)), tolerance));
            Assert.False(surfaceWall_Hole.IsValidHost(new Window(OpeningFixture_Plane(6, 0, 1)), new WindowConstruction("Covering", OpeningFixture_Rectangle(0, 0, 1, 1)), tolerance));

            Assert.False(((IPhysicalComponent?)null).IsValidHost(new Window(OpeningFixture_Plane(2, 0, 0.5)), windowConstruction, tolerance));
            Assert.False(surfaceWall.IsValidHost(null, windowConstruction, tolerance));
            Assert.False(surfaceWall.IsValidHost(new Window(OpeningFixture_Plane(2, 0, 0.5)), null, tolerance));
        }

        /// <summary>
        /// Tests <see cref="BuildingModel.Assign(IPhysicalComponent, IWindow, IWindowConstruction, double)"/> and <see cref="BuildingModel.Assign(IPhysicalComponent, IDoor, IDoorConstruction, double)"/>: a valid window and door are hosted on the wall with their constructions in one call, an invalid placement returns false and leaves the model unchanged, and a window is moved when re-hosted on a second wall.
        /// </summary>
        [Fact]
        public void BuildingModelAssign_PhysicalComponent_Opening()
        {
            SurfaceWall surfaceWall = OpeningFixture_SurfaceWall();

            Window window = new(OpeningFixture_Plane(2, 0, 0.5));
            WindowConstruction windowConstruction = OpeningFixture_WindowConstruction(1.2, 1.5);

            Door door = new(OpeningFixture_Plane(5, 0, 0));
            DoorConstruction doorConstruction = new("Door Construction", OpeningFixture_Rectangle(0, 0, 1, 2.1));

            BuildingModel buildingModel = new();
            Assert.True(buildingModel.Assign(surfaceWall, window, windowConstruction));
            Assert.True(buildingModel.Assign(surfaceWall, door, doorConstruction));

            List<IWindow>? windows = buildingModel.GetOpenings<IWindow>(surfaceWall);
            Assert.NotNull(windows);
            Assert.Single(windows);
            Assert.Equal(window.Guid, windows[0].Guid);

            List<IDoor>? doors = buildingModel.GetOpenings<IDoor>(surfaceWall);
            Assert.NotNull(doors);
            Assert.Single(doors);
            Assert.Equal(door.Guid, doors[0].Guid);

            Assert.Equal(windowConstruction.Guid, buildingModel.GetWindowConstruction(window)?.Guid);
            Assert.Equal(doorConstruction.Guid, buildingModel.GetDoorConstruction(door)?.Guid);
            Assert.Equal(windowConstruction.Guid, buildingModel.GetOpeningConstruction<IOpeningConstruction>(window)?.Guid);
            Assert.Equal(surfaceWall.Guid, buildingModel.GetComponent<IWall>(window)?.Guid);
            Assert.Equal(surfaceWall.Guid, buildingModel.GetComponent<IWall>(door)?.Guid);

            Core.xUnit.Query.SerializationCheck(buildingModel);

            string? json_Before = Core.Convert.ToSystem_String(buildingModel);

            Window window_Invalid = new(OpeningFixture_Plane(9.5, 0, 1));
            Assert.False(buildingModel.Assign(surfaceWall, window_Invalid, new WindowConstruction("Invalid", OpeningFixture_Rectangle(0, 0, 1.2, 1.5))));
            Assert.False(buildingModel.Assign(surfaceWall, (IWindow?)null, windowConstruction));
            Assert.False(buildingModel.Assign(null, window, windowConstruction));

            Assert.Equal(json_Before, Core.Convert.ToSystem_String(buildingModel));

            PolygonalFace3D? polygonalFace3D_Second = OpeningFixture_Plane(0, 0, 0).Convert(OpeningFixture_Rectangle(0, 0, 5, 3));
            Assert.NotNull(polygonalFace3D_Second);
            SurfaceWall surfaceWall_Second = new(polygonalFace3D_Second);

            Assert.True(buildingModel.Assign(surfaceWall_Second, window, windowConstruction));

            List<IWindow>? windows_First = buildingModel.GetOpenings<IWindow>(surfaceWall);
            Assert.True(windows_First == null || windows_First.Count == 0);

            List<IWindow>? windows_Second = buildingModel.GetOpenings<IWindow>(surfaceWall_Second);
            Assert.NotNull(windows_Second);
            Assert.Single(windows_Second);
            Assert.Equal(window.Guid, windows_Second[0].Guid);

            Assert.Equal(surfaceWall.Guid, buildingModel.GetComponent<IWall>(door)?.Guid);

            Core.xUnit.Query.SerializationCheck(buildingModel);
        }
    }
}
