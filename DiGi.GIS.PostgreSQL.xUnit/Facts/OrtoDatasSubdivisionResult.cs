using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the four categories partition the matched references, that the samples survive a JSON round trip, and that a clone holds its own lists.
        /// <para>The partition is the property the whole result rests on: a reference present on both sides falls into exactly one of both-linked, orthophoto-only, building-only and neither, so those four have to add up to the matched count or the comparison is losing rows.</para>
        /// </summary>
        [Fact]
        public void OrtoDatasSubdivisionResult_Serialization()
        {
            OrtoDatasSubdivisionResult ortoDatasSubdivisionResult = new(55417, 21_004, 33_687, 20_800, 15_300, 42, 1_200, 3_100, 1_200, ["reference_1", "reference_2"], ["reference_3"], ["reference_4", "reference_5", "reference_6"]);

            Assert.Equal(55417, ortoDatasSubdivisionResult.CountyId);
            Assert.Equal(21_004, ortoDatasSubdivisionResult.OrtoDatasCount);
            Assert.Equal(33_687, ortoDatasSubdivisionResult.Building2DCount);
            Assert.Equal(20_800, ortoDatasSubdivisionResult.MatchedCount);
            Assert.Equal(15_300, ortoDatasSubdivisionResult.BothCount);
            Assert.Equal(42, ortoDatasSubdivisionResult.DisagreeCount);
            Assert.Equal(1_200, ortoDatasSubdivisionResult.OrtoDatasOnlyCount);
            Assert.Equal(3_100, ortoDatasSubdivisionResult.Building2DOnlyCount);
            Assert.Equal(1_200, ortoDatasSubdivisionResult.NeitherCount);

            // The four categories have to account for every matched reference and nothing else.
            Assert.Equal(
                ortoDatasSubdivisionResult.MatchedCount,
                ortoDatasSubdivisionResult.BothCount + ortoDatasSubdivisionResult.OrtoDatasOnlyCount + ortoDatasSubdivisionResult.Building2DOnlyCount + ortoDatasSubdivisionResult.NeitherCount);

            // A disagreement is a kind of both-linked, never a category of its own.
            Assert.True(ortoDatasSubdivisionResult.DisagreeCount <= ortoDatasSubdivisionResult.BothCount);

            string? json = Core.Convert.ToSystem_String(ortoDatasSubdivisionResult);
            Assert.NotNull(json);

            OrtoDatasSubdivisionResult? ortoDatasSubdivisionResult_Json = Core.Convert.ToDiGi<OrtoDatasSubdivisionResult>(json)?.FirstOrDefault();
            Assert.NotNull(ortoDatasSubdivisionResult_Json);

            Assert.Equal(20_800, ortoDatasSubdivisionResult_Json.MatchedCount);
            Assert.Equal(1_200, ortoDatasSubdivisionResult_Json.OrtoDatasOnlyCount);
            Assert.Equal(3_100, ortoDatasSubdivisionResult_Json.Building2DOnlyCount);
            Assert.Equal(2, ortoDatasSubdivisionResult_Json.References_OrtoDatasOnly.Count);
            Assert.Contains("reference_1", ortoDatasSubdivisionResult_Json.References_OrtoDatasOnly);
            Assert.Single(ortoDatasSubdivisionResult_Json.References_Building2DOnly);
            Assert.Equal(3, ortoDatasSubdivisionResult_Json.References_Disagree.Count);

            OrtoDatasSubdivisionResult ortoDatasSubdivisionResult_Clone = new(ortoDatasSubdivisionResult);

            Assert.Equal(20_800, ortoDatasSubdivisionResult_Clone.MatchedCount);
            Assert.Equal(2, ortoDatasSubdivisionResult_Clone.References_OrtoDatasOnly.Count);

            // The clone has to hold its own lists, or editing one result would rewrite the other.
            Assert.NotSame(ortoDatasSubdivisionResult.References_OrtoDatasOnly, ortoDatasSubdivisionResult_Clone.References_OrtoDatasOnly);
            Assert.NotSame(ortoDatasSubdivisionResult.References_Building2DOnly, ortoDatasSubdivisionResult_Clone.References_Building2DOnly);
            Assert.NotSame(ortoDatasSubdivisionResult.References_Disagree, ortoDatasSubdivisionResult_Clone.References_Disagree);

            Core.xUnit.Query.SerializationCheck(ortoDatasSubdivisionResult);
        }

        /// <summary>
        /// Verifies that null sample collections become empty lists rather than nulls.
        /// <para>A caller reading the samples should never have to null check them, and a county with no disagreement at all is the ordinary case rather than an exceptional one.</para>
        /// </summary>
        [Fact]
        public void OrtoDatasSubdivisionResult_NullSamples()
        {
            OrtoDatasSubdivisionResult ortoDatasSubdivisionResult = new(55417, 0, 0, 0, 0, 0, 0, 0, 0, null, null, null);

            Assert.NotNull(ortoDatasSubdivisionResult.References_OrtoDatasOnly);
            Assert.Empty(ortoDatasSubdivisionResult.References_OrtoDatasOnly);
            Assert.NotNull(ortoDatasSubdivisionResult.References_Building2DOnly);
            Assert.Empty(ortoDatasSubdivisionResult.References_Building2DOnly);
            Assert.NotNull(ortoDatasSubdivisionResult.References_Disagree);
            Assert.Empty(ortoDatasSubdivisionResult.References_Disagree);

            Core.xUnit.Query.SerializationCheck(ortoDatasSubdivisionResult);
        }

        /// <summary>
        /// Verifies that the comparison answers null when either converter is missing, without touching a database.
        /// <para>The two sides live in different databases, so the method needs both converters and can be asked for neither. Answering null rather than throwing is what lets the endpoint report a missing converter as a bad request.</para>
        /// </summary>
        [Fact]
        public async Task SubdivisionLinksAsync_NullConverter_ReturnsNull()
        {
            OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(null);
            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);

            OrtoDatasSubdivisionResult? result_NullOrtoDatas = await Query.SubdivisionLinksAsync(null, building2DPostgreSQLConverter, 55417);
            Assert.Null(result_NullOrtoDatas);

            OrtoDatasSubdivisionResult? result_NullBuilding2D = await ortoDatasPostgreSQLConverter.SubdivisionLinksAsync(null, 55417);
            Assert.Null(result_NullBuilding2D);

            // Both present but neither able to connect: the orthophoto side is read first and answers null,
            // so the comparison stops there rather than half completing.
            OrtoDatasSubdivisionResult? result_NoConnection = await ortoDatasPostgreSQLConverter.SubdivisionLinksAsync(building2DPostgreSQLConverter, 55417);
            Assert.Null(result_NoConnection);
        }
    }
}
