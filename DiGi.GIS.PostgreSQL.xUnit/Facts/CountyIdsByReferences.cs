using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a lookup which could not run answers <c>null</c>, distinct from an empty map.
        /// <para>The converter is built on null connection data, so <c>DiGi.PostgreSQL.Create.NpgsqlConnection</c> hands back null and the lookup cannot execute without touching a server. This fact's earlier form read that as "a part that holds nothing" and asserted an empty map - the conflation that let a broken connection look like a clean no-op while items were silently dropped.</para>
        /// </summary>
        [Fact]
        public async Task CountyIdsByReferences_LookupNotRunAnswersNull()
        {
            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);

            Dictionary<string, int>? countyIds_ByReference = await building2DPostgreSQLConverter.CountyIdsByReferencesAsync(["reference_1", "reference_2"], [73482, 73485]);

            Assert.Null(countyIds_ByReference);
        }

        /// <summary>
        /// Tests that the degenerate inputs answer an empty map instead of throwing or <c>null</c>.
        /// <para>Nothing being asked is a legitimate empty answer, not a failure: a null reference list or an empty candidate set must cost the caller nothing. Only the null converter means the lookup could not run, and that case answers <c>null</c>.</para>
        /// </summary>
        [Fact]
        public async Task CountyIdsByReferences_DegenerateInputs()
        {
            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);

            Dictionary<string, int>? countyIds_ByReference;

            countyIds_ByReference = await building2DPostgreSQLConverter.CountyIdsByReferencesAsync(null, [73482]);
            Assert.NotNull(countyIds_ByReference);
            Assert.Empty(countyIds_ByReference);

            countyIds_ByReference = await building2DPostgreSQLConverter.CountyIdsByReferencesAsync(["reference_1"], null);
            Assert.NotNull(countyIds_ByReference);
            Assert.Empty(countyIds_ByReference);

            countyIds_ByReference = await building2DPostgreSQLConverter.CountyIdsByReferencesAsync(["reference_1"], []);
            Assert.NotNull(countyIds_ByReference);
            Assert.Empty(countyIds_ByReference);

            countyIds_ByReference = await building2DPostgreSQLConverter.CountyIdsByReferencesAsync([null, string.Empty, "   "], [73482]);
            Assert.NotNull(countyIds_ByReference);
            Assert.Empty(countyIds_ByReference);

            Assert.Null(await Query.CountyIdsByReferencesAsync(null, ["reference_1"], [73482]));
        }

        /// <summary>
        /// Tests that the sibling-fallback resolver answers an empty map on degenerate inputs rather than throwing or <c>null</c>.
        /// <para>Nothing being asked - a null reference list, an empty candidate set, blank references - is decided before the widening, so it answers an empty map even though the widening cannot run on a null connection. A null building converter means the first-pass lookup could not run, and that answers <c>null</c>.</para>
        /// </summary>
        [Fact]
        public async Task CountyIdsByReferencesWithSiblingFallback_DegenerateInputs()
        {
            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);
            AdministrativeAreal2DPostgreSQLConverter administrativeAreal2DPostgreSQLConverter = new(null);

            Dictionary<string, int>? countyIds_ByReference;

            Assert.Null(await Query.CountyIdsByReferencesWithSiblingFallbackAsync(null, null, null, null));

            countyIds_ByReference = await building2DPostgreSQLConverter.CountyIdsByReferencesWithSiblingFallbackAsync(administrativeAreal2DPostgreSQLConverter, ["reference_1"], null);
            Assert.NotNull(countyIds_ByReference);
            Assert.Empty(countyIds_ByReference);

            countyIds_ByReference = await building2DPostgreSQLConverter.CountyIdsByReferencesWithSiblingFallbackAsync(administrativeAreal2DPostgreSQLConverter, ["reference_1"], []);
            Assert.NotNull(countyIds_ByReference);
            Assert.Empty(countyIds_ByReference);

            countyIds_ByReference = await building2DPostgreSQLConverter.CountyIdsByReferencesWithSiblingFallbackAsync(administrativeAreal2DPostgreSQLConverter, [null, string.Empty, "   "], [73482]);
            Assert.NotNull(countyIds_ByReference);
            Assert.Empty(countyIds_ByReference);

            Assert.Null(await Query.CountyIdsByReferencesWithSiblingFallbackAsync(null, administrativeAreal2DPostgreSQLConverter, ["reference_1"], [73482]));
        }

        /// <summary>
        /// Tests against a live database that a reference no part holds is absent from the result rather than answered <c>null</c>.
        /// <para>This is the only fact that proves the "ran, resolved nothing" leg with a real connection: a fabricated reference resolves to no row, the lookup itself executed, and the answer is an empty map - not <c>null</c>, which is reserved for a lookup that could not run.</para>
        /// <para>Skipped by default: it executes an integration query requiring <c>GIS_PostgreSQL_Main.conf</c> pointing at a development database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a development database before running.")]
        public async Task CountyIdsByReferences_UnresolvedReferencesAreAbsent_Database()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            // 2212 is a two-part county (73482, 73485), so both parts are probed and the answer exercises
            // the full loop rather than a single-part shortcut.
            Dictionary<string, int>? countyIds_ByReference = await building2DPostgreSQLConverter.CountyIdsByReferencesAsync(["reference_that_no_building_holds"], [73482, 73485]);

            Assert.NotNull(countyIds_ByReference);
            Assert.Empty(countyIds_ByReference);
        }
    }
}
