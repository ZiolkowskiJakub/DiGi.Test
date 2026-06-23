namespace DiGi.Unit.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the functionality of the Query.TryConvert method by verifying various unit conversion scenarios, including length, temperature, and time units, as well as tolerance handling.
        /// </summary>
        [Fact]
        public void TryConvert()
        {
            bool converted;
            double? value;

            converted = Query.TryConvert(10, Enums.LengthUnit.Meter, Enums.LengthUnit.Milimeter, out value);

            Assert.True(converted);

            Assert.Equal(10000, value);

            converted = Query.TryConvert(2, Enums.LengthUnit.Meter, Enums.LengthUnit.Feet, out value);

            Assert.True(converted);

            Assert.Equal(6.561679790026246, value);

            converted = Query.TryConvert(2, Enums.TemperatureUnit.Kelvin, Enums.TemperatureUnit.Fahrenheit, out value);

            Assert.True(converted);

            Assert.Equal(-456.07, value);

            converted = Query.TryConvert(2, Enums.TimeUnit.Second, Enums.TimeUnit.Minute, out value, Core.Constants.Tolerance.MacroDistance);

            Assert.True(converted);

            Assert.Equal(0.033, value);
        }
    }
}