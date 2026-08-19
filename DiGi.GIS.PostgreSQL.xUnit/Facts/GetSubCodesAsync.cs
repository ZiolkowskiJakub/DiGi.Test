using DiGi.GIS.PostgreSQL.Classes;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="AdministrativeAreal2DPostgreSQLConverter.GetSubCodesAsync(NpgsqlConnection?, string?, System.Threading.CancellationToken)"/>
        /// and <see cref="AdministrativeAreal2DPostgreSQLConverter.GetSubCodesAsync(string?, System.Threading.CancellationToken)"/>
        /// return an empty set when given null or whitespace input codes, and null when the connection is null.
        /// </summary>
        [Fact]
        public async Task GetSubCodesAsync_NullOrWhitespace_ReturnsEmpty()
        {
            HashSet<string>? subCodes_NullConnection = await AdministrativeAreal2DPostgreSQLConverter.GetSubCodesAsync(null, "2212");
            Assert.Null(subCodes_NullConnection);

            HashSet<string>? subCodes_NullCode = await AdministrativeAreal2DPostgreSQLConverter.GetSubCodesAsync((NpgsqlConnection?)null, null);
            Assert.Null(subCodes_NullCode);

            AdministrativeAreal2DPostgreSQLConverter administrativeAreal2DPostgreSQLConverter = new(null);

            HashSet<string>? subCodes_Empty = await administrativeAreal2DPostgreSQLConverter.GetSubCodesAsync(string.Empty);
            Assert.NotNull(subCodes_Empty);
            Assert.Empty(subCodes_Empty);

            HashSet<string>? subCodes_Whitespace = await administrativeAreal2DPostgreSQLConverter.GetSubCodesAsync("   ");
            Assert.NotNull(subCodes_Whitespace);
            Assert.Empty(subCodes_Whitespace);

            HashSet<string>? subCodes_Null = await administrativeAreal2DPostgreSQLConverter.GetSubCodesAsync(null);
            Assert.NotNull(subCodes_Null);
            Assert.Empty(subCodes_Null);
        }

        /// <summary>
        /// Verifies that <see cref="AdministrativeAreal2DPostgreSQLConverter.GetSubCodesAsync(string?, System.Threading.CancellationToken)"/>
        /// excludes the exact query code match and returns only sub-codes starting with the prefix.
        /// <para>Skipped by default: it executes an integration query requiring <c>GIS_PostgreSQL_Main.conf</c> pointing at a populated database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a database before running.")]
        public async Task GetSubCodesAsync_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            string queryCode = "2212";
            HashSet<string>? subCodes = await administrativeAreal2DPostgreSQLConverter.GetSubCodesAsync(queryCode);
            Assert.NotNull(subCodes);
            Assert.NotEmpty(subCodes);

            // The exact query code itself must be excluded
            Assert.DoesNotContain(queryCode, subCodes);

            // All returned codes must start with the query code prefix and have length greater than the query code
            foreach (string subCode in subCodes)
            {
                Assert.StartsWith(queryCode, subCode);
                Assert.NotEqual(queryCode, subCode);
            }
        }
    }
}
