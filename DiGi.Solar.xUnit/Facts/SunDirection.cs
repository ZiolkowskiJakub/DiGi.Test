using DiGi.Core.Classes;
using DiGi.Geometry.Spatial.Classes;
using System;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that the sun direction calculation correctly determines the solar vector during the day and handles nighttime exclusions.
        /// </summary>
        [Fact]
        public void SunDirection()
        {
            Coordinates coordinates = new(51.4778, 0.0); // Greenwich, UK
            Core.Enums.UTC uTC = Core.Enums.UTC.PlusMinus0000;

            DateTime dateTime_Day = new(2026, 6, 21, 12, 0, 0); // Noon on summer solstice
            DateTime dateTime_Night = new(2026, 6, 21, 0, 0, 0); // Midnight on summer solstice

            // Test day calculation (should return a valid direction vector)
            Vector3D? vector3D_Day = Query.SunDirection(coordinates, uTC, dateTime_Day, false);
            Assert.NotNull(vector3D_Day);
            Assert.Equal(1.0, vector3D_Day.Length, 5);

            // Test night calculation with includeNight = false (should return null)
            Vector3D? vector3D_NightExcluded = Query.SunDirection(coordinates, uTC, dateTime_Night, false);
            Assert.Null(vector3D_NightExcluded);

            // Test night calculation with includeNight = true (should return a valid direction vector pointing downwards)
            Vector3D? vector3D_NightIncluded = Query.SunDirection(coordinates, uTC, dateTime_Night, true);
            Assert.NotNull(vector3D_NightIncluded);
            Assert.Equal(1.0, vector3D_NightIncluded.Length, 5);

            // Test fractional timezone (UTC+05:30, India Standard Time)
            Coordinates coordinates_India = new(28.6139, 77.2090);
            Core.Enums.UTC uTC_India = Core.Enums.UTC.Plus0530;

            DateTime dateTime_IndiaNoon = new(2026, 6, 21, 12, 0, 0);
            Vector3D? vector3D_India = Query.SunDirection(coordinates_India, uTC_India, dateTime_IndiaNoon, false);

            Assert.NotNull(vector3D_India);
            Assert.Equal(1.0, vector3D_India.Length, 5);
        }
    }
}
