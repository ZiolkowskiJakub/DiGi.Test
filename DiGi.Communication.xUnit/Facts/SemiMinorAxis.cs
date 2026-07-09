namespace DiGi.Communication.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the propagation ellipsoid semi-minor axis b_n = sqrt(c * tau_n * (c * tau_n + 2 * d)) / 2, including the degenerate zero-delay ellipsoid and the consistency relation a_n^2 = b_n^2 + (d / 2)^2.
        /// </summary>
        [Fact]
        public void SemiMinorAxis()
        {
            double distance = 300;
            double delay = 1e-6;

            double semiMinorAxis = Query.SemiMinorAxis(delay, distance);

            // c = 299792458 m/s (exact SI value): b_n = sqrt(299.792458 * 899.792458) / 2 = 259.687789794631
            Assert.Equal(259.687789794631, semiMinorAxis, 9);
            Assert.Equal(0.0, Query.SemiMinorAxis(0, distance), 9);

            double semiMajorAxis = Query.SemiMajorAxis(delay, distance);
            Assert.Equal(semiMajorAxis * semiMajorAxis, (semiMinorAxis * semiMinorAxis) + (distance * distance / 4.0), 6);
        }
    }
}
