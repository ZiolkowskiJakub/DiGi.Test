using DiGi.Core.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Solar.Classes;

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

        /// <summary>
        /// Tests that the sun direction calculation correctly calculates the solar vector for a ShadingModel.
        /// </summary>
        [Fact]
        public void SunDirection_ShadingModel()
        {
            Coordinates coordinates = new(51.4778, 0.0); // Greenwich, UK
            ShadingModel shadingModel = new(Core.Enums.UTC.PlusMinus0000, coordinates);
            DateTime dateTime = new(2026, 6, 21, 12, 0, 0);

            Vector3D? vector3D = Query.SunDirection(shadingModel, dateTime, false);
            Assert.NotNull(vector3D);
            Assert.Equal(1.0, vector3D.Length, 5);
        }

        /// <summary>
        /// Tests that the sun direction calculation correctly calculates the solar vector from a SolarTimes object.
        /// </summary>
        [Fact]
        public void SunDirection_SolarTimes()
        {
            DateTime dateTime = new(2026, 6, 21, 12, 0, 0);
            int timeOffset = 0;
            Innovative.Geometry.Angle angle_Latitude = new(51.4778);
            Innovative.Geometry.Angle angle_Longitude = new(0.0);
            Innovative.SolarCalculator.SolarTimes solarTimes = new(dateTime, timeOffset, angle_Latitude, angle_Longitude);

            Vector3D? vector3D = Query.SunDirection(solarTimes);
            Assert.NotNull(vector3D);
            Assert.Equal(1.0, vector3D.Length, 5);
        }

        /// <summary>
        /// Tests that the sun direction calculation handles null input references and boundary date-time parameters by returning null.
        /// </summary>
        [Fact]
        public void SunDirection_NullAndBoundaryCases()
        {
            Coordinates coordinates = new(51.4778, 0.0);

            // Null Coordinates
            Vector3D? vector3D_NullCoords = Query.SunDirection((Coordinates)null!, Core.Enums.UTC.PlusMinus0000, DateTime.Now, false);
            Assert.Null(vector3D_NullCoords);

            // MinValue DateTime
            Vector3D? vector3D_MinDateTime = Query.SunDirection(coordinates, Core.Enums.UTC.PlusMinus0000, DateTime.MinValue, false);
            Assert.Null(vector3D_MinDateTime);

            // MaxValue DateTime
            Vector3D? vector3D_MaxDateTime = Query.SunDirection(coordinates, Core.Enums.UTC.PlusMinus0000, DateTime.MaxValue, false);
            Assert.Null(vector3D_MaxDateTime);

            // Null ShadingModel
            Vector3D? vector3D_NullShadingModel = Query.SunDirection((ShadingModel)null!, DateTime.Now, false);
            Assert.Null(vector3D_NullShadingModel);

            // Null SolarTimes
            Vector3D? vector3D_NullSolarTimes = Query.SunDirection((Innovative.SolarCalculator.SolarTimes)null!);
            Assert.Null(vector3D_NullSolarTimes);
        }
    }
}