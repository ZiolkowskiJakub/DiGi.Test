using DiGi.WebAPI.Classes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiGi.User.WebAPI.xUnit
{
    /// <summary>
    /// Wraps an in-memory server hosting the UserController pipeline, wired through the same Modify.InitializeAsync that the deployed extension host invokes.
    /// </summary>
    internal sealed class UserWebAPIHost : IAsyncDisposable
    {
        private readonly WebApplication webApplication;

        private UserWebAPIHost(WebApplication webApplication, HttpClient httpClient)
        {
            this.webApplication = webApplication;
            HttpClient = httpClient;
        }

        /// <summary>
        /// Gets the HTTP client wired to the in-memory server.
        /// </summary>
        public HttpClient HttpClient { get; }

        /// <summary>
        /// Gets the security key manager singleton whose keys sign and validate the tokens of the in-memory server.
        /// </summary>
        public SecurityKeyManager SecurityKeyManager
        {
            get
            {
                return webApplication.Services.GetRequiredService<SecurityKeyManager>();
            }
        }

        /// <summary>
        /// Builds and starts the in-memory server, wiring authentication, revocation checking and the UserController through Modify.InitializeAsync.
        /// </summary>
        /// <returns>A started <see cref="UserWebAPIHost"/>.</returns>
        public static async Task<UserWebAPIHost> CreateAsync()
        {
            WebApplicationBuilder webApplicationBuilder = WebApplication.CreateBuilder();
            webApplicationBuilder.WebHost.UseTestServer();

            await webApplicationBuilder.Services.InitializeAsync();

            IMvcBuilder mvcBuilder = webApplicationBuilder.Services.AddControllers();
            _ = mvcBuilder.AddApplicationPart(typeof(Classes.UserController).Assembly);

            WebApplication webApplication = webApplicationBuilder.Build();
            _ = webApplication.UseAuthentication();
            _ = webApplication.UseAuthorization();
            _ = webApplication.MapControllers();
            await webApplication.StartAsync();

            HttpClient httpClient = webApplication.GetTestClient();
            return new UserWebAPIHost(webApplication, httpClient);
        }

        /// <summary>
        /// Shuts down and disposes the in-memory server and its client.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        public async ValueTask DisposeAsync()
        {
            HttpClient.Dispose();
            await webApplication.DisposeAsync();
        }
    }
}
