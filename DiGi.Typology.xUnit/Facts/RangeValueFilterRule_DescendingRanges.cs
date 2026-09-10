using DiGi.Core.Classes;
using DiGi.Typology.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that RangeValueFilterRule resolves a value to its bucket whatever order the ranges were declared in.
        /// <para>The rule keys its ranges on Range.Min and abandons the search as soon as a key exceeds the value, which is correct only while enumeration is ascending. Declared descending, every lookup abandoned on the first key and matched nothing.</para>
        /// </summary>
        [Fact]
        public void RangeValueFilterRule_DescendingRanges()
        {
            List<Range<int>> ranges_Ascending = [new Range<int>(0, 2003), new Range<int>(2004, 2020), new Range<int>(2021, int.MaxValue)];
            List<Range<int>> ranges_Descending = [new Range<int>(2021, int.MaxValue), new Range<int>(2004, 2020), new Range<int>(0, 2003)];

            IntegerRangeFilterRule integerRangeFilterRule_Ascending = new(ranges_Ascending);
            IntegerRangeFilterRule integerRangeFilterRule_Descending = new(ranges_Descending);

            AssertBucket(integerRangeFilterRule_Ascending, 2010, 2004, 2020);

            AssertBucket(integerRangeFilterRule_Descending, 1990, 0, 2003);
            AssertBucket(integerRangeFilterRule_Descending, 2010, 2004, 2020);
            AssertBucket(integerRangeFilterRule_Descending, 2025, 2021, int.MaxValue);

            static void AssertBucket(IntegerRangeFilterRule integerRangeFilterRule, int value, int min, int max)
            {
                RangeValueRuleData<int>? rangeValueRuleData = integerRangeFilterRule.RuleData(value);
                Assert.NotNull(rangeValueRuleData);
                Assert.NotNull(rangeValueRuleData.Range);
                Assert.Equal(min, rangeValueRuleData.Range.Min);
                Assert.Equal(max, rangeValueRuleData.Range.Max);
            }
        }
    }
}
