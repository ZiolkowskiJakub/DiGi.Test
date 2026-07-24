namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the Transform method of the Circle2D class under translation and scaling.
        /// </summary>
        [Fact]
        public void Circle2D_TransformMethod()
        {
            Planar.Classes.Point2D point2D_Center = new(1.0, 2.0);
            Planar.Classes.Circle2D circle2D_Test = new(point2D_Center, 5.0);

            // Verify initial state
            Assert.NotNull(circle2D_Test.Center);
            Assert.Equal(1.0, circle2D_Test.Center.X, 9);
            Assert.Equal(2.0, circle2D_Test.Center.Y, 9);
            Assert.Equal(5.0, circle2D_Test.Radius, 9);

            // 1. Apply translation transform
            Planar.Classes.Transform2D? transform2D_Translate = Planar.Create.Transform2D.Translation(3.0, 4.0);
            Assert.NotNull(transform2D_Translate);

            bool bool_SuccessTranslate = circle2D_Test.Transform(transform2D_Translate);
            Assert.True(bool_SuccessTranslate);

            // Center should be translated, radius must remain unchanged
            Assert.NotNull(circle2D_Test.Center);
            Assert.Equal(4.0, circle2D_Test.Center.X, 9);
            Assert.Equal(6.0, circle2D_Test.Center.Y, 9);
            Assert.Equal(5.0, circle2D_Test.Radius, 9);

            // 2. Apply uniform scaling transform
            Planar.Classes.Transform2D? transform2D_Scale = Planar.Create.Transform2D.Scale(2.0, 2.0);
            Assert.NotNull(transform2D_Scale);

            bool bool_SuccessScale = circle2D_Test.Transform(transform2D_Scale);
            Assert.True(bool_SuccessScale);

            // Center and radius should be scaled by 2
            Assert.NotNull(circle2D_Test.Center);
            Assert.Equal(8.0, circle2D_Test.Center.X, 9);
            Assert.Equal(12.0, circle2D_Test.Center.Y, 9);
            Assert.Equal(10.0, circle2D_Test.Radius, 9);
        }

        /// <summary>
        /// Tests the Transform method of the Ellipse2D class under translation and scaling.
        /// </summary>
        [Fact]
        public void Ellipse2D_TransformMethod()
        {
            Planar.Classes.Point2D point2D_Center = new(1.0, 2.0);
            Planar.Classes.Vector2D vector2D_DirA = new(1.0, 0.0);
            Planar.Classes.Ellipse2D ellipse2D_Test = new(point2D_Center, 5.0, 3.0, vector2D_DirA);

            // Verify initial state
            Assert.NotNull(ellipse2D_Test.Center);
            Assert.Equal(1.0, ellipse2D_Test.Center.X, 9);
            Assert.Equal(2.0, ellipse2D_Test.Center.Y, 9);
            Assert.Equal(5.0, ellipse2D_Test.A, 9);
            Assert.Equal(3.0, ellipse2D_Test.B, 9);
            Assert.NotNull(ellipse2D_Test.DirectionA);
            Assert.Equal(1.0, ellipse2D_Test.DirectionA.X, 9);
            Assert.Equal(0.0, ellipse2D_Test.DirectionA.Y, 9);

            // 1. Apply translation transform
            Planar.Classes.Transform2D? transform2D_Translate = Planar.Create.Transform2D.Translation(3.0, 4.0);
            Assert.NotNull(transform2D_Translate);

            bool bool_SuccessTranslate = ellipse2D_Test.Transform(transform2D_Translate);
            Assert.True(bool_SuccessTranslate);

            // Center should be translated, sizes and direction must remain unchanged
            Assert.NotNull(ellipse2D_Test.Center);
            Assert.Equal(4.0, ellipse2D_Test.Center.X, 9);
            Assert.Equal(6.0, ellipse2D_Test.Center.Y, 9);
            Assert.Equal(5.0, ellipse2D_Test.A, 9);
            Assert.Equal(3.0, ellipse2D_Test.B, 9);
            Assert.NotNull(ellipse2D_Test.DirectionA);
            Assert.Equal(1.0, ellipse2D_Test.DirectionA.X, 9);
            Assert.Equal(0.0, ellipse2D_Test.DirectionA.Y, 9);

            // 2. Apply scaling transform
            Planar.Classes.Transform2D? transform2D_Scale = Planar.Create.Transform2D.Scale(2.0, 2.0);
            Assert.NotNull(transform2D_Scale);

            bool bool_SuccessScale = ellipse2D_Test.Transform(transform2D_Scale);
            Assert.True(bool_SuccessScale);

            // Center, a, and b should be scaled by 2
            Assert.NotNull(ellipse2D_Test.Center);
            Assert.Equal(8.0, ellipse2D_Test.Center.X, 9);
            Assert.Equal(12.0, ellipse2D_Test.Center.Y, 9);
            Assert.Equal(10.0, ellipse2D_Test.A, 9);
            Assert.Equal(6.0, ellipse2D_Test.B, 9);
            Assert.NotNull(ellipse2D_Test.DirectionA);
            Assert.Equal(1.0, ellipse2D_Test.DirectionA.X, 9);
            Assert.Equal(0.0, ellipse2D_Test.DirectionA.Y, 9);
        }
    }
}