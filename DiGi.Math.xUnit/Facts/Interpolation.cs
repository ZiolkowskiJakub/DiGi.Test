using DiGi.Math.Classes;

namespace DiGi.Math.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the functionality of the LinearInterpolation class, verifying point additions, boundary limits, and Y values calculations.
        /// </summary>
        [Fact]
        public void LinearInterpolation()
        {
            // Empty constructor
            LinearInterpolation linearInterpolation_Empty = new();
            Assert.Equal(-1, linearInterpolation_Empty.Count);
            Assert.True(double.IsNaN(linearInterpolation_Empty.MinX));
            Assert.True(double.IsNaN(linearInterpolation_Empty.MaxX));

            // Point constructors
            LinearInterpolation linearInterpolation_1 = new(1.0, 10.0, 3.0, 30.0);
            Assert.Equal(2, linearInterpolation_1.Count);
            Assert.Equal(1.0, linearInterpolation_1.MinX);
            Assert.Equal(3.0, linearInterpolation_1.MaxX);

            // Adding points
            Assert.True(linearInterpolation_1.Add(2.0, 20.0));
            Assert.Equal(3, linearInterpolation_1.Count);

            // Adding invalid values
            Assert.False(linearInterpolation_1.Add(double.NaN, 5.0));
            Assert.False(linearInterpolation_1.Add(5.0, double.NaN));

            // InRange checks
            Assert.True(linearInterpolation_1.InRange(1.0));
            Assert.True(linearInterpolation_1.InRange(2.0));
            Assert.True(linearInterpolation_1.InRange(3.0));
            Assert.False(linearInterpolation_1.InRange(0.9));
            Assert.False(linearInterpolation_1.InRange(3.1));

            // Interpolation calculations
            // Points are (1, 10), (3, 30), (2, 20) in order of insertion.
            // Let's check interpolation at x = 1.5.
            // Under linear interpolation:
            // For interval [1.0, 3.0], since we search sequentially:
            // values contains KeyValuePairs in insertion order: (1.0, 10.0), (3.0, 30.0), (2.0, 20.0).
            // CalculateY finds matching intervals.
            double result = linearInterpolation_1.CalculateY(1.5);
            // Since x = 1.5 is between 1.0 and 3.0, it interpolates:
            // 10 + (30 - 10) * (1.5 - 1) / (3 - 1) = 10 + 20 * 0.5 / 2 = 15.0
            Assert.Equal(15.0, result);

            // Check out-of-range returns NaN
            Assert.True(double.IsNaN(linearInterpolation_1.CalculateY(4.0)));

            // 2D Array constructor
            double[,] doubleArray_Data = new double[2, 3]
            {
                { 1.0, 2.0, 3.0 }, // X coordinates
                { 100.0, 200.0, 300.0 } // Y coordinates
            };
            LinearInterpolation linearInterpolation_Array = new(doubleArray_Data);
            Assert.Equal(3, linearInterpolation_Array.Count);
            Assert.Equal(150.0, linearInterpolation_Array.CalculateY(1.5));

            // Serialization Check
            Core.xUnit.Query.SerializationCheck(linearInterpolation_1);
        }

        /// <summary>
        /// Tests the functionality of the BilinearInterpolation class, verifying grid construction, interpolation, and edge boundary checks.
        /// </summary>
        [Fact]
        public void BilinearInterpolation()
        {
            // Set up a 2x2 grid:
            // x coordinates: [1.0, 3.0]
            // y coordinates: [10.0, 20.0]
            // Matrix layout (3x3):
            // NaN   1.0   3.0
            // 10.0  100   200
            // 20.0  300   400
            double[,] gridData = new double[3, 3]
            {
                { double.NaN, 1.0, 3.0 },
                { 10.0, 100.0, 200.0 },
                { 20.0, 300.0, 400.0 }
            };

            BilinearInterpolation bilinearInterpolation_1 = new(gridData);

            // Test copy constructor
            BilinearInterpolation bilinearInterpolation_Copy = new(bilinearInterpolation_1);

            // Test bilinear interpolation calculations
            // Check exact grid point matches:
            Assert.Equal(100.0, bilinearInterpolation_1.Calculate(1.0, 10.0));
            Assert.Equal(200.0, bilinearInterpolation_1.Calculate(3.0, 10.0));
            Assert.Equal(300.0, bilinearInterpolation_1.Calculate(1.0, 20.0));
            Assert.Equal(400.0, bilinearInterpolation_1.Calculate(3.0, 20.0));

            // Interpolate at center point (x = 2.0, y = 15.0)
            // x is halfway between 1 and 3; y is halfway between 10 and 20.
            // Expected result is average of all 4 corners: (100 + 200 + 300 + 400) / 4 = 250.0
            double resultCenter = bilinearInterpolation_1.Calculate(2.0, 15.0);
            Assert.Equal(250.0, resultCenter);

            // Interpolate on a boundary line where y = 10.0 (horizontal line)
            // x = 2.0, y = 10.0
            // Expected: 100.0 + (200.0 - 100.0) * (2.0 - 1.0) / (3.0 - 1.0) = 150.0
            double resultHoriz = bilinearInterpolation_1.Calculate(2.0, 10.0);
            Assert.Equal(150.0, resultHoriz);

            // Interpolate on a boundary line where x = 1.0 (vertical line)
            // x = 1.0, y = 15.0
            // Expected: 100.0 + (300.0 - 100.0) * (15.0 - 10.0) / (20.0 - 10.0) = 200.0
            double resultVert = bilinearInterpolation_1.Calculate(1.0, 15.0);
            Assert.Equal(200.0, resultVert);

            // Out-of-bounds evaluation (extrapolates using boundary edges)
            Assert.Equal(300.0, bilinearInterpolation_1.Calculate(4.0, 15.0));
            Assert.Equal(350.0, bilinearInterpolation_1.Calculate(2.0, 25.0));

            // Serialization Check
            Core.xUnit.Query.SerializationCheck(bilinearInterpolation_1);
        }
    }
}