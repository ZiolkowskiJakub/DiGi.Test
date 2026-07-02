using DiGi.ComputeSharp.Core.Classes;
using DiGi.ComputeSharp.Core.Constants;
using DiGi.ComputeSharp.Planar.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Contains unit tests for the GPU-accelerated 2D intersection host wrappers.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Tests the intersection of one 2D line against a collection of other 2D lines on the GPU.
        /// Handles UnsupportedDoubleOperationException gracefully for FP64 unsupported GPUs.
        /// </summary>
        [Fact]
        public void Line2Intersections_LineVsLines()
        {
            if (!Query.IsComputeSharpSupported(testOutputHelper))
            {
                return;
            }

            Line2 line2_1 = new(new Coordinate2(0, 0), new Coordinate2(10, 0));
            Line2 line2_2 = new(new Coordinate2(1, -1), new Coordinate2(1, 1));
            Line2[] line2s_Target = [line2_2];

            try
            {
                IEnumerable<Line2Intersection>? line2Intersections = Planar.Create.Line2Intersections(line2_1, line2s_Target, Tolerance.Distance);

                Assert.NotNull(line2Intersections);
                Line2Intersection line2Intersection_First = line2Intersections!.First();

                Assert.False(line2Intersection_First.IsNaN());
                Assert.Equal(1.0, line2Intersection_First.Point_1.X, Tolerance.Distance);
                Assert.Equal(0.0, line2Intersection_First.Point_1.Y, Tolerance.Distance);
            }
            catch (Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
            {
                testOutputHelper.WriteLine("WARNING: GPU FP64 double-precision operations are not supported on this graphics card: " + exception.Message);
            }
        }

        /// <summary>
        /// Tests the GPU grid intersection of a collection of 2D lines against a collection of 2D triangles.
        /// Handles UnsupportedDoubleOperationException gracefully for FP64 unsupported GPUs.
        /// </summary>
        [Fact]
        public void Line2Intersections_LinesVsTriangles()
        {
            if (!Query.IsComputeSharpSupported(testOutputHelper))
            {
                return;
            }

            Triangle2 triangle2 = new(new Bool(true), new Coordinate2(0, 0), new Coordinate2(10, 0), new Coordinate2(5, 10));
            Line2 line2 = new(new Coordinate2(-5, 2), new Coordinate2(15, 2));

            Line2[] line2s = [line2];
            Triangle2[] triangle2s = [triangle2];

            try
            {
                IEnumerable<Line2Intersection>? line2Intersections = Planar.Create.Line2Intersections(line2s, triangle2s, Tolerance.Distance);

                Assert.NotNull(line2Intersections);
                Line2Intersection line2Intersection_First = line2Intersections!.First();

                Assert.False(line2Intersection_First.IsNaN());
                Assert.False(line2Intersection_First.Point_2.IsNaN());

                bool sameOrder = new Coordinate2(1, 2).AlmostEquals(line2Intersection_First.Point_1, Tolerance.Distance) && new Coordinate2(9, 2).AlmostEquals(line2Intersection_First.Point_2, Tolerance.Distance);
                bool swappedOrder = new Coordinate2(1, 2).AlmostEquals(line2Intersection_First.Point_2, Tolerance.Distance) && new Coordinate2(9, 2).AlmostEquals(line2Intersection_First.Point_1, Tolerance.Distance);
                Assert.True(sameOrder || swappedOrder);
            }
            catch (Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
            {
                testOutputHelper.WriteLine("WARNING: GPU FP64 double-precision operations are not supported on this graphics card: " + exception.Message);
            }
        }
    }
}
