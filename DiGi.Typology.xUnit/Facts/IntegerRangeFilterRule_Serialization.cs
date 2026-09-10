using DiGi.Core.Classes;
using DiGi.Typology.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that IntegerRangeFilterRule round-trips through its string form and passes SerializationCheck, and that its ranges are held in ascending Min order whatever order they were declared in.
        /// <para>Ranges are stored keyed on Min, so their order is a property of the store rather than of the caller's declaration. A round-trip that dropped or reordered one would change which bucket a value resolves to, and the rule abandons its search as soon as a key exceeds the value.</para>
        /// </summary>
        [Fact]
        public void IntegerRangeFilterRule_Serialization()
        {
            IntegerRangeFilterRule integerRangeFilterRule = new([new Range<int>(2021, int.MaxValue), new Range<int>(0, 2003), new Range<int>(2004, 2020)]);

            AssertAscending(integerRangeFilterRule);

            string? text = Core.Convert.ToSystem_String(integerRangeFilterRule);
            Assert.False(string.IsNullOrWhiteSpace(text));

            IntegerRangeFilterRule? integerRangeFilterRule_Parsed = Core.Convert.ToDiGi<IntegerRangeFilterRule>(text)?.FirstOrDefault();
            Assert.NotNull(integerRangeFilterRule_Parsed);

            AssertAscending(integerRangeFilterRule_Parsed);

            RangeValueRuleData<int>? rangeValueRuleData = integerRangeFilterRule_Parsed.RuleData(2010);
            Assert.NotNull(rangeValueRuleData);
            Assert.NotNull(rangeValueRuleData.Range);
            Assert.Equal(2004, rangeValueRuleData.Range.Min);
            Assert.Equal(2020, rangeValueRuleData.Range.Max);

            Core.xUnit.Query.SerializationCheck(integerRangeFilterRule);

            static void AssertAscending(IntegerRangeFilterRule integerRangeFilterRule)
            {
                List<Range<int>> ranges = [.. integerRangeFilterRule.Ranges];

                Assert.Equal(3, ranges.Count);
                Assert.Equal(0, ranges[0].Min);
                Assert.Equal(2003, ranges[0].Max);
                Assert.Equal(2004, ranges[1].Min);
                Assert.Equal(2020, ranges[1].Max);
                Assert.Equal(2021, ranges[2].Min);
                Assert.Equal(int.MaxValue, ranges[2].Max);
            }
        }
    }
}
