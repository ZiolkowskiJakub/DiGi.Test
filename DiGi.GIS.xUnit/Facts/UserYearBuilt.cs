using DiGi.Core;
using System;
using System.Linq;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Builds a user year built entry with every member populated, asserts each property, round-trips the string form, and runs the serialization check.
        /// <para>Populating every member is what makes the copy-constructor leg meaningful: a member the copy constructor forgot is caught only because this fact set it to something.</para>
        /// </summary>
        [Fact]
        public void UserYearBuilt()
        {
            DateTimeOffset dateTime = new(2025, 5, 29, 9, 41, 47, TimeSpan.FromHours(2));
            string userName = "reviewer@example.com";

            Classes.UserYearBuilt userYearBuilt = new((short)2008, Enums.YearBuiltRelation.AtOrBefore, dateTime, userName);

            Assert.Equal((short)2008, userYearBuilt.Year);
            Assert.Equal(Enums.YearBuiltRelation.AtOrBefore, userYearBuilt.YearBuiltRelation);
            Assert.Equal(dateTime, userYearBuilt.DateTime);
            Assert.Equal(userName, userYearBuilt.UserName);
            Assert.Equal(Enums.YearBuiltSource.User, userYearBuilt.YearBuiltSource);

            string? json = userYearBuilt.ToSystem_String();
            Assert.NotNull(json);

            Classes.UserYearBuilt? userYearBuilt_RoundTripped = Core.Convert.ToDiGi<Classes.UserYearBuilt>(json)?.FirstOrDefault();
            Assert.NotNull(userYearBuilt_RoundTripped);

            Assert.Equal(userYearBuilt.Year, userYearBuilt_RoundTripped.Year);
            Assert.Equal(userYearBuilt.YearBuiltRelation, userYearBuilt_RoundTripped.YearBuiltRelation);
            Assert.Equal(userYearBuilt.DateTime, userYearBuilt_RoundTripped.DateTime);
            Assert.Equal(userYearBuilt.UserName, userYearBuilt_RoundTripped.UserName);

            //Copy constructor: Clone() (used by SerializationCheck) is a JSON round-trip, not the copy constructor, so the copy leg needs its own direct assertion.
            Classes.UserYearBuilt userYearBuilt_Copy = new(userYearBuilt);

            Assert.Equal(userYearBuilt.Year, userYearBuilt_Copy.Year);
            Assert.Equal(userYearBuilt.YearBuiltRelation, userYearBuilt_Copy.YearBuiltRelation);
            Assert.Equal(userYearBuilt.DateTime, userYearBuilt_Copy.DateTime);
            Assert.Equal(userYearBuilt.UserName, userYearBuilt_Copy.UserName);

            Core.xUnit.Query.SerializationCheck(userYearBuilt);
        }

        /// <summary>
        /// Parses a user year built entry written before the relation and provenance members existed and asserts it deserializes as an exact year with null provenance.
        /// </summary>
        [Fact]
        public void UserYearBuilt_Legacy()
        {
            string json = "{\"_type\":\"DiGi.GIS.Classes.UserYearBuilt,DiGi.GIS\",\"Year\":1960}";

            Classes.UserYearBuilt? userYearBuilt = Core.Convert.ToDiGi<Classes.UserYearBuilt>(json)?.FirstOrDefault();
            Assert.NotNull(userYearBuilt);

            Assert.Equal(Enums.YearBuiltRelation.Exact, userYearBuilt.YearBuiltRelation);
            Assert.Null(userYearBuilt.DateTime);
            Assert.Null(userYearBuilt.UserName);
        }
    }
}
