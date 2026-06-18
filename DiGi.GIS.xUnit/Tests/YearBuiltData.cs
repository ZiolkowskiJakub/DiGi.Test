using DiGi.Core;
using System.Linq;

namespace DiGi.GIS.xUnit
{
    public partial class Tests
    {
        /// <summary>
        /// Tests the serialization and deserialization of year built data to ensure that the object can be correctly converted from a JSON string and back again without loss of information.
        /// </summary>
        [Fact]
        public void YearBuiltData()
        {
            string json = "{\"_type\":\"DiGi.GIS.Classes.YearBuiltData,DiGi.GIS\",\"Guid\":\"dc6d8f48-048e-47cd-bad0-d747f9d8888b\",\"YearBuilts\":[{\"_type\":\"DiGi.GIS.Classes.UserYearBuilt,DiGi.GIS\",\"Year\":2008},{\"_type\":\"DiGi.GIS.Classes.PredictedYearBuilt,DiGi.GIS\",\"Year\":2008,\"DateTime\":\"2025-05-29T09:41:47.8773778+02:00\"}],\"Reference\":\"272D6AAF-9D86-9B0E-E053-CC2BA8C0B5EA\"}";

            Interfaces.IYearBuiltData? yearBuiltData = Core.Convert.ToDiGi<Interfaces.IYearBuiltData>(json)?.FirstOrDefault();
            Assert.NotNull(yearBuiltData);

            Core.xUnit.Query.SerializationCheck(yearBuiltData);

            Assert.Equal(json, yearBuiltData.ToSystem_String());
        }
    }
}
