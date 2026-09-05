using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a reference no county part holds is absent from the result rather than mapped to a guess.
        /// <para>The converter is built on null connection data, so <c>DiGi.PostgreSQL.Create.NpgsqlConnection</c> hands back null and the lookup answers null without touching a server - which stands in for a part that holds nothing. The caller has to see "not resolved" here: filing an item under a part its 2D building is not stored in is exactly what left sibling parts reading back empty while the upload reported success.</para>
        /// </summary>
        [Fact]
        public async Task CountyIdsByReferences_UnresolvedReferencesAreAbsent()
        {
            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);

            Dictionary<string, int> countyIds_ByReference = await building2DPostgreSQLConverter.CountyIdsByReferencesAsync(["reference_1", "reference_2"], [73482, 73485]);

            Assert.Empty(countyIds_ByReference);
        }

        /// <summary>
        /// Tests that the degenerate inputs answer an empty map instead of throwing.
        /// <para>It runs per posted batch on a path that already has nothing to fall back on, so a null reference list or an empty candidate set must cost the caller a warning, not a 500.</para>
        /// </summary>
        [Fact]
        public async Task CountyIdsByReferences_DegenerateInputs()
        {
            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);

            Assert.Empty(await building2DPostgreSQLConverter.CountyIdsByReferencesAsync(null, [73482]));
            Assert.Empty(await building2DPostgreSQLConverter.CountyIdsByReferencesAsync(["reference_1"], null));
            Assert.Empty(await building2DPostgreSQLConverter.CountyIdsByReferencesAsync(["reference_1"], []));
            Assert.Empty(await building2DPostgreSQLConverter.CountyIdsByReferencesAsync([null, string.Empty, "   "], [73482]));
            Assert.Empty(await Query.CountyIdsByReferencesAsync(null, ["reference_1"], [73482]));
        }
    }
}
