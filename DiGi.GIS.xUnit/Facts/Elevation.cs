using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the Url_Elevation extension method to ensure a valid GUGiK elevation query URL is constructed.
        /// <para>The x parameter has to carry the northing and the y parameter the easting, the opposite of the order a <see cref="Point2D"/> holds - the GetHByXY service follows the official PL-1992 axis order. Sending the two the other way round does not fail loudly: outside the coverage of the terrain model the service answers zero, which parses as a valid elevation and drops the building to sea level, and a swapped pair that stays inside the country returns the elevation of a different place altogether. Both coordinates are therefore asserted in their own parameter rather than merely being present.</para>
        /// </summary>
        [Fact]
        public void Url_Elevation()
        {
            double easting = 482430.74;
            double northing = 559048.76;

            Point2D point2D = new(easting, northing);
            string? url = point2D.Url_Elevation();

            Assert.NotNull(url);
            Assert.Contains("x=559048.76", url);
            Assert.Contains("y=482430.74", url);
            Assert.EndsWith("x=559048.76&y=482430.74", url);

            Point2D? point2D_Null = null;
            Assert.Null(point2D_Null.Url_Elevation());
        }

        /// <summary>
        /// Tests the ElevationAsync and ElevationsAsync methods with null inputs to verify safe failure behavior.
        /// </summary>
        [Fact]
        public async Task ElevationAsync_NullInputs()
        {
            HttpClient? httpClient_Null = null;
            Point2D? point2D_Null = null;

            Point3D? point3D_Result = await httpClient_Null.ElevationAsync(point2D_Null);
            Assert.Null(point3D_Result);

            IEnumerable<Point2D>? points2D_Null = null;
            List<Point3D>? points3D_Result = await httpClient_Null.ElevationsAsync(points2D_Null);
            Assert.Null(points3D_Result);
        }

        /// <summary>
        /// Tests that the index aligned overload returns one entry per input point, at the same position, holding null where the elevation could not be retrieved.
        /// <para>The stub answers each request with the easting it was asked for, so asserting that every resolved point carries its own easting as its elevation proves the answers did not merely arrive in the right number but reached the point that asked for them. The overload this replaces dropped the unresolved entries, which shifted every answer after them.</para>
        /// </summary>
        [Fact]
        public async Task ElevationsAsync_IndexAlignment()
        {
            List<Point2D> point2Ds = [new(480000, 550000), new(480010, 550000), new(480020, 550000), new(480030, 550000), new(480040, 550000)];

            string url_Failing = point2Ds[2].Url_Elevation() ?? string.Empty;

            using ElevationHttpMessageHandler elevationHttpMessageHandler = new((string url, int attempt) => url == url_Failing ? Response_NotFound() : Response_Easting(url));
            using HttpClient httpClient = new(elevationHttpMessageHandler);

            List<Point3D?>? point3Ds = await httpClient.ElevationsAsync(point2Ds, 4, 2, TimeSpan.Zero);

            Assert.NotNull(point3Ds);
            Assert.Equal(point2Ds.Count, point3Ds.Count);
            Assert.Null(point3Ds[2]);

            for (int i = 0; i < point2Ds.Count; i++)
            {
                if (i == 2)
                {
                    continue;
                }

                Point3D? point3D = point3Ds[i];
                Assert.NotNull(point3D);
                Assert.Equal(point2Ds[i].X, point3D.X);
                Assert.Equal(point2Ds[i].Y, point3D.Y);
                Assert.Equal(point2Ds[i].X, point3D.Z);
            }
        }

        /// <summary>
        /// Tests that a transient failure is retried and that the point resolves once the service answers, rather than being recorded as a point that has no elevation.
        /// </summary>
        [Fact]
        public async Task ElevationsAsync_RetriesTransient()
        {
            List<Point2D> point2Ds = [new(480000, 550000)];
            string url = point2Ds[0].Url_Elevation() ?? string.Empty;

            using ElevationHttpMessageHandler elevationHttpMessageHandler = new((string requestUrl, int attempt) => attempt < 3 ? Response_ServiceUnavailable() : Response_Easting(requestUrl));
            using HttpClient httpClient = new(elevationHttpMessageHandler);

            List<Point3D?>? point3Ds = await httpClient.ElevationsAsync(point2Ds, 1, 3, TimeSpan.Zero);

            Assert.NotNull(point3Ds);
            Assert.Single(point3Ds);
            Assert.NotNull(point3Ds[0]);
            Assert.Equal(480000d, point3Ds[0]!.Z);
            Assert.Equal(3, elevationHttpMessageHandler.CountByUrl(url));
        }

        /// <summary>
        /// Tests that a failure which is not transient is not retried, so a point genuinely outside the coverage of the service costs one request rather than several.
        /// </summary>
        [Fact]
        public async Task ElevationsAsync_DoesNotRetryNonTransient()
        {
            List<Point2D> point2Ds = [new(480000, 550000)];
            string url = point2Ds[0].Url_Elevation() ?? string.Empty;

            using ElevationHttpMessageHandler elevationHttpMessageHandler = new((string requestUrl, int attempt) => Response_NotFound());
            using HttpClient httpClient = new(elevationHttpMessageHandler);

            List<Point3D?>? point3Ds = await httpClient.ElevationsAsync(point2Ds, 1, 3, TimeSpan.Zero);

            Assert.NotNull(point3Ds);
            Assert.Single(point3Ds);
            Assert.Null(point3Ds[0]);
            Assert.Equal(1, elevationHttpMessageHandler.CountByUrl(url));
        }

        /// <summary>
        /// Tests that no more requests are in flight at once than the caller allowed, which is what keeps a large run from overwhelming a public service.
        /// </summary>
        [Fact]
        public async Task ElevationsAsync_RespectsConcurrencyLimit()
        {
            List<Point2D> point2Ds = [];
            for (int i = 0; i < 40; i++)
            {
                point2Ds.Add(new Point2D(480000 + (i * 10), 550000));
            }

            using ElevationHttpMessageHandler elevationHttpMessageHandler = new((string url, int attempt) => Response_Easting(url), TimeSpan.FromMilliseconds(20));
            using HttpClient httpClient = new(elevationHttpMessageHandler);

            List<Point3D?>? point3Ds = await httpClient.ElevationsAsync(point2Ds, 4, 0, TimeSpan.Zero);

            Assert.NotNull(point3Ds);
            Assert.Equal(40, point3Ds.Count);
            Assert.DoesNotContain(point3Ds, x => x is null);
            Assert.True(elevationHttpMessageHandler.CountInFlightMax <= 4, $"Peak in flight was {elevationHttpMessageHandler.CountInFlightMax}, limit was 4.");
        }

        /// <summary>
        /// Tests that a cancelled run stops rather than working through the remaining points.
        /// </summary>
        [Fact]
        public async Task ElevationsAsync_Cancellation()
        {
            List<Point2D> point2Ds = [];
            for (int i = 0; i < 20; i++)
            {
                point2Ds.Add(new Point2D(480000 + (i * 10), 550000));
            }

            using ElevationHttpMessageHandler elevationHttpMessageHandler = new((string url, int attempt) => Response_Easting(url));
            using HttpClient httpClient = new(elevationHttpMessageHandler);

            using CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await httpClient.ElevationsAsync(point2Ds, 4, 0, TimeSpan.Zero, cancellationTokenSource.Token));
        }

        /// <summary>
        /// Tests that the collection overload that predates the aligned one still drops the points it could not resolve, so its existing callers are unaffected by the rewrite behind it.
        /// </summary>
        [Fact]
        public async Task ElevationsAsync_LegacyOverloadStillDropsNulls()
        {
            List<Point2D> point2Ds = [new(480000, 550000), new(480010, 550000), new(480020, 550000)];

            string url_Failing = point2Ds[1].Url_Elevation() ?? string.Empty;

            using ElevationHttpMessageHandler elevationHttpMessageHandler = new((string url, int attempt) => url == url_Failing ? Response_NotFound() : Response_Easting(url));
            using HttpClient httpClient = new(elevationHttpMessageHandler);

            List<Point3D>? point3Ds = await httpClient.ElevationsAsync(point2Ds, 4);

            Assert.NotNull(point3Ds);
            Assert.Equal(2, point3Ds.Count);
            Assert.Equal(480000d, point3Ds[0].X);
            Assert.Equal(480020d, point3Ds[1].X);
        }

        /// <summary>
        /// Tests that a success carrying an empty body is asked again rather than recorded as a point that has no elevation.
        /// <para>An empty two hundred is a body cut short or a success with nothing behind it, not a considered answer, and it is one of the ways single points went missing from a sampling run: nodes the service answers for perfectly well when asked a second time were absent from the store afterwards.</para>
        /// </summary>
        [Fact]
        public async Task ElevationsAsync_RetriesEmptyBody()
        {
            List<Point2D> point2Ds = [new(480000, 550000)];
            string url = point2Ds[0].Url_Elevation() ?? string.Empty;

            using ElevationHttpMessageHandler elevationHttpMessageHandler = new((string requestUrl, int attempt) => attempt < 3 ? Response_Empty() : Response_Easting(requestUrl));
            using HttpClient httpClient = new(elevationHttpMessageHandler);

            List<Point3D?>? point3Ds = await httpClient.ElevationsAsync(point2Ds, 1, 3, TimeSpan.Zero);

            Assert.NotNull(point3Ds);
            Assert.Single(point3Ds);
            Assert.NotNull(point3Ds[0]);
            Assert.Equal(480000d, point3Ds[0]!.Z);
            Assert.Equal(3, elevationHttpMessageHandler.CountByUrl(url));
        }

        /// <summary>
        /// Tests that a success carrying content which is not a number is not asked again, because a considered answer will not read differently next time.
        /// </summary>
        [Fact]
        public async Task ElevationsAsync_DoesNotRetryUnreadableBody()
        {
            List<Point2D> point2Ds = [new(480000, 550000)];
            string url = point2Ds[0].Url_Elevation() ?? string.Empty;

            using ElevationHttpMessageHandler elevationHttpMessageHandler = new((string requestUrl, int attempt) => Response_Unreadable());
            using HttpClient httpClient = new(elevationHttpMessageHandler);

            List<Point3D?>? point3Ds = await httpClient.ElevationsAsync(point2Ds, 1, 3, TimeSpan.Zero);

            Assert.NotNull(point3Ds);
            Assert.Single(point3Ds);
            Assert.Null(point3Ds[0]);
            Assert.Equal(1, elevationHttpMessageHandler.CountByUrl(url));
        }

        /// <summary>
        /// Tests that an internal server error is asked again on this path, even though the shared transient policy does not count it as one.
        /// <para>A public elevation service asked for hundreds of thousands of single points answers 500 to load and answers correctly moments later, so giving up on the first one loses a point that was never really unavailable. <see cref="DiGi.GIS.Query.IsTransient(HttpStatusCode)"/> itself is deliberately left alone, because DiGi.WebAPI keeps a copy of it in step.</para>
        /// </summary>
        [Fact]
        public async Task ElevationsAsync_RetriesInternalServerError()
        {
            List<Point2D> point2Ds = [new(480000, 550000)];
            string url = point2Ds[0].Url_Elevation() ?? string.Empty;

            Assert.False(HttpStatusCode.InternalServerError.IsTransient());

            using ElevationHttpMessageHandler elevationHttpMessageHandler = new((string requestUrl, int attempt) => attempt < 3 ? Response_InternalServerError() : Response_Easting(requestUrl));
            using HttpClient httpClient = new(elevationHttpMessageHandler);

            List<Point3D?>? point3Ds = await httpClient.ElevationsAsync(point2Ds, 1, 3, TimeSpan.Zero);

            Assert.NotNull(point3Ds);
            Assert.Single(point3Ds);
            Assert.NotNull(point3Ds[0]);
            Assert.Equal(480000d, point3Ds[0]!.Z);
            Assert.Equal(3, elevationHttpMessageHandler.CountByUrl(url));
        }

        /// <summary>
        /// Tests that a body which never becomes readable still gives up once the attempts are spent.
        /// </summary>
        [Fact]
        public async Task ElevationsAsync_EmptyBodyGivesUp()
        {
            List<Point2D> point2Ds = [new(480000, 550000)];
            string url = point2Ds[0].Url_Elevation() ?? string.Empty;

            using ElevationHttpMessageHandler elevationHttpMessageHandler = new((string requestUrl, int attempt) => Response_Empty());
            using HttpClient httpClient = new(elevationHttpMessageHandler);

            List<Point3D?>? point3Ds = await httpClient.ElevationsAsync(point2Ds, 1, 2, TimeSpan.Zero);

            Assert.NotNull(point3Ds);
            Assert.Single(point3Ds);
            Assert.Null(point3Ds[0]);
            Assert.Equal(3, elevationHttpMessageHandler.CountByUrl(url));
        }

        /// <summary>
        /// Tests that zero and negative-zero responses from the elevation service are treated as no-data sentinels and return null.
        /// </summary>
        [Fact]
        public async Task ElevationAsync_ZeroSentinelReturnsNull()
        {
            Point2D point2D = new(480000, 550000);
            string[] sentinels = ["0", "-0", "0.0", "-0.00", "0.000"];

            foreach (string sentinel in sentinels)
            {
                using ElevationHttpMessageHandler elevationHttpMessageHandler = new((string url, int attempt) => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(sentinel) });
                using HttpClient httpClient = new(elevationHttpMessageHandler);

                Point3D? point3D_Single = await httpClient.ElevationAsync(point2D);
                Assert.Null(point3D_Single);

                Point3D? point3D_Retry = await httpClient.ElevationAsync(point2D, 2, TimeSpan.Zero);
                Assert.Null(point3D_Retry);

                List<Point3D?>? point3Ds = await httpClient.ElevationsAsync([point2D], 1, 0, TimeSpan.Zero);
                Assert.NotNull(point3Ds);
                Assert.Single(point3Ds);
                Assert.Null(point3Ds[0]);
            }
        }

        /// <summary>
        /// Tests that positive and negative elevations returned by the elevation service are parsed correctly into 3D points.
        /// </summary>
        [Fact]
        public async Task ElevationAsync_ValidElevations()
        {
            Point2D point2D = new(480000, 550000);

            // Test positive elevation (e.g. standard terrain)
            {
                using ElevationHttpMessageHandler elevationHttpMessageHandler = new((string url, int attempt) => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("112.5") });
                using HttpClient httpClient = new(elevationHttpMessageHandler);

                Point3D? point3D = await httpClient.ElevationAsync(point2D);
                Assert.NotNull(point3D);
                Assert.Equal(480000d, point3D.X);
                Assert.Equal(550000d, point3D.Y);
                Assert.Equal(112.5d, point3D.Z);
            }

            // Test negative elevation (e.g. Polish depression terrain at Raczki Elbląskie)
            {
                using ElevationHttpMessageHandler elevationHttpMessageHandler = new((string url, int attempt) => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("-1.8") });
                using HttpClient httpClient = new(elevationHttpMessageHandler);

                Point3D? point3D = await httpClient.ElevationAsync(point2D);
                Assert.NotNull(point3D);
                Assert.Equal(480000d, point3D.X);
                Assert.Equal(550000d, point3D.Y);
                Assert.Equal(-1.8d, point3D.Z);
            }
        }

        /// <summary>
        /// Builds a successful response carrying nothing, which is what a body cut short looks like.
        /// </summary>
        /// <returns>A success response with an empty body.</returns>
        private static HttpResponseMessage Response_Empty()
        {
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(string.Empty) };
        }

        /// <summary>
        /// Builds a successful response carrying content that is not a number.
        /// </summary>
        /// <returns>A success response whose body cannot be read as an elevation.</returns>
        private static HttpResponseMessage Response_Unreadable()
        {
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("poza zakresem") };
        }

        /// <summary>
        /// Builds an internal server error response, which the shared transient policy does not count as worth retrying.
        /// </summary>
        /// <returns>An internal server error response.</returns>
        private static HttpResponseMessage Response_InternalServerError()
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Builds a successful response whose body is the easting the request asked for, so a fact can tell which point an answer belongs to.
        /// </summary>
        /// <param name="url">The request URL, which carries the easting in its y parameter.</param>
        /// <returns>A response holding the easting as its content.</returns>
        private static HttpResponseMessage Response_Easting(string url)
        {
            int index = url.LastIndexOf("y=", StringComparison.Ordinal);
            string easting = index < 0 ? "0" : url.Substring(index + 2);

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(easting) };
        }

        /// <summary>
        /// Builds a response for a condition that is worth retrying.
        /// </summary>
        /// <returns>A service unavailable response.</returns>
        private static HttpResponseMessage Response_ServiceUnavailable()
        {
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        }

        /// <summary>
        /// Builds a response for a condition that is not worth retrying.
        /// </summary>
        /// <returns>A not found response.</returns>
        private static HttpResponseMessage Response_NotFound()
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}
