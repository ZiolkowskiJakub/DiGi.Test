using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByCodeAsync(string, AdministrativeArealType, System.Threading.CancellationToken)"/>
        /// filters target entities matching the specified code and target administrative areal type.
        /// <para>Skipped by default: it executes an integration query requiring <c>GIS_PostgreSQL_Main.conf</c> pointing at a database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a database before running.")]
        public async Task GetAdministrativeAreal2DReferencesByCode_TargetType_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            List<AdministrativeAreal2DReference>? administrativeAreal2DReferences = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByCodeAsync("2212", AdministrativeArealType.County);
            Assert.NotNull(administrativeAreal2DReferences);
            Assert.NotEmpty(administrativeAreal2DReferences);
            Assert.Equal(2, administrativeAreal2DReferences.Count);

            foreach (AdministrativeAreal2DReference administrativeAreal2DReference in administrativeAreal2DReferences)
            {
                Assert.Equal("2212", administrativeAreal2DReference.Code);
                Assert.Equal(AdministrativeArealType.County, administrativeAreal2DReference.AdministrativeArealType);
            }
        }

        /// <summary>
        /// Verifies that <see cref="AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByParentCodeAsync(string, AdministrativeArealType, System.Threading.CancellationToken)"/>
        /// retrieves child administrative areal references belonging to parents identified by the specified parent code.
        /// <para>Skipped by default: it executes an integration query requiring <c>GIS_PostgreSQL_Main.conf</c> pointing at a database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a database before running.")]
        public async Task GetAdministrativeAreal2DReferencesByParentCode_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            List<AdministrativeAreal2DReference>? administrativeAreal2DReferences_ParentCode = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByParentCodeAsync("02", AdministrativeArealType.Municipality);
            Assert.NotNull(administrativeAreal2DReferences_ParentCode);
            Assert.NotEmpty(administrativeAreal2DReferences_ParentCode);

            foreach (AdministrativeAreal2DReference administrativeAreal2DReference in administrativeAreal2DReferences_ParentCode)
            {
                Assert.Equal(AdministrativeArealType.Municipality, administrativeAreal2DReference.AdministrativeArealType);
            }
        }
    }
}
