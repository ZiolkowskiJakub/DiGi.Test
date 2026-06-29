using System.Collections.Generic;

namespace DiGi.EPW.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the construction, property values, and JSON serialization round-trip of the <see cref="Classes.DesignConditions"/> class, including the heating, cooling, and extreme value lists.
        /// </summary>
        [Fact]
        public void DesignConditions()
        {
            List<double> doubles_Heating = [1, -16.6, -13.1, -19.2, 0.7, -16.1, -15.8, 1, -12.7, 14.1, 4.6, 12.4, 4.5, 2.8, 90];
            List<double> doubles_Cooling = [7, 10.3, 29.6, 20, 27.6, 19.2, 25.9, 18.1, 21.2, 27.2, 20.2, 25.7, 19.2, 24.2, 3.9, 170];
            List<double> doubles_Extreme = [10.3, 9.1, 8.2, 24.2, -18.4, 32.9];

            Classes.DesignConditions designConditions = new(1, "Climate Design Data 2009 ASHRAE Handbook", null, doubles_Heating, doubles_Cooling, doubles_Extreme);

            Assert.Equal(1, designConditions.NumberOfDesignConditions);
            Assert.Equal("Climate Design Data 2009 ASHRAE Handbook", designConditions.Source);
            Assert.Null(designConditions.Name);
            Assert.Equal(doubles_Heating.Count, designConditions.HeatingValues?.Count);
            Assert.Equal(doubles_Cooling.Count, designConditions.CoolingValues?.Count);
            Assert.Equal(doubles_Extreme.Count, designConditions.ExtremeValues?.Count);
            Assert.Equal(-16.6, designConditions.HeatingValues?[1]);

            Core.xUnit.Query.SerializationCheck(designConditions);
        }
    }
}
