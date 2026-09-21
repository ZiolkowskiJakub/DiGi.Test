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
        /// Verifies that <see cref="OrtoDatasPostgreSQLConverter.GetYearsByReferenceAsync(NpgsqlConnection, string, int?, bool, int, System.Threading.CancellationToken)"/> answers null when the connection is missing, without touching a database.
        /// </summary>
        [Fact]
        public async Task GetYearsByReferenceAsync_NullConnection_ReturnsNull()
        {
            List<short>? result = await OrtoDatasPostgreSQLConverter.GetYearsByReferenceAsync(null, "XUNIT-YRS-NULL", 5);
            Assert.Null(result);
        }

        /// <summary>
        /// Verifies, measured on the development database, that the years read answers exactly the years that hold a card, in stored order, and nothing else.
        /// <para>A building holding cards in 2004, 2010 and 2015 answers those three years; a building with no cards answers an empty list, not null. The read projects the <c>DateTime</c> of the stored <c>Values</c> elements and never reads the <c>Bytes</c>.</para>
        /// <para>Skipped by default: it seeds scratch county 990101 and needs <c>GIS_PostgreSQL_Main.conf</c> beside the test assembly pointing at a scratch database - never the deployed one.</para>
        /// </summary>
        [Fact(Skip = "Seeds scratch county 990101. Point GIS_PostgreSQL_Main.conf at a scratch database before running.")]
        public async Task YearsByReference_OnlyYearsWithImagery_DevDb()
        {
            (NpgsqlConnection? npgsqlConnection, _) = await ScratchConnectionAsync();
            Assert.NotNull(npgsqlConnection);

            int countyId = 0;
            try
            {
                countyId = await SeedScratchCountyAsync(npgsqlConnection);

                // A building holding cards in three distinct years, and one holding none.
                await SeedBuilding2DAsync(npgsqlConnection, countyId, "XUNIT-YRS-MULTI");
                string objectJson_Multi = "{\"_type\":\"DiGi.GIS.Classes.OrtoDatas,DiGi.GIS\",\"Guid\":\"11111111-0000-0000-0000-000000000001\",\"Reference\":\"XUNIT-YRS-MULTI\",\"Values\":[{\"Bytes\":[1,2,3],\"DateTime\":\"2004-01-01T00:00:00\",\"Scale\":2.5},{\"Bytes\":[1,2,3],\"DateTime\":\"2010-01-01T00:00:00\",\"Scale\":2.5},{\"Bytes\":[1,2,3],\"DateTime\":\"2015-01-01T00:00:00\",\"Scale\":2.5}]}";
                await ExecuteAsync(npgsqlConnection, $"DELETE FROM orto_datas WHERE county_id = {countyId} AND reference = 'XUNIT-YRS-MULTI';");
                await ExecuteAsync(npgsqlConnection, $"INSERT INTO orto_datas (county_id, reference, object) VALUES ({countyId}, 'XUNIT-YRS-MULTI', '{objectJson_Multi}'::jsonb);");

                await SeedBuilding2DAsync(npgsqlConnection, countyId, "XUNIT-YRS-EMPTY");
                await SeedOrtoDatasAsync(npgsqlConnection, countyId, "XUNIT-YRS-EMPTY", null);

                List<short>? years_Multi = await OrtoDatasPostgreSQLConverter.GetYearsByReferenceAsync(npgsqlConnection, "XUNIT-YRS-MULTI", countyId);
                Assert.NotNull(years_Multi);
                Assert.Equal([2004, 2010, 2015], years_Multi);

                List<short>? years_Empty = await OrtoDatasPostgreSQLConverter.GetYearsByReferenceAsync(npgsqlConnection, "XUNIT-YRS-EMPTY", countyId);
                Assert.NotNull(years_Empty);
                Assert.Empty(years_Empty);

                // The fallback re-runs by reference alone on the same connection after the county-scoped read found
                // nothing; it threw NpgsqlOperationInProgressException while the first reader was still open (DiGi.GIS.PostgreSQL#91).
                List<short>? years_Fallback = await OrtoDatasPostgreSQLConverter.GetYearsByReferenceAsync(npgsqlConnection, "XUNIT-YRS-MULTI", countyId + 1_000_000, fallbackByReference: true);
                Assert.NotNull(years_Fallback);
                Assert.Equal([2004, 2010, 2015], years_Fallback);
            }
            finally
            {
                await CleanupScratchCountyAsync(npgsqlConnection, countyId);
                npgsqlConnection?.Dispose();
            }
        }
    }
}
