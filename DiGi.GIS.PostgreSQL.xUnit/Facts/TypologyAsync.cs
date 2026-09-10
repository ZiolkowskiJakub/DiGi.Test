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
        /// Verifies against a live database that Create.TypologyAsync pages a county partition and classifies what it read.
        /// <para>The figures asserted here describe whichever database the local connection configuration resolves to, which is a development one. It is not the production estate, and nothing measured here describes it.</para>
        /// </summary>
        [Fact(Skip = "Requires database connection")]
        public async Task TypologyAsync_Database_ClassifiesCountyPartition()
        {
            GISPostgreSQLConverterManager gISPostgreSQLConverterManager = new();
            Assert.NotNull(gISPostgreSQLConverterManager);

            BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingDataPostgreSQLConverter>();
            Assert.NotNull(buildingDataPostgreSQLConverter);

            int countyId = 55417;

            long count = await buildingDataPostgreSQLConverter.GetCountAsync(countyId);
            if (count <= 0)
            {
                // The partition holds nothing in this database, so there is nothing to classify.
                return;
            }

            Typology.Classes.Typology? typology = await buildingDataPostgreSQLConverter.TypologyAsync(TypologyFilter(), [countyId]);
            Assert.NotNull(typology);

            HashSet<string> references = DiGi.Typology.Query.ReferenceSet(typology, true);
            Assert.NotEmpty(references);

            // Every row carries a reference and a county name, so the first level accounts for all of them.
            Assert.True(references.Count <= count);

            List<Typology.Classes.Typology>? subTypologies = typology.SubTypologies;
            Assert.NotNull(subTypologies);
            Assert.NotEmpty(subTypologies);

            // Solving the same partition without references must produce the same structure and store nothing.
            Typology.Classes.Typology? typology_Metadata = await buildingDataPostgreSQLConverter.TypologyAsync(TypologyFilter(), [countyId], includeReferences: false);
            Assert.NotNull(typology_Metadata);
            Assert.Empty(DiGi.Typology.Query.ReferenceSet(typology_Metadata, true));
            Assert.Equal(DiGi.Typology.Query.TypologyPaths(typology, true).Count, DiGi.Typology.Query.TypologyPaths(typology_Metadata, true).Count);
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
