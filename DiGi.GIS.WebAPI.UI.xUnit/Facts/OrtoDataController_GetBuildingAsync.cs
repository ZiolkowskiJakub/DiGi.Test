using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.WebAPI.UI.Classes;
using DiGi.GIS.WebAPI.UI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        // The status-preserving stand-in for the Orto Data direct-mode read: it answers per request path, so the
        // reference read, the years read and the year-built read can each be configured independently, and it can
        // refuse (throw the way an unreachable service does) for a single path rather than all of them.
        private sealed class RouteStubWebApi : HttpMessageHandler
        {
            private sealed class RouteAnswer
            {
                public RouteAnswer(HttpStatusCode statusCode, string? body, bool refuse)
                {
                    StatusCode = statusCode;
                    Body = body;
                    Refuse = refuse;
                }

                public HttpStatusCode StatusCode { get; }

                public string? Body { get; }

                public bool Refuse { get; }
            }

            private readonly Dictionary<string, RouteAnswer> answers = [];

            public HttpRequestMessage? LastRequest { get; private set; }

            public RouteStubWebApi Answer(string pathFragment, HttpStatusCode statusCode, string body)
            {
                answers[pathFragment] = new RouteAnswer(statusCode, body, false);
                return this;
            }

            public RouteStubWebApi Refuse(string pathFragment)
            {
                answers[pathFragment] = new RouteAnswer(HttpStatusCode.OK, null, true);
                return this;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;

                string path = request.RequestUri?.AbsolutePath ?? string.Empty;
                foreach (KeyValuePair<string, RouteAnswer> answer in answers)
                {
                    if (path.Contains(answer.Key))
                    {
                        if (answer.Value.Refuse)
                        {
                            throw new HttpRequestException("The service is unreachable.");
                        }

                        return Task.FromResult(new HttpResponseMessage(answer.Value.StatusCode) { Content = new StringContent(answer.Value.Body ?? string.Empty) });
                    }
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent(string.Empty) });
            }
        }

        private sealed class HandlerHttpClientFactory : IHttpClientFactory
        {
            private readonly HttpMessageHandler httpMessageHandler;

            public HandlerHttpClientFactory(HttpMessageHandler httpMessageHandler)
            {
                this.httpMessageHandler = httpMessageHandler;
            }

            public HttpClient CreateClient(string name)
            {
                return new HttpClient(this.httpMessageHandler);
            }
        }

        // The gated relay can only be reached with the deployment gate open. The gate is temporary code
        // (TODO [OrtoDataEndpoints]) that is removed once the upstream build deploys, so this flip goes with it.
        // TODO [OrtoDataEndpoints]: delete this helper together with Constants.Default.OrtoDataEndpointsDeployed.
        private static void OrtoDataEndpointsDeployed(bool value)
        {
            DiGi.GIS.WebAPI.UI.Constants.Default.OrtoDataEndpointsDeployed = value;
        }

        private static OrtoDataController CreateOrtoDataController(RouteStubWebApi routeStubWebApi)
        {
            OrtoDataController controller = new(new HandlerHttpClientFactory(routeStubWebApi));

            DefaultHttpContext httpContext = new();
            httpContext.Request.Headers["Cookie"] = $"{DiGi.GIS.WebAPI.UI.Constants.Default.UserTokenCookieName}=test-token";

            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            return controller;
        }

        /// <summary>
        /// Tests the status contract of the Orto Data direct-mode building read (#48): an unknown reference stays the 404 the page reads as "Building not found.", a service that answered nothing at all is a 503, and an answered fault is the status the service answered with.
        /// <para>Before the fix every one of those collapsed through <c>Building2DReferenceAsync</c> into a 404, so an outage was reported to the reviewer as a building that does not exist.</para>
        /// </summary>
        [Fact]
        public async Task GetBuildingAsync()
        {
            OrtoDataEndpointsDeployed(true);
            try
            {
                // The upstream's "no such building" is the page's 404, with the wording that names the absence.
                RouteStubWebApi routeStubWebApi_404 = new RouteStubWebApi().Answer("building2Dreferencebyreference", HttpStatusCode.NotFound, string.Empty);
                IActionResult result_404 = await CreateOrtoDataController(routeStubWebApi_404).GetBuildingAsync(1465, "unknown");
                Assert.IsType<NotFoundResult>(result_404);

                // A service that answered nothing at all is a 503, never a 404 - the reported symptom.
                IActionResult result_Unreachable = await CreateOrtoDataController(new RouteStubWebApi().Refuse("building2Dreferencebyreference")).GetBuildingAsync(1465, "3020");
                StatusCodeResult statusCodeResult_Unreachable = Assert.IsType<StatusCodeResult>(result_Unreachable);
                Assert.Equal(503, statusCodeResult_Unreachable.StatusCode);

                // An answered fault is mirrored, so the page can name it rather than read it as an absence.
                RouteStubWebApi routeStubWebApi_500 = new RouteStubWebApi().Answer("building2Dreferencebyreference", HttpStatusCode.InternalServerError, "the service failed");
                IActionResult result_500 = await CreateOrtoDataController(routeStubWebApi_500).GetBuildingAsync(1465, "3020");
                StatusCodeResult statusCodeResult_500 = Assert.IsType<StatusCodeResult>(result_500);
                Assert.Equal(500, statusCodeResult_500.StatusCode);

                // A known reference is read and composed with its photo years and any recorded answer.
                Building2DReference building2DReference = new() { Id = 123, SubdivisionId = 5, CountyId = 1465, Reference = "3020" };
                string buildingBody = Core.Convert.ToSystem_String(building2DReference) ?? string.Empty;
                RouteStubWebApi routeStubWebApi_200 = new RouteStubWebApi()
                    .Answer("building2Dreferencebyreference", HttpStatusCode.OK, buildingBody)
                    .Answer("yearsbyreference", HttpStatusCode.OK, "[1990,2005]");
                IActionResult result_200 = await CreateOrtoDataController(routeStubWebApi_200).GetBuildingAsync(1465, "3020");
                OkObjectResult okObjectResult_200 = Assert.IsType<OkObjectResult>(result_200);
                OrtoDataBuildingResponse ortoDataBuildingResponse_200 = Assert.IsType<OrtoDataBuildingResponse>(okObjectResult_200.Value);
                Assert.Equal(1465, ortoDataBuildingResponse_200.CountyId);
                Assert.Equal("3020", ortoDataBuildingResponse_200.Reference);
                Assert.NotNull(ortoDataBuildingResponse_200.Years);
                Assert.Equal(2, ortoDataBuildingResponse_200.Years.Count);
            }
            finally
            {
                OrtoDataEndpointsDeployed(false);
            }
        }

        /// <summary>
        /// Tests that the building details panel still renders without its context record when the reference read fails (#48): the refusal is not this application's error, and the panel's subject - the year built data - is untouched by it.
        /// </summary>
        [Fact]
        public async Task GetBuildingAsync_PanelWithoutReferenceRecord()
        {
            DiGi.GIS.Classes.YearBuiltData yearBuiltData = new("3020");
            string yearBuiltDataBody = Core.Convert.ToSystem_String(yearBuiltData) ?? string.Empty;

            RouteStubWebApi routeStubWebApi = new RouteStubWebApi()
                .Answer("yearbuiltdata/itemsbyreference", HttpStatusCode.OK, yearBuiltDataBody)
                .Refuse("building2Dreferencebyreference");

            YearBuiltDataController controller = new(new HandlerHttpClientFactory(routeStubWebApi));
            IActionResult result = await controller.GetItemByReferenceAsync("3020", 1465);

            Assert.IsType<PartialViewResult>(result);
        }
    }
}
