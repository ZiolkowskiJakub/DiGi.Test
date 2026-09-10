using DiGi.Core.IO.Table.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the per-node extraction the classification exists for - a node's own references, the references of its whole subtree, and membership.
        /// <para>Also tests that clearing includeReferences leaves the structure identical while storing no references at all, which is the form to solve when the node to object association is held outside the tree.</para>
        /// </summary>
        [Fact]
        public void Typology_NodeExtraction()
        {
            Table table = TypologyTable();

            DiGi.Typology.Classes.Typology? typology = Create.Typology(table, TypologyFilter(), IO.Constants.Column.Reference);
            Assert.NotNull(typology);

            DiGi.Typology.Classes.Typology? typology_Occupied = DiGi.Typology.Query.SubTypology(typology, [0, 0]);
            Assert.NotNull(typology_Occupied);

            HashSet<string> references_Own = DiGi.Typology.Query.ReferenceSet(typology_Occupied, false);
            HashSet<string> references_Nested = DiGi.Typology.Query.ReferenceSet(typology_Occupied, true);

            Assert.Equal(3, references_Own.Count);
            Assert.Equal(references_Own, references_Nested);

            Assert.True(DiGi.Typology.Query.Contains(typology_Occupied, "b1"));
            Assert.False(DiGi.Typology.Query.Contains(typology_Occupied, "b4"));

            Assert.False(DiGi.Typology.Query.Contains(typology, "b1"));
            Assert.True(DiGi.Typology.Query.Contains(typology, "b1", true));

            DiGi.Typology.Classes.Typology? typology_Metadata = Create.Typology(table, TypologyFilter(), IO.Constants.Column.Reference, null, false);
            Assert.NotNull(typology_Metadata);

            Assert.Empty(DiGi.Typology.Query.ReferenceSet(typology_Metadata, true));

            List<DiGi.Typology.Classes.TypologyPath> typologyPaths = DiGi.Typology.Query.TypologyPaths(typology, true);
            List<DiGi.Typology.Classes.TypologyPath> typologyPaths_Metadata = DiGi.Typology.Query.TypologyPaths(typology_Metadata, true);

            Assert.NotEmpty(typologyPaths);
            Assert.Equal(typologyPaths.Count, typologyPaths_Metadata.Count);

            foreach (DiGi.Typology.Classes.TypologyPath typologyPath in typologyPaths)
            {
                DiGi.Typology.Classes.Typology? typology_Node = DiGi.Typology.Query.SubTypology(typology, typologyPath);
                DiGi.Typology.Classes.Typology? typology_Node_Metadata = DiGi.Typology.Query.SubTypology(typology_Metadata, typologyPath);

                Assert.NotNull(typology_Node);
                Assert.NotNull(typology_Node_Metadata);
                Assert.Equal(typology_Node.Name, typology_Node_Metadata.Name);
            }
        }
    }
}
