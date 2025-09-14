using DiGi.Core.Enums;

namespace DiGi.Core.xUnit
{
    public partial class Classes
    {
        [Fact]
        public void Address()
        {
            Core.Classes.Address address_1 = new Core.Classes.Address("123 Main St", "Anytown", "12345", CountryCode.US);

            string? string_1 = address_1.ToSystem_String();

            Assert.NotNull(string_1);

            Core.Classes.Address? address_2 = Convert.ToDiGi<Core.Classes.Address>(string_1)?.FirstOrDefault();

            Assert.NotNull(address_2);

            string? string_2 = address_2.ToSystem_String();

            Assert.NotNull(string_2);

            Assert.Equal(string_1, string_2);

            Core.Classes.Address? address_3 = Convert.ToDiGi<Core.Classes.Address>(string_2)?.FirstOrDefault();

            Assert.Equal(address_3.ToSystem_String(), string_2);
        }
    }
}