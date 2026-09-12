using DiGi.Typology.Classes;
using DiGi.Typology.Interfaces;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that UniqueValueFilterRule round-trips through its string form, is rebuilt as the same type from the
        /// polymorphic rule interface, passes SerializationCheck, and that the copy constructor yields a rule that
        /// buckets exactly as the source does.
        /// <para>The rule is stateless, so the constructors carry nothing of their own; they exist so that a derived rule
        /// carrying state can chain them, and this fact pins the base behaviour they must preserve.</para>
        /// </summary>
        [Fact]
        public void UniqueValueFilterRule_Serialization()
        {
            UniqueValueFilterRule uniqueValueFilterRule = new();

            string? text = Core.Convert.ToSystem_String(uniqueValueFilterRule);
            Assert.False(string.IsNullOrWhiteSpace(text));

            ITypologyFilterRule? typologyFilterRule_Parsed = Core.Convert.ToDiGi<ITypologyFilterRule>(text)?.FirstOrDefault();
            UniqueValueFilterRule uniqueValueFilterRule_Parsed = Assert.IsType<UniqueValueFilterRule>(typologyFilterRule_Parsed);

            UniqueValueRuleData? uniqueValueRuleData = uniqueValueFilterRule_Parsed.RuleData("Residential");
            Assert.NotNull(uniqueValueRuleData);
            Assert.Equal("Residential", uniqueValueRuleData.Value);
            Assert.Equal(uniqueValueFilterRule.RuleData("Residential"), uniqueValueRuleData);
            Assert.NotEqual(uniqueValueFilterRule.RuleData("Industrial"), uniqueValueRuleData);

            UniqueValueFilterRule uniqueValueFilterRule_Copy = new(uniqueValueFilterRule);
            Assert.Equal(uniqueValueFilterRule.RuleData(2010), uniqueValueFilterRule_Copy.RuleData(2010));
            Assert.Equal(uniqueValueFilterRule.RuleData(null), uniqueValueFilterRule_Copy.RuleData(null));

            // A bare rule data wrapping a whole number survives the clone leg (an object member holding a boxed int),
            // the surface of ZiolkowskiJakub/DiGi.Core#6.
            Core.xUnit.Query.SerializationCheck(new UniqueValueRuleData(2010));
            Core.xUnit.Query.SerializationCheck(uniqueValueFilterRule);
        }
    }
}
