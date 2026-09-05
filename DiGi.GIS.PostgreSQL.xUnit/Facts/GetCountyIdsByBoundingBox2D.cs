using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the county identifiers a spatial query prunes to include every polygon part of a county, not only the parts overlapping the box.
        /// <para>A county whose territory is disconnected is one row per part, each with its own identifier, and <c>building_2d</c> is partitioned by that identifier. A building is filed under one part and need not be filed under the part whose polygon covers it, so pruning to the overlapping parts alone answers nothing at all in those counties - silently, because an empty result reads exactly like an area holding no buildings. See https://github.com/ZiolkowskiJakub/DiGi.GIS.PostgreSQL/issues/64.</para>
        /// <para>Asserted against the bounding box of one part of a multi-part county: the answer has to name every sibling part of that county.</para>
        /// <para>Skipped by default: it queries a populated database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a populated database before running.")]
        public async Task GetCountyIdsByBoundingBox2D_MultiPartCounty_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            List<AdministrativeAreal2DReference>? countyReferences = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.County);
            Assert.NotNull(countyReferences);
            Assert.NotEmpty(countyReferences);

            // The same grouping the writes use, so the fact cannot disagree with production about what a sibling is.
            Dictionary<int, HashSet<int>> siblingCountyGroups = countyReferences.SiblingCountyGroups();

            KeyValuePair<int, HashSet<int>> siblingCountyGroup = siblingCountyGroups.FirstOrDefault(x => x.Value.Count > 1);
            Assert.True(siblingCountyGroup.Value is not null && siblingCountyGroup.Value.Count > 1, "The database holds no multi-part county, so there is nothing for this fact to measure.");

            List<AdministrativeAreal2D>? administrativeAreal2Ds = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByIdsAsync([siblingCountyGroup.Key]);
            Assert.NotNull(administrativeAreal2Ds);
            Assert.Single(administrativeAreal2Ds);

            BoundingBox2D? boundingBox2D = administrativeAreal2Ds[0].BoundingBox2D;
            Assert.NotNull(boundingBox2D);

            HashSet<int>? countyIds = await administrativeAreal2DPostgreSQLConverter.GetCountyIdsByBoundingBox2DAsync(boundingBox2D);
            Assert.NotNull(countyIds);

            // Every part of the county the box sits in, whether or not that part reaches the box.
            foreach (int countyId in siblingCountyGroup.Value)
            {
                Assert.Contains(countyId, countyIds);
            }
        }
    }
}
