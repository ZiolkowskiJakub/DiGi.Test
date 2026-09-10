using System.Collections.Generic;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a sub-typology is resolved by the index it is filed under rather than by the index its
        /// own path reports, and that the two survive a clone and a serialization round trip.
        /// <para>Create.Typology files a sub-typology carrying no path, or one whose index is already taken,
        /// under the next free index, so the filing key and the reported path deliberately disagree here.
        /// This is the divergence a lookup driven by the reported path would silently get wrong.</para>
        /// </summary>
        [Fact]
        public void Typology_SubTypology()
        {
            Typology.Classes.Typology typology_Taken = new(new Typology.Classes.TypologyItem([0], "Taken", null));
            Typology.Classes.Typology typology_Pathless = new("Pathless", null);
            Typology.Classes.Typology typology_Colliding = new(new Typology.Classes.TypologyItem([0], "Colliding", null));

            Typology.Classes.Typology? typology = DiGi.Typology.Create.Typology(new Typology.Classes.TypologyItem([9], "Root", null), [typology_Taken, typology_Pathless, typology_Colliding]);

            Assert.NotNull(typology);

            List<int> indexes = [0, 1, 2];

            // Indexes reports dictionary keys, whose enumeration order is unspecified - compare it sorted.
            List<int> indexes_Typology = typology.Indexes;
            indexes_Typology.Sort();

            Assert.Equal(indexes, indexes_Typology);

            Assert.Equal("Taken", typology.SubTypology([0])?.Name);
            Assert.Equal("Pathless", typology.SubTypology([1])?.Name);
            Assert.Equal("Colliding", typology.SubTypology([2])?.Name);

            // The third sub-typology is filed under [2] while still reporting [0] as its own path.
            Assert.Equal(0, typology.SubTypology([2])?.TypologyPath?.Index);

            // A pathless sub-typology contributes no path at all, so only two of the three are reported.
            Assert.Equal(2, typology.TypologyPaths().Count);

            Typology.Classes.Typology? typology_Clone = Core.Query.Clone(typology);

            Assert.NotNull(typology_Clone);
            Assert.Equal(typology, typology_Clone);
            List<int> indexes_Clone = typology_Clone.Indexes;
            indexes_Clone.Sort();

            Assert.Equal(indexes, indexes_Clone);
            Assert.Equal("Taken", typology_Clone.SubTypology([0])?.Name);
            Assert.Equal("Pathless", typology_Clone.SubTypology([1])?.Name);
            Assert.Equal("Colliding", typology_Clone.SubTypology([2])?.Name);

            Core.xUnit.Query.SerializationCheck(typology);
        }
    }
}
