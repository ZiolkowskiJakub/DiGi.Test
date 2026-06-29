using System.Collections.Generic;

namespace DiGi.EPW.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the construction, property values, and JSON serialization round-trip of the <see cref="Classes.HolidaysDaylightSaving"/> class, including the nested list of <see cref="Classes.Holiday"/> objects.
        /// </summary>
        [Fact]
        public void HolidaysDaylightSaving()
        {
            List<Classes.Holiday> holidays = [new("New Year's Day", "1/1"), new("Christmas Day", "12/25")];

            Classes.HolidaysDaylightSaving holidaysDaylightSaving = new(true, "4/ 7", "10/27", holidays);

            Assert.True(holidaysDaylightSaving.LeapYearObserved);
            Assert.Equal("4/ 7", holidaysDaylightSaving.DaylightSavingStartDate);
            Assert.Equal("10/27", holidaysDaylightSaving.DaylightSavingEndDate);
            Assert.Equal(2, holidaysDaylightSaving.Holidays?.Count);
            Assert.Equal("New Year's Day", holidaysDaylightSaving.Holidays?[0].Name);

            Core.xUnit.Query.SerializationCheck(holidaysDaylightSaving);

            Classes.HolidaysDaylightSaving holidaysDaylightSaving_NoHolidays = new(false, "0", "0", []);

            Assert.False(holidaysDaylightSaving_NoHolidays.LeapYearObserved);
            Assert.Empty(holidaysDaylightSaving_NoHolidays.Holidays!);

            Core.xUnit.Query.SerializationCheck(holidaysDaylightSaving_NoHolidays);
        }
    }
}
