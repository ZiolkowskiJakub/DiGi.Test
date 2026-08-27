using DiGi.GIS.PostgreSQL.Classes;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the batched estimated-count reads answer the "not available" value rather than throwing when there is no connection.
        /// <para>Mirrors the convention the other converter guard facts follow, so a converter built with null connection data is a supported state rather than an accident.</para>
        /// </summary>
        [Fact]
        public async Task GetEstimatedCountsAsync_NoConnection_ReturnDefaults()
        {
            Assert.Null(await new OrtoDatasPostgreSQLConverter(null).GetEstimatedCountsAsync([5, 6]));
            Assert.Null(await new Building2DPostgreSQLConverter(null).GetEstimatedCountsAsync([5, 6]));
            Assert.Null(await new BuildingPostgreSQLConverter(null).GetEstimatedCountsAsync([5, 6]));
            Assert.Null(await new TerrainPointPostgreSQLConverter(null).GetEstimatedCountsAsync([5, 6]));

            Assert.Null(await OrtoDatasPostgreSQLConverter.GetEstimatedCountsAsync(null, [5, 6]));
            Assert.Null(await Building2DPostgreSQLConverter.GetEstimatedCountsAsync(null, [5, 6]));
            Assert.Null(await BuildingPostgreSQLConverter.GetEstimatedCountsAsync(null, [5, 6]));
            Assert.Null(await TerrainPointPostgreSQLConverter.GetEstimatedCountsAsync(null, [5, 6]));

            // The summing overloads keep their own convention, which is -1 rather than null.
            Assert.Equal(-1, await OrtoDatasPostgreSQLConverter.GetEstimatedCountAsync(null, [5, 6]));
            Assert.Equal(-1, await Building2DPostgreSQLConverter.GetEstimatedCountAsync(null, [5, 6]));
        }

        /// <summary>
        /// Reproduces the null-<c>countyIds</c> crash in the <c>Building2DReferencedObject</c> converter's summing overload.
        /// <para>Alone among the sibling converters that overload walked the collection with no null check, so a null argument threw <see cref="System.NullReferenceException"/> from inside the loop instead of answering -1 the way every other converter does. Every other overload guards, which is why this went unnoticed.</para>
        /// <para>This needs no database: the connection is never opened, because the guard under test decides the answer before a statement is built. Against the unfixed converter it throws rather than failing an assertion.</para>
        /// </summary>
        [Fact]
        public async Task GetEstimatedCountAsync_NullCountyIds_ReturnsMinusOne()
        {
            await using NpgsqlConnection npgsqlConnection = new();

            Building2DOccupancyDataPostgreSQLConverter building2DOccupancyDataPostgreSQLConverter = new(null);

            // The cast picks the summing overload - a bare null is ambiguous against the int? one.
            Assert.Equal(-1, await building2DOccupancyDataPostgreSQLConverter.GetEstimatedCountAsync(npgsqlConnection, (IEnumerable<int>)null!));
            Assert.Null(await building2DOccupancyDataPostgreSQLConverter.GetEstimatedCountsAsync(npgsqlConnection, null));
        }

        /// <summary>
        /// Reproduces the Issue #44 mixed state: one named county unanalysed, the other analysed.
        /// <para>Against the unfixed overload the plural sum silently keeps the analysed county's figure - a lower bound; the fixed overload answers -1 instead. The two scratch partitions are created and dropped around the read, so the fact is self-contained.</para>
        /// <para>Skipped by default: it executes an integration query requiring <c>GIS_PostgreSQL_Main.conf</c> pointing at a database. That conf resolves to a development database, so these figures describe that database and not the deployed estate.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a database before running.")]
        public async Task GetEstimatedCountAsync_MixedAnalysedState_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            OrtoDatasPostgreSQLConverter? ortoDatasPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<OrtoDatasPostgreSQLConverter>();
            Assert.NotNull(ortoDatasPostgreSQLConverter);

            const int countyId_Unanalysed = 99998;
            const int countyId_Analysed = 99999;

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ortoDatasPostgreSQLConverter.ConnectionData);
            Assert.NotNull(npgsqlConnection);
            await npgsqlConnection.OpenAsync();

            // Two scratch partitions: one created but never analysed (answers -1), one analysed and holding one row.
            await new NpgsqlCommand("DROP TABLE IF EXISTS \"orto_datas_99998\"; DROP TABLE IF EXISTS \"orto_datas_99999\"; CREATE TABLE \"orto_datas_99998\" (x integer); CREATE TABLE \"orto_datas_99999\" (x integer); INSERT INTO \"orto_datas_99999\" VALUES (1); ANALYZE \"orto_datas_99999\";", npgsqlConnection).ExecuteNonQueryAsync();

            try
            {
                long? count_Single = await ortoDatasPostgreSQLConverter.GetEstimatedCountAsync(countyId_Analysed);
                Assert.NotNull(count_Single);
                Assert.True(count_Single >= 1);

                // Against the unfixed overload this fails with the analysed county's count (a lower bound); the fixed overload answers -1.
                Assert.Equal(-1, await ortoDatasPostgreSQLConverter.GetEstimatedCountAsync([countyId_Unanalysed, countyId_Analysed]));
            }
            finally
            {
                await new NpgsqlCommand("DROP TABLE IF EXISTS \"orto_datas_99998\"; DROP TABLE IF EXISTS \"orto_datas_99999\";", npgsqlConnection).ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Verifies that the batched read agrees with the per-county singular read for the same counties, including the counties that have no partition and the counties whose partition has never been analysed.
        /// <para>This is the fact that establishes the rewrite is behaviour-preserving: the endpoints divide one of these sums by another, so an estimate that moved would move a published coverage factor.</para>
        /// <para>Skipped by default: it executes an integration query requiring <c>GIS_PostgreSQL_Main.conf</c> pointing at a database. That conf resolves to a development database, so these figures describe that database and not the deployed estate.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a database before running.")]
        public async Task GetEstimatedCountsAsync_MatchesSingular_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            List<int> countyIds = [];
            for (int i = 1; i <= 20; i++)
            {
                countyIds.Add(i);
            }

            Dictionary<int, long>? counts = await building2DPostgreSQLConverter.GetEstimatedCountsAsync(countyIds);
            Assert.NotNull(counts);

            foreach (int countyId in countyIds)
            {
                long? count_Singular = await building2DPostgreSQLConverter.GetEstimatedCountAsync(countyId);

                if (count_Singular is null)
                {
                    // No partition: the batched read leaves the county out rather than reporting a zero.
                    Assert.False(counts.ContainsKey(countyId));
                    continue;
                }

                Assert.True(counts.ContainsKey(countyId));
                Assert.Equal(count_Singular.Value, counts[countyId]);
            }
        }
    }
}
