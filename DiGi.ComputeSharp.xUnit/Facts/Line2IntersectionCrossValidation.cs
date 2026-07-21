using ComputeSharp;
using DiGi.ComputeSharp.Planar.Classes;
using DiGi.Geometry.Planar.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Cross-validates the GPU 2D line intersection kernel against the native CPU
    /// <c>DiGi.Geometry.Planar</c> segment intersection library over a batch of random segments.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// The single knob for this test: how many random segments are intersected against the reference
        /// segment on both the GPU and the native CPU library. Raise it (e.g. 1_000_000) for a one-off
        /// stress run, then lower it back for fast everyday test runs.
        /// </summary>
        private const int Line2Intersection_CrossValidation_SegmentCount = 1000;

        /// <summary>
        /// Intersects one reference segment against <see cref="Line2Intersection_CrossValidation_SegmentCount"/>
        /// random segments on the GPU (<see cref="Line2IntersectionComputeShader"/>) and on the native
        /// <c>DiGi.Geometry.Planar</c> library, then asserts both agree per segment. Crossings that fall within a
        /// guard band of any endpoint are skipped as numerically ambiguous (the two implementations use slightly
        /// different on-segment acceptance criteria there). Handles UnsupportedDoubleOperationException gracefully
        /// for FP64 unsupported GPUs.
        /// </summary>
        [Fact]
        public void Line2Intersection_GpuVsNative()
        {
            if (!Query.IsComputeSharpSupported(testOutputHelper))
            {
                return;
            }

            int segmentCount = Line2Intersection_CrossValidation_SegmentCount;
            Assert.True(segmentCount > 0);

            double tolerance = Core.Constants.Tolerance.Distance;

            // Comparison tolerance for matching two intersection points. Both sides solve the same FP64 linear
            // system so agreement is far tighter than this, but a relaxed band keeps the test stable.
            double comparisonTolerance = 1e-6;

            // Crossings closer than this to any of the four endpoints are skipped: exactly there the GPU
            // (perpendicular-distance On test) and the native library (parametric [0,1] test) can legitimately
            // disagree on whether the point is "on" the bounded segment. This region is measure-zero for random
            // input, so skipping it does not weaken coverage.
            double endpointGuardBand = 1e-3;

            // Deterministic input so a failing stress run can be reproduced.
            Random random = new(20260702);

            const double domain = 100.0;

            // Reference: a long horizontal segment through the origin; roughly half the random segments straddle
            // it and produce a genuine crossing, the rest miss.
            Line2 reference_Line2 = new(new Coordinate2(-1.5 * domain, 0.0), new Coordinate2(1.5 * domain, 0.0));
            Segment2D reference_Segment2D = new(-1.5 * domain, 0.0, 1.5 * domain, 0.0);

            Line2[] line2s = new Line2[segmentCount];
            Segment2D[] segment2Ds = new Segment2D[segmentCount];

            for (int i = 0; i < segmentCount; i++)
            {
                double x_1 = (random.NextDouble() * 2.0 * domain) - domain;
                double y_1 = (random.NextDouble() * 2.0 * domain) - domain;
                double x_2 = (random.NextDouble() * 2.0 * domain) - domain;
                double y_2 = (random.NextDouble() * 2.0 * domain) - domain;

                line2s[i] = new Line2(new Coordinate2(x_1, y_1), new Coordinate2(x_2, y_2));
                segment2Ds[i] = new Segment2D(x_1, y_1, x_2, y_2);
            }

            Line2Intersection[] line2Intersections = new Line2Intersection[segmentCount];

            try
            {
                using GraphicsDevice graphicsDevice = GraphicsDevice.GetDefault();

                using ReadOnlyBuffer<Line2> linesBuffer = graphicsDevice.AllocateReadOnlyBuffer(line2s);
                using ReadWriteBuffer<Line2Intersection> intersectionsBuffer = graphicsDevice.AllocateReadWriteBuffer(new Line2Intersection[segmentCount]);

                graphicsDevice.For(segmentCount, new Line2IntersectionComputeShader(reference_Line2, linesBuffer, intersectionsBuffer, tolerance));

                intersectionsBuffer.CopyTo(line2Intersections);
            }
            catch (Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
            {
                testOutputHelper.WriteLine("WARNING: GPU FP64 double-precision operations are not supported on this graphics card: " + exception.Message);
                return;
            }

            int mismatchCount = 0;
            int agreedCrossingCount = 0;
            int agreedMissCount = 0;
            int skippedAmbiguousCount = 0;
            int skippedCollinearCount = 0;

            for (int i = 0; i < segmentCount; i++)
            {
                Line2Intersection line2Intersection = line2Intersections[i];
                Segment2D segment2D = segment2Ds[i];

                bool gpu_HasPoint = !line2Intersection.IsNaN() && line2Intersection.Point_2.IsNaN();
                bool gpu_IsCollinear = !line2Intersection.IsNaN() && !line2Intersection.Point_2.IsNaN();

                if (gpu_IsCollinear)
                {
                    // Collinear overlap: the native single-point API cannot represent a shared sub-segment.
                    skippedCollinearCount++;
                    continue;
                }

                Point2D? native_Point = DiGi.Geometry.Planar.Query.IntersectionPoint(
                    reference_Segment2D[0], reference_Segment2D[1], segment2D[0], segment2D[1],
                    out Point2D? native_Closest_1, out Point2D? native_Closest_2, tolerance);

                bool native_HasPoint = native_Point != null && native_Closest_1 == null && native_Closest_2 == null;

                if (native_Point == null)
                {
                    // Native treats the pair as parallel/degenerate (no unique crossing). The GPU shares the same
                    // determinant test, so it must not report an on-segment crossing here.
                    if (gpu_HasPoint)
                    {
                        mismatchCount++;
                    }
                    else
                    {
                        agreedMissCount++;
                    }

                    continue;
                }

                // Guard band around the four endpoints (ambiguous on-segment acceptance).
                if (IsNearAnyEndpoint(native_Point!, reference_Segment2D[0]!, reference_Segment2D[1]!, segment2D[0]!, segment2D[1]!, endpointGuardBand))
                {
                    skippedAmbiguousCount++;
                    continue;
                }

                if (gpu_HasPoint != native_HasPoint)
                {
                    mismatchCount++;
                    continue;
                }

                if (gpu_HasPoint && native_HasPoint)
                {
                    if (Math.Abs(line2Intersection.Point_1.X - native_Point!.X) > comparisonTolerance ||
                        Math.Abs(line2Intersection.Point_1.Y - native_Point!.Y) > comparisonTolerance)
                    {
                        mismatchCount++;
                        continue;
                    }

                    agreedCrossingCount++;
                }
                else
                {
                    agreedMissCount++;
                }
            }

            testOutputHelper.WriteLine($"segments={segmentCount}, crossings={agreedCrossingCount}, misses={agreedMissCount}, " +
                $"ambiguousSkipped={skippedAmbiguousCount}, collinearSkipped={skippedCollinearCount}, mismatches={mismatchCount}");

            Assert.Equal(0, mismatchCount);

            // Sanity: the random layout must actually exercise on-segment crossings, otherwise the comparison is vacuous.
            Assert.True(agreedCrossingCount > 0, "Expected at least one genuine on-segment crossing to compare.");
        }

        /// <summary>
        /// Determines whether a candidate point lies within a guard band of any of the four supplied endpoints.
        /// </summary>
        /// <param name="point">The candidate intersection point.</param>
        /// <param name="endpoint_1">The first endpoint to test against.</param>
        /// <param name="endpoint_2">The second endpoint to test against.</param>
        /// <param name="endpoint_3">The third endpoint to test against.</param>
        /// <param name="endpoint_4">The fourth endpoint to test against.</param>
        /// <param name="band">The guard-band distance.</param>
        /// <returns>True if the point is within <paramref name="band"/> of any endpoint; otherwise, false.</returns>
        private static bool IsNearAnyEndpoint(Point2D point, Point2D endpoint_1, Point2D endpoint_2, Point2D endpoint_3, Point2D endpoint_4, double band)
        {
            return IsNear(point, endpoint_1, band) || IsNear(point, endpoint_2, band) || IsNear(point, endpoint_3, band) || IsNear(point, endpoint_4, band);
        }

        /// <summary>
        /// Determines whether two points are within a given axis-aligned band of each other.
        /// </summary>
        /// <param name="point_1">The first point.</param>
        /// <param name="point_2">The second point.</param>
        /// <param name="band">The band distance applied independently to each axis.</param>
        /// <returns>True if both the X and Y separations are within <paramref name="band"/>; otherwise, false.</returns>
        private static bool IsNear(Point2D point_1, Point2D point_2, double band)
        {
            return Math.Abs(point_1.X - point_2.X) <= band && Math.Abs(point_1.Y - point_2.Y) <= band;
        }
    }
}