using DiGi.Typology.Classes;
using DiGi.Typology.Interfaces;
using DiGi.Typology.Visual.Classes;

namespace DiGi.Typology.Visual.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests <see cref="Classes.VisualUniqueValueFilterRule"/>: a fresh rule holds an empty, never-null collection;
        /// entries filed through it survive a string round trip and SerializationCheck; the rule data the rule produces
        /// resolves its own appearance; the copy constructor clones the collection; and the rule still buckets exactly
        /// as its base does, both directly and through the reflective <see cref="Query.RuleData(ITypologyFilterRule, object)"/>
        /// - the type must not add a hiding <c>RuleData</c> member, or that lookup becomes ambiguous.
        /// </summary>
        [Fact]
        public void VisualUniqueValueFilterRule()
        {
            Classes.VisualUniqueValueFilterRule visualUniqueValueFilterRule = new();

            Assert.NotNull(visualUniqueValueFilterRule.TypologyAppearanceCollection);
            Assert.Equal(0, visualUniqueValueFilterRule.TypologyAppearanceCollection.Count);

            TypologyAppearance typologyAppearance_Residential = Create.TypologyAppearance(System.Drawing.Color.Red);
            TypologyAppearance typologyAppearance_2010 = Create.TypologyAppearance(System.Drawing.Color.Green);

            visualUniqueValueFilterRule.TypologyAppearanceCollection["Residential"] = typologyAppearance_Residential;
            visualUniqueValueFilterRule.TypologyAppearanceCollection[2010] = typologyAppearance_2010;

            // The rule buckets as its base does, and the rule data resolves its own appearance.
            UniqueValueRuleData? uniqueValueRuleData = visualUniqueValueFilterRule.RuleData(2010);

            Assert.NotNull(uniqueValueRuleData);
            Assert.Equal(2010, uniqueValueRuleData.Value);
            Assert.Same(typologyAppearance_2010, visualUniqueValueFilterRule.TypologyAppearanceCollection[uniqueValueRuleData]);
            Assert.Null(visualUniqueValueFilterRule.TypologyAppearanceCollection[visualUniqueValueFilterRule.RuleData("Industrial")]);

            ITypologyFilterRuleData? typologyFilterRuleData = Typology.Query.RuleData(visualUniqueValueFilterRule, "Residential");

            Assert.NotNull(typologyFilterRuleData);
            Assert.Same(typologyAppearance_Residential, visualUniqueValueFilterRule.TypologyAppearanceCollection[typologyFilterRuleData]);

            // The copy constructor clones the collection and its entries.
            Classes.VisualUniqueValueFilterRule visualUniqueValueFilterRule_Copy = new(visualUniqueValueFilterRule);

            Assert.NotSame(visualUniqueValueFilterRule.TypologyAppearanceCollection, visualUniqueValueFilterRule_Copy.TypologyAppearanceCollection);
            Assert.Equal(visualUniqueValueFilterRule.TypologyAppearanceCollection.Keys, visualUniqueValueFilterRule_Copy.TypologyAppearanceCollection.Keys);
            Assert.NotSame(typologyAppearance_2010, visualUniqueValueFilterRule_Copy.TypologyAppearanceCollection[2010]);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance_2010), Core.Convert.ToSystem_String(visualUniqueValueFilterRule_Copy.TypologyAppearanceCollection[2010]));

            // String round trip: the entries come back and the rule is still a Visual rule by its _type.
            string? json = Core.Convert.ToSystem_String(visualUniqueValueFilterRule);

            Assert.False(string.IsNullOrWhiteSpace(json));

            ITypologyFilterRule? typologyFilterRule_RoundTrip = Core.Convert.ToDiGi<ITypologyFilterRule>(json)?.FirstOrDefault();

            Classes.VisualUniqueValueFilterRule visualUniqueValueFilterRule_RoundTrip = Assert.IsType<Classes.VisualUniqueValueFilterRule>(typologyFilterRule_RoundTrip);

            Assert.Equal(["Residential", "2010"], visualUniqueValueFilterRule_RoundTrip.TypologyAppearanceCollection.Keys);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance_2010), Core.Convert.ToSystem_String(visualUniqueValueFilterRule_RoundTrip.TypologyAppearanceCollection[2010]));
            Assert.NotNull(visualUniqueValueFilterRule_RoundTrip.RuleData(2010));

            Core.xUnit.Query.SerializationCheck(visualUniqueValueFilterRule);
            Core.xUnit.Query.SerializationCheck(new Classes.VisualUniqueValueFilterRule());
        }
    }
}
