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
        /// Verifies that a bounding box read returns the buildings standing inside it in a county whose territory is held as several polygon parts.
        /// <para>The partitions to read used to come from the <c>county_id</c> the subdivisions in the box carry, and that column names one part. Where the buildings sit under a sibling part the read answered nothing whatsoever, which is what left twelve multi-part counties with no radial ratios, or with zeros, after a national run that reported success - the ratios are measured against whatever this read returns, and an empty answer is written as a zero rather than raised. See https://github.com/ZiolkowskiJakub/DiGi.GIS.PostgreSQL/issues/64.</para>
        /// <para>A building is picked out of a multi-part county and looked for in a box drawn around its own outline, which it cannot fail to be inside.</para>
        /// <para>Skipped by default: it queries a populated database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a populated database before running.")]
        public async Task GetBuilding2DsByBoundingBox2D_SiblingCountyPart_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            List<AdministrativeAreal2DReference>? countyReferences = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.County);
            Assert.NotNull(countyReferences);
            Assert.NotEmpty(countyReferences);

            Dictionary<int, HashSet<int>> siblingCountyGroups = countyReferences.SiblingCountyGroups();

            Building2D? building2D = null;

            foreach (KeyValuePair<int, HashSet<int>> siblingCountyGroup in siblingCountyGroups.Where(x => x.Value.Count > 1))
            {
                List<Building2DReference>? building2DReferences = await building2DPostgreSQLConverter.GetBuilding2DReferencesByCountyIdAsync(siblingCountyGroup.Key, subdivisionId: null, lastReference: null, pageSize: 1);
                if (building2DReferences is null || building2DReferences.Count == 0)
                {
                    continue;
                }

                List<Building2D>? building2Ds = await building2DPostgreSQLConverter.GetBuilding2DsByBuilding2DReferencesAsync(building2DReferences);
                building2D = building2Ds?.FirstOrDefault(x => x.BoundingBox2D is not null && !string.IsNullOrWhiteSpace(x.Reference));
                if (building2D is not null)
                {
                    break;
                }
            }

            Assert.True(building2D is not null, "The database holds no building under any part of a multi-part county, so there is nothing for this fact to measure.");

            BoundingBox2D? boundingBox2D = building2D.BoundingBox2D;
            Assert.NotNull(boundingBox2D);

            // 300 m around the building's own outline. Whatever else the box catches, the building itself
            // is inside it, so an answer without it is the read failing rather than the area being empty.
            BoundingBox2D boundingBox2D_Search = new(boundingBox2D);
            boundingBox2D_Search.Offset(300);

            List<Building2D>? building2Ds_Found = await building2DPostgreSQLConverter.GetBuilding2DsByBoundingBox2DAsync(boundingBox2D_Search);
            Assert.NotNull(building2Ds_Found);
            Assert.NotEmpty(building2Ds_Found);

            Assert.Contains(building2Ds_Found, x => x.Reference == building2D.Reference && x.CountyId == building2D.CountyId);
        }
    }
}
