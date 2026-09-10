using DiGi.Core.Classes;
using DiGi.Typology.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that two ranges sharing a Min collapse to one bucket, the later declaration winning.
        /// <para>Ranges are stored keyed on Range.Min, so a second range with the same Min replaces the first rather than joining it. This pins the behaviour rather than endorsing it - a caller declaring overlapping buckets silently loses one.</para>
        /// </summary>
        [Fact]
        public void RangeValueFilterRule_SharedMin()
        {
            IntegerRangeFilterRule integerRangeFilterRule = new([new Range<int>(0, 10), new Range<int>(0, 100)]);

            Assert.Single(integerRangeFilterRule.Ranges);

            RangeValueRuleData<int>? rangeValueRuleData = integerRangeFilterRule.RuleData(50);
            Assert.NotNull(rangeValueRuleData);
            Assert.NotNull(rangeValueRuleData.Range);
            Assert.Equal(0, rangeValueRuleData.Range.Min);
            Assert.Equal(100, rangeValueRuleData.Range.Max);
        }
    }
}
