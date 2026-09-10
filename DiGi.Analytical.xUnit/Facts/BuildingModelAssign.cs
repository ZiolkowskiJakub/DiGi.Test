using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Analytical.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="BuildingModel.Assign(IZone, IEnumerable{ISpace})"/> creates a relation holding every given space.
        /// </summary>
        [Fact]
        public void BuildingModelAssign_Zone_Spaces()
        {
            Zone zone = new("Zone 1");

            Space space_1 = new(new Point3D(5, 5, 0), "Space 1");
            Space space_2 = new(new Point3D(50, 50, 0), "Space 2");

            BuildingModel buildingModel = new();
            List<ISpace> spaces = [space_1, space_2];
            Assert.True(buildingModel.Assign(zone, spaces));

            List<ISpace>? spaces_Zone = buildingModel.GetSpaces(zone);
            Assert.NotNull(spaces_Zone);
            Assert.Equal(2, spaces_Zone!.Count);
        }

        /// <summary>
        /// Tests that <see cref="BuildingModel.Assign(IZone, IEnumerable{ISpace})"/> fails without creating a relation when any space cannot be stored, instead of silently creating a partial relation.
        /// </summary>
        [Fact]
        public void BuildingModelAssign_Zone_Spaces_FailFast()
        {
            Zone zone = new("Zone 1");

            Space space_1 = new(new Point3D(5, 5, 0), "Space 1");

            BuildingModel buildingModel = new();
            List<ISpace> spaces = [space_1, null!];
            Assert.False(buildingModel.Assign(zone, spaces));

            Assert.Null(buildingModel.GetRelation<ZoneRelation>(zone));
        }

        /// <summary>
        /// Tests that <see cref="BuildingModel.Assign{TSpace}(IEnumerable{TSpace}, IInternalCondition, HourRange, string)"/> gives every created relation the same identifier, which is the group tag the consumers of <see cref="SpaceInternalConditionRelation.Id"/> rely on.
        /// </summary>
        [Fact]
        public void BuildingModelAssign_InternalConditions_ManySpaces_IdGroup()
        {
            Space space_1 = new(new Point3D(5, 5, 0), "Space 1");
            Space space_2 = new(new Point3D(50, 50, 0), "Space 2");

            InternalCondition internalCondition = new("Internal Condition 1");
            HourRange hourRange = new(0, 23);
            string id = "Version 1";

            BuildingModel buildingModel = new();
            List<Space> spaces = [space_1, space_2];
            Assert.True(buildingModel.Assign(spaces, internalCondition, hourRange, id));

            foreach (Space space in spaces)
            {
                List<SpaceInternalCondition>? spaceInternalConditions = buildingModel.GetSpaceInternalConditions(space);
                Assert.NotNull(spaceInternalConditions);
                Assert.Single(spaceInternalConditions!);
                Assert.Equal(id, spaceInternalConditions![0].Id);
                Assert.Equal(hourRange.Min, spaceInternalConditions[0].HourRange!.Min);
                Assert.Equal(hourRange.Max, spaceInternalConditions[0].HourRange!.Max);
            }
        }

        /// <summary>
        /// Tests that <see cref="BuildingModel.Assign{TSpace}(IEnumerable{TSpace}, IInternalCondition, HourRange, string)"/> skips spaces that cannot be stored and still succeeds for the remaining ones - the documented at-least-one-of-many contract.
        /// </summary>
        [Fact]
        public void BuildingModelAssign_InternalConditions_ManySpaces_NullEntry()
        {
            Space space_1 = new(new Point3D(5, 5, 0), "Space 1");

            InternalCondition internalCondition = new("Internal Condition 1");
            HourRange hourRange = new(0, 23);
            string id = "Version 1";

            BuildingModel buildingModel = new();
            List<Space> spaces = [space_1, null!];
            Assert.True(buildingModel.Assign(spaces, internalCondition, hourRange, id));

            List<SpaceInternalCondition>? spaceInternalConditions = buildingModel.GetSpaceInternalConditions(space_1);
            Assert.NotNull(spaceInternalConditions);
            Assert.Single(spaceInternalConditions!);
            Assert.Equal(id, spaceInternalConditions![0].Id);
        }
    }
}
