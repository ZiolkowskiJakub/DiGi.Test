using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DiGi.GIS.WebAPI.UI.Controllers;
using DiGi.GIS.WebAPI.UI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the status contract of the column-catalog relay (#40): a 200 answer converts to the page's view models, the upstream's empty answer (404) keeps the 204 the page reads as "no columns", an answered failure is a 502, and a service that answered nothing at all is a 503.
        /// <para>Before the fix the last three all answered the same 204 - the reported symptom, an upstream failure indistinguishable from an empty catalog.</para>
        /// </summary>
        [Fact]
        public async Task GetColumnsAsync()
        {
            // The relay is intact: the 200 body converts to the page's view models, which the page reads by slug, name and numeric eligibility.
            const string columnBody = "[{\"_type\":\"DiGi.PostgreSQL.Table.Classes.Column,DiGi.PostgreSQL.Table\",\"Category\":\"Year built\",\"Index\":2,\"Name\":\"Predicted year built\",\"UniqueId\":\"predicted_year_built\"}]";
            StubWebApi stubWebApi_200 = StubWebApi.Answer(HttpStatusCode.OK, columnBody);
            IActionResult result_200 = await CreateTypologyController(stubWebApi_200).GetColumnsAsync();
            OkObjectResult okObjectResult_200 = Assert.IsType<OkObjectResult>(result_200);
            List<TypologyColumnViewModel> viewModels_200 = Assert.IsType<List<TypologyColumnViewModel>>(okObjectResult_200.Value);
            Assert.Single(viewModels_200);
            Assert.Equal("predicted_year_built", viewModels_200[0].UniqueId);
            Assert.Equal("Predicted year built", viewModels_200[0].Name);

            // The request is the wire contract: a GET of the declared column catalog URI (Coding - WebAPI Contracts, section 4).
            HttpRequestMessage? request_200 = stubWebApi_200.LastRequest;
            Assert.NotNull(request_200);
            Assert.Equal(HttpMethod.Get, request_200!.Method);
            Assert.EndsWith("/gis/BuildingData/columns", request_200.RequestUri!.AbsolutePath);

            // The empty contract is preserved: the upstream's 404 is the page's 204.
            IActionResult result_404 = await CreateTypologyController(StubWebApi.Answer(HttpStatusCode.NotFound, string.Empty)).GetColumnsAsync();
            Assert.IsType<NoContentResult>(result_404);

            // A 200 that answers no columns is the empty catalog - the page's 204, not a refusal.
            IActionResult result_Empty = await CreateTypologyController(StubWebApi.Answer(HttpStatusCode.OK, "[]")).GetColumnsAsync();
            Assert.IsType<NoContentResult>(result_Empty);

            // An answered failure is a refusal the page's outcome must be able to name - the reported symptom is it collapsing into the 204 above.
            IActionResult result_500 = await CreateTypologyController(StubWebApi.Answer(HttpStatusCode.InternalServerError, "the service failed")).GetColumnsAsync();
            StatusCodeResult statusCodeResult_500 = Assert.IsType<StatusCodeResult>(result_500);
            Assert.Equal(502, statusCodeResult_500.StatusCode);

            // A service that answered nothing at all is a 503, distinct from an answered refusal.
            IActionResult result_Refused = await CreateTypologyController(StubWebApi.Refuse()).GetColumnsAsync();
            StatusCodeResult statusCodeResult_Refused = Assert.IsType<StatusCodeResult>(result_Refused);
            Assert.Equal(503, statusCodeResult_Refused.StatusCode);
        }
    }
}
