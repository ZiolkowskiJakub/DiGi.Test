using ComputeSharp;
using ComputeSharp.Descriptors;

namespace DiGi.ComputeSharp.xUnit
{
    public static partial class Query
    {
        /// <summary>
        /// Gets the number of resources (shader resource views and unordered access views) ComputeSharp binds for a compute shader.
        /// <para>Reading it needs no graphics device.</para>
        /// </summary>
        /// <typeparam name="T">The compute shader type.</typeparam>
        /// <returns>The number of resource descriptor ranges of the shader.</returns>
        public static int ResourceCount<T>() where T : struct, IComputeShader, IComputeShaderDescriptor<T>
        {
            return T.ResourceDescriptorRanges.Length;
        }
    }
}
