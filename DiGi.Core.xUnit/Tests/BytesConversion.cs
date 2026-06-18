using DiGi.Core.Classes;
using System.Linq;

namespace DiGi.Core.xUnit
{
    public partial class Tests
    {
        /// <summary>
        /// Verifies that an <see cref="Address"/> object can be correctly converted to a byte array and subsequently deserialized back into an <see cref="Address"/> object while maintaining data integrity.
        /// </summary>
        [Fact]
        public void BytesConversion()
        {
            Address address_1 = new("Street", "City", "00-000", Core.Enums.CountryCode.PL);

            byte[]? bytes = Convert.ToSystem_Bytes(address_1);
            Assert.NotNull(bytes);

            Address? address_2 = Convert.ToDiGi<Address>(bytes)?.FirstOrDefault();
            Assert.NotNull(address_2);

            Assert.Equal(Convert.ToSystem_String(address_1), Convert.ToSystem_String(address_2));

            Assert.True(!string.IsNullOrWhiteSpace(address_2.City));

            Assert.True(address_1.City == address_2.City);
        }
    }
}
