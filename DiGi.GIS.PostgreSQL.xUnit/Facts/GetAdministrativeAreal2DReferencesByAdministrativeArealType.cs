using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType, int?, bool, int, System.Threading.CancellationToken)"/>
        /// resolves sibling voivodeship part identifiers when filtering by a single parent voivodeship ID, returning all child counties.
        /// <para>Skipped by default: it executes an integration query requiring <c>GIS_PostgreSQL_Main.conf</c> pointing at a database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a database before running.")]
        public async Task GetAdministrativeAreal2DReferencesByAdministrativeArealType_ParentId_CrossPartVoivodeship_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            // Dolnośląskie (code "02") has 30 voivodeship rows (one per county part). Sibling row ID 6 is one part.
            List<AdministrativeAreal2DReference>? administrativeAreal2DReferences_AllParts = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.County, 6, false);
            Assert.NotNull(administrativeAreal2DReferences_AllParts);
            Assert.Equal(30, administrativeAreal2DReferences_AllParts.Count);

            foreach (AdministrativeAreal2DReference administrativeAreal2DReference in administrativeAreal2DReferences_AllParts)
            {
                Assert.Equal(AdministrativeArealType.County, administrativeAreal2DReference.AdministrativeArealType);
                Assert.NotNull(administrativeAreal2DReference.Code);
                Assert.StartsWith("02", administrativeAreal2DReference.Code);
            }

            // With uniqueCode = true, it should collapse to the 26 distinct county codes in Dolnośląskie.
            List<AdministrativeAreal2DReference>? administrativeAreal2DReferences_UniqueCodes = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.County, 6, true);
            Assert.NotNull(administrativeAreal2DReferences_UniqueCodes);
            Assert.Equal(26, administrativeAreal2DReferences_UniqueCodes.Count);

            foreach (AdministrativeAreal2DReference administrativeAreal2DReference in administrativeAreal2DReferences_UniqueCodes)
            {
                Assert.Equal(AdministrativeArealType.County, administrativeAreal2DReference.AdministrativeArealType);
                Assert.NotNull(administrativeAreal2DReference.Code);
                Assert.StartsWith("02", administrativeAreal2DReference.Code);
            }
        }

        /// <summary>
        /// Verifies that <see cref="AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(Npgsql.NpgsqlConnection, AdministrativeArealType, IEnumerable{int}, bool, System.Threading.CancellationToken)"/>
        /// resolves sibling part identifiers when given parent identifiers, returning all matching child references.
        /// <para>Skipped by default: it executes an integration query requiring <c>GIS_PostgreSQL_Main.conf</c> pointing at a database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a database before running.")]
        public async Task GetAdministrativeAreal2DReferencesByAdministrativeArealType_ParentIds_CrossPart_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            using Npgsql.NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(administrativeAreal2DPostgreSQLConverter.ConnectionData);
            Assert.NotNull(npgsqlConnection);
            await npgsqlConnection.OpenAsync();

            List<AdministrativeAreal2DReference>? administrativeAreal2DReferences_AllParts = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(npgsqlConnection, AdministrativeArealType.County, [6], false);
            Assert.NotNull(administrativeAreal2DReferences_AllParts);
            Assert.Equal(30, administrativeAreal2DReferences_AllParts.Count);

            List<AdministrativeAreal2DReference>? administrativeAreal2DReferences_UniqueCodes = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(npgsqlConnection, AdministrativeArealType.County, [6], true);
            Assert.NotNull(administrativeAreal2DReferences_UniqueCodes);
            Assert.Equal(26, administrativeAreal2DReferences_UniqueCodes.Count);
        }

        /// <summary>
        /// Verifies that <see cref="AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByAdministrativeArealType(AdministrativeArealType, int?, System.Threading.CancellationToken)"/>
        /// resolves sibling voivodeship part identifiers when filtering by a single parent voivodeship ID, returning all full child county entities.
        /// <para>Skipped by default: it executes an integration query requiring <c>GIS_PostgreSQL_Main.conf</c> pointing at a database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a database before running.")]
        public async Task GetAdministrativeAreal2DsByAdministrativeArealType_ParentId_CrossPartVoivodeship_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            List<AdministrativeAreal2D>? administrativeAreal2Ds = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByAdministrativeArealType(AdministrativeArealType.County, 6);
            Assert.NotNull(administrativeAreal2Ds);
            Assert.Equal(30, administrativeAreal2Ds.Count);

            foreach (AdministrativeAreal2D administrativeAreal2D in administrativeAreal2Ds)
            {
                Assert.Equal(AdministrativeArealType.County, administrativeAreal2D.AdministrativeArealType);
                Assert.NotNull(administrativeAreal2D.Code);
                Assert.StartsWith("02", administrativeAreal2D.Code);
            }
        }

        /// <summary>
        /// Verifies that <see cref="AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(Npgsql.NpgsqlConnection, AdministrativeArealType, IEnumerable{int}, bool, System.Threading.CancellationToken)"/>
        /// resolves sibling part identifiers using batched parent code lookups when given parent identifiers (such as Country level), returning all child references.
        /// <para>Skipped by default: it executes an integration query requiring <c>GIS_PostgreSQL_Main.conf</c> pointing at a database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a database before running.")]
        public async Task GetAdministrativeAreal2DReferencesByAdministrativeArealType_ParentIds_MultipleCodes_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            using Npgsql.NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(administrativeAreal2DPostgreSQLConverter.ConnectionData);
            Assert.NotNull(npgsqlConnection);
            await npgsqlConnection.OpenAsync();

            List<AdministrativeAreal2DReference>? administrativeAreal2DReferences_AllParts = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(npgsqlConnection, AdministrativeArealType.County, [7], false);
            Assert.NotNull(administrativeAreal2DReferences_AllParts);
            Assert.Equal(406, administrativeAreal2DReferences_AllParts.Count);

            List<AdministrativeAreal2DReference>? administrativeAreal2DReferences_UniqueCodes = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(npgsqlConnection, AdministrativeArealType.County, [7], true);
            Assert.NotNull(administrativeAreal2DReferences_UniqueCodes);
            Assert.Equal(380, administrativeAreal2DReferences_UniqueCodes.Count);
        }
    }
}
