using DiGi.Analytical.Building.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the constructors of <see cref="Window"/>: the placement plane is kept by every constructor, the unique identifier is kept by the copy constructor and replaced by the Guid constructors, and the window survives a string round trip and <see cref="Core.xUnit.Query.SerializationCheck{T}(T)"/>.
        /// <para>The copy constructors are asserted directly because <c>SerializationCheck</c> clones through the JSON constructor and never runs them.</para>
        /// </summary>
        [Fact]
        public void Window_Constructors()
        {
            Plane plane = OpeningFixture_Plane(2, 0, 0.5);

            Window window = new(plane);
            Assert.NotNull(window.Geometry);
            Assert.Equal(0, window.Geometry!.Origin!.Distance(plane.Origin));

            System.Guid guid = System.Guid.NewGuid();
            Window window_Guid = new(guid, plane);
            Assert.Equal(guid, window_Guid.Guid);
            Assert.Equal(0, window_Guid.Geometry!.Origin!.Distance(plane.Origin));

            Window window_Copy = new(window);
            Assert.Equal(window.Guid, window_Copy.Guid);
            Assert.Equal(Core.Convert.ToSystem_String(window), Core.Convert.ToSystem_String(window_Copy));

            Window window_GuidCopy = new(guid, window);
            Assert.Equal(guid, window_GuidCopy.Guid);
            Assert.Equal(Core.Convert.ToSystem_String(window.Geometry), Core.Convert.ToSystem_String(window_GuidCopy.Geometry));

            Window window_Null = new((Plane?)null);
            Assert.Null(window_Null.Geometry);

            string? json = Core.Convert.ToSystem_String(window);
            Assert.False(string.IsNullOrWhiteSpace(json));

            Window? window_Json = Core.Convert.ToDiGi<Window>(json)?.FirstOrDefault();
            Assert.NotNull(window_Json);
            Assert.Equal(window.Guid, window_Json.Guid);
            Assert.Equal(0, window_Json.Geometry!.Origin!.Distance(plane.Origin));

            Core.xUnit.Query.SerializationCheck(window);
            Core.xUnit.Query.SerializationCheck(window_Null);
        }

        /// <summary>
        /// Tests the constructors of <see cref="Door"/>: the placement plane is kept by every constructor, the unique identifier is kept by the copy constructor and replaced by the Guid constructors, and the door survives a string round trip and <see cref="Core.xUnit.Query.SerializationCheck{T}(T)"/>.
        /// <para>The copy constructors are asserted directly because <c>SerializationCheck</c> clones through the JSON constructor and never runs them.</para>
        /// </summary>
        [Fact]
        public void Door_Constructors()
        {
            Plane plane = OpeningFixture_Plane(5, 0, 0);

            Door door = new(plane);
            Assert.NotNull(door.Geometry);
            Assert.Equal(0, door.Geometry!.Origin!.Distance(plane.Origin));

            System.Guid guid = System.Guid.NewGuid();
            Door door_Guid = new(guid, plane);
            Assert.Equal(guid, door_Guid.Guid);
            Assert.Equal(0, door_Guid.Geometry!.Origin!.Distance(plane.Origin));

            Door door_Copy = new(door);
            Assert.Equal(door.Guid, door_Copy.Guid);
            Assert.Equal(Core.Convert.ToSystem_String(door), Core.Convert.ToSystem_String(door_Copy));

            Door door_GuidCopy = new(guid, door);
            Assert.Equal(guid, door_GuidCopy.Guid);
            Assert.Equal(Core.Convert.ToSystem_String(door.Geometry), Core.Convert.ToSystem_String(door_GuidCopy.Geometry));

            string? json = Core.Convert.ToSystem_String(door);
            Assert.False(string.IsNullOrWhiteSpace(json));

            Door? door_Json = Core.Convert.ToDiGi<Door>(json)?.FirstOrDefault();
            Assert.NotNull(door_Json);
            Assert.Equal(door.Guid, door_Json.Guid);
            Assert.Equal(0, door_Json.Geometry!.Origin!.Distance(plane.Origin));

            Core.xUnit.Query.SerializationCheck(door);
        }

        /// <summary>
        /// Regression test: the copy constructors of <see cref="WindowConstruction"/> and <see cref="DoorConstruction"/> keep the <see cref="OpeningConstruction.Name"/>, <see cref="OpeningConstruction.Description"/> and the outline geometry. They used to drop the name and the description, which <c>SerializationCheck</c> cannot detect.
        /// </summary>
        [Fact]
        public void OpeningConstruction_Copy()
        {
            WindowConstruction windowConstruction = new("Window Construction", OpeningFixture_Rectangle(0, 0, 1.2, 1.5))
            {
                Description = "Window Description"
            };

            System.Guid guid = System.Guid.NewGuid();
            List<WindowConstruction> windowConstructions_Copy = [new(windowConstruction), new(guid, windowConstruction)];
            foreach (WindowConstruction windowConstruction_Copy in windowConstructions_Copy)
            {
                Assert.Equal("Window Construction", windowConstruction_Copy.Name);
                Assert.Equal("Window Description", windowConstruction_Copy.Description);
                Assert.Equal(Core.Convert.ToSystem_String(windowConstruction.Geometry), Core.Convert.ToSystem_String(windowConstruction_Copy.Geometry));
            }

            Assert.Equal(guid, new WindowConstruction(guid, windowConstruction).Guid);

            DoorConstruction doorConstruction = new("Door Construction", OpeningFixture_Rectangle(0, 0, 1, 2.1))
            {
                Description = "Door Description"
            };

            List<DoorConstruction> doorConstructions_Copy = [new(doorConstruction), new(guid, doorConstruction)];
            foreach (DoorConstruction doorConstruction_Copy in doorConstructions_Copy)
            {
                Assert.Equal("Door Construction", doorConstruction_Copy.Name);
                Assert.Equal("Door Description", doorConstruction_Copy.Description);
                Assert.Equal(Core.Convert.ToSystem_String(doorConstruction.Geometry), Core.Convert.ToSystem_String(doorConstruction_Copy.Geometry));
            }

            Core.xUnit.Query.SerializationCheck(windowConstruction);
            Core.xUnit.Query.SerializationCheck(doorConstruction);
        }
    }
}
