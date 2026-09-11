using DiGi.Core.Classes;
using DiGi.Typology.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that copying a range filter rule preserves its ranges, resolves every value to the same bucket as the source, and is instance-independent of the source.
        /// <para>The copy constructor previously chained to base and copied nothing, so a copy of a rule with two ranges resolved to no buckets at all.</para>
        /// <para>The deep-copy requirement is pinned by <c>Assert.NotSame</c>: each <see cref="Range{T}"/> in the copy is a distinct, mutable instance, so mutating one copy's range cannot corrupt the other's.</para>
        /// </summary>
        [Fact]
        public void RangeValueFilterRule_CopyConstructor()
        {
            IntegerRangeFilterRule source = new([new Range<int>(0, 100), new Range<int>(101, 200)]);
            IntegerRangeFilterRule copy = new(source);

            List<Range<int>> sourceRanges = [.. source.Ranges];
            List<Range<int>> copyRanges = [.. copy.Ranges];
            Assert.Equal(2, copyRanges.Count);

            foreach (Range<int> sourceRange in sourceRanges)
            {
                foreach (Range<int> copyRange in copyRanges)
                {
                    Assert.NotSame(sourceRange, copyRange);
                }
            }

            for (int value = 0; value <= 200; value++)
            {
                Assert.Equal(source.RuleData(value), copy.RuleData(value));
            }
        }
    }
}
