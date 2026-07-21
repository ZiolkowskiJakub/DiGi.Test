namespace DiGi.Communication.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the propagation ellipsoid semi-major axis a_n = (c * tau_n + d) / 2, including the degenerate zero-delay ellipsoid and invalid input.
        /// </summary>
        [Fact]
        public void SemiMajorAxis()
        {
            double distance = 300;

            // c = 299792458 m/s (exact SI value): a_n = (299792458 * 1e-6 + 300) / 2 = 299.896229
            Assert.Equal(299.896229, Query.SemiMajorAxis(1e-6, distance), 9);
            Assert.Equal(150.0, Query.SemiMajorAxis(0, distance), 9);
            Assert.True(double.IsNaN(Query.SemiMajorAxis(-1e-6, distance)));
            Assert.True(double.IsNaN(Query.SemiMajorAxis(1e-6, 0)));
        }
    }
}