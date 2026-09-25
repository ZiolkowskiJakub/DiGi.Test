using DiGi.ComputeSharp.Spatial.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Tests for the DiGi.Geometry &lt;-&gt; ComputeSharp conversion adapters.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Coordinate3 -> Vector3D must map X, Y, Z one-to-one. Regression test for the bug where the Z
        /// component was filled from X.
        /// </summary>
        [Fact]
        public void Convert_Coordinate3_ToDiGi_Vector3D()
        {
            Coordinate3 coordinate3 = new(2, 3, 5);

            Vector3D? vector3D = Geometry.Spatial.Convert.ToDiGi_Vector3D(coordinate3);

            Assert.NotNull(vector3D);
            Assert.Equal(2.0, vector3D!.X, 1e-9);
            Assert.Equal(3.0, vector3D.Y, 1e-9);
            Assert.Equal(5.0, vector3D.Z, 1e-9);
        }

        /// <summary>
        /// Coordinate3 -> Point3D must map X, Y, Z one-to-one (control for the converter pattern).
        /// </summary>
        [Fact]
        public void Convert_Coordinate3_ToDiGi_Point3D()
        {
            Coordinate3 coordinate3 = new(2, 3, 5);

            Point3D? point3D = Geometry.Spatial.Convert.ToDiGi(coordinate3);

            Assert.NotNull(point3D);
            Assert.Equal(2.0, point3D!.X, 1e-9);
            Assert.Equal(3.0, point3D.Y, 1e-9);
            Assert.Equal(5.0, point3D.Z, 1e-9);
        }

        /// <summary>
        /// IPolygonalFace3D -> ShadowReceiver must carry the plane's origin, normal and both axes, and the bounding box of the face's 2D geometry; a null face gives null.
        /// </summary>
        [Fact]
        public void Convert_IPolygonalFace3D_ToComputeSharp_ShadowReceiver()
        {
            Assert.Null(Geometry.Spatial.Convert.ToComputeSharp((DiGi.Geometry.Spatial.Interfaces.IPolygonalFace3D?)null));

            DiGi.Geometry.Spatial.Classes.Plane plane = new(new Point3D(1, 2, 3), new Vector3D(0, 1, 0), new Vector3D(0, 0, 1));
            PolygonalFace3D? polygonalFace3D = DiGi.Geometry.Spatial.Create.PolygonalFace3D(plane, new DiGi.Geometry.Planar.Classes.Point2D(1, 2), new DiGi.Geometry.Planar.Classes.Point2D(4, 2), new DiGi.Geometry.Planar.Classes.Point2D(4, 7), new DiGi.Geometry.Planar.Classes.Point2D(1, 7));
            Assert.NotNull(polygonalFace3D);

            ShadowReceiver? shadowReceiver = Geometry.Spatial.Convert.ToComputeSharp(polygonalFace3D);
            Assert.NotNull(shadowReceiver);

            DiGi.Geometry.Spatial.Classes.Plane plane_Face = polygonalFace3D!.Plane!;
            Assert.Equal(plane_Face.Origin!.X, shadowReceiver!.Value.Origin.X, 1e-12);
            Assert.Equal(plane_Face.Origin.Z, shadowReceiver.Value.Origin.Z, 1e-12);
            Assert.Equal(plane_Face.Normal!.X, shadowReceiver.Value.Normal.X, 1e-12);
            Assert.Equal(plane_Face.AxisX!.Y, shadowReceiver.Value.AxisX.Y, 1e-12);
            Assert.Equal(plane_Face.AxisY!.Z, shadowReceiver.Value.AxisY.Z, 1e-12);
            Assert.Equal(1.0, shadowReceiver.Value.Min.X, 1e-9);
            Assert.Equal(2.0, shadowReceiver.Value.Min.Y, 1e-9);
            Assert.Equal(4.0, shadowReceiver.Value.Max.X, 1e-9);
            Assert.Equal(7.0, shadowReceiver.Value.Max.Y, 1e-9);
        }

        /// <summary>
        /// ShadowPolygon2 -> PolygonalFace2D must use the first Count points (3 or 4); any other Count, or a NaN among the used points, gives null.
        /// </summary>
        [Fact]
        public void Convert_ShadowPolygon2_ToDiGi_PolygonalFace2D()
        {
            DiGi.ComputeSharp.Planar.Classes.Coordinate2 nan = new(double.NaN, double.NaN);

            ShadowPolygon2 shadowPolygon2_Triangle = new(new(0, 0), new(2, 0), new(0, 2), nan, 0, 0, 3);
            DiGi.Geometry.Planar.Classes.PolygonalFace2D? polygonalFace2D_Triangle = Geometry.Planar.Convert.ToDiGi(shadowPolygon2_Triangle);
            Assert.NotNull(polygonalFace2D_Triangle);
            Assert.Equal(2.0, polygonalFace2D_Triangle!.GetArea(), 1e-9);

            ShadowPolygon2 shadowPolygon2_Quad = new(new(0, 0), new(2, 0), new(2, 3), new(0, 3), 0, 0, 4);
            DiGi.Geometry.Planar.Classes.PolygonalFace2D? polygonalFace2D_Quad = Geometry.Planar.Convert.ToDiGi(shadowPolygon2_Quad);
            Assert.NotNull(polygonalFace2D_Quad);
            Assert.Equal(6.0, polygonalFace2D_Quad!.GetArea(), 1e-9);

            Assert.Null(Geometry.Planar.Convert.ToDiGi(new ShadowPolygon2()));
            Assert.Null(Geometry.Planar.Convert.ToDiGi(new ShadowPolygon2(new(0, 0), new(2, 0), nan, nan, 0, 0, 3)));
        }
    }
}