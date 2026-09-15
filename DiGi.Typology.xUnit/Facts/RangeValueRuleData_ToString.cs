using DiGi.Core.Classes;
using DiGi.Typology.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a range rule data's rendered name describes the interval it actually matches.
        /// <para>Ranges kept apart are closed on both ends, so both boundary values resolve to the bucket and the name must say so - the old rendering `(min,max>` claimed the minimum was excluded even though it matched. Where the next range starts exactly at this one's Max, that value resolves to the next range and the name closes with `)` instead.</para>
        /// </summary>
        [Fact]
        public void RangeValueRuleData_ToString()
        {
            IntegerRangeFilterRule integerRangeFilterRule = new([new Range<int>(0, 2003), new Range<int>(2004, 2020)]);

            // Both boundary values resolve to the bucket - the interval is closed on both ends.
            RangeValueRuleData<int>? rangeValueRuleData_Min = integerRangeFilterRule.RuleData(0);
            Assert.NotNull(rangeValueRuleData_Min);

            RangeValueRuleData<int>? rangeValueRuleData_Max = integerRangeFilterRule.RuleData(2003);
            Assert.NotNull(rangeValueRuleData_Max);

            // The rendered name must describe the closed interval the rule actually matches.
            Assert.Equal("[0, 2003]", rangeValueRuleData_Min.ToString());
            Assert.Equal("[0, 2003]", rangeValueRuleData_Max.ToString());

            // Touching ranges: the shared value is the upper range's, and the lower one's name says its Max is excluded.
            IntegerRangeFilterRule integerRangeFilterRule_Touching = new([new Range<int>(0, 2003), new Range<int>(2003, 2020)]);

            Assert.Equal("[0, 2003)", integerRangeFilterRule_Touching.RuleData(1990)?.ToString());
            Assert.Equal("[2003, 2020]", integerRangeFilterRule_Touching.RuleData(2003)?.ToString());
        }
    }
}
