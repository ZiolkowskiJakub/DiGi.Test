using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the basic arithmetic operators, dot product, and length calculation of the Vector2D class.
        /// </summary>
        [Fact]
        public void Vector2D_Arithmetic()
        {
            Vector2D vector2D_1 = new(3.0, 4.0);
            Vector2D vector2D_2 = new(-1.0, 2.0);

            Vector2D? vector2D_Sum = vector2D_1 + vector2D_2;
            Vector2D? vector2D_Diff = vector2D_1 - vector2D_2;
            double double_Dot = vector2D_1 * vector2D_2;
            double double_Len = vector2D_1.Length;
            double double_SqLen = vector2D_1.SquaredLength;

            Assert.NotNull(vector2D_Sum);
            Assert.Equal(2.0, vector2D_Sum.X);
            Assert.Equal(6.0, vector2D_Sum.Y);

            Assert.NotNull(vector2D_Diff);
            Assert.Equal(4.0, vector2D_Diff.X);
            Assert.Equal(2.0, vector2D_Diff.Y);

            Assert.Equal(5.0, double_Dot);
            Assert.Equal(5.0, double_Len);
            Assert.Equal(25.0, double_SqLen);
        }

        /// <summary>
        /// Tests normalization behavior of Vector2D, including unit vector conversion and handling of zero vectors.
        /// </summary>
        [Fact]
        public void Vector2D_Normalization()
        {
            Vector2D vector2D_Main = new(0.0, 10.0);
            Vector2D? vector2D_Unit = vector2D_Main.Unit;

            Assert.NotNull(vector2D_Unit);
            Assert.Equal(0.0, vector2D_Unit.X);
            Assert.Equal(1.0, vector2D_Unit.Y);
            Assert.Equal(1.0, vector2D_Unit.Length);

            Vector2D vector2D_Zero = new(0.0, 0.0);
            Vector2D? vector2D_ZeroUnit = vector2D_Zero.Unit;
            Assert.NotNull(vector2D_ZeroUnit);
            Assert.True(double.IsNaN(vector2D_ZeroUnit.X) || vector2D_ZeroUnit.X == 0.0);
        }

        /// <summary>
        /// Tests explicit and implicit conversion operators for Vector2D.
        /// </summary>
        [Fact]
        public void Vector2D_Conversions()
        {
            Point2D point2D_Source = new(5.0, -5.0);
            Vector2D? vector2D_Converted = (Vector2D?)point2D_Source;

            Assert.NotNull(vector2D_Converted);
            Assert.Equal(5.0, vector2D_Converted.X);
            Assert.Equal(-5.0, vector2D_Converted.Y);

            Vector2D? vector2D_FromTuple = (10.0, 20.0);
            Assert.NotNull(vector2D_FromTuple);
            Assert.Equal(10.0, vector2D_FromTuple.X);
            Assert.Equal(20.0, vector2D_FromTuple.Y);
        }
    }
}