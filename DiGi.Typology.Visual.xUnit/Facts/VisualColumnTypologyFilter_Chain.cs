using DiGi.Core.Classes;
using DiGi.Core.IO.Table.Classes;
using DiGi.Typology.Classes;
using DiGi.Typology.Visual.Classes;

namespace DiGi.Typology.Visual.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests a whole Visual definition chain through a string round trip: a level with a node appearance and a
        /// <see cref="VisualUniqueValueFilterRule"/> mapping two values, nesting a level whose
        /// <see cref="IntegerRangeFilterRule"/> holds two <see cref="VisualRange{T}"/> and one plain <see cref="Range{T}"/>.
        /// <para>Asserted, not measured: after the round trip the ranges are still held in ascending order, each visual
        /// range is still a <c>VisualRange&lt;int&gt;</c> carrying its appearance while the plain one is still plain, the
        /// rule still buckets a value onto the very range instance the definition holds, the mapped values still resolve
        /// their appearance, and the nested level is still Visual by its <c>_type</c>.</para>
        /// </summary>
        [Fact]
        public void VisualColumnTypologyFilter_Chain()
        {
            TypologyAppearance typologyAppearance_Node = Create.TypologyAppearance(System.Drawing.Color.Gray);
            TypologyAppearance typologyAppearance_Residential = Create.TypologyAppearance(System.Drawing.Color.Red);
            TypologyAppearance typologyAppearance_Industrial = Create.TypologyAppearance(System.Drawing.Color.Purple);
            TypologyAppearance typologyAppearance_Old = Create.TypologyAppearance(System.Drawing.Color.Brown);
            TypologyAppearance typologyAppearance_New = Create.TypologyAppearance(System.Drawing.Color.Green);

            Classes.VisualUniqueValueFilterRule visualUniqueValueFilterRule = new();

            visualUniqueValueFilterRule.TypologyAppearanceCollection["Residential"] = typologyAppearance_Residential;
            visualUniqueValueFilterRule.TypologyAppearanceCollection["Industrial"] = typologyAppearance_Industrial;

            IntegerRangeFilterRule integerRangeFilterRule = new([new VisualRange<int>(2021, int.MaxValue, typologyAppearance_New), new Range<int>(2004, 2020), new VisualRange<int>(0, 2003, typologyAppearance_Old)]);

            Classes.VisualColumnTypologyFilter visualColumnTypologyFilter = new()
            {
                Value = new Column(0, "occupancy", typeof(string)),
                Rule = visualUniqueValueFilterRule,
                Appearance = typologyAppearance_Node,
                Filter = new Classes.VisualColumnTypologyFilter()
                {
                    Value = new Column(1, "year_built", typeof(int)),
                    Rule = integerRangeFilterRule
                }
            };

            string? json = Core.Convert.ToSystem_String(visualColumnTypologyFilter);

            Assert.False(string.IsNullOrWhiteSpace(json));

            Classes.VisualColumnTypologyFilter? visualColumnTypologyFilter_RoundTrip = Core.Convert.ToDiGi<Classes.VisualColumnTypologyFilter>(json)?.FirstOrDefault();

            Assert.NotNull(visualColumnTypologyFilter_RoundTrip);
            Assert.NotNull(visualColumnTypologyFilter_RoundTrip.Appearance);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance_Node), Core.Convert.ToSystem_String(visualColumnTypologyFilter_RoundTrip.Appearance));

            // Level 1: the unique value rule and its mapped values.
            Classes.VisualUniqueValueFilterRule visualUniqueValueFilterRule_RoundTrip = Assert.IsType<Classes.VisualUniqueValueFilterRule>(visualColumnTypologyFilter_RoundTrip.Rule);

            Assert.Equal(["Residential", "Industrial"], visualUniqueValueFilterRule_RoundTrip.TypologyAppearanceCollection.Keys);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance_Industrial), Core.Convert.ToSystem_String(visualUniqueValueFilterRule_RoundTrip.TypologyAppearanceCollection[visualUniqueValueFilterRule_RoundTrip.RuleData("Industrial")]));
            Assert.Null(visualUniqueValueFilterRule_RoundTrip.TypologyAppearanceCollection["Agricultural"]);

            // Level 2: the nested level is Visual by its _type, and the ranges are still ascending with their kinds intact.
            Classes.VisualColumnTypologyFilter visualColumnTypologyFilter_Nested = Assert.IsType<Classes.VisualColumnTypologyFilter>(visualColumnTypologyFilter_RoundTrip.Filter);

            Assert.Null(visualColumnTypologyFilter_Nested.Appearance);
            Assert.Equal("year_built", visualColumnTypologyFilter_Nested.Value?.Name);

            IntegerRangeFilterRule integerRangeFilterRule_RoundTrip = Assert.IsType<IntegerRangeFilterRule>(visualColumnTypologyFilter_Nested.Rule);

            List<Range<int>> ranges = [.. integerRangeFilterRule_RoundTrip.Ranges];

            Assert.Equal(3, ranges.Count);
            Assert.Equal([0, 2004, 2021], ranges.ConvertAll(x => x.Min));

            VisualRange<int> visualRange_Old = Assert.IsType<VisualRange<int>>(ranges[0]);
            VisualRange<int> visualRange_New = Assert.IsType<VisualRange<int>>(ranges[2]);

            Assert.IsType<Range<int>>(ranges[1]);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance_Old), Core.Convert.ToSystem_String(visualRange_Old.Appearance));
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance_New), Core.Convert.ToSystem_String(visualRange_New.Appearance));

            // The rule still buckets, onto the very instance the definition holds - which is how a solver reads the appearance.
            RangeValueRuleData<int>? rangeValueRuleData = integerRangeFilterRule_RoundTrip.RuleData(1995);

            Assert.NotNull(rangeValueRuleData);
            Assert.Same(visualRange_Old, rangeValueRuleData.Range);
            Assert.Same(ranges[1], integerRangeFilterRule_RoundTrip.RuleData(2010)?.Range);
            Assert.Null(integerRangeFilterRule_RoundTrip.RuleData(-1));

            Core.xUnit.Query.SerializationCheck(visualColumnTypologyFilter);
        }
    }
}
