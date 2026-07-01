namespace DiGi.EPW.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the construction, property values, and JSON serialization round-trip of the <see cref="Classes.DataPeriod"/> class.
        /// </summary>
        [Fact]
        public void DataPeriod()
        {
            Classes.DataPeriod dataPeriod = new("Data", "Sunday", " 1/ 1", "12/31");

            Assert.Equal("Data", dataPeriod.Name);
            Assert.Equal("Sunday", dataPeriod.StartDayOfWeek);
            Assert.Equal(" 1/ 1", dataPeriod.StartDate);
            Assert.Equal("12/31", dataPeriod.EndDate);

            Core.xUnit.Query.SerializationCheck(dataPeriod);
        }
    }
}