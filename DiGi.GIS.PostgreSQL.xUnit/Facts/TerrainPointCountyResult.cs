using DiGi.GIS.PostgreSQL.Classes;
using System;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="TerrainPointCountyResult"/> properties are correctly initialized and survive serialization and copying.
        /// <para>The two timestamps are carried as one value and one null, because a county whose points were written before the column existed reports no moment at all and that has to round-trip as a null rather than as a default date.</para>
        /// <para>The moment is asserted after the round trip as well as before it, which is what pins the choice of an offset over a plain date: a plain date is written as UTC, read back in the local zone, and written again with an offset, so the check that the two documents match would fail on any machine not sitting at UTC.</para>
        /// </summary>
        [Fact]
        public void TerrainPointCountyResult_Serialization()
        {
            int countyId = 2412;
            long count = 1234567;
            double minX = 471200;
            double maxX = 512800;
            double minY = 243100;
            double maxY = 281900;
            double minZ = 197.4;
            double maxZ = 486.1;
            long zeroElevationCount = 18;
            long subdivisionCount = 11;
            long unassignedSubdivisionCount = 0;
            DateTimeOffset createdAt_First = new(2026, 8, 18, 21, 4, 33, TimeSpan.Zero);

            TerrainPointCountyResult terrainPointCountyResult = new(countyId, count, minX, maxX, minY, maxY, minZ, maxZ, zeroElevationCount, subdivisionCount, unassignedSubdivisionCount, createdAt_First, null);

            Assert.Equal(countyId, terrainPointCountyResult.CountyId);
            Assert.Equal(count, terrainPointCountyResult.Count);
            Assert.Equal(minX, terrainPointCountyResult.MinX);
            Assert.Equal(maxX, terrainPointCountyResult.MaxX);
            Assert.Equal(minY, terrainPointCountyResult.MinY);
            Assert.Equal(maxY, terrainPointCountyResult.MaxY);
            Assert.Equal(minZ, terrainPointCountyResult.MinZ);
            Assert.Equal(maxZ, terrainPointCountyResult.MaxZ);
            Assert.Equal(zeroElevationCount, terrainPointCountyResult.ZeroElevationCount);
            Assert.Equal(subdivisionCount, terrainPointCountyResult.SubdivisionCount);
            Assert.Equal(unassignedSubdivisionCount, terrainPointCountyResult.UnassignedSubdivisionCount);
            Assert.Equal(createdAt_First, terrainPointCountyResult.CreatedAt_First);
            Assert.Null(terrainPointCountyResult.CreatedAt_Last);

            string? json = Core.Convert.ToSystem_String(terrainPointCountyResult);
            Assert.False(string.IsNullOrWhiteSpace(json));

            TerrainPointCountyResult? terrainPointCountySummary_Json = Core.Convert.ToDiGi<TerrainPointCountyResult>(json)?.FirstOrDefault();
            Assert.NotNull(terrainPointCountySummary_Json);
            Assert.Equal(countyId, terrainPointCountySummary_Json.CountyId);
            Assert.Equal(count, terrainPointCountySummary_Json.Count);
            Assert.Equal(zeroElevationCount, terrainPointCountySummary_Json.ZeroElevationCount);
            // Compared exactly rather than as an instant. An offset survives the round trip as it was written,
            // where a plain DateTime comes back rendered in the local zone and would only match as an instant.
            Assert.Equal(createdAt_First, terrainPointCountySummary_Json.CreatedAt_First);
            Assert.Null(terrainPointCountySummary_Json.CreatedAt_Last);

            Core.xUnit.Query.SerializationCheck(terrainPointCountyResult);

            TerrainPointCountyResult terrainPointCountySummary_Clone = new(terrainPointCountyResult);

            Assert.Equal(terrainPointCountyResult.CountyId, terrainPointCountySummary_Clone.CountyId);
            Assert.Equal(terrainPointCountyResult.Count, terrainPointCountySummary_Clone.Count);
            Assert.Equal(terrainPointCountyResult.MinZ, terrainPointCountySummary_Clone.MinZ);
            Assert.Equal(terrainPointCountyResult.MaxZ, terrainPointCountySummary_Clone.MaxZ);
            Assert.Equal(terrainPointCountyResult.SubdivisionCount, terrainPointCountySummary_Clone.SubdivisionCount);
            Assert.Equal(terrainPointCountyResult.CreatedAt_First, terrainPointCountySummary_Clone.CreatedAt_First);
            Assert.Null(terrainPointCountySummary_Clone.CreatedAt_Last);
        }
    }
}
