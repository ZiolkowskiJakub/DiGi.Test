using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests circle property calculations such as area, circumference, and bounding box.
        /// </summary>
        [Fact]
        public void Circle2D_Properties()
        {
            Point2D point2D_Center = new(0.0, 0.0);
            Circle2D circle2D_Target = new(point2D_Center, 5.0);

            double double_Area = circle2D_Target.GetArea();
            double double_Perimeter = circle2D_Target.GetPerimeter();
            BoundingBox2D? boundingBox2D = circle2D_Target.GetBoundingBox();

            Assert.Equal(System.Math.PI * 25.0, double_Area, 5);
            Assert.Equal(2.0 * System.Math.PI * 5.0, double_Perimeter, 5);

            Assert.NotNull(boundingBox2D);
            Assert.NotNull(boundingBox2D.Min);
            Assert.NotNull(boundingBox2D.Max);
            Assert.Equal(-5.0, boundingBox2D.Min.X);
            Assert.Equal(-5.0, boundingBox2D.Min.Y);
            Assert.Equal(5.0, boundingBox2D.Max.X);
            Assert.Equal(5.0, boundingBox2D.Max.Y);
        }

        /// <summary>
        /// Tests the InRange, Inside, and On checks of the Circle2D class for various point coordinates.
        /// </summary>
        [Fact]
        public void Circle2D_Containment()
        {
            Point2D point2D_Center = new(0.0, 0.0);
            Circle2D circle2D_Target = new(point2D_Center, 5.0);

            Point2D point2D_Inside = new(3.0, 0.0);
            Point2D point2D_Outside = new(6.0, 0.0);
            Point2D point2D_Boundary = new(0.0, 5.0);

            Assert.True(circle2D_Target.Inside(point2D_Inside, 1e-9));
            Assert.False(circle2D_Target.Inside(point2D_Outside, 1e-9));

            Assert.True(circle2D_Target.On(point2D_Boundary, 1e-9));
            Assert.False(circle2D_Target.On(point2D_Inside, 1e-9));

            Assert.True(circle2D_Target.InRange(point2D_Boundary, 1e-9));
            Assert.False(circle2D_Target.InRange(point2D_Outside, 1e-9));
        }

        /// <summary>
        /// Tests transforming and translating a Circle2D object, verifying translation, scaling, and state-safety properties.
        /// </summary>
        [Fact]
        public void Circle2D_Transform()
        {
            DiGi.Geometry.Planar.Classes.Point2D point2D_Center = new(0.0, 0.0);
            DiGi.Geometry.Planar.Classes.Circle2D circle2D_Target = new(point2D_Center, 5.0);

            // 1. Test Move method
            DiGi.Geometry.Planar.Classes.Vector2D vector2D_Translation = new(10.0, -10.0);
            bool bool_MoveResult = circle2D_Target.Move(vector2D_Translation);

            Assert.True(bool_MoveResult);
            Assert.NotNull(circle2D_Target.Center);
            Assert.Equal(10.0, circle2D_Target.Center.X, 9);
            Assert.Equal(-10.0, circle2D_Target.Center.Y, 9);
            Assert.Equal(5.0, circle2D_Target.Radius, 9);

            // 2. Test Transform method with Translation
            DiGi.Geometry.Planar.Classes.Transform2D? transform2D_Trans = DiGi.Geometry.Planar.Create.Transform2D.Translation(-10.0, 10.0);
            Assert.NotNull(transform2D_Trans);
            bool bool_TransResult = circle2D_Target.Transform(transform2D_Trans);
            Assert.True(bool_TransResult);
            Assert.NotNull(circle2D_Target.Center);
            Assert.Equal(0.0, circle2D_Target.Center.X, 9);
            Assert.Equal(0.0, circle2D_Target.Center.Y, 9);
            Assert.Equal(5.0, circle2D_Target.Radius, 9);

            // 3. Test Transform method with Scaling
            DiGi.Geometry.Planar.Classes.Transform2D? transform2D_Scale = DiGi.Geometry.Planar.Create.Transform2D.Scale(2.0);
            Assert.NotNull(transform2D_Scale);
            bool bool_ScaleResult = circle2D_Target.Transform(transform2D_Scale);
            Assert.True(bool_ScaleResult);
            Assert.NotNull(circle2D_Target.Center);
            Assert.Equal(0.0, circle2D_Target.Center.X, 9);
            Assert.Equal(0.0, circle2D_Target.Center.Y, 9);
            Assert.Equal(10.0, circle2D_Target.Radius, 9);

            // 4. Test state safety on transformation failure
            DiGi.Geometry.Planar.Classes.Transform2D transform2D_Invalid = new((System.Text.Json.Nodes.JsonObject?)null);
            bool bool_InvalidResult = circle2D_Target.Transform(transform2D_Invalid);
            Assert.False(bool_InvalidResult);
            Assert.NotNull(circle2D_Target.Center);
            Assert.Equal(0.0, circle2D_Target.Center.X, 9);
            Assert.Equal(0.0, circle2D_Target.Center.Y, 9);
            Assert.Equal(10.0, circle2D_Target.Radius, 9);
        }
    }
}
