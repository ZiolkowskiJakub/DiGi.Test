using DiGi.Core.Classes;
using DiGi.Typology.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that copying a range rule data preserves its range and yields an instance independent of the source.
        /// <para>The copy constructor previously chained to base and left the range null, so a copy reported no range at all.</para>
        /// <para>The deep-copy requirement is pinned by <c>Assert.NotSame</c>: <see cref="Range{T}"/> is mutable (its <c>Add</c> methods rewrite the bounds), so sharing the source instance would let a mutation to one copy corrupt the other.</para>
        /// </summary>
        [Fact]
        public void RangeValueRuleData_CopyConstructor()
        {
            RangeValueRuleData<int> source = new(new Range<int>(0, 100));
            RangeValueRuleData<int> copy = new(source);

            Assert.NotNull(copy.Range);
            Assert.Equal(source.Range, copy.Range);
            Assert.NotSame(source.Range, copy.Range);
        }
    }
}
