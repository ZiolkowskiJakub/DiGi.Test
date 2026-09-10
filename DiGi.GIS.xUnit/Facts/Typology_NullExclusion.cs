using DiGi.Core.IO.Table.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a row holding no value at a range level is present under its ancestors and absent from every node below that level.
        /// <para>A range rule resolves nothing for a null value, and the solver then drops the row from that level and from everything under it. There is no catch-all bucket, so such a row is reachable only through a shallower node.</para>
        /// </summary>
        [Fact]
        public void Typology_NullExclusion()
        {
            Table table = TypologyTable();

            Typology.Classes.Typology? typology = Create.Typology(table, TypologyFilter(), IO.Constants.Column.Reference);
            Assert.NotNull(typology);

            Typology.Classes.Typology? typology_County = Typology.Query.SubTypology(typology, [1]);
            Assert.NotNull(typology_County);
            Assert.Equal("County name Beta", typology_County.Name);
            Assert.Contains("b7", typology_County.References);

            Typology.Classes.Typology? typology_Occupied = Typology.Query.SubTypology(typology_County, [0]);
            Assert.NotNull(typology_Occupied);
            Assert.Equal("Is occupied True", typology_Occupied.Name);
            Assert.Contains("b7", typology_Occupied.References);

            List<Typology.Classes.Typology>? subTypologies = typology_Occupied.SubTypologies;
            Assert.NotNull(subTypologies);
            Assert.NotEmpty(subTypologies);

            foreach (Typology.Classes.Typology subTypology in subTypologies)
            {
                Assert.DoesNotContain("b7", Typology.Query.ReferenceSet(subTypology, true));
            }

            Assert.Contains("b5", Typology.Query.ReferenceSet(typology_Occupied, true));
        }
    }
}
