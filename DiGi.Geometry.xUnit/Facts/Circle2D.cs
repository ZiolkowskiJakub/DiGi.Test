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
        /// Tests the Circle2D InRange overload for axis-aligned bounding boxes, covering containment, corner exclusion, tolerance boundaries, and degenerate inputs.
        /// </summary>
        [Fact]
        public void Circle2D_InRange_BoundingBox2D()
        {
            Point2D point2D_Center = new(0.0, 0.0);
            Circle2D circle2D_Target = new(point2D_Center, 5.0);

            // Bounding box whose closest point is well within the radius.
            BoundingBox2D boundingBox2D_Inside = new(new Point2D(3.0, 0.0), new Point2D(4.0, 1.0));
            Assert.True(circle2D_Target.InRange(boundingBox2D_Inside, 1e-9));

            // Circle centre inside the bounding box.
            BoundingBox2D boundingBox2D_Containing = new(new Point2D(-1.0, -1.0), new Point2D(1.0, 1.0));
            Assert.True(circle2D_Target.InRange(boundingBox2D_Containing, 1e-9));

            // Bounding box far outside the circle.
            BoundingBox2D boundingBox2D_Outside = new(new Point2D(20.0, 20.0), new Point2D(21.0, 21.0));
            Assert.False(circle2D_Target.InRange(boundingBox2D_Outside, 1e-9));

            // Key case: bounding box sits in the corner of the circle's bounding square
            // (inside [-5, 5] x [-5, 5]) but outside the true radius, so it must be excluded.
            BoundingBox2D boundingBox2D_Corner = new(new Point2D(4.9, 4.9), new Point2D(5.0, 5.0));
            Assert.False(circle2D_Target.InRange(boundingBox2D_Corner, 1e-9));

            // Tolerance boundary: closest point sits at distance 5.05 from the centre.
            BoundingBox2D boundingBox2D_NearEdge = new(new Point2D(5.05, 0.0), new Point2D(6.0, 1.0));
            Assert.True(circle2D_Target.InRange(boundingBox2D_NearEdge, 0.1));
            Assert.False(circle2D_Target.InRange(boundingBox2D_NearEdge, 0.01));

            // Degenerate inputs.
            Assert.False(circle2D_Target.InRange((BoundingBox2D?)null, 1e-9));
            BoundingBox2D boundingBox2D_NaN = new(new Point2D(double.NaN, double.NaN), new Point2D(double.NaN, double.NaN));
            Assert.False(circle2D_Target.InRange(boundingBox2D_NaN, 1e-9));
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
            DiGi.Geometry.Planar.Classes.Transform2D? transform2D_Trans = Planar.Create.Transform2D.Translation(-10.0, 10.0);
            Assert.NotNull(transform2D_Trans);
            bool bool_TransResult = circle2D_Target.Transform(transform2D_Trans);
            Assert.True(bool_TransResult);
            Assert.NotNull(circle2D_Target.Center);
            Assert.Equal(0.0, circle2D_Target.Center.X, 9);
            Assert.Equal(0.0, circle2D_Target.Center.Y, 9);
            Assert.Equal(5.0, circle2D_Target.Radius, 9);

            // 3. Test Transform method with Scaling
            DiGi.Geometry.Planar.Classes.Transform2D? transform2D_Scale = Planar.Create.Transform2D.Scale(2.0);
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