namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Create.BoundingBox(double, double, double, double, double, double)"/> calculates center normalized coordinates and rejects <see cref="double.NaN"/> values.
        /// </summary>
        [Fact]
        public void BoundingBox()
        {
            Classes.BoundingBox? boundingBox = Create.BoundingBox(1000.0, 500.0, 100.0, 50.0, 200.0, 100.0);

            Assert.NotNull(boundingBox);
            Assert.Equal(0.2, boundingBox!.X);
            Assert.Equal(0.2, boundingBox.Y);
            Assert.Equal(0.2, boundingBox.Width);
            Assert.Equal(0.2, boundingBox.Height);

            Assert.Null(Create.BoundingBox(double.NaN, 500.0, 100.0, 50.0, 200.0, 100.0));
            Assert.Null(Create.BoundingBox(1000.0, double.NaN, 100.0, 50.0, 200.0, 100.0));
            Assert.Null(Create.BoundingBox(1000.0, 500.0, double.NaN, 50.0, 200.0, 100.0));
            Assert.Null(Create.BoundingBox(1000.0, 500.0, 100.0, double.NaN, 200.0, 100.0));
            Assert.Null(Create.BoundingBox(1000.0, 500.0, 100.0, 50.0, double.NaN, 100.0));
            Assert.Null(Create.BoundingBox(1000.0, 500.0, 100.0, 50.0, 200.0, double.NaN));
        }
    }
}
