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
        /// Tests the status contract of the histogram relay (#40): the same five mappings as <see cref="GetUniqueValuesAsync"/> - 200 passes through, the upstream's 404 keeps the 204, an answered failure is a 502, no answer at all is a 503, a 200 with no body is a 502.
        /// <para>It also records the request the relay sends and pins its wire contract (Coding - WebAPI Contracts, section 4): a POST whose body is the declared property names the upstream binds - the body the collapsing PostJsonAsync used to send, byte for byte.</para>
        /// </summary>
        [Fact]
        public async Task GetHistogramSummaryAsync()
        {
            // The relay is intact: the 200 body passes through unchanged, in its content type.
            StubWebApi stubWebApi_200 = StubWebApi.Answer(HttpStatusCode.OK, "[{\"bucket\":1}]");
            IActionResult result_200 = await CreateTypologyController(stubWebApi_200).GetHistogramSummaryAsync("predicted_year_built", 5);
            ContentResult contentResult_200 = Assert.IsType<ContentResult>(result_200);
            Assert.Equal("[{\"bucket\":1}]", contentResult_200.Content);
            Assert.Equal("application/json", contentResult_200.ContentType);

            // The request is the wire contract: a POST of the declared names, the county scope, the fixed bucket
            // count and the equal-count bucketing - serialized as declared, not renamed on the way out.
            HttpRequestMessage? request = stubWebApi_200.LastRequest;
            Assert.NotNull(request);
            Assert.Equal(HttpMethod.Post, request!.Method);
            Assert.Equal("/gis/BuildingData/histogramsummary", request.RequestUri!.AbsolutePath);
            Assert.Equal("{\"ColumnUniqueId\":\"predicted_year_built\",\"CountyId\":5,\"BucketCount\":1000,\"HistogramBucketing\":1}", stubWebApi_200.LastRequestBody);

            // The empty contract is preserved: the upstream's 404 is the page's 204.
            IActionResult result_404 = await CreateTypologyController(StubWebApi.Answer(HttpStatusCode.NotFound, string.Empty)).GetHistogramSummaryAsync("predicted_year_built", 5);
            Assert.IsType<NoContentResult>(result_404);

            // An answered failure is a refusal the page's outcome must be able to name - the reported symptom is it collapsing into the 204 above.
            IActionResult result_500 = await CreateTypologyController(StubWebApi.Answer(HttpStatusCode.InternalServerError, "the service failed")).GetHistogramSummaryAsync("predicted_year_built", 5);
            StatusCodeResult statusCodeResult_500 = Assert.IsType<StatusCodeResult>(result_500);
            Assert.Equal(502, statusCodeResult_500.StatusCode);

            // A service that answered nothing at all is a 503, distinct from an answered refusal.
            IActionResult result_Refused = await CreateTypologyController(StubWebApi.Refuse()).GetHistogramSummaryAsync("predicted_year_built", 5);
            StatusCodeResult statusCodeResult_Refused = Assert.IsType<StatusCodeResult>(result_Refused);
            Assert.Equal(503, statusCodeResult_Refused.StatusCode);

            // A 200 with no body is a broken contract - the upstream's empty answer is the 404 - and answers the refusal.
            IActionResult result_Empty = await CreateTypologyController(StubWebApi.Answer(HttpStatusCode.OK, string.Empty)).GetHistogramSummaryAsync("predicted_year_built", 5);
            StatusCodeResult statusCodeResult_Empty = Assert.IsType<StatusCodeResult>(result_Empty);
            Assert.Equal(502, statusCodeResult_Empty.StatusCode);
        }
    }
}
