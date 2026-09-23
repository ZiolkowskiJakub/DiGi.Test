using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// A stand-in for the GIS Web API that answers every request through a script and records what it was sent - the method, the URI and the body - so a fact can drive a multi-request client (a paged read, a relay making several calls) and assert both halves of the conversation.
        /// <para>The single-answer <see cref="StubWebApi"/> covers relays that make one call; this one covers the rest.</para>
        /// </summary>
        private sealed class ScriptedWebApi : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, string?, HttpResponseMessage> respond;

            /// <summary>
            /// Initializes a new instance of the <see cref="ScriptedWebApi"/> class.
            /// </summary>
            /// <param name="respond">The script: answers a request, given the request and its body as text.</param>
            public ScriptedWebApi(Func<HttpRequestMessage, string?, HttpResponseMessage> respond)
            {
                this.respond = respond;
            }

            /// <summary>
            /// Gets every request received, in order, with its body as text.
            /// </summary>
            public List<(HttpMethod Method, Uri? RequestUri, string? Body)> Requests { get; } = [];

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                string? body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
                this.Requests.Add((request.Method, request.RequestUri, body));
                return this.respond(request, body);
            }
        }
    }
}
