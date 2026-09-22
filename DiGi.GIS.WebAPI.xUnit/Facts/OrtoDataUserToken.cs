using DiGi.GIS.WebAPI.Classes;
using DiGi.WebAPI.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// A GIS-only host registers neither user singleton, so every user-token endpoint must answer 401 - not 500,
        /// which is what <c>[Authorize]</c> would do there (acceptance criterion 1).
        /// </summary>
        [Fact]
        public async Task RandomBuilding2DReference_Anonymous_Answers401()
        {
            using GISWebAPIConfigurationFileWatcher watcher = new(ConfigurationFilePath());
            OrtoDatasController controller = new(watcher, new PostgreSQL.Classes.OrtoDatasPostgreSQLConverter(null), new PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            Assert.IsType<UnauthorizedResult>(await controller.GetRandomBuilding2DReferenceAsync());
        }

        /// <summary>
        /// The optional <c>countyids</c> filter is additive: it reaches the same user-token gate as the unfiltered call, so an anonymous request with a filter still answers 401 and never touches the converter.
        /// </summary>
        [Fact]
        public async Task RandomBuilding2DReference_CountyIds_Anonymous_Answers401()
        {
            using GISWebAPIConfigurationFileWatcher watcher = new(ConfigurationFilePath());
            OrtoDatasController controller = new(watcher, new PostgreSQL.Classes.OrtoDatasPostgreSQLConverter(null), new PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            Assert.IsType<UnauthorizedResult>(await controller.GetRandomBuilding2DReferenceAsync(countyIds: [1, 2]));
        }

        [Fact]
        public async Task YearsByReference_Anonymous_Answers401()
        {
            using GISWebAPIConfigurationFileWatcher watcher = new(ConfigurationFilePath());
            OrtoDatasController controller = new(watcher, new PostgreSQL.Classes.OrtoDatasPostgreSQLConverter(null), new PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            Assert.IsType<UnauthorizedResult>(await controller.GetYearsByReferenceAsync("reference"));
        }

        [Fact]
        public async Task SetUserYearBuilt_Anonymous_Answers401()
        {
            using GISWebAPIConfigurationFileWatcher watcher = new(ConfigurationFilePath());
            YearBuiltDataController controller = new(watcher, new PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter(null), new PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            Classes.Parameter.UserYearBuiltParameter parameter = new() { CountyId = 1, Reference = "reference", Year = 2000 };

            Assert.IsType<UnauthorizedResult>(await controller.SetUserYearBuiltAsync(parameter));
        }

        /// <summary>
        /// Pins the wire shape of the random draw: the DiGi serializer's form (<c>_type</c>, declared property names), which is what <c>building2Dreferencebyreference</c> answers and every DiGi reader parses - and not the host's default <c>Ok(object)</c> form, which the deployed host writes through its own output formatter as camelCase with a <c>fullTypeName</c> member (<c>{"id":…,"countyId":…,"fullTypeName":…}</c>, observed live on 2026-09-22) and which <c>Core.Convert.ToDiGi</c> reads as no building at all - the UI rendered it as "nothing left to verify" (DiGi.GIS.WebAPI#39). The host formatter is not reproduced here; the fact pins the shape the action must write and that a DiGi reader round-trips it.
        /// </summary>
        [Fact]
        public void RandomBuilding2DReference_WireShape_IsDiGiSerializer()
        {
            PostgreSQL.Classes.Building2DReference building2DReference = new() { Id = 13737260, SubdivisionId = 73837, CountyId = 73485, Reference = "b5b8b7fa-33ef-4f54-b95b-81c0aa8b7626" };

            // The shape the action must write.
            string? json_DiGi = Core.Convert.ToSystem_String(building2DReference);
            Assert.NotNull(json_DiGi);
            Assert.Contains("\"_type\":\"DiGi.GIS.PostgreSQL.Classes.Building2DReference,DiGi.GIS.PostgreSQL\"", json_DiGi);
            Assert.Contains("\"CountyId\":73485", json_DiGi);
            PostgreSQL.Classes.Building2DReference? building2DReference_Read = Core.Convert.ToDiGi<PostgreSQL.Classes.Building2DReference>(json_DiGi)?.FirstOrDefault();
            Assert.NotNull(building2DReference_Read);
            Assert.Equal(building2DReference.Reference, building2DReference_Read.Reference);
            Assert.Equal(building2DReference.CountyId, building2DReference_Read.CountyId);
        }

        /// <summary>
        /// Verifies that the random draw answers 503, not 404, when the main-store converter it needs is not configured: a host that cannot reach <c>building_2d</c> / <c>year_built_data</c> is an outage, not an empty pool (DiGi.GIS.WebAPI#39).
        /// <para>The draw spans two databases; the orthophoto converter alone can never answer it, which is what filed every county as "nothing left to verify" on the deployed host.</para>
        /// </summary>
        [Fact]
        public async Task RandomBuilding2DReference_NoYearBuiltConverter_Answers503()
        {
            using GISWebAPIConfigurationFileWatcher watcher = new(ConfigurationFilePath());
            SecurityKeyManager securityKeyManager = new();
            _ = securityKeyManager.Generate();
            TokenRevocationStore tokenRevocationStore = new();
            string token = CreateToken(securityKeyManager, "reviewer@example.com");

            DefaultHttpContext httpContext = new();
            httpContext.Request.Headers.Authorization = "Bearer " + token;

            OrtoDatasController controller = new(watcher, new PostgreSQL.Classes.OrtoDatasPostgreSQLConverter(null), new PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null), securityKeyManager, tokenRevocationStore);
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            ObjectResult objectResult = Assert.IsType<ObjectResult>(await controller.GetRandomBuilding2DReferenceAsync());
            Assert.Equal(503, objectResult.StatusCode);
        }

        /// <summary>
        /// Verifies that, with every converter present but none connected, the random draw answers 404 - the pool cannot be read, which the two-database Query reports as null - rather than throwing.
        /// </summary>
        [Fact]
        public async Task RandomBuilding2DReference_UnconnectedConverters_Answers404()
        {
            using GISWebAPIConfigurationFileWatcher watcher = new(ConfigurationFilePath());
            SecurityKeyManager securityKeyManager = new();
            _ = securityKeyManager.Generate();
            TokenRevocationStore tokenRevocationStore = new();
            string token = CreateToken(securityKeyManager, "reviewer@example.com");

            DefaultHttpContext httpContext = new();
            httpContext.Request.Headers.Authorization = "Bearer " + token;

            OrtoDatasController controller = new(watcher, new PostgreSQL.Classes.OrtoDatasPostgreSQLConverter(null), new PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null), securityKeyManager, tokenRevocationStore, new PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter(null));
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            Assert.IsType<NotFoundResult>(await controller.GetRandomBuilding2DReferenceAsync(countyIds: [1]));
        }

        /// <summary>
        /// A valid user token but <c>AllowUpdateYearBuiltData=false</c>: the token check passes and the flag check
        /// denies, so the order (token before flag) is what produces the 400 rather than a 401.
        /// </summary>
        [Fact]
        public async Task SetUserYearBuilt_FlagDisabled_Answers400()
        {
            using GISWebAPIConfigurationFileWatcher watcher = new(DisabledYearBuiltFlagPath());

            SecurityKeyManager securityKeyManager = new();
            _ = securityKeyManager.Generate();
            TokenRevocationStore tokenRevocationStore = new();
            string token = CreateToken(securityKeyManager, "reviewer@example.com");

            DefaultHttpContext httpContext = new();
            httpContext.Request.Headers.Authorization = "Bearer " + token;

            YearBuiltDataController controller = new(watcher, new PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter(null), new PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null), securityKeyManager, tokenRevocationStore);
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            Classes.Parameter.UserYearBuiltParameter parameter = new() { CountyId = 1, Reference = "reference", Year = 2000 };

            Assert.IsType<BadRequestResult>(await controller.SetUserYearBuiltAsync(parameter));
        }

        /// <summary>
        /// Body validation runs after the token and flag checks pass. Each of the four members is bound nullable so an
        /// omitted value is representable and can be rejected explicitly (WebAPI Contracts §2); a missing
        /// <c>CountyId</c> is a 400, not a write recorded under a guessed part.
        /// </summary>
        [Fact]
        public async Task SetUserYearBuilt_CountyIdOmitted_Answers400()
        {
            using GISWebAPIConfigurationFileWatcher watcher = new(ConfigurationFilePath());

            SecurityKeyManager securityKeyManager = new();
            _ = securityKeyManager.Generate();
            TokenRevocationStore tokenRevocationStore = new();
            string token = CreateToken(securityKeyManager, "reviewer@example.com");

            DefaultHttpContext httpContext = new();
            httpContext.Request.Headers.Authorization = "Bearer " + token;

            YearBuiltDataController controller = new(watcher, new PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter(null), new PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null), securityKeyManager, tokenRevocationStore);
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            Classes.Parameter.UserYearBuiltParameter parameter = new() { Reference = "reference", Year = 2000 };   // CountyId omitted

            Assert.IsType<BadRequestResult>(await controller.SetUserYearBuiltAsync(parameter));
        }

        /// <summary>
        /// A blank <c>Reference</c> is a 400.
        /// </summary>
        [Fact]
        public async Task SetUserYearBuilt_ReferenceBlank_Answers400()
        {
            using GISWebAPIConfigurationFileWatcher watcher = new(ConfigurationFilePath());

            SecurityKeyManager securityKeyManager = new();
            _ = securityKeyManager.Generate();
            TokenRevocationStore tokenRevocationStore = new();
            string token = CreateToken(securityKeyManager, "reviewer@example.com");

            DefaultHttpContext httpContext = new();
            httpContext.Request.Headers.Authorization = "Bearer " + token;

            YearBuiltDataController controller = new(watcher, new PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter(null), new PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null), securityKeyManager, tokenRevocationStore);
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            Classes.Parameter.UserYearBuiltParameter parameter = new() { CountyId = 1, Reference = "  ", Year = 2000 };   // blank

            Assert.IsType<BadRequestResult>(await controller.SetUserYearBuiltAsync(parameter));
        }

        /// <summary>
        /// An omitted <c>Year</c> is a 400 - a non-nullable binding cannot tell "omitted" from a legitimate value
        /// (WebAPI Contracts §2), so it is bound <c>short?</c> and rejected.
        /// </summary>
        [Fact]
        public async Task SetUserYearBuilt_YearOmitted_Answers400()
        {
            using GISWebAPIConfigurationFileWatcher watcher = new(ConfigurationFilePath());

            SecurityKeyManager securityKeyManager = new();
            _ = securityKeyManager.Generate();
            TokenRevocationStore tokenRevocationStore = new();
            string token = CreateToken(securityKeyManager, "reviewer@example.com");

            DefaultHttpContext httpContext = new();
            httpContext.Request.Headers.Authorization = "Bearer " + token;

            YearBuiltDataController controller = new(watcher, new PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter(null), new PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null), securityKeyManager, tokenRevocationStore);
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            Classes.Parameter.UserYearBuiltParameter parameter = new() { CountyId = 1, Reference = "reference" };   // Year omitted

            Assert.IsType<BadRequestResult>(await controller.SetUserYearBuiltAsync(parameter));
        }

        /// <summary>
        /// A <c>Relation</c> that is not a <c>YearBuiltRelation</c> member is a 400. The guard validates against the
        /// enum members (0/1/2), not a non-zero sentinel, so the omitted case (<c>null</c> → <c>Exact</c>) is distinct
        /// from an out-of-range value (WebAPI Contracts §2).
        /// </summary>
        [Fact]
        public async Task SetUserYearBuilt_RelationOutOfRange_Answers400()
        {
            using GISWebAPIConfigurationFileWatcher watcher = new(ConfigurationFilePath());

            SecurityKeyManager securityKeyManager = new();
            _ = securityKeyManager.Generate();
            TokenRevocationStore tokenRevocationStore = new();
            string token = CreateToken(securityKeyManager, "reviewer@example.com");

            DefaultHttpContext httpContext = new();
            httpContext.Request.Headers.Authorization = "Bearer " + token;

            YearBuiltDataController controller = new(watcher, new PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter(null), new PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null), securityKeyManager, tokenRevocationStore);
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            Classes.Parameter.UserYearBuiltParameter parameter = new() { CountyId = 1, Reference = "reference", Year = 2000, Relation = 99 };   // not a YearBuiltRelation member

            Assert.IsType<BadRequestResult>(await controller.SetUserYearBuiltAsync(parameter));
        }

        private static string DisabledYearBuiltFlagPath()
        {
            string result = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllLines(result,
            [
                $"{nameof(GISWebAPIConfigurationFileWatcher.Open)}=true",
                $"{nameof(GISWebAPIConfigurationFileWatcher.AllowUpdateYearBuiltData)}=false",
            ]);
            return result;
        }
    }
}