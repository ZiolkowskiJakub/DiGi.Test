using DiGi.Core.Classes;
using DiGi.Typology.Visual.Classes;

namespace DiGi.Typology.Visual.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests <see cref="VisualRange{T}"/>: the bounds order as in <see cref="Range{T}"/>, a plain range upgrades to
        /// a visual one, equality ignores the appearance, the copy constructor clones it, and both <c>VisualRange&lt;int&gt;</c>
        /// and <c>VisualRange&lt;double&gt;</c> survive a string round trip - their <c>_type</c> discriminator carries the
        /// generic argument - and SerializationCheck.
        /// </summary>
        [Fact]
        public void VisualRange()
        {
            TypologyAppearance typologyAppearance = Create.TypologyAppearance(System.Drawing.Color.Red);

            VisualRange<int> visualRange = new(2020, 2004, typologyAppearance);

            Assert.Equal(2004, visualRange.Min);
            Assert.Equal(2020, visualRange.Max);
            Assert.NotNull(visualRange.Appearance);
            Assert.NotSame(typologyAppearance, visualRange.Appearance);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance), Core.Convert.ToSystem_String(visualRange.Appearance));
            Assert.True(visualRange.In(2010));
            Assert.False(visualRange.In(2021));

            // A plain range upgrades to a visual one, and a visual range with no appearance is allowed.
            Range<int> range = new(0, 2003);
            VisualRange<int> visualRange_Upgraded = new(range, typologyAppearance);
            VisualRange<int> visualRange_Bare = new(2021, int.MaxValue, null);

            Assert.Equal(0, visualRange_Upgraded.Min);
            Assert.Equal(2003, visualRange_Upgraded.Max);
            Assert.NotNull(visualRange_Upgraded.Appearance);
            Assert.Null(visualRange_Bare.Appearance);

            // Equality is that of the bounds: the appearance is metadata.
            Assert.True(visualRange.Equals(new Range<int>(2004, 2020)));
            Assert.True(new Range<int>(2004, 2020).Equals(visualRange));
            Assert.True(visualRange.Equals(new VisualRange<int>(2004, 2020, null)));
            Assert.Equal(new Range<int>(2004, 2020).GetHashCode(), visualRange.GetHashCode());
            Assert.False(visualRange.Equals(visualRange_Upgraded));

            // The copy constructor clones the appearance rather than aliasing it.
            VisualRange<int> visualRange_Copy = new(visualRange);

            Assert.Equal(2004, visualRange_Copy.Min);
            Assert.Equal(2020, visualRange_Copy.Max);
            Assert.NotNull(visualRange_Copy.Appearance);
            Assert.NotSame(visualRange.Appearance, visualRange_Copy.Appearance);
            Assert.Equal(Core.Convert.ToSystem_String(visualRange.Appearance), Core.Convert.ToSystem_String(visualRange_Copy.Appearance));

            // The appearance is settable.
            visualRange_Copy.Appearance = null;

            Assert.Null(visualRange_Copy.Appearance);
            Assert.NotNull(visualRange.Appearance);

            // String round trip through the generic _type, for both closed types in use.
            string? json = Core.Convert.ToSystem_String(visualRange);

            Assert.False(string.IsNullOrWhiteSpace(json));

            // The discriminator carries the closed generic type; the JSON encoder escapes the backtick, so read it back parsed.
            string? fullTypeName = Core.Query.FullTypeName(System.Text.Json.Nodes.JsonNode.Parse(json)?.AsObject());

            Assert.Equal(Core.Query.FullTypeName(typeof(VisualRange<int>)), fullTypeName);
            Assert.Contains("VisualRange`1[[System.Int32", fullTypeName);
            Assert.EndsWith(",DiGi.Typology.Visual", fullTypeName);

            VisualRange<int>? visualRange_RoundTrip = Core.Convert.ToDiGi<VisualRange<int>>(json)?.FirstOrDefault();

            Assert.NotNull(visualRange_RoundTrip);
            Assert.Equal(2004, visualRange_RoundTrip.Min);
            Assert.Equal(2020, visualRange_RoundTrip.Max);
            Assert.NotNull(visualRange_RoundTrip.Appearance);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance), Core.Convert.ToSystem_String(visualRange_RoundTrip.Appearance));

            Core.xUnit.Query.SerializationCheck(visualRange);
            Core.xUnit.Query.SerializationCheck(visualRange_Bare);

            VisualRange<double> visualRange_Double = new(0.5, 12.25, typologyAppearance);
            VisualRange<double>? visualRange_Double_RoundTrip = Core.Convert.ToDiGi<VisualRange<double>>(Core.Convert.ToSystem_String(visualRange_Double))?.FirstOrDefault();

            Assert.NotNull(visualRange_Double_RoundTrip);
            Assert.Equal(0.5, visualRange_Double_RoundTrip.Min);
            Assert.Equal(12.25, visualRange_Double_RoundTrip.Max);
            Assert.NotNull(visualRange_Double_RoundTrip.Appearance);

            Core.xUnit.Query.SerializationCheck(visualRange_Double);
        }
    }
}
