using DiGi.GIS.PostgreSQL.Classes;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="BuildingDataPostgreSQLConverter.GetUniqueValuesAsync{T}(NpgsqlConnection?, string?, int, DiGi.PostgreSQL.Table.Classes.FilterGroup?, int, System.Threading.CancellationToken)"/>
        /// and its instance overload return null when the connection is missing or no column is named.
        /// </summary>
        [Fact]
        public async Task GetUniqueValuesAsync_NullOrWhitespace_ReturnsNull()
        {
            BuildingDataPostgreSQLConverter buildingDataPostgreSQLConverter = new(null);

            IEnumerable<object?>? values_NullConnection = await buildingDataPostgreSQLConverter.GetUniqueValuesAsync<object?>((NpgsqlConnection?)null, "county_name", 5);
            Assert.Null(values_NullConnection);

            IEnumerable<object?>? values_NullColumn = await buildingDataPostgreSQLConverter.GetUniqueValuesAsync<object?>((string?)null, 5);
            Assert.Null(values_NullColumn);

            IEnumerable<object?>? values_EmptyColumn = await buildingDataPostgreSQLConverter.GetUniqueValuesAsync<object?>(string.Empty, 5);
            Assert.Null(values_EmptyColumn);

            IEnumerable<object?>? values_WhitespaceColumn = await buildingDataPostgreSQLConverter.GetUniqueValuesAsync<object?>("   ", 5);
            Assert.Null(values_WhitespaceColumn);
        }

        /// <summary>
        /// Verifies that a column identifier the stored table does not hold is rejected before any statement is built, whether or not a county is named.
        /// <para>This is a regression test rather than an input-validation nicety. The county overload used to write the caller's identifier straight into <c>SELECT DISTINCT</c>, the <c>WHERE</c> clause and <c>ORDER BY</c>, and that value arrives from the <c>columnuniqueid</c> query parameter of a public endpoint. An identifier cannot be parameterised, so the stored column list is the only guard, and the two branches have to answer alike: without a county the base method rejected an unknown column, with one the text reached PostgreSQL.</para>
        /// <para>Skipped by default: it executes an integration query requiring <c>GIS_PostgreSQL_Storage.conf</c> pointing at a populated database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Storage.conf at a database before running.")]
        public async Task GetUniqueValuesAsync_UnknownColumn_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingDataPostgreSQLConverter>();
            Assert.NotNull(buildingDataPostgreSQLConverter);

            int countyId = 55417;

            // A column the table does hold still answers, so the guard rejects rather than breaking the read.
            IEnumerable<object?>? values_Known = await buildingDataPostgreSQLConverter.GetUniqueValuesAsync<object?>("county_name", countyId);
            Assert.NotNull(values_Known);

            // The same identifier, with and without a county, has to be rejected the same way.
            IEnumerable<object?>? values_Unknown_NoCounty = await buildingDataPostgreSQLConverter.GetUniqueValuesAsync<object?>("no_such_column");
            Assert.Null(values_Unknown_NoCounty);

            IEnumerable<object?>? values_Unknown_County = await buildingDataPostgreSQLConverter.GetUniqueValuesAsync<object?>("no_such_column", countyId);
            Assert.Null(values_Unknown_County);

            // An identifier carrying SQL punctuation is not a column name, so it is rejected on the list rather
            // than escaped. Kept read-only on purpose - the assertion is that nothing reaches the database.
            IEnumerable<object?>? values_Punctuation = await buildingDataPostgreSQLConverter.GetUniqueValuesAsync<object?>("county_name\" FROM building_data --", countyId);
            Assert.Null(values_Punctuation);
        }
    }
}
