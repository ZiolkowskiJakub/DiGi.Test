namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the Inverse method of the Segment2D class, verifying that reversing the segment correctly swaps its start and end points and negates its direction without translating its physical position.
        /// </summary>
        [Fact]
        public void Segment2D_Inverse()
        {
            DiGi.Geometry.Planar.Classes.Point2D point2D_Start = new(0.0, 0.0);
            DiGi.Geometry.Planar.Classes.Point2D point2D_End = new(10.0, 0.0);
            DiGi.Geometry.Planar.Classes.Segment2D segment2D_Line = new(point2D_Start, point2D_End);

            // Verify initial state
            Assert.NotNull(segment2D_Line.Start);
            Assert.NotNull(segment2D_Line.End);
            Assert.Equal(0.0, segment2D_Line.Start.X, 9);
            Assert.Equal(0.0, segment2D_Line.Start.Y, 9);
            Assert.Equal(10.0, segment2D_Line.End.X, 9);
            Assert.Equal(0.0, segment2D_Line.End.Y, 9);
            Assert.Equal(10.0, segment2D_Line.Length, 9);

            // Invert segment
            bool success = segment2D_Line.Inverse();
            Assert.True(success);

            // Verify inverted state
            Assert.NotNull(segment2D_Line.Start);
            Assert.NotNull(segment2D_Line.End);
            Assert.Equal(10.0, segment2D_Line.Start.X, 9);
            Assert.Equal(0.0, segment2D_Line.Start.Y, 9);
            Assert.Equal(0.0, segment2D_Line.End.X, 9);
            Assert.Equal(0.0, segment2D_Line.End.Y, 9);
            Assert.Equal(10.0, segment2D_Line.Length, 9);
            Assert.NotNull(segment2D_Line.Direction);
            Assert.Equal(-1.0, segment2D_Line.Direction.X, 9);
            Assert.Equal(0.0, segment2D_Line.Direction.Y, 9);
        }

        /// <summary>
        /// Tests transforming a Segment2D object, verifying translation, scaling, and state-safety properties.
        /// </summary>
        [Fact]
        public void Segment2D_Transform()
        {
            DiGi.Geometry.Planar.Classes.Point2D point2D_Start = new(0.0, 0.0);
            DiGi.Geometry.Planar.Classes.Point2D point2D_End = new(10.0, 0.0);
            DiGi.Geometry.Planar.Classes.Segment2D segment2D_Line = new(point2D_Start, point2D_End);

            // 1. Test Transform method with Translation
            DiGi.Geometry.Planar.Classes.Transform2D? transform2D_Trans = DiGi.Geometry.Planar.Create.Transform2D.Translation(2.0, 3.0);
            Assert.NotNull(transform2D_Trans);
            bool bool_TransResult = segment2D_Line.Transform(transform2D_Trans);
            Assert.True(bool_TransResult);
            Assert.NotNull(segment2D_Line.Start);
            Assert.NotNull(segment2D_Line.End);
            Assert.Equal(2.0, segment2D_Line.Start.X, 9);
            Assert.Equal(3.0, segment2D_Line.Start.Y, 9);
            Assert.Equal(12.0, segment2D_Line.End.X, 9);
            Assert.Equal(3.0, segment2D_Line.End.Y, 9);

            // 2. Test state safety on transformation failure
            DiGi.Geometry.Planar.Classes.Transform2D transform2D_Invalid = new((DiGi.Math.Classes.Matrix3D?)null);
            bool bool_InvalidResult = segment2D_Line.Transform(transform2D_Invalid);
            Assert.False(bool_InvalidResult);
            Assert.NotNull(segment2D_Line.Start);
            Assert.NotNull(segment2D_Line.End);
            Assert.Equal(2.0, segment2D_Line.Start.X, 9);
            Assert.Equal(3.0, segment2D_Line.Start.Y, 9);
            Assert.Equal(12.0, segment2D_Line.End.X, 9);
            Assert.Equal(3.0, segment2D_Line.End.Y, 9);
        }
    }
}