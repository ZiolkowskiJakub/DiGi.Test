using DiGi.Core.Classes;
using DiGi.Typology.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that ranges meeting at a boundary hand the boundary to the upper one: a value equal to a range's Min
        /// belongs to that range, so every range another one follows on matches [Min, Max) and its rule data says so,
        /// while the last range (nothing starts at its Max) and a single-value range keep Max inclusive.
        /// <para>Before, matching was closed on both ends and the lower range won a shared boundary, which forced the
        /// definition editor to keep consecutive ranges strictly apart (an epsilon gap on doubles).</para>
        /// </summary>
        [Fact]
        public void RangeValueFilterRule_TouchingRanges()
        {
            DoubleRangeFilterRule doubleRangeFilterRule = new([new Range<double>(0, 11.38), new Range<double>(11.38, 20.1), new Range<double>(20.1, 22.1)]);

            // A shared boundary belongs to the range starting at it.
            AssertBucket(doubleRangeFilterRule.RuleData(11.38), 11.38, 20.1, true, "[11.38, 20.1)");
            AssertBucket(doubleRangeFilterRule.RuleData(20.1), 20.1, 22.1, false, "[20.1, 22.1]");

            // Inside a range nothing changes; the lower range still reads as half-open because the next one starts at its Max.
            AssertBucket(doubleRangeFilterRule.RuleData(5.0), 0, 11.38, true, "[0, 11.38)");
            AssertBucket(doubleRangeFilterRule.RuleData(0), 0, 11.38, true, "[0, 11.38)");

            // The last range keeps its Max: 22.1 is inside it, and beyond it there is no bucket.
            AssertBucket(doubleRangeFilterRule.RuleData(22.1), 20.1, 22.1, false, "[20.1, 22.1]");
            Assert.Null(doubleRangeFilterRule.RuleData(22.2));

            Assert.True(doubleRangeFilterRule.MaxExclusive(new Range<double>(0, 11.38)));
            Assert.False(doubleRangeFilterRule.MaxExclusive(new Range<double>(20.1, 22.1)));
            Assert.False(doubleRangeFilterRule.MaxExclusive(null));

            // Ranges kept apart stay closed on both ends, as before.
            IntegerRangeFilterRule integerRangeFilterRule = new([new Range<int>(0, 2), new Range<int>(3, 5), new Range<int>(5, 5)]);

            AssertBucket(integerRangeFilterRule.RuleData(2), 0, 2, false, "[0, 2]");
            AssertBucket(integerRangeFilterRule.RuleData(3), 3, 5, true, "[3, 5)");
            // A single-value range starting at the previous Max takes that value and is not exclusive of itself.
            AssertBucket(integerRangeFilterRule.RuleData(5), 5, 5, false, "[5, 5]");

            static void AssertBucket<T>(RangeValueRuleData<T>? rangeValueRuleData, T min, T max, bool maxExclusive, string text) where T : System.IComparable<T>
            {
                Assert.NotNull(rangeValueRuleData);
                Assert.NotNull(rangeValueRuleData.Range);
                Assert.Equal(min, rangeValueRuleData.Range.Min);
                Assert.Equal(max, rangeValueRuleData.Range.Max);
                Assert.Equal(maxExclusive, rangeValueRuleData.MaxExclusive);
                Assert.Equal(text, rangeValueRuleData.ToString());
            }
        }
    }
}
