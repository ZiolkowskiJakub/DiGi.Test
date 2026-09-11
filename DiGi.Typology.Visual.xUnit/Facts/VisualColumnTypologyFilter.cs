using DiGi.Core.IO.Table.Classes;
using DiGi.Typology.Classes;
using DiGi.Typology.Visual.Interfaces;

namespace DiGi.Typology.Visual.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests <see cref="Classes.VisualColumnTypologyFilter"/>: the value, the rule and a nested filter all survive a
        /// string round trip and SerializationCheck, the copy constructor clones the chain, the nested filter is typed
        /// Visual so the chain cannot degrade to plain levels, and the type is a member of the Visual family by its
        /// marker interface and by its <c>_type</c>.
        /// </summary>
        [Fact]
        public void VisualColumnTypologyFilter()
        {
            Classes.VisualColumnTypologyFilter visualColumnTypologyFilter = new()
            {
                Value = new Column(0, "occupancy", typeof(string)),
                Rule = new UniqueValueFilterRule(),
                Filter = new Classes.VisualColumnTypologyFilter()
                {
                    Value = new Column(1, "year_built", typeof(int)),
                    Rule = new IntegerRangeFilterRule([new Core.Classes.Range<int>(0, 2003), new Core.Classes.Range<int>(2004, int.MaxValue)])
                }
            };

            Assert.IsAssignableFrom<ITypologyVisualSerializableObject>(visualColumnTypologyFilter);
            Assert.IsAssignableFrom<TypologyFilter<Classes.VisualColumnTypologyFilter<Column>, Column>>(visualColumnTypologyFilter);
            Assert.Equal("occupancy", visualColumnTypologyFilter.Value?.Name);
            Assert.IsType<Classes.VisualColumnTypologyFilter>(visualColumnTypologyFilter.Filter);

            // The copy constructor clones the chain below it.
            Classes.VisualColumnTypologyFilter visualColumnTypologyFilter_Copy = new(visualColumnTypologyFilter);

            Assert.NotSame(visualColumnTypologyFilter.Filter, visualColumnTypologyFilter_Copy.Filter);
            Assert.IsType<Classes.VisualColumnTypologyFilter>(visualColumnTypologyFilter_Copy.Filter);
            Assert.Equal("year_built", visualColumnTypologyFilter_Copy.Filter?.Value?.Name);

            // String round trip.
            string? json = Core.Convert.ToSystem_String(visualColumnTypologyFilter);

            Assert.False(string.IsNullOrWhiteSpace(json));

            Classes.VisualColumnTypologyFilter? visualColumnTypologyFilter_RoundTrip = Core.Convert.ToDiGi<Classes.VisualColumnTypologyFilter>(json)?.FirstOrDefault();

            Assert.NotNull(visualColumnTypologyFilter_RoundTrip);
            Assert.Equal("occupancy", visualColumnTypologyFilter_RoundTrip.Value?.Name);
            Assert.IsType<UniqueValueFilterRule>(visualColumnTypologyFilter_RoundTrip.Rule);
            Assert.IsType<Classes.VisualColumnTypologyFilter>(visualColumnTypologyFilter_RoundTrip.Filter);
            Assert.IsType<IntegerRangeFilterRule>(visualColumnTypologyFilter_RoundTrip.Filter?.Rule);

            Core.xUnit.Query.SerializationCheck(visualColumnTypologyFilter);
        }
    }
}
