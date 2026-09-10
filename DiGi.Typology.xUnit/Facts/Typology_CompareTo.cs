using DiGi.Typology.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the ordering contract of <see cref="Typology.Classes.Typology"/>: the primary path ordering
        /// and the tie-breakers on name, description, reference set and sub-typologies.
        /// <para>Regression guard for ZiolkowskiJakub/DiGi.Typology#13: ordering and equality must not split,
        /// so CompareTo returns zero exactly when Equals returns true.</para>
        /// </summary>
        [Fact]
        public void Typology_CompareTo()
        {
            Typology.Classes.Typology Create(int index, string? name, string? description)
            {
                return new Typology.Classes.Typology(new Typology.Classes.TypologyItem(new Typology.Classes.TypologyPath([index]), name, description));
            }

            Typology.Classes.Typology typology_1 = Create(1, "AAA", "Test AAA");
            Typology.Classes.Typology typology_2 = Create(2, "AAA", "Test AAA");

            Assert.True(typology_1.CompareTo(typology_2) < 0);
            Assert.True(typology_2.CompareTo(typology_1) > 0);
            Assert.True(typology_1.CompareTo(null!) > 0);
            Assert.Equal(0, typology_1.CompareTo(Create(1, "AAA", "Test AAA")));

            Typology.Classes.Typology typology_Name = Create(1, "BBB", "Test AAA");

            Assert.True(typology_1.CompareTo(typology_Name) < 0);
            Assert.True(typology_Name.CompareTo(typology_1) > 0);

            Typology.Classes.Typology typology_Description = Create(1, "AAA", "Test BBB");

            Assert.True(typology_1.CompareTo(typology_Description) < 0);
            Assert.True(typology_Description.CompareTo(typology_1) > 0);

            Typology.Classes.Typology typology_Reference = Create(1, "AAA", "Test AAA");

            Assert.True(typology_Reference.AddReference("ref-1"));
            Assert.True(typology_1.CompareTo(typology_Reference) < 0);
            Assert.True(typology_Reference.CompareTo(typology_1) > 0);

            Typology.Classes.Typology typology_SubTypology = Create(1, "AAA", "Test AAA");

            Assert.NotNull(typology_SubTypology.Update([0], "Child", "Child description"));
            Assert.True(typology_1.CompareTo(typology_SubTypology) < 0);
            Assert.True(typology_SubTypology.CompareTo(typology_1) > 0);

            List<Typology.Classes.Typology> typologies = [typology_1, typology_2, typology_Name, typology_Description, typology_Reference, typology_SubTypology];

            foreach (Typology.Classes.Typology typology_A in typologies)
            {
                foreach (Typology.Classes.Typology typology_B in typologies)
                {
                    Assert.Equal(typology_A.Equals(typology_B), typology_A.CompareTo(typology_B) == 0);
                }
            }

            typologies.Sort();

            Assert.Equal(6, typologies.Count);
            Assert.Equal(new Typology.Classes.TypologyPath([2]), typologies[^1].TypologyPath);
        }
    }
}
