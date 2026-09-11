using DiGi.Typology.Classes;
using DiGi.Typology.Visual.Classes;

namespace DiGi.Typology.Visual.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests <see cref="Classes.TypologyAppearanceCollection"/>: entries filed for a string, an integer, a decimal and
        /// the NULL bucket resolve by the value in any width, by its string form, by the rule data produced for it and by
        /// the key itself; assigning null removes; the copy constructor clones every entry; and every entry still
        /// resolves by its original value after a string round trip.
        /// <para>The round-trip leg is the reason the keys are strings: an object-keyed dictionary comes back from JSON
        /// with string keys, so an entry filed under the integer 2010 would no longer be found by 2010.</para>
        /// </summary>
        [Fact]
        public void TypologyAppearanceCollection()
        {
            Classes.TypologyAppearance typologyAppearance_Residential = Create.TypologyAppearance(System.Drawing.Color.Red);
            Classes.TypologyAppearance typologyAppearance_2010 = Create.TypologyAppearance(System.Drawing.Color.Green);
            Classes.TypologyAppearance typologyAppearance_Area = Create.TypologyAppearance(System.Drawing.Color.Blue);
            Classes.TypologyAppearance typologyAppearance_Null = Create.TypologyAppearance(System.Drawing.Color.Gray);

            Classes.TypologyAppearanceCollection typologyAppearanceCollection = new();

            Assert.Equal(0, typologyAppearanceCollection.Count);
            Assert.Null(typologyAppearanceCollection["Residential"]);
            Assert.False(typologyAppearanceCollection.Contains("Residential"));

            typologyAppearanceCollection["Residential"] = typologyAppearance_Residential;
            typologyAppearanceCollection[2010] = typologyAppearance_2010;
            typologyAppearanceCollection[12.5m] = typologyAppearance_Area;
            typologyAppearanceCollection[null] = typologyAppearance_Null;

            Assert.Equal(4, typologyAppearanceCollection.Count);
            Assert.Equal(["Residential", "2010", "12.5", "null"], typologyAppearanceCollection.Keys);
            Assert.Equal(4, typologyAppearanceCollection.Appearances.Count);

            // The same entry by the value in any width, by its string form, by its rule data and by its key.
            Assert.Same(typologyAppearance_2010, typologyAppearanceCollection[2010]);
            Assert.Same(typologyAppearance_2010, typologyAppearanceCollection["2010"]);
            Assert.Same(typologyAppearance_2010, typologyAppearanceCollection[2010L]);
            Assert.Same(typologyAppearance_2010, typologyAppearanceCollection[(short)2010]);
            Assert.Same(typologyAppearance_2010, typologyAppearanceCollection[new UniqueValueRuleData(2010)]);
            Assert.Same(typologyAppearance_2010, typologyAppearanceCollection[Classes.TypologyAppearanceCollection.Key(2010)]);
            Assert.True(typologyAppearanceCollection.Contains(2010L));

            // A decimal of another scale is the same value, so it finds the same entry.
            Assert.Same(typologyAppearance_Area, typologyAppearanceCollection[12.50m]);
            Assert.Same(typologyAppearance_Area, typologyAppearanceCollection["12.5"]);

            // The NULL bucket.
            Assert.Same(typologyAppearance_Null, typologyAppearanceCollection[null]);
            Assert.Same(typologyAppearance_Null, typologyAppearanceCollection[new UniqueValueRuleData((object?)null)]);
            Assert.True(typologyAppearanceCollection.Contains(null));

            // Assigning null removes.
            typologyAppearanceCollection[12.5m] = null;

            Assert.Equal(3, typologyAppearanceCollection.Count);
            Assert.Null(typologyAppearanceCollection[12.5m]);
            Assert.False(typologyAppearanceCollection.Contains("12.5"));

            typologyAppearanceCollection["missing"] = null;

            Assert.Equal(3, typologyAppearanceCollection.Count);

            // The copy constructor clones every entry under its key.
            Classes.TypologyAppearanceCollection typologyAppearanceCollection_Copy = new(typologyAppearanceCollection);

            Assert.Equal(typologyAppearanceCollection.Keys, typologyAppearanceCollection_Copy.Keys);
            Assert.NotSame(typologyAppearance_2010, typologyAppearanceCollection_Copy[2010]);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance_2010), Core.Convert.ToSystem_String(typologyAppearanceCollection_Copy[2010]));
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearanceCollection), Core.Convert.ToSystem_String(typologyAppearanceCollection_Copy));

            // After a string round trip every entry still resolves by its original value, in the same order.
            string? json = Core.Convert.ToSystem_String(typologyAppearanceCollection);

            Assert.False(string.IsNullOrWhiteSpace(json));

            Classes.TypologyAppearanceCollection? typologyAppearanceCollection_RoundTrip = Core.Convert.ToDiGi<Classes.TypologyAppearanceCollection>(json)?.FirstOrDefault();

            Assert.NotNull(typologyAppearanceCollection_RoundTrip);
            Assert.Equal(["Residential", "2010", "null"], typologyAppearanceCollection_RoundTrip.Keys);
            Assert.NotNull(typologyAppearanceCollection_RoundTrip["Residential"]);
            Assert.NotNull(typologyAppearanceCollection_RoundTrip[2010]);
            Assert.NotNull(typologyAppearanceCollection_RoundTrip[null]);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance_2010), Core.Convert.ToSystem_String(typologyAppearanceCollection_RoundTrip[2010]));

            Core.xUnit.Query.SerializationCheck(typologyAppearanceCollection);
        }
    }
}
