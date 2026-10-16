using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Enums;
using DiGi.Core.Interfaces;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.GLTF.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <c>DiGi.GLTF.Analytical.Convert.ToGLTF_GLTFNodes</c> (<c>Component</c> detail level) exports the openings hosted on a component: a wall hosting a window and a door yields exactly three nodes, the window node carries opacity 0.5, the door and the wall carry opacity 1, and each opening node's reference extends the wall node's reference.
        /// <para>On the unmodified converter the openings are never exported (an <c>IOpening</c> is not an <c>IComponent</c>), so this fact fails with one node instead of three.</para>
        /// </summary>
        [Fact]
        public void ToGLTF_GLTFNodes_BuildingModel_Openings()
        {
            SurfaceWall surfaceWall = ToGLTF_GLTFNodes_Fixture_SurfaceWall();

            Window window = new(ToGLTF_GLTFNodes_Fixture_Plane(2, 0, 0.5));
            WindowConstruction windowConstruction = ToGLTF_GLTFNodes_Fixture_WindowConstruction(1.2, 1.5);

            Door door = new(ToGLTF_GLTFNodes_Fixture_Plane(5, 0, 0));
            DoorConstruction doorConstruction = new("Door Construction", ToGLTF_GLTFNodes_Fixture_Rectangle(0, 0, 1, 2.1));

            BuildingModel buildingModel = new();
            Assert.True(buildingModel.Assign(surfaceWall, window, windowConstruction));
            Assert.True(buildingModel.Assign(surfaceWall, door, doorConstruction));

            IReference? reference = Core.Create.UniqueReference(buildingModel);

            List<DiGi.GLTF.Classes.GLTFNode>? gLTFNodes = DiGi.GLTF.Analytical.Convert.ToGLTF_GLTFNodes(buildingModel, reference, Core.Constants.Tolerance.Distance, BuildingModelDetailLevel.Component);
            Assert.NotNull(gLTFNodes);
            Assert.Equal(3, gLTFNodes!.Count);

            DiGi.GLTF.Classes.GLTFNode? gLTFNode_Wall = gLTFNodes.Find(x => x.Reference is not null && x.Reference.Contains(surfaceWall.Guid.ToString("N")));
            DiGi.GLTF.Classes.GLTFNode? gLTFNode_Window = gLTFNodes.Find(x => x.Reference is not null && x.Reference.Contains(window.Guid.ToString("N")));
            DiGi.GLTF.Classes.GLTFNode? gLTFNode_Door = gLTFNodes.Find(x => x.Reference is not null && x.Reference.Contains(door.Guid.ToString("N")));
            Assert.NotNull(gLTFNode_Wall);
            Assert.NotNull(gLTFNode_Window);
            Assert.NotNull(gLTFNode_Door);

            Assert.Equal(1, gLTFNode_Wall!.Opacity);
            Assert.Equal(0.5, gLTFNode_Window!.Opacity);
            Assert.Equal(1, gLTFNode_Door!.Opacity);

            // Each opening node's reference extends the wall node's reference.
            Assert.Contains(gLTFNode_Wall.Reference!, gLTFNode_Window.Reference!);
            Assert.Contains(gLTFNode_Wall.Reference!, gLTFNode_Door.Reference!);
        }

        /// <summary>
        /// Tests that <c>DiGi.GLTF.Analytical.Convert.ToGLTF_GLTFNodes</c> (<c>Component</c> detail level) skips an opening that has no construction without throwing: a wall hosting a construction-less window yields exactly one node (the wall).
        /// </summary>
        [Fact]
        public void ToGLTF_GLTFNodes_OpeningWithoutConstruction_Skipped()
        {
            SurfaceWall surfaceWall = ToGLTF_GLTFNodes_Fixture_SurfaceWall();
            Window window = new(ToGLTF_GLTFNodes_Fixture_Plane(2, 0, 0.5));

            BuildingModel buildingModel = new();
            Assert.True(buildingModel.Assign(surfaceWall, window));

            IReference? reference = Core.Create.UniqueReference(buildingModel);

            List<DiGi.GLTF.Classes.GLTFNode>? gLTFNodes = DiGi.GLTF.Analytical.Convert.ToGLTF_GLTFNodes(buildingModel, reference, Core.Constants.Tolerance.Distance, BuildingModelDetailLevel.Component);
            Assert.NotNull(gLTFNodes);
            Assert.Single(gLTFNodes!);
            Assert.Equal(1, gLTFNodes[0].Opacity);
            Assert.NotNull(gLTFNodes[0].Reference);
            Assert.Contains(surfaceWall.Guid.ToString("N"), gLTFNodes[0].Reference!);
        }

        /// <summary>
        /// Creates the vertical XZ plane used by the fixtures - axis X along world X, axis Y along world Z.
        /// </summary>
        /// <param name="x">The X coordinate of the plane origin.</param>
        /// <param name="y">The Y coordinate of the plane origin (the out-of-plane offset).</param>
        /// <param name="z">The Z coordinate of the plane origin.</param>
        /// <returns>The created <see cref="Plane"/>.</returns>
        private static Plane ToGLTF_GLTFNodes_Fixture_Plane(double x, double y, double z)
        {
            return new Plane(new Point3D(x, y, z), new Vector3D(1, 0, 0), new Vector3D(0, 0, 1));
        }

        /// <summary>
        /// Creates an axis-aligned rectangular <see cref="PolygonalFace2D"/>.
        /// </summary>
        /// <param name="minX">The minimum X coordinate.</param>
        /// <param name="minY">The minimum Y coordinate.</param>
        /// <param name="maxX">The maximum X coordinate.</param>
        /// <param name="maxY">The maximum Y coordinate.</param>
        /// <returns>The created <see cref="PolygonalFace2D"/>.</returns>
        private static PolygonalFace2D ToGLTF_GLTFNodes_Fixture_Rectangle(double minX, double minY, double maxX, double maxY)
        {
            Polygon2D polygon2D = new([new Point2D(minX, minY), new Point2D(maxX, minY), new Point2D(maxX, maxY), new Point2D(minX, maxY)]);
            PolygonalFace2D? polygonalFace2D = DiGi.Geometry.Planar.Create.PolygonalFace2D(polygon2D, null);
            Assert.NotNull(polygonalFace2D);

            return polygonalFace2D;
        }

        /// <summary>
        /// Creates a 10 x 3 <see cref="SurfaceWall"/> on the vertical XZ plane through the world origin.
        /// </summary>
        /// <returns>The created <see cref="SurfaceWall"/>.</returns>
        private static SurfaceWall ToGLTF_GLTFNodes_Fixture_SurfaceWall()
        {
            PolygonalFace3D? polygonalFace3D = ToGLTF_GLTFNodes_Fixture_Plane(0, 0, 0).Convert(ToGLTF_GLTFNodes_Fixture_Rectangle(0, 0, 10, 3));
            Assert.NotNull(polygonalFace3D);

            SurfaceWall? surfaceWall = DiGi.Analytical.Building.Create.SurfaceWall(polygonalFace3D);
            Assert.NotNull(surfaceWall);

            return surfaceWall;
        }

        /// <summary>
        /// Creates a <see cref="WindowConstruction"/> with a rectangular outline anchored at the plane origin.
        /// </summary>
        /// <param name="width">The width of the outline.</param>
        /// <param name="height">The height of the outline.</param>
        /// <returns>The created <see cref="WindowConstruction"/>.</returns>
        private static WindowConstruction ToGLTF_GLTFNodes_Fixture_WindowConstruction(double width, double height)
        {
            return new WindowConstruction("Window Construction", ToGLTF_GLTFNodes_Fixture_Rectangle(0, 0, width, height));
        }
    }
}
