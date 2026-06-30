namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Unit tests for <see cref="Spatial.Query.Round(double, double)"/>.
    /// </summary>
    public partial class Facts
    {
        [Fact]
        public void Round_Basic()
        {
            // Round to the nearest multiple of the tolerance.
            Assert.Equal(1.2300, Spatial.Query.Round(1.23456, 0.01), 1e-9);
            Assert.Equal(-1.2300, Spatial.Query.Round(-1.23456, 0.01), 1e-9);
        }

        /// <summary>
        /// Regression test for the integer overflow: value / tolerance can exceed int range
        /// (here 1e12), which the old (int) cast wrapped to a nonsense result.
        /// </summary>
        [Fact]
        public void Round_LargeValueDoesNotOverflow()
        {
            // 1_000_000.0000004 / 1e-6 == ~1e12, far beyond int.MaxValue (~2.1e9).
            double rounded = Spatial.Query.Round(1000000.0000004, 1e-6);

            Assert.Equal(1000000.0, rounded, 1e-6);
        }
    }
}
