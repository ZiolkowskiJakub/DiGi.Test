using DiGi.BDL.Classes;
using DiGi.GIS.Classes;
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
        /// Verifies that a populated <see cref="UnitComplianceResult"/> survives a JSON round trip and a clone.
        /// </summary>
        [Fact]
        public void UnitComplianceResult_Serialization()
        {
            List<AdministrativeAreal2DReference> unmatched =
            [
                new AdministrativeAreal2DReference
                {
                    Id = 101,
                    Code = "020101",
                    Name = "Gmina Test",
                    AdministrativeArealType = AdministrativeArealType.Municipality,
                    CountryId = 1,
                    VoivodeshipId = 2,
                    CountyId = 3
                }
            ];

            UnitComplianceResult complianceResult = new(AdministrativeArealType.Municipality, 2477, 2476, unmatched);

            Assert.Equal(AdministrativeArealType.Municipality, complianceResult.AdministrativeArealType);
            Assert.Equal(2477, complianceResult.TotalCount);
            Assert.Equal(2476, complianceResult.MatchedCount);
            Assert.Equal(1, complianceResult.UnmatchedCount);
            Assert.True(complianceResult.ComplianceRate > 0.99);
            Assert.NotNull(complianceResult.UnmatchedReferences);
            Assert.Single(complianceResult.UnmatchedReferences);

            string? text = Core.Convert.ToSystem_String(complianceResult);
            Assert.False(string.IsNullOrWhiteSpace(text));

            UnitComplianceResult? parsed = Core.Convert.ToDiGi<UnitComplianceResult>(text)?.FirstOrDefault();
            Assert.NotNull(parsed);
            Assert.Equal(AdministrativeArealType.Municipality, parsed.AdministrativeArealType);
            Assert.Equal(2477, parsed.TotalCount);
            Assert.Equal(2476, parsed.MatchedCount);
            Assert.Equal(1, parsed.UnmatchedCount);

            UnitComplianceResult clone = new(complianceResult);
            Assert.Equal(AdministrativeArealType.Municipality, clone.AdministrativeArealType);
            Assert.Equal(2477, clone.TotalCount);
            Assert.Equal(2476, clone.MatchedCount);

            Core.xUnit.Query.SerializationCheck(complianceResult);
        }

        /// <summary>
        /// Verifies that <see cref="PostgreSQLUnitPopulateOptions"/> property assignment and copy constructor work correctly.
        /// </summary>
        [Fact]
        public void PostgreSQLUnitPopulateOptions_RoundTrip()
        {
            PostgreSQLUnitPopulateOptions options = new()
            {
                PageSize = 250,
                Clear = true,
                BatchSize = 500
            };

            Assert.Equal(250, options.PageSize);
            Assert.True(options.Clear);
            Assert.Equal(500, options.BatchSize);

            PostgreSQLUnitPopulateOptions clone = new(options);
            Assert.Equal(250, clone.PageSize);
            Assert.True(clone.Clear);
            Assert.Equal(500, clone.BatchSize);

            Core.xUnit.Query.SerializationCheck(options);
        }

        /// <summary>
        /// Verifies null checks and safety guards on <see cref="UnitPostgreSQLConverter"/> methods.
        /// </summary>
        [Fact]
        public async Task UnitPostgreSQLConverter_NullGuards()
        {
            UnitPostgreSQLConverter converter = new(null);

            Assert.Equal("unit", UnitPostgreSQLConverter.TableName);
            Assert.False(await converter.CreateTableAsync());
            Assert.False(await converter.ClearAsync());
            Assert.Empty(await converter.InsertAsync(null));
            Assert.Empty(await converter.InsertAsync([]));
            Assert.Null(await converter.GetUnitsAsync());
            Assert.Null(await converter.GetUnitByIdAsync(null));
            Assert.Null(await converter.GetUnitByIdAsync(""));
            Assert.Null(await converter.GetUnitsByIdsAsync(null));
            Assert.Null(await converter.GetUnitsByNameAsync(null));
            Assert.Null(await converter.GetCountsByLevelAsync());
            Assert.Null(await converter.GetStatisticalUnitAsync());
            Assert.Null(await converter.GetStatisticalUnitAsync((AdministrativeAreal2DReference?)null));
            Assert.Null(await converter.GetComplianceAsync(null, AdministrativeArealType.County));
        }

        /// <summary>
        /// Verifies that <see cref="UnitPostgreSQLConverter.GetStatisticalUnitAsync(Npgsql.NpgsqlConnection?, int, System.Threading.CancellationToken)"/> builds the correct hierarchy from flat <see cref="Unit"/> entities.
        /// </summary>
        [Fact]
        public void UnitPostgreSQLConverter_HierarchyConstruction()
        {
            List<Unit> units =
            [
                new Unit { id = "000000000000", name = "POLSKA", level = 0 },
                new Unit { id = "030000000000", name = "MAKROREGION POŁUDNIOWO-ZACHODNI", level = 1 },
                new Unit { id = "030200000000", name = "DOLNOŚLĄSKIE", level = 2 },
                new Unit { id = "030210000000", name = "REGION DOLNOŚLĄSKIE", level = 3 },
                new Unit { id = "030210100000", name = "PODREGION JELENIOGÓRSKI", level = 4 },
                new Unit { id = "030210101000", name = "POWIAT BOLESŁAWIECKI", level = 5 },
                new Unit { id = "030210101011", name = "BOLESŁAWIEC", level = 6 }
            ];

            StatisticalUnit? root = GIS.Create.StatisticalUnit(units);
            Assert.NotNull(root);
            Assert.Equal("POLSKA", root.Name);

            // Test voivodeship matching
            StatisticalUnit? matchedVoivodeship = root.Match("DOLNOŚLĄSKIE", "02", AdministrativeArealType.Voivodeship);
            Assert.NotNull(matchedVoivodeship);
            Assert.Equal("DOLNOŚLĄSKIE", matchedVoivodeship.Name);

            // Test county matching
            StatisticalUnit? matchedCounty = root.Match("BOLESŁAWIECKI", "0201", AdministrativeArealType.County);
            Assert.NotNull(matchedCounty);
            Assert.Equal("POWIAT BOLESŁAWIECKI", matchedCounty.Name);

            // Test municipality matching
            StatisticalUnit? matchedMunicipality = root.Match("BOLESŁAWIEC (GM. MIEJSKA)", "0201011", AdministrativeArealType.Municipality);
            Assert.NotNull(matchedMunicipality);
            Assert.Equal("BOLESŁAWIEC", matchedMunicipality.Name);

            // Test reference matching
            AdministrativeAreal2DReference reference = new()
            {
                Id = 1,
                Code = "0201",
                Name = "BOLESŁAWIECKI",
                AdministrativeArealType = AdministrativeArealType.County
            };
            StatisticalUnit? matchedFromRef = root.Match(reference);
            Assert.NotNull(matchedFromRef);
            Assert.Equal("POWIAT BOLESŁAWIECKI", matchedFromRef.Name);
        }

        /// <summary>
        /// Verifies that compliance calculation returns null when either converter is missing.
        /// </summary>
        [Fact]
        public async Task UnitComplianceResultAsync_MissingConverter_ReturnsNull()
        {
            UnitPostgreSQLConverter unitConverter = new(null);
            AdministrativeAreal2DPostgreSQLConverter adminConverter = new(null);

            Assert.Null(await Create.UnitComplianceResultAsync(null, adminConverter, AdministrativeArealType.County));
            Assert.Null(await Create.UnitComplianceResultAsync(unitConverter, null, AdministrativeArealType.County));
            Assert.Null(await Create.UnitComplianceResultAsync(unitConverter, adminConverter, AdministrativeArealType.Undefined));
        }
    }
}
