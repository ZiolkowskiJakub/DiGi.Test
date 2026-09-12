using DiGi.Typology.Classes;
using DiGi.Typology.xUnit.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the generic Modify.Update and Modify.TryUpdateByName on a derived typology: every node created,
        /// intermediate and leaf alike, is of the derived type and its item takes the name, the description and the
        /// item's extra field from the source item; a missing intermediate node is created bare; an existing node is
        /// updated in place, keeping its sub-typologies, its references and its item's extra field; and the whole
        /// tree round-trips through serialization.
        /// <para>The item shape is called without explicit type arguments, because the item type is inferable from
        /// the item; the string shapes are called with explicit TypologyTest and TypologyItemTest arguments, because
        /// the item type appears in no parameter there and cannot be inferred.</para>
        /// <para>Regression guard: the concrete Update re-pathed every created node through the base item
        /// constructor, which flattens a derived item's extra field; the generic form must route node creation
        /// through the derived type's CreateNode instead. On a plain Typology the concrete overloads must still win
        /// the binding, so the existing call sites keep their non-generic routing.</para>
        /// </summary>
        [Fact]
        public void Typology_Update_Generic()
        {
            TypologyTest typology = new(new TypologyItemTest(null, "Root", 0));

            // The item shape: no explicit type arguments - the item type is inferable from the item.
            TypologyTest? typology_Child = typology.Update(new TypologyItemTest([1], "Child", 1.5) { Description = "Child description" });

            Assert.NotNull(typology_Child);
            Assert.IsType<TypologyTest>(typology_Child);
            Assert.Equal("Child", typology_Child.Name);
            Assert.Equal("Child description", typology_Child.Description);
            Assert.Equal(1.5, typology_Child.TypologyItem?.Weight);
            Assert.Equal(new TypologyPath([1]), typology_Child.TypologyPath);

            // The string shapes with explicit type arguments: a missing intermediate node is created bare, the leaf
            // carries the name and the description.
            TypologyTest? typology_Deep = typology.Update<TypologyTest, TypologyItemTest>([2, 1], "Deep", "Deep description");

            Assert.NotNull(typology_Deep);
            Assert.IsType<TypologyTest>(typology_Deep);
            Assert.Equal("Deep", typology_Deep.Name);
            Assert.Equal("Deep description", typology_Deep.Description);
            Assert.Equal(0.0, typology_Deep.TypologyItem?.Weight);
            Assert.Equal(new TypologyPath([2, 1]), typology_Deep.TypologyPath);

            TypologyTest? typology_Intermediate = typology[2];

            Assert.NotNull(typology_Intermediate);
            Assert.IsType<TypologyTest>(typology_Intermediate);
            Assert.Null(typology_Intermediate.Name);
            Assert.Equal(0.0, typology_Intermediate.TypologyItem?.Weight);

            // The name and description, name only and values and name shapes.
            Assert.NotNull(typology.Update<TypologyTest, TypologyItemTest>("Sibling", "Sibling description"));
            Assert.Equal("Sibling", typology[3]?.Name);

            Assert.NotNull(typology.Update<TypologyTest, TypologyItemTest>("Solo"));
            Assert.Equal("Solo", typology[4]?.Name);

            Assert.NotNull(typology.Update<TypologyTest, TypologyItemTest>([5], "Named"));
            Assert.Equal("Named", typology[5]?.Name);

            // In place: the node is kept, the name and description are applied, and the extra field, the
            // sub-typology and the references are untouched.
            Assert.True(typology_Child!.AddReference("ref-1"));

            typology_Child[0] = new TypologyTest(new TypologyItemTest([1, 0], "Sub", 0));

            TypologyTest? typology_Child_Updated = typology.Update(new TypologyItemTest([1], "Renamed", 2.5) { Description = "Renamed description" });

            Assert.NotNull(typology_Child_Updated);
            Assert.Same(typology_Child, typology_Child_Updated);
            Assert.Equal("Renamed", typology_Child_Updated.Name);
            Assert.Equal("Renamed description", typology_Child_Updated.Description);
            Assert.Equal(1.5, typology_Child_Updated.TypologyItem?.Weight);
            Assert.True(typology_Child_Updated.Contains("ref-1"));
            Assert.NotNull(typology_Child_Updated[0]);

            // TryUpdateByName: a matching direct child is updated in place, a missing one is created.
            bool found = typology.TryUpdateByName<TypologyTest, TypologyItemTest>([6], "Renamed", "Try updated description", out TypologyTest? typology_Try);

            Assert.True(found);
            Assert.NotNull(typology_Try);
            Assert.Same(typology_Child, typology_Try);
            Assert.Equal("Try updated description", typology_Try.Description);
            Assert.Equal(1.5, typology_Try.TypologyItem?.Weight);

            bool created = typology.TryUpdateByName<TypologyTest, TypologyItemTest>([7], "Fresh", "Fresh description", out TypologyTest? typology_Fresh);

            Assert.True(created);
            Assert.IsType<TypologyTest>(typology_Fresh);
            Assert.Equal("Fresh", typology_Fresh.Name);
            Assert.Equal("Fresh description", typology_Fresh.Description);
            Assert.Equal(0.0, typology_Fresh.TypologyItem?.Weight);
            Assert.Equal(new TypologyPath([7]), typology_Fresh.TypologyPath);

            bool refused = typology.TryUpdateByName<TypologyTest, TypologyItemTest>(null, null, null, out TypologyTest? typology_Refused);

            Assert.False(refused);
            Assert.Null(typology_Refused);

            // Binding guard: a plain Typology binds the concrete overload - the non-generic one wins the tie - and
            // produces plain nodes.
            Typology.Classes.Typology plain = new("Root", "Root description");

            Typology.Classes.Typology? plain_Child = plain.Update(new TypologyItem([1], "Child"));

            Assert.NotNull(plain_Child);
            Assert.IsType<Typology.Classes.Typology>(plain_Child);
            Assert.IsType<TypologyItem>(plain_Child.TypologyItem);
            Assert.Equal("Child", plain_Child.Name);

            bool plainFound = plain.TryUpdateByName([1], "Child", "Plain updated", out Typology.Classes.Typology? plain_Matched);

            Assert.True(plainFound);
            Assert.Same(plain_Child, plain_Matched);
            Assert.IsType<Typology.Classes.Typology>(plain_Matched);
            Assert.Equal("Plain updated", plain_Matched.Description);

            bool plainCreated = plain.TryUpdateByName([2], "New", "New description", out Typology.Classes.Typology? plain_New);

            Assert.True(plainCreated);
            Assert.IsType<Typology.Classes.Typology>(plain_New);
            Assert.Equal("New", plain_New.Name);
            Assert.Equal("New description", plain_New.Description);

            Core.xUnit.Query.SerializationCheck(typology);
        }
    }
}
