using System.Collections.Generic;

namespace DiGi.EPW.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the construction, property values, and JSON serialization round-trip of the <see cref="Classes.GroundTemperature"/> class, including the monthly value list and nullable soil property values.
        /// </summary>
        [Fact]
        public void GroundTemperature()
        {
            List<double> monthlyValues = [0.27, -0.52, 0.88, 3.08, 8.83, 13.26, 16.26, 17.17, 15.62, 12.17, 7.57, 3.30];

            Classes.GroundTemperature groundTemperature = new(0.5, 1.1, 1200, 1450, monthlyValues);

            Assert.Equal(0.5, groundTemperature.Depth);
            Assert.Equal(1.1, groundTemperature.Conductivity);
            Assert.Equal(1200, groundTemperature.Density);
            Assert.Equal(1450, groundTemperature.SpecificHeat);
            Assert.Equal(12, groundTemperature.MonthlyValues?.Count);
            Assert.Equal(0.27, groundTemperature.MonthlyValues?[0]);

            Core.xUnit.Query.SerializationCheck(groundTemperature);

            Classes.GroundTemperature groundTemperature_NoOptionalValues = new(2, null, null, null, null);

            Assert.Null(groundTemperature_NoOptionalValues.Conductivity);
            Assert.Null(groundTemperature_NoOptionalValues.Density);
            Assert.Null(groundTemperature_NoOptionalValues.SpecificHeat);
            Assert.Null(groundTemperature_NoOptionalValues.MonthlyValues);

            Core.xUnit.Query.SerializationCheck(groundTemperature_NoOptionalValues);
        }
    }
}
