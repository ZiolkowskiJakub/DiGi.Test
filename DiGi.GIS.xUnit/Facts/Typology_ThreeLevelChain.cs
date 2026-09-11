using DiGi.Core.IO.Table.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a three level chain over county name, occupancy and predicted year built produces the expected tree, naming each node after its column and bucket and filing the reference of every row that reaches it.
        /// <para>References accumulate upwards, so an ancestor holds those of its whole subtree while the root itself holds none.</para>
        /// </summary>
        [Fact]
        public void Typology_ThreeLevelChain()
        {
            Table table = TypologyTable();

            Typology.Classes.Typology? typology = Create.Typology(table, TypologyFilter(), IO.Constants.Column.Reference);

            Assert.NotNull(typology);

            Assert.Empty(typology.References);
            Assert.Equal(7, Typology.Query.ReferenceSet(typology, true).Count);

            AssertNode(typology, [0], "County name Alpha", ["b1", "b2", "b3", "b4"]);
            AssertNode(typology, [1], "County name Beta", ["b5", "b6", "b7"]);

            AssertNode(typology, [0, 0], "Is occupied True", ["b1", "b2", "b3"]);
            AssertNode(typology, [0, 1], "Is occupied False", ["b4"]);

            AssertNode(typology, [0, 0, 0], "Predicted year built [0, 2003]", ["b1", "b2"]);
            AssertNode(typology, [0, 0, 1], "Predicted year built [2004, 2020]", ["b3"]);

            static void AssertNode(Typology.Classes.Typology typology, int[] values, string name, string[] references)
            {
                Typology.Classes.Typology? typology_Node = Typology.Query.SubTypology(typology, values);

                Assert.NotNull(typology_Node);
                Assert.Equal(name, typology_Node.Name);

                List<string> references_Node = typology_Node.References;
                references_Node.Sort();

                Assert.Equal(references, references_Node);
            }
        }
    }
}
