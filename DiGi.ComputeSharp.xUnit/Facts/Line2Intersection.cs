using DiGi.ComputeSharp.Core.Classes;
using DiGi.ComputeSharp.Core.Constants;
using DiGi.ComputeSharp.Planar.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Contains unit tests for the CPU-side 2D intersection kernel <see cref="Planar.Create"/>.Line2Intersection.
    /// These run without a GPU because the kernel is plain managed math shared with the compute shaders.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Two crossing bounded segments intersect at a single interior point.
        /// </summary>
        [Fact]
        public void Line2Intersection_LineLine()
        {
            double tolerance = Tolerance.Distance;

            Line2 line2_1 = new(new Coordinate2(0, 0), new Coordinate2(10, 0));
            Line2 line2_2 = new(new Coordinate2(1, -1), new Coordinate2(1, 1));

            Line2Intersection line2Intersection = Planar.Create.Line2Intersection(line2_1, line2_2, tolerance);

            Assert.False(line2Intersection.IsNaN());
            Assert.True(line2Intersection.Point_2.IsNaN());
            Assert.Equal(1.0, line2Intersection.Point_1.X, tolerance);
            Assert.Equal(0.0, line2Intersection.Point_1.Y, tolerance);
        }

        /// <summary>
        /// Two collinear overlapping segments intersect along the shared sub-segment, returned as two points.
        /// </summary>
        [Fact]
        public void Line2Intersection_Collinear()
        {
            double tolerance = Tolerance.Distance;

            Line2 line2_1 = new(new Coordinate2(0, 0), new Coordinate2(10, 0));
            Line2 line2_2 = new(new Coordinate2(5, 0), new Coordinate2(15, 0));

            Line2Intersection line2Intersection = Planar.Create.Line2Intersection(line2_1, line2_2, tolerance);

            Assert.False(line2Intersection.IsNaN());
            Assert.False(line2Intersection.Point_2.IsNaN());
            AssertUnorderedPair(new Coordinate2(5, 0), new Coordinate2(10, 0), line2Intersection.Point_1, line2Intersection.Point_2, tolerance);
        }

        /// <summary>
        /// Parallel but non-collinear segments do not intersect.
        /// </summary>
        [Fact]
        public void Line2Intersection_ParallelDisjoint()
        {
            double tolerance = Tolerance.Distance;

            Line2 line2_1 = new(new Coordinate2(0, 0), new Coordinate2(10, 0));
            Line2 line2_2 = new(new Coordinate2(0, 1), new Coordinate2(10, 1));

            Line2Intersection line2Intersection = Planar.Create.Line2Intersection(line2_1, line2_2, tolerance);

            Assert.True(line2Intersection.IsNaN());
        }

        /// <summary>
        /// A line passing through a triangle exits via two edges; the intersection is the chord between the crossings.
        /// </summary>
        [Fact]
        public void Line2Intersection_Triangle_Chord()
        {
            double tolerance = Tolerance.Distance;

            Triangle2 triangle2 = new(new Bool(true), new Coordinate2(0, 0), new Coordinate2(10, 0), new Coordinate2(5, 10));
            Line2 line2 = new(new Coordinate2(-5, 2), new Coordinate2(15, 2));

            Line2Intersection line2Intersection = Planar.Create.Line2Intersection(line2, triangle2, tolerance);

            Assert.False(line2Intersection.IsNaN());
            Assert.False(line2Intersection.Point_2.IsNaN());
            AssertUnorderedPair(new Coordinate2(1, 2), new Coordinate2(9, 2), line2Intersection.Point_1, line2Intersection.Point_2, tolerance);
        }

        /// <summary>
        /// A line whose bounding box is clear of the triangle returns no intersection.
        /// </summary>
        [Fact]
        public void Line2Intersection_Triangle_Miss()
        {
            double tolerance = Tolerance.Distance;

            Triangle2 triangle2 = new(new Bool(true), new Coordinate2(0, 0), new Coordinate2(10, 0), new Coordinate2(5, 10));
            Line2 line2 = new(new Coordinate2(-5, 20), new Coordinate2(15, 20));

            Line2Intersection line2Intersection = Planar.Create.Line2Intersection(line2, triangle2, tolerance);

            Assert.True(line2Intersection.IsNaN());
        }

        /// <summary>
        /// A segment lying entirely inside a solid triangle (no edge crossing) intersects it along the whole segment.
        /// Regression test for the fully-inside case that previously returned an empty result.
        /// </summary>
        [Fact]
        public void Line2Intersection_Triangle_FullyInside()
        {
            double tolerance = Tolerance.Distance;

            Triangle2 triangle2 = new(new Bool(true), new Coordinate2(0, 0), new Coordinate2(10, 0), new Coordinate2(5, 10));
            Line2 line2 = new(new Coordinate2(4, 2), new Coordinate2(6, 2));

            Line2Intersection line2Intersection = Planar.Create.Line2Intersection(line2, triangle2, tolerance);

            Assert.False(line2Intersection.IsNaN());
            Assert.False(line2Intersection.Point_2.IsNaN());
            Assert.True(line2Intersection.Solid.ToBool());
            AssertUnorderedPair(new Coordinate2(4, 2), new Coordinate2(6, 2), line2Intersection.Point_1, line2Intersection.Point_2, tolerance);
        }

        /// <summary>
        /// Asserts that the pair (<paramref name="actual_1"/>, <paramref name="actual_2"/>) equals the pair
        /// (<paramref name="expected_1"/>, <paramref name="expected_2"/>) regardless of ordering.
        /// </summary>
        /// <param name="expected_1">The first expected coordinate.</param>
        /// <param name="expected_2">The second expected coordinate.</param>
        /// <param name="actual_1">The first actual coordinate.</param>
        /// <param name="actual_2">The second actual coordinate.</param>
        /// <param name="tolerance">The comparison tolerance.</param>
        private static void AssertUnorderedPair(Coordinate2 expected_1, Coordinate2 expected_2, Coordinate2 actual_1, Coordinate2 actual_2, double tolerance)
        {
            bool sameOrder = expected_1.AlmostEquals(actual_1, tolerance) && expected_2.AlmostEquals(actual_2, tolerance);
            bool swappedOrder = expected_1.AlmostEquals(actual_2, tolerance) && expected_2.AlmostEquals(actual_1, tolerance);

            Assert.True(sameOrder || swappedOrder, $"Expected {{{expected_1};{expected_2}}} but got {{{actual_1};{actual_2}}}.");
        }
    }
}
