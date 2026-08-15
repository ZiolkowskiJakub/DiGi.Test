using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a null filter admits every county row, which is what makes a national pass the default.
        /// </summary>
        [Fact]
        public void IsInScope_NoFilterAdmitsEverything()
        {
            Assert.True(Query.IsInScope(5, "0201", null, null));
            Assert.True(Query.IsInScope(0, null, null, null));
        }

        /// <summary>
        /// Verifies that a negative county row identifier is never in scope, whatever the filters say.
        /// </summary>
        [Fact]
        public void IsInScope_NegativeIdentifier()
        {
            Assert.False(Query.IsInScope(-1, "0201", null, null));
            Assert.False(Query.IsInScope(-1, "0201", null, ["02"]));
        }

        /// <summary>
        /// Verifies that a voivodeship code admits the counties whose code starts with it and rejects the rest.
        /// <para>The two-digit prefix of a county code is the voivodeship it belongs to. It is the only usable grouping key: every county part carries its own private ancestor chain, so a voivodeship row parents exactly one county row and cannot stand for the voivodeship.</para>
        /// </summary>
        [Fact]
        public void IsInScope_VoivodeshipPrefix()
        {
            HashSet<string> voivodeshipCodes = ["16"];

            Assert.True(Query.IsInScope(5, "1601", null, voivodeshipCodes));
            Assert.True(Query.IsInScope(7, "1661", null, voivodeshipCodes));
            Assert.False(Query.IsInScope(9, "0201", null, voivodeshipCodes));

            // A code longer than two digits that merely contains the voivodeship code is not in it.
            Assert.False(Query.IsInScope(11, "3216", null, voivodeshipCodes));
        }

        /// <summary>
        /// Verifies that several voivodeship codes are admitted together.
        /// </summary>
        [Fact]
        public void IsInScope_SeveralVoivodeships()
        {
            HashSet<string> voivodeshipCodes = ["16", "22"];

            Assert.True(Query.IsInScope(5, "1601", null, voivodeshipCodes));
            Assert.True(Query.IsInScope(6, "2212", null, voivodeshipCodes));
            Assert.False(Query.IsInScope(7, "2405", null, voivodeshipCodes));
        }

        /// <summary>
        /// Verifies that the two filters intersect rather than union - a county has to be admitted by both.
        /// </summary>
        [Fact]
        public void IsInScope_FiltersIntersect()
        {
            HashSet<int> countyIds = [73482, 73485];
            HashSet<string> voivodeshipCodes = ["22"];

            Assert.True(Query.IsInScope(73482, "2212", countyIds, voivodeshipCodes));

            // Named by identifier but in another voivodeship.
            Assert.False(Query.IsInScope(73485, "2405", countyIds, voivodeshipCodes));

            // In the voivodeship but not named by identifier.
            Assert.False(Query.IsInScope(73490, "2212", countyIds, voivodeshipCodes));
        }

        /// <summary>
        /// Verifies that a county row without a code is out of scope whenever a voivodeship filter is given, and unaffected by one that is not.
        /// <para>A row with no code cannot be placed in a voivodeship. Admitting it would file it into whichever voivodeship happened to be running.</para>
        /// </summary>
        [Fact]
        public void IsInScope_MissingCode()
        {
            Assert.False(Query.IsInScope(5, null, null, ["02"]));
            Assert.False(Query.IsInScope(5, "   ", null, ["02"]));
            Assert.True(Query.IsInScope(5, null, [5], null));
        }

        /// <summary>
        /// Verifies that a blank entry in the voivodeship filter is ignored rather than admitting every county.
        /// </summary>
        [Fact]
        public void IsInScope_BlankVoivodeshipCode()
        {
            Assert.False(Query.IsInScope(5, "0201", null, [string.Empty]));
            Assert.True(Query.IsInScope(5, "0201", null, [string.Empty, "02"]));
        }
    }
}
