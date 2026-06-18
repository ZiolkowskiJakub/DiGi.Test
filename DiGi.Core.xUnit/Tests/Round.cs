namespace DiGi.Core.xUnit
{
    public partial class Tests
    {
        /// <summary>
        /// Tests that the rounding functionality correctly processes a zero value using the defined distance tolerance.
        /// </summary>
        [Fact]
        public void Round()
        {
            double value = Core.Query.Round(0, Constants.Tolerance.Distance);

            Assert.Equal(0.0, value);
        }
    }
}
