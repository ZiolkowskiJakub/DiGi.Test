using DiGi.GIS.ML;

namespace DiGi.GIS.ML.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a raw score rounds to the year the deployed path writes: an exact half rounds down, anything above it rounds up, and the result is clamped to the stored range.
        /// <para>The evaluation app judges the raw model against a detection year through this rule, and Query.PredictedYearBuilts writes through it, so the two cannot disagree by a rounding convention. The boundary is asserted on both sides of the half.</para>
        /// </summary>
        [Fact]
        public void PredictedYear()
        {
            Assert.Equal(2008, 2008.0.PredictedYear());
            Assert.Equal(2008, 2008.5.PredictedYear());
            Assert.Equal(2009, 2008.5001.PredictedYear());
            Assert.Equal(2008, 2008.4999.PredictedYear());
            Assert.Equal(2010, 2009.9.PredictedYear());

            Assert.Equal(0, (-3.2).PredictedYear());
            Assert.Equal(ushort.MaxValue, 70000.0.PredictedYear());
            Assert.Equal(0, double.NaN.PredictedYear());
        }
    }
}
