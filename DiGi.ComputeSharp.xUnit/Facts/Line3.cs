using DiGi.ComputeSharp.Spatial.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Contains unit tests for <see cref="Line3"/> geometry.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Line3.On(Coordinate3, double)"/> treats <c>tolerance</c> as a linear distance.
        /// Regression test for the bug where a squared distance was compared against the (non-squared) tolerance,
        /// which made the effective threshold sqrt(tolerance) (e.g. 1e-3 for a 1e-6 tolerance).
        /// </summary>
        [Fact]
        public void Line3_On_Tolerance()
        {
            // Segment along the X axis from (0,0,0) to (10,0,0).
            Line3 line3 = new(new DiGi.ComputeSharp.Core.Classes.Bool(true), new Coordinate3(0, 0, 0), new Coordinate3(10, 0, 0));

            double tolerance = 1e-6;

            // 0.5 mm off the line (distance 5e-4). With a 1e-6 distance tolerance this point is NOT on the line.
            // Before the fix: squaredDistance (2.5e-7) <= tolerance (1e-6) => wrongly reported as On.
            Coordinate3 pointOff = new(5.0, 5e-4, 0.0);
            Assert.False(line3.On(pointOff, tolerance));

            // Well within tolerance (distance 5e-7 < 1e-6) -> on the line.
            Coordinate3 pointOn = new(5.0, 5e-7, 0.0);
            Assert.True(line3.On(pointOn, tolerance));

            // Exactly on the line.
            Assert.True(line3.On(new Coordinate3(5.0, 0.0, 0.0), tolerance));

            // Clearly off the line.
            Assert.False(line3.On(new Coordinate3(5.0, 1.0, 0.0), tolerance));
        }

        /// <summary>
        /// Line3.Project(Line3) must project both endpoints (start and end), not the start twice.
        /// Regression test for the copy-paste bug that always collapsed the result to an empty line.
        /// </summary>
        [Fact]
        public void Line3_ProjectLine()
        {
            // Project a slanted segment onto the X-axis segment -> its X-extent on the axis.
            Line3 axis = new(new Coordinate3(0, 0, 0), new Coordinate3(10, 0, 0));
            Line3 other = new(new Coordinate3(2, 3, 0), new Coordinate3(6, 5, 0));

            Line3 projected = axis.Project(other);

            Assert.False(projected.IsNaN());
            Assert.Equal(2.0, projected.Start.X, 1e-9);
            Assert.Equal(0.0, projected.Start.Y, 1e-9);
            Assert.Equal(6.0, projected.End.X, 1e-9);
            Assert.Equal(0.0, projected.End.Y, 1e-9);
        }
    }
}