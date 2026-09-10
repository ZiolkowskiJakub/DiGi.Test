namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the ground reflectance resolution rules, covering the missing marker, the zero-valued column that weather records without an albedo field produce, out-of-range values and the snow override.
        /// </summary>
        [Fact]
        public void Albedo()
        {
            // Missing marker used by weather files.
            Assert.Equal(Solar.Constants.Albedo.Default, Query.Albedo(999, null), 10);

            // Not supplied at all.
            Assert.Equal(Solar.Constants.Albedo.Default, Query.Albedo(null, null), 10);

            // A weather record carrying no albedo column reports zero rather than the missing
            // marker, and taking that at face value would remove the ground-reflected component.
            Assert.Equal(Solar.Constants.Albedo.Default, Query.Albedo(0, null), 10);

            // Out of range.
            Assert.Equal(Solar.Constants.Albedo.Default, Query.Albedo(1.5, null), 10);
            Assert.Equal(Solar.Constants.Albedo.Default, Query.Albedo(-0.3, null), 10);
            Assert.Equal(Solar.Constants.Albedo.Default, Query.Albedo(double.NaN, null), 10);

            // A usable value is returned unchanged.
            Assert.Equal(0.35, Query.Albedo(0.35, null), 10);
            Assert.Equal(1.0, Query.Albedo(1.0, null), 10);

            // Snow on the ground overrides any supplied albedo.
            Assert.Equal(Solar.Constants.Albedo.Snow, Query.Albedo(0.35, 5), 10);
            Assert.Equal(Solar.Constants.Albedo.Snow, Query.Albedo(999, 5), 10);
            Assert.Equal(Solar.Constants.Albedo.Snow, Query.Albedo(null, 0.1), 10);

            // No snow, so no override.
            Assert.Equal(0.35, Query.Albedo(0.35, 0), 10);

            // A snow depth carrying the missing marker is not snow.
            Assert.Equal(0.35, Query.Albedo(0.35, 999), 10);
            Assert.Equal(0.35, Query.Albedo(0.35, double.NaN), 10);
        }
    }
}
