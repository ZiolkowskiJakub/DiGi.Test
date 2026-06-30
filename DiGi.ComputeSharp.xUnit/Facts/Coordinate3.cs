using DiGi.ComputeSharp.Spatial.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Contains unit tests for <see cref="Coordinate3"/> vector operations.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Coordinate3.Project(Coordinate3)"/> returns the true vector projection
        /// b * (a·b / |b|²). This is a regression test for the bug where the scalar factor was multiplied
        /// by |b|² instead of divided by it.
        /// </summary>
        [Fact]
        public void Coordinate3_Project()
        {
            // Project a = (1,1,0) onto b = (2,0,0).
            // Expected: b * (a·b / |b|²) = (2,0,0) * (2 / 4) = (1,0,0).
            Coordinate3 b1 = new(2.0, 0.0, 0.0);
            Coordinate3 a1 = new(1.0, 1.0, 0.0);

            Coordinate3 projection1 = b1.Project(a1);

            Assert.Equal(1.0, projection1.X, 1e-9);
            Assert.Equal(0.0, projection1.Y, 1e-9);
            Assert.Equal(0.0, projection1.Z, 1e-9);

            // Project a = (3,4,5) onto b = (0,2,0).
            // Expected: b * (a·b / |b|²) = (0,2,0) * (8 / 4) = (0,4,0).
            Coordinate3 b2 = new(0.0, 2.0, 0.0);
            Coordinate3 a2 = new(3.0, 4.0, 5.0);

            Coordinate3 projection2 = b2.Project(a2);

            Assert.Equal(0.0, projection2.X, 1e-9);
            Assert.Equal(4.0, projection2.Y, 1e-9);
            Assert.Equal(0.0, projection2.Z, 1e-9);

            // Projecting onto a unit-length axis must leave the component unchanged.
            // Project a = (7,-3,2) onto unit z-axis (0,0,1) -> (0,0,2).
            Coordinate3 b3 = new(0.0, 0.0, 1.0);
            Coordinate3 a3 = new(7.0, -3.0, 2.0);

            Coordinate3 projection3 = b3.Project(a3);

            Assert.Equal(0.0, projection3.X, 1e-9);
            Assert.Equal(0.0, projection3.Y, 1e-9);
            Assert.Equal(2.0, projection3.Z, 1e-9);
        }
    }
}
