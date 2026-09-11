using DiGi.Core.IO.Table.Classes;
using DiGi.Typology.Classes;
using DiGi.Typology.Visual.Classes;

namespace DiGi.Typology.Visual.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests <see cref="Classes.VisualColumnTypologyFilter"/>: the level appearance, the value, the rule and a nested
        /// filter all survive a string round trip and SerializationCheck, the copy constructor clones the appearance, and
        /// the type is a <see cref="ColumnTypologyFilter{UColumn}"/> so a consumer typed on the base accepts it.
        /// </summary>
        [Fact]
        public void VisualColumnTypologyFilter()
        {
            TypologyAppearance typologyAppearance = Create.TypologyAppearance(System.Drawing.Color.Red);

            Classes.VisualColumnTypologyFilter visualColumnTypologyFilter = new()
            {
                Value = new Column(0, "occupancy", typeof(string)),
                Rule = new UniqueValueFilterRule(),
                Appearance = typologyAppearance,
                Filter = new Classes.VisualColumnTypologyFilter()
                {
                    Value = new Column(1, "year_built", typeof(int)),
                    Rule = new IntegerRangeFilterRule([new Core.Classes.Range<int>(0, 2003), new Core.Classes.Range<int>(2004, int.MaxValue)])
                }
            };

            ColumnTypologyFilter<Column> columnTypologyFilter = visualColumnTypologyFilter;

            Assert.Same(typologyAppearance, visualColumnTypologyFilter.Appearance);
            Assert.Equal("occupancy", columnTypologyFilter.Value?.Name);
            Assert.NotNull(columnTypologyFilter.Filter);
            Assert.Null((columnTypologyFilter.Filter as Classes.VisualColumnTypologyFilter)?.Appearance);

            // The copy constructor clones the appearance and the chain below it.
            Classes.VisualColumnTypologyFilter visualColumnTypologyFilter_Copy = new(visualColumnTypologyFilter);

            Assert.NotNull(visualColumnTypologyFilter_Copy.Appearance);
            Assert.NotSame(typologyAppearance, visualColumnTypologyFilter_Copy.Appearance);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance), Core.Convert.ToSystem_String(visualColumnTypologyFilter_Copy.Appearance));
            Assert.NotSame(visualColumnTypologyFilter.Filter, visualColumnTypologyFilter_Copy.Filter);
            Assert.IsType<Classes.VisualColumnTypologyFilter>(visualColumnTypologyFilter_Copy.Filter);
            Assert.Equal("year_built", visualColumnTypologyFilter_Copy.Filter?.Value?.Name);

            // String round trip.
            string? json = Core.Convert.ToSystem_String(visualColumnTypologyFilter);

            Assert.False(string.IsNullOrWhiteSpace(json));

            Classes.VisualColumnTypologyFilter? visualColumnTypologyFilter_RoundTrip = Core.Convert.ToDiGi<Classes.VisualColumnTypologyFilter>(json)?.FirstOrDefault();

            Assert.NotNull(visualColumnTypologyFilter_RoundTrip);
            Assert.NotNull(visualColumnTypologyFilter_RoundTrip.Appearance);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance), Core.Convert.ToSystem_String(visualColumnTypologyFilter_RoundTrip.Appearance));
            Assert.Equal("occupancy", visualColumnTypologyFilter_RoundTrip.Value?.Name);
            Assert.IsType<UniqueValueFilterRule>(visualColumnTypologyFilter_RoundTrip.Rule);
            Assert.IsType<Classes.VisualColumnTypologyFilter>(visualColumnTypologyFilter_RoundTrip.Filter);
            Assert.IsType<IntegerRangeFilterRule>(visualColumnTypologyFilter_RoundTrip.Filter?.Rule);

            Core.xUnit.Query.SerializationCheck(visualColumnTypologyFilter);
        }
    }
}
