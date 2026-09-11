using DiGi.Core.Classes;
using DiGi.Core.IO.Table.Classes;
using DiGi.Typology.Classes;
using DiGi.Typology.Interfaces;
using DiGi.Typology.Visual.Classes;

namespace DiGi.Typology.Visual.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests a whole Visual definition chain through a string round trip: a level with a
        /// <see cref="VisualUniqueValueFilterRule"/> mapping two values, nesting a level whose
        /// <see cref="VisualIntegerRangeFilterRule"/> holds three ranges and maps two of them.
        /// <para>Asserted, not measured: after the round trip the ranges are still held in ascending order, the mapped
        /// ranges and values still resolve their appearance and the unmapped ones resolve none, each rule still buckets a
        /// value onto the very range instance the definition holds and hands the bucket's appearance to its rule data,
        /// the reflection path of <see cref="Typology.Query.RuleData(ITypologyFilterRule, object)"/> reaches the Visual
        /// rule data, and the nested level is still Visual by its <c>_type</c>.</para>
        /// </summary>
        [Fact]
        public void VisualColumnTypologyFilter_Chain()
        {
            TypologyAppearance typologyAppearance_Residential = Create.TypologyAppearance(System.Drawing.Color.Red);
            TypologyAppearance typologyAppearance_Industrial = Create.TypologyAppearance(System.Drawing.Color.Purple);
            TypologyAppearance typologyAppearance_Old = Create.TypologyAppearance(System.Drawing.Color.Brown);
            TypologyAppearance typologyAppearance_New = Create.TypologyAppearance(System.Drawing.Color.Green);

            Classes.VisualUniqueValueFilterRule visualUniqueValueFilterRule = new();

            visualUniqueValueFilterRule.TypologyAppearanceCollection["Residential"] = typologyAppearance_Residential;
            visualUniqueValueFilterRule.TypologyAppearanceCollection["Industrial"] = typologyAppearance_Industrial;

            Range<int> range_New = new(2021, int.MaxValue);
            Range<int> range_Middle = new(2004, 2020);
            Range<int> range_Old = new(0, 2003);

            VisualIntegerRangeFilterRule visualIntegerRangeFilterRule = new([range_New, range_Middle, range_Old]);

            visualIntegerRangeFilterRule.TypologyAppearanceCollection[range_New] = typologyAppearance_New;
            visualIntegerRangeFilterRule.TypologyAppearanceCollection[range_Old] = typologyAppearance_Old;

            Classes.VisualColumnTypologyFilter visualColumnTypologyFilter = new()
            {
                Value = new Column(0, "occupancy", typeof(string)),
                Rule = visualUniqueValueFilterRule,
                Filter = new Classes.VisualColumnTypologyFilter()
                {
                    Value = new Column(1, "year_built", typeof(int)),
                    Rule = visualIntegerRangeFilterRule
                }
            };

            string? json = Core.Convert.ToSystem_String(visualColumnTypologyFilter);

            Assert.False(string.IsNullOrWhiteSpace(json));

            Classes.VisualColumnTypologyFilter? visualColumnTypologyFilter_RoundTrip = Core.Convert.ToDiGi<Classes.VisualColumnTypologyFilter>(json)?.FirstOrDefault();

            Assert.NotNull(visualColumnTypologyFilter_RoundTrip);

            // Level 1: the unique value rule and its mapped values.
            Classes.VisualUniqueValueFilterRule visualUniqueValueFilterRule_RoundTrip = Assert.IsType<Classes.VisualUniqueValueFilterRule>(visualColumnTypologyFilter_RoundTrip.Rule);

            Assert.Equal(["Residential", "Industrial"], visualUniqueValueFilterRule_RoundTrip.TypologyAppearanceCollection.Keys);

            VisualUniqueValueRuleData? visualUniqueValueRuleData = visualUniqueValueFilterRule_RoundTrip.RuleData("Industrial");

            Assert.NotNull(visualUniqueValueRuleData);
            Assert.Equal("Industrial", visualUniqueValueRuleData.Value);
            Assert.Same(visualUniqueValueFilterRule_RoundTrip.TypologyAppearanceCollection["Industrial"], visualUniqueValueRuleData.Appearance);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance_Industrial), Core.Convert.ToSystem_String(visualUniqueValueRuleData.Appearance));
            Assert.Null(visualUniqueValueFilterRule_RoundTrip.RuleData("Agricultural")?.Appearance);
            Assert.Null(visualUniqueValueFilterRule_RoundTrip.TypologyAppearanceCollection["Agricultural"]);

            // Level 2: the nested level is Visual by its _type, and the ranges are still ascending.
            Classes.VisualColumnTypologyFilter visualColumnTypologyFilter_Nested = Assert.IsType<Classes.VisualColumnTypologyFilter>(visualColumnTypologyFilter_RoundTrip.Filter);

            Assert.Equal("year_built", visualColumnTypologyFilter_Nested.Value?.Name);

            VisualIntegerRangeFilterRule visualIntegerRangeFilterRule_RoundTrip = Assert.IsType<VisualIntegerRangeFilterRule>(visualColumnTypologyFilter_Nested.Rule);

            List<Range<int>> ranges = [.. visualIntegerRangeFilterRule_RoundTrip.Ranges];

            Assert.Equal(3, ranges.Count);
            Assert.Equal([0, 2004, 2021], ranges.ConvertAll(x => x.Min));
            Assert.Equal(2, visualIntegerRangeFilterRule_RoundTrip.TypologyAppearanceCollection.Count);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance_Old), Core.Convert.ToSystem_String(visualIntegerRangeFilterRule_RoundTrip.TypologyAppearanceCollection[range_Old]));
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance_New), Core.Convert.ToSystem_String(visualIntegerRangeFilterRule_RoundTrip.TypologyAppearanceCollection[ranges[2]]));
            Assert.Null(visualIntegerRangeFilterRule_RoundTrip.TypologyAppearanceCollection[range_Middle]);

            // The rule still buckets, onto the very instance the definition holds, and hands the bucket's appearance to the rule data.
            VisualRangeValueRuleData<int>? visualRangeValueRuleData_Old = visualIntegerRangeFilterRule_RoundTrip.RuleData(1995);
            VisualRangeValueRuleData<int>? visualRangeValueRuleData_Middle = visualIntegerRangeFilterRule_RoundTrip.RuleData(2010);

            Assert.NotNull(visualRangeValueRuleData_Old);
            Assert.NotNull(visualRangeValueRuleData_Middle);
            Assert.Same(ranges[0], visualRangeValueRuleData_Old.Range);
            Assert.Same(visualIntegerRangeFilterRule_RoundTrip.TypologyAppearanceCollection[range_Old], visualRangeValueRuleData_Old.Appearance);
            Assert.Same(ranges[1], visualRangeValueRuleData_Middle.Range);
            Assert.Null(visualRangeValueRuleData_Middle.Appearance);
            Assert.Null(visualIntegerRangeFilterRule_RoundTrip.RuleData(-1));

            // The reflection path the solver takes reaches the Visual rule data of both rules.
            Assert.IsType<VisualUniqueValueRuleData>(Typology.Query.RuleData(visualUniqueValueFilterRule_RoundTrip, "Residential"));
            Assert.IsType<VisualRangeValueRuleData<int>>(Typology.Query.RuleData(visualIntegerRangeFilterRule_RoundTrip, 2021));
            Assert.NotNull((Typology.Query.RuleData(visualIntegerRangeFilterRule_RoundTrip, 2021) as VisualRangeValueRuleData<int>)?.Appearance);

            Core.xUnit.Query.SerializationCheck(visualColumnTypologyFilter);
        }
    }
}
