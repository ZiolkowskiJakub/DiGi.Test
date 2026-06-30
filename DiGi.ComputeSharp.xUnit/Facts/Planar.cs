using DiGi.ComputeSharp.Core.Classes;
using DiGi.ComputeSharp.Core.Constants;
using DiGi.ComputeSharp.Planar.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Unit tests for the planar (2D) geometry primitives.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Triangle2.IsNaN must detect a NaN in any of the three vertices. Regression test for the bug
        /// where the second term re-checked Point_1 instead of Point_2.
        /// </summary>
        [Fact]
        public void Triangle2_IsNaN_DetectsEachVertex()
        {
            Coordinate2 a = new(0, 0);
            Coordinate2 b = new(1, 0);
            Coordinate2 c = new(0, 1);

            Assert.False(new Triangle2(new Bool(true), a, b, c).IsNaN());
            Assert.True(new Triangle2(new Bool(true), new Coordinate2(), b, c).IsNaN());  // Point_1 NaN
            Assert.True(new Triangle2(new Bool(true), a, new Coordinate2(), c).IsNaN());  // Point_2 NaN
            Assert.True(new Triangle2(new Bool(true), a, b, new Coordinate2()).IsNaN());  // Point_3 NaN
        }

        /// <summary>
        /// Coordinate2.Project must return the true vector projection b * (a·b / |b|²).
        /// Regression test for the scalar factor being multiplied by |b|² instead of divided.
        /// </summary>
        [Fact]
        public void Coordinate2_Project()
        {
            // Project (1,1) onto (2,0) -> (2,0) * (2/4) = (1,0).
            Coordinate2 projection_1 = new Coordinate2(2, 0).Project(new Coordinate2(1, 1));
            Assert.Equal(1.0, projection_1.X, 1e-9);
            Assert.Equal(0.0, projection_1.Y, 1e-9);

            // Project (3,4) onto (0,2) -> (0,2) * (8/4) = (0,4).
            Coordinate2 projection_2 = new Coordinate2(0, 2).Project(new Coordinate2(3, 4));
            Assert.Equal(0.0, projection_2.X, 1e-9);
            Assert.Equal(4.0, projection_2.Y, 1e-9);
        }

        /// <summary>
        /// Triangle2.Inside (and Triangle2.On for solid triangles, which delegates to it) must perform a real
        /// point-in-triangle test instead of throwing NotImplementedException.
        /// </summary>
        [Fact]
        public void Triangle2_Inside()
        {
            double tolerance = Tolerance.Distance;

            Triangle2 triangle = new(new Bool(true), new Coordinate2(0, 0), new Coordinate2(10, 0), new Coordinate2(0, 10));

            Assert.True(triangle.Inside(new Coordinate2(2, 2), tolerance));    // interior
            Assert.True(triangle.Inside(new Coordinate2(5, 5), tolerance));    // on the hypotenuse edge
            Assert.False(triangle.Inside(new Coordinate2(6, 6), tolerance));   // outside (x + y > 10)
            Assert.False(triangle.Inside(new Coordinate2(20, 20), tolerance)); // far outside

            // On() of a solid triangle delegates to Inside and must not throw.
            Assert.True(triangle.On(new Coordinate2(2, 2), tolerance));
            Assert.False(triangle.On(new Coordinate2(20, 20), tolerance));
        }
    }
}
