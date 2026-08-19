using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DByCodeAsync(string, AdministrativeArealType?, System.Threading.CancellationToken)"/>
        /// retrieves an administrative areal entity matching the specified code and optional administrative areal type.
        /// <para>Skipped by default: it executes an integration query requiring <c>GIS_PostgreSQL_Main.conf</c> pointing at a database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a database before running.")]
        public async Task GetAdministrativeAreal2DByCode_TargetType_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            AdministrativeAreal2D? administrativeAreal2D_Country = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DByCodeAsync("10", AdministrativeArealType.Country);
            Assert.NotNull(administrativeAreal2D_Country);
            Assert.Equal("10", administrativeAreal2D_Country.Code);
            Assert.Equal(AdministrativeArealType.Country, administrativeAreal2D_Country.AdministrativeArealType);

            AdministrativeAreal2D? administrativeAreal2D_Voivodeship = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DByCodeAsync("10", AdministrativeArealType.Voivodeship);
            Assert.NotNull(administrativeAreal2D_Voivodeship);
            Assert.Equal("10", administrativeAreal2D_Voivodeship.Code);
            Assert.Equal(AdministrativeArealType.Voivodeship, administrativeAreal2D_Voivodeship.AdministrativeArealType);
        }
    }
}
