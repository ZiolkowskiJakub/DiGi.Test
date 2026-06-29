namespace DiGi.EPW.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the construction, property values, and JSON serialization round-trip of the <see cref="Classes.TypicalExtremePeriod"/> class.
        /// </summary>
        [Fact]
        public void TypicalExtremePeriod()
        {
            Classes.TypicalExtremePeriod typicalExtremePeriod = new("Summer - Week Nearest Max Temperature For Period", "Extreme", "6/29", "7/ 5");

            Assert.Equal("Summer - Week Nearest Max Temperature For Period", typicalExtremePeriod.Name);
            Assert.Equal("Extreme", typicalExtremePeriod.PeriodType);
            Assert.Equal("6/29", typicalExtremePeriod.StartDate);
            Assert.Equal("7/ 5", typicalExtremePeriod.EndDate);

            Core.xUnit.Query.SerializationCheck(typicalExtremePeriod);
        }
    }
}
