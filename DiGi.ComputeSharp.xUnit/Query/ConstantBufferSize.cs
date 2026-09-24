using ComputeSharp;
using ComputeSharp.Descriptors;

namespace DiGi.ComputeSharp.xUnit
{
    public static partial class Query
    {
        /// <summary>
        /// Gets the size in bytes of the constant buffer ComputeSharp generates for a compute shader, including the 12-byte <c>__x</c>/<c>__y</c>/<c>__z</c> dispatch header.
        /// <para>The constant buffer is bound as 32-bit root constants, so the root constant count is this size divided by 4. Reading it needs no graphics device.</para>
        /// </summary>
        /// <typeparam name="T">The compute shader type.</typeparam>
        /// <returns>The constant buffer size in bytes.</returns>
        public static int ConstantBufferSize<T>() where T : struct, IComputeShader, IComputeShaderDescriptor<T>
        {
            return T.ConstantBufferSize;
        }
    }
}
