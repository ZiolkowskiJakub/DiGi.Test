using DiGi.ComputeSharp.Spatial.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Contains unit tests for calculating the centroid of 3D coordinates.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Tests the Centroid calculation with standard valid coordinates.
        /// </summary>
        [Fact]
        public void Centroid_ValidPoints()
        {
            Coordinate3 point_1 = new(1.0, 2.0, 3.0);
            Coordinate3 point_2 = new(2.0, 4.0, 6.0);
            Coordinate3 point_3 = new(3.0, 6.0, 9.0);
            Coordinate3 point_4 = new(4.0, 8.0, 12.0);
            Coordinate3 point_5 = new(5.0, 10.0, 15.0);
            Coordinate3 point_6 = new(6.0, 12.0, 18.0);

            Coordinate3 centroid_Result = Spatial.Query.Centroid(point_1, point_2, point_3, point_4, point_5, point_6);

            Assert.False(centroid_Result.IsNaN());
            Assert.Equal(3.5, centroid_Result.X);
            Assert.Equal(7.0, centroid_Result.Y);
            Assert.Equal(10.5, centroid_Result.Z);
        }

        /// <summary>
        /// Tests that a NaN point in the middle does not cause subsequent valid points to be ignored during Centroid calculation.
        /// </summary>
        [Fact]
        public void Centroid_WithNaNPoints()
        {
            Coordinate3 point_1 = new(1.0, 2.0, 3.0);
            Coordinate3 point_2 = new(); // NaN
            Coordinate3 point_3 = new(3.0, 4.0, 5.0);
            Coordinate3 point_4 = new(); // NaN
            Coordinate3 point_5 = new(); // NaN
            Coordinate3 point_6 = new(); // NaN

            Coordinate3 centroid_Result = Spatial.Query.Centroid(point_1, point_2, point_3, point_4, point_5, point_6);

            Assert.False(centroid_Result.IsNaN());
            Assert.Equal(2.0, centroid_Result.X);
            Assert.Equal(3.0, centroid_Result.Y);
            Assert.Equal(4.0, centroid_Result.Z);
        }

        /// <summary>
        /// Tests that Centroid returns a NaN coordinate when all provided points are NaN.
        /// </summary>
        [Fact]
        public void Centroid_AllNaNPoints()
        {
            Coordinate3 point_1 = new();
            Coordinate3 point_2 = new();
            Coordinate3 point_3 = new();
            Coordinate3 point_4 = new();
            Coordinate3 point_5 = new();
            Coordinate3 point_6 = new();

            Coordinate3 centroid_Result = Spatial.Query.Centroid(point_1, point_2, point_3, point_4, point_5, point_6);

            Assert.True(centroid_Result.IsNaN());
        }
    }
}
