namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the projection, distance, and boundary-on functionalities of a Ray2D instance.
        /// </summary>
        [Fact]
        public void Ray2D_Project()
        {
            Planar.Classes.Point2D point2D_Origin = new(0.0, 0.0);
            Planar.Classes.Vector2D vector2D_Direction = new(0.0, 1.0);
            Planar.Classes.Ray2D ray2D_Test = new(point2D_Origin, vector2D_Direction);

            // Test projection of point in front of the origin
            Planar.Classes.Point2D? point2D_ProjFront = ray2D_Test.Project(new Planar.Classes.Point2D(2.0, 10.0));
            Assert.NotNull(point2D_ProjFront);
            if (point2D_ProjFront is not null)
            {
                Assert.Equal(point2D_ProjFront, new Planar.Classes.Point2D(0.0, 10.0));
            }

            // Test projection of point behind the origin (should clamp to origin)
            Planar.Classes.Point2D? point2D_ProjBehind = ray2D_Test.Project(new Planar.Classes.Point2D(2.0, -10.0));
            Assert.NotNull(point2D_ProjBehind);
            if (point2D_ProjBehind is not null)
            {
                Assert.Equal(point2D_ProjBehind, new Planar.Classes.Point2D(0.0, 0.0));
            }

            // Test Distance method for point in front
            double double_DistFront = ray2D_Test.Distance(new Planar.Classes.Point2D(2.0, 10.0));
            Assert.Equal(2.0, double_DistFront, 9);

            // Test Distance method for point behind origin
            double double_DistBehind = ray2D_Test.Distance(new Planar.Classes.Point2D(2.0, -10.0));
            // Projection of (2, -10) is (0, 0). Distance from (2, -10) to (0, 0) is sqrt(2^2 + (-10)^2) = sqrt(104)
            double double_ExpectedDistBehind = System.Math.Sqrt(104.0);
            Assert.Equal(double_ExpectedDistBehind, double_DistBehind, 9);

            // Test On method for point on the ray
            bool bool_OnRay = ray2D_Test.On(new Planar.Classes.Point2D(0.0, 5.0));
            Assert.True(bool_OnRay);

            // Test On method for point behind the ray's origin (should return false)
            bool bool_BehindRay = ray2D_Test.On(new Planar.Classes.Point2D(0.0, -5.0));
            Assert.False(bool_BehindRay);
        }

        /// <summary>
        /// Tests transforming a Ray2D object, verifying origin translation, direction rotation, and state-safety properties.
        /// </summary>
        [Fact]
        public void Ray2D_Transform()
        {
            Planar.Classes.Point2D point2D_Origin = new(1.0, 2.0);
            Planar.Classes.Vector2D vector2D_Direction = new(1.0, 0.0);
            Planar.Classes.Ray2D ray2D_Target = new(point2D_Origin, vector2D_Direction);

            // 1. Test Transform method with Translation and Rotation (90 deg CCW around origin)
            Planar.Classes.Transform2D? transform2D_Combined = Planar.Create.Transform2D.Rotation(System.Math.PI / 2.0) * Planar.Create.Transform2D.Translation(1.0, 2.0);
            Assert.NotNull(transform2D_Combined);

            bool bool_Result = ray2D_Target.Transform(transform2D_Combined);
            Assert.True(bool_Result);
            Assert.NotNull(ray2D_Target.Origin);
            Assert.Equal(-4.0, ray2D_Target.Origin.X, 9);
            Assert.Equal(2.0, ray2D_Target.Origin.Y, 9);
            Assert.NotNull(ray2D_Target.Direction);
            Assert.Equal(0.0, ray2D_Target.Direction.X, 9);
            Assert.Equal(1.0, ray2D_Target.Direction.Y, 9);

            // 2. Test state safety on transformation failure
            Planar.Classes.Transform2D transform2D_Invalid = new((Math.Classes.Matrix3D?)null);
            bool bool_InvalidResult = ray2D_Target.Transform(transform2D_Invalid);
            Assert.False(bool_InvalidResult);
            Assert.NotNull(ray2D_Target.Origin);
            Assert.Equal(-4.0, ray2D_Target.Origin.X, 9);
            Assert.Equal(2.0, ray2D_Target.Origin.Y, 9);
            Assert.NotNull(ray2D_Target.Direction);
            Assert.Equal(0.0, ray2D_Target.Direction.X, 9);
            Assert.Equal(1.0, ray2D_Target.Direction.Y, 9);
        }
    }
}