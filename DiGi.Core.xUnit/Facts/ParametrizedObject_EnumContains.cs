using DiGi.Core.Parameter.Classes;
using DiGi.Core.Enums;
using Xunit;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the new Contains overloads for enum parameters work correctly on ParametrizedObject.
        /// </summary>
        [Fact]
        public void ParametrizedObject_ContainsEnum()
        {
            // Arrange: create a parametrized object and set a value using an enum key
            ParametrizedObject parametrizedObject = new();
            bool setResult = parametrizedObject.SetValue(CountryCode.PL, CountryCode.PL);
            Assert.True(setResult);

            // Act & Assert: the Contains overload should report true for the stored enum
            Assert.True(parametrizedObject.Contains(CountryCode.PL));
        }
    }
}
