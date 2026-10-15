using DiGi.GIS.PostgreSQL.Classes;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Reports the stored size of building solar radiation rows on the throwaway storage test database: 50 scratch buildings of 20 surfaces each (19 walls and a roof), every surface carrying 8 760 bytes of synthetic shading factors.
        /// <para>The report (<c>BuildingSolarRadiation_Size_*.txt</c> in <c>user files/reports/</c>) gives per row <c>pg_column_size(shading_factors)</c>, <c>pg_column_size(object)</c> and the raw factor bytes, and the partition's <c>pg_total_relation_size</c>. It describes the <b>test database and synthetic factors</b>, not production and not solved factors: the factors follow the run structure of solved ones (night zeros, shaded and open runs, ramps), which is what lz4 compresses, so the figures bound the size of real rows. The fact asserts that every row is lz4-compressed and smaller than its raw factors.</para>
        /// <para>Medium test (a live database server): it writes the scratch partition of 990303 and drops it before and after. Reference: DiGi.GIS.PostgreSQL#100.</para>
        /// </summary>
        [MediumSkippableFact]
        public async Task BuildingSolarRadiation_Size()
        {
            const int buildingCount = 50;
            double[] azimuths = new double[19];
            for (int i = 0; i < azimuths.Length; i++)
            {
                azimuths[i] = (i * 360.0 / azimuths.Length) + 7;
            }

            (_, ConnectionData connectionData_Storage) = TestConnectionDatas();

            BuildingSolarRadiationPostgreSQLConverter buildingSolarRadiationPostgreSQLConverter = new(connectionData_Storage);

            await CleanupBuildingSolarRadiationScratchAsync(connectionData_Storage);
            try
            {
                for (int i = 0; i < buildingCount; i++)
                {
                    GIS.Classes.BuildingSolarRadiationResult buildingSolarRadiationResult = ScratchBuildingSolarRadiationResult($"XUNIT-SR-SIZE-{i}", 990303000L + i, "17", "size@example.com", 1.0 + (i * 0.01), azimuths);
                    Assert.NotNull(await buildingSolarRadiationPostgreSQLConverter.UpdateAsync(countyId_SolarRadiation_Size, buildingSolarRadiationResult));
                }

                await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(connectionData_Storage);
                Assert.NotNull(npgsqlConnection);
                await npgsqlConnection.OpenAsync();

                string partitionName = $"{Constants.TableName.BuildingSolarRadiation}_{countyId_SolarRadiation_Size}";

                List<(long ShadingFactors, long Object, long Raw, string? Compression)> sizes = [];
                await using (NpgsqlCommand npgsqlCommand = new($"SELECT pg_column_size(shading_factors), pg_column_size(object), octet_length(shading_factors), pg_column_compression(shading_factors) FROM {partitionName} ORDER BY building_model_id;", npgsqlConnection))
                {
                    await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync();
                    while (await npgsqlDataReader.ReadAsync())
                    {
                        sizes.Add((npgsqlDataReader.GetInt32(0), npgsqlDataReader.GetInt32(1), npgsqlDataReader.GetInt32(2), npgsqlDataReader.IsDBNull(3) ? null : npgsqlDataReader.GetString(3)));
                    }
                }

                long totalRelationSize = (long)(await ScalarAsync(npgsqlConnection, $"SELECT pg_total_relation_size('{partitionName}');"))!;
                string? serverVersion = (await ScalarAsync(npgsqlConnection, "SHOW server_version;"))?.ToString();

                Assert.Equal(buildingCount, sizes.Count);
                Assert.All(sizes, x => Assert.Equal("lz4", x.Compression));
                Assert.All(sizes, x => Assert.True(x.ShadingFactors < x.Raw, $"Stored factors of {x.ShadingFactors} B are not smaller than the raw {x.Raw} B."));

                long shadingFactors_Sum = 0;
                long object_Sum = 0;
                long shadingFactors_Max = 0;
                foreach ((long ShadingFactors, long Object, long Raw, string? Compression) size in sizes)
                {
                    shadingFactors_Sum += size.ShadingFactors;
                    object_Sum += size.Object;
                    shadingFactors_Max = Math.Max(shadingFactors_Max, size.ShadingFactors);
                }

                StringBuilder stringBuilder = new();
                stringBuilder.AppendLine("BuildingSolarRadiation_Size - DiGi.GIS.PostgreSQL#100");
                stringBuilder.AppendLine("Database: the throwaway STORAGE TEST database (GIS_PostgreSQL_Storage_Test.conf), PostgreSQL " + serverVersion + ". Not production.");
                stringBuilder.AppendLine("Factors: SYNTHETIC (ScratchShadingFactors: 52 N daylength, back-facing hours shaded, low-sun ramps), not solved.");
                stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Rows: {0}, surfaces per row: {1}, hours per surface: {2}, raw factor bytes per row: {3}", sizes.Count, azimuths.Length + 1, hourCount_SolarReferenceYear, sizes[0].Raw));
                stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "pg_column_size(shading_factors): mean {0:F0} B, max {1} B ({2:F1} % of raw)", (double)shadingFactors_Sum / sizes.Count, shadingFactors_Max, 100.0 * shadingFactors_Sum / sizes.Count / sizes[0].Raw));
                stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "pg_column_size(object): mean {0:F0} B", (double)object_Sum / sizes.Count));
                stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "pg_total_relation_size({0}): {1} B, {2:F0} B per row (indexes and TOAST included)", partitionName, totalRelationSize, (double)totalRelationSize / sizes.Count));

                string? directory = Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly());
                Assert.NotNull(directory);
                string path = Path.Combine(directory, $"BuildingSolarRadiation_Size_{DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture)}.txt");
                await File.WriteAllTextAsync(path, stringBuilder.ToString());
            }
            finally
            {
                await CleanupBuildingSolarRadiationScratchAsync(connectionData_Storage);
            }
        }
    }
}
