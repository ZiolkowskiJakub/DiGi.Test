using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the level-by-level bounding box search steps over an administrative level that is missing from the source data
        /// instead of stopping at it.
        /// <para>m. Poznan (<c>3064</c>) holds no <c>gmina</c> feature in BDOT10k, so the Municipality level answers nothing anywhere
        /// inside the city. While that ended the search, a box over the middle of Poznan returned no subdivision at all - and, through
        /// <see cref="Building2DPostgreSQLConverter"/>, no building either, in a city holding 82 075 of them. A box over Krakow, whose
        /// gmina row does exist, always worked. See https://github.com/ZiolkowskiJakub/DiGi.GIS.PostgreSQL/issues/15.</para>
        /// <para>Municipality is still expected to come back empty for Poznan - there genuinely is no gmina there - so only the
        /// Subdivision level is asserted.</para>
        /// <para>Skipped by default: it queries a populated database, and the subdivisions only carry a county after a refresh run.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a refreshed database before running.")]
        public async Task GetAdministrativeAreal2DsByBoundingBox2D_MissingLevel_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            // An 800 m box over Jezyce, well inside Poznan, and one over Bienczyce in Krakow as the control.
            BoundingBox2D boundingBox2D_Poznan = new(new Point2D(356763, 507289), new Point2D(357563, 508089));
            BoundingBox2D boundingBox2D_Krakow = new(new Point2D(573089, 246710), new Point2D(573889, 247510));

            List<AdministrativeArealType> administrativeArealTypes = [AdministrativeArealType.Subdivision];

            List<AdministrativeAreal2D>? administrativeAreal2Ds_Krakow = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByBoundingBox2DAsync(boundingBox2D_Krakow, administrativeArealTypes);
            Assert.NotNull(administrativeAreal2Ds_Krakow);
            Assert.NotEmpty(administrativeAreal2Ds_Krakow);

            List<AdministrativeAreal2D>? administrativeAreal2Ds_Poznan = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByBoundingBox2DAsync(boundingBox2D_Poznan, administrativeArealTypes);
            Assert.NotNull(administrativeAreal2Ds_Poznan);
            Assert.NotEmpty(administrativeAreal2Ds_Poznan);

            foreach (AdministrativeAreal2D administrativeAreal2D in administrativeAreal2Ds_Poznan)
            {
                Assert.Equal(AdministrativeArealType.Subdivision, administrativeAreal2D.AdministrativeArealType);

                // Reached through the county, because there is no municipality to reach them through.
                Assert.NotNull(administrativeAreal2D.CountyId);
                Assert.StartsWith("3064", administrativeAreal2D.Code);
            }
        }
    }
}
