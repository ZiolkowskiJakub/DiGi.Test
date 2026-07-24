using DiGi.Analytical.Building;
using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Creates the components of a box shaped space and assigns them together with the given constructions to the specified <see cref="BuildingModel"/>.
        /// <para>The bottom face becomes a <see cref="FaceFloor"/>, the top face a <see cref="SurfaceRoof"/> and the vertical faces become <see cref="SurfaceWall"/> components.</para>
        /// </summary>
        /// <param name="buildingModel">The <see cref="BuildingModel"/> the components are added to.</param>
        /// <param name="boundingBox3D">The <see cref="BoundingBox3D"/> defining the extents of the box.</param>
        /// <param name="space">The <see cref="ISpace"/> the created components are assigned to.</param>
        /// <param name="wallConstruction">The construction assigned to the created walls.</param>
        /// <param name="floorConstruction">The construction assigned to the created floor.</param>
        /// <param name="roofConstruction">The construction assigned to the created roof.</param>
        /// <returns>A <see cref="List{IComponent}"/> containing the created components, or null if the box could not be converted.</returns>
        private static List<IComponent>? AddBoxSpace(
            BuildingModel buildingModel,
            BoundingBox3D boundingBox3D,
            ISpace space,
            IWallConstruction? wallConstruction,
            IFloorConstruction? floorConstruction,
            IRoofConstruction? roofConstruction)
        {
            Polyhedron? polyhedron = Geometry.Spatial.Create.Polyhedron(boundingBox3D);
            if (polyhedron?.PolygonalFaces is not List<IPolygonalFace3D> polygonalFace3Ds)
            {
                return null;
            }

            List<IComponent> components = [];

            for (int i = 0; i < polygonalFace3Ds.Count; i++)
            {
                if (polygonalFace3Ds[i] is not PolygonalFace3D polygonalFace3D || polygonalFace3D.GetBoundingBox() is not BoundingBox3D boundingBox3D_Face)
                {
                    continue;
                }

                IComponent? component;

                if (boundingBox3D_Face.MaxZ - boundingBox3D_Face.MinZ < Core.Constants.Tolerance.Distance)
                {
                    if (boundingBox3D_Face.MinZ - boundingBox3D.MinZ < Core.Constants.Tolerance.Distance)
                    {
                        FaceFloor faceFloor = new(polygonalFace3D);
                        buildingModel.Assign(faceFloor, floorConstruction);
                        component = faceFloor;
                    }
                    else
                    {
                        SurfaceRoof surfaceRoof = new(polygonalFace3D);
                        buildingModel.Assign(surfaceRoof, roofConstruction);
                        component = surfaceRoof;
                    }
                }
                else
                {
                    SurfaceWall surfaceWall = new(polygonalFace3D);
                    buildingModel.Assign(surfaceWall, wallConstruction);
                    component = surfaceWall;
                }

                buildingModel.Assign(component, space);
                components.Add(component);
            }

            return components;
        }

        /// <summary>
        /// Tests splitting a <see cref="BuildingModel"/> holding a single box shaped space by a horizontal plane.
        /// <para>Verifies that two spaces are created with one of them keeping the identifier of the original space, that every wall is split into two components of the same type keeping the wall construction, that exactly one of the fragments of each wall keeps the identifier of the original wall, that the original floor and roof stay untouched and that a single floor is created on the cutting plane and assigned to both spaces.</para>
        /// </summary>
        [Fact]
        public void TrySplit_BuildingModel()
        {
            BuildingModel buildingModel = new();

            Space space = new(new Point3D(5, 5, 2), "Space 1");

            WallConstruction wallConstruction = new();
            FloorConstruction floorConstruction = new();
            RoofConstruction roofConstruction = new();

            List<IComponent>? components = AddBoxSpace(buildingModel, new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10)), space, wallConstruction, floorConstruction, roofConstruction);

            Assert.NotNull(components);
            Assert.Equal(6, components.Count);

            List<IWall>? walls_Source = buildingModel.GetComponents<IWall>();
            Assert.NotNull(walls_Source);
            Assert.Equal(4, walls_Source.Count);

            List<Guid> guids_Source = walls_Source.ConvertAll(x => x.Guid);

            IFloor? floor_Source = buildingModel.GetComponents<IFloor>()?.Find(x => true);
            Assert.NotNull(floor_Source);

            FloorConstruction floorConstruction_Split = new();

            Assert.True(buildingModel.TrySplit(5, 1, floorConstruction_Split));

            // Two spaces, one of them keeping the identifier of the original space
            List<ISpace>? spaces = buildingModel.GetSpaces<ISpace>();
            Assert.NotNull(spaces);
            Assert.Equal(2, spaces.Count);
            Assert.Contains(spaces, x => x.Guid == space.Guid);

            // Every wall split into two walls, each of them keeping the wall construction
            List<IWall>? walls = buildingModel.GetComponents<IWall>();
            Assert.NotNull(walls);
            Assert.Equal(8, walls.Count);

            for (int i = 0; i < walls.Count; i++)
            {
                IWall wall = walls[i];

                Assert.True(wallConstruction.Guid == buildingModel.GetWallConstruction(wall)?.Guid);

                List<ISpace>? spaces_Wall = buildingModel.GetSpaces(wall);
                Assert.NotNull(spaces_Wall);
                Assert.Single(spaces_Wall);
            }

            // Exactly one fragment of each source wall inherits the identifier of the source wall
            Assert.Equal(4, walls.FindAll(x => guids_Source.Contains(x.Guid)).Count);

            // Original roof untouched
            List<IRoof>? roofs = buildingModel.GetComponents<IRoof>();
            Assert.NotNull(roofs);
            Assert.Single(roofs);

            // Original floor plus the floor created on the cutting plane
            List<IFloor>? floors = buildingModel.GetComponents<IFloor>();
            Assert.NotNull(floors);
            Assert.Equal(2, floors.Count);
            Assert.Contains(floors, x => x.Guid == floor_Source.Guid);

            IFloor? floor_Split = floors.Find(x => x.Guid != floor_Source.Guid);
            Assert.NotNull(floor_Split);

            BoundingBox3D? boundingBox3D_Floor = floor_Split.GetBoundingBox();
            Assert.NotNull(boundingBox3D_Floor);
            Assert.True(Math.Abs(boundingBox3D_Floor.MinZ - 5) < Core.Constants.Tolerance.Distance);
            Assert.True(Math.Abs(boundingBox3D_Floor.MaxZ - 5) < Core.Constants.Tolerance.Distance);

            // Floor created on the cutting plane carries the given construction and is shared by both spaces
            Assert.True(floorConstruction_Split.Guid == buildingModel.GetFloorConstruction(floor_Split)?.Guid);

            List<ISpace>? spaces_Floor = buildingModel.GetSpaces(floor_Split);
            Assert.NotNull(spaces_Floor);
            Assert.Equal(2, spaces_Floor.Count);

            Core.xUnit.Query.SerializationCheck(buildingModel);
        }

        /// <summary>
        /// Tests the internal points of the spaces created by splitting a <see cref="BuildingModel"/>.
        /// <para>Verifies that the space containing the internal point of the original space keeps that space untouched and that the internal point of the additional space keeps the horizontal location of the original point while its elevation is measured from the cutting plane.</para>
        /// </summary>
        [Fact]
        public void TrySplit_BuildingModel_InternalPoint()
        {
            BuildingModel buildingModel = new();

            Point3D point3D_Space = new(5, 5, 2);

            Space space = new(point3D_Space, "Space 1");

            Assert.NotNull(AddBoxSpace(buildingModel, new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10)), space, null, null, null));

            Assert.True(buildingModel.TrySplit(5));

            List<ISpace>? spaces = buildingModel.GetSpaces<ISpace>();
            Assert.NotNull(spaces);
            Assert.Equal(2, spaces.Count);

            // Space containing the original internal point stays untouched
            ISpace? space_Base = spaces.Find(x => x.Guid == space.Guid);
            Assert.NotNull(space_Base);

            Point3D? point3D_Base = space_Base.Geometry;
            Assert.NotNull(point3D_Base);
            Assert.True(point3D_Space.Distance(point3D_Base) < Core.Constants.Tolerance.Distance);

            // Additional space keeps X and Y, its elevation is the elevation of the cutting plane increased by the distance measured downwards from the original point
            ISpace? space_New = spaces.Find(x => x.Guid != space.Guid);
            Assert.NotNull(space_New);
            Assert.Equal(space.Name, space_New.Name);

            Point3D? point3D_New = space_New.Geometry;
            Assert.NotNull(point3D_New);
            Assert.True(Math.Abs(point3D_New.X - 5) < Core.Constants.Tolerance.Distance);
            Assert.True(Math.Abs(point3D_New.Y - 5) < Core.Constants.Tolerance.Distance);
            Assert.True(Math.Abs(point3D_New.Z - 7) < Core.Constants.Tolerance.Distance);
        }

        /// <summary>
        /// Tests splitting a <see cref="BuildingModel"/> without a floor construction.
        /// <para>Verifies that the component created on the cutting plane is an air component shared by both spaces and that no floor is created.</para>
        /// </summary>
        [Fact]
        public void TrySplit_BuildingModel_SurfaceAir()
        {
            BuildingModel buildingModel = new();

            Space space = new(new Point3D(5, 5, 2), "Space 1");

            Assert.NotNull(AddBoxSpace(buildingModel, new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10)), space, null, null, null));

            Assert.True(buildingModel.TrySplit(5));

            // Only the original floor exists
            List<IFloor>? floors = buildingModel.GetComponents<IFloor>();
            Assert.NotNull(floors);
            Assert.Single(floors);

            List<IAir>? airs = buildingModel.GetComponents<IAir>();
            Assert.NotNull(airs);
            Assert.Single(airs);

            BoundingBox3D? boundingBox3D_Air = airs[0].GetBoundingBox();
            Assert.NotNull(boundingBox3D_Air);
            Assert.True(Math.Abs(boundingBox3D_Air.MinZ - 5) < Core.Constants.Tolerance.Distance);
            Assert.True(Math.Abs(boundingBox3D_Air.MaxZ - 5) < Core.Constants.Tolerance.Distance);

            List<ISpace>? spaces_Air = buildingModel.GetSpaces(airs[0]);
            Assert.NotNull(spaces_Air);
            Assert.Equal(2, spaces_Air.Count);
        }

        /// <summary>
        /// Tests the minimal height of the part of a space above the cutting plane.
        /// <para>Verifies that a space with a part above the cutting plane smaller than the given minimal height is not split and that the same cut succeeds for a smaller minimal height.</para>
        /// </summary>
        [Fact]
        public void TrySplit_BuildingModel_MinHeight()
        {
            BuildingModel buildingModel = new();

            Space space = new(new Point3D(5, 5, 2), "Space 1");

            Assert.NotNull(AddBoxSpace(buildingModel, new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10)), space, null, null, null));

            // Part above the cutting plane is 0.5 high, less than the minimal height
            Assert.False(buildingModel.TrySplit(9.5, 1));

            List<ISpace>? spaces = buildingModel.GetSpaces<ISpace>();
            Assert.NotNull(spaces);
            Assert.Single(spaces);

            List<IComponent>? components = buildingModel.GetComponents<IComponent>();
            Assert.NotNull(components);
            Assert.Equal(6, components.Count);

            // The same cut passes for a smaller minimal height
            Assert.True(buildingModel.TrySplit(9.5, 0.25));

            spaces = buildingModel.GetSpaces<ISpace>();
            Assert.NotNull(spaces);
            Assert.Equal(2, spaces.Count);
        }

        /// <summary>
        /// Tests splitting a <see cref="BuildingModel"/> holding two spaces sharing a single wall component.
        /// <para>Verifies that the shared wall is split once and that both of its fragments stay assigned to two spaces.</para>
        /// </summary>
        [Fact]
        public void TrySplit_BuildingModel_SharedComponent()
        {
            BuildingModel buildingModel = new();

            Space space_1 = new(new Point3D(5, 5, 2), "Space 1");
            Space space_2 = new(new Point3D(15, 5, 2), "Space 2");

            List<IComponent>? components_1 = AddBoxSpace(buildingModel, new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10)), space_1, null, null, null);
            List<IComponent>? components_2 = AddBoxSpace(buildingModel, new BoundingBox3D(new Point3D(10, 0, 0), new Point3D(20, 10, 10)), space_2, null, null, null);

            Assert.NotNull(components_1);
            Assert.NotNull(components_2);

            bool OnSharedPlane(IComponent component)
            {
                return component.GetBoundingBox() is BoundingBox3D boundingBox3D && Math.Abs(boundingBox3D.MinX - 10) < Core.Constants.Tolerance.Distance && Math.Abs(boundingBox3D.MaxX - 10) < Core.Constants.Tolerance.Distance;
            }

            IComponent? component_Shared = components_1.Find(OnSharedPlane);
            IComponent? component_Duplicate = components_2.Find(OnSharedPlane);

            Assert.NotNull(component_Shared);
            Assert.NotNull(component_Duplicate);

            // Replace the two coincident walls by a single wall bounding both spaces
            Assert.True(buildingModel.Remove(component_Duplicate));
            Assert.True(buildingModel.Assign(component_Shared, space_1, space_2));

            Assert.True(buildingModel.TrySplit(5));

            List<ISpace>? spaces = buildingModel.GetSpaces<ISpace>();
            Assert.NotNull(spaces);
            Assert.Equal(4, spaces.Count);

            List<IWall>? walls = buildingModel.GetComponents<IWall>();
            Assert.NotNull(walls);

            List<IWall> walls_Shared = walls.FindAll(x => OnSharedPlane(x));

            // The shared wall is split once, not once per space
            Assert.Equal(2, walls_Shared.Count);
            Assert.Contains(walls_Shared, x => x.Guid == component_Shared.Guid);

            for (int i = 0; i < walls_Shared.Count; i++)
            {
                List<ISpace>? spaces_Wall = buildingModel.GetSpaces(walls_Shared[i]);
                Assert.NotNull(spaces_Wall);
                Assert.Equal(2, spaces_Wall.Count);
            }
        }

        /// <summary>
        /// Tests splitting only the spaces given on the input of the method.
        /// <para>Verifies that the spaces which were not given stay untouched together with their components.</para>
        /// </summary>
        [Fact]
        public void TrySplit_BuildingModel_Spaces()
        {
            BuildingModel buildingModel = new();

            Space space_1 = new(new Point3D(5, 5, 2), "Space 1");
            Space space_2 = new(new Point3D(35, 5, 2), "Space 2");

            Assert.NotNull(AddBoxSpace(buildingModel, new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10)), space_1, null, null, null));
            Assert.NotNull(AddBoxSpace(buildingModel, new BoundingBox3D(new Point3D(30, 0, 0), new Point3D(40, 10, 10)), space_2, null, null, null));

            Assert.True(buildingModel.TrySplit(5, 1, null, [space_1]));

            List<ISpace>? spaces = buildingModel.GetSpaces<ISpace>();
            Assert.NotNull(spaces);
            Assert.Equal(3, spaces.Count);

            // Space which was not given on the input keeps its six components
            List<IComponent>? components_2 = buildingModel.GetComponents<IComponent>(space_2);
            Assert.NotNull(components_2);
            Assert.Equal(6, components_2.Count);
        }

        /// <summary>
        /// Tests splitting a <see cref="BuildingModel"/> holding a single box shaped space by several horizontal planes.
        /// <para>Verifies that three spaces are created with one of them keeping the identifier of the original space, that every wall is split into three components keeping the wall construction, that the original floor and roof stay untouched and that a floor carrying the given construction is created on each cutting plane and shared by two spaces.</para>
        /// </summary>
        [Fact]
        public void TrySplit_BuildingModel_Elevations()
        {
            BuildingModel buildingModel = new();

            Space space = new(new Point3D(5, 5, 2), "Space 1");

            WallConstruction wallConstruction = new();
            FloorConstruction floorConstruction = new();
            RoofConstruction roofConstruction = new();

            Assert.NotNull(AddBoxSpace(buildingModel, new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 12)), space, wallConstruction, floorConstruction, roofConstruction));

            IFloor? floor_Source = buildingModel.GetComponents<IFloor>()?.Find(x => true);
            Assert.NotNull(floor_Source);

            FloorConstruction floorConstruction_Split = new();

            Assert.True(buildingModel.TrySplit([4, 8], 1, floorConstruction_Split));

            // Three spaces, one of them keeping the identifier of the original space
            List<ISpace>? spaces = buildingModel.GetSpaces<ISpace>();
            Assert.NotNull(spaces);
            Assert.Equal(3, spaces.Count);
            Assert.Contains(spaces, x => x.Guid == space.Guid);

            // Every wall split into three walls, each of them keeping the wall construction
            List<IWall>? walls = buildingModel.GetComponents<IWall>();
            Assert.NotNull(walls);
            Assert.Equal(12, walls.Count);

            for (int i = 0; i < walls.Count; i++)
            {
                Assert.True(wallConstruction.Guid == buildingModel.GetWallConstruction(walls[i])?.Guid);
            }

            // Original roof untouched
            List<IRoof>? roofs = buildingModel.GetComponents<IRoof>();
            Assert.NotNull(roofs);
            Assert.Single(roofs);

            // Original floor plus a floor created on each cutting plane
            List<IFloor>? floors = buildingModel.GetComponents<IFloor>();
            Assert.NotNull(floors);
            Assert.Equal(3, floors.Count);
            Assert.Contains(floors, x => x.Guid == floor_Source.Guid);

            List<IFloor> floors_Split = floors.FindAll(x => x.Guid != floor_Source.Guid);
            Assert.Equal(2, floors_Split.Count);

            List<double> elevations = [];

            for (int i = 0; i < floors_Split.Count; i++)
            {
                IFloor floor_Split = floors_Split[i];

                BoundingBox3D? boundingBox3D_Floor = floor_Split.GetBoundingBox();
                Assert.NotNull(boundingBox3D_Floor);
                Assert.True(Math.Abs(boundingBox3D_Floor.MaxZ - boundingBox3D_Floor.MinZ) < Core.Constants.Tolerance.Distance);

                elevations.Add(boundingBox3D_Floor.MinZ);

                // Floor created on the cutting plane carries the given construction and is shared by two spaces
                Assert.True(floorConstruction_Split.Guid == buildingModel.GetFloorConstruction(floor_Split)?.Guid);

                List<ISpace>? spaces_Floor = buildingModel.GetSpaces(floor_Split);
                Assert.NotNull(spaces_Floor);
                Assert.Equal(2, spaces_Floor.Count);
            }

            Assert.Contains(elevations, x => Math.Abs(x - 4) < Core.Constants.Tolerance.Distance);
            Assert.Contains(elevations, x => Math.Abs(x - 8) < Core.Constants.Tolerance.Distance);

            Core.xUnit.Query.SerializationCheck(buildingModel);
        }

        /// <summary>
        /// Tests the minimal height taken between two consecutive cutting planes.
        /// <para>Verifies that an elevation closer to the previous one than the given minimal height is dropped and that the same pair of elevations produces three spaces for a smaller minimal height.</para>
        /// </summary>
        [Fact]
        public void TrySplit_BuildingModel_Elevations_MinHeight()
        {
            BuildingModel buildingModel = new();

            Space space = new(new Point3D(5, 5, 2), "Space 1");

            Assert.NotNull(AddBoxSpace(buildingModel, new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10)), space, null, null, null));

            // The second cut is 0.5 above the first one, less than the minimal height
            Assert.True(buildingModel.TrySplit([3, 3.5], 1));

            List<ISpace>? spaces = buildingModel.GetSpaces<ISpace>();
            Assert.NotNull(spaces);
            Assert.Equal(2, spaces.Count);

            BuildingModel buildingModel_Temp = new();

            Space space_Temp = new(new Point3D(5, 5, 2), "Space 1");

            Assert.NotNull(AddBoxSpace(buildingModel_Temp, new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10)), space_Temp, null, null, null));

            // Both cuts pass for a smaller minimal height
            Assert.True(buildingModel_Temp.TrySplit([3, 3.5], 0.25));

            List<ISpace>? spaces_Temp = buildingModel_Temp.GetSpaces<ISpace>();
            Assert.NotNull(spaces_Temp);
            Assert.Equal(3, spaces_Temp.Count);
        }

        /// <summary>
        /// Tests the order of the elevations given on the input of the split.
        /// <para>Verifies that elevations given in a descending order produce the same spaces and the same cutting planes as the same elevations given in an ascending order.</para>
        /// </summary>
        [Fact]
        public void TrySplit_BuildingModel_Elevations_Order()
        {
            List<double> Split(IEnumerable<double> elevations)
            {
                BuildingModel buildingModel = new();

                Space space = new(new Point3D(5, 5, 2), "Space 1");

                Assert.NotNull(AddBoxSpace(buildingModel, new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 12)), space, null, null, null));

                Assert.True(buildingModel.TrySplit(elevations, 1, new FloorConstruction()));

                List<ISpace>? spaces = buildingModel.GetSpaces<ISpace>();
                Assert.NotNull(spaces);
                Assert.Equal(3, spaces.Count);

                List<IFloor>? floors = buildingModel.GetComponents<IFloor>();
                Assert.NotNull(floors);

                List<double> result = [];
                for (int i = 0; i < floors.Count; i++)
                {
                    if (floors[i].GetBoundingBox() is BoundingBox3D boundingBox3D && Math.Abs(boundingBox3D.MaxZ - boundingBox3D.MinZ) < Core.Constants.Tolerance.Distance)
                    {
                        result.Add(boundingBox3D.MinZ);
                    }
                }

                result.Sort();
                return result;
            }

            List<double> elevations_Ascending = Split([4, 8]);
            List<double> elevations_Descending = Split([8, 4]);

            Assert.Equal(elevations_Ascending.Count, elevations_Descending.Count);

            for (int i = 0; i < elevations_Ascending.Count; i++)
            {
                Assert.True(Math.Abs(elevations_Ascending[i] - elevations_Descending[i]) < Core.Constants.Tolerance.Distance);
            }
        }

        /// <summary>
        /// Tests splitting only the spaces given on the input of the multiple elevations overload.
        /// <para>Verifies that the spaces created by an earlier cut are split by the following cuts and that the spaces which were not given stay untouched together with their components.</para>
        /// </summary>
        [Fact]
        public void TrySplit_BuildingModel_Elevations_Spaces()
        {
            BuildingModel buildingModel = new();

            Space space_1 = new(new Point3D(5, 5, 2), "Space 1");
            Space space_2 = new(new Point3D(35, 5, 2), "Space 2");

            Assert.NotNull(AddBoxSpace(buildingModel, new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 12)), space_1, null, null, null));
            Assert.NotNull(AddBoxSpace(buildingModel, new BoundingBox3D(new Point3D(30, 0, 0), new Point3D(40, 10, 12)), space_2, null, null, null));

            Assert.True(buildingModel.TrySplit([4, 8], 1, null, [space_1]));

            // Three parts of the given space plus the space which was not given
            List<ISpace>? spaces = buildingModel.GetSpaces<ISpace>();
            Assert.NotNull(spaces);
            Assert.Equal(4, spaces.Count);

            // Space which was not given on the input keeps its six components
            List<IComponent>? components_2 = buildingModel.GetComponents<IComponent>(space_2);
            Assert.NotNull(components_2);
            Assert.Equal(6, components_2.Count);
        }

        /// <summary>
        /// Tests the behavior of the multiple elevations overload for inputs which cannot be split.
        /// <para>Verifies that a null model, an empty model, an empty collection of elevations and elevations outside the extents of the model return false and leave the components of the model untouched.</para>
        /// </summary>
        [Fact]
        public void TrySplit_BuildingModel_Elevations_NullInputs()
        {
            Assert.False(Modify.TrySplit(null, [5]));

            BuildingModel buildingModel_Empty = new();
            Assert.False(buildingModel_Empty.TrySplit([5]));

            BuildingModel buildingModel = new();

            Space space = new(new Point3D(5, 5, 2), "Space 1");

            Assert.NotNull(AddBoxSpace(buildingModel, new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10)), space, null, null, null));

            Assert.False(buildingModel.TrySplit([]));

            // Cutting planes above and below the model
            Assert.False(buildingModel.TrySplit([20, 25]));
            Assert.False(buildingModel.TrySplit([-10, -5]));

            List<IComponent>? components = buildingModel.GetComponents<IComponent>();
            Assert.NotNull(components);
            Assert.Equal(6, components.Count);
        }

        /// <summary>
        /// Tests the behavior of the split for inputs which cannot be split.
        /// <para>Verifies that a null model, an empty model and elevations outside the extents of the model return false.</para>
        /// </summary>
        [Fact]
        public void TrySplit_BuildingModel_NullInputs()
        {
            Assert.False(Modify.TrySplit(null, 5));

            BuildingModel buildingModel_Empty = new();
            Assert.False(buildingModel_Empty.TrySplit(5));

            BuildingModel buildingModel = new();

            Space space = new(new Point3D(5, 5, 2), "Space 1");

            Assert.NotNull(AddBoxSpace(buildingModel, new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10)), space, null, null, null));

            // Cutting plane above and below the model
            Assert.False(buildingModel.TrySplit(20));
            Assert.False(buildingModel.TrySplit(-5));

            List<IComponent>? components = buildingModel.GetComponents<IComponent>();
            Assert.NotNull(components);
            Assert.Equal(6, components.Count);
        }
    }
}
