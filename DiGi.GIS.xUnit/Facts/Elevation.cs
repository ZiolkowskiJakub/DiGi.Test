using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using System.Collections.Generic;
using System.Net.Http;
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
    }
}
