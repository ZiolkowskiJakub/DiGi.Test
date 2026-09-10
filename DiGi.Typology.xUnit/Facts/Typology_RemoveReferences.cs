using DiGi.Typology.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that RemoveReferences strips a whole tree, that it strips a single branch when applied to a sub-typology, and that structure survives either way.
        /// <para>The branch case also pins that Query.SubTypology hands back the instance the tree holds rather than a clone of it - against a clone the strip would silently do nothing to the tree.</para>
        /// </summary>
        [Fact]
        public void Typology_RemoveReferences()
        {
            Typology.Classes.Typology typology = Create();

            Typology.Classes.Typology? typology_Branch = typology.SubTypology(new TypologyPath([0]));
            Assert.NotNull(typology_Branch);

            Assert.True(typology_Branch.RemoveReferences(true));

            Assert.Empty(typology_Branch.ReferenceSet(true));
            Assert.Contains("root", typology.References);
            Assert.Contains("second", typology.ReferenceSet(true));
            Assert.DoesNotContain("first", typology.ReferenceSet(true));
            Assert.Equal(2, typology.TypologyPaths(true).Count);

            typology = Create();

            Assert.True(typology.RemoveReferences(true));
            Assert.Empty(typology.ReferenceSet(true));
            Assert.Equal(2, typology.TypologyPaths(true).Count);

            Assert.False(typology.RemoveReferences(true));

            typology = Create();

            Assert.True(typology.RemoveReferences(false));
            Assert.Empty(typology.References);
            Assert.Contains("first", typology.ReferenceSet(true));
            Assert.Contains("second", typology.ReferenceSet(true));

            Assert.False(Modify.RemoveReferences(null));

            static Typology.Classes.Typology Create()
            {
                Typology.Classes.Typology typology = new("Root", "Description");
                typology.AddReference("root");

                Typology.Classes.Typology? typology_First = typology.Update(new TypologyItem([0], "First"));
                Assert.NotNull(typology_First);
                typology_First.AddReference("first");

                Typology.Classes.Typology? typology_Second = typology.Update(new TypologyItem([1], "Second"));
                Assert.NotNull(typology_Second);
                typology_Second.AddReference("second");

                return typology;
            }
        }
    }
}
