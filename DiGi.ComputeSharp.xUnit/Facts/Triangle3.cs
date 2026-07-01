using DiGi.ComputeSharp.Spatial.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Contains unit tests for validating 3D triangle properties and behavior.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Tests that a valid Triangle3 returns false for IsNaN.
        /// </summary>
        [Fact]
        public void Triangle3_IsNaN_Valid()
        {
            Coordinate3 point_1 = new(0.0, 0.0, 0.0);
            Coordinate3 point_2 = new(1.0, 0.0, 0.0);
            Coordinate3 point_3 = new(0.0, 1.0, 0.0);

            Triangle3 triangle3_Result = new(point_1, point_2, point_3);

            Assert.False(triangle3_Result.IsNaN());
        }

        /// <summary>
        /// Tests that Triangle3 returns true for IsNaN if the first vertex is NaN.
        /// </summary>
        [Fact]
        public void Triangle3_IsNaN_Point1NaN()
        {
            Coordinate3 point_1 = new(); // NaN
            Coordinate3 point_2 = new(1.0, 0.0, 0.0);
            Coordinate3 point_3 = new(0.0, 1.0, 0.0);

            Triangle3 triangle3_Result = new(point_1, point_2, point_3);

            Assert.True(triangle3_Result.IsNaN());
        }

        /// <summary>
        /// Tests that Triangle3 returns true for IsNaN if the second vertex is NaN.
        /// </summary>
        [Fact]
        public void Triangle3_IsNaN_Point2NaN()
        {
            Coordinate3 point_1 = new(0.0, 0.0, 0.0);
            Coordinate3 point_2 = new(); // NaN
            Coordinate3 point_3 = new(0.0, 1.0, 0.0);

            Triangle3 triangle3_Result = new(point_1, point_2, point_3);

            Assert.True(triangle3_Result.IsNaN());
        }

        /// <summary>
        /// Tests that Triangle3 returns true for IsNaN if the third vertex is NaN.
        /// </summary>
        [Fact]
        public void Triangle3_IsNaN_Point3NaN()
        {
            Coordinate3 point_1 = new(0.0, 0.0, 0.0);
            Coordinate3 point_2 = new(1.0, 0.0, 0.0);
            Coordinate3 point_3 = new(); // NaN

            Triangle3 triangle3_Result = new(point_1, point_2, point_3);

            Assert.True(triangle3_Result.IsNaN());
        }
    }
}