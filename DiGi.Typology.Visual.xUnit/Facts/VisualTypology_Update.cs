using DiGi.Typology.Classes;
using DiGi.Typology.Visual.Classes;

namespace DiGi.Typology.Visual.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests Modify.Update, Modify.TryUpdateByName and Create.VisualTypology on a VisualTypology: every node created is
        /// a VisualTypology, a leaf takes its name, description and appearance from the source item while a missing
        /// intermediate node is created bare, an existing node is updated in place keeping its sub-typologies, its
        /// references and its appearance, and the appearance survives a RemoveReferences and a serialization
        /// round-trip.
        /// <para>The string-based shapes are called without explicit type arguments, locking the Visual convenience
        /// overloads: the item type appears in no parameter of the generic shapes, so it cannot be inferred there.
        /// One shape is also driven through the generic form with explicit VisualTypology and VisualTypologyItem
        /// arguments.</para>
        /// <para>Regression guard: the concrete Update re-pathed every created node through the base item
        /// constructor, which flattens a VisualTypologyItem's appearance; the Visual forms must route node creation
        /// through VisualTypology's CreateNode instead.</para>
        /// </summary>
        [Fact]
        public void VisualTypology_Update()
        {
            TypologyAppearance typologyAppearance_Child = Create.TypologyAppearance(System.Drawing.Color.Red);
            TypologyAppearance typologyAppearance_Root = Create.TypologyAppearance(System.Drawing.Color.Blue);

            VisualTypology visualTypology = new(new VisualTypologyItem(null, "Root", "Root description", null));

            // The item shape: inferable from the item, so no explicit type arguments. The created leaf takes the
            // source item's appearance.
            VisualTypology? visualTypology_Child = visualTypology.Update(new VisualTypologyItem([1], "Child", "Child description", typologyAppearance_Child));

            Assert.NotNull(visualTypology_Child);
            Assert.IsType<VisualTypology>(visualTypology_Child);
            Assert.Equal("Child", visualTypology_Child.Name);
            Assert.Equal("Child description", visualTypology_Child.Description);
            AssertAppearance(typologyAppearance_Child, visualTypology_Child);
            Assert.Equal(new TypologyPath([1]), visualTypology_Child.TypologyPath);

            // The string-based shapes without explicit type arguments: the Visual convenience overload names the
            // type for the caller. A missing intermediate node is created bare, the leaf carries name and
            // description only.
            VisualTypology? visualTypology_Deep = visualTypology.Update([2, 1], "Deep", "Deep description");

            Assert.NotNull(visualTypology_Deep);
            Assert.IsType<VisualTypology>(visualTypology_Deep);
            Assert.Equal("Deep", visualTypology_Deep.Name);
            Assert.Equal("Deep description", visualTypology_Deep.Description);
            AssertAppearance(null, visualTypology_Deep);

            VisualTypology? visualTypology_Intermediate = visualTypology[2];

            Assert.NotNull(visualTypology_Intermediate);
            Assert.IsType<VisualTypology>(visualTypology_Intermediate);
            Assert.Null(visualTypology_Intermediate.Name);
            AssertAppearance(null, visualTypology_Intermediate);

            // The name and description, name only and values and name shapes.
            Assert.NotNull(visualTypology.Update("Sibling", "Sibling description"));
            Assert.Equal("Sibling", visualTypology[3]?.Name);

            Assert.NotNull(visualTypology.Update("Solo"));
            Assert.Equal("Solo", visualTypology[4]?.Name);

            Assert.NotNull(visualTypology.Update([5], "Named"));
            Assert.Equal("Named", visualTypology[5]?.Name);

            // The generic form with explicit type arguments.
            VisualTypology? visualTypology_Generic = DiGi.Typology.Modify.Update<VisualTypology, VisualTypologyItem>(visualTypology, [6], "Generic", "Generic description");

            Assert.NotNull(visualTypology_Generic);
            Assert.IsType<VisualTypology>(visualTypology_Generic);
            Assert.Equal("Generic", visualTypology_Generic.Name);
            Assert.Equal("Generic description", visualTypology_Generic.Description);
            AssertAppearance(null, visualTypology_Generic);

            // In place: the node is kept, the name and description are applied, and the appearance, the
            // sub-typology and the references are untouched.
            Assert.True(visualTypology_Child!.AddReference("ref-1"));

            visualTypology_Child[0] = new VisualTypology(new VisualTypologyItem([1, 0], "Sub", null, null));

            VisualTypology? visualTypology_Child_Updated = visualTypology.Update(new VisualTypologyItem([1], "Renamed", "Renamed description", Create.TypologyAppearance(System.Drawing.Color.Orange)));

            Assert.NotNull(visualTypology_Child_Updated);
            Assert.Same(visualTypology_Child, visualTypology_Child_Updated);
            Assert.Equal("Renamed", visualTypology_Child_Updated.Name);
            Assert.Equal("Renamed description", visualTypology_Child_Updated.Description);
            AssertAppearance(typologyAppearance_Child, visualTypology_Child_Updated);
            Assert.True(visualTypology_Child_Updated.Contains("ref-1"));
            Assert.NotNull(visualTypology_Child_Updated[0]);

            // TryUpdateByName: a matching direct child is updated in place, a missing one is created.
            bool found = visualTypology.TryUpdateByName([7], "Renamed", "Try updated description", out VisualTypology? visualTypology_Try);

            Assert.True(found);
            Assert.NotNull(visualTypology_Try);
            Assert.Same(visualTypology_Child, visualTypology_Try);
            Assert.Equal("Try updated description", visualTypology_Try.Description);
            AssertAppearance(typologyAppearance_Child, visualTypology_Try);

            bool created = visualTypology.TryUpdateByName([8], "Fresh", "Fresh description", out VisualTypology? visualTypology_Fresh);

            Assert.True(created);
            Assert.NotNull(visualTypology_Fresh);
            Assert.Equal("Fresh", visualTypology_Fresh.Name);
            Assert.Equal("Fresh description", visualTypology_Fresh.Description);
            AssertAppearance(null, visualTypology_Fresh);
            Assert.Equal(new TypologyPath([8]), visualTypology_Fresh.TypologyPath);

            bool refused = visualTypology.TryUpdateByName(null, null, null, out VisualTypology? visualTypology_Refused);

            Assert.False(refused);
            Assert.Null(visualTypology_Refused);

            // Create.VisualTypology: the node takes the item, appearance included, and the sub-typologies are filed.
            VisualTypology? visualTypology_Built = Typology.Visual.Create.VisualTypology(new VisualTypologyItem(null, "Root", "Root description", typologyAppearance_Root),
            [
                new VisualTypology(new VisualTypologyItem([0], "A", "A description", typologyAppearance_Child)),
                new VisualTypology(new VisualTypologyItem([1], "B", "B description", null))
            ]);

            Assert.NotNull(visualTypology_Built);
            Assert.IsType<VisualTypology>(visualTypology_Built);
            Assert.Equal("Root", visualTypology_Built.Name);
            AssertAppearance(typologyAppearance_Root, visualTypology_Built);

            VisualTypology? visualTypology_Built_A = visualTypology_Built[0];

            Assert.NotNull(visualTypology_Built_A);
            Assert.IsType<VisualTypology>(visualTypology_Built_A);
            Assert.Equal("A", visualTypology_Built_A.Name);
            AssertAppearance(typologyAppearance_Child, visualTypology_Built_A);
            Assert.Equal("B", visualTypology_Built[1]?.Name);
            AssertAppearance(null, visualTypology_Built[1]!);

            // The tree, its references and every appearance round-trip.
            Core.xUnit.Query.SerializationCheck(visualTypology);

            VisualTypology? visualTypology_RoundTrip = Core.Convert.ToDiGi<VisualTypology>(Core.Convert.ToSystem_String(visualTypology))?.FirstOrDefault();

            Assert.NotNull(visualTypology_RoundTrip);
            Assert.True(visualTypology_RoundTrip == visualTypology);
            AssertAppearance(typologyAppearance_Child, visualTypology_RoundTrip[1]!);
            Assert.Equal("Renamed", visualTypology_RoundTrip[1]?.Name);

            // Stripping the references keeps every appearance: the appearance is node metadata, not link data.
            Assert.True(visualTypology.RemoveReferences(true));
            Assert.Empty(visualTypology.ReferenceSet(true));
            AssertAppearance(typologyAppearance_Child, visualTypology_Child);
            AssertAppearance(typologyAppearance_Root, visualTypology_Built);

            static void AssertAppearance(TypologyAppearance? typologyAppearance_Expected, VisualTypology visualTypology)
            {
                TypologyAppearance? typologyAppearance_Actual = visualTypology.TypologyItem?.Appearance;

                if (typologyAppearance_Expected is null)
                {
                    Assert.Null(typologyAppearance_Actual);
                    return;
                }

                Assert.NotNull(typologyAppearance_Actual);
                Assert.Equal(Core.Convert.ToSystem_String(typologyAppearance_Expected), Core.Convert.ToSystem_String(typologyAppearance_Actual));
            }
        }
    }
}
