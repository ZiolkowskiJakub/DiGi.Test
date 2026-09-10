using DiGi.Typology.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that re-updating an existing node keeps that node rather than replacing it.
        /// <para>Regression guard: the lookup inside Update used to compose an absolute path and
        /// then resolve it against the nested node it was invoked on, so a second update of a node
        /// at depth two or more found nothing, built a fresh instance and assigned it over the
        /// existing one, discarding that node's sub-typologies and references.</para>
        /// </summary>
        [Fact]
        public void Typology_Update()
        {
            Typology.Classes.Typology typology = new("Root", "Root description");

            Typology.Classes.Typology? typology_Child = typology.Update([1], "Child", "Child description");

            Assert.NotNull(typology_Child);

            Typology.Classes.Typology? typology_GrandChild = typology.Update([1, 1], "GrandChild", "GrandChild description");

            Assert.NotNull(typology_GrandChild);
            Assert.True(typology_GrandChild.AddReference("reference_1"));

            Typology.Classes.Typology? typology_GrandGrandChild = typology.Update([1, 1, 1], "GrandGrandChild", "GrandGrandChild description");

            Assert.NotNull(typology_GrandGrandChild);
            Assert.Equal(new TypologyPath([1, 1, 1]), typology_GrandGrandChild.TypologyPath);

            Typology.Classes.Typology? typology_GrandChild_Updated = typology.Update([1, 1], "GrandChild", "Updated description");

            Assert.NotNull(typology_GrandChild_Updated);
            Assert.Same(typology_GrandChild, typology_GrandChild_Updated);
            Assert.Equal("Updated description", typology_GrandChild_Updated.Description);
            Assert.True(typology_GrandChild_Updated.Contains("reference_1"));

            Assert.Equal("GrandGrandChild", typology.SubTypology([1, 1, 1])?.Name);
            Assert.Single(typology_GrandChild_Updated.ReferenceSet(true));
            Assert.Empty(typology.ReferenceSet());
            Assert.Equal(typology.ReferenceSet(false), typology.ReferenceSet());
            Assert.NotEqual(typology.ReferenceSet(false), typology.ReferenceSet(true));

            Assert.NotNull(typology.Update([1, 2], "Sibling", "Sibling description"));
            Assert.Equal("GrandChild", typology.SubTypology([1, 1])?.Name);
            Assert.Equal("Sibling", typology.SubTypology([1, 2])?.Name);
            Assert.Equal("Child", typology_Child.Name);

            List<TypologyPath> typologyPaths = typology.TypologyPaths(true);

            Assert.Equal(4, typologyPaths.Count);

            foreach (TypologyPath typologyPath in typologyPaths)
            {
                Assert.NotNull(typology.SubTypology(typologyPath));
            }

            Core.xUnit.Query.SerializationCheck(typology);
        }
    }
}
