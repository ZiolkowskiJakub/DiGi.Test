using DiGi.ComputeSharp.Core.Constants;
using DiGi.ComputeSharp.Spatial.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Contains regression tests for the CPU-side line/triangle intersection kernel in 3D.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// A coplanar segment lying entirely inside a solid triangle (no edge crossing) intersects it along the
        /// whole segment. Regression test for the fully-inside case that previously returned an empty result.
        /// </summary>
        [Fact]
        public void Line3Intersection_Triangle_FullyInside()
        {
            double tolerance = Tolerance.Distance;

            Triangle3 triangle3 = new(new(true), 0, 0, 0, 10, 0, 0, 5, 10, 0);
            Line3 line3 = new(4, 2, 0, 6, 2, 0);

            Line3Intersection line3Intersection = Spatial.Create.Line3Intersection(line3, triangle3, tolerance);

            Assert.False(line3Intersection.IsNaN());
            Assert.False(line3Intersection.Point_2.IsNaN());
            Assert.True(line3Intersection.Solid.ToBool());

            Coordinate3 expected_1 = new(4, 2, 0);
            Coordinate3 expected_2 = new(6, 2, 0);

            bool sameOrder = expected_1.AlmostEquals(line3Intersection.Point_1, tolerance) && expected_2.AlmostEquals(line3Intersection.Point_2, tolerance);
            bool swappedOrder = expected_1.AlmostEquals(line3Intersection.Point_2, tolerance) && expected_2.AlmostEquals(line3Intersection.Point_1, tolerance);

            Assert.True(sameOrder || swappedOrder);
        }
    }
}
