using DiGi.Core.Classes;
using DiGi.Typology.Classes;
using DiGi.Typology.Visual.Classes;
using DiGi.Typology.Visual.Interfaces;

namespace DiGi.Typology.Visual.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests <see cref="VisualRangeValueRuleData{TValueType}"/>: the constructor stores the range and the appearance
        /// by reference, the copy constructor clones both, equality, ordering and the hash follow the range alone, the
        /// text form is the closed interval, and the instance survives a string round trip and SerializationCheck with
        /// and without an appearance.
        /// </summary>
        [Fact]
        public void VisualRangeValueRuleData()
        {
            TypologyAppearance typologyAppearance = Create.TypologyAppearance(System.Drawing.Color.Red);
            Range<int> range = new(2004, 2020);

            VisualRangeValueRuleData<int> visualRangeValueRuleData = new(range, typologyAppearance);

            Assert.Same(range, visualRangeValueRuleData.Range);
            Assert.Same(typologyAppearance, visualRangeValueRuleData.Appearance);
            Assert.Same(typologyAppearance, Assert.IsAssignableFrom<IVisualTypologyFilterRuleData>(visualRangeValueRuleData).Appearance);
            Assert.Equal("[2004, 2020]", visualRangeValueRuleData.ToString());
            Assert.Equal("null", new VisualRangeValueRuleData<int>().ToString());

            // Equality and the hash consider the range alone; a plain RangeValueRuleData is a different type and not equal.
            VisualRangeValueRuleData<int> visualRangeValueRuleData_Bare = new(new Range<int>(2020, 2004), null);

            Assert.True(visualRangeValueRuleData.Equals(visualRangeValueRuleData_Bare));
            Assert.True(visualRangeValueRuleData.Equals((object)visualRangeValueRuleData_Bare));
            Assert.Equal(visualRangeValueRuleData.GetHashCode(), visualRangeValueRuleData_Bare.GetHashCode());
            Assert.False(visualRangeValueRuleData.Equals(new VisualRangeValueRuleData<int>(new Range<int>(2004, 2021), typologyAppearance)));
            Assert.False(visualRangeValueRuleData.Equals(new RangeValueRuleData<int>(range)));
            Assert.False(visualRangeValueRuleData.Equals((object?)null));
            Assert.True(new VisualRangeValueRuleData<int>().Equals(new VisualRangeValueRuleData<int>()));

            // The copy constructor clones the range and the appearance rather than aliasing them.
            VisualRangeValueRuleData<int> visualRangeValueRuleData_Copy = new(visualRangeValueRuleData);

            Assert.NotSame(range, visualRangeValueRuleData_Copy.Range);
            Assert.Equal(range, visualRangeValueRuleData_Copy.Range);
            Assert.NotNull(visualRangeValueRuleData_Copy.Appearance);
            Assert.NotSame(typologyAppearance, visualRangeValueRuleData_Copy.Appearance);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance), Core.Convert.ToSystem_String(visualRangeValueRuleData_Copy.Appearance));

            // The appearance is settable.
            visualRangeValueRuleData_Copy.Appearance = null;

            Assert.Null(visualRangeValueRuleData_Copy.Appearance);
            Assert.Same(typologyAppearance, visualRangeValueRuleData.Appearance);

            // String round trip.
            VisualRangeValueRuleData<int>? visualRangeValueRuleData_RoundTrip = Core.Convert.ToDiGi<VisualRangeValueRuleData<int>>(Core.Convert.ToSystem_String(visualRangeValueRuleData))?.FirstOrDefault();

            Assert.NotNull(visualRangeValueRuleData_RoundTrip);
            Assert.Equal(range, visualRangeValueRuleData_RoundTrip.Range);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance), Core.Convert.ToSystem_String(visualRangeValueRuleData_RoundTrip.Appearance));

            Core.xUnit.Query.SerializationCheck(visualRangeValueRuleData);
            Core.xUnit.Query.SerializationCheck(visualRangeValueRuleData_Bare);
            Core.xUnit.Query.SerializationCheck(new VisualRangeValueRuleData<double>(new Range<double>(0.5, 12.25), typologyAppearance));
        }
    }
}
