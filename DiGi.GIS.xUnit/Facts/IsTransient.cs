using System.Net;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that the statuses worth retrying are told apart from the ones that are not.
        /// <para>A server error is deliberately not transient: repeating a request the server already failed to handle only repeats the failure, whereas 429 and 503 are the service asking for the request to come again later.</para>
        /// </summary>
        [Fact]
        public void IsTransient()
        {
            Assert.True(((HttpStatusCode)429).IsTransient());
            Assert.True(HttpStatusCode.BadGateway.IsTransient());
            Assert.True(HttpStatusCode.ServiceUnavailable.IsTransient());
            Assert.True(HttpStatusCode.GatewayTimeout.IsTransient());
            Assert.True(HttpStatusCode.RequestTimeout.IsTransient());

            Assert.False(HttpStatusCode.InternalServerError.IsTransient());
            Assert.False(HttpStatusCode.NotFound.IsTransient());
            Assert.False(HttpStatusCode.BadRequest.IsTransient());
            Assert.False(HttpStatusCode.OK.IsTransient());
        }
    }
}
