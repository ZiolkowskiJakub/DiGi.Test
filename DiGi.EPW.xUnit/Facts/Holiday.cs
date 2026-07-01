namespace DiGi.EPW.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the construction, property values, and JSON serialization round-trip of the <see cref="Classes.Holiday"/> class.
        /// </summary>
        [Fact]
        public void Holiday()
        {
            Classes.Holiday holiday = new("New Year's Day", "1/1");

            Assert.Equal("New Year's Day", holiday.Name);
            Assert.Equal("1/1", holiday.Date);

            Core.xUnit.Query.SerializationCheck(holiday);
        }
    }
}