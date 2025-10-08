using DiGi.Core.Classes;

namespace DiGi.Core.xUnit
{
    public partial class Classes
    {
        [Fact]
        public void Value()
        {
            Query.SerializationCheck(new Value(10.0));
            Query.SerializationCheck(new Value("AAA"));
            Query.SerializationCheck(new Value(DateTime.Now));
            Query.SerializationCheck(new Value(DateTime.Now));
            Query.SerializationCheck(new Value(new Address("street", "city", "postalCode", Core.Enums.CountryCode.PL)));
            Query.SerializationCheck(new Value(["10", "20"]));
            Query.SerializationCheck(new Value(typeof(double)));
            Query.SerializationCheck(new Value(typeof(Address)));
        }
    }
}