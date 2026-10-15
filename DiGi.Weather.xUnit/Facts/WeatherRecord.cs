using System;
using System.Linq;

namespace DiGi.Weather.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.WeatherRecord"/> keeps every value its constructor stores - <see cref="Classes.WeatherRecord.Albedo"/> included, which the constructor used to accept and discard - through the string round trip, the clone (<c>SerializationCheck</c>) and the copy constructor, which <c>SerializationCheck</c> never runs.
        /// <para>Reference: DiGi.Weather#2.</para>
        /// </summary>
        [Fact]
        public void WeatherRecord()
        {
            Classes.WeatherRecord weatherRecord = new(
                new DateTime(2026, 6, 21, 12, 30, 0, DateTimeKind.Unspecified),
                null,
                24.5f,
                12.25f,
                45f,
                100500f,
                1300f,
                1360f,
                350f,
                812.5f,
                705.25f,
                160.75f,
                90000f,
                70000f,
                18000f,
                5000f,
                270f,
                3.5f,
                2,
                1,
                30f,
                77777f,
                0,
                null,
                18f,
                0.12f,
                0f,
                88,
                0.2f,
                0f,
                0f);

            AssertWeatherRecord(weatherRecord);

            string? json = Core.Convert.ToSystem_String(weatherRecord);
            Assert.NotNull(json);
            Assert.Contains("\"Albedo\":0.2", json);
            AssertWeatherRecord(Core.Convert.ToDiGi<Classes.WeatherRecord>(json)?.FirstOrDefault());

            // SerializationCheck never runs the copy constructor, so it is asserted directly.
            AssertWeatherRecord(new Classes.WeatherRecord(weatherRecord));

            Core.xUnit.Query.SerializationCheck(weatherRecord);

            static void AssertWeatherRecord(Classes.WeatherRecord? weatherRecord_Actual)
            {
                Assert.NotNull(weatherRecord_Actual);
                Assert.Equal(new DateTime(2026, 6, 21, 12, 30, 0, DateTimeKind.Unspecified), weatherRecord_Actual.DateTime);
                Assert.Equal(24.5f, weatherRecord_Actual.DryBulbTemperature);
                Assert.Equal(12.25f, weatherRecord_Actual.DewPointTemperature);
                Assert.Equal(45f, weatherRecord_Actual.RelativeHumidity);
                Assert.Equal(100500f, weatherRecord_Actual.AtmosphericStationPressure);
                Assert.Equal(350f, weatherRecord_Actual.HorizontalInfraredRadiationIntensity);
                Assert.Equal(812.5f, weatherRecord_Actual.GlobalHorizontalRadiation);
                Assert.Equal(705.25f, weatherRecord_Actual.DirectNormalRadiation);
                Assert.Equal(160.75f, weatherRecord_Actual.DiffuseHorizontalRadiation);
                Assert.Equal(90000f, weatherRecord_Actual.GlobalHorizontalIlluminance);
                Assert.Equal(70000f, weatherRecord_Actual.DirectNormalIlluminance);
                Assert.Equal(18000f, weatherRecord_Actual.DiffuseHorizontalIlluminance);
                Assert.Equal(270f, weatherRecord_Actual.WindDirection);
                Assert.Equal(3.5f, weatherRecord_Actual.WindSpeed);
                Assert.Equal(2, weatherRecord_Actual.TotalSkyCover);
                Assert.Equal(1, weatherRecord_Actual.OpaqueSkyCover);
                Assert.Equal(77777f, weatherRecord_Actual.CeilingHeight);
                Assert.Equal(0f, weatherRecord_Actual.SnowDepth);
                Assert.Equal(0.2f, weatherRecord_Actual.Albedo);
            }
        }
    }
}
