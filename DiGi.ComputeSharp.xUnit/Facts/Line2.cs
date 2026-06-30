using DiGi.ComputeSharp.Core.Classes;
using DiGi.ComputeSharp.Core.Constants;
using DiGi.ComputeSharp.Planar.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Unit tests for the 2D <see cref="Line2"/> primitive (regression coverage for the latent bugs).
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// A bounded segment must report its finite length, not infinity. Regression test for the inverted
        /// Bounded checks in GetSquaredLength / GetApproximateLength.
        /// </summary>
        [Fact]
        public void Line2_BoundedLength_IsFinite()
        {
            double tolerance = Tolerance.Distance;

            Line2 line = new(new Coordinate2(0, 0), new Coordinate2(3, 4));

            Assert.Equal(25.0, line.GetSquaredLength(), 1e-9);
            Assert.Equal(5.0, line.GetLength(tolerance), 1e-9);
            Assert.False(double.IsInfinity(line.GetApproximateLength()));

            // An unbounded line still reports an infinite length.
            Line2 unbounded = new(new Bool(false), new Coordinate2(0, 0), new Coordinate2(3, 4));
            Assert.True(double.IsInfinity(unbounded.GetSquaredLength()));
        }

        /// <summary>
        /// GetClosestPoint on an unbounded line must project beyond the segment endpoints (no clamping).
        /// Regression test for the `if (true)` that always clamped.
        /// </summary>
        [Fact]
        public void Line2_GetClosestPoint_UnboundedDoesNotClamp()
        {
            // Unbounded line along the X axis from (0,0) to (1,0).
            Line2 unbounded = new(new Bool(false), new Coordinate2(0, 0), new Coordinate2(1, 0));

            // Closest point to (5, 2) on the infinite X axis is (5, 0) - past the (1,0) endpoint.
            Coordinate2 closest = unbounded.GetClosestPoint(new Coordinate2(5, 2));
            Assert.Equal(5.0, closest.X, 1e-9);
            Assert.Equal(0.0, closest.Y, 1e-9);

            // A bounded segment clamps to its endpoint.
            Line2 bounded = new(new Coordinate2(0, 0), new Coordinate2(1, 0));
            Coordinate2 clamped = bounded.GetClosestPoint(new Coordinate2(5, 2));
            Assert.Equal(1.0, clamped.X, 1e-9);
            Assert.Equal(0.0, clamped.Y, 1e-9);
        }

        /// <summary>
        /// Line2.On must treat tolerance as a linear distance. Regression test for the squared-vs-linear bug.
        /// </summary>
        [Fact]
        public void Line2_On_Tolerance()
        {
            double tolerance = 1e-6;

            Line2 line = new(new Coordinate2(0, 0), new Coordinate2(10, 0));

            Assert.False(line.On(new Coordinate2(5, 5e-4), tolerance)); // 0.5 mm off -> not on
            Assert.True(line.On(new Coordinate2(5, 5e-7), tolerance));  // within tolerance -> on
            Assert.True(line.On(new Coordinate2(5, 0), tolerance));     // exactly on
        }

        /// <summary>
        /// Line2.Project(Line2) must project both endpoints (start and end), not the start twice.
        /// </summary>
        [Fact]
        public void Line2_ProjectLine()
        {
            // Project a slanted segment onto the X axis -> its X-extent.
            Line2 axis = new(new Coordinate2(0, 0), new Coordinate2(10, 0));
            Line2 other = new(new Coordinate2(2, 3), new Coordinate2(6, 5));

            Line2 projected = axis.Project(other);

            Assert.False(projected.IsNaN());
            Assert.Equal(2.0, projected.Start.X, 1e-9);
            Assert.Equal(0.0, projected.Start.Y, 1e-9);
            Assert.Equal(6.0, projected.End.X, 1e-9);
            Assert.Equal(0.0, projected.End.Y, 1e-9);
        }
    }
}
