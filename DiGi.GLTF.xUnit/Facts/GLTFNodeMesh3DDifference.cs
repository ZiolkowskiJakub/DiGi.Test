using DiGi.Core.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.GLTF.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests rebuilding the node holding the ground surface of a scene around a surface with a building outline cut out of it.
        /// <para>This is the shape of the scene assembly step that stops the ground from running through the interiors of the buildings standing on it: the surface changes, everything the node is known by in the scene - its name, its reference, its colour, its opacity and its properties - does not.</para>
        /// </summary>
        [Fact]
        public void GLTFNode_Mesh3DDifference()
        {
            // A 20 x 20 surface of four square cells, each split into two triangles.
            Mesh3D mesh3D = new(
                [
                    new Point3D(0, 0, 0), new Point3D(10, 0, 0), new Point3D(20, 0, 0),
                    new Point3D(0, 10, 0), new Point3D(10, 10, 0), new Point3D(20, 10, 0),
                    new Point3D(0, 20, 0), new Point3D(10, 20, 0), new Point3D(20, 20, 0)
                ],
                [
                    new int[] { 0, 1, 4 }, new int[] { 0, 4, 3 },
                    new int[] { 1, 2, 5 }, new int[] { 1, 5, 4 },
                    new int[] { 3, 4, 7 }, new int[] { 3, 7, 6 },
                    new int[] { 4, 5, 8 }, new int[] { 4, 8, 7 }
                ]);

            Color color = new(byte.MaxValue, 138, 128, 102);

            Classes.GLTFNode gLTFNode = new("Terrain", "reference_Terrain", mesh3D, color, 1, "{\"Source\":\"NMT\"}");

            PolygonalFace2D? polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D(new Polygon2D([new Point2D(12, 2), new Point2D(18, 2), new Point2D(18, 8), new Point2D(12, 8)]));
            Assert.NotNull(polygonalFace2D);

            Mesh3D? mesh3D_Cut = mesh3D.Difference([polygonalFace2D]);
            Assert.NotNull(mesh3D_Cut);
            Assert.Equal(400 - 36, mesh3D_Cut.GetArea(), 6);

            Classes.GLTFNode gLTFNode_Cut = new(gLTFNode.Name, gLTFNode.Reference, mesh3D_Cut, gLTFNode.Color, gLTFNode.Opacity, gLTFNode.Properties);
            Assert.Equal(gLTFNode.Name, gLTFNode_Cut.Name);
            Assert.Equal(gLTFNode.Reference, gLTFNode_Cut.Reference);
            Assert.Equal(gLTFNode.Opacity, gLTFNode_Cut.Opacity);
            Assert.Equal(gLTFNode.Properties, gLTFNode_Cut.Properties);
            Assert.NotNull(gLTFNode_Cut.Color);

            Mesh3D? mesh3D_Node = gLTFNode_Cut.Mesh3D;
            Assert.NotNull(mesh3D_Node);
            Assert.True(mesh3D_Node.GetArea() < mesh3D.GetArea(), "The node kept the surface the building was supposed to be cut out of.");

            Core.xUnit.Query.SerializationCheck(gLTFNode_Cut);
        }
    }
}
