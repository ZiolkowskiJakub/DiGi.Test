using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Regression guard for "System.ArgumentException: 'points must form a closed linestring'", thrown when building models loaded from the database (carrying non-finite coordinates left over from prior corrupt writes) were converted to NetTopologySuite primitives.
        /// <para>NetTopologySuite closes a ring by comparing the first and last coordinate with <c>==</c>; NaN never equals itself, so a ring that <see cref="DiGi.Geometry.Planar.Convert.ToNTS_Coordinates(Segmentable2D, bool)"/> closed correctly by re-appending its first point was still rejected and threw. <see cref="DiGi.Geometry.Planar.Convert.ToNTS(IPolygonal2D)"/> must detect non-finite coordinates itself and report failure (null) rather than let NetTopologySuite throw.</para>
        /// </summary>
        [Fact]
        public void ToNTS_LinearRing_NaNCoordinates()
        {
            // Five points: enough to bypass the 3/4 point fast paths used elsewhere and exercise the NetTopologySuite conversion.
            Polygon2D Pentagon(bool nan)
            {
                return new Polygon2D(
                [
                    nan ? new Point2D(double.NaN, double.NaN) : new Point2D(0, 0),
                    new Point2D(4, 0),
                    new Point2D(5, 3),
                    new Point2D(2, 5),
                    new Point2D(-1, 3)
                ]);
            }

            NetTopologySuite.Geometries.LinearRing? linearRing_Valid = Pentagon(false).ToNTS();
            Assert.NotNull(linearRing_Valid);
            Assert.Equal(6, linearRing_Valid.Coordinates.Length);
            Assert.Equal(linearRing_Valid.Coordinates[0].X, linearRing_Valid.Coordinates[^1].X, 9);
            Assert.Equal(linearRing_Valid.Coordinates[0].Y, linearRing_Valid.Coordinates[^1].Y, 9);

            NetTopologySuite.Geometries.LinearRing? linearRing_NaN = Pentagon(true).ToNTS();
            Assert.Null(linearRing_NaN);
        }

        /// <summary>
        /// Same non-finite coordinate guard as <see cref="ToNTS_LinearRing_NaNCoordinates"/>, exercising the other conversion branch of <see cref="DiGi.Geometry.Planar.Convert.ToNTS(IPolygonal2D)"/>, used by <see cref="IPolygonal2D"/> implementations that are not <see cref="Segmentable2D"/>.
        /// <para><see cref="Rectangle2D"/> is such a type: a NaN width propagates into a NaN point through <see cref="Rectangle2D.GetPoints"/>, without ever going through <see cref="DiGi.Geometry.Planar.Convert.ToNTS_Coordinates(Segmentable2D, bool)"/>.</para>
        /// </summary>
        [Fact]
        public void ToNTS_LinearRing_NaNCoordinates_NonSegmentable2D()
        {
            NetTopologySuite.Geometries.LinearRing? linearRing_Valid = new Rectangle2D(new Point2D(0, 0), 4, 3).ToNTS();
            Assert.NotNull(linearRing_Valid);

            NetTopologySuite.Geometries.LinearRing? linearRing_NaN = new Rectangle2D(new Point2D(0, 0), double.NaN, 3).ToNTS();
            Assert.Null(linearRing_NaN);
        }

        /// <summary>
        /// Verifies that <see cref="DiGi.Geometry.Planar.Convert.ToNTS_Polygon(IPolygonal2D)"/> propagates the same non-finite coordinate guard instead of constructing a <see cref="NetTopologySuite.Geometries.Polygon"/> around a null exterior ring.
        /// </summary>
        [Fact]
        public void ToNTS_Polygon_NaNCoordinates()
        {
            Polygon2D Pentagon(bool nan)
            {
                return new Polygon2D(
                [
                    nan ? new Point2D(double.NaN, double.NaN) : new Point2D(0, 0),
                    new Point2D(4, 0),
                    new Point2D(5, 3),
                    new Point2D(2, 5),
                    new Point2D(-1, 3)
                ]);
            }

            NetTopologySuite.Geometries.Polygon? polygon_Valid = Pentagon(false).ToNTS_Polygon();
            Assert.NotNull(polygon_Valid);
            Assert.True(polygon_Valid.IsValid);

            NetTopologySuite.Geometries.Polygon? polygon_NaN = Pentagon(true).ToNTS_Polygon();
            Assert.Null(polygon_NaN);
        }

        /// <summary>
        /// End-to-end regression at the exact reported failure site: triangulating a <see cref="Polygon2D"/> with a NaN coordinate must return null instead of throwing "points must form a closed linestring", while an otherwise identical valid polygon must still triangulate correctly.
        /// <para>Five or more points route through <see cref="Polygon2D.Triangulate"/>'s NetTopologySuite path (three and four point polygons take a fast path that never touches NetTopologySuite), matching the shape of the roof and wall faces that triggered the crash.</para>
        /// </summary>
        [Fact]
        public void Triangulate_Polygon2D_NaNCoordinate_DoesNotThrow()
        {
            Polygon2D Pentagon(bool nan)
            {
                return new Polygon2D(
                [
                    nan ? new Point2D(double.NaN, double.NaN) : new Point2D(0, 0),
                    new Point2D(4, 0),
                    new Point2D(5, 3),
                    new Point2D(2, 5),
                    new Point2D(-1, 3)
                ]);
            }

            List<Triangle2D>? triangle2Ds_Valid = Pentagon(false).Triangulate(DiGi.Core.Constants.Tolerance.Distance);
            Assert.NotNull(triangle2Ds_Valid);
            Assert.NotEmpty(triangle2Ds_Valid);

            List<Triangle2D>? triangle2Ds_NaN = Pentagon(true).Triangulate(DiGi.Core.Constants.Tolerance.Distance);
            Assert.Null(triangle2Ds_NaN);
        }

        /// <summary>
        /// Full-pipeline regression matching the reported stack trace (<see cref="PolygonalFace3D.Triangulate"/> reached through <see cref="DiGi.Geometry.Spatial.Create.Mesh3D(IPolygonalFace3D, double)"/>): a wall or roof face whose stored geometry carries a NaN coordinate must be reported as not convertible (a null mesh) instead of crashing the whole scene it belongs to.
        /// </summary>
        [Fact]
        public void Mesh3D_PolygonalFace3D_NaNCoordinate_DoesNotThrow()
        {
            Plane plane = new(new Point3D(0, 0, 0), new Spatial.Classes.Vector3D(0, 0, 1));

            PolygonalFace2D? Face(bool nan)
            {
                Polygon2D polygon2D = new(
                [
                    nan ? new Point2D(double.NaN, double.NaN) : new Point2D(0, 0),
                    new Point2D(4, 0),
                    new Point2D(5, 3),
                    new Point2D(2, 5),
                    new Point2D(-1, 3)
                ]);

                return DiGi.Geometry.Planar.Create.PolygonalFace2D(polygon2D);
            }

            PolygonalFace2D? polygonalFace2D_Valid = Face(false);
            Assert.NotNull(polygonalFace2D_Valid);

            PolygonalFace3D polygonalFace3D_Valid = new(plane, polygonalFace2D_Valid);
            Mesh3D? mesh3D_Valid = polygonalFace3D_Valid.Mesh3D(DiGi.Core.Constants.Tolerance.Distance);
            Assert.NotNull(mesh3D_Valid);
            Assert.True(mesh3D_Valid.TrianglesCount > 0);

            PolygonalFace2D? polygonalFace2D_NaN = Face(true);
            Assert.NotNull(polygonalFace2D_NaN);

            PolygonalFace3D polygonalFace3D_NaN = new(plane, polygonalFace2D_NaN);
            Mesh3D? mesh3D_NaN = polygonalFace3D_NaN.Mesh3D(DiGi.Core.Constants.Tolerance.Distance);
            Assert.Null(mesh3D_NaN);
        }
    }
}
