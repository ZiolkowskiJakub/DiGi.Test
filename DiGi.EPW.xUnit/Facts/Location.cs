using DiGi.EPW.Classes;

namespace DiGi.EPW.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the construction, property values, and JSON serialization round-trip of the <see cref="Location"/> class.
        /// </summary>
        [Fact]
        public void Location()
        {
            Location location = new("WARSAW", "-", "POL", "IWEC Data", "123750", 52.17, 20.97, 1.0, 107.0);

            Assert.Equal("WARSAW", location.City);
            Assert.Equal("-", location.Region);
            Assert.Equal("POL", location.Country);
            Assert.Equal("IWEC Data", location.Source);
            Assert.Equal("123750", location.WHO);
            Assert.Equal(52.17, location.Latitude);
            Assert.Equal(20.97, location.Longitude);
            Assert.Equal(1.0, location.TimeZone);
            Assert.Equal(107.0, location.Elevation);

            Core.xUnit.Query.SerializationCheck(location);
        }
    }
}
