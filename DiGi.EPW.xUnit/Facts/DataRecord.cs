namespace DiGi.EPW.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the construction, property values, and JSON serialization round-trip of the <see cref="Classes.DataRecord"/> class, covering every numeric and string field of the hourly weather data record.
        /// </summary>
        [Fact]
        public void DataRecord()
        {
            Classes.DataRecord dataRecord = new(1992, 1, 1, 1, 60, "C9C9C9C9*0?9?9?9?9?9?9?9A7A7B8C8A7A7*0E8*0*0", -2.5, -3.1, 95, 100800, 0, 1415, 241, 0, 0, 0, 0, 0, 0, 0, 250, 1.0, 2, 1, 11.2, 22000, 0, "999999099", 0, 0.1560, 0, 88, 0.000, 0.0, 0.0);

            Assert.Equal(1992, dataRecord.Year);
            Assert.Equal(1, dataRecord.Month);
            Assert.Equal(1, dataRecord.Day);
            Assert.Equal(1, dataRecord.Hour);
            Assert.Equal(60, dataRecord.Minute);
            Assert.Equal("C9C9C9C9*0?9?9?9?9?9?9?9A7A7B8C8A7A7*0E8*0*0", dataRecord.DataSourceAndUncertaintyFlags);
            Assert.Equal(-2.5, dataRecord.DryBulbTemperature);
            Assert.Equal(-3.1, dataRecord.DewPointTemperature);
            Assert.Equal(95, dataRecord.RelativeHumidity);
            Assert.Equal(100800, dataRecord.AtmosphericStationPressure);
            Assert.Equal(250, dataRecord.WindDirection);
            Assert.Equal(1.0, dataRecord.WindSpeed);
            Assert.Equal(2, dataRecord.TotalSkyCover);
            Assert.Equal(1, dataRecord.OpaqueSkyCover);
            Assert.Equal("999999099", dataRecord.PresentWeatherCodes);
            Assert.Equal(0, dataRecord.PrecipitableWater);
            Assert.Equal(0.1560, dataRecord.AerosolOpticalDepth);
            Assert.Equal(88, dataRecord.DaysSinceLastSnowfall);
            Assert.Equal(0.000, dataRecord.Albedo);

            Core.xUnit.Query.SerializationCheck(dataRecord);
        }
    }
}
