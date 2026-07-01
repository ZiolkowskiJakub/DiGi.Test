namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the projection, distance, and boundary-on functionalities of a Ray3D instance.
        /// </summary>
        [Fact]
        public void Ray3D_Project()
        {
            DiGi.Geometry.Spatial.Classes.Point3D point3D_Origin = new(0.0, 0.0, 0.0);
            DiGi.Geometry.Spatial.Classes.Vector3D vector3D_Direction = new(0.0, 0.0, 1.0);
            DiGi.Geometry.Spatial.Classes.Ray3D ray3D_Test = new(point3D_Origin, vector3D_Direction);

            // Test projection of point in front of the origin
            DiGi.Geometry.Spatial.Classes.Point3D? point3D_ProjFront = ray3D_Test.Project(new DiGi.Geometry.Spatial.Classes.Point3D(3.0, 4.0, 10.0));
            Assert.NotNull(point3D_ProjFront);
            if (point3D_ProjFront is not null)
            {
                Assert.Equal(point3D_ProjFront, new DiGi.Geometry.Spatial.Classes.Point3D(0.0, 0.0, 10.0));
            }

            // Test projection of point behind the origin (should clamp to origin)
            DiGi.Geometry.Spatial.Classes.Point3D? point3D_ProjBehind = ray3D_Test.Project(new DiGi.Geometry.Spatial.Classes.Point3D(3.0, 4.0, -10.0));
            Assert.NotNull(point3D_ProjBehind);
            if (point3D_ProjBehind is not null)
            {
                Assert.Equal(point3D_ProjBehind, new DiGi.Geometry.Spatial.Classes.Point3D(0.0, 0.0, 0.0));
            }

            // Test Distance method for point in front
            double double_DistFront = ray3D_Test.Distance(new DiGi.Geometry.Spatial.Classes.Point3D(3.0, 4.0, 10.0));
            // Distance from (3, 4, 10) to projected point (0, 0, 10) is sqrt(3^2 + 4^2) = 5
            Assert.Equal(5.0, double_DistFront, 9);

            // Test Distance method for point behind origin
            double double_DistBehind = ray3D_Test.Distance(new DiGi.Geometry.Spatial.Classes.Point3D(3.0, 4.0, -10.0));
            // Projection of (3, 4, -10) is (0, 0, 0). Distance from (3, 4, -10) to (0, 0, 0) is sqrt(3^2 + 4^2 + (-10)^2) = sqrt(9 + 16 + 100) = sqrt(125)
            double double_ExpectedDistBehind = System.Math.Sqrt(125.0);
            Assert.Equal(double_ExpectedDistBehind, double_DistBehind, 9);

            // Test On method for point on the ray
            bool bool_OnRay = ray3D_Test.On(new DiGi.Geometry.Spatial.Classes.Point3D(0.0, 0.0, 5.0));
            Assert.True(bool_OnRay);

            // Test On method for point behind the ray's origin (should return false)
            bool bool_BehindRay = ray3D_Test.On(new DiGi.Geometry.Spatial.Classes.Point3D(0.0, 0.0, -5.0));
            Assert.False(bool_BehindRay);
        }

        /// <summary>
        /// Tests transforming a Ray3D object, verifying origin translation, direction rotation, and state-safety properties.
        /// </summary>
        [Fact]
        public void Ray3D_Transform()
        {
            DiGi.Geometry.Spatial.Classes.Point3D point3D_Origin = new(1.0, 2.0, 3.0);
            DiGi.Geometry.Spatial.Classes.Vector3D vector3D_Direction = new(0.0, 0.0, 1.0);
            DiGi.Geometry.Spatial.Classes.Ray3D ray3D_Target = new(point3D_Origin, vector3D_Direction);

            // 1. Test Transform method with Translation and Rotation (90 deg around X-axis)
            DiGi.Geometry.Spatial.Classes.Transform3D? transform3D_Translation = DiGi.Geometry.Spatial.Create.Transform3D.Translation(new DiGi.Geometry.Spatial.Classes.Vector3D(1.0, 2.0, 3.0));
            DiGi.Geometry.Spatial.Classes.Transform3D transform3D_Rotation = DiGi.Geometry.Spatial.Create.Transform3D.RotationX(System.Math.PI / 2.0);
            DiGi.Geometry.Spatial.Classes.Transform3D? transform3D_Combined = transform3D_Translation * transform3D_Rotation;
            Assert.NotNull(transform3D_Combined);

            bool bool_Result = ray3D_Target.Transform(transform3D_Combined);
            Assert.True(bool_Result);
            Assert.NotNull(ray3D_Target.Origin);
            Assert.Equal(2.0, ray3D_Target.Origin.X, 9);
            Assert.Equal(-1.0, ray3D_Target.Origin.Y, 9);
            Assert.Equal(5.0, ray3D_Target.Origin.Z, 9);
            Assert.NotNull(ray3D_Target.Direction);
            Assert.Equal(0.0, ray3D_Target.Direction.X, 9);
            Assert.Equal(-1.0, ray3D_Target.Direction.Y, 9);
            Assert.Equal(0.0, ray3D_Target.Direction.Z, 9);

            // 2. Test state safety on transformation failure
            DiGi.Geometry.Spatial.Classes.Transform3D transform3D_Invalid = new((DiGi.Math.Classes.Matrix4D?)null);
            bool bool_InvalidResult = ray3D_Target.Transform(transform3D_Invalid);
            Assert.False(bool_InvalidResult);
            Assert.NotNull(ray3D_Target.Origin);
            Assert.Equal(2.0, ray3D_Target.Origin.X, 9);
            Assert.Equal(-1.0, ray3D_Target.Origin.Y, 9);
            Assert.Equal(5.0, ray3D_Target.Origin.Z, 9);
            Assert.NotNull(ray3D_Target.Direction);
            Assert.Equal(0.0, ray3D_Target.Direction.X, 9);
            Assert.Equal(-1.0, ray3D_Target.Direction.Y, 9);
            Assert.Equal(0.0, ray3D_Target.Direction.Z, 9);
        }
    }
}