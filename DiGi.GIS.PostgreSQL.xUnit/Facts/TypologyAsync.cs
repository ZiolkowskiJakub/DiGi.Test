using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using DiGi.Typology.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that Create.TypologyAsync answers null rather than throwing for every input it cannot classify, and that it refuses before reaching for a connection.
        /// <para>The partition list is deliberately not optional: a county identifier addresses one polygon part, and defaulting it to every partition would read millions of rows into memory.</para>
        /// </summary>
        [Fact]
        public async Task TypologyAsync_NullOrEmpty_ReturnsNull()
        {
            BuildingDataPostgreSQLConverter buildingDataPostgreSQLConverter = new(null);

            List<int> countyIds = [55417];

            Assert.Null(await Create.TypologyAsync(null, TypologyFilter(), countyIds));
            Assert.Null(await buildingDataPostgreSQLConverter.TypologyAsync(null, countyIds));
            Assert.Null(await buildingDataPostgreSQLConverter.TypologyAsync(TypologyFilter(), null));
            Assert.Null(await buildingDataPostgreSQLConverter.TypologyAsync(TypologyFilter(), countyIds, pageSize: 0));

            // A chain naming no column resolves to no projection at all, so it is refused before a connection.
            Assert.Null(await buildingDataPostgreSQLConverter.TypologyAsync(new ColumnTypologyFilter<Column>(), countyIds));

            // The converter holds no connection data, so a well-formed call still returns null rather than throwing.
            Assert.Null(await buildingDataPostgreSQLConverter.TypologyAsync(TypologyFilter(), countyIds));
            Assert.Null(await buildingDataPostgreSQLConverter.TypologyAsync(TypologyFilter(), []));
        }

        /// <summary>
        /// Verifies against a live database that Create.TypologyAsync pages a county partition across multiple pages and classifies what it read.
        /// <para>The page size is a function of the partition's row count so the read spans roughly twenty pages, exercising the keyset cursor, the multi-page accumulation and the per-page column mapping rather than the single-page fast path. The defect this guards is a partial read: the loop stopping after the first page and reporting it as the whole, which would hold at most one page's worth of references and fail the count check below.</para>
        /// <para>The "one connection for the whole run" property is structural: the loop passes a single open connection to the connection-taking PullAsync overload. It is not observable through the public API, so it is verified by reading the code rather than asserted here.</para>
        /// <para>The figures asserted here describe whichever database the local connection configuration resolves to, which is a development one. It is not the production estate, and nothing measured here describes it.</para>
        /// </summary>
        [Fact(Skip = "Requires database connection")]
        public async Task TypologyAsync_Database_ClassifiesCountyPartition()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingDataPostgreSQLConverter>();
            Assert.NotNull(buildingDataPostgreSQLConverter);

            int countyId = 204;

            long count = await buildingDataPostgreSQLConverter.GetCountAsync(countyId);
            if (count <= 0)
            {
                // The partition holds nothing in this database, so there is nothing to classify.
                return;
            }

            // Ceiling of count / 20, at least 1: the partition spans roughly twenty pages rather than one.
            int pageSize = (int)((count + 19) / 20);

            Typology.Classes.Typology? typology = await buildingDataPostgreSQLConverter.TypologyAsync(TypologyFilter(), [countyId], pageSize: pageSize);
            Assert.NotNull(typology);

            HashSet<string> references = Typology.Query.ReferenceSet(typology, true);
            Assert.NotEmpty(references);

            // Ground truth: every reference the partition holds. The classification may drop rows at a filter level, but it never invents one.
            HashSet<string> full = await buildingDataPostgreSQLConverter.GetReferencesByCountyIdAsync(countyId) ?? [];
            Assert.True(full.Count > 0);

            // No reference the typology claims that the partition does not actually hold.
            HashSet<string> phantom = [];
            phantom.UnionWith(references);
            phantom.ExceptWith(full);
            Assert.Empty(phantom);

            // A partial read stops after the first page and holds at most one page's worth; a full read holds the whole partition.
            Assert.True(references.Count > pageSize, $"Read {references.Count} of {full.Count} references with a page size of {pageSize}; a single-page read would hold at most {pageSize}.");
            Assert.True(references.Count <= count, $"Read {references.Count} references but the partition holds only {count} rows.");

            List<Typology.Classes.Typology>? subTypologies = typology.SubTypologies;
            Assert.NotNull(subTypologies);
            Assert.NotEmpty(subTypologies);

            // Solving the same partition without references must produce the same structure and store nothing.
            Typology.Classes.Typology? typology_Metadata = await buildingDataPostgreSQLConverter.TypologyAsync(TypologyFilter(), [countyId], includeReferences: false, pageSize: pageSize);
            Assert.NotNull(typology_Metadata);
            Assert.Empty(Typology.Query.ReferenceSet(typology_Metadata, true));
            Assert.Equal(Typology.Query.TypologyPaths(typology, true).Count, Typology.Query.TypologyPaths(typology_Metadata, true).Count);
        }

        /// <summary>
        /// Verifies against a live database that two partitions passed in one call accumulate into a single typology tree, each contributing the rows it holds.
        /// <para>A county identifier addresses one polygon part rather than a county, so the two identifiers address two parts and the classification must merge whatever it read from both. A read that only reached the first partition, or that bailed out when it met the second, would miss the references that exist only under the other and fail the per-county membership check below.</para>
        /// <para>The figures asserted here describe whichever database the local connection configuration resolves to, which is a development one. It is not the production estate, and nothing measured here describes it.</para>
        /// </summary>
        [Fact(Skip = "Requires database connection")]
        public async Task TypologyAsync_Database_TwoPartitions_AccumulateIntoOneTree()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingDataPostgreSQLConverter>();
            Assert.NotNull(buildingDataPostgreSQLConverter);

            int countyId_First = 204;
            int countyId_Second = 5;

            long count_First = await buildingDataPostgreSQLConverter.GetCountAsync(countyId_First);
            long count_Second = await buildingDataPostgreSQLConverter.GetCountAsync(countyId_Second);
            if (count_First <= 0 && count_Second <= 0)
            {
                // Neither partition holds anything in this database, so there is nothing to accumulate.
                return;
            }

            HashSet<string> full_First = [];
            if (count_First > 0)
            {
                full_First = await buildingDataPostgreSQLConverter.GetReferencesByCountyIdAsync(countyId_First) ?? [];
            }

            HashSet<string> full_Second = [];
            if (count_Second > 0)
            {
                full_Second = await buildingDataPostgreSQLConverter.GetReferencesByCountyIdAsync(countyId_Second) ?? [];
            }

            HashSet<string> full_Union = [];
            full_Union.UnionWith(full_First);
            full_Union.UnionWith(full_Second);

            // Small enough to cross at least one page boundary, so the per-partition cursor is exercised as well.
            long count_Largest = count_First > count_Second ? count_First : count_Second;
            int pageSize = (int)((count_Largest + 9) / 10);

            Typology.Classes.Typology? typology = await buildingDataPostgreSQLConverter.TypologyAsync(TypologyFilter(), [countyId_First, countyId_Second], pageSize: pageSize);
            Assert.NotNull(typology);

            List<Typology.Classes.Typology>? subTypologies = typology.SubTypologies;
            Assert.NotNull(subTypologies);
            Assert.NotEmpty(subTypologies);

            HashSet<string> references = Typology.Query.ReferenceSet(typology, true);

            // Nothing the two named parts do not hold: the read stayed inside them.
            HashSet<string> phantom = [];
            phantom.UnionWith(references);
            phantom.ExceptWith(full_Union);
            Assert.Empty(phantom);

            // When the two parts hold distinct references, each must contribute one of its own, so both were actually read.
            if (count_First > 0)
            {
                HashSet<string> only_First = [];
                only_First.UnionWith(full_First);
                only_First.ExceptWith(full_Second);
                if (only_First.Count > 0)
                {
                    HashSet<string> present_First = [];
                    present_First.UnionWith(only_First);
                    present_First.IntersectWith(references);
                    Assert.True(present_First.Count > 0, "The first partition held references the second did not, yet none of them reached the tree.");
                }
            }

            if (count_Second > 0)
            {
                HashSet<string> only_Second = [];
                only_Second.UnionWith(full_Second);
                only_Second.ExceptWith(full_First);
                if (only_Second.Count > 0)
                {
                    HashSet<string> present_Second = [];
                    present_Second.UnionWith(only_Second);
                    present_Second.IntersectWith(references);
                    Assert.True(present_Second.Count > 0, "The second partition held references the first did not, yet none of them reached the tree.");
                }
            }
        }

        /// <summary>
        /// Verifies against a live database that a call handing the same partition identifier twice yields the same tree as a call handing it once.
        /// <para>A caller collecting the parts of a county code can hand the same part over twice. The loop deduplicates the identifiers before reading, so the duplicated call must be indistinguishable from the single one. The deduplication itself is structural: a node holds its references in a set, so a doubled read collapses into the same tree and is visible only in the wasted work, which is why the loop's identifier set is verified by reading the code rather than asserted here.</para>
        /// <para>The figures asserted here describe whichever database the local connection configuration resolves to, which is a development one. It is not the production estate, and nothing measured here describes it.</para>
        /// </summary>
        [Fact(Skip = "Requires database connection")]
        public async Task TypologyAsync_Database_DuplicateCountyIds_SameTreeAsOnce()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingDataPostgreSQLConverter>();
            Assert.NotNull(buildingDataPostgreSQLConverter);

            int countyId = 204;

            long count = await buildingDataPostgreSQLConverter.GetCountAsync(countyId);
            if (count <= 0)
            {
                // The partition holds nothing in this database, so there is nothing to hand over twice.
                return;
            }

            // Small enough to cross at least one page boundary, so neither call takes the single-page fast path.
            int pageSize = (int)((count + 19) / 20);

            Typology.Classes.Typology? typology_Once = await buildingDataPostgreSQLConverter.TypologyAsync(TypologyFilter(), [countyId], pageSize: pageSize);
            Assert.NotNull(typology_Once);

            Typology.Classes.Typology? typology_Duplicate = await buildingDataPostgreSQLConverter.TypologyAsync(TypologyFilter(), [countyId, countyId], pageSize: pageSize);
            Assert.NotNull(typology_Duplicate);

            HashSet<string> references_Once = Typology.Query.ReferenceSet(typology_Once, true);
            HashSet<string> references_Duplicate = Typology.Query.ReferenceSet(typology_Duplicate, true);
            Assert.NotEmpty(references_Once);

            // The duplicated call must hold exactly the references the single call holds - no more, no fewer.
            HashSet<string> only_Duplicate = [];
            only_Duplicate.UnionWith(references_Duplicate);
            only_Duplicate.ExceptWith(references_Once);
            Assert.Empty(only_Duplicate);

            HashSet<string> only_Once = [];
            only_Once.UnionWith(references_Once);
            only_Once.ExceptWith(references_Duplicate);
            Assert.Empty(only_Once);

            // The grouping levels must agree as well, so the two calls built the same tree rather than trees that merely hold the same references.
            Assert.Equal(Typology.Query.TypologyPaths(typology_Once, true).Count, Typology.Query.TypologyPaths(typology_Duplicate, true).Count);
        }

        /// <summary>
        /// Builds the filter chain the typology facts group building data by - county name, then occupancy, then predicted year built bucketed into three ranges.
        /// <para>The columns are declared here rather than taken from the shared GIS.IO constants so that this project need not reference DiGi.GIS.IO. That assembly's output folder carries copies of its own dependencies, and putting it on the reference search path binds this project's compilation against those copies instead of the ones it declares. Only a column's name matters to the classification, because a column's unique id is derived from it.</para>
        /// </summary>
        /// <returns>The root of the chain.</returns>
        private static ColumnTypologyFilter<Column> TypologyFilter()
        {
            return new ColumnTypologyFilter<Column>()
            {
                Value = new ExtendedColumn("County name", typeof(string), null, null),
                Rule = new UniqueValueFilterRule(),
                Filter = new ColumnTypologyFilter<Column>()
                {
                    Value = new ExtendedColumn("Is occupied", typeof(bool), null, null),
                    Rule = new UniqueValueFilterRule(),
                    Filter = new ColumnTypologyFilter<Column>()
                    {
                        Value = new ExtendedColumn("Predicted year built", typeof(ushort), null, null),
                        Rule = new IntegerRangeFilterRule([new Core.Classes.Range<int>(0, 2003), new Core.Classes.Range<int>(2004, 2020), new Core.Classes.Range<int>(2021, int.MaxValue)])
                    }
                }
            };
        }
    }
}
