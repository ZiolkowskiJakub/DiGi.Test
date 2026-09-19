using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the <see cref="Building2DPostgreSQLConverter.RefreshAsync(PostgreSQLBuilding2DRefreshOptions, System.IProgress{long}, int, System.Threading.CancellationToken)"/> walk is per county polygon part: the union of the parts' visits covers every row of the scoped parts, each building lands on the smallest subdivision containing it, and the <c>StartId</c> anchor is evaluated within each part - a part skips its rows at or below the anchor and visits the rest.
        /// <para>Two county parts of one code hold four buildings under a nested subdivision layer. A full walk re-derives all four onto their smallest container; clearing one building of its subdivision and restarting the walk at the middle of its part's identifiers then admits no row at all, while a restart from zero admits exactly that one.</para>
        /// <para>Skipped by default: it writes to a database, and it also writes the two county parts and their subdivisions into <c>administrative_areal_2d</c>. Point <c>GIS_PostgreSQL_Main.conf</c> at a scratch database before running it.</para>
        /// <para>See https://github.com/ZiolkowskiJakub/DiGi.GIS.PostgreSQL/issues/87.</para>
        /// </summary>
        [Fact(Skip = "Writes to a database. Point GIS_PostgreSQL_Main.conf at a scratch database before running.")]
        public async Task Building2DRefreshAsync_PerPartWalk()
        {
            const string code = "9002";

            const string reference_A1 = "REFRESH_PART_WALK_A1";
            const string reference_A2 = "REFRESH_PART_WALK_A2";
            const string reference_A3 = "REFRESH_PART_WALK_A3";
            const string reference_B1 = "REFRESH_PART_WALK_B1";

            const string reference_County_A = "REFRESH_PART_WALK_COUNTY_A";
            const string reference_County_B = "REFRESH_PART_WALK_COUNTY_B";
            const string reference_Subdivision_A_Outer = "REFRESH_PART_WALK_SA_OUTER";
            const string reference_Subdivision_A_Inner = "REFRESH_PART_WALK_SA_INNER";
            const string reference_Subdivision_B = "REFRESH_PART_WALK_SB";

            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            ConnectionData? connectionData = administrativeAreal2DPostgreSQLConverter.ConnectionData;
            Assert.NotNull(connectionData);

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(connectionData);
            Assert.NotNull(npgsqlConnection);

            await npgsqlConnection.OpenAsync();

            // Two parts of one code, far apart, so neither part's subdivision layer reaches the other's buildings.
            // The reference readers of the write path read <c>name</c> with GetString, so every row carries one.
            // The layer search walks Country first and stops when that level is empty, so the area carries a country too.
            const string reference_Country = "REFRESH_PART_WALK_COUNTRY";

            // The hierarchy reader reads <c>code</c> with GetString, so the country - like every other row -
            // carries one, or the moment the search reaches the country level the whole part's batch fails.
            AdministrativeAreal2D administrativeAreal2D_Country = new()
            {
                Code = "10",
                Reference = reference_Country,
                Name = "country",
                AdministrativeArealType = AdministrativeArealType.Country,
                BoundingBox2D = new BoundingBox2D(new Point2D(0, 0), new Point2D(1100, 100)),
                Object = new GIS.Classes.AdministrativeDivision(Guid.NewGuid(), reference_Country, "10", PolygonalFace2D_Square(0, 0, 1100), GIS.Enums.AdministrativeDivisionType.country, "country").ToJsonObject()
            };

            AdministrativeAreal2D administrativeAreal2D_A = AdministrativeAreal2D_SquareWithReferenceAndBoundingBox2D(reference_County_A, code, "part A", 0, 0, 100);
            AdministrativeAreal2D administrativeAreal2D_B = AdministrativeAreal2D_SquareWithReferenceAndBoundingBox2D(reference_County_B, code, "part B", 1000, 0, 100);

            // Part A nests - the outer holds the inner - so the pick between them is the smallest container,
            // and part B holds a single subdivision.
            AdministrativeAreal2D administrativeAreal2D_A_Outer = AdministrativeAreal2D_SubdivisionWithBoundingBox2D(reference_Subdivision_A_Outer, code, "A outer", 0, 0, 100);
            AdministrativeAreal2D administrativeAreal2D_A_Inner = AdministrativeAreal2D_SubdivisionWithBoundingBox2D(reference_Subdivision_A_Inner, code, "A inner", 10, 10, 80);
            AdministrativeAreal2D administrativeAreal2D_B_Sole = AdministrativeAreal2D_SubdivisionWithBoundingBox2D(reference_Subdivision_B, code, "B sole", 1000, 0, 100);

            HashSet<int>? ids = await administrativeAreal2DPostgreSQLConverter.UpdateAsync([administrativeAreal2D_Country, administrativeAreal2D_A, administrativeAreal2D_B, administrativeAreal2D_A_Outer, administrativeAreal2D_A_Inner, administrativeAreal2D_B_Sole]);
            Assert.NotNull(ids);
            Assert.Equal(6, ids.Count);

            int countyId_A = await GetAdministrativeAreal2DIdAsync(npgsqlConnection, reference_County_A);
            int countyId_B = await GetAdministrativeAreal2DIdAsync(npgsqlConnection, reference_County_B);
            int subdivisionId_A_Outer = await GetAdministrativeAreal2DIdAsync(npgsqlConnection, reference_Subdivision_A_Outer);
            int subdivisionId_A_Inner = await GetAdministrativeAreal2DIdAsync(npgsqlConnection, reference_Subdivision_A_Inner);
            int subdivisionId_B = await GetAdministrativeAreal2DIdAsync(npgsqlConnection, reference_Subdivision_B);

            // A1 and A3 sit in the inner subdivision, A2 only in the outer one, B1 in part B's sole one.
            GIS.Classes.Building2D building2D_GIS_A1 = new(Guid.NewGuid(), reference_A1, PolygonalFace2D_Square(20, 20, 10), 1, null, null, []);
            GIS.Classes.Building2D building2D_GIS_A2 = new(Guid.NewGuid(), reference_A2, PolygonalFace2D_Square(92, 92, 5), 1, null, null, []);
            GIS.Classes.Building2D building2D_GIS_A3 = new(Guid.NewGuid(), reference_A3, PolygonalFace2D_Square(30, 30, 10), 1, null, null, []);
            GIS.Classes.Building2D building2D_GIS_B1 = new(Guid.NewGuid(), reference_B1, PolygonalFace2D_Square(1020, 20, 10), 1, null, null, []);

            Building2D? building2D_A1 = building2D_GIS_A1.ToPostgreSQL(code);
            Building2D? building2D_A2 = building2D_GIS_A2.ToPostgreSQL(code);
            Building2D? building2D_A3 = building2D_GIS_A3.ToPostgreSQL(code);
            Building2D? building2D_B1 = building2D_GIS_B1.ToPostgreSQL(code);
            Assert.NotNull(building2D_A1);
            Assert.NotNull(building2D_A2);
            Assert.NotNull(building2D_A3);
            Assert.NotNull(building2D_B1);

            building2D_A1.CountyId = countyId_A;
            building2D_A2.CountyId = countyId_A;
            building2D_A3.CountyId = countyId_A;
            building2D_B1.CountyId = countyId_B;

            PostgreSQLUpdateResult? postgreSQLUpdateResult = await building2DPostgreSQLConverter.UpdateAsync([building2D_A1, building2D_A2, building2D_A3, building2D_B1]);
            Assert.NotNull(postgreSQLUpdateResult);
            Assert.Equal(4, postgreSQLUpdateResult.Ids.Count);

            long id_A1 = await GetBuilding2DIdAsync(npgsqlConnection, reference_A1);
            long id_A2 = await GetBuilding2DIdAsync(npgsqlConnection, reference_A2);
            long id_A3 = await GetBuilding2DIdAsync(npgsqlConnection, reference_A3);

            long[] ids_A = [id_A1, id_A2, id_A3];
            long id_A_Middle = ids_A.OrderBy(x => x).ElementAt(1);

            try
            {
                // A full walk visits every row of both parts and re-derives each one onto its smallest container.
                PostgreSQLBuilding2DRefreshOptions postgreSQLBuilding2DRefreshOptions_Full = new()
                {
                    CountyIds = [countyId_A, countyId_B],
                    OverrideExistingSubdivisionIds = true,
                    StartId = 0
                };

                PostgreSQLBuilding2DRefreshResult? postgreSQLBuilding2DRefreshResult_Full = await building2DPostgreSQLConverter.RefreshAsync(postgreSQLBuilding2DRefreshOptions_Full);
                Assert.NotNull(postgreSQLBuilding2DRefreshResult_Full);

                Assert.False(postgreSQLBuilding2DRefreshResult_Full.Cancelled);
                Assert.Equal(0, postgreSQLBuilding2DRefreshResult_Full.FailedBatchCount);
                Assert.Equal(4, postgreSQLBuilding2DRefreshResult_Full.ReadCount);
                Assert.Equal(4, postgreSQLBuilding2DRefreshResult_Full.UpdatedCount);

                Dictionary<string, int?> subdivisionIds_ByReference = await GetSubdivisionIdsByReferenceAsync(npgsqlConnection);
                Assert.Equal(subdivisionId_A_Inner, subdivisionIds_ByReference[reference_A1]);
                Assert.Equal(subdivisionId_A_Outer, subdivisionIds_ByReference[reference_A2]);
                Assert.Equal(subdivisionId_A_Inner, subdivisionIds_ByReference[reference_A3]);
                Assert.Equal(subdivisionId_B, subdivisionIds_ByReference[reference_B1]);

                // Clear one building of its subdivision, then restart the walk at the middle of its part's
                // identifiers: the cleared row sits at or below the anchor within its own part, so it is
                // skipped, and the anchor admits no row of either part.
                await SetSubdivisionIdAsync(npgsqlConnection, reference_A1, null);

                PostgreSQLBuilding2DRefreshOptions postgreSQLBuilding2DRefreshOptions_Anchored = new()
                {
                    CountyIds = [countyId_A, countyId_B],
                    OverrideExistingSubdivisionIds = false,
                    StartId = id_A_Middle
                };

                PostgreSQLBuilding2DRefreshResult? postgreSQLBuilding2DRefreshResult_Anchored = await building2DPostgreSQLConverter.RefreshAsync(postgreSQLBuilding2DRefreshOptions_Anchored);
                Assert.NotNull(postgreSQLBuilding2DRefreshResult_Anchored);
                Assert.False(postgreSQLBuilding2DRefreshResult_Anchored.Cancelled);
                Assert.Equal(0, postgreSQLBuilding2DRefreshResult_Anchored.FailedBatchCount);
                Assert.Equal(0, postgreSQLBuilding2DRefreshResult_Anchored.ReadCount);
                Assert.Equal(0, postgreSQLBuilding2DRefreshResult_Anchored.UpdatedCount);

                Assert.Null((await GetSubdivisionIdsByReferenceAsync(npgsqlConnection))[reference_A1]);

                // Restarted from zero the same cleared row is within its part's anchor and is visited -
                // and only it is, the other three rows carrying a subdivision already.
                PostgreSQLBuilding2DRefreshOptions postgreSQLBuilding2DRefreshOptions_Restarted = new()
                {
                    CountyIds = [countyId_A, countyId_B],
                    OverrideExistingSubdivisionIds = false,
                    StartId = 0
                };

                PostgreSQLBuilding2DRefreshResult? postgreSQLBuilding2DRefreshResult_Restarted = await building2DPostgreSQLConverter.RefreshAsync(postgreSQLBuilding2DRefreshOptions_Restarted);
                Assert.NotNull(postgreSQLBuilding2DRefreshResult_Restarted);
                Assert.False(postgreSQLBuilding2DRefreshResult_Restarted.Cancelled);
                Assert.Equal(0, postgreSQLBuilding2DRefreshResult_Restarted.FailedBatchCount);
                Assert.Equal(1, postgreSQLBuilding2DRefreshResult_Restarted.ReadCount);
                Assert.Equal(1, postgreSQLBuilding2DRefreshResult_Restarted.UpdatedCount);

                Assert.Equal(subdivisionId_A_Inner, (await GetSubdivisionIdsByReferenceAsync(npgsqlConnection))[reference_A1]);
            }
            finally
            {
                await building2DPostgreSQLConverter.RemoveAsync([reference_A1, reference_A2, reference_A3], countyId_A);
                await building2DPostgreSQLConverter.RemoveAsync([reference_B1], countyId_B);
            }
        }

        /// <summary>
        /// Reads the identifier of an <c>administrative_areal_2d</c> row by its unique reference.
        /// </summary>
        /// <param name="npgsqlConnection">The open connection to read through.</param>
        /// <param name="reference">The reference of the row.</param>
        /// <returns>The row's identifier.</returns>
        private static async Task<int> GetAdministrativeAreal2DIdAsync(NpgsqlConnection npgsqlConnection, string reference)
        {
            await using NpgsqlCommand npgsqlCommand = new($"SELECT id FROM {Constants.TableName.AdministrativeAreal2D} WHERE reference = @reference", npgsqlConnection);
            npgsqlCommand.Parameters.AddWithValue("reference", reference);

            object? result = await npgsqlCommand.ExecuteScalarAsync();
            Assert.NotNull(result);

            return (int)result;
        }

        /// <summary>
        /// Reads the identifier of a <c>building_2d</c> row by its reference.
        /// </summary>
        /// <param name="npgsqlConnection">The open connection to read through.</param>
        /// <param name="reference">The reference of the row.</param>
        /// <returns>The row's identifier.</returns>
        private static async Task<long> GetBuilding2DIdAsync(NpgsqlConnection npgsqlConnection, string reference)
        {
            await using NpgsqlCommand npgsqlCommand = new($"SELECT id FROM {Constants.TableName.Building2D} WHERE reference = @reference", npgsqlConnection);
            npgsqlCommand.Parameters.AddWithValue("reference", reference);

            object? result = await npgsqlCommand.ExecuteScalarAsync();
            Assert.NotNull(result);

            return (long)result;
        }

        /// <summary>
        /// Sets the <c>subdivision_id</c> of the four buildings of this fact's parts, or clears it when the value is null.
        /// </summary>
        /// <param name="npgsqlConnection">The open connection to write through.</param>
        /// <param name="reference">The reference of the row.</param>
        /// <param name="subdivisionId">The subdivision identifier to store, or null to clear the column.</param>
        private static async Task SetSubdivisionIdAsync(NpgsqlConnection npgsqlConnection, string reference, int? subdivisionId)
        {
            await using NpgsqlCommand npgsqlCommand = new($"UPDATE {Constants.TableName.Building2D} SET subdivision_id = @subdivisionId WHERE reference = @reference", npgsqlConnection);
            npgsqlCommand.Parameters.AddWithValue("subdivisionId", (object?)subdivisionId ?? DBNull.Value);
            npgsqlCommand.Parameters.AddWithValue("reference", reference);

            Assert.Equal(1, await npgsqlCommand.ExecuteNonQueryAsync());
        }

        /// <summary>
        /// Reads the <c>subdivision_id</c> of every building of this fact's parts, keyed by reference.
        /// </summary>
        /// <param name="npgsqlConnection">The open connection to read through.</param>
        /// <returns>The stored subdivision identifiers.</returns>
        private static async Task<Dictionary<string, int?>> GetSubdivisionIdsByReferenceAsync(NpgsqlConnection npgsqlConnection)
        {
            Dictionary<string, int?> subdivisionIds_ByReference = [];

            await using NpgsqlCommand npgsqlCommand = new($"SELECT reference, subdivision_id FROM {Constants.TableName.Building2D} WHERE reference IN ('REFRESH_PART_WALK_A1', 'REFRESH_PART_WALK_A2', 'REFRESH_PART_WALK_A3', 'REFRESH_PART_WALK_B1')", npgsqlConnection);
            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync();
            while (await npgsqlDataReader.ReadAsync())
            {
                subdivisionIds_ByReference[npgsqlDataReader.GetString(0)] = npgsqlDataReader.IsDBNull(1) ? null : npgsqlDataReader.GetInt32(1);
            }

            return subdivisionIds_ByReference;
        }

        /// <summary>
        /// Builds a county polygon part as a square, with a unique reference and the stored extent a row of the table carries.
        /// </summary>
        /// <param name="reference">The unique reference of the part.</param>
        /// <param name="code">The county code shared by every part of the county.</param>
        /// <param name="name">The name of the part.</param>
        /// <param name="x">The X coordinate of the lower left corner.</param>
        /// <param name="y">The Y coordinate of the lower left corner.</param>
        /// <param name="size">The edge length of the square.</param>
        /// <returns>The county row.</returns>
        private static AdministrativeAreal2D AdministrativeAreal2D_SquareWithReferenceAndBoundingBox2D(string reference, string code, string name, double x, double y, double size)
        {
            AdministrativeAreal2D administrativeAreal2D = AdministrativeAreal2D_Square(0, code, x, y, size);
            administrativeAreal2D.Reference = reference;
            administrativeAreal2D.Name = name;
            administrativeAreal2D.AdministrativeArealType = AdministrativeArealType.County;
            administrativeAreal2D.BoundingBox2D = BoundingBox2D_Square(x, y, size);

            return administrativeAreal2D;
        }

        /// <summary>
        /// Builds a subdivision as a square with a unique reference and the stored extent a row of the table carries.
        /// </summary>
        /// <param name="reference">The unique reference of the subdivision.</param>
        /// <param name="code">The code the subdivision carries.</param>
        /// <param name="name">The name of the subdivision.</param>
        /// <param name="x">The X coordinate of the lower left corner.</param>
        /// <param name="y">The Y coordinate of the lower left corner.</param>
        /// <param name="size">The edge length of the square.</param>
        /// <returns>The subdivision row.</returns>
        private static AdministrativeAreal2D AdministrativeAreal2D_SubdivisionWithBoundingBox2D(string reference, string code, string name, double x, double y, double size)
        {
            IPolygonal2D_Square(x, y, size, out Polygon2D polygon2D);

            PolygonalFace2D? polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D(polygon2D);
            Assert.NotNull(polygonalFace2D);

            GIS.Classes.AdministrativeSubdivision administrativeSubdivision = new(Guid.NewGuid(), reference, code, polygonalFace2D, GIS.Enums.AdministrativeSubdivisionType.part_of_city, name, null);

            return new AdministrativeAreal2D()
            {
                Code = code,
                Reference = reference,
                Name = name,
                AdministrativeArealType = AdministrativeArealType.Subdivision,
                UniqueId = administrativeSubdivision.UniqueId,
                BoundingBox2D = BoundingBox2D_Square(x, y, size),
                Object = administrativeSubdivision.ToJsonObject()
            };
        }
    }
}
