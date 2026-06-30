using DiGi.ComputeSharp.Core.Constants;
using DiGi.ComputeSharp.Spatial.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Behaviour tests for <see cref="Spatial.Query.SameHalf(Coordinate3, Coordinate3, double)"/>.
    /// Acts as a regression guard for the square-root-free optimization (the result must be unchanged).
    /// </summary>
    public partial class Facts
    {
        [Fact]
        public void SameHalf_Behavior()
        {
            double tolerance = Tolerance.Distance;

            // Same direction / acute angle -> same half.
            Assert.True(Spatial.Query.SameHalf(new Coordinate3(1, 0, 0), new Coordinate3(2, 0, 0), tolerance));
            Assert.True(Spatial.Query.SameHalf(new Coordinate3(1, 0, 0), new Coordinate3(1, 1, 0), tolerance));

            // Magnitudes must not change the outcome (sign of the dot product only).
            Assert.True(Spatial.Query.SameHalf(new Coordinate3(100, 0, 0), new Coordinate3(0.001, 0.0005, 0), tolerance));

            // Perpendicular -> dot product is 0, which is within tolerance -> same half (matches the original `> -tolerance`).
            Assert.True(Spatial.Query.SameHalf(new Coordinate3(1, 0, 0), new Coordinate3(0, 1, 0), tolerance));

            // Opposite / obtuse angle -> not the same half.
            Assert.False(Spatial.Query.SameHalf(new Coordinate3(1, 0, 0), new Coordinate3(-1, 0, 0), tolerance));
            Assert.False(Spatial.Query.SameHalf(new Coordinate3(1, 0, 0), new Coordinate3(-1, 1, 0), tolerance));

            // NaN inputs -> false.
            Assert.False(Spatial.Query.SameHalf(new Coordinate3(), new Coordinate3(1, 0, 0), tolerance));
        }
    }
}
