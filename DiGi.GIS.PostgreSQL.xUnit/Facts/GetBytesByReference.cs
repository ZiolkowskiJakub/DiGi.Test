using DiGi.GIS.PostgreSQL.Classes;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="OrtoDatasPostgreSQLConverter.GetBytesByReferenceAsync(NpgsqlConnection, string, int?, short, bool, int, System.Threading.CancellationToken)"/> answers null when the connection is missing, without touching a database.
        /// </summary>
        [Fact]
        public async Task GetBytesByReferenceAsync_NullConnection_ReturnsNull()
        {
            byte[]? result = await OrtoDatasPostgreSQLConverter.GetBytesByReferenceAsync(null, "XUNIT-BYTES-NULL", 5, 2010);
            Assert.Null(result);
        }

        /// <summary>
        /// Verifies, measured on the development database, that the exact-year read answers the stored bytes for the matching year and null for a year the building does not hold.
        /// <para>The match is on the stored <c>DateTime</c> of the <c>Values</c> elements against <c>make_date(@year, 1, 1)</c> - the shape the producer stores - so a neighbouring year never answers a different building's image.</para>
        /// <para>Skipped by default: it seeds scratch county 990101 and needs <c>GIS_PostgreSQL_Main.conf</c> beside the test assembly pointing at a scratch database - never the deployed one.</para>
        /// </summary>
        [Fact(Skip = "Seeds scratch county 990101. Point GIS_PostgreSQL_Main.conf at a scratch database before running.")]
        public async Task BytesByReference_ExactYearOnly_DevDb()
        {
            (NpgsqlConnection? npgsqlConnection, _) = await ScratchConnectionAsync();
            Assert.NotNull(npgsqlConnection);

            int countyId = 0;
            try
            {
                countyId = await SeedScratchCountyAsync(npgsqlConnection);

                await SeedBuilding2DAsync(npgsqlConnection, countyId, "XUNIT-BYTES-MATCH");
                await SeedOrtoDatasAsync(npgsqlConnection, countyId, "XUNIT-BYTES-MATCH", "2010");

                // The matching year answers the stored bytes.
                byte[]? bytes_Match = await OrtoDatasPostgreSQLConverter.GetBytesByReferenceAsync(npgsqlConnection, "XUNIT-BYTES-MATCH", countyId, 2010);
                Assert.NotNull(bytes_Match);
                Assert.Equal([1, 2, 3], bytes_Match);

                // A year the building does not hold answers null, not a neighbour's image.
                byte[]? bytes_Miss = await OrtoDatasPostgreSQLConverter.GetBytesByReferenceAsync(npgsqlConnection, "XUNIT-BYTES-MATCH", countyId, 2007);
                Assert.Null(bytes_Miss);

                // The fallback re-runs by reference alone after the county-scoped read found nothing; the first reader
                // must be closed by then (DiGi.GIS.PostgreSQL#91).
                byte[]? bytes_Fallback = await OrtoDatasPostgreSQLConverter.GetBytesByReferenceAsync(npgsqlConnection, "XUNIT-BYTES-MATCH", countyId + 1_000_000, 2010, fallbackByReference: true);
                Assert.Equal([1, 2, 3], bytes_Fallback);
                Assert.Null(await OrtoDatasPostgreSQLConverter.GetBytesByReferenceAsync(npgsqlConnection, "XUNIT-BYTES-MATCH", countyId + 1_000_000, 2007, fallbackByReference: true));
            }
            finally
            {
                await CleanupScratchCountyAsync(npgsqlConnection, countyId);
                npgsqlConnection?.Dispose();
            }
        }
    }
}
