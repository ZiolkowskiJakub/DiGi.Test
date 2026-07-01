using DiGi.ComputeSharp.Core.Classes;
using DiGi.ComputeSharp.Spatial.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Contains unit tests for the GPU point-in-triangle containment query.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Spatial.Query.Inside(System.Collections.Generic.IEnumerable{Coordinate3}, System.Collections.Generic.IEnumerable{Triangle3})"/>
        /// returns the containing triangle index, and -1 for points inside no triangle.
        /// Regression test for the sentinel bug where the result was initialised to 0, making a point inside
        /// triangle index 0 indistinguishable from a point inside nothing.
        /// Handles UnsupportedDoubleOperationException gracefully for FP64 unsupported GPUs.
        /// </summary>
        [Fact]
        public void Inside_IndexAndSentinel()
        {
            if (!Query.IsComputeSharpSupported(testOutputHelper))
            {
                return;
            }

            // Two coplanar (z = 0) triangles in well-separated XY regions.
            Triangle3[] triangles =
            [
                new Triangle3(new Bool(true),   0, 0, 0,  10, 0, 0,   0, 10, 0),
                new Triangle3(new Bool(true), 100, 0, 0, 110, 0, 0, 100, 10, 0),
            ];

            // p0 lies on triangle 0, p1 lies on triangle 1, p2 lies on the plane but inside no triangle.
            Coordinate3 pointInside0 = new(2, 2, 0);
            Coordinate3 pointInside1 = new(102, 2, 0);
            Coordinate3 pointOutside = new(1000, 1000, 0);

            Coordinate3[] points = [pointInside0, pointInside1, pointOutside];

            try
            {
                List<int>? result = Spatial.Query.Inside(points, triangles);

                Assert.NotNull(result);
                Assert.Equal(3, result!.Count);

                testOutputHelper.WriteLine($"[Inside] indexes = [{string.Join(", ", result)}]");

                Assert.Equal(0, result[0]);   // inside triangle 0
                Assert.Equal(1, result[1]);   // inside triangle 1
                Assert.Equal(-1, result[2]);  // inside nothing

                // The crux of the fix: "inside triangle 0" must be distinguishable from "inside nothing".
                Assert.NotEqual(result[0], result[2]);
            }
            catch (Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
            {
                testOutputHelper.WriteLine("WARNING: GPU FP64 double-precision operations are not supported on this graphics card: " + exception.Message);
            }
        }

        /// <summary>
        /// Verifies the single-point <see cref="Spatial.Query.Inside(Coordinate3, System.Collections.Generic.IEnumerable{Triangle3})"/>
        /// overload returns the triangle index for a contained point and -1 for an uncontained one.
        /// Handles UnsupportedDoubleOperationException gracefully for FP64 unsupported GPUs.
        /// </summary>
        [Fact]
        public void Inside_SinglePoint()
        {
            if (!Query.IsComputeSharpSupported(testOutputHelper))
            {
                return;
            }

            Triangle3[] triangles =
            [
                new Triangle3(new Bool(true),   0, 0, 0,  10, 0, 0,   0, 10, 0),
                new Triangle3(new Bool(true), 100, 0, 0, 110, 0, 0, 100, 10, 0),
            ];

            try
            {
                // A point on triangle index 0 must report 0, not -1.
                Assert.Equal(0, Spatial.Query.Inside(new Coordinate3(2, 2, 0), triangles));

                // A point off every triangle must report -1.
                Assert.Equal(-1, Spatial.Query.Inside(new Coordinate3(5, 5, 100), triangles));
            }
            catch (Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
            {
                testOutputHelper.WriteLine("WARNING: GPU FP64 double-precision operations are not supported on this graphics card: " + exception.Message);
            }
        }
    }
}