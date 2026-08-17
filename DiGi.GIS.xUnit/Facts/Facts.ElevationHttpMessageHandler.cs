using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// A stub message handler that answers elevation requests from a script rather than from the network, so the retry, alignment and throttling behaviour can be tested without reaching the GUGiK service.
        /// <para>Counts are kept per request URL as well as in total, which is what lets a fact assert that a point was asked for exactly once, or exactly three times.</para>
        /// </summary>
        private sealed class ElevationHttpMessageHandler : HttpMessageHandler
        {
            private readonly ConcurrentDictionary<string, int> counts_ByUrl = new();
            private readonly Func<string, int, HttpResponseMessage> response;
            private readonly TimeSpan delay;
            private int count;
            private int count_InFlight;
            private int count_InFlight_Max;

            /// <summary>
            /// Initializes a new instance of the <see cref="ElevationHttpMessageHandler"/> class.
            /// </summary>
            /// <param name="response">Builds the answer from the request URL and the number of times that URL has been asked for, counting from one.</param>
            /// <param name="delay">A delay held before answering, used to keep requests overlapping long enough for the throttling to be observable.</param>
            public ElevationHttpMessageHandler(Func<string, int, HttpResponseMessage> response, TimeSpan delay = default)
            {
                this.response = response;
                this.delay = delay;
            }

            /// <summary>
            /// Gets the total number of requests the handler has answered.
            /// </summary>
            public int Count
            {
                get
                {
                    return count;
                }
            }

            /// <summary>
            /// Gets the greatest number of requests that were in flight at the same time.
            /// </summary>
            public int CountInFlightMax
            {
                get
                {
                    return count_InFlight_Max;
                }
            }

            /// <summary>
            /// Gets the number of times the specified URL has been requested.
            /// </summary>
            /// <param name="url">The request URL.</param>
            /// <returns>The number of requests made for that URL.</returns>
            public int CountByUrl(string url)
            {
                return counts_ByUrl.TryGetValue(url, out int result) ? result : 0;
            }

            /// <summary>
            /// Answers a request from the script supplied to the constructor.
            /// </summary>
            /// <param name="httpRequestMessage">The request being sent.</param>
            /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
            /// <returns>The scripted response.</returns>
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
            {
                string url = httpRequestMessage.RequestUri?.ToString() ?? string.Empty;

                int count_InFlight_Current = Interlocked.Increment(ref count_InFlight);

                int count_InFlight_Max_Observed = Volatile.Read(ref count_InFlight_Max);
                while (count_InFlight_Current > count_InFlight_Max_Observed)
                {
                    int count_InFlight_Max_Previous = Interlocked.CompareExchange(ref count_InFlight_Max, count_InFlight_Current, count_InFlight_Max_Observed);
                    if (count_InFlight_Max_Previous == count_InFlight_Max_Observed)
                    {
                        break;
                    }

                    count_InFlight_Max_Observed = count_InFlight_Max_Previous;
                }

                try
                {
                    Interlocked.Increment(ref count);
                    int attempt = counts_ByUrl.AddOrUpdate(url, 1, (string key, int value) => value + 1);

                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, cancellationToken);
                    }

                    return response(url, attempt);
                }
                finally
                {
                    Interlocked.Decrement(ref count_InFlight);
                }
            }
        }
    }
}
