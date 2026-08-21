using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="AdministrativeAreal2DReferencePath"/> containing only a subset of territorial hierarchy
        /// levels stores and serializes only those levels without injecting dummy unassigned or undefined references.
        /// </summary>
        [Fact]
        public void AdministrativeAreal2DReferencePath_PartialHierarchy_NoDummyNodes()
        {
            AdministrativeAreal2DReference reference_Country = new()
            {
                Id = 1,
                Code = "10",
                Name = "Polska",
                AdministrativeArealType = AdministrativeArealType.Country,
            };

            AdministrativeAreal2DReference reference_Voivodeship = new()
            {
                Id = 2,
                Code = "30",
                Name = "WIELKOPOLSKIE",
                AdministrativeArealType = AdministrativeArealType.Voivodeship,
                CountryId = 1,
            };

            AdministrativeAreal2DReference reference_Subdivision = new()
            {
                Id = 100,
                Code = "3064011_01",
                Name = "Poznań-Stare Miasto",
                AdministrativeArealType = AdministrativeArealType.Subdivision,
                CountryId = 1,
                VoivodeshipId = 2,
            };

            List<AdministrativeAreal2DReference> administrativeAreal2DReferences = [reference_Country, reference_Voivodeship, reference_Subdivision];
            AdministrativeAreal2DReferencePath administrativeAreal2DReferencePath = new(administrativeAreal2DReferences);

            Assert.NotNull(administrativeAreal2DReferencePath[AdministrativeArealType.Country]);
            Assert.NotNull(administrativeAreal2DReferencePath[AdministrativeArealType.Voivodeship]);
            Assert.Null(administrativeAreal2DReferencePath[AdministrativeArealType.County]);
            Assert.Null(administrativeAreal2DReferencePath[AdministrativeArealType.Municipality]);
            Assert.NotNull(administrativeAreal2DReferencePath[AdministrativeArealType.Subdivision]);
            Assert.Null(administrativeAreal2DReferencePath[AdministrativeArealType.Undefined]);

            List<AdministrativeAreal2DReference> references_Result = administrativeAreal2DReferencePath.AdministrativeAreal2DReferences;
            Assert.Equal(3, references_Result.Count);

            foreach (AdministrativeAreal2DReference reference in references_Result)
            {
                Assert.NotEqual(-1, reference.Id);
                Assert.NotEqual(AdministrativeArealType.Undefined, reference.AdministrativeArealType);
            }
        }

        /// <summary>
        /// Verifies that <see cref="AdministrativeAreal2DReferencePath"/> can be cloned and serialized to and from JSON
        /// preserving only the assigned administrative areal references.
        /// </summary>
        [Fact]
        public void AdministrativeAreal2DReferencePath_Clone_And_JsonRoundTrip()
        {
            AdministrativeAreal2DReference reference_Voivodeship = new()
            {
                Id = 12,
                Code = "24",
                Name = "ŚLĄSKIE",
                AdministrativeArealType = AdministrativeArealType.Voivodeship,
            };

            AdministrativeAreal2DReference reference_County = new()
            {
                Id = 120,
                Code = "2412",
                Name = "powiat rybnicki",
                AdministrativeArealType = AdministrativeArealType.County,
                VoivodeshipId = 12,
            };

            AdministrativeAreal2DReferencePath administrativeAreal2DReferencePath = new([reference_Voivodeship, reference_County]);

            AdministrativeAreal2DReferencePath? administrativeAreal2DReferencePath_Clone = Core.Query.Clone(administrativeAreal2DReferencePath);
            Assert.NotNull(administrativeAreal2DReferencePath_Clone);
            Assert.Equal(2, administrativeAreal2DReferencePath_Clone.AdministrativeAreal2DReferences.Count);
            Assert.NotNull(administrativeAreal2DReferencePath_Clone[AdministrativeArealType.Voivodeship]);
            Assert.NotNull(administrativeAreal2DReferencePath_Clone[AdministrativeArealType.County]);
            Assert.Null(administrativeAreal2DReferencePath_Clone[AdministrativeArealType.Country]);

            JsonObject? jsonObject = Core.Convert.ToJson(administrativeAreal2DReferencePath);
            Assert.NotNull(jsonObject);

            AdministrativeAreal2DReferencePath administrativeAreal2DReferencePath_Deserialized = new(jsonObject);
            Assert.Equal(2, administrativeAreal2DReferencePath_Deserialized.AdministrativeAreal2DReferences.Count);
            Assert.Equal("2412", administrativeAreal2DReferencePath_Deserialized[AdministrativeArealType.County]?.Code);
        }

        /// <summary>
        /// Verifies that adding and removing references from <see cref="AdministrativeAreal2DReferencePath"/> works correctly.
        /// </summary>
        [Fact]
        public void AdministrativeAreal2DReferencePath_Add_Remove()
        {
            AdministrativeAreal2DReferencePath administrativeAreal2DReferencePath = new();
            Assert.Empty(administrativeAreal2DReferencePath.AdministrativeAreal2DReferences);

            Assert.False(administrativeAreal2DReferencePath.Add(null));

            AdministrativeAreal2DReference reference_County = new()
            {
                Id = 2412,
                Code = "2412",
                Name = "powiat rybnicki",
                AdministrativeArealType = AdministrativeArealType.County,
            };

            Assert.True(administrativeAreal2DReferencePath.Add(reference_County));
            Assert.Single(administrativeAreal2DReferencePath.AdministrativeAreal2DReferences);
            Assert.NotNull(administrativeAreal2DReferencePath[AdministrativeArealType.County]);

            Assert.True(administrativeAreal2DReferencePath.Remove(AdministrativeArealType.County));
            Assert.Empty(administrativeAreal2DReferencePath.AdministrativeAreal2DReferences);
            Assert.Null(administrativeAreal2DReferencePath[AdministrativeArealType.County]);
        }

        /// <summary>
        /// Verifies that <see cref="AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencePathsAsync(IEnumerable{AdministrativeAreal2DReference}, System.Threading.CancellationToken)"/>
        /// retrieves valid reference paths for a collection of references.
        /// <para>Skipped by default: it executes an integration query requiring <c>GIS_PostgreSQL_Main.conf</c> pointing at a database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a database before running.")]
        public async Task GetAdministrativeAreal2DReferencePaths_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            List<AdministrativeAreal2DReference>? administrativeAreal2DReferences = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByCodeAsync("2212", AdministrativeArealType.County);
            Assert.NotNull(administrativeAreal2DReferences);
            Assert.NotEmpty(administrativeAreal2DReferences);

            List<AdministrativeAreal2DReferencePath>? referencePaths = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencePathsAsync(administrativeAreal2DReferences);
            Assert.NotNull(referencePaths);
            Assert.Equal(administrativeAreal2DReferences.Count, referencePaths.Count);

            foreach (AdministrativeAreal2DReferencePath path in referencePaths)
            {
                foreach (AdministrativeAreal2DReference reference in path.AdministrativeAreal2DReferences)
                {
                    Assert.NotEqual(-1, reference.Id);
                    Assert.NotEqual(AdministrativeArealType.Undefined, reference.AdministrativeArealType);
                }
            }
        }
    }
}
