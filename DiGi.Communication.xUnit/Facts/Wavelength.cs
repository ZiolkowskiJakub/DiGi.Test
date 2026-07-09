namespace DiGi.Communication.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the wavelength calculation lambda = c / f [MHz], including the invalid non-positive frequency input.
        /// </summary>
        [Fact]
        public void Wavelength()
        {
            // c = 299792458 m/s (exact SI value): lambda = 299.792458 / f
            Assert.Equal(0.999308193333333, Query.Wavelength(300), 9);
            Assert.Equal(1.998616386666667, Query.Wavelength(150), 9);
            Assert.True(double.IsNaN(Query.Wavelength(0)));
            Assert.True(double.IsNaN(Query.Wavelength(-1)));
        }
    }
}
