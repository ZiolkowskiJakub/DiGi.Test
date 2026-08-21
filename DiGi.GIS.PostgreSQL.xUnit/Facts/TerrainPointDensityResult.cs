using DiGi.GIS.PostgreSQL.Classes;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Create.TerrainPointDensityResult(int, long, double, double?)"/> derives the density, the equivalent spacing and the completeness, and that the result survives serialization and copying.
        /// <para>A county holding exactly one point per cell of the lattice it was sampled on is the case worth pinning: it has to report a spacing equal to that lattice and a completeness of one, and the two are derived by different routes.</para>
        /// </summary>
        [Fact]
        public void TerrainPointDensityResult_Create()
        {
            int countyId = 2412;
            double gridSize = 100;

            // A square 100 km on a side, sampled at every node of a 100 m lattice.
            double area = 100000000;
            long count = 10000;

            TerrainPointDensityResult? terrainPointDensityResult = Create.TerrainPointDensityResult(countyId, count, area, gridSize);

            Assert.NotNull(terrainPointDensityResult);
            Assert.Equal(countyId, terrainPointDensityResult.CountyId);
            Assert.Equal(count, terrainPointDensityResult.Count);
            Assert.Equal(area, terrainPointDensityResult.Area);
            Assert.Equal(0.0001, terrainPointDensityResult.Density);
            Assert.Equal(gridSize, terrainPointDensityResult.SpacingEquivalent);
            Assert.Equal(0.0001, terrainPointDensityResult.ExpectedDensity);
            Assert.Equal(1, terrainPointDensityResult.Completeness);

            string? json = Core.Convert.ToSystem_String(terrainPointDensityResult);
            Assert.False(string.IsNullOrWhiteSpace(json));

            TerrainPointDensityResult? terrainPointDensity_Json = Core.Convert.ToDiGi<TerrainPointDensityResult>(json)?.FirstOrDefault();
            Assert.NotNull(terrainPointDensity_Json);
            Assert.Equal(terrainPointDensityResult.Density, terrainPointDensity_Json.Density);
            Assert.Equal(terrainPointDensityResult.SpacingEquivalent, terrainPointDensity_Json.SpacingEquivalent);
            Assert.Equal(terrainPointDensityResult.Completeness, terrainPointDensity_Json.Completeness);

            Core.xUnit.Query.SerializationCheck(terrainPointDensityResult);

            TerrainPointDensityResult terrainPointDensity_Clone = new(terrainPointDensityResult);

            Assert.Equal(terrainPointDensityResult.CountyId, terrainPointDensity_Clone.CountyId);
            Assert.Equal(terrainPointDensityResult.Count, terrainPointDensity_Clone.Count);
            Assert.Equal(terrainPointDensityResult.Area, terrainPointDensity_Clone.Area);
            Assert.Equal(terrainPointDensityResult.Density, terrainPointDensity_Clone.Density);
            Assert.Equal(terrainPointDensityResult.SpacingEquivalent, terrainPointDensity_Clone.SpacingEquivalent);
            Assert.Equal(terrainPointDensityResult.Completeness, terrainPointDensity_Clone.Completeness);
        }

        /// <summary>
        /// Verifies that <see cref="Create.TerrainPointDensityResult(int, long, double, double?)"/> leaves a figure it cannot derive null rather than filling it with a not-a-number, and that a null survives the round trip.
        /// <para>The distinction matters at the wire: strict JSON has no token for a not-a-number, so one reaching a response body is a serialization failure rather than a value a reader can act on.</para>
        /// </summary>
        [Fact]
        public void TerrainPointDensityResult_Create_Undeterminable()
        {
            // No grid size, so neither the expected density nor the completeness can be worked out.
            TerrainPointDensityResult? terrainPointDensityResult = Create.TerrainPointDensityResult(2412, 10000, 100000000, null);

            Assert.NotNull(terrainPointDensityResult);
            Assert.Equal(0.0001, terrainPointDensityResult.Density);
            Assert.Equal(100, terrainPointDensityResult.SpacingEquivalent);
            Assert.Null(terrainPointDensityResult.ExpectedDensity);
            Assert.Null(terrainPointDensityResult.Completeness);

            string? json = Core.Convert.ToSystem_String(terrainPointDensityResult);
            Assert.False(string.IsNullOrWhiteSpace(json));

            TerrainPointDensityResult? terrainPointDensity_Json = Core.Convert.ToDiGi<TerrainPointDensityResult>(json)?.FirstOrDefault();
            Assert.NotNull(terrainPointDensity_Json);
            Assert.Null(terrainPointDensity_Json.ExpectedDensity);
            Assert.Null(terrainPointDensity_Json.Completeness);

            Core.xUnit.Query.SerializationCheck(terrainPointDensityResult);

            // A county with no area to measure against reports no density and no spacing rather than an infinite one.
            TerrainPointDensityResult? terrainPointDensity_NoArea = Create.TerrainPointDensityResult(2412, 10000, 0, 100);

            Assert.NotNull(terrainPointDensity_NoArea);
            Assert.Null(terrainPointDensity_NoArea.Density);
            Assert.Null(terrainPointDensity_NoArea.SpacingEquivalent);
            Assert.Null(terrainPointDensity_NoArea.Completeness);
            Assert.Equal(0.0001, terrainPointDensity_NoArea.ExpectedDensity);

            // A county holding nothing reports a density of zero, which is a measurement, but no spacing.
            TerrainPointDensityResult? terrainPointDensity_NoPoint = Create.TerrainPointDensityResult(2412, 0, 100000000, 100);

            Assert.NotNull(terrainPointDensity_NoPoint);
            Assert.Equal(0, terrainPointDensity_NoPoint.Density);
            Assert.Null(terrainPointDensity_NoPoint.SpacingEquivalent);
            Assert.Equal(0, terrainPointDensity_NoPoint.Completeness);

            // Input that cannot describe a county at all is refused rather than reported as an empty measurement.
            Assert.Null(Create.TerrainPointDensityResult(2412, -1, 100000000, 100));
            Assert.Null(Create.TerrainPointDensityResult(2412, 10000, double.NaN, 100));
        }
    }
}
