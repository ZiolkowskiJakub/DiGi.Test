using System;

namespace DiGi.Weather.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the missing-value convention of <see cref="Classes.WeatherRecord"/>: each <c>Query.*Value</c> helper answers null for a null record and for its field's <see cref="Constants.MissingValue"/> sentinel, and the reading itself otherwise - 0 included, and a value just below the sentinel (a sentinel is matched exactly, not by range).
        /// <para>Reference: DiGi.Weather#2.</para>
        /// </summary>
        [Fact]
        public void Query_MissingValue()
        {
            Assert.Null(Query.GlobalHorizontalRadiationValue(null));
            Assert.Null(Query.DirectNormalRadiationValue(null));
            Assert.Null(Query.DiffuseHorizontalRadiationValue(null));
            Assert.Null(Query.SnowDepthValue(null));
            Assert.Null(Query.AlbedoValue(null));

            Classes.WeatherRecord weatherRecord_Missing = ScratchWeatherRecord(Constants.MissingValue.GlobalHorizontalRadiation, Constants.MissingValue.DirectNormalRadiation, Constants.MissingValue.DiffuseHorizontalRadiation, Constants.MissingValue.SnowDepth, Constants.MissingValue.Albedo);
            Assert.Null(weatherRecord_Missing.GlobalHorizontalRadiationValue());
            Assert.Null(weatherRecord_Missing.DirectNormalRadiationValue());
            Assert.Null(weatherRecord_Missing.DiffuseHorizontalRadiationValue());
            Assert.Null(weatherRecord_Missing.SnowDepthValue());
            Assert.Null(weatherRecord_Missing.AlbedoValue());

            Classes.WeatherRecord weatherRecord_Zero = ScratchWeatherRecord(0, 0, 0, 0, 0);
            Assert.Equal(0f, weatherRecord_Zero.GlobalHorizontalRadiationValue());
            Assert.Equal(0f, weatherRecord_Zero.DirectNormalRadiationValue());
            Assert.Equal(0f, weatherRecord_Zero.DiffuseHorizontalRadiationValue());
            Assert.Equal(0f, weatherRecord_Zero.SnowDepthValue());
            Assert.Equal(0f, weatherRecord_Zero.AlbedoValue());

            Classes.WeatherRecord weatherRecord_Inside = ScratchWeatherRecord(9998.9f, 9998.9f, 9998.9f, 998.9f, 998.9f);
            Assert.Equal(9998.9f, weatherRecord_Inside.GlobalHorizontalRadiationValue());
            Assert.Equal(9998.9f, weatherRecord_Inside.DirectNormalRadiationValue());
            Assert.Equal(9998.9f, weatherRecord_Inside.DiffuseHorizontalRadiationValue());
            Assert.Equal(998.9f, weatherRecord_Inside.SnowDepthValue());
            Assert.Equal(998.9f, weatherRecord_Inside.AlbedoValue());

            Classes.WeatherRecord weatherRecord_Reading = ScratchWeatherRecord(812.5f, 705.25f, 160.75f, 12f, 0.2f);
            Assert.Equal(812.5f, weatherRecord_Reading.GlobalHorizontalRadiationValue());
            Assert.Equal(705.25f, weatherRecord_Reading.DirectNormalRadiationValue());
            Assert.Equal(160.75f, weatherRecord_Reading.DiffuseHorizontalRadiationValue());
            Assert.Equal(12f, weatherRecord_Reading.SnowDepthValue());
            Assert.Equal(0.2f, weatherRecord_Reading.AlbedoValue());

            static Classes.WeatherRecord ScratchWeatherRecord(float globalHorizontalRadiation, float directNormalRadiation, float diffuseHorizontalRadiation, float snowDepth, float albedo)
            {
                return new Classes.WeatherRecord(new DateTime(2026, 6, 21, 12, 30, 0, DateTimeKind.Unspecified), null, 20, 10, 50, 101325, 0, 0, 300, globalHorizontalRadiation, directNormalRadiation, diffuseHorizontalRadiation, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, null, 0, 0, snowDepth, 0, albedo, 0, 0);
            }
        }
    }
}
