using DiGi.Core.IO.Table.Classes;
using DiGi.Typology.Classes;
using DiGi.Typology.Visual.Classes;
using System.Collections.Generic;

namespace DiGi.Typology.Visual.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that Create.VisualTypology solves the same structure as DiGi.GIS Create.Typology over the same table and an equivalent chain, the only difference being the appearance each Visual node also carries.
        /// <para>Both trees are walked filing key by filing key: names, descriptions, references and indexes are asserted equal at every level, with the references compared as ordered sets because a node holds them in a set. The walk proves the reference-column identity matches too, since the references are its cell values. Solving without references is asserted against the base solver's own metadata-only form, appearance intact.</para>
        /// </summary>
        [Fact]
        public void VisualTypology_StructureParity()
        {
            Table table = VisualTypologyTable();

            Column column_Reference = new("building_reference", typeof(string));

            Typology.Classes.Typology? typology = GIS.Create.Typology(table, PlainTypologyFilter(), column_Reference);
            VisualTypology? visualTypology = Typology.Visual.Create.VisualTypology(table, VisualTypologyFilter(), column_Reference);

            Assert.NotNull(typology);
            Assert.NotNull(visualTypology);

            AssertNode(typology, visualTypology);

            // Solving without references answers structure and node metadata only - the appearance included.
            Typology.Classes.Typology? typology_Metadata = GIS.Create.Typology(table, PlainTypologyFilter(), null, null, false);
            VisualTypology? visualTypology_Metadata = Typology.Visual.Create.VisualTypology(table, VisualTypologyFilter(), null, null, false);

            Assert.NotNull(typology_Metadata);
            Assert.NotNull(visualTypology_Metadata);

            Assert.Empty(visualTypology_Metadata.ReferenceSet(true));
            AssertNode(typology_Metadata, visualTypology_Metadata);

            Assert.Contains(SubTypologies(visualTypology_Metadata), subTypology => string.Equals(subTypology.Name, "occupancy Residential", StringComparison.Ordinal) && subTypology.TypologyItem?.Appearance is not null);
            Assert.Contains(SubTypologies(visualTypology_Metadata), subTypology => string.Equals(subTypology.Name, "occupancy Agricultural", StringComparison.Ordinal) && subTypology.TypologyItem?.Appearance is null);

            void AssertNode(Typology.Classes.Typology typology_Level, VisualTypology visualTypology_Level)
            {
                Assert.Equal(typology_Level.Name, visualTypology_Level.Name);
                Assert.Equal(typology_Level.Description, visualTypology_Level.Description);

                List<string> references = [.. typology_Level.References.OrderBy(x => x)];
                Assert.Equal(references, [.. visualTypology_Level.References.OrderBy(x => x)]);

                List<int> indexes = [.. typology_Level.Indexes.OrderBy(x => x)];
                Assert.Equal(indexes, [.. visualTypology_Level.Indexes.OrderBy(x => x)]);

                foreach (int index in indexes)
                {
                    Assert.NotNull(typology_Level[index]);
                    Assert.NotNull(visualTypology_Level[index]);

                    AssertNode(typology_Level[index]!, visualTypology_Level[index]!);
                }
            }

            static List<VisualTypology> SubTypologies(VisualTypology typology)
            {
                Assert.NotNull(typology.SubTypologies);
                return typology.SubTypologies!;
            }
        }
    }
}
