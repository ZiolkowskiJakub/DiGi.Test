using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Enums;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Creates a vertical square <see cref="PolygonalFace3D"/> used as the geometry of the air components under test.
        /// </summary>
        /// <returns>The created <see cref="PolygonalFace3D"/>.</returns>
        private static PolygonalFace3D BuildingModelConvertAir_PolygonalFace3D()
        {
            Plane plane = new(new Point3D(0, 0, 0), new Vector3D(0, 1, 0));

            PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(plane,
            [
                new Geometry.Planar.Classes.Point2D(0, 0),
                new Geometry.Planar.Classes.Point2D(10, 0),
                new Geometry.Planar.Classes.Point2D(10, 5),
                new Geometry.Planar.Classes.Point2D(0, 5)
            ]);

            Assert.NotNull(polygonalFace3D);

            return polygonalFace3D;
        }

        /// <summary>
        /// Tests that <see cref="BuildingModel.ConvertAir(IAir, PhysicalComponentType, out IPhysicalComponent)"/> replaces an air bound by two spaces with a <see cref="SurfaceWall"/> carrying the identifier of the air and both space assignments.
        /// </summary>
        [Fact]
        public void BuildingModelConvertAir_Wall()
        {
            PolygonalFace3D polygonalFace3D = BuildingModelConvertAir_PolygonalFace3D();

            SurfaceAir surfaceAir = new(polygonalFace3D);

            Space space_1 = new(new Point3D(0, -5, 2.5), "Space 1");
            Space space_2 = new(new Point3D(0, 5, 2.5), "Space 2");

            BuildingModel buildingModel = new();
            Assert.True(buildingModel.Assign(surfaceAir, space_1, space_2));

            Assert.True(buildingModel.ConvertAir(surfaceAir, PhysicalComponentType.Wall, out IPhysicalComponent? physicalComponent));

            Assert.NotNull(physicalComponent);
            Assert.IsType<SurfaceWall>(physicalComponent);
            Assert.Equal(surfaceAir.Guid, physicalComponent.Guid);

            List<IAir>? airs = buildingModel.GetComponents<IAir>();
            Assert.NotNull(airs);
            Assert.Empty(airs);

            List<ISurfaceWall>? surfaceWalls = buildingModel.GetComponents<ISurfaceWall>();
            Assert.NotNull(surfaceWalls);
            Assert.Single(surfaceWalls);
            Assert.NotNull(surfaceWalls[0].Geometry);

            List<ISpace>? spaces = buildingModel.GetSpaces(physicalComponent);
            Assert.NotNull(spaces);
            Assert.Equal(2, spaces.Count);
            Assert.Contains(spaces, x => x.Guid == space_1.Guid);
            Assert.Contains(spaces, x => x.Guid == space_2.Guid);

            Core.xUnit.Query.SerializationCheck(buildingModel);
        }

        /// <summary>
        /// Tests that <see cref="BuildingModel.ConvertAir(IAir, PhysicalComponentType, out IPhysicalComponent)"/> replaces an air bound by a single space with a <see cref="FaceFloor"/> that stays bound by that one space only.
        /// </summary>
        [Fact]
        public void BuildingModelConvertAir_Floor()
        {
            PolygonalFace3D polygonalFace3D = BuildingModelConvertAir_PolygonalFace3D();

            SurfaceAir surfaceAir = new(polygonalFace3D);

            Space space = new(new Point3D(0, -5, 2.5), "Space 1");

            BuildingModel buildingModel = new();
            Assert.True(buildingModel.Assign(surfaceAir, space));

            Assert.True(buildingModel.ConvertAir(surfaceAir, PhysicalComponentType.Floor, out IPhysicalComponent? physicalComponent));

            Assert.NotNull(physicalComponent);
            Assert.IsType<FaceFloor>(physicalComponent);
            Assert.Equal(surfaceAir.Guid, physicalComponent.Guid);

            List<IFloor>? floors = buildingModel.GetComponents<IFloor>();
            Assert.NotNull(floors);
            Assert.Single(floors);

            List<ISpace>? spaces = buildingModel.GetSpaces(physicalComponent);
            Assert.NotNull(spaces);
            Assert.Single(spaces);
            Assert.Equal(space.Guid, spaces[0].Guid);

            Core.xUnit.Query.SerializationCheck(buildingModel);
        }

        /// <summary>
        /// Tests that <see cref="BuildingModel.ConvertAir(IAir, PhysicalComponentType, out IPhysicalComponent)"/> replaces an air that is not bound by any space with a <see cref="SurfaceRoof"/> and leaves it unassigned.
        /// </summary>
        [Fact]
        public void BuildingModelConvertAir_Roof()
        {
            PolygonalFace3D polygonalFace3D = BuildingModelConvertAir_PolygonalFace3D();

            SurfaceAir surfaceAir = new(polygonalFace3D);

            BuildingModel buildingModel = new();
            Assert.True(buildingModel.Update(surfaceAir));

            Assert.True(buildingModel.ConvertAir(surfaceAir, PhysicalComponentType.Roof, out IPhysicalComponent? physicalComponent));

            Assert.NotNull(physicalComponent);
            Assert.IsType<SurfaceRoof>(physicalComponent);
            Assert.Equal(surfaceAir.Guid, physicalComponent.Guid);

            List<IRoof>? roofs = buildingModel.GetComponents<IRoof>();
            Assert.NotNull(roofs);
            Assert.Single(roofs);

            List<ISpace>? spaces = buildingModel.GetSpaces(physicalComponent);
            Assert.True(spaces == null || spaces.Count == 0);

            Core.xUnit.Query.SerializationCheck(buildingModel);
        }

        /// <summary>
        /// Tests that <see cref="BuildingModel.ConvertAir(IAir, PhysicalComponentType, out IPhysicalComponent)"/> leaves the model untouched when the air is null or the requested type is <see cref="PhysicalComponentType.Undefined"/>.
        /// </summary>
        [Fact]
        public void BuildingModelConvertAir_Invalid()
        {
            PolygonalFace3D polygonalFace3D = BuildingModelConvertAir_PolygonalFace3D();

            SurfaceAir surfaceAir = new(polygonalFace3D);

            Space space = new(new Point3D(0, -5, 2.5), "Space 1");

            BuildingModel buildingModel = new();
            Assert.True(buildingModel.Assign(surfaceAir, space));

            Assert.False(buildingModel.ConvertAir(null, PhysicalComponentType.Wall, out IPhysicalComponent? physicalComponent));
            Assert.Null(physicalComponent);

            Assert.False(buildingModel.ConvertAir(surfaceAir, PhysicalComponentType.Undefined, out physicalComponent));
            Assert.Null(physicalComponent);

            List<IAir>? airs = buildingModel.GetComponents<IAir>();
            Assert.NotNull(airs);
            Assert.Single(airs);

            List<IPhysicalComponent>? physicalComponents = buildingModel.GetComponents<IPhysicalComponent>();
            Assert.NotNull(physicalComponents);
            Assert.Empty(physicalComponents);

            List<ISpace>? spaces = buildingModel.GetSpaces(surfaceAir);
            Assert.NotNull(spaces);
            Assert.Single(spaces);
        }
    }
}
