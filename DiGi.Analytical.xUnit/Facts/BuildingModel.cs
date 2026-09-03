using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.HVAC;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Analytical.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
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

        /// <summary>
        /// Tests that <see cref="BuildingModel.GetObject{TBuildingGuidObject}(IBuildingRelation?)"/> and <see cref="BuildingModel.GetObjects{TBuildingGuidObject}(IBuildingRelation?)"/> retrieve the associated object(s) when referenced by a building relation.
        /// </summary>
        [Fact]
        public void BuildingModel_GetObject()
        {
            Plane? plane = Geometry.Spatial.Create.Plane(0.0);
            PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(plane,
            [
                new Geometry.Planar.Classes.Point2D(0, 0),
                new Geometry.Planar.Classes.Point2D(10, 0),
                new Geometry.Planar.Classes.Point2D(10, 10),
                new Geometry.Planar.Classes.Point2D(0, 10)
            ]);

            Assert.NotNull(polygonalFace3D);

            FaceFloor faceFloor = new(polygonalFace3D);
            Space space_1 = new(new Point3D(0, 0, 0), "Space 1");
            Space space_2 = new(new Point3D(0, 0, 3), "Space 2");

            BuildingModel buildingModel = new();
            buildingModel.Assign(faceFloor, space_1, space_2);

            SpaceRelation? spaceRelation = buildingModel.GetRelation<SpaceRelation>(faceFloor);
            Assert.NotNull(spaceRelation);

            ISpace? retrievedSpace = buildingModel.GetObject<ISpace>(spaceRelation);
            Assert.NotNull(retrievedSpace);
            Assert.Equal(space_1.Guid, retrievedSpace.Guid);
            Assert.NotSame(space_1, retrievedSpace);

            IFloor? retrievedFloor = buildingModel.GetObject<IFloor>(spaceRelation);
            Assert.NotNull(retrievedFloor);
            Assert.Equal(faceFloor.Guid, retrievedFloor.Guid);
            Assert.NotSame(faceFloor, retrievedFloor);

            IZone? retrievedZone = buildingModel.GetObject<IZone>(spaceRelation);
            Assert.Null(retrievedZone);

            Assert.Null(buildingModel.GetObject<ISpace>((IBuildingRelation?)null));

            List<ISpace>? retrievedSpaces = buildingModel.GetObjects<ISpace>(spaceRelation);
            Assert.NotNull(retrievedSpaces);
            Assert.Equal(2, retrievedSpaces.Count);
            Assert.Equal(space_1.Guid, retrievedSpaces[0].Guid);
            Assert.Equal(space_2.Guid, retrievedSpaces[1].Guid);
            Assert.NotSame(space_1, retrievedSpaces[0]);
            Assert.NotSame(space_2, retrievedSpaces[1]);

            Assert.Null(buildingModel.GetObjects<ISpace>((IBuildingRelation?)null));
        }
    }
}