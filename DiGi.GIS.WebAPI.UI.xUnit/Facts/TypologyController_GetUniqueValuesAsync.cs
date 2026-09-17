using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DiGi.GIS.WebAPI.UI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        // The upstream stand-in of the two relay Facts (#40): it records the request it received and then
        // answers with the fixed status and body, or throws the way an unreachable service does. Declared
        // here, used by TypologyController_GetHistogramSummaryAsync.cs through the partial class.
        private sealed class StubWebApi : HttpMessageHandler
        {
            private readonly HttpStatusCode statusCode;
            private readonly string body;
            private readonly Exception? exception;

            private StubWebApi(HttpStatusCode statusCode, string body, Exception? exception)
            {
                this.statusCode = statusCode;
                this.body = body;
                this.exception = exception;
            }

            public static StubWebApi Answer(HttpStatusCode statusCode, string body)
            {
                return new StubWebApi(statusCode, body, null);
            }

            public static StubWebApi Refuse()
            {
                return new StubWebApi(HttpStatusCode.OK, string.Empty, new HttpRequestException("The service is unreachable."));
            }

            /// <summary>Gets the request this stub last received.</summary>
            public HttpRequestMessage? LastRequest { get; private set; }

            /// <summary>Gets the body of the request this stub last received, read before the client disposed it.</summary>
            public string? LastRequestBody { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.LastRequest = request;
                this.LastRequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

                if (this.exception is not null)
                {
                    throw this.exception;
                }

                return new HttpResponseMessage(this.statusCode) { Content = new StringContent(this.body) };
            }
        }

        // The IHttpClientFactory the relay Facts hand the controller: every CreateClient answers a client on the stub.
        private sealed class StubHttpClientFactory : IHttpClientFactory
        {
            private readonly StubWebApi stubWebApi;

            public StubHttpClientFactory(StubWebApi stubWebApi)
            {
                this.stubWebApi = stubWebApi;
            }

            public HttpClient CreateClient(string name)
            {
                return new HttpClient(this.stubWebApi);
            }
        }

        // The relay under test, wired exactly as Program.cs wires it: one client factory, one logger.
        private static TypologyController CreateTypologyController(StubWebApi stubWebApi)
        {
            return new TypologyController(new StubHttpClientFactory(stubWebApi), NullLogger<TypologyController>.Instance);
        }

        /// <summary>
        /// Tests the status contract of the unique-values relay (#40): a 200 answer passes the body through byte-identical, the upstream's empty answer (404) keeps the 204 the page reads as "no values in scope", an answered failure becomes a 502, and a service that answered nothing at all becomes a 503.
        /// <para>Before the fix the last two answered the same 204 as the empty column - the reported symptom, an upstream failure indistinguishable from a column with no values.</para>
        /// </summary>
        [Fact]
        public async Task GetUniqueValuesAsync()
        {
            // The relay is intact: the 200 body passes through unchanged, in its content type.
            StubWebApi stubWebApi_200 = StubWebApi.Answer(HttpStatusCode.OK, "[1990,2005]");
            IActionResult result_200 = await CreateTypologyController(stubWebApi_200).GetUniqueValuesAsync("predicted_year_built", 5);
            ContentResult contentResult_200 = Assert.IsType<ContentResult>(result_200);
            Assert.Equal("[1990,2005]", contentResult_200.Content);
            Assert.Equal("application/json", contentResult_200.ContentType);

            // The empty contract is preserved: the upstream's 404 is the page's 204.
            IActionResult result_404 = await CreateTypologyController(StubWebApi.Answer(HttpStatusCode.NotFound, string.Empty)).GetUniqueValuesAsync("predicted_year_built", 5);
            Assert.IsType<NoContentResult>(result_404);

            // An answered failure is a refusal the page's outcome must be able to name - the reported symptom is it collapsing into the 204 above.
            IActionResult result_500 = await CreateTypologyController(StubWebApi.Answer(HttpStatusCode.InternalServerError, "the service failed")).GetUniqueValuesAsync("predicted_year_built", 5);
            StatusCodeResult statusCodeResult_500 = Assert.IsType<StatusCodeResult>(result_500);
            Assert.Equal(502, statusCodeResult_500.StatusCode);

            // A service that answered nothing at all is a 503, distinct from an answered refusal.
            IActionResult result_Refused = await CreateTypologyController(StubWebApi.Refuse()).GetUniqueValuesAsync("predicted_year_built", 5);
            StatusCodeResult statusCodeResult_Refused = Assert.IsType<StatusCodeResult>(result_Refused);
            Assert.Equal(503, statusCodeResult_Refused.StatusCode);

            // A 200 with no body is a broken contract - the upstream's empty answer is the 404 - and answers the refusal.
            IActionResult result_Empty = await CreateTypologyController(StubWebApi.Answer(HttpStatusCode.OK, string.Empty)).GetUniqueValuesAsync("predicted_year_built", 5);
            StatusCodeResult statusCodeResult_Empty = Assert.IsType<StatusCodeResult>(result_Empty);
            Assert.Equal(502, statusCodeResult_Empty.StatusCode);
        }
    }
}
