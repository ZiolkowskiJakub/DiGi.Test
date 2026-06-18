using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.HVAC;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Analytical.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Analytical.xUnit
{
    public partial class Classes
    {
        /// <summary>
        /// Tests the creation and configuration of a building model, including the assignment of floors, spaces, and internal conditions.
        /// </summary>
        [Fact]
        public void BuildingModel()
        {
            Plane? plane = Geometry.Spatial.Create.Plane(0.0);

            PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(plane,
            [
                new Geometry.Planar.Classes.Point2D(0, 0),
                new Geometry.Planar.Classes.Point2D(0, 10),
                new Geometry.Planar.Classes.Point2D(10, 0),
                new Geometry.Planar.Classes.Point2D(10, 10)
            ]);

            FaceFloor faceFloor = new(polygonalFace3D);

            Assert.NotNull(faceFloor.Geometry);

            Space space = new(new Point3D(0, 0, 0), "Space 1");

            Profile profile = new([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24]);

            InternalCondition internalCondition = new("Internal Condition 1");
            internalCondition.SetProfile(Building.HVAC.Enums.InternalGainProfileType.LightingGain, profile);

            Assert.NotNull(space.Name);

            BuildingModel buildingModel = new();
            buildingModel.Assign(faceFloor, space);

            HourRange hourRange = new(0, 11);
            string id = "Version 1";
            buildingModel.Assign(space, internalCondition, hourRange, id);

            List<IFloor>? floors = buildingModel.GetComponents<IFloor>();

            Assert.NotNull(floors);

            Assert.Single(floors);

            List<ISpace>? spaces = buildingModel.GetSpaces(floors[0]);

            List<SpaceInternalCondition>? spaceInternalConditions = buildingModel.GetSpaceInternalConditions(space);
            Assert.NotNull(spaceInternalConditions);

            if (spaceInternalConditions != null)
            {
                Assert.Single(spaceInternalConditions);

                if (spaceInternalConditions.Count > 0)
                {
                    SpaceInternalCondition? spaceInternalCondition = spaceInternalConditions.Find(x => x.Id == id);
                    Assert.NotNull(spaceInternalCondition);

                    Assert.Equal(id, spaceInternalCondition.Id);

                    HourRange? hourRange_Temp = spaceInternalCondition.HourRange;
                    Assert.NotNull(hourRange_Temp);

                    if (hourRange_Temp is not null)
                    {
                        Assert.Equal(hourRange.Length, hourRange_Temp.Length);
                        if (hourRange.Length == hourRange_Temp.Length)
                        {
                            Assert.Equal(hourRange.Length, hourRange_Temp.Length);
                            Assert.Equal(hourRange.Min, hourRange_Temp.Min);
                            Assert.Equal(hourRange.Max, hourRange_Temp.Max);
                        }
                    }
                }
            }

            Assert.NotNull(spaces);

            if (spaces is null)
            {
                return;
            }

            Assert.Single(spaces);
        }
    }
}
