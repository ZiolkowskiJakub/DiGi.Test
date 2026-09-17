using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.WebAPI.UI.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the status contract of the county-parts relay (#40): a resolved area passes its parts through byte-identical, a country answers the empty parts, a 200 with no parts and the upstream's 404 keep the 204 the page reads as "no parts", an answered failure is a 502, and a service that answered nothing at all is a 503.
        /// <para>Before the fix the failure and no-answer cases answered the same 204 as an area with no parts - the reported symptom, a refused load indistinguishable from an area that resolves to nothing.</para>
        /// </summary>
        [Fact]
        public async Task GetCountyIdsAsync()
        {
            // A blank code is a visitor error, refused before anything is read.
            IActionResult result_Blank = await CreateTypologyController(StubWebApi.Answer(HttpStatusCode.OK, "[]")).GetCountyIdsAsync(string.Empty, AdministrativeArealType.County);
            Assert.IsType<BadRequestResult>(result_Blank);

            // An absent type is refused rather than read as Country - the binding trap of Coding - WebAPI Contracts, section 2.
            IActionResult result_NullType = await CreateTypologyController(StubWebApi.Answer(HttpStatusCode.OK, "[]")).GetCountyIdsAsync("1432", null);
            Assert.IsType<BadRequestResult>(result_NullType);

            // A country carries no parts whatever its code - the in-scope empty answer, passed through (no request is made).
            IActionResult result_Country = await CreateTypologyController(StubWebApi.Answer(HttpStatusCode.OK, "[]")).GetCountyIdsAsync("10", AdministrativeArealType.Country);
            ContentResult contentResult_Country = Assert.IsType<ContentResult>(result_Country);
            Assert.Equal("[]", contentResult_Country.Content);
            Assert.Equal("application/json", contentResult_Country.ContentType);

            // A resolved county passes its parts through byte-identical.
            StubWebApi stubWebApi_200 = StubWebApi.Answer(HttpStatusCode.OK, "[53477]");
            IActionResult result_200 = await CreateTypologyController(stubWebApi_200).GetCountyIdsAsync("1432", AdministrativeArealType.County);
            ContentResult contentResult_200 = Assert.IsType<ContentResult>(result_200);
            Assert.Equal("[53477]", contentResult_200.Content);
            Assert.Equal("application/json", contentResult_200.ContentType);

            // The request is the wire contract: the idsbycode URI, a GET of the county code and type (Coding - WebAPI Contracts, section 4).
            HttpRequestMessage? request_200 = stubWebApi_200.LastRequest;
            Assert.NotNull(request_200);
            Assert.Equal(HttpMethod.Get, request_200!.Method);
            Assert.EndsWith("/gis/administrativeareal2D/idsbycode", request_200.RequestUri!.AbsolutePath);

            // A 200 with no parts is the empty answer - the page's 204, not a refusal.
            IActionResult result_Empty = await CreateTypologyController(StubWebApi.Answer(HttpStatusCode.OK, "[]")).GetCountyIdsAsync("1432", AdministrativeArealType.County);
            Assert.IsType<NoContentResult>(result_Empty);

            // The upstream's empty answer (404) is the page's 204.
            IActionResult result_404 = await CreateTypologyController(StubWebApi.Answer(HttpStatusCode.NotFound, string.Empty)).GetCountyIdsAsync("1432", AdministrativeArealType.County);
            Assert.IsType<NoContentResult>(result_404);

            // An answered failure is a refusal the page's outcome must be able to name - the reported symptom is it collapsing into the 204 above.
            IActionResult result_500 = await CreateTypologyController(StubWebApi.Answer(HttpStatusCode.InternalServerError, "the service failed")).GetCountyIdsAsync("1432", AdministrativeArealType.County);
            StatusCodeResult statusCodeResult_500 = Assert.IsType<StatusCodeResult>(result_500);
            Assert.Equal(502, statusCodeResult_500.StatusCode);

            // A service that answered nothing at all is a 503, distinct from an answered refusal.
            IActionResult result_Refused = await CreateTypologyController(StubWebApi.Refuse()).GetCountyIdsAsync("1432", AdministrativeArealType.County);
            StatusCodeResult statusCodeResult_Refused = Assert.IsType<StatusCodeResult>(result_Refused);
            Assert.Equal(503, statusCodeResult_Refused.StatusCode);
        }
    }
}
