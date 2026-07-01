namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the InRange and Inside methods of the Ellipse3D class to verify that they correctly evaluate points inside, on the boundary, outside, and off the plane.
        /// </summary>
        [Fact]
        public void Ellipse3D_InsideAndInRange()
        {
            DiGi.Geometry.Spatial.Classes.Plane plane_XY = new(new DiGi.Geometry.Spatial.Classes.Point3D(0.0, 0.0, 0.0), new DiGi.Geometry.Spatial.Classes.Vector3D(0.0, 0.0, 1.0));
            DiGi.Geometry.Planar.Classes.Ellipse2D ellipse2D_Base = new(new DiGi.Geometry.Planar.Classes.Point2D(0.0, 0.0), 5.0, 10.0);
            DiGi.Geometry.Spatial.Classes.Ellipse3D ellipse3D_Main = new(plane_XY, ellipse2D_Base);

            DiGi.Geometry.Spatial.Classes.Point3D point3D_Inside = new(1.0, 2.0, 0.0);
            DiGi.Geometry.Spatial.Classes.Point3D point3D_Outside = new(6.0, 0.0, 0.0);
            DiGi.Geometry.Spatial.Classes.Point3D point3D_OffPlane = new(1.0, 2.0, 1.0);

            // Test points inside
            Assert.True(ellipse3D_Main.Inside(point3D_Inside));
            Assert.True(ellipse3D_Main.InRange(point3D_Inside));

            // Test points outside
            Assert.False(ellipse3D_Main.Inside(point3D_Outside));
            Assert.False(ellipse3D_Main.InRange(point3D_Outside));

            // Test points off the plane
            Assert.False(ellipse3D_Main.Inside(point3D_OffPlane));
            Assert.False(ellipse3D_Main.InRange(point3D_OffPlane));

            // Test null inputs
            Assert.False(ellipse3D_Main.Inside(null));
            Assert.False(ellipse3D_Main.InRange(null));
        }
    }
}