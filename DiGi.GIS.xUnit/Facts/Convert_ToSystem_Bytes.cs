using System;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the shading-factor quantization pair <see cref="Convert.ToSystem_Bytes(System.Collections.Generic.IEnumerable{double})"/> / <see cref="Convert.ToSystem_Doubles(byte[])"/>: null in gives null out, out-of-range values clamp to 0 and 255, NaN maps to 0, a value just inside the half step rounds down and one just outside rounds up, and the round-trip error over 10 000 random values stays within 0.5/255.
        /// <para>Reference: DiGi.GIS#8 (stored solar radiation, DiGi.GIS.WebAPI.UI#73).</para>
        /// </summary>
        [Fact]
        public void Convert_ToSystem_Bytes()
        {
            Assert.Null(Convert.ToSystem_Bytes(null));
            Assert.Null(Convert.ToSystem_Doubles(null));
            Assert.Empty(Convert.ToSystem_Bytes([])!);
            Assert.Empty(Convert.ToSystem_Doubles([])!);

            byte[] bytes_Boundary = [0, 255, 0, 255, 0, 255, 0];
            Assert.Equal(bytes_Boundary, Convert.ToSystem_Bytes([0.0, 1.0, -0.1, 1.1, double.NaN, double.PositiveInfinity, double.NegativeInfinity]));

            double halfStep = 0.5 / 255;
            byte[] bytes_HalfStep = [0, 1, 255, 254];
            Assert.Equal(bytes_HalfStep, Convert.ToSystem_Bytes([halfStep - 1e-9, halfStep + 1e-9, 1 - halfStep + 1e-9, 1 - halfStep - 1e-9]));

            double[]? values_Decoded = Convert.ToSystem_Doubles([0, 1, 128, 255]);
            Assert.NotNull(values_Decoded);
            Assert.Equal(0.0, values_Decoded[0]);
            Assert.Equal(1.0 / 255, values_Decoded[1]);
            Assert.Equal(128.0 / 255, values_Decoded[2]);
            Assert.Equal(1.0, values_Decoded[3]);

            Random random = new(73);
            double[] values = new double[10000];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = random.NextDouble();
            }

            double[]? values_RoundTrip = Convert.ToSystem_Doubles(Convert.ToSystem_Bytes(values));
            Assert.NotNull(values_RoundTrip);
            Assert.Equal(values.Length, values_RoundTrip.Length);

            double error_Max = 0;
            for (int i = 0; i < values.Length; i++)
            {
                error_Max = Math.Max(error_Max, Math.Abs(values_RoundTrip[i] - values[i]));
            }

            Assert.True(error_Max <= halfStep + 1e-12, $"Maximum round-trip error {error_Max} exceeds 0.5/255.");
        }
    }
}
