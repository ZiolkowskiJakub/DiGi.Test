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
        /// </summary>
        [Fact]
        public void Url_Elevation()
        {
            Point2D point2D = new(482430.74, 559048.76);
            string? url = point2D.Url_Elevation();

            Assert.NotNull(url);
            Assert.Contains("x=482430.74", url);
            Assert.Contains("y=559048.76", url);

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
