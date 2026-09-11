using DiGi.Typology.Classes;
using DiGi.Typology.Visual.Classes;
using DiGi.Typology.Visual.Interfaces;

namespace DiGi.Typology.Visual.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests <see cref="Classes.VisualUniqueValueRuleData"/>: the constructors store the value and the appearance by
        /// reference, the copy constructor shares the value and clones the appearance, equality and the hash follow the
        /// value alone, the text form is the value's, and the instance survives a string round trip and
        /// SerializationCheck with and without an appearance.
        /// </summary>
        [Fact]
        public void VisualUniqueValueRuleData()
        {
            TypologyAppearance typologyAppearance = Create.TypologyAppearance(System.Drawing.Color.Red);

            Classes.VisualUniqueValueRuleData visualUniqueValueRuleData = new("Residential", typologyAppearance);

            Assert.Equal("Residential", visualUniqueValueRuleData.Value);
            Assert.Same(typologyAppearance, visualUniqueValueRuleData.Appearance);
            Assert.Same(typologyAppearance, Assert.IsAssignableFrom<IVisualTypologyFilterRuleData>(visualUniqueValueRuleData).Appearance);
            Assert.Equal("Residential", visualUniqueValueRuleData.ToString());
            Assert.Null(new Classes.VisualUniqueValueRuleData("Industrial").Appearance);
            Assert.Equal("null", new Classes.VisualUniqueValueRuleData().ToString());

            // Equality and the hash consider the value alone; a plain UniqueValueRuleData is a different type and not equal.
            Classes.VisualUniqueValueRuleData visualUniqueValueRuleData_Bare = new("Residential");

            Assert.True(visualUniqueValueRuleData.Equals(visualUniqueValueRuleData_Bare));
            Assert.True(visualUniqueValueRuleData.Equals((object)visualUniqueValueRuleData_Bare));
            Assert.Equal(visualUniqueValueRuleData.GetHashCode(), visualUniqueValueRuleData_Bare.GetHashCode());
            Assert.False(visualUniqueValueRuleData.Equals(new Classes.VisualUniqueValueRuleData("Industrial", typologyAppearance)));
            Assert.False(visualUniqueValueRuleData.Equals(new UniqueValueRuleData("Residential")));
            Assert.False(visualUniqueValueRuleData.Equals((object?)null));
            Assert.True(new Classes.VisualUniqueValueRuleData().Equals(new Classes.VisualUniqueValueRuleData((object?)null, typologyAppearance)));

            // The copy constructor shares the value and clones the appearance.
            Classes.VisualUniqueValueRuleData visualUniqueValueRuleData_Copy = new(visualUniqueValueRuleData);

            Assert.Same(visualUniqueValueRuleData.Value, visualUniqueValueRuleData_Copy.Value);
            Assert.NotNull(visualUniqueValueRuleData_Copy.Appearance);
            Assert.NotSame(typologyAppearance, visualUniqueValueRuleData_Copy.Appearance);
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance), Core.Convert.ToSystem_String(visualUniqueValueRuleData_Copy.Appearance));

            // The appearance is settable.
            visualUniqueValueRuleData_Copy.Appearance = null;

            Assert.Null(visualUniqueValueRuleData_Copy.Appearance);
            Assert.Same(typologyAppearance, visualUniqueValueRuleData.Appearance);

            // String round trip. An object-typed int comes back as a double, so it is no longer Equals-equal to the int it
            // was, while its key is unchanged - which is what the appearance collection relies on.
            Classes.VisualUniqueValueRuleData visualUniqueValueRuleData_Int = new(2010, typologyAppearance);

            Classes.VisualUniqueValueRuleData? visualUniqueValueRuleData_RoundTrip = Core.Convert.ToDiGi<Classes.VisualUniqueValueRuleData>(Core.Convert.ToSystem_String(visualUniqueValueRuleData_Int))?.FirstOrDefault();

            Assert.NotNull(visualUniqueValueRuleData_RoundTrip);
            Assert.NotEqual(2010, visualUniqueValueRuleData_RoundTrip.Value);
            Assert.Equal(Query.Key(2010), Query.Key(visualUniqueValueRuleData_RoundTrip.Value));
            Assert.Equal(Query.Key(2010), Query.Key(visualUniqueValueRuleData_RoundTrip));
            Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance), Core.Convert.ToSystem_String(visualUniqueValueRuleData_RoundTrip.Appearance));

            Core.xUnit.Query.SerializationCheck(visualUniqueValueRuleData);
            Core.xUnit.Query.SerializationCheck(visualUniqueValueRuleData_Bare);

            // TODO [ObjectMemberClone]: add SerializationCheck(visualUniqueValueRuleData_Int) once ZiolkowskiJakub/DiGi.Core#6 ships and Query.Value reads a
            // CLR-backed JsonValue holding a boxed int under an object member - today Clone() throws on it (the same holds for
            // the base UniqueValueRuleData(2010)), while the text round trip above passes.
        }
    }
}
