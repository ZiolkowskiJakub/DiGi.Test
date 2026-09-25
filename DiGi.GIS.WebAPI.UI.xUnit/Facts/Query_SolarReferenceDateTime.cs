using System;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the mapping of EPW hour-ending time stamps to the mid-hour instants of the reference year the sun is sampled at.
        /// <para>Hour 1 of 1 January (01:00) covers 00:00-01:00 and maps to 00:30. Hour 24 of 31 December is stamped 1 January 00:00 of the next year and must still map to 31 December 23:30 of the reference year, not to 1 January. A typical meteorological year mixes calendar years, so every record lands in the one reference year. The 29 February hours of a leap year are skipped, including hour 24, which is stamped 1 March 00:00.</para>
        /// </summary>
        [Fact]
        public void Query_SolarReferenceDateTime()
        {
            int year = Constants.Default.SolarReferenceYear;
            Assert.False(DateTime.IsLeapYear(year));

            Assert.Equal(new DateTime(year, 1, 1, 0, 30, 0), Query.SolarReferenceDateTime(EPW.Convert.ToSystem_DateTime(1995, 1, 1, 1, 0)));
            Assert.Equal(new DateTime(year, 12, 31, 23, 30, 0), Query.SolarReferenceDateTime(EPW.Convert.ToSystem_DateTime(2002, 12, 31, 24, 0)));
            Assert.Equal(new DateTime(year, 6, 21, 11, 30, 0), Query.SolarReferenceDateTime(EPW.Convert.ToSystem_DateTime(1988, 6, 21, 12, 60)));

            Assert.Null(Query.SolarReferenceDateTime(EPW.Convert.ToSystem_DateTime(2004, 2, 29, 12, 0)));
            Assert.Null(Query.SolarReferenceDateTime(EPW.Convert.ToSystem_DateTime(2004, 2, 29, 24, 0)));
            Assert.Equal(new DateTime(year, 3, 1, 0, 30, 0), Query.SolarReferenceDateTime(EPW.Convert.ToSystem_DateTime(2004, 3, 1, 1, 0)));
        }
    }
}
