using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DiGi.GIS.WebAPI.UI.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the status contract of the area-search relay (#40): a 200 answer passes the body through byte-identical, the upstream's empty answer (404) keeps the 204 the page reads as "no matches", an answered failure is a 502, and a service that answered nothing at all is a 503.
        /// <para>It also records the request the relay sends and pins its wire contract (Coding - WebAPI Contracts, section 4): a POST of the search text as a JSON string - the body the collapsing PostJsonAsync used to send, byte for byte.</para>
        /// </summary>
        [Fact]
        public async Task GetAdministrativeAreal2DReferencePathsByNameAsync()
        {
            // The relay is intact: the 200 body passes through unchanged, in its content type.
            const string pathsBody = "[{\"AdministrativeAreal2DReferences\":[]}]";
            StubWebApi stubWebApi_200 = StubWebApi.Answer(HttpStatusCode.OK, pathsBody);
            IActionResult result_200 = await new AdministrativeAreal2DController(new StubHttpClientFactory(stubWebApi_200)).GetAdministrativeAreal2DReferencePathsByNameAsync("warszawski zachodni");
            ContentResult contentResult_200 = Assert.IsType<ContentResult>(result_200);
            Assert.Equal(pathsBody, contentResult_200.Content);
            Assert.Equal("application/json", contentResult_200.ContentType);

            // The request is the wire contract: a POST of the search text as a JSON string (Coding - WebAPI Contracts, section 4).
            HttpRequestMessage? request_200 = stubWebApi_200.LastRequest;
            Assert.NotNull(request_200);
            Assert.Equal(HttpMethod.Post, request_200!.Method);
            Assert.EndsWith("/gis/administrativeareal2D/administrativeareal2Dreferencepathsbyname", request_200.RequestUri!.AbsolutePath);
            Assert.Equal("\"warszawski zachodni\"", stubWebApi_200.LastRequestBody);

            // A blank search answers the empty 200 the page already handles (the proxy's established blank-text contract).
            IActionResult result_Blank = await new AdministrativeAreal2DController(new StubHttpClientFactory(StubWebApi.Answer(HttpStatusCode.OK, "[]"))).GetAdministrativeAreal2DReferencePathsByNameAsync(string.Empty);
            Assert.IsType<OkResult>(result_Blank);

            // The upstream's empty answer (404) is the page's 204 "no matches".
            IActionResult result_404 = await new AdministrativeAreal2DController(new StubHttpClientFactory(StubWebApi.Answer(HttpStatusCode.NotFound, string.Empty))).GetAdministrativeAreal2DReferencePathsByNameAsync("warszawski zachodni");
            Assert.IsType<NoContentResult>(result_404);

            // An answered failure is a refusal the page's outcome must be able to name - the reported symptom is it collapsing into the 204 above.
            IActionResult result_500 = await new AdministrativeAreal2DController(new StubHttpClientFactory(StubWebApi.Answer(HttpStatusCode.InternalServerError, "the service failed"))).GetAdministrativeAreal2DReferencePathsByNameAsync("warszawski zachodni");
            StatusCodeResult statusCodeResult_500 = Assert.IsType<StatusCodeResult>(result_500);
            Assert.Equal(502, statusCodeResult_500.StatusCode);

            // A service that answered nothing at all is a 503, distinct from an answered refusal.
            IActionResult result_Refused = await new AdministrativeAreal2DController(new StubHttpClientFactory(StubWebApi.Refuse())).GetAdministrativeAreal2DReferencePathsByNameAsync("warszawski zachodni");
            StatusCodeResult statusCodeResult_Refused = Assert.IsType<StatusCodeResult>(result_Refused);
            Assert.Equal(503, statusCodeResult_Refused.StatusCode);
        }
    }
}
