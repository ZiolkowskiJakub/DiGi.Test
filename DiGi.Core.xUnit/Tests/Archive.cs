using DiGi.Core.Classes;
using DiGi.Core.IO.Core.Interfaces;

namespace DiGi.Core.xUnit
{
    public partial class Tests
    {
        /// <summary>
        /// Tests the serialization and deserialization process of an <see cref="Address"/> object to ensure that the archived data is correctly preserved and restored.
        /// </summary>
        [Fact]
        public void ArchiveTest()
        {
            Address address_1 = new("Street", "City", "00-000", Core.Enums.CountryCode.PL);

            IArchive? archive = IO.Query.Serialize(address_1);
            Assert.NotNull(archive);

            Address? address_2 = IO.Query.Deserialize<Address>(archive);
            Assert.NotNull(address_2);

            Assert.Equal(Convert.ToSystem_String(address_1), Convert.ToSystem_String(address_2));

            Assert.True(!string.IsNullOrWhiteSpace(address_2.City));

            Assert.True(address_1.City == address_2.City);
        }
    }
}
