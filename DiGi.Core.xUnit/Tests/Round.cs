namespace DiGi.Core.xUnit
{
    public partial class Tests
    {
        [Fact]
        public void Round()
        {
            double value = Core.Query.Round(0, Constans.Tolerance.Distance);

            Assert.Equal(0.0, value);
        }
    }
}