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
        /// Verifies <see cref="BuildingSolarRadiationPostgreSQLConverter"/> on the throwaway storage test database:
        /// <para>The table creation is idempotent and the partition stores <c>shading_factors</c> with lz4. A read before any write answers null. A written result reads back with the same summary and, with <c>includeShadingFactors</c>, byte-identical factors; without it, every surface's factors are null. A second write for the same key replaces the row - one row remains, under the same id, with the new summary. A read under a sibling county part finds the row only with <c>fallbackByReference</c>. Another radius or solver version is a miss, and factors of unequal length are refused.</para>
        /// <para>Medium test (a live database server): it writes the scratch partitions of 990301 and 990302 and drops them before and after. Reference: DiGi.GIS.PostgreSQL#100.</para>
        /// </summary>
        [MediumSkippableFact]
        public async Task BuildingSolarRadiationPostgreSQLConverter_RoundTrip()
        {
            (_, ConnectionData connectionData_Storage) = TestConnectionDatas();

            BuildingSolarRadiationPostgreSQLConverter buildingSolarRadiationPostgreSQLConverter = new(connectionData_Storage);

            await CleanupBuildingSolarRadiationScratchAsync(connectionData_Storage);
            try
            {
                await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(connectionData_Storage);
                Assert.NotNull(npgsqlConnection);
                await npgsqlConnection.OpenAsync();

                Assert.True(await npgsqlConnection.TableAsync_BuildingSolarRadiation());
                Assert.True(await npgsqlConnection.TableAsync_BuildingSolarRadiation());

                Assert.Null(await buildingSolarRadiationPostgreSQLConverter.GetAsync(countyId_SolarRadiation_A, 990301001L, 50.0, 1, true));

                GIS.Classes.BuildingSolarRadiationResult buildingSolarRadiationResult_1 = ScratchBuildingSolarRadiationResult("XUNIT-SR-1", 990301001L, "17", "first@example.com", 1.0, 180, 270);
                long? id_1 = await buildingSolarRadiationPostgreSQLConverter.UpdateAsync(countyId_SolarRadiation_A, buildingSolarRadiationResult_1);
                Assert.NotNull(id_1);

                Assert.Equal("l", (await ScalarAsync(npgsqlConnection, $"SELECT attcompression::text FROM pg_attribute WHERE attrelid = '{Constants.TableName.BuildingSolarRadiation}_{countyId_SolarRadiation_A}'::regclass AND attname = 'shading_factors';"))?.ToString());
                Assert.Equal("lz4", (await ScalarAsync(npgsqlConnection, $"SELECT pg_column_compression(shading_factors) FROM {Constants.TableName.BuildingSolarRadiation} WHERE id = {id_1.Value};"))?.ToString());

                GIS.Classes.BuildingSolarRadiationResult? buildingSolarRadiationResult_Read = await buildingSolarRadiationPostgreSQLConverter.GetAsync(countyId_SolarRadiation_A, 990301001L, 50.0, 1, true);
                AssertBuildingSolarRadiationResult(buildingSolarRadiationResult_1, buildingSolarRadiationResult_Read, true);
                Assert.Equal(countyId_SolarRadiation_A, buildingSolarRadiationResult_Read!.CountyId);

                GIS.Classes.BuildingSolarRadiationResult? buildingSolarRadiationResult_Summary = await buildingSolarRadiationPostgreSQLConverter.GetAsync(countyId_SolarRadiation_A, 990301001L, 50.0, 1);
                AssertBuildingSolarRadiationResult(buildingSolarRadiationResult_1, buildingSolarRadiationResult_Summary, false);
                Assert.All(buildingSolarRadiationResult_Summary!.SurfaceSolarRadiations!, x => Assert.Null(x.ShadingFactors));

                // A write for the same key replaces the row.
                GIS.Classes.BuildingSolarRadiationResult buildingSolarRadiationResult_2 = ScratchBuildingSolarRadiationResult("XUNIT-SR-1", 990301001L, "23", "second@example.com", 1.5, 90, 180, 270);
                long? id_2 = await buildingSolarRadiationPostgreSQLConverter.UpdateAsync(countyId_SolarRadiation_A, buildingSolarRadiationResult_2);
                Assert.Equal(id_1, id_2);
                Assert.Equal(1L, await ScalarAsync(npgsqlConnection, $"SELECT count(*) FROM {Constants.TableName.BuildingSolarRadiation} WHERE building_model_id = 990301001;"));
                AssertBuildingSolarRadiationResult(buildingSolarRadiationResult_2, await buildingSolarRadiationPostgreSQLConverter.GetAsync(countyId_SolarRadiation_A, 990301001L, 50.0, 1, true), true);

                // A sibling county part finds the row only through the fallback.
                Assert.Null(await buildingSolarRadiationPostgreSQLConverter.GetAsync(countyId_SolarRadiation_B, 990301001L, 50.0, 1, false, false));
                GIS.Classes.BuildingSolarRadiationResult? buildingSolarRadiationResult_Sibling = await buildingSolarRadiationPostgreSQLConverter.GetAsync(countyId_SolarRadiation_B, 990301001L, 50.0, 1, true);
                AssertBuildingSolarRadiationResult(buildingSolarRadiationResult_2, buildingSolarRadiationResult_Sibling, true);
                Assert.Equal(countyId_SolarRadiation_A, buildingSolarRadiationResult_Sibling!.CountyId);

                // The key is exact: another radius or solver version is a miss.
                Assert.Null(await buildingSolarRadiationPostgreSQLConverter.GetAsync(countyId_SolarRadiation_A, 990301001L, 50.1, 1));
                Assert.Null(await buildingSolarRadiationPostgreSQLConverter.GetAsync(countyId_SolarRadiation_A, 990301001L, 50.0, 2));

                // Factors that cannot be packed are refused, and nothing is written.
                List<GIS.Classes.SurfaceSolarRadiation> surfaceSolarRadiations_Unequal =
                [
                    new("b1a5c1de-0000-4000-8000-000000000001", 10, null, 1, 1, 0, 0, 0, 0, 0, 0, new byte[hourCount_SolarReferenceYear]),
                    new("b1a5c1de-0000-4000-8000-000000000002", 10, null, 1, 1, 0, 0, 0, 0, 0, 0, new byte[hourCount_SolarReferenceYear - 1]),
                ];
                GIS.Classes.BuildingSolarRadiationResult buildingSolarRadiationResult_Unequal = new("XUNIT-SR-2", countyId_SolarRadiation_A, 990301002L, 50.0, 1, GIS.Enums.WeatherSourceType.EPW, "17", null, null, null, null, surfaceSolarRadiations_Unequal);
                Assert.Null(await buildingSolarRadiationPostgreSQLConverter.UpdateAsync(countyId_SolarRadiation_A, buildingSolarRadiationResult_Unequal));
                Assert.Null(await buildingSolarRadiationPostgreSQLConverter.GetAsync(countyId_SolarRadiation_A, 990301002L, 50.0, 1));
            }
            finally
            {
                await CleanupBuildingSolarRadiationScratchAsync(connectionData_Storage);
            }
        }

        /// <summary>
        /// Asserts that a stored building solar radiation result read back matches the one written, member by member and surface by surface.
        /// </summary>
        /// <param name="expected">The result written.</param>
        /// <param name="actual">The result read back.</param>
        /// <param name="includeShadingFactors">True to compare the shading factors; false to require them to be null.</param>
        private static void AssertBuildingSolarRadiationResult(GIS.Classes.BuildingSolarRadiationResult expected, GIS.Classes.BuildingSolarRadiationResult? actual, bool includeShadingFactors)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Reference, actual.Reference);
            Assert.Equal(expected.BuildingModelId, actual.BuildingModelId);
            Assert.Equal(expected.Radius, actual.Radius);
            Assert.Equal(expected.SolverVersion, actual.SolverVersion);
            Assert.Equal(expected.WeatherSourceType, actual.WeatherSourceType);
            Assert.Equal(expected.WeatherSourceId, actual.WeatherSourceId);
            Assert.Equal(expected.WeatherPeriodStart, actual.WeatherPeriodStart);
            Assert.Equal(expected.WeatherPeriodEnd, actual.WeatherPeriodEnd);
            Assert.Equal(expected.UserEmail, actual.UserEmail);
            Assert.Equal(expected.CreatedAt, actual.CreatedAt);

            List<GIS.Classes.SurfaceSolarRadiation>? surfaceSolarRadiations_Expected = expected.SurfaceSolarRadiations;
            List<GIS.Classes.SurfaceSolarRadiation>? surfaceSolarRadiations_Actual = actual.SurfaceSolarRadiations;
            Assert.NotNull(surfaceSolarRadiations_Expected);
            Assert.NotNull(surfaceSolarRadiations_Actual);
            Assert.Equal(surfaceSolarRadiations_Expected.Count, surfaceSolarRadiations_Actual.Count);
            for (int i = 0; i < surfaceSolarRadiations_Expected.Count; i++)
            {
                GIS.Classes.SurfaceSolarRadiation surfaceSolarRadiation_Expected = surfaceSolarRadiations_Expected[i];
                GIS.Classes.SurfaceSolarRadiation surfaceSolarRadiation_Actual = surfaceSolarRadiations_Actual[i];
                Assert.Equal(surfaceSolarRadiation_Expected.Reference, surfaceSolarRadiation_Actual.Reference);
                Assert.Equal(surfaceSolarRadiation_Expected.Area, surfaceSolarRadiation_Actual.Area);
                Assert.Equal(surfaceSolarRadiation_Expected.Irradiation, surfaceSolarRadiation_Actual.Irradiation);
                Assert.Equal(surfaceSolarRadiation_Expected.Beam, surfaceSolarRadiation_Actual.Beam);
                Assert.Equal(surfaceSolarRadiation_Expected.Diffuse, surfaceSolarRadiation_Actual.Diffuse);
                Assert.Equal(surfaceSolarRadiation_Expected.Ground, surfaceSolarRadiation_Actual.Ground);
                Assert.Equal(surfaceSolarRadiation_Expected.IrradiationUnshaded, surfaceSolarRadiation_Actual.IrradiationUnshaded);
                Assert.Equal(surfaceSolarRadiation_Expected.Energy, surfaceSolarRadiation_Actual.Energy);
                Assert.Equal(surfaceSolarRadiation_Expected.SkyVisibility, surfaceSolarRadiation_Actual.SkyVisibility);
                Assert.Equal(surfaceSolarRadiation_Expected.GroundVisibility, surfaceSolarRadiation_Actual.GroundVisibility);
                Assert.Equal(surfaceSolarRadiation_Expected.Normal?.X, surfaceSolarRadiation_Actual.Normal?.X);
                Assert.Equal(surfaceSolarRadiation_Expected.Normal?.Y, surfaceSolarRadiation_Actual.Normal?.Y);
                Assert.Equal(surfaceSolarRadiation_Expected.Normal?.Z, surfaceSolarRadiation_Actual.Normal?.Z);
                Assert.Equal(includeShadingFactors ? surfaceSolarRadiation_Expected.ShadingFactors : null, surfaceSolarRadiation_Actual.ShadingFactors);
            }
        }
    }
}
