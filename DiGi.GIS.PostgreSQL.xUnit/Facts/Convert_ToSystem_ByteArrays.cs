using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the packing of the <c>shading_factors</c> column, <see cref="Convert.ToSystem_Bytes(IEnumerable{byte[]})"/> and <see cref="Convert.ToSystem_ByteArrays(byte[])"/>: the value is a little-endian <c>uint32</c> hour count followed by each surface's bytes in order, it unpacks to the same arrays, and every input that cannot describe one common hour count is refused - null, empty, a null or empty element, unequal lengths, a value shorter than its header, a zero count, or a length that is not a whole number of surfaces.
        /// <para>Reference: DiGi.GIS.PostgreSQL#100.</para>
        /// </summary>
        [Fact]
        public void Convert_ToSystem_ByteArrays()
        {
            byte[] shadingFactors_1 = [0, 1, 2];
            byte[] shadingFactors_2 = [253, 254, 255];

            byte[]? bytes = Convert.ToSystem_Bytes([shadingFactors_1, shadingFactors_2]);
            byte[] bytes_Expected = [3, 0, 0, 0, 0, 1, 2, 253, 254, 255];
            Assert.Equal(bytes_Expected, bytes);

            List<byte[]>? shadingFactors = Convert.ToSystem_ByteArrays(bytes);
            Assert.NotNull(shadingFactors);
            Assert.Equal(2, shadingFactors.Count);
            Assert.Equal(shadingFactors_1, shadingFactors[0]);
            Assert.Equal(shadingFactors_2, shadingFactors[1]);

            // One surface of a whole reference year round-trips too.
            byte[] shadingFactors_Year = ScratchShadingFactors(180, 2);
            List<byte[]>? shadingFactors_YearRoundTrip = Convert.ToSystem_ByteArrays(Convert.ToSystem_Bytes([shadingFactors_Year]));
            Assert.NotNull(shadingFactors_YearRoundTrip);
            Assert.Equal(shadingFactors_Year, Assert.Single(shadingFactors_YearRoundTrip));

            Assert.Null(Convert.ToSystem_Bytes((IEnumerable<byte[]?>?)null));
            Assert.Null(Convert.ToSystem_Bytes(new List<byte[]?>()));
            Assert.Null(Convert.ToSystem_Bytes([shadingFactors_1, null]));
            Assert.Null(Convert.ToSystem_Bytes([shadingFactors_1, []]));
            Assert.Null(Convert.ToSystem_Bytes([shadingFactors_1, [1, 2]]));

            Assert.Null(Convert.ToSystem_ByteArrays(null));
            Assert.Null(Convert.ToSystem_ByteArrays([3, 0, 0]));
            Assert.Null(Convert.ToSystem_ByteArrays([0, 0, 0, 0, 1, 2]));
            Assert.Null(Convert.ToSystem_ByteArrays([3, 0, 0, 0, 1, 2, 3, 4]));

            List<byte[]>? shadingFactors_None = Convert.ToSystem_ByteArrays([3, 0, 0, 0]);
            Assert.NotNull(shadingFactors_None);
            Assert.Empty(shadingFactors_None);
        }
    }
}
