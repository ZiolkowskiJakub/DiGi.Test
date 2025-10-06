using DiGi.Core.Classes;

namespace DiGi.Core.xUnit
{
    public partial class Classes
    {
        [Fact]
        public void Value()
        {
            Action<Value> check = (Value value) =>
            {
                string? json_Expected = Convert.ToSystem_String(value);

                Assert.NotNull(json_Expected);

                Value? value_Actual = Convert.ToDiGi<Value>(json_Expected)?.FirstOrDefault();

                Assert.NotNull(value_Actual);

                string? json_Actual = Convert.ToSystem_String(value_Actual);

                Assert.Equal(json_Expected, json_Actual);
            };

            check.Invoke(new Value(10.0));
            check.Invoke(new Value("AAA"));
            check.Invoke(new Value(DateTime.Now));
            check.Invoke(new Value(new Address("street", "city", "postalCode", Core.Enums.CountryCode.PL)));
            check.Invoke(new Value(["10", "20"]));
            check.Invoke(new Value(typeof(double)));
            check.Invoke(new Value(typeof(Address)));
        }
    }
}