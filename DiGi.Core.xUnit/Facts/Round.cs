namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that the rounding functionality correctly processes a zero value using the defined distance tolerance.
        /// </summary>
        [Fact]
        public void Round()
        {
            double double_Value = Core.Query.Round(0, Constants.Tolerance.Distance);

            Assert.Equal(0.0, double_Value);
        }

        /// <summary>
        /// Tests that rounding a large value with an extremely small tolerance succeeds without throwing an OverflowException.
        /// </summary>
        [Fact]
        public void Round_OverflowSafety()
        {
            double double_Value = 1e20;
            double double_Tolerance = 1e-20;
            double double_Result = Core.Query.Round(double_Value, double_Tolerance);

            Assert.Equal(double_Value, double_Result);
        }
    }
}