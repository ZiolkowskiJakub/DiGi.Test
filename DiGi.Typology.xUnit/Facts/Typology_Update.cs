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
            Assert.Equal(new Typology.Classes.TypologyPath([1, 1, 1]), typology_GrandGrandChild.TypologyPath);

            Typology.Classes.Typology? typology_GrandChild_Updated = typology.Update([1, 1], "GrandChild", "Updated description");

            Assert.NotNull(typology_GrandChild_Updated);
            Assert.Same(typology_GrandChild, typology_GrandChild_Updated);
            Assert.Equal("Updated description", typology_GrandChild_Updated.Description);
            Assert.True(typology_GrandChild_Updated.Contains("reference_1"));

            Assert.Equal("GrandGrandChild", typology.GetTypology([1, 1, 1])?.Name);
            Assert.Single(typology_GrandChild_Updated.GetReferences(true));

            Assert.NotNull(typology.Update([1, 2], "Sibling", "Sibling description"));
            Assert.Equal("GrandChild", typology.GetTypology([1, 1])?.Name);
            Assert.Equal("Sibling", typology.GetTypology([1, 2])?.Name);
            Assert.Equal("Child", typology_Child.Name);

            List<Typology.Classes.TypologyPath> typologyPaths = typology.GetTypologyPaths(true);

            Assert.Equal(4, typologyPaths.Count);

            foreach (Typology.Classes.TypologyPath typologyPath in typologyPaths)
            {
                Assert.NotNull(typology.GetTypology(typologyPath));
            }

            Core.xUnit.Query.SerializationCheck(typology);
        }
    }
}
