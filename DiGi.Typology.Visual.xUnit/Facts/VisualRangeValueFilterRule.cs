using DiGi.Core.Classes;
using DiGi.Typology.Interfaces;
using DiGi.Typology.Visual.Classes;
using DiGi.Typology.Visual.Interfaces;

namespace DiGi.Typology.Visual.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests <see cref="VisualRangeValueFilterRule{TValueType}"/> through <see cref="VisualIntegerRangeFilterRule"/>
        /// and <see cref="VisualDoubleRangeFilterRule"/>: the ranges are filed by minimum and enumerated ascending, the
        /// appearance collection is filed by range, the rule buckets a value onto the very range instance it holds and
        /// hands the bucket's appearance to the <see cref="VisualRangeValueRuleData{TValueType}"/> it produces (by
        /// reference, null where none is filed), a value outside every range gets no bucket, the reflection path of
        /// <see cref="Typology.Query.RuleData(ITypologyFilterRule, object)"/> reaches it, the copy constructor clones the
        /// ranges and the appearances, and the rule survives a string round trip and SerializationCheck.
        /// </summary>
        [Fact]
        public void VisualRangeValueFilterRule()
        {
            TypologyAppearance typologyAppearance_Old = Create.TypologyAppearance(System.Drawing.Color.Brown);
            TypologyAppearance typologyAppearance_New = Create.TypologyAppearance(System.Drawing.Color.Green);

            Range<int> range_New = new(2021, int.MaxValue);
            Range<int> range_Middle = new(2004, 2020);
            Range<int> range_Old = new(0, 2003);

            VisualIntegerRangeFilterRule visualIntegerRangeFilterRule = new([range_New, null!, range_Middle]);

            Assert.IsAssignableFrom<IVisualTypologyFilterRule>(visualIntegerRangeFilterRule);
            Assert.IsAssignableFrom<ITypologyFilterRule<VisualRangeValueRuleData<int>>>(visualIntegerRangeFilterRule);
            Assert.True(visualIntegerRangeFilterRule.Add(range_Old));
            Assert.False(visualIntegerRangeFilterRule.Add(null));
            Assert.Equal(0, visualIntegerRangeFilterRule.TypologyAppearanceCollection.Count);

            List<Range<int>> ranges = [.. visualIntegerRangeFilterRule.Ranges];

            Assert.Equal(3, ranges.Count);
            Assert.Same(range_Old, ranges[0]);
            Assert.Same(range_Middle, ranges[1]);
            Assert.Same(range_New, ranges[2]);

            visualIntegerRangeFilterRule.TypologyAppearanceCollection[range_Old] = typologyAppearance_Old;
            visualIntegerRangeFilterRule.TypologyAppearanceCollection[range_New] = typologyAppearance_New;

            Assert.Equal(2, visualIntegerRangeFilterRule.TypologyAppearanceCollection.Count);
            Assert.True(visualIntegerRangeFilterRule.TypologyAppearanceCollection.Contains(new Range<int>(2003, 0)));
            Assert.False(visualIntegerRangeFilterRule.TypologyAppearanceCollection.Contains(range_Middle));

            // The rule buckets onto the range instance it holds, with the appearance filed for it, and converts the value first.
            VisualRangeValueRuleData<int>? visualRangeValueRuleData_Old = visualIntegerRangeFilterRule.RuleData(1995);
            VisualRangeValueRuleData<int>? visualRangeValueRuleData_Middle = visualIntegerRangeFilterRule.RuleData("2010");
            VisualRangeValueRuleData<int>? visualRangeValueRuleData_New = visualIntegerRangeFilterRule.RuleData(2021L);

            Assert.NotNull(visualRangeValueRuleData_Old);
            Assert.NotNull(visualRangeValueRuleData_Middle);
            Assert.NotNull(visualRangeValueRuleData_New);
            Assert.Same(range_Old, visualRangeValueRuleData_Old.Range);
            Assert.Same(typologyAppearance_Old, visualRangeValueRuleData_Old.Appearance);
            Assert.Same(range_Middle, visualRangeValueRuleData_Middle.Range);
            Assert.Null(visualRangeValueRuleData_Middle.Appearance);
            Assert.Same(range_New, visualRangeValueRuleData_New.Range);
            Assert.Same(typologyAppearance_New, visualRangeValueRuleData_New.Appearance);
            Assert.Same(typologyAppearance_Old, visualIntegerRangeFilterRule.TypologyAppearanceCollection[visualRangeValueRuleData_Old]);

            // Both bounds are inclusive; outside every range, or not convertible, there is no bucket.
            Assert.Same(range_Old, visualIntegerRangeFilterRule.RuleData(2003)?.Range);
            Assert.Same(range_Middle, visualIntegerRangeFilterRule.RuleData(2004)?.Range);
            Assert.Null(visualIntegerRangeFilterRule.RuleData(-1));
            Assert.Null(visualIntegerRangeFilterRule.RuleData(null));
            Assert.Null(visualIntegerRangeFilterRule.RuleData("year"));

            // Two rule data of one bucket are equal whatever their appearance, which is how a solver groups them.
            Assert.Equal(visualRangeValueRuleData_Old, new VisualRangeValueRuleData<int>(new Range<int>(0, 2003), null));
            Assert.Equal(visualRangeValueRuleData_Old.GetHashCode(), new VisualRangeValueRuleData<int>(new Range<int>(0, 2003), null).GetHashCode());
            Assert.NotEqual(visualRangeValueRuleData_Old, visualRangeValueRuleData_Middle);

            // The reflection path the solver takes reaches the Visual rule data.
            VisualRangeValueRuleData<int> visualRangeValueRuleData_Reflection = Assert.IsType<VisualRangeValueRuleData<int>>(Typology.Query.RuleData(visualIntegerRangeFilterRule, 2030));

            Assert.Same(range_New, visualRangeValueRuleData_Reflection.Range);
            Assert.Same(typologyAppearance_New, visualRangeValueRuleData_Reflection.Appearance);

            // The copy constructor clones the ranges and the appearances rather than aliasing them.
            VisualIntegerRangeFilterRule visualIntegerRangeFilterRule_Copy = new(visualIntegerRangeFilterRule);

            List<Range<int>> ranges_Copy = [.. visualIntegerRangeFilterRule_Copy.Ranges];

            Assert.Equal(3, ranges_Copy.Count);
            Assert.Equal([0, 2004, 2021], ranges_Copy.ConvertAll(x => x.Min));
            Assert.NotSame(range_Old, ranges_Copy[0]);
            Assert.NotSame(visualIntegerRangeFilterRule.TypologyAppearanceCollection, visualIntegerRangeFilterRule_Copy.TypologyAppearanceCollection);
            Assert.Equal(visualIntegerRangeFilterRule.TypologyAppearanceCollection.Keys, visualIntegerRangeFilterRule_Copy.TypologyAppearanceCollection.Keys);
            Assert.NotSame(typologyAppearance_Old, visualIntegerRangeFilterRule_Copy.TypologyAppearanceCollection[range_Old]);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance_Old), Core.Convert.ToSystem_String(visualIntegerRangeFilterRule_Copy.TypologyAppearanceCollection[range_Old]));
            Assert.Same(ranges_Copy[0], visualIntegerRangeFilterRule_Copy.RuleData(1995)?.Range);
            Assert.NotNull(visualIntegerRangeFilterRule_Copy.RuleData(1995)?.Appearance);

            // String round trip: the ranges and the entries come back and the rule is still a Visual rule by its _type.
            string? json = Core.Convert.ToSystem_String(visualIntegerRangeFilterRule);

            Assert.False(string.IsNullOrWhiteSpace(json));

            ITypologyFilterRule? typologyFilterRule_RoundTrip = Core.Convert.ToDiGi<ITypologyFilterRule>(json)?.FirstOrDefault();

            VisualIntegerRangeFilterRule visualIntegerRangeFilterRule_RoundTrip = Assert.IsType<VisualIntegerRangeFilterRule>(typologyFilterRule_RoundTrip);

            Assert.Equal([0, 2004, 2021], new List<Range<int>>(visualIntegerRangeFilterRule_RoundTrip.Ranges).ConvertAll(x => x.Min));
            Assert.Equal(visualIntegerRangeFilterRule.TypologyAppearanceCollection.Keys, visualIntegerRangeFilterRule_RoundTrip.TypologyAppearanceCollection.Keys);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance_New), Core.Convert.ToSystem_String(visualIntegerRangeFilterRule_RoundTrip.RuleData(2021)?.Appearance));

            Core.xUnit.Query.SerializationCheck(visualIntegerRangeFilterRule);
            Core.xUnit.Query.SerializationCheck(new VisualIntegerRangeFilterRule());

            // The double rule keys its ranges invariantly, so an entry filed under one culture is found under another.
            Range<double> range_Double = new(0.5, 12.25);

            VisualDoubleRangeFilterRule visualDoubleRangeFilterRule = new([range_Double, new Range<double>(12.5, 100)]);

            visualDoubleRangeFilterRule.TypologyAppearanceCollection[range_Double] = typologyAppearance_Old;

            Assert.Equal(["[0.5, 12.25]"], visualDoubleRangeFilterRule.TypologyAppearanceCollection.Keys);

            System.Globalization.CultureInfo cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("pl-PL");

                Assert.Same(typologyAppearance_Old, visualDoubleRangeFilterRule.RuleData(1.5)?.Appearance);
                Assert.Same(typologyAppearance_Old, visualDoubleRangeFilterRule.TypologyAppearanceCollection[new Range<double>(12.25, 0.5)]);
                Assert.Null(visualDoubleRangeFilterRule.RuleData(50)?.Appearance);
                Assert.Null(visualDoubleRangeFilterRule.RuleData(12.3));
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = cultureInfo;
            }

            VisualDoubleRangeFilterRule? visualDoubleRangeFilterRule_RoundTrip = Core.Convert.ToDiGi<VisualDoubleRangeFilterRule>(Core.Convert.ToSystem_String(visualDoubleRangeFilterRule))?.FirstOrDefault();

            Assert.NotNull(visualDoubleRangeFilterRule_RoundTrip);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance_Old), Core.Convert.ToSystem_String(visualDoubleRangeFilterRule_RoundTrip.RuleData(0.5)?.Appearance));

            Core.xUnit.Query.SerializationCheck(visualDoubleRangeFilterRule);
        }
    }
}
