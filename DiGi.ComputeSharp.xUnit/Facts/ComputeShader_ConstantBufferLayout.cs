using ComputeSharp;
using System.Reflection;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Contains unit tests for the constant buffer layout of every compute shader in DiGi.ComputeSharp.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Verifies that no compute shader in DiGi.ComputeSharp combines an odd root constant count with an odd number of
        /// bound resources, and pins the constant buffer size and resource count of every shader.
        /// <para>On NVIDIA RTX 5090 (driver 617.14) a shader whose 32-bit root constant count (<c>ConstantBufferSize / 4</c>)
        /// and resource count (shader resource views plus unordered access views) are both odd returns silently wrong
        /// double-precision results, although its DXIL is identical to a working shader apart from the constant buffer size
        /// (ZiolkowskiJakub/DiGi.ComputeSharp#3). Either count being even avoids it.</para>
        /// <para>The pinned table makes every layout change and every new shader fail this fact until its entry is added,
        /// so the change is made deliberately and its parity with a proven layout is checked. The layout is read from the
        /// generated shader descriptors, so no graphics device is needed.</para>
        /// </summary>
        [Fact]
        public void ComputeShader_ConstantBufferLayout()
        {
            Dictionary<string, (int ConstantBufferSize, int ResourceCount)> layouts = new()
            {
                ["Coordinate3InsideComputeShader"] = (24, 3),
                ["Line2IntersectComputeShader"] = (32, 3),
                ["Line2IntersectionComputeShader"] = (80, 2),
                ["Line2IntersectionsComputeShader"] = (24, 3),
                ["Line3IntersectComputeShader"] = (32, 3),
                ["Line3IntersectionComputeShader"] = (104, 2),
                ["Triangle3ExternalShadingComputeShader"] = (56, 3),
                ["Triangle3ExternalShadingRowOffsetComputeShader"] = (56, 3),
                ["Triangle3IntersectionComputeShader"] = (132, 2),
                ["Triangle3IntersectionsComputeShader"] = (24, 3),
                ["Triangle3ShadingComputeShader"] = (56, 2),
                ["Triangle3ShadingRowOffsetComputeShader"] = (56, 2),
                ["Triangle3ShadowProjectionComputeShader"] = (56, 5),
            };

            Assembly assembly = typeof(Spatial.Classes.Triangle3).Assembly;
            List<Type> types = [.. assembly.GetTypes().Where(x => x.IsValueType && typeof(IComputeShader).IsAssignableFrom(x)).OrderBy(x => x.Name)];

            Assert.Equal(layouts.Keys.OrderBy(x => x), types.Select(x => x.Name));

            MethodInfo methodInfo_ConstantBufferSize = typeof(Query).GetMethod(nameof(Query.ConstantBufferSize))!;
            MethodInfo methodInfo_ResourceCount = typeof(Query).GetMethod(nameof(Query.ResourceCount))!;

            foreach (Type type in types)
            {
                int constantBufferSize = (int)methodInfo_ConstantBufferSize.MakeGenericMethod(type).Invoke(null, null)!;
                int resourceCount = (int)methodInfo_ResourceCount.MakeGenericMethod(type).Invoke(null, null)!;
                int constantCount = constantBufferSize / 4;

                testOutputHelper.WriteLine($"{type.Name}: {constantBufferSize} B, {constantCount} root constants, {resourceCount} resources");

                Assert.False(constantCount % 2 == 1 && resourceCount % 2 == 1, $"{type.Name} has {constantCount} root constants and {resourceCount} resources; an odd count of both returns wrong results on NVIDIA RTX 5090 (ZiolkowskiJakub/DiGi.ComputeSharp#3). Reorder or pad its fields to an even root constant count.");

                (int ConstantBufferSize, int ResourceCount) layout = layouts[type.Name];
                Assert.True(layout.ConstantBufferSize == constantBufferSize && layout.ResourceCount == resourceCount, $"{type.Name} layout changed: expected {layout.ConstantBufferSize} B and {layout.ResourceCount} resources, found {constantBufferSize} B and {resourceCount} resources. Check parity with the previous layout, then update the pinned table.");
            }
        }
    }
}
