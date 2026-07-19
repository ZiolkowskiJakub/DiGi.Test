using DiGi.Core.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.GLTF.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the <see cref="Classes.GLTFNode"/> primary constructor.
        /// <para>Verifies the constructor-exposed properties for a realistic instance and an instance with null optional values, then checks the JSON serialization round-trip and the copy-constructor clone path for both.</para>
        /// </summary>
        [Fact]
        public void GLTFNode()
        {
            Mesh3D mesh3D = new(
                [new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0), new Point3D(0, 0, 1)],
                [new int[] { 0, 2, 1 }, new int[] { 0, 1, 3 }, new int[] { 1, 2, 3 }, new int[] { 0, 3, 2 }]);

            Color color = new(byte.MaxValue, 200, 120, 80);

            Classes.GLTFNode gLTFNode = new("Building 1", "reference_1", mesh3D, color, 0.75, "{\"Storeys\":3}");
            Assert.Equal("Building 1", gLTFNode.Name);
            Assert.Equal("reference_1", gLTFNode.Reference);
            Assert.Equal(0.75, gLTFNode.Opacity);
            Assert.Equal("{\"Storeys\":3}", gLTFNode.Properties);

            Mesh3D? mesh3D_Property = gLTFNode.Mesh3D;
            Assert.NotNull(mesh3D_Property);
            Assert.Equal(mesh3D.TrianglesCount, mesh3D_Property!.TrianglesCount);
            Assert.NotNull(gLTFNode.Color);

            Core.xUnit.Query.SerializationCheck(gLTFNode);

            Classes.GLTFNode gLTFNode_Minimal = new("Component", null, mesh3D, null, 1, null);
            Assert.Null(gLTFNode_Minimal.Reference);
            Assert.Null(gLTFNode_Minimal.Properties);

            Core.xUnit.Query.SerializationCheck(gLTFNode_Minimal);
        }
    }
}
