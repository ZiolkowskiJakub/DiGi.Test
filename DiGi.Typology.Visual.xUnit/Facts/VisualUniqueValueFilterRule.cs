using DiGi.Typology.Interfaces;
using DiGi.Typology.Visual.Classes;
using DiGi.Typology.Visual.Interfaces;

namespace DiGi.Typology.Visual.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests <see cref="Classes.VisualUniqueValueFilterRule"/>: the appearance collection is filed by value, the rule
        /// buckets every value and hands the bucket's appearance to the <see cref="VisualUniqueValueRuleData"/> it
        /// produces (by reference, null where none is filed), the reflection path of
        /// <see cref="Typology.Query.RuleData(ITypologyFilterRule, object)"/> reaches it, the copy constructor clones the
        /// collection and its entries, and the rule survives a string round trip and SerializationCheck.
        /// </summary>
        [Fact]
        public void VisualUniqueValueFilterRule()
        {
            Classes.VisualUniqueValueFilterRule visualUniqueValueFilterRule = new();

            Assert.IsAssignableFrom<IVisualTypologyFilterRule>(visualUniqueValueFilterRule);
            Assert.IsAssignableFrom<ITypologyFilterRule<VisualUniqueValueRuleData>>(visualUniqueValueFilterRule);
            Assert.NotNull(visualUniqueValueFilterRule.TypologyAppearanceCollection);
            Assert.Equal(0, visualUniqueValueFilterRule.TypologyAppearanceCollection.Count);

            TypologyAppearance typologyAppearance_Residential = Create.TypologyAppearance(System.Drawing.Color.Red);
            TypologyAppearance typologyAppearance_2010 = Create.TypologyAppearance(System.Drawing.Color.Green);

            visualUniqueValueFilterRule.TypologyAppearanceCollection["Residential"] = typologyAppearance_Residential;
            visualUniqueValueFilterRule.TypologyAppearanceCollection[2010] = typologyAppearance_2010;

            // The rule buckets by value and the rule data carries the very appearance filed on the rule.
            VisualUniqueValueRuleData? visualUniqueValueRuleData = visualUniqueValueFilterRule.RuleData(2010);

            Assert.NotNull(visualUniqueValueRuleData);
            Assert.Equal(2010, visualUniqueValueRuleData.Value);
            Assert.Same(typologyAppearance_2010, visualUniqueValueRuleData.Appearance);
            Assert.Same(typologyAppearance_2010, visualUniqueValueFilterRule.TypologyAppearanceCollection[visualUniqueValueRuleData]);

            // An unmapped value still gets a bucket, with no appearance; so does null.
            VisualUniqueValueRuleData? visualUniqueValueRuleData_Unmapped = visualUniqueValueFilterRule.RuleData("Industrial");

            Assert.NotNull(visualUniqueValueRuleData_Unmapped);
            Assert.Equal("Industrial", visualUniqueValueRuleData_Unmapped.Value);
            Assert.Null(visualUniqueValueRuleData_Unmapped.Appearance);
            Assert.Null(visualUniqueValueFilterRule.RuleData(null)?.Value);
            Assert.Null(visualUniqueValueFilterRule.RuleData(null)?.Appearance);

            // Two rule data of one bucket are equal whatever their appearance, which is how a solver groups them.
            Assert.Equal(visualUniqueValueRuleData, new VisualUniqueValueRuleData(2010));
            Assert.Equal(visualUniqueValueRuleData.GetHashCode(), new VisualUniqueValueRuleData(2010).GetHashCode());
            Assert.NotEqual(visualUniqueValueRuleData, visualUniqueValueRuleData_Unmapped);

            // The reflection path the solver takes reaches the Visual rule data.
            ITypologyFilterRuleData? typologyFilterRuleData = Typology.Query.RuleData(visualUniqueValueFilterRule, "Residential");

            VisualUniqueValueRuleData visualUniqueValueRuleData_Reflection = Assert.IsType<VisualUniqueValueRuleData>(typologyFilterRuleData);

            Assert.Same(typologyAppearance_Residential, visualUniqueValueRuleData_Reflection.Appearance);
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
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance_2010), Core.Convert.ToSystem_String(visualUniqueValueFilterRule_RoundTrip.RuleData(2010)?.Appearance));

            Core.xUnit.Query.SerializationCheck(visualUniqueValueFilterRule);
            Core.xUnit.Query.SerializationCheck(new Classes.VisualUniqueValueFilterRule());
        }
    }
}
