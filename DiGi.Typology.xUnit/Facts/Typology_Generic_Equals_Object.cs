namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the object-typed equality surface of <see cref="Typology.Classes.Typology"/> now that it is
        /// inherited from the generic base: <see cref="object.Equals(object)"/>, the operators and
        /// hash-based membership.
        /// <para>Regression guard: the first generic base pattern-matched on the base type in
        /// Equals(object), which bound the inner call back to Equals(object) and overflowed the stack on
        /// the first call through any of these paths.</para>
        /// </summary>
        [Fact]
        public void Typology_Generic_Equals_Object()
        {
            Typology.Classes.Typology Create(string name)
            {
                Typology.Classes.Typology typology = new(name, "Description");

                Assert.True(typology.AddReference("ref-1"));
                Assert.NotNull(typology.Update([0], "Child", "Child description"));

                return typology;
            }

            Typology.Classes.Typology typology_1 = Create("Root");
            Typology.Classes.Typology typology_2 = Create("Root");
            Typology.Classes.Typology typology_Other = Create("Other");

            object object_2 = typology_2;

            Assert.True(typology_1.Equals(object_2));
            Assert.False(typology_1.Equals((object)typology_Other));
            Assert.False(typology_1.Equals((object?)null));
            Assert.False(typology_1.Equals("Root"));

            Assert.True(typology_1 == typology_2);
            Assert.False(typology_1 == typology_Other);
            Assert.True(typology_1 != typology_Other);
            Assert.True(typology_1 != null);
            Assert.True(null == (Typology.Classes.Typology?)null);

            HashSet<Typology.Classes.Typology> typologies = [typology_1];

            Assert.Contains(typology_2, typologies);
            Assert.DoesNotContain(typology_Other, typologies);
            Assert.False(typologies.Add(typology_2));

            List<Typology.Classes.Typology> typologies_List = [typology_1];

            Assert.Contains(typology_2, typologies_List);
            Assert.Equal(0, typologies_List.IndexOf(typology_2));

            Dictionary<Typology.Classes.Typology, int> dictionary = new() { { typology_1, 1 } };

            Assert.True(dictionary.TryGetValue(typology_2, out int value));
            Assert.Equal(1, value);
        }
    }
}
