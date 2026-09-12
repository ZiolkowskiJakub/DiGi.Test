using DiGi.Typology.Classes;
using DiGi.Typology.Visual.Classes;

namespace DiGi.Typology.Visual.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests <see cref="Classes.VisualTypology"/> as a tree: appearances on the root, a child and a grandchild take
        /// part in equality, ordering and the hash; the setters create a visual item; the appearance survives
        /// <see cref="DiGi.Typology.Modify.RemoveReferences{TTypology, TTypologyItem}(Typology{TTypology, TTypologyItem}, bool)"/>,
        /// <see cref="Core.Query.Clone{TSerializableObject}(TSerializableObject)"/>, the copy constructor and a string
        /// round trip; and the generic Query and Modify extensions bind to the derived typology.
        /// <para>Appearance is metadata, not link data - it is exactly what a reference-free tree must keep.</para>
        /// </summary>
        [Fact]
        public void VisualTypology()
        {
            static Classes.VisualTypology CreateVisualTypology(System.Drawing.Color color)
            {
                Classes.VisualTypology visualTypology = new(new Classes.VisualTypologyItem(null, "Root", "All buildings", Create.TypologyAppearance(color)));

                Assert.True(visualTypology.AddReference("ref-1"));
                Assert.True(visualTypology.AddReference("ref-2"));

                visualTypology[0] = new Classes.VisualTypology(new Classes.VisualTypologyItem([0], "Residential", null, Create.TypologyAppearance(color, 2)));
                visualTypology[3] = new Classes.VisualTypology(new Classes.VisualTypologyItem([3], "Industrial", null, null));

                Classes.VisualTypology? visualTypology_Child = visualTypology[0];

                Assert.NotNull(visualTypology_Child);
                Assert.True(visualTypology_Child.AddReference("ref-3"));

                visualTypology_Child[0] = new Classes.VisualTypology(new Classes.VisualTypologyItem([0, 0], "Detached", null, Create.TypologyAppearance(color, 3)));

                return visualTypology;
            }

            Classes.VisualTypology visualTypology_1 = CreateVisualTypology(System.Drawing.Color.Red);
            Classes.VisualTypology visualTypology_2 = CreateVisualTypology(System.Drawing.Color.Red);
            Classes.VisualTypology visualTypology_Blue = CreateVisualTypology(System.Drawing.Color.Blue);

            Assert.Equal([0, 3], visualTypology_1.Indexes);
            Assert.NotNull(visualTypology_1.TypologyItem?.Appearance);
            Assert.NotNull(visualTypology_1[0]?.TypologyItem?.Appearance);
            Assert.Null(visualTypology_1[3]?.TypologyItem?.Appearance);
            Assert.NotNull(visualTypology_1[0]?[0]?.TypologyItem?.Appearance);

            // The appearances take part in equality, ordering and the hash of the whole tree.
            Assert.True(visualTypology_1.Equals(visualTypology_2));
            Assert.True(visualTypology_1 == visualTypology_2);
            Assert.Equal(0, visualTypology_1.CompareTo(visualTypology_2));
            Assert.Equal(visualTypology_1.GetHashCode(), visualTypology_2.GetHashCode());

            Assert.False(visualTypology_1.Equals(visualTypology_Blue));
            Assert.True(visualTypology_1 != visualTypology_Blue);
            Assert.NotEqual(0, visualTypology_1.CompareTo(visualTypology_Blue));
            Assert.NotEqual(visualTypology_1.GetHashCode(), visualTypology_Blue.GetHashCode());

            // A tree differing only in a grandchild's appearance is a different tree.
            Classes.VisualTypology visualTypology_3 = CreateVisualTypology(System.Drawing.Color.Red);
            Classes.VisualTypologyItem? visualTypologyItem_Grandchild = visualTypology_3[0]?[0]?.TypologyItem;

            Assert.NotNull(visualTypologyItem_Grandchild);
            Assert.True(visualTypology_3 == visualTypology_1);

            visualTypologyItem_Grandchild.Appearance = null;

            Assert.True(visualTypology_3 != visualTypology_1);

            // The setters create a visual item when there is none.
            Classes.VisualTypology visualTypology_Empty = new((Classes.VisualTypologyItem?)null);

            Assert.Null(visualTypology_Empty.Name);
            Assert.Null(visualTypology_Empty.TypologyItem);

            visualTypology_Empty.Name = "Named";

            Assert.Equal("Named", visualTypology_Empty.Name);
            Assert.IsType<Classes.VisualTypologyItem>(visualTypology_Empty.TypologyItem);
            Assert.Null(visualTypology_Empty.TypologyItem?.Appearance);

            Classes.VisualTypology visualTypology_Named = new("Named", "Described");

            Assert.Equal("Named", visualTypology_Named.Name);
            Assert.Equal("Described", visualTypology_Named.Description);
            Assert.Null(visualTypology_Named.TypologyItem?.Appearance);

            // Removing the references keeps every appearance: it is metadata, not link data.
            Classes.VisualTypology visualTypology_Stripped = new(visualTypology_1);

            Assert.True(visualTypology_Stripped.RemoveReferences());
            Assert.Empty(visualTypology_Stripped.ReferenceSet(true));
            Assert.Equal(Core.Convert.ToSystem_String(visualTypology_1.TypologyItem?.Appearance), Core.Convert.ToSystem_String(visualTypology_Stripped.TypologyItem?.Appearance));
            Assert.Equal(Core.Convert.ToSystem_String(visualTypology_1[0]?[0]?.TypologyItem?.Appearance), Core.Convert.ToSystem_String(visualTypology_Stripped[0]?[0]?.TypologyItem?.Appearance));
            Assert.True(visualTypology_Stripped != visualTypology_1);

            // The copy constructor and Core.Query.Clone copy the tree with its appearances rather than aliasing them.
            Classes.VisualTypology visualTypology_Copy = new(visualTypology_1);

            Assert.True(visualTypology_Copy == visualTypology_1);
            Assert.NotSame(visualTypology_1[0], visualTypology_Copy[0]);
            Assert.NotSame(visualTypology_1.TypologyItem?.Appearance, visualTypology_Copy.TypologyItem?.Appearance);

            Classes.VisualTypology? visualTypology_Clone = Core.Query.Clone(visualTypology_1);

            Assert.NotNull(visualTypology_Clone);
            Assert.True(visualTypology_Clone == visualTypology_1);
            Assert.NotNull(visualTypology_Clone[0]?[0]?.TypologyItem?.Appearance);

            // The generic Query and Modify extensions bind to the derived typology.
            Assert.True(visualTypology_1.Contains("ref-3", true));
            Assert.False(visualTypology_1.Contains("ref-3"));
            Assert.Equal(["ref-1", "ref-2", "ref-3"], visualTypology_1.ReferenceSet(true).OrderBy(x => x));
            Assert.Equal("Detached", visualTypology_1.SubTypology([0, 0])?.Name);
            Assert.True(visualTypology_1.TryGetLastIndex(out int index_Last));
            Assert.Equal(3, index_Last);
            Assert.True(visualTypology_1.TryGetTypologies("Industrial", out List<Classes.VisualTypology>? visualTypologies));
            Assert.Single(visualTypologies!);
            Assert.Equal(3, visualTypology_1.TypologyPaths(true).Count);

            Classes.VisualTypology visualTypology_Filed = new((Classes.VisualTypologyItem?)null);

            Assert.Equal(2, visualTypology_Filed.AddSubTypologies([visualTypology_1[0], visualTypology_1[0]]));
            Assert.Equal([0, 1], visualTypology_Filed.Indexes);
            Assert.NotSame(visualTypology_1[0], visualTypology_Filed[0]);
            Assert.NotNull(visualTypology_Filed[1]?.TypologyItem?.Appearance);

            // The whole tree, its references and every appearance round-trip.
            Core.xUnit.Query.SerializationCheck(visualTypology_1);

            Classes.VisualTypology? visualTypology_RoundTrip = Core.Convert.ToDiGi<Classes.VisualTypology>(Core.Convert.ToSystem_String(visualTypology_1))?.FirstOrDefault();

            Assert.NotNull(visualTypology_RoundTrip);
            Assert.True(visualTypology_RoundTrip == visualTypology_1);
            Assert.Equal([0, 3], visualTypology_RoundTrip.Indexes);
            Assert.Equal(["ref-3"], visualTypology_RoundTrip[0]?.References);
            Assert.Equal("Detached", visualTypology_RoundTrip[0]?[0]?.Name);
            Assert.Equal(Core.Convert.ToSystem_String(visualTypology_1[0]?[0]?.TypologyItem?.Appearance), Core.Convert.ToSystem_String(visualTypology_RoundTrip[0]?[0]?.TypologyItem?.Appearance));
        }
    }
}
