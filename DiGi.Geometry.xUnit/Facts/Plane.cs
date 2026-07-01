namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that constructing a Plane with three null points does not throw exceptions and results in null properties.
        /// </summary>
        [Fact]
        public void Plane_ThreeNullPoints()
        {
            DiGi.Geometry.Spatial.Classes.Plane plane_Null = new((DiGi.Geometry.Spatial.Classes.Point3D?)null, (DiGi.Geometry.Spatial.Classes.Point3D?)null, (DiGi.Geometry.Spatial.Classes.Point3D?)null);

            Assert.Null(plane_Null.Origin);
            Assert.Null(plane_Null.Normal);
            Assert.Null(plane_Null.AxisY);
            Assert.True(double.IsNaN(plane_Null.A));
            Assert.True(double.IsNaN(plane_Null.B));
            Assert.True(double.IsNaN(plane_Null.C));
            Assert.True(double.IsNaN(plane_Null.D));
        }

        /// <summary>
        /// Tests that constructing a Plane with three collinear points results in a null normal and null AxisY without throwing exceptions.
        /// </summary>
        [Fact]
        public void Plane_CollinearPoints()
        {
            DiGi.Geometry.Spatial.Classes.Point3D point3D_1 = new(0.0, 0.0, 0.0);
            DiGi.Geometry.Spatial.Classes.Point3D point3D_2 = new(1.0, 1.0, 1.0);
            DiGi.Geometry.Spatial.Classes.Point3D point3D_3 = new(2.0, 2.0, 2.0);

            DiGi.Geometry.Spatial.Classes.Plane plane_Collinear = new(point3D_1, point3D_2, point3D_3);

            Assert.NotNull(plane_Collinear.Origin);
            Assert.Null(plane_Collinear.Normal);
            Assert.Null(plane_Collinear.AxisY);
            Assert.True(double.IsNaN(plane_Collinear.A));
        }

        /// <summary>
        /// Tests that copy-constructing a Plane with null properties on the source plane correctly preserves those properties as null.
        /// </summary>
        [Fact]
        public void Plane_CopyConstructorNullProperties()
        {
            DiGi.Geometry.Spatial.Classes.Plane plane_Source = new((DiGi.Geometry.Spatial.Classes.Point3D?)null, (DiGi.Geometry.Spatial.Classes.Vector3D?)null);

            Assert.Null(plane_Source.Origin);
            Assert.Null(plane_Source.Normal);
            Assert.Null(plane_Source.AxisY);

            DiGi.Geometry.Spatial.Classes.Plane plane_Copy1 = new(plane_Source);
            Assert.Null(plane_Copy1.Origin);
            Assert.Null(plane_Copy1.Normal);
            Assert.Null(plane_Copy1.AxisY);
            Assert.True(double.IsNaN(plane_Copy1.A));

            DiGi.Geometry.Spatial.Classes.Plane plane_Copy2 = new(plane_Source, null);
            Assert.Null(plane_Copy2.Origin);
            Assert.Null(plane_Copy2.Normal);
            Assert.Null(plane_Copy2.AxisY);
            Assert.True(double.IsNaN(plane_Copy2.A));
        }
    }
}