using DiGi.Typology.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that copying a unique value rule data preserves its wrapped value.
        /// <para>The copy constructor previously chained to base and left the value null, so a copy reported no value.</para>
        /// </summary>
        [Fact]
        public void UniqueValueRuleData_CopyConstructor()
        {
            UniqueValueRuleData source = new("test-value");
            UniqueValueRuleData copy = new(source);

            Assert.NotNull(copy.Value);
            Assert.Equal("test-value", copy.Value);
        }
    }
}
