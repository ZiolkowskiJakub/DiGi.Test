using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="TerrainPointCoverageResult"/> properties are correctly initialized and survive serialization and copying.
        /// <para>The sample of missing coordinates is deliberately shorter than the count it belongs to, which is the ordinary case: the count is reported in full while the coordinates are capped by the caller.</para>
        /// </summary>
        [Fact]
        public void TerrainPointCoverageResult_Serialization()
        {
            int countyId = 2412;
            double gridSize = 100;
            double originX = 0;
            double originY = 0;
            long expectedCount = 104832;
            long storedCount = 104780;
            long missingCount = 52;
            long offGridCount = 0;

            List<Point2D> point2Ds_Missing =
            [
                new Point2D(471200, 243100),
                new Point2D(471300, 243100),
                new Point2D(471200, 243200)
            ];

            TerrainPointCoverageResult terrainPointCoverageResult = new(countyId, gridSize, originX, originY, expectedCount, storedCount, missingCount, offGridCount, point2Ds_Missing);

            Assert.Equal(countyId, terrainPointCoverageResult.CountyId);
            Assert.Equal(gridSize, terrainPointCoverageResult.GridSize);
            Assert.Equal(originX, terrainPointCoverageResult.OriginX);
            Assert.Equal(originY, terrainPointCoverageResult.OriginY);
            Assert.Equal(expectedCount, terrainPointCoverageResult.ExpectedCount);
            Assert.Equal(storedCount, terrainPointCoverageResult.StoredCount);
            Assert.Equal(missingCount, terrainPointCoverageResult.MissingCount);
            Assert.Equal(offGridCount, terrainPointCoverageResult.OffGridCount);
            Assert.Equal(3, terrainPointCoverageResult.Point2Ds_Missing.Count);

            string? json = Core.Convert.ToSystem_String(terrainPointCoverageResult);
            Assert.False(string.IsNullOrWhiteSpace(json));

            TerrainPointCoverageResult? terrainPointCoverage_Json = Core.Convert.ToDiGi<TerrainPointCoverageResult>(json)?.FirstOrDefault();
            Assert.NotNull(terrainPointCoverage_Json);
            Assert.Equal(expectedCount, terrainPointCoverage_Json.ExpectedCount);
            Assert.Equal(missingCount, terrainPointCoverage_Json.MissingCount);
            Assert.Equal(3, terrainPointCoverage_Json.Point2Ds_Missing.Count);
            Assert.Equal(471300, terrainPointCoverage_Json.Point2Ds_Missing[1].X);

            Core.xUnit.Query.SerializationCheck(terrainPointCoverageResult);

            TerrainPointCoverageResult terrainPointCoverage_Clone = new(terrainPointCoverageResult);

            Assert.Equal(terrainPointCoverageResult.CountyId, terrainPointCoverage_Clone.CountyId);
            Assert.Equal(terrainPointCoverageResult.ExpectedCount, terrainPointCoverage_Clone.ExpectedCount);
            Assert.Equal(terrainPointCoverageResult.StoredCount, terrainPointCoverage_Clone.StoredCount);
            Assert.Equal(terrainPointCoverageResult.MissingCount, terrainPointCoverage_Clone.MissingCount);
            Assert.Equal(terrainPointCoverageResult.Point2Ds_Missing.Count, terrainPointCoverage_Clone.Point2Ds_Missing.Count);

            // Cloned element by element rather than by sharing the list, so a change to one result cannot reach the other.
            Assert.NotSame(terrainPointCoverageResult.Point2Ds_Missing[0], terrainPointCoverage_Clone.Point2Ds_Missing[0]);
            Assert.Equal(terrainPointCoverageResult.Point2Ds_Missing[0].X, terrainPointCoverage_Clone.Point2Ds_Missing[0].X);
            Assert.Equal(terrainPointCoverageResult.Point2Ds_Missing[0].Y, terrainPointCoverage_Clone.Point2Ds_Missing[0].Y);
        }

        /// <summary>
        /// Verifies that <see cref="TerrainPointCoverageResult"/> accepts a run with nothing missing, and that the empty sample round-trips as an empty list rather than as null.
        /// </summary>
        [Fact]
        public void TerrainPointCoverageResult_Complete()
        {
            TerrainPointCoverageResult terrainPointCoverageResult = new(2412, 100, 0, 0, 104832, 104832, 0, 0, null);

            Assert.Equal(0, terrainPointCoverageResult.MissingCount);
            Assert.NotNull(terrainPointCoverageResult.Point2Ds_Missing);
            Assert.Empty(terrainPointCoverageResult.Point2Ds_Missing);

            string? json = Core.Convert.ToSystem_String(terrainPointCoverageResult);
            Assert.False(string.IsNullOrWhiteSpace(json));

            TerrainPointCoverageResult? terrainPointCoverage_Json = Core.Convert.ToDiGi<TerrainPointCoverageResult>(json)?.FirstOrDefault();
            Assert.NotNull(terrainPointCoverage_Json);
            Assert.Equal(terrainPointCoverageResult.ExpectedCount, terrainPointCoverage_Json.StoredCount);
            Assert.NotNull(terrainPointCoverage_Json.Point2Ds_Missing);
            Assert.Empty(terrainPointCoverage_Json.Point2Ds_Missing);

            Core.xUnit.Query.SerializationCheck(terrainPointCoverageResult);
        }
    }
}
