using DiGi.ComputeSharp.Spatial.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Contains unit tests for <see cref="Plane"/> geometry.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Plane.On(Coordinate3, double)"/> treats <c>tolerance</c> as a linear distance.
        /// Regression test for the bug where a squared distance was compared against the (non-squared) tolerance.
        /// </summary>
        [Fact]
        public void Plane_On_Tolerance()
        {
            double tolerance = 1e-6;

            // XY plane through the origin (normal +Z).
            Plane plane = new(new Coordinate3(0, 0, 0), new Coordinate3(0, 0, 1), tolerance);

            // 0.5 mm above the plane (distance 5e-4). With a 1e-6 distance tolerance this is NOT on the plane.
            // Before the fix: squaredDistance (2.5e-7) <= tolerance (1e-6) => wrongly reported as On.
            Coordinate3 pointOff = new(1.0, 1.0, 5e-4);
            Assert.False(plane.On(pointOff, tolerance));

            // Well within tolerance (distance 5e-7 < 1e-6) -> on the plane.
            Coordinate3 pointOn = new(1.0, 1.0, 5e-7);
            Assert.True(plane.On(pointOn, tolerance));

            // Exactly on the plane.
            Assert.True(plane.On(new Coordinate3(2.0, -3.0, 0.0), tolerance));
        }
    }
}
