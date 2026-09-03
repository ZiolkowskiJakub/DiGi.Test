using DiGi.GIS.PostgreSQL.Classes;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a populated <see cref="BuildingDataCoverageResult"/> survives a JSON round trip and a clone, with every figure carried over.
        /// </summary>
        [Fact]
        public void BuildingDataCoverageResult_Serialization()
        {
            BuildingDataCoverageResult buildingDataCoverageResult = new(55417, 33687, 21894, 11793, 42, 11751, 2);

            Assert.Equal(55417, buildingDataCoverageResult.CountyId);
            Assert.Equal(33687, buildingDataCoverageResult.Building2DCount);
            Assert.Equal(21894, buildingDataCoverageResult.BuildingDataCount);
            Assert.Equal(11793, buildingDataCoverageResult.MissingReferenceCount);
            Assert.Equal(42, buildingDataCoverageResult.OrphanReferenceCount);
            Assert.Equal(11751, buildingDataCoverageResult.UnassignedSubdivisionCount);
            Assert.Equal(2, buildingDataCoverageResult.CrossCountySubdivisionCount);

            string? text = Core.Convert.ToSystem_String(buildingDataCoverageResult);
            Assert.False(string.IsNullOrWhiteSpace(text));

            BuildingDataCoverageResult? buildingDataCoverageResult_Parsed = Core.Convert.ToDiGi<BuildingDataCoverageResult>(text)?.FirstOrDefault();
            Assert.NotNull(buildingDataCoverageResult_Parsed);

            Assert.Equal(55417, buildingDataCoverageResult_Parsed.CountyId);
            Assert.Equal(33687, buildingDataCoverageResult_Parsed.Building2DCount);
            Assert.Equal(21894, buildingDataCoverageResult_Parsed.BuildingDataCount);
            Assert.Equal(11793, buildingDataCoverageResult_Parsed.MissingReferenceCount);
            Assert.Equal(42, buildingDataCoverageResult_Parsed.OrphanReferenceCount);
            Assert.Equal(11751, buildingDataCoverageResult_Parsed.UnassignedSubdivisionCount);
            Assert.Equal(2, buildingDataCoverageResult_Parsed.CrossCountySubdivisionCount);

            BuildingDataCoverageResult buildingDataCoverageResult_Clone = new(buildingDataCoverageResult);

            Assert.Equal(55417, buildingDataCoverageResult_Clone.CountyId);
            Assert.Equal(33687, buildingDataCoverageResult_Clone.Building2DCount);
            Assert.Equal(11751, buildingDataCoverageResult_Clone.UnassignedSubdivisionCount);
            Assert.Equal(2, buildingDataCoverageResult_Clone.CrossCountySubdivisionCount);

            Core.xUnit.Query.SerializationCheck(buildingDataCoverageResult);
        }

        /// <summary>
        /// Verifies that the coverage factory returns null rather than a partly measured result when either converter is missing.
        /// <para>A coverage figure worked out from one side only would read as a total shortfall, which is exactly the shape of a real defect - so the absence of an answer has to stay distinguishable from an answer of zero coverage.</para>
        /// </summary>
        [Fact]
        public async Task BuildingDataCoverageResultAsync_MissingConverter_ReturnsNull()
        {
            BuildingDataPostgreSQLConverter buildingDataPostgreSQLConverter = new(null);
            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);

            Assert.Null(await buildingDataPostgreSQLConverter.BuildingDataCoverageResultAsync(null, 55417));
            Assert.Null(await Create.BuildingDataCoverageResultAsync(null, building2DPostgreSQLConverter, 55417));
        }

        /// <summary>
        /// Verifies that the coverage of a populated county is internally consistent and that the two sides are actually being compared.
        /// <para>Skipped by default: it reads both databases and therefore needs <c>GIS_PostgreSQL_Main.conf</c> and <c>GIS_PostgreSQL_Storage.conf</c> pointing at populated ones.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query against both databases. Point GIS_PostgreSQL_Main.conf and GIS_PostgreSQL_Storage.conf at a database before running.")]
        public async Task BuildingDataCoverageResultAsync_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingDataPostgreSQLConverter>();
            Assert.NotNull(buildingDataPostgreSQLConverter);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            int countyId = 55417;

            BuildingDataCoverageResult? buildingDataCoverageResult = await buildingDataPostgreSQLConverter.BuildingDataCoverageResultAsync(building2DPostgreSQLConverter, countyId);
            Assert.NotNull(buildingDataCoverageResult);

            Assert.Equal(countyId, buildingDataCoverageResult.CountyId);

            // No figure can be negative, and neither difference can exceed the side it was taken from.
            Assert.True(buildingDataCoverageResult.Building2DCount >= 0);
            Assert.True(buildingDataCoverageResult.BuildingDataCount >= 0);
            Assert.True(buildingDataCoverageResult.MissingReferenceCount >= 0);
            Assert.True(buildingDataCoverageResult.OrphanReferenceCount >= 0);
            Assert.True(buildingDataCoverageResult.UnassignedSubdivisionCount >= 0);
            Assert.True(buildingDataCoverageResult.CrossCountySubdivisionCount >= 0);
            Assert.True(buildingDataCoverageResult.MissingReferenceCount <= buildingDataCoverageResult.Building2DCount);
            Assert.True(buildingDataCoverageResult.OrphanReferenceCount <= buildingDataCoverageResult.BuildingDataCount);

            // The set identity the two differences have to satisfy: what each side holds, less what only it holds,
            // is the overlap - and the overlap is the same number seen from either side.
            long overlap_Building2D = buildingDataCoverageResult.Building2DCount - buildingDataCoverageResult.MissingReferenceCount;
            long overlap_BuildingData = buildingDataCoverageResult.BuildingDataCount - buildingDataCoverageResult.OrphanReferenceCount;
            Assert.Equal(overlap_Building2D, overlap_BuildingData);

            // A building that names no subdivision is never visited by the update, so it cannot have been covered.
            Assert.True(buildingDataCoverageResult.UnassignedSubdivisionCount <= buildingDataCoverageResult.MissingReferenceCount);
        }
    }
}
