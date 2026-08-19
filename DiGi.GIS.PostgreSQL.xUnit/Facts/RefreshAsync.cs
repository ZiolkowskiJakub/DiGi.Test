using DiGi.GIS.PostgreSQL.Classes;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that no stored subdivision is left without a parent chain after
        /// <see cref="AdministrativeAreal2DPostgreSQLConverter.RefreshAsync(PostgreSQLAdministrativeAreal2DRefreshOptions, System.IProgress{long}, System.Threading.CancellationToken)"/> has run.
        /// <para>91 rows used to store a null chain: 87 subdivisions of Poznan (<c>3064</c>), whose county holds no municipality feature in
        /// BDOT10k at all, and 4 settlements in <c>2412</c> / <c>3003</c> / <c>3015</c> whose sample point lands in a gap between
        /// municipality polygons. The four keep a real municipality, so they are asserted separately. See
        /// https://github.com/ZiolkowskiJakub/DiGi.GIS.PostgreSQL/issues/14.</para>
        /// <para>A further 36 rows stored a parent chain that was simply wrong - 26 of Poznan's filed under county <c>3021</c>, and 10
        /// elsewhere including one in the wrong voivodeship - which a null check cannot see. Every subdivision's county must therefore
        /// carry the code its own code starts with. See https://github.com/ZiolkowskiJakub/DiGi.GIS.PostgreSQL/issues/15.</para>
        /// <para>Skipped by default: it queries a populated database and only means anything after a refresh run against it.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a refreshed database before running.")]
        public async Task RefreshAsync_ParentChain_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            ConnectionData? connectionData = administrativeAreal2DPostgreSQLConverter.ConnectionData;
            Assert.NotNull(connectionData);

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(connectionData);
            Assert.NotNull(npgsqlConnection);

            await npgsqlConnection.OpenAsync();

            async Task<long> CountAsync(string condition)
            {
                await using NpgsqlCommand npgsqlCommand = new($"SELECT COUNT(*) FROM {Constants.TableName.AdministrativeAreal2D} WHERE {condition};", npgsqlConnection);

                object? result = await npgsqlCommand.ExecuteScalarAsync();
                Assert.NotNull(result);

                return (long)result;
            }

            // Every subdivision reaches a county, and through it the rest of the chain.
            Assert.Equal(0, await CountAsync("type_id = 4 AND county_id IS NULL"));
            Assert.Equal(0, await CountAsync("type_id = 4 AND voivodeship_id IS NULL"));
            Assert.Equal(0, await CountAsync("type_id = 4 AND country_id IS NULL"));

            // The border slivers keep the municipality they actually sit in - dropping to the county is the
            // answer for Poznan, not for them.
            Assert.Equal(0, await CountAsync("type_id = 4 AND municipality_id IS NULL AND name IN ('Przegędza', 'Majdany', 'Kłecko-Kolonia', 'Separówko')"));

            // Poznan holds no municipality, so its subdivisions are filed straight under the county - all 113 of
            // them, none of which was correct before.
            Assert.Equal(113, await CountAsync("type_id = 4 AND municipality_id IS NULL AND county_id IN (SELECT id FROM administrative_areal_2d WHERE type_id = 2 AND code = '3064')"));

            // No subdivision may sit under a county its own code does not name. This subsumes the null check
            // above and is the only thing that catches a row filed under the wrong neighbour.
            await using NpgsqlCommand npgsqlCommand_County = new($@"
                SELECT COUNT(*)
                FROM {Constants.TableName.AdministrativeAreal2D} s
                JOIN {Constants.TableName.AdministrativeAreal2D} c ON c.id = s.county_id
                WHERE s.type_id = 4 AND LEFT(s.code, 4) <> c.code;", npgsqlConnection);

            Assert.Equal(0L, await npgsqlCommand_County.ExecuteScalarAsync());
        }
    }
}
