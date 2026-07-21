namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the construction, distance, projection, and transformation of a Line3D object, including state-safety checks on transformation failures.
        /// </summary>
        [Fact]
        public void Line3D()
        {
            DiGi.Geometry.Spatial.Classes.Point3D point3D_Origin = new(0.0, 0.0, 0.0);
            DiGi.Geometry.Spatial.Classes.Vector3D vector3D_Direction = new(0.0, 0.0, 1.0);
            DiGi.Geometry.Spatial.Classes.Line3D line3D_Target = new(point3D_Origin, vector3D_Direction);

            // 1. Test properties
            Assert.NotNull(line3D_Target.Origin);
            Assert.Equal(0.0, line3D_Target.Origin.X, 9);
            Assert.Equal(0.0, line3D_Target.Origin.Y, 9);
            Assert.Equal(0.0, line3D_Target.Origin.Z, 9);
            Assert.NotNull(line3D_Target.Direction);
            Assert.Equal(0.0, line3D_Target.Direction.X, 9);
            Assert.Equal(0.0, line3D_Target.Direction.Y, 9);
            Assert.Equal(1.0, line3D_Target.Direction.Z, 9);

            // 2. Test Distance method
            DiGi.Geometry.Spatial.Classes.Point3D point3D_Test = new(3.0, 4.0, 5.0);
            double double_Distance = line3D_Target.Distance(point3D_Test);
            Assert.Equal(5.0, double_Distance, 9);

            // 3. Test Project method
            DiGi.Geometry.Spatial.Classes.Point3D? point3D_Projected = line3D_Target.Project(point3D_Test);
            Assert.NotNull(point3D_Projected);
            Assert.Equal(0.0, point3D_Projected.X, 9);
            Assert.Equal(0.0, point3D_Projected.Y, 9);
            Assert.Equal(5.0, point3D_Projected.Z, 9);

            // 4. Test Transform method with Translation and Rotation (90 deg around X-axis)
            DiGi.Geometry.Spatial.Classes.Transform3D? transform3D_Translation = Spatial.Create.Transform3D.Translation(new DiGi.Geometry.Spatial.Classes.Vector3D(1.0, 2.0, 3.0));
            DiGi.Geometry.Spatial.Classes.Transform3D transform3D_Rotation = Spatial.Create.Transform3D.RotationX(System.Math.PI / 2.0);
            DiGi.Geometry.Spatial.Classes.Transform3D? transform3D_Combined = transform3D_Translation * transform3D_Rotation;
            Assert.NotNull(transform3D_Combined);

            bool bool_Result = line3D_Target.Transform(transform3D_Combined);
            Assert.True(bool_Result);
            Assert.NotNull(line3D_Target.Origin);
            Assert.Equal(1.0, line3D_Target.Origin.X, 9);
            Assert.Equal(2.0, line3D_Target.Origin.Y, 9);
            Assert.Equal(3.0, line3D_Target.Origin.Z, 9);
            Assert.NotNull(line3D_Target.Direction);
            Assert.Equal(0.0, line3D_Target.Direction.X, 9);
            Assert.Equal(-1.0, line3D_Target.Direction.Y, 9);
            Assert.Equal(0.0, line3D_Target.Direction.Z, 9);

            // 5. Test state safety on transformation failure
            DiGi.Geometry.Spatial.Classes.Transform3D transform3D_Invalid = new((DiGi.Math.Classes.Matrix4D?)null);
            bool bool_InvalidResult = line3D_Target.Transform(transform3D_Invalid);
            Assert.False(bool_InvalidResult);
            Assert.NotNull(line3D_Target.Origin);
            Assert.Equal(1.0, line3D_Target.Origin.X, 9);
            Assert.Equal(2.0, line3D_Target.Origin.Y, 9);
            Assert.Equal(3.0, line3D_Target.Origin.Z, 9);
            Assert.NotNull(line3D_Target.Direction);
            Assert.Equal(0.0, line3D_Target.Direction.X, 9);
            Assert.Equal(-1.0, line3D_Target.Direction.Y, 9);
            Assert.Equal(0.0, line3D_Target.Direction.Z, 9);
        }
    }
}