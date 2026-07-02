using System;

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
            Classes.DataRecord dataRecord = new(
                new DateTime(1992, 1, 1, 1, 0, 0, DateTimeKind.Unspecified),
                "C9C9C9C9*0?9?9?9?9?9?9?9A7A7B8C8A7A7*0E8*0*0",
                -2.5f,
                -3.1f,
                95f,
                100800f,
                0f,
                1415f,
                241f,
                0f,
                0f,
                0f,
                0f,
                0f,
                0f,
                0f,
                250f,
                1.0f,
                2,
                1,
                11.2f,
                22000f,
                0,
                "999999099",
                0f,
                0.1560f,
                0f,
                88,
                0.000f,
                0.0f,
                0.0f);

            Assert.Equal(new DateTime(1992, 1, 1, 1, 0, 0, DateTimeKind.Unspecified), dataRecord.DateTime);
            Assert.Equal("C9C9C9C9*0?9?9?9?9?9?9?9A7A7B8C8A7A7*0E8*0*0", dataRecord.DataSourceAndUncertaintyFlags);
            Assert.Equal(-2.5f, dataRecord.DryBulbTemperature);
            Assert.Equal(-3.1f, dataRecord.DewPointTemperature);
            Assert.Equal(95f, dataRecord.RelativeHumidity);
            Assert.Equal(100800f, dataRecord.AtmosphericStationPressure);
            Assert.Equal(250f, dataRecord.WindDirection);
            Assert.Equal(1.0f, dataRecord.WindSpeed);
            Assert.Equal(2, dataRecord.TotalSkyCover);
            Assert.Equal(1, dataRecord.OpaqueSkyCover);
            Assert.Equal("999999099", dataRecord.PresentWeatherCodes);
            Assert.Equal(0f, dataRecord.PrecipitableWater);
            Assert.Equal(0.1560f, dataRecord.AerosolOpticalDepth);
            Assert.Equal(88, dataRecord.DaysSinceLastSnowfall);
            Assert.Equal(0.000f, dataRecord.Albedo);

            Core.xUnit.Query.SerializationCheck(dataRecord);
        }
    }
}