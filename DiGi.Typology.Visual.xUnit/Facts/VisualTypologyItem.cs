using DiGi.Typology.Classes;
using DiGi.Typology.Visual.Classes;

namespace DiGi.Typology.Visual.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests <see cref="Classes.VisualTypologyItem"/>: the constructors clone the appearance, the appearance takes part
        /// in equality, ordering and the hash (an absent appearance orders first, a plain <see cref="TypologyItem"/> orders
        /// before an equal-valued visual item), the appearance is settable, the re-path constructor keeps it, the copy
        /// constructor clones it, and the item survives a string round trip and SerializationCheck.
        /// </summary>
        [Fact]
        public void VisualTypologyItem()
        {
            TypologyAppearance typologyAppearance_Red = Create.TypologyAppearance(System.Drawing.Color.Red);
            TypologyAppearance typologyAppearance_Blue = Create.TypologyAppearance(System.Drawing.Color.Blue);

            Classes.VisualTypologyItem visualTypologyItem_1 = new([0, 2], "Residential", "Dwellings", typologyAppearance_Red);
            Classes.VisualTypologyItem visualTypologyItem_2 = new(new TypologyPath([0, 2]), "Residential", "Dwellings", typologyAppearance_Red);
            Classes.VisualTypologyItem visualTypologyItem_Blue = new([0, 2], "Residential", "Dwellings", typologyAppearance_Blue);
            Classes.VisualTypologyItem visualTypologyItem_Bare = new([0, 2], "Residential", "Dwellings", null);

            Assert.Equal("Residential", visualTypologyItem_1.Name);
            Assert.Equal("Dwellings", visualTypologyItem_1.Description);
            Assert.Equal(2, visualTypologyItem_1.TypologyPath?.Index);
            Assert.NotNull(visualTypologyItem_1.Appearance);
            Assert.NotSame(typologyAppearance_Red, visualTypologyItem_1.Appearance);
            Assert.Null(visualTypologyItem_Bare.Appearance);

            // The appearance takes part in equality, ordering and the hash.
            Assert.True(visualTypologyItem_1.Equals(visualTypologyItem_2));
            Assert.True(visualTypologyItem_1 == visualTypologyItem_2);
            Assert.Equal(0, visualTypologyItem_1.CompareTo(visualTypologyItem_2));
            Assert.Equal(visualTypologyItem_1.GetHashCode(), visualTypologyItem_2.GetHashCode());

            Assert.False(visualTypologyItem_1.Equals(visualTypologyItem_Blue));
            Assert.True(visualTypologyItem_1 != visualTypologyItem_Blue);
            Assert.NotEqual(0, visualTypologyItem_1.CompareTo(visualTypologyItem_Blue));
            Assert.Equal(-System.Math.Sign(visualTypologyItem_Blue.CompareTo(visualTypologyItem_1)), System.Math.Sign(visualTypologyItem_1.CompareTo(visualTypologyItem_Blue)));
            Assert.NotEqual(visualTypologyItem_1.GetHashCode(), visualTypologyItem_Blue.GetHashCode());

            // An absent appearance orders first; a plain item orders before an equal-valued visual item.
            Assert.True(visualTypologyItem_Bare.CompareTo(visualTypologyItem_1) < 0);
            Assert.True(visualTypologyItem_1.CompareTo(visualTypologyItem_Bare) > 0);
            Assert.False(visualTypologyItem_Bare.Equals(visualTypologyItem_1));

            TypologyItem typologyItem_Plain = new([0, 2], "Residential", "Dwellings");

            Assert.True(visualTypologyItem_Bare.CompareTo(typologyItem_Plain) > 0);
            Assert.False(visualTypologyItem_Bare.Equals(typologyItem_Plain));

            // Different path or name still differs, whatever the appearance.
            Assert.False(visualTypologyItem_1.Equals(new Classes.VisualTypologyItem([0, 3], "Residential", "Dwellings", typologyAppearance_Red)));
            Assert.False(visualTypologyItem_1.Equals(new Classes.VisualTypologyItem([0, 2], "Industrial", "Dwellings", typologyAppearance_Red)));

            // The appearance is settable, like the name and the description.
            visualTypologyItem_Bare.Appearance = typologyAppearance_Red;

            Assert.True(visualTypologyItem_Bare == visualTypologyItem_1);

            visualTypologyItem_Bare.Appearance = null;

            Assert.True(visualTypologyItem_Bare != visualTypologyItem_1);

            // The re-path constructor keeps the name, the description and the appearance.
            Classes.VisualTypologyItem visualTypologyItem_RePathed = new(new TypologyPath([5]), visualTypologyItem_1);

            Assert.Equal(5, visualTypologyItem_RePathed.TypologyPath?.Index);
            Assert.Equal("Residential", visualTypologyItem_RePathed.Name);
            Assert.Equal("Dwellings", visualTypologyItem_RePathed.Description);
            Assert.NotNull(visualTypologyItem_RePathed.Appearance);
            Assert.NotSame(visualTypologyItem_1.Appearance, visualTypologyItem_RePathed.Appearance);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance_Red), Core.Convert.ToSystem_String(visualTypologyItem_RePathed.Appearance));

            // The copy constructor clones the appearance rather than aliasing it.
            Classes.VisualTypologyItem visualTypologyItem_Copy = new(visualTypologyItem_1);

            Assert.True(visualTypologyItem_Copy == visualTypologyItem_1);
            Assert.NotSame(visualTypologyItem_1.Appearance, visualTypologyItem_Copy.Appearance);

            // String round trip.
            Classes.VisualTypologyItem? visualTypologyItem_RoundTrip = Core.Convert.ToDiGi<Classes.VisualTypologyItem>(Core.Convert.ToSystem_String(visualTypologyItem_1))?.FirstOrDefault();

            Assert.NotNull(visualTypologyItem_RoundTrip);
            Assert.True(visualTypologyItem_RoundTrip == visualTypologyItem_1);
            Assert.NotNull(visualTypologyItem_RoundTrip.TypologyPath);
            Assert.Equal([0, 2], visualTypologyItem_RoundTrip.TypologyPath);
            Assert.NotNull(visualTypologyItem_RoundTrip.Appearance);

            Core.xUnit.Query.SerializationCheck(visualTypologyItem_1);
            Core.xUnit.Query.SerializationCheck(visualTypologyItem_Bare);
        }
    }
}
