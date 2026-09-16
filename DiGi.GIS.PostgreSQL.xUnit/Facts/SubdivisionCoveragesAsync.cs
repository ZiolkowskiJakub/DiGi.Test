using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the per-subdivision coverage read answers "not available" rather than throwing when a converter is missing or has no connection.
        /// <para>All three converters are required: the two tables live in different databases and the comparison is made in memory, and the subdivisions measured come from the administrative store. This needs no database: the guards under test decide the answer before a statement is built.</para>
        /// </summary>
        [Fact]
        public async Task SubdivisionCoveragesAsync_NoConnection_ReturnsNull()
        {
            OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(null);
            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);
            AdministrativeAreal2DPostgreSQLConverter administrativeAreal2DPostgreSQLConverter = new(null);

            Assert.Null(await ortoDatasPostgreSQLConverter.SubdivisionCoveragesAsync(building2DPostgreSQLConverter, administrativeAreal2DPostgreSQLConverter, 5));
            Assert.Null(await ortoDatasPostgreSQLConverter.SubdivisionCoveragesAsync(null, administrativeAreal2DPostgreSQLConverter, 5));
            Assert.Null(await ortoDatasPostgreSQLConverter.SubdivisionCoveragesAsync(building2DPostgreSQLConverter, null, 5));
            Assert.Null(await Query.SubdivisionCoveragesAsync(null, building2DPostgreSQLConverter, administrativeAreal2DPostgreSQLConverter, 5));
        }

        /// <summary>
        /// Verifies that the polygon coverage read answers "not available" rather than throwing when a converter or the polygon map is missing, or the converters have no connection.
        /// <para>A missing polygon map is not an empty one: nothing was asked to be measured, so there is no measurement, where an empty map measures the county and files every building under the null identifier. Needs no database.</para>
        /// </summary>
        [Fact]
        public async Task CoveragesAsync_NoConnection_ReturnsNull()
        {
            OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(null);
            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);
            Dictionary<int, PolygonalFace2D> polygonalFace2Ds_ById = [];

            Assert.Null(await ortoDatasPostgreSQLConverter.CoveragesAsync(building2DPostgreSQLConverter, 5, polygonalFace2Ds_ById));
            Assert.Null(await ortoDatasPostgreSQLConverter.CoveragesAsync(null, 5, polygonalFace2Ds_ById));
            Assert.Null(await ortoDatasPostgreSQLConverter.CoveragesAsync(building2DPostgreSQLConverter, 5, null));
            Assert.Null(await Query.CoveragesAsync(null, building2DPostgreSQLConverter, 5, polygonalFace2Ds_ById));
        }

        /// <summary>
        /// Verifies that a populated <see cref="OrtoDatasCoverageResult"/> survives a JSON round trip and a clone, with every figure carried over.
        /// </summary>
        [Fact]
        public void OrtoDatasCoverageResult_Serialization()
        {
            OrtoDatasCoverageResult ortoDatasCoverageResult = new(5, 19, 412, 407);

            Assert.Equal(5, ortoDatasCoverageResult.CountyId);
            Assert.Equal(19, ortoDatasCoverageResult.AdministrativeAreal2DId);
            Assert.Equal(412, ortoDatasCoverageResult.Building2DCount);
            Assert.Equal(407, ortoDatasCoverageResult.OrtoDatasCount);

            string? text = Core.Convert.ToSystem_String(ortoDatasCoverageResult);
            Assert.False(string.IsNullOrWhiteSpace(text));

            OrtoDatasCoverageResult? ortoDatasCoverageResult_Parsed = Core.Convert.ToDiGi<OrtoDatasCoverageResult>(text)?.FirstOrDefault();
            Assert.NotNull(ortoDatasCoverageResult_Parsed);

            Assert.Equal(5, ortoDatasCoverageResult_Parsed.CountyId);
            Assert.Equal(19, ortoDatasCoverageResult_Parsed.AdministrativeAreal2DId);
            Assert.Equal(412, ortoDatasCoverageResult_Parsed.Building2DCount);
            Assert.Equal(407, ortoDatasCoverageResult_Parsed.OrtoDatasCount);

            OrtoDatasCoverageResult ortoDatasCoverageResult_Clone = new(ortoDatasCoverageResult);

            Assert.Equal(5, ortoDatasCoverageResult_Clone.CountyId);
            Assert.Equal(19, ortoDatasCoverageResult_Clone.AdministrativeAreal2DId);
            Assert.Equal(412, ortoDatasCoverageResult_Clone.Building2DCount);
            Assert.Equal(407, ortoDatasCoverageResult_Clone.OrtoDatasCount);

            Core.xUnit.Query.SerializationCheck(ortoDatasCoverageResult);
        }

        /// <summary>
        /// Verifies that the row standing for the buildings inside none of the measured polygons keeps its null through a round trip and a clone.
        /// <para>That null is not a missing value, it is a fact about the county: those buildings lie in no subdivision polygon, so they belong to no subdivision and to no municipality and nothing below county level may count them. Were it to collapse to a zero on the wire it would name area 0 and quietly attribute them to a real area.</para>
        /// </summary>
        [Fact]
        public void OrtoDatasCoverageResult_UnassignedSubdivision()
        {
            OrtoDatasCoverageResult ortoDatasCoverageResult = new(5, null, 17, 0);

            Assert.Null(ortoDatasCoverageResult.AdministrativeAreal2DId);
            Assert.Equal(17, ortoDatasCoverageResult.Building2DCount);
            Assert.Equal(0, ortoDatasCoverageResult.OrtoDatasCount);

            OrtoDatasCoverageResult? ortoDatasCoverageResult_Parsed = Core.Convert.ToDiGi<OrtoDatasCoverageResult>(Core.Convert.ToSystem_String(ortoDatasCoverageResult))?.FirstOrDefault();
            Assert.NotNull(ortoDatasCoverageResult_Parsed);
            Assert.Null(ortoDatasCoverageResult_Parsed.AdministrativeAreal2DId);
            Assert.Equal(17, ortoDatasCoverageResult_Parsed.Building2DCount);

            OrtoDatasCoverageResult ortoDatasCoverageResult_Clone = new(ortoDatasCoverageResult);

            Assert.Null(ortoDatasCoverageResult_Clone.AdministrativeAreal2DId);
            Assert.Equal(17, ortoDatasCoverageResult_Clone.Building2DCount);

            Core.xUnit.Query.SerializationCheck(ortoDatasCoverageResult);
        }
    }
}
