using DiGi.Core;
using System.Linq;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the serialization and deserialization of year built data to ensure that the object can be correctly converted from a JSON string and back again without loss of information.
        /// <para>The copy is checked as well: a stored year built row is addressed by the reference of the building it describes together with the unique identifier of the object itself, so a copy that reissues either one can no longer be matched to the row it came from.</para>
        /// </summary>
        [Fact]
        public void YearBuiltData()
        {
            string json = "{\"_type\":\"DiGi.GIS.Classes.YearBuiltData,DiGi.GIS\",\"Guid\":\"dc6d8f48-048e-47cd-bad0-d747f9d8888b\",\"YearBuilts\":[{\"_type\":\"DiGi.GIS.Classes.UserYearBuilt,DiGi.GIS\",\"Year\":2008},{\"_type\":\"DiGi.GIS.Classes.PredictedYearBuilt,DiGi.GIS\",\"Year\":2008,\"DateTime\":\"2025-05-29T09:41:47.8773778+02:00\"}],\"Reference\":\"272D6AAF-9D86-9B0E-E053-CC2BA8C0B5EA\"}";

            Interfaces.IYearBuiltData? yearBuiltData = Core.Convert.ToDiGi<Interfaces.IYearBuiltData>(json)?.FirstOrDefault();
            Assert.NotNull(yearBuiltData);

            Core.xUnit.Query.SerializationCheck(yearBuiltData);

            Assert.Equal(json, yearBuiltData.ToSystem_String());

            //Copy

            Classes.YearBuiltData? yearBuiltData_Source = yearBuiltData as Classes.YearBuiltData;
            Assert.NotNull(yearBuiltData_Source);

            Classes.YearBuiltData yearBuiltData_Copy = new(yearBuiltData_Source);

            Assert.Equal(yearBuiltData_Source.Reference, yearBuiltData_Copy.Reference);
            Assert.Equal(yearBuiltData_Source.UniqueId, yearBuiltData_Copy.UniqueId);
            Assert.Equal(json, yearBuiltData_Copy.ToSystem_String());
        }
    }
}
