using DiGi.Typology.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests deep value equality of <see cref="Typology.Classes.Typology"/> - the typology item, the
        /// reference set and the whole sub-typology tree - together with the equality operators, the hash
        /// contract and a clone round-trip.
        /// <para>Regression guard for ZiolkowskiJakub/DiGi.Typology#13: the class carried no equality
        /// members, so every comparison silently fell back to System.Object identity.</para>
        /// </summary>
        [Fact]
        public void Typology_Equality()
        {
            Typology.Classes.Typology Create()
            {
                Typology.Classes.Typology typology = new("Root", "Root description");

                Assert.True(typology.AddReference("ref-1"));
                Assert.True(typology.AddReference("ref-2"));

                Typology.Classes.Typology? typology_Child = typology.Update([0], "Child", "Child description");

                Assert.NotNull(typology_Child);
                Assert.True(typology_Child.AddReference("ref-3"));
                Assert.NotNull(typology_Child.Update([0], "Grandchild", "Grandchild description"));

                return typology;
            }

            Typology.Classes.Typology typology_1 = Create();
            Typology.Classes.Typology typology_2 = Create();

            Assert.NotSame(typology_1, typology_2);
            Assert.True(typology_1.Equals(typology_2));
            Assert.True(typology_1 == typology_2);
            Assert.False(typology_1 != typology_2);
            Assert.Equal(typology_1.GetHashCode(), typology_2.GetHashCode());
            Assert.True(EqualityComparer<Typology.Classes.Typology>.Default.Equals(typology_1, typology_2));
            Assert.Equal(typology_1, typology_2);

            Assert.False(typology_1.Equals(null));
            Assert.False(typology_1.Equals("Root"));
            Assert.False(typology_1 == null);
            Assert.False(null == typology_1);
            Assert.True(typology_1 != null);
            Assert.True((Typology.Classes.Typology?)null == (Typology.Classes.Typology?)null);

            Typology.Classes.Typology typology_Name = Create();
            typology_Name.Name = "Other";

            Assert.NotEqual(typology_1, typology_Name);
            Assert.True(typology_1 != typology_Name);

            Typology.Classes.Typology typology_Description = Create();
            typology_Description.Description = "Other description";

            Assert.NotEqual(typology_1, typology_Description);

            Typology.Classes.Typology typology_Reference = Create();

            Assert.True(typology_Reference.AddReference("ref-4"));
            Assert.NotEqual(typology_1, typology_Reference);

            Typology.Classes.Typology typology_SubTypology = Create();

            Assert.NotNull(typology_SubTypology.Update([1], "Extra", null));
            Assert.NotEqual(typology_1, typology_SubTypology);

            Typology.Classes.Typology typology_Nested = Create();
            Typology.Classes.Typology? typology_Nested_Grandchild = typology_Nested.SubTypology([0, 0]);

            Assert.NotNull(typology_Nested_Grandchild);
            typology_Nested_Grandchild.Name = "Renamed";

            Assert.NotEqual(typology_1, typology_Nested);

            Typology.Classes.Typology? typology_Clone = Core.Query.Clone(typology_1);

            Assert.NotNull(typology_Clone);
            Assert.NotSame(typology_1, typology_Clone);
            Assert.Equal(typology_1, typology_Clone);
            Assert.Equal(typology_1.GetHashCode(), typology_Clone.GetHashCode());

            Typology.Classes.Typology typology_Copy = new(typology_1);

            Assert.NotSame(typology_1, typology_Copy);
            Assert.Equal(typology_1, typology_Copy);
            Assert.Equal(typology_1.GetHashCode(), typology_Copy.GetHashCode());

            Typology.Classes.Typology? typology_Fallback = DiGi.Typology.Create.Typology(new TypologyItem([9], "Composed", null), [new Typology.Classes.Typology("A", null), new Typology.Classes.Typology("B", null), new Typology.Classes.Typology(new TypologyItem([0], "C", null))]);

            Assert.NotNull(typology_Fallback);

            Typology.Classes.Typology? typology_Fallback_Clone = Core.Query.Clone(typology_Fallback);

            Assert.NotNull(typology_Fallback_Clone);
            Assert.Equal(typology_Fallback, typology_Fallback_Clone);
            Assert.Equal(typology_Fallback, new Typology.Classes.Typology(typology_Fallback));
            Assert.Equal(0, typology_Fallback.CompareTo(typology_Fallback_Clone));

            Typology.Classes.Typology typology_Empty = new((TypologyItem?)null);

            Assert.Equal(typology_Empty, new Typology.Classes.Typology((TypologyItem?)null));
            Assert.Equal(typology_Empty.GetHashCode(), new Typology.Classes.Typology((TypologyItem?)null).GetHashCode());
            Assert.Equal(0, typology_Empty.CompareTo(new Typology.Classes.Typology((TypologyItem?)null)));
            Assert.NotEqual(typology_1, typology_Empty);
            Assert.True(typology_Empty.CompareTo(typology_1) != 0);

            Core.xUnit.Query.SerializationCheck(typology_1);
            Core.xUnit.Query.SerializationCheck(typology_Fallback);
        }
    }
}
