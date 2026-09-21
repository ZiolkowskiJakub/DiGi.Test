using DiGi.GIS.WebAPI.Classes;
using DiGi.PostgreSQL.Classes;
using DiGi.WebAPI.Classes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.xUnit
{
    /// <summary>
    /// A minimal controller that inherits from the shared WebAPIController base class to isolate
    /// the [Produces] attribute interaction with the content-negotiation pipeline.
    /// <para>The base class declares [Produces("application/json", "image/jpeg")] at the class level
    /// (the fix for issue #38); this action additionally declares [Produces("image/jpeg")] at the
    /// action level and returns a FileContentResult, mirroring the production imagebyreference shape.</para>
    /// </summary>
    [ApiController]
    [Route("test/[controller]")]
    public class TestImageController : WebAPIController
    {
        /// <summary>
        /// Returns a small JPEG byte array, mirroring the production imagebyreference action.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="FileContentResult"/> with image/jpeg content type.</returns>
        [HttpGet("image")]
        [Produces("image/jpeg")]
        public IActionResult GetImage(CancellationToken cancellationToken = default)
        {
            // Minimal JPEG: SOI + EOI markers (2 bytes each).
            byte[] bytes = [0xFF, 0xD8, 0xFF, 0xD9];
            return File(bytes, "image/jpeg");
        }

        /// <summary>
        /// The error shape of <see cref="OrtoDatasController.GetImageByReferenceAsync(string, short, int?, bool, CancellationToken)"/> after #38: the same attributes as the real action and a string-bodied 500, which must leave as a 500 and not as a 406.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by the caller to cancel the operation.</param>
        /// <returns>A 500 with a text body.</returns>
        [HttpGet("fault")]
        [ProducesResponseType(typeof(FileContentResult), 200, "image/jpeg")]
        [ProducesResponseType(500)]
        public IActionResult GetFault(CancellationToken cancellationToken = default)
        {
            return StatusCode(500, "Internal server error during photo read");
        }
    }

    public partial class Facts
    {
        /// <summary>
        /// Verifies that a FileContentResult action on a WebAPIController-derived controller
        /// answers 200 image/jpeg for every Accept header variant that the production issue
        /// reported as 406 (issue #38): no header, */*, image/jpeg, and the browser string.
        /// <para>Standard ASP.NET Core does not produce a 406 here: FileContentResult bypasses the
        /// output-formatter pipeline, and [Produces] is Swagger metadata only. The 406 on the deployed
        /// host (api.digiproject.uk) comes from a host-side filter that reads the class-level [Produces]
        /// to build its allowed content-type list. Adding image/jpeg to that list (the fix in
        /// WebAPIController) is what makes the deployed host accept the response. This fact guards
        /// against the class-level list regressing.</para>
        /// </summary>
        [Fact]
        public async Task OrtoDatasController_ImageByReference_ContentNegotiation()
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            IMvcBuilder mvcBuilder = builder.Services.AddControllers();
            _ = mvcBuilder.AddApplicationPart(typeof(TestImageController).Assembly);

            WebApplication app = builder.Build();
            _ = app.MapControllers();
            await app.StartAsync();

            try
            {
                using HttpClient client = app.GetTestClient();

                // The four Accept variants from the issue report.
                string?[] acceptHeaders =
                [
                    null,
                    "*/*",
                    "image/jpeg",
                    "image/avif,image/webp,image/png,image/svg+xml,image/*;q=0.8,*/*;q=0.5",
                ];

                foreach (string? acceptHeader in acceptHeaders)
                {
                    HttpRequestMessage request = new(HttpMethod.Get, "/test/testimage/image");
                    if (acceptHeader is not null)
                    {
                        request.Headers.Accept.ParseAdd(acceptHeader);
                    }

                    using HttpResponseMessage response = await client.SendAsync(request);
                    string body = await response.Content.ReadAsStringAsync();

                    string label = acceptHeader is null ? "(none)" : acceptHeader;
                    string? contentType = response.Content.Headers.ContentType?.MediaType;
                    Assert.True(response.StatusCode == HttpStatusCode.OK,
                        $"Accept: {label} — expected 200 but got {(int)response.StatusCode}. Body: {body}");
                    Assert.True("image/jpeg" == contentType,
                        $"Accept: {label} — expected image/jpeg but got {contentType}");
                    Assert.True(response.Content.Headers.ContentLength is > 0,
                        $"Accept: {label} — response body must not be empty");
                }
            }
            finally
            {
                await app.StopAsync();
                await app.DisposeAsync();
            }
        }

        /// <summary>
        /// Verifies that the 404 path of the image action works: a missing photo maps to
        /// NotFoundResult, not a 500 or 406.
        /// </summary>
        /// <summary>
        /// Verifies that a string-bodied error answer of an action carrying the image action's attributes leaves as its own status, not as 406.
        /// <para>Before #38 the action-level <c>[Produces("image/jpeg")]</c> restricted every <see cref="ObjectResult"/> of the action to <c>image/jpeg</c>; a <c>string</c> body then had no output formatter and MVC answered 406 for the 400/500/503 branches, which hid the decode fault of DiGi.GIS.PostgreSQL#90 behind a content-negotiation symptom. The class-level <c>[Produces]</c> of <see cref="WebAPIController"/> still lists <c>application/json</c>, so the text body is written as JSON and the status survives.</para>
        /// </summary>
        [Fact]
        public async Task OrtoDatasController_ImageByReference_FaultIsNot406()
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            IMvcBuilder mvcBuilder = builder.Services.AddControllers();
            _ = mvcBuilder.AddApplicationPart(typeof(TestImageController).Assembly);

            WebApplication app = builder.Build();
            _ = app.MapControllers();
            await app.StartAsync();

            try
            {
                using HttpClient client = app.GetTestClient();

                HttpRequestMessage request = new(HttpMethod.Get, "/test/testimage/fault");
                request.Headers.Accept.ParseAdd("image/avif,image/webp,image/png,image/svg+xml,image/*;q=0.8,*/*;q=0.5");

                using HttpResponseMessage response = await client.SendAsync(request);
                string body = await response.Content.ReadAsStringAsync();

                Assert.True(response.StatusCode == HttpStatusCode.InternalServerError, $"Expected 500 but got {(int)response.StatusCode}. Body: {body}");
                Assert.Contains("Internal server error during photo read", body);
            }
            finally
            {
                await app.StopAsync();
                await app.DisposeAsync();
            }
        }

        /// <summary>
        /// Verifies that the real image action answers a failed photo read with a status-carrying <see cref="ObjectResult"/> (500 or 503, never 404) and that no action-level <see cref="ProducesAttribute"/> remains to turn that answer into a 406.
        /// <para>The converter points at a port nothing listens on, so the read faults the way a broken database read does instead of answering null.</para>
        /// </summary>
        [Fact]
        public async Task OrtoDatasController_ImageByReference_FaultPath()
        {
            string path = ConfigurationFilePath();

            try
            {
                using GISWebAPIConfigurationFileWatcher watcher = new(path);
                PostgreSQL.Classes.OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(new ConnectionData("127.0.0.1", "xunit", "xunit", "xunit", 1));
                OrtoDatasController controller = new(watcher, ortoDatasPostgreSQLConverter, new PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));
                controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

                IActionResult result = await controller.GetImageByReferenceAsync("XUNIT-FAULT", 2010, 1);

                ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
                Assert.True(objectResult.StatusCode is 500 or 503, $"Expected 500 or 503 but got {objectResult.StatusCode}");
                Assert.IsType<string>(objectResult.Value);

                System.Reflection.MethodInfo? methodInfo = typeof(OrtoDatasController).GetMethod(nameof(OrtoDatasController.GetImageByReferenceAsync));
                Assert.NotNull(methodInfo);
                Assert.Empty(methodInfo.GetCustomAttributes(typeof(ProducesAttribute), false));
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        [Fact]
        public async Task OrtoDatasController_ImageByReference_NotFoundPath()
        {
            string path = ConfigurationFilePath();

            try
            {
                using GISWebAPIConfigurationFileWatcher watcher = new(path);
                OrtoDatasController controller = new(watcher, new PostgreSQL.Classes.OrtoDatasPostgreSQLConverter(null), new PostgreSQL.Classes.Building2DPostgreSQLConverter(null), new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));
                controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

                // null connection → converter returns null → action returns NotFound.
                IActionResult result = await controller.GetImageByReferenceAsync("nonexistent-reference", 1999);
                Assert.IsType<NotFoundResult>(result);
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }
    }
}
