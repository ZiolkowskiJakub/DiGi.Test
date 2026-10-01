using DiGi.GIS.WebAPI.Classes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Drives a real request through the host's MVC pipeline and asserts that an omitted mandatory query parameter answers HTTP 400 naming it, rather than silently binding <c>default(T)</c>.
        /// <para><c>[BindRequired]</c> is enforced by <c>ParameterBinder</c>, not by the attribute alone: Swashbuckle reads the attribute for the document, but only a request proves the runtime half. A non-nullable value type is the case that used to succeed with the wrong scope - a county id of 0 or a coordinate of 0 - so both an <c>int</c> and a <c>double</c> are asserted. The action is never reached: model state is invalidated before it runs, which is why the converters registered here are never used (ZiolkowskiJakub/DiGi.GIS.WebAPI#43).</para>
        /// </summary>
        [Fact]
        public async Task GisControllers_OmittedMandatoryQueryParameter_Answers400()
        {
            using IHost host = await new HostBuilder()
                .ConfigureWebHost(webHostBuilder =>
                {
                    webHostBuilder.UseTestServer();
                    webHostBuilder.ConfigureServices(serviceCollection =>
                    {
                        serviceCollection.AddControllers().AddApplicationPart(typeof(TerrainController).Assembly);
                        serviceCollection.AddSingleton(new PostgreSQL.Classes.TerrainPointPostgreSQLConverter(new DiGi.PostgreSQL.Classes.ConnectionData("127.0.0.1", "user", "pass", "db", 1)));
                        serviceCollection.AddSingleton(new PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter(null));
                    });
                    webHostBuilder.Configure(webApplication =>
                    {
                        webApplication.UseRouting();
                        webApplication.UseEndpoints(endpoints => endpoints.MapControllers());
                    });
                })
                .StartAsync();

            HttpClient httpClient = host.GetTestClient();

            await AssertBindRequired400(httpClient, "/gis/terrain/countbycountyid", "countyid");
            await AssertBindRequired400(httpClient, "/gis/terrain/mesh3dbycircle?y=1", "x");
        }

        private static async Task AssertBindRequired400(HttpClient httpClient, string requestUri, string parameterName)
        {
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(requestUri);
            string body = await httpResponseMessage.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, httpResponseMessage.StatusCode);
            Assert.Contains(parameterName, body, StringComparison.OrdinalIgnoreCase);
        }
    }
}
