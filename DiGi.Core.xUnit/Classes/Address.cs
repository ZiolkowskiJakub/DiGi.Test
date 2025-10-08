using DiGi.Core.Enums;

namespace DiGi.Core.xUnit
{
    public partial class Classes
    {
        [Fact]
        public void Address()
        {
            Core.Classes.Address address = new Core.Classes.Address("123 Main St", "Anytown", "12345", CountryCode.US);

            Query.SerializationCheck(address);
        }
    }
}