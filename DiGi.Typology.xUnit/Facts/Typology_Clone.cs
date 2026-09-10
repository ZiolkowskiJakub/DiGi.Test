using DiGi.Typology.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that the copy constructor and the sub-typology constructor copy the whole sub-tree
        /// rather than aliasing it.
        /// <para>Regression guard: both used to store the caller's child instances, so mutating a
        /// descendant of the copy mutated the source. Core.Query.Clone is unaffected because it
        /// round-trips through JSON rather than through these constructors, so the guard has to
        /// invoke them directly.</para>
        /// </summary>
        [Fact]
        public void Typology_Clone()
        {
            Typology.Classes.Typology typology = new("Root", "Root description");

            Assert.NotNull(typology.Update([1], "Child", "Child description"));

            Typology.Classes.Typology? typology_GrandChild = typology.Update([1, 1], "GrandChild", "GrandChild description");

            Assert.NotNull(typology_GrandChild);
            Assert.True(typology_GrandChild.AddReference("reference_1"));

            Typology.Classes.Typology typology_Copy = new(typology);

            Typology.Classes.Typology? typology_GrandChild_Copy = typology_Copy.SubTypology([1, 1]);

            Assert.NotNull(typology_GrandChild_Copy);
            Assert.NotSame(typology_GrandChild, typology_GrandChild_Copy);
            Assert.Equal("GrandChild", typology_GrandChild_Copy.Name);
            Assert.True(typology_GrandChild_Copy.Contains("reference_1"));

            typology_GrandChild_Copy.Name = "Renamed";
            Assert.True(typology_GrandChild_Copy.AddReference("reference_2"));

            Assert.Equal("GrandChild", typology_GrandChild.Name);
            Assert.False(typology_GrandChild.Contains("reference_2"));

            Assert.Equal(2, typology_Copy.ReferenceSet(true).Count);
            Assert.Single(typology.ReferenceSet(true));

            Typology.Classes.Typology? typology_Composed = DiGi.Typology.Create.Typology(new Typology.Classes.TypologyItem([9], "Composed", null), [typology_GrandChild]);

            Assert.NotNull(typology_Composed);

            Typology.Classes.Typology? typology_Composed_Child = typology_Composed.SubTypology([1]);

            Assert.NotNull(typology_Composed_Child);
            Assert.NotSame(typology_GrandChild, typology_Composed_Child);

            typology_Composed_Child.Name = "Renamed";

            Assert.Equal("GrandChild", typology_GrandChild.Name);

            Typology.Classes.Typology? typology_Clone = Core.Query.Clone(typology);

            Assert.NotNull(typology_Clone);
            Assert.Single(typology_Clone.ReferenceSet(true));
            Assert.Equal("GrandChild", typology_Clone.SubTypology([1, 1])?.Name);

            List<Typology.Classes.Typology> typologies = [new(new Typology.Classes.TypologyItem()), new(new Typology.Classes.TypologyItem()), typology];
            typologies.Sort();

            Assert.Equal(3, typologies.Count);
        }
    }
}
