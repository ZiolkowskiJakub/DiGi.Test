using DiGi.Core;
using System;
using System.Linq;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Builds a year built data object holding one predicted entry and one bounded user entry, verifies the user entry is retrieved with its relation, and checks serialization.
        /// <para>The copy is checked as well: a stored year built row is addressed by the reference of the building it describes together with the unique identifier of the object itself, so a copy that reissues either one can no longer be matched to the row it came from.</para>
        /// </summary>
        [Fact]
        public void YearBuiltData()
        {
            string reference = "272D6AAF-9D86-9B0E-E053-CC2BA8C0B5EA";
            Classes.YearBuiltData yearBuiltData = new(reference);

            Assert.True(yearBuiltData.SetPredictedYearBuilt(new DateTime(2025, 5, 29, 9, 41, 47), (short)2008));

            Classes.UserYearBuilt userYearBuilt = new((short)2008, Enums.YearBuiltRelation.AtOrBefore, new DateTimeOffset(2025, 5, 29, 9, 41, 47, TimeSpan.FromHours(2)), "reviewer@example.com");
            Assert.True(yearBuiltData.SetUserYearBuilt(userYearBuilt));

            Classes.UserYearBuilt? userYearBuilt_Read = yearBuiltData.GetUserYearBuilt();
            Assert.NotNull(userYearBuilt_Read);
            Assert.Equal(Enums.YearBuiltRelation.AtOrBefore, userYearBuilt_Read.YearBuiltRelation);
            Assert.Equal(userYearBuilt.DateTime, userYearBuilt_Read.DateTime);
            Assert.Equal(userYearBuilt.UserName, userYearBuilt_Read.UserName);

            string? json = yearBuiltData.ToSystem_String();
            Assert.NotNull(json);
            Assert.Contains("\"YearBuiltRelation\":1", json);

            Core.xUnit.Query.SerializationCheck(yearBuiltData);

            //Copy

            Classes.YearBuiltData yearBuiltData_Copy = new(yearBuiltData);

            Assert.Equal(yearBuiltData.Reference, yearBuiltData_Copy.Reference);
            Assert.Equal(yearBuiltData.UniqueId, yearBuiltData_Copy.UniqueId);

            Classes.UserYearBuilt? userYearBuilt_Copy = yearBuiltData_Copy.GetUserYearBuilt();
            Assert.NotNull(userYearBuilt_Copy);
            Assert.Equal(Enums.YearBuiltRelation.AtOrBefore, userYearBuilt_Copy.YearBuiltRelation);
            Assert.Equal(userYearBuilt.UserName, userYearBuilt_Copy.UserName);
        }
    }
}
