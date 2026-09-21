using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="OrtoDatasPostgreSQLConverter.GetRandomBuilding2DReferenceWithoutUserYearBuiltAsync"/> answers null in both overloads when no connection is available, without touching a database.
        /// </summary>
        [Fact]
        public async Task GetRandomBuilding2DReferenceWithoutUserYearBuiltAsync_NullConnection_ReturnsNull()
        {
            Building2DReference? result_Static = await OrtoDatasPostgreSQLConverter.GetRandomBuilding2DReferenceWithoutUserYearBuiltAsync(null);
            Assert.Null(result_Static);

            OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(null);
            Building2DReference? result_Instance = await ortoDatasPostgreSQLConverter.GetRandomBuilding2DReferenceWithoutUserYearBuiltAsync();
            Assert.Null(result_Instance);
        }

        /// <summary>
        /// Verifies that the <c>countyIds</c>-filtering overloads of <see cref="OrtoDatasPostgreSQLConverter.GetRandomBuilding2DReferenceWithoutUserYearBuiltAsync"/> answer null when no connection is available, without touching a database.
        /// </summary>
        [Fact]
        public async Task GetRandomBuilding2DReferenceWithoutUserYearBuiltAsync_CountyIds_NullConnection_ReturnsNull()
        {
            Building2DReference? result_Static = await OrtoDatasPostgreSQLConverter.GetRandomBuilding2DReferenceWithoutUserYearBuiltAsync(null, [1]);
            Assert.Null(result_Static);

            OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(null);
            Building2DReference? result_Instance = await ortoDatasPostgreSQLConverter.GetRandomBuilding2DReferenceWithoutUserYearBuiltAsync([1]);
            Assert.Null(result_Instance);
        }

        /// <summary>
        /// Verifies, measured on the development database, that a <c>countyIds</c> filter confines the draw to the requested <c>building_2d</c> parts.
        /// <para>The scratch county 990101 is one of hundreds of covered parts, so an unfiltered draw lands elsewhere with overwhelming probability; 20 filtered draws that all land on the scratch part is the differential that fails if the filter is dead. A part id that names nothing empties the pool and answers null; an empty filter behaves as the baseline.</para>
        /// <para>Skipped by default: it seeds scratch county 990101 and needs <c>GIS_PostgreSQL_Main.conf</c> beside the test assembly pointing at a scratch database - never the deployed one.</para>
        /// </summary>
        [Fact(Skip = "Seeds scratch county 990101. Point GIS_PostgreSQL_Main.conf at a scratch database before running.")]
        public async Task RandomBuilding2DReference_CountyIds_RestrictsToRequestedParts_DevDb()
        {
            (NpgsqlConnection? npgsqlConnection, YearBuiltDataPostgreSQLConverter? yearBuiltDataPostgreSQLConverter) = await ScratchConnectionAsync();
            Assert.NotNull(yearBuiltDataPostgreSQLConverter);
            Assert.NotNull(npgsqlConnection);

            int countyId = 0;
            try
            {
                countyId = await SeedScratchCountyAsync(npgsqlConnection);

                await SeedBuilding2DAsync(npgsqlConnection, countyId, "XUNIT-RND-PART");
                await SeedOrtoDatasAsync(npgsqlConnection, countyId, "XUNIT-RND-PART", "2010");
                await AnalyzeOrtoDatasAsync(npgsqlConnection, countyId);

                OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(yearBuiltDataPostgreSQLConverter.ConnectionData);

                for (int i = 0; i < 20; i++)
                {
                    Building2DReference? drawn = await ortoDatasPostgreSQLConverter.GetRandomBuilding2DReferenceWithoutUserYearBuiltAsync([countyId]);
                    Assert.NotNull(drawn);
                    Assert.Equal(countyId, drawn.CountyId);
                    Assert.Equal("XUNIT-RND-PART", drawn.Reference);
                }

                // A part id that names nothing: the pool is empty, so the answer is null rather than a building from elsewhere.
                Building2DReference? drawn_Unknown = await ortoDatasPostgreSQLConverter.GetRandomBuilding2DReferenceWithoutUserYearBuiltAsync([countyId + 1_000_000]);
                Assert.Null(drawn_Unknown);

                // An empty filter is the baseline: a draw is possible because the scratch part is covered.
                Building2DReference? drawn_Empty = await ortoDatasPostgreSQLConverter.GetRandomBuilding2DReferenceWithoutUserYearBuiltAsync([]);
                Assert.NotNull(drawn_Empty);
            }
            finally
            {
                await CleanupScratchCountyAsync(npgsqlConnection, countyId);
                npgsqlConnection?.Dispose();
            }
        }

        /// <summary>
        /// Verifies, measured on the development database, that the drawn building is orthophoto-covered with at least one card and carries no user year built entry.
        /// <para>The scratch county 990101 is seeded with one eligible building so a draw is always possible; the assertions are on the property of whatever is drawn, so the fact holds whether the draw lands on the scratch county or on any other covered county of the database.</para>
        /// <para>Skipped by default: it seeds scratch county 990101 and needs <c>GIS_PostgreSQL_Main.conf</c> beside the test assembly pointing at a scratch database - never the deployed one.</para>
        /// </summary>
        [Fact(Skip = "Seeds scratch county 990101. Point GIS_PostgreSQL_Main.conf at a scratch database before running.")]
        public async Task RandomBuilding2DReference_WithoutUserYearBuilt_DevDb()
        {
            (NpgsqlConnection? npgsqlConnection, YearBuiltDataPostgreSQLConverter? yearBuiltDataPostgreSQLConverter) = await ScratchConnectionAsync();
            Assert.NotNull(yearBuiltDataPostgreSQLConverter);
            Assert.NotNull(npgsqlConnection);

            int countyId = 0;
            try
            {
                countyId = await SeedScratchCountyAsync(npgsqlConnection);

                // One eligible building: covered, one card, no year_built_data row at all.
                await SeedBuilding2DAsync(npgsqlConnection, countyId, "XUNIT-RND-ELIGIBLE");
                await SeedOrtoDatasAsync(npgsqlConnection, countyId, "XUNIT-RND-ELIGIBLE", "2010");
                await AnalyzeOrtoDatasAsync(npgsqlConnection, countyId);

                OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(yearBuiltDataPostgreSQLConverter.ConnectionData);

                Building2DReference? drawn = await ortoDatasPostgreSQLConverter.GetRandomBuilding2DReferenceWithoutUserYearBuiltAsync();
                Assert.NotNull(drawn);

                // Orthophoto-covered with at least one card: the exact-years read answers non-empty.
                List<short>? years = await OrtoDatasPostgreSQLConverter.GetYearsByReferenceAsync(npgsqlConnection, drawn.Reference!, drawn.CountyId);
                Assert.NotNull(years);
                Assert.NotEmpty(years);

                // No user year built entry anywhere in the building's stored rows.
                Assert.True(await HasNoUserYearBuiltAsync(npgsqlConnection, drawn.CountyId!.Value, drawn.Reference!), "the drawn building carries a user year built entry");
            }
            finally
            {
                await CleanupScratchCountyAsync(npgsqlConnection, countyId);
                npgsqlConnection?.Dispose();
            }
        }

        /// <summary>
        /// Verifies, measured on the development database, that a building holding a user year built entry is never drawn, and that every drawn building carries no user entry.
        /// <para>The exclusion is asserted over 25 draws: the seeded pair is in scratch county 990101, so it can only enter the pool when that county is drawn - and then the anti-join removes it. The property that every drawn building is user-entry-free is the invariant the fact pins, on whichever county the draw lands.</para>
        /// <para>Skipped by default: it seeds scratch county 990101 and needs <c>GIS_PostgreSQL_Main.conf</c> beside the test assembly pointing at a scratch database - never the deployed one.</para>
        /// </summary>
        [Fact(Skip = "Seeds scratch county 990101. Point GIS_PostgreSQL_Main.conf at a scratch database before running.")]
        public async Task RandomBuilding2DReference_ExcludesUserVerified_DevDb()
        {
            (NpgsqlConnection? npgsqlConnection, YearBuiltDataPostgreSQLConverter? yearBuiltDataPostgreSQLConverter) = await ScratchConnectionAsync();
            Assert.NotNull(yearBuiltDataPostgreSQLConverter);
            Assert.NotNull(npgsqlConnection);

            int countyId = 0;
            try
            {
                countyId = await SeedScratchCountyAsync(npgsqlConnection);

                // A verified building (user entry) and an eligible one in the same scratch county.
                await SeedBuilding2DAsync(npgsqlConnection, countyId, "XUNIT-RND-VERIFIED");
                await SeedOrtoDatasAsync(npgsqlConnection, countyId, "XUNIT-RND-VERIFIED", "2010");
                await SeedYearBuiltDataAsync(npgsqlConnection, countyId, "XUNIT-RND-VERIFIED", "xunit-rnd-1", 1975, null);

                await SeedBuilding2DAsync(npgsqlConnection, countyId, "XUNIT-RND-ELIGIBLE");
                await SeedOrtoDatasAsync(npgsqlConnection, countyId, "XUNIT-RND-ELIGIBLE", "2010");
                await AnalyzeOrtoDatasAsync(npgsqlConnection, countyId);

                OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(yearBuiltDataPostgreSQLConverter.ConnectionData);

                for (int i = 0; i < 25; i++)
                {
                    Building2DReference? drawn = await ortoDatasPostgreSQLConverter.GetRandomBuilding2DReferenceWithoutUserYearBuiltAsync();
                    if (drawn is null)
                    {
                        continue;
                    }

                    Assert.NotEqual("XUNIT-RND-VERIFIED", drawn.Reference);
                    Assert.True(await HasNoUserYearBuiltAsync(npgsqlConnection, drawn.CountyId!.Value, drawn.Reference!), "a drawn building carries a user year built entry");
                }
            }
            finally
            {
                await CleanupScratchCountyAsync(npgsqlConnection, countyId);
                npgsqlConnection?.Dispose();
            }
        }

        /// <summary>
        /// Verifies, measured on the development database, that a building holding only a predicted year built entry is eligible and is drawn.
        /// <para>Predictions never affect eligibility: the anti-join is an equality on the user entry's <c>_type</c>, so a prediction-only building passes it. The scratch county is the only candidate, so the draw lands on its single eligible building.</para>
        /// <para>Skipped by default: it seeds scratch county 990101 and needs <c>GIS_PostgreSQL_Main.conf</c> beside the test assembly pointing at a scratch database where 990101 is the only county with orthophoto coverage - the fact asserts that precondition, so a database holding other covered counties fails it deliberately.</para>
        /// </summary>
        [Fact(Skip = "Seeds scratch county 990101. Point GIS_PostgreSQL_Main.conf at a scratch database where 990101 is the only ortho-covered county before running.")]
        public async Task RandomBuilding2DReference_PredictedOnlyIsEligible_DevDb()
        {
            (NpgsqlConnection? npgsqlConnection, YearBuiltDataPostgreSQLConverter? yearBuiltDataPostgreSQLConverter) = await ScratchConnectionAsync();
            Assert.NotNull(yearBuiltDataPostgreSQLConverter);
            Assert.NotNull(npgsqlConnection);

            int countyId = 0;
            try
            {
                countyId = await SeedScratchCountyAsync(npgsqlConnection);

                await SeedBuilding2DAsync(npgsqlConnection, countyId, "XUNIT-RND-PRED");
                await SeedOrtoDatasAsync(npgsqlConnection, countyId, "XUNIT-RND-PRED", "2010");
                await SeedYearBuiltDataAsync(npgsqlConnection, countyId, "XUNIT-RND-PRED", "xunit-rnd-2", null, 2008);
                await AnalyzeOrtoDatasAsync(npgsqlConnection, countyId);
                await AssertScratchCountyIsOnlyCandidateAsync(npgsqlConnection, countyId);

                OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(yearBuiltDataPostgreSQLConverter.ConnectionData);

                Building2DReference? drawn = await ortoDatasPostgreSQLConverter.GetRandomBuilding2DReferenceWithoutUserYearBuiltAsync();
                Assert.NotNull(drawn);
                Assert.Equal(countyId, drawn.CountyId);
                Assert.Equal("XUNIT-RND-PRED", drawn.Reference);
            }
            finally
            {
                await CleanupScratchCountyAsync(npgsqlConnection, countyId);
                npgsqlConnection?.Dispose();
            }
        }

        /// <summary>
        /// Verifies, measured on the development database, that an <c>orto_datas</c> row with an empty <c>Values</c> array never produces a candidate, while the covered pair in the same county does.
        /// <para>Without the guard the empty-Values building would enter the pool and the page would show it with zero cards. The scratch county is the only candidate, so the 25 draws alternate over its two buildings only: the empty one never appears, the covered one does.</para>
        /// <para>Skipped by default: it seeds scratch county 990101 and needs <c>GIS_PostgreSQL_Main.conf</c> beside the test assembly pointing at a scratch database where 990101 is the only county with orthophoto coverage - the fact asserts that precondition, so a database holding other covered counties fails it deliberately.</para>
        /// </summary>
        [Fact(Skip = "Seeds scratch county 990101. Point GIS_PostgreSQL_Main.conf at a scratch database where 990101 is the only ortho-covered county before running.")]
        public async Task RandomBuilding2DReference_ExcludesEmptyValues_DevDb()
        {
            (NpgsqlConnection? npgsqlConnection, YearBuiltDataPostgreSQLConverter? yearBuiltDataPostgreSQLConverter) = await ScratchConnectionAsync();
            Assert.NotNull(yearBuiltDataPostgreSQLConverter);
            Assert.NotNull(npgsqlConnection);

            int countyId = 0;
            try
            {
                countyId = await SeedScratchCountyAsync(npgsqlConnection);

                // Covered with a card, and covered with an empty Values array.
                await SeedBuilding2DAsync(npgsqlConnection, countyId, "XUNIT-RND-PHOTO");
                await SeedOrtoDatasAsync(npgsqlConnection, countyId, "XUNIT-RND-PHOTO", "2010");

                await SeedBuilding2DAsync(npgsqlConnection, countyId, "XUNIT-RND-EMPTY");
                await SeedOrtoDatasAsync(npgsqlConnection, countyId, "XUNIT-RND-EMPTY", null);
                await AnalyzeOrtoDatasAsync(npgsqlConnection, countyId);
                await AssertScratchCountyIsOnlyCandidateAsync(npgsqlConnection, countyId);

                OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(yearBuiltDataPostgreSQLConverter.ConnectionData);

                bool photoSeen = false;
                for (int i = 0; i < 25; i++)
                {
                    Building2DReference? drawn = await ortoDatasPostgreSQLConverter.GetRandomBuilding2DReferenceWithoutUserYearBuiltAsync();
                    if (drawn is null)
                    {
                        continue;
                    }

                    Assert.NotEqual("XUNIT-RND-EMPTY", drawn.Reference);
                    if (drawn.Reference == "XUNIT-RND-PHOTO")
                    {
                        photoSeen = true;
                    }
                }

                Assert.True(photoSeen, "the covered pair was never drawn");
            }
            finally
            {
                await CleanupScratchCountyAsync(npgsqlConnection, countyId);
                npgsqlConnection?.Dispose();
            }
        }

        /// <summary>
        /// Opens a connection through the converter manager's connection data, the same way the integration facts in this file family do.
        /// </summary>
        /// <returns>The open connection and the converter whose <c>ConnectionData</c> seeded it, or nulls when the configuration is missing.</returns>
        private static async Task<(NpgsqlConnection? Connection, YearBuiltDataPostgreSQLConverter? Converter)> ScratchConnectionAsync()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            YearBuiltDataPostgreSQLConverter? converter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<YearBuiltDataPostgreSQLConverter>();
            Assert.NotNull(converter);
            Assert.NotNull(converter.ConnectionData);

            NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(converter.ConnectionData);
            if (npgsqlConnection is not null)
            {
                await npgsqlConnection.OpenAsync();
            }

            return (npgsqlConnection, converter);
        }

        /// <summary>
        /// Seeds the scratch county part in <c>administrative_areal_2d</c> and the three building-keyed partitions under it, returning the county's part id.
        /// </summary>
        /// <param name="npgsqlConnection">The open connection.</param>
        /// <returns>The identifier of the seeded county part.</returns>
        private static async Task<int> SeedScratchCountyAsync(NpgsqlConnection npgsqlConnection)
        {
            await ExecuteAsync(npgsqlConnection, "DELETE FROM administrative_areal_2d WHERE reference = 'XUNIT-COUNTY-990101';");
            await ExecuteAsync(npgsqlConnection, "INSERT INTO administrative_areal_2d (reference, code, name, type_id) VALUES ('XUNIT-COUNTY-990101', '990101', 'xunit scratch county', 2);");

            await using NpgsqlCommand npgsqlCommand = new("SELECT id FROM administrative_areal_2d WHERE reference = 'XUNIT-COUNTY-990101';", npgsqlConnection);
            object? id = await npgsqlCommand.ExecuteScalarAsync();
            Assert.NotNull(id);
            int countyId = System.Convert.ToInt32(id);

            await ExecuteAsync(npgsqlConnection, $"DROP TABLE IF EXISTS building_2d_{countyId};");
            await ExecuteAsync(npgsqlConnection, $"DROP TABLE IF EXISTS orto_datas_{countyId};");
            await ExecuteAsync(npgsqlConnection, $"DROP TABLE IF EXISTS year_built_data_{countyId};");

            Assert.True(await npgsqlConnection.TableAsync_Building2D());
            Assert.True(await npgsqlConnection.TableAsync_OrtoDatas());
            Assert.True(await npgsqlConnection.TableAsync_Building2DReferencedObject("year_built_data"));
            Assert.True(await npgsqlConnection.TableAsync_Building2D_Partition(countyId));
            Assert.True(await npgsqlConnection.TableAsync_OrtoDatas_Partition(countyId));
            Assert.True(await npgsqlConnection.TableAsync_Building2DReferencedObject_Partition("year_built_data", countyId));

            return countyId;
        }

        /// <summary>
        /// Seeds one <c>building_2d</c> row for the named reference under the named county part.
        /// </summary>
        /// <param name="npgsqlConnection">The open connection.</param>
        /// <param name="countyId">The county part to file the building under.</param>
        /// <param name="reference">The building reference to seed.</param>
        private static async Task SeedBuilding2DAsync(NpgsqlConnection npgsqlConnection, int countyId, string reference)
        {
            await ExecuteAsync(npgsqlConnection, $"DELETE FROM building_2d WHERE county_id = {countyId} AND reference = '{reference}';");
            await ExecuteAsync(npgsqlConnection, $"INSERT INTO building_2d (county_id, reference) VALUES ({countyId}, '{reference}');");
        }

        /// <summary>
        /// Seeds one <c>orto_datas</c> row for the named reference: a single <c>Values</c> card in the named photo year, or an empty <c>Values</c> array when the year is null.
        /// </summary>
        /// <param name="npgsqlConnection">The open connection.</param>
        /// <param name="countyId">The county part to file the row under.</param>
        /// <param name="reference">The building reference to seed.</param>
        /// <param name="year">The photo year to store, or null for an empty <c>Values</c> array.</param>
        private static async Task SeedOrtoDatasAsync(NpgsqlConnection npgsqlConnection, int countyId, string reference, string? year)
        {
            // Bytes is a JSON array of numbers - the shape the DiGi serializer writes for a byte[] - never base64 (DiGi.GIS.PostgreSQL#90).
            string values = year is null ? "[]" : $"[{{\"Bytes\":[1,2,3],\"DateTime\":\"{year}-01-01T00:00:00\",\"Scale\":2.5}}]";
            string objectJson = $"{{\"_type\":\"DiGi.GIS.Classes.OrtoDatas,DiGi.GIS\",\"Guid\":\"11111111-0000-0000-0000-000000000001\",\"Reference\":\"{reference}\",\"Values\":[{values}]}}";

            await ExecuteAsync(npgsqlConnection, $"DELETE FROM orto_datas WHERE county_id = {countyId} AND reference = '{reference}';");
            await ExecuteAsync(npgsqlConnection, $"INSERT INTO orto_datas (county_id, reference, object) VALUES ({countyId}, '{reference}', '{objectJson}'::jsonb);");
        }

        /// <summary>
        /// Seeds one <c>year_built_data</c> row holding a user entry and/or a predicted entry for the named reference.
        /// </summary>
        /// <param name="npgsqlConnection">The open connection.</param>
        /// <param name="countyId">The county part to file the row under.</param>
        /// <param name="reference">The building reference to seed.</param>
        /// <param name="uniqueId">The stored object identifier of the row.</param>
        /// <param name="userYear">The user entry year to store, or null for none.</param>
        /// <param name="predictedYear">The predicted entry year to store, or null for none.</param>
        private static async Task SeedYearBuiltDataAsync(NpgsqlConnection npgsqlConnection, int countyId, string reference, string uniqueId, int? userYear, int? predictedYear)
        {
            List<string> entries = [];
            if (userYear.HasValue)
            {
                entries.Add($"{{\"_type\":\"DiGi.GIS.Classes.UserYearBuilt,DiGi.GIS\",\"Year\":{userYear.Value}}}");
            }

            if (predictedYear.HasValue)
            {
                entries.Add($"{{\"_type\":\"DiGi.GIS.Classes.PredictedYearBuilt,DiGi.GIS\",\"Year\":{predictedYear.Value},\"DateTime\":\"2025-05-29T09:41:47.8773778+02:00\"}}");
            }

            string objectJson = $"{{\"_type\":\"DiGi.GIS.Classes.YearBuiltData,DiGi.GIS\",\"Guid\":\"22222222-0000-0000-0000-000000000001\",\"YearBuilts\":[{string.Join(",", entries)}],\"Reference\":\"{reference}\"}}";

            await ExecuteAsync(npgsqlConnection, $"DELETE FROM year_built_data WHERE county_id = {countyId} AND reference = '{reference}';");
            await ExecuteAsync(npgsqlConnection, $"INSERT INTO year_built_data (county_id, unique_id, reference, object) VALUES ({countyId}, '{uniqueId}', '{reference}', '{objectJson}'::jsonb);");
        }

        /// <summary>
        /// Analyses the scratch <c>orto_datas</c> partition so its <c>reltuples</c> estimate is the real row count and the random pool sees it.
        /// </summary>
        /// <param name="npgsqlConnection">The open connection.</param>
        /// <param name="countyId">The county part whose partition is analysed.</param>
        private static async Task AnalyzeOrtoDatasAsync(NpgsqlConnection npgsqlConnection, int countyId)
        {
            await ExecuteAsync(npgsqlConnection, $"ANALYZE orto_datas_{countyId};");
        }

        /// <summary>
        /// Asserts the precondition of the determinism-sensitive facts: the scratch county is the only county part holding an orthophoto row with a positive estimate, so the weighted draw is forced onto it.
        /// </summary>
        /// <param name="npgsqlConnection">The open connection.</param>
        /// <param name="countyId">The scratch county part id.</param>
        private static async Task AssertScratchCountyIsOnlyCandidateAsync(NpgsqlConnection npgsqlConnection, int countyId)
        {
            List<AdministrativeAreal2DReference>? countyReferences = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(npgsqlConnection, AdministrativeArealType.County);
            Assert.NotNull(countyReferences);

            List<int> countyIds_All = [];
            foreach (AdministrativeAreal2DReference? countyReference in countyReferences)
            {
                if (countyReference is not null)
                {
                    countyIds_All.Add(countyReference.Id);
                }
            }

            Dictionary<int, long>? estimates = await OrtoDatasPostgreSQLConverter.GetEstimatedCountsAsync(npgsqlConnection, countyIds_All);
            Assert.NotNull(estimates);

            foreach (KeyValuePair<int, long> estimate in estimates)
            {
                if (estimate.Key == countyId)
                {
                    Assert.True(estimate.Value > 0, "the scratch county's estimate is not positive");
                }
                else
                {
                    Assert.True(estimate.Value <= 0, $"county {estimate.Key} holds {estimate.Value} estimated ortho rows; the fact needs {countyId} to be the only covered county");
                }
            }
        }

        /// <summary>
        /// Asserts that no <c>year_built_data</c> row of the named building carries a user year built entry.
        /// </summary>
        /// <param name="npgsqlConnection">The open connection.</param>
        /// <param name="countyId">The county part to check under.</param>
        /// <param name="reference">The building reference to check.</param>
        /// <returns>True when the building holds no user entry.</returns>
        private static async Task<bool> HasNoUserYearBuiltAsync(NpgsqlConnection npgsqlConnection, int countyId, string reference)
        {
            string commandText = $@"
                SELECT COUNT(*)
                FROM year_built_data y, jsonb_array_elements(y.object->'YearBuilts') AS entry(value)
                WHERE y.county_id = {countyId}
                  AND y.reference = '{reference}'
                  AND entry->>'_type' = 'DiGi.GIS.Classes.UserYearBuilt,DiGi.GIS';";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            object? count = await npgsqlCommand.ExecuteScalarAsync();
            return System.Convert.ToInt64(count) == 0;
        }

        /// <summary>
        /// Drops the scratch partitions and the scratch county row the facts seeded.
        /// </summary>
        /// <param name="npgsqlConnection">The open connection.</param>
        /// <param name="countyId">The scratch county part id, or 0 when seeding did not reach that far.</param>
        private static async Task CleanupScratchCountyAsync(NpgsqlConnection npgsqlConnection, int countyId)
        {
            if (countyId > 0)
            {
                await ExecuteAsync(npgsqlConnection, $"DROP TABLE IF EXISTS year_built_data_{countyId};");
                await ExecuteAsync(npgsqlConnection, $"DROP TABLE IF EXISTS orto_datas_{countyId};");
                await ExecuteAsync(npgsqlConnection, $"DROP TABLE IF EXISTS building_2d_{countyId};");
            }

            await ExecuteAsync(npgsqlConnection, "DELETE FROM administrative_areal_2d WHERE reference = 'XUNIT-COUNTY-990101';");
        }
    }
}
