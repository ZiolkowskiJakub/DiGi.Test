using DiGi.Typology.xUnit.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests deriving from <see cref="Typology.Classes.Typology{TTypology, TTypologyItem}"/> with a
        /// specialised item type: the item's extra field takes part in equality, ordering and the hash, the
        /// setters create an item of the derived type, the copy constructor copies the whole tree, and the
        /// tree with its references survives a serialization round-trip.
        /// <para>Regression guard: the first generic base had no constructors to chain, no SubTypologies
        /// property (the tree was never serialized) and compared items through the non-virtual base
        /// members, so a derived item's fields were invisible to Equals and CompareTo.</para>
        /// </summary>
        [Fact]
        public void Typology_Generic_Derived()
        {
            TypologyTest Create(double weight)
            {
                TypologyTest typologyTest = new(new TypologyItemTest(null, "Root", weight));

                Assert.True(typologyTest.AddReference("ref-1"));
                Assert.True(typologyTest.AddReference("ref-2"));

                typologyTest[0] = new TypologyTest(new TypologyItemTest([0], "Child", weight));
                typologyTest[3] = new TypologyTest(new TypologyItemTest([3], "Child 3", weight + 1));

                TypologyTest? typologyTest_Child = typologyTest[0];

                Assert.NotNull(typologyTest_Child);
                Assert.True(typologyTest_Child.AddReference("ref-3"));

                typologyTest_Child[0] = new TypologyTest(new TypologyItemTest([0, 0], "Grandchild", weight));

                return typologyTest;
            }

            TypologyTest typologyTest_1 = Create(1.5);
            TypologyTest typologyTest_2 = Create(1.5);
            TypologyTest typologyTest_Heavier = Create(2.5);

            Assert.Equal([0, 3], typologyTest_1.Indexes);
            Assert.Equal(1.5, typologyTest_1[0]?.TypologyItem?.Weight);
            Assert.Null(typologyTest_1[1]);

            // The derived item's extra field takes part in equality, ordering and the hash.
            Assert.True(typologyTest_1.Equals(typologyTest_2));
            Assert.True(typologyTest_1 == typologyTest_2);
            Assert.Equal(0, typologyTest_1.CompareTo(typologyTest_2));
            Assert.Equal(typologyTest_1.GetHashCode(), typologyTest_2.GetHashCode());

            Assert.False(typologyTest_1.Equals(typologyTest_Heavier));
            Assert.True(typologyTest_1 != typologyTest_Heavier);
            Assert.True(typologyTest_1.CompareTo(typologyTest_Heavier) < 0);
            Assert.True(typologyTest_Heavier.CompareTo(typologyTest_1) > 0);
            Assert.NotEqual(typologyTest_1.GetHashCode(), typologyTest_Heavier.GetHashCode());

            // The setters create an item of the derived type when there is none.
            TypologyTest typologyTest_Empty = new((TypologyItemTest?)null);

            Assert.Null(typologyTest_Empty.Name);

            typologyTest_Empty.Name = "Named";
            typologyTest_Empty.Description = "Described";

            Assert.Equal("Named", typologyTest_Empty.Name);
            Assert.Equal("Described", typologyTest_Empty.Description);
            Assert.Null(typologyTest_Empty.TypologyPath);

            // The copy constructor copies the tree rather than aliasing it.
            TypologyTest typologyTest_Copy = new(typologyTest_1);

            Assert.True(typologyTest_Copy == typologyTest_1);
            Assert.NotSame(typologyTest_Copy[0], typologyTest_1[0]);

            TypologyTest? typologyTest_Copy_Child = typologyTest_Copy[0];

            Assert.NotNull(typologyTest_Copy_Child);

            typologyTest_Copy_Child.Name = "Renamed";

            Assert.Equal("Child", typologyTest_1[0]?.Name);
            Assert.True(typologyTest_Copy != typologyTest_1);

            // Removing by index and the reference members.
            typologyTest_Copy[3] = null;

            Assert.Equal([0], typologyTest_Copy.Indexes);
            Assert.True(typologyTest_Copy.ContainsReference("ref-1"));
            Assert.True(typologyTest_Copy.RemoveReference("ref-1"));
            Assert.False(typologyTest_Copy.ContainsReference("ref-1"));
            Assert.Equal(["ref-2"], typologyTest_Copy.References);

            // A plain Typology holding a derived item compares and orders the item by its runtime type, so equality
            // and the hash stay consistent even when the declared item type is the base one.
            Typology.Classes.Typology typology_Light = new(new TypologyItemTest(null, "Root", 1.5));
            Typology.Classes.Typology typology_Heavy = new(new TypologyItemTest(null, "Root", 2.5));

            Assert.IsType<TypologyItemTest>(typology_Light.TypologyItem);
            Assert.False(typology_Light == typology_Heavy);
            Assert.True(typology_Light.CompareTo(typology_Heavy) < 0);
            Assert.NotEqual(typology_Light.GetHashCode(), typology_Heavy.GetHashCode());
            Assert.True(typology_Light == new Typology.Classes.Typology(typology_Light));

            // The whole tree, its references and the derived item's field round-trip.
            Core.xUnit.Query.SerializationCheck(typologyTest_1);

            TypologyTest? typologyTest_RoundTrip = Core.Convert.ToDiGi<TypologyTest>(Core.Convert.ToSystem_String(typologyTest_1))?.FirstOrDefault();

            Assert.NotNull(typologyTest_RoundTrip);
            Assert.True(typologyTest_RoundTrip == typologyTest_1);
            Assert.Equal([0, 3], typologyTest_RoundTrip.Indexes);
            Assert.Equal(2.5, typologyTest_RoundTrip[3]?.TypologyItem?.Weight);
            Assert.Equal(["ref-3"], typologyTest_RoundTrip[0]?.References);
            Assert.Equal("Grandchild", typologyTest_RoundTrip[0]?[0]?.Name);
        }
    }
}
