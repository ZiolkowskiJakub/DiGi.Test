using DiGi.WebAPI.Classes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a transient failure is retried and that a later success is reported as success.
        /// <para>A bulk import used to lose an hour of work to a single 502 from the reverse proxy; the run must survive a blip that resolves on its own.</para>
        /// </summary>
        [Fact]
        public async Task PostAsync_RetriesTransientFailure()
        {
            StubHttpMessageHandler stubHttpMessageHandler = new([HttpStatusCode.BadGateway, HttpStatusCode.BadGateway, HttpStatusCode.OK]);

            using HttpClient httpClient = new(stubHttpMessageHandler);

            PostResponse postResponse = await Modify.PostAsync(httpClient, "https://localhost/updateitems", HttpContentFactory(), PostOptions());

            Assert.True(postResponse.Succeeded);
            Assert.Equal(3, stubHttpMessageHandler.RequestCount);
        }

        /// <summary>
        /// Tests that a transient failure which never resolves still throws once the attempts are exhausted.
        /// <para>Retrying must not turn a genuinely broken server into a silent success; the task should fail exactly as it did before, only later.</para>
        /// </summary>
        [Fact]
        public async Task PostAsync_ThrowsWhenRetriesExhausted()
        {
            StubHttpMessageHandler stubHttpMessageHandler = new([HttpStatusCode.BadGateway, HttpStatusCode.BadGateway, HttpStatusCode.BadGateway, HttpStatusCode.BadGateway]);

            using HttpClient httpClient = new(stubHttpMessageHandler);

            Exception exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await Modify.PostAsync(httpClient, "https://localhost/updateitems", HttpContentFactory(), PostOptions());
            });

            // RetryCount 3 means one initial attempt plus three retries.
            Assert.Equal(4, stubHttpMessageHandler.RequestCount);

            // The caller sees the underlying failure, not the internal retry signal.
            Assert.Contains("BadGateway", exception.Message);
        }

        /// <summary>
        /// Tests that a non-transient response fails on the first attempt without being retried.
        /// <para>A 400 or a 500 from a DiGi controller is a real fault the server has already logged - repeating the request only repeats the fault.</para>
        /// </summary>
        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.Unauthorized)]
        public async Task PostAsync_DoesNotRetryPermanentFailure(HttpStatusCode httpStatusCode)
        {
            StubHttpMessageHandler stubHttpMessageHandler = new([httpStatusCode, HttpStatusCode.OK]);

            using HttpClient httpClient = new(stubHttpMessageHandler);

            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await Modify.PostAsync(httpClient, "https://localhost/updateitems", HttpContentFactory(), PostOptions());
            });

            Assert.Equal(1, stubHttpMessageHandler.RequestCount);
        }

        /// <summary>
        /// Tests that every attempt sends a complete request body.
        /// <para>Sending consumes and disposes the content, so a retry that reused one instance would post an empty body and silently write nothing; the factory overload exists precisely to prevent that.</para>
        /// </summary>
        [Fact]
        public async Task PostAsync_RebuildsBodyForEveryAttempt()
        {
            const string content = "[{\"_type\":\"Test\"}]";

            StubHttpMessageHandler stubHttpMessageHandler = new([HttpStatusCode.BadGateway, HttpStatusCode.OK]);

            using HttpClient httpClient = new(stubHttpMessageHandler);

            await Modify.PostAsync(httpClient, "https://localhost/updateitems", HttpContentFactory(content), PostOptions());

            Assert.Equal(2, stubHttpMessageHandler.RequestCount);
            Assert.Equal(2, stubHttpMessageHandler.RequestBodies.Count);

            // Both attempts carried the full payload - the retry did not send an empty body.
            Assert.All(stubHttpMessageHandler.RequestBodies, x => Assert.Equal(content, x));
        }

        /// <summary>
        /// Tests that supplying a single-use <see cref="HttpContent"/> instance disables retrying rather than resending a drained body.
        /// <para>The instance overload cannot rebuild the payload, so it must fail fast instead of quietly posting nothing on the second attempt.</para>
        /// </summary>
        [Fact]
        public async Task PostAsync_InstanceContentIsNotRetried()
        {
            StubHttpMessageHandler stubHttpMessageHandler = new([HttpStatusCode.BadGateway, HttpStatusCode.OK]);

            using HttpClient httpClient = new(stubHttpMessageHandler);

            using StringContent stringContent = new("[]", Encoding.UTF8, "application/json");

            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await Modify.PostAsync(httpClient, "https://localhost/updateitems", stringContent, PostOptions());
            });

            Assert.Equal(1, stubHttpMessageHandler.RequestCount);
        }

        /// <summary>
        /// Tests the classification that decides whether a response is worth retrying.
        /// </summary>
        [Fact]
        public void IsTransient()
        {
            Assert.True(HttpStatusCode.BadGateway.IsTransient());
            Assert.True(HttpStatusCode.ServiceUnavailable.IsTransient());
            Assert.True(HttpStatusCode.GatewayTimeout.IsTransient());
            Assert.True(HttpStatusCode.RequestTimeout.IsTransient());
            Assert.True(((HttpStatusCode)429).IsTransient());

            Assert.False(HttpStatusCode.InternalServerError.IsTransient());
            Assert.False(HttpStatusCode.BadRequest.IsTransient());
            Assert.False(HttpStatusCode.Unauthorized.IsTransient());
            Assert.False(HttpStatusCode.NotFound.IsTransient());
            Assert.False(HttpStatusCode.OK.IsTransient());
        }

        /// <summary>
        /// Builds post options with a retry backoff short enough to keep the tests fast.
        /// </summary>
        /// <returns>The options used by the retry facts.</returns>
        private static PostOptions PostOptions()
        {
            return new PostOptions()
            {
                RetryCount = 3,
                RetryDelay = TimeSpan.FromMilliseconds(1),
                Delay = TimeSpan.FromSeconds(30),
            };
        }

        /// <summary>
        /// Builds a factory that produces a fresh request body for every attempt.
        /// </summary>
        /// <param name="content">The body text each attempt should carry.</param>
        /// <returns>A factory suitable for the retrying overload of PostAsync.</returns>
        private static Func<Task<HttpContent?>> HttpContentFactory(string content = "[]")
        {
            return () => Task.FromResult<HttpContent?>(new StringContent(content, Encoding.UTF8, "application/json"));
        }

        /// <summary>
        /// An HTTP handler that replays a scripted sequence of status codes and records what was sent.
        /// </summary>
        private sealed class StubHttpMessageHandler : HttpMessageHandler
        {
            private readonly List<string> requestBodies = [];
            private readonly IReadOnlyList<HttpStatusCode> httpStatusCodes;

            private int requestCount = 0;

            /// <summary>
            /// Initializes a new instance of the <see cref="StubHttpMessageHandler"/> class.
            /// </summary>
            /// <param name="httpStatusCodes">The status codes to return, one per request, in order.</param>
            public StubHttpMessageHandler(IReadOnlyList<HttpStatusCode> httpStatusCodes)
            {
                this.httpStatusCodes = httpStatusCodes;
            }

            /// <summary>
            /// Gets the body text of every request received, in order.
            /// </summary>
            public List<string> RequestBodies => requestBodies;

            /// <summary>
            /// Gets how many requests the handler received.
            /// </summary>
            public int RequestCount => requestCount;

            /// <inheritdoc />
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
            {
                if (httpRequestMessage.Content is not null)
                {
                    requestBodies.Add(await httpRequestMessage.Content.ReadAsStringAsync(cancellationToken));
                }

                HttpStatusCode httpStatusCode = requestCount < httpStatusCodes.Count ? httpStatusCodes[requestCount] : HttpStatusCode.OK;

                requestCount++;

                return new HttpResponseMessage(httpStatusCode) { Content = new StringContent(string.Empty) };
            }
        }
    }
}
