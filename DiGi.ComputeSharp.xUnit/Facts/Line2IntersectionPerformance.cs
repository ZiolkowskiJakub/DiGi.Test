using ComputeSharp;
using DiGi.ComputeSharp.Planar.Classes;
using DiGi.Geometry.Planar.Classes;
using System.Diagnostics;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Benchmarks the GPU 2D line intersection kernel (<c>DiGi.ComputeSharp</c>) against the native
    /// single-threaded CPU library (<c>DiGi.Geometry.Planar</c>) across a range of segment counts.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// The segment counts benchmarked, smallest to largest. Edit this list to change the sweep.
        /// </summary>
        private static readonly int[] Line2Intersection_Performance_SegmentCounts = [1_000, 10_000, 100_000, 1_000_000];

        /// <summary>
        /// For each segment count, intersects one reference segment against that many random segments on the GPU
        /// (end-to-end: buffer upload, dispatch, read-back) and on the native CPU library (single-threaded loop),
        /// then writes both timings and their ratio to the test output. A warm-up run precedes the measured runs so
        /// GPU shader compilation and JIT costs are excluded. Handles UnsupportedDoubleOperationException gracefully
        /// for FP64 unsupported GPUs.
        /// </summary>
        [Fact]
        public void Line2Intersection_Performance()
        {
            if (!Query.IsComputeSharpSupported(testOutputHelper))
            {
                return;
            }

            double tolerance = Core.Constants.Tolerance.Distance;
            const double domain = 100.0;

            Random random = new(20260702);

            Line2 reference_Line2 = new(new Coordinate2(-1.5 * domain, 0.0), new Coordinate2(1.5 * domain, 0.0));
            Segment2D reference_Segment2D = new(-1.5 * domain, 0.0, 1.5 * domain, 0.0);

            int maxCount = 0;
            foreach (int count in Line2Intersection_Performance_SegmentCounts)
            {
                if (count > maxCount)
                {
                    maxCount = count;
                }
            }

            // One shared random pool sized to the largest sweep entry; each count uses the first N of it.
            Line2[] line2s_All = new Line2[maxCount];
            Segment2D[] segment2Ds_All = new Segment2D[maxCount];
            for (int i = 0; i < maxCount; i++)
            {
                double x_1 = (random.NextDouble() * 2.0 * domain) - domain;
                double y_1 = (random.NextDouble() * 2.0 * domain) - domain;
                double x_2 = (random.NextDouble() * 2.0 * domain) - domain;
                double y_2 = (random.NextDouble() * 2.0 * domain) - domain;

                line2s_All[i] = new Line2(new Coordinate2(x_1, y_1), new Coordinate2(x_2, y_2));
                segment2Ds_All[i] = new Segment2D(x_1, y_1, x_2, y_2);
            }

            try
            {
                using GraphicsDevice graphicsDevice = GraphicsDevice.GetDefault();

                // Warm-up: force HLSL compilation / JIT on a small batch so it is excluded from the measured runs.
                int warmUpCount = Math.Min(256, maxCount);
                RunGpu(graphicsDevice, reference_Line2, line2s_All, warmUpCount, tolerance);
                RunNative(reference_Segment2D, segment2Ds_All, warmUpCount, tolerance);
                RunNativeParallel(reference_Segment2D, segment2Ds_All, warmUpCount, tolerance);

                testOutputHelper.WriteLine($"processor count: {Environment.ProcessorCount}");
                testOutputHelper.WriteLine($"{"segments",12} | {"GPU (ms)",10} | {"CPU 1T (ms)",12} | {"CPU MT (ms)",12} | {"GPU vs 1T",10} | {"GPU vs MT",10}");
                testOutputHelper.WriteLine(new string('-', 82));

                foreach (int count in Line2Intersection_Performance_SegmentCounts)
                {
                    Stopwatch stopwatch_Gpu = Stopwatch.StartNew();
                    int gpuHits = RunGpu(graphicsDevice, reference_Line2, line2s_All, count, tolerance);
                    stopwatch_Gpu.Stop();

                    Stopwatch stopwatch_Cpu = Stopwatch.StartNew();
                    int cpuHits = RunNative(reference_Segment2D, segment2Ds_All, count, tolerance);
                    stopwatch_Cpu.Stop();

                    Stopwatch stopwatch_CpuParallel = Stopwatch.StartNew();
                    int cpuParallelHits = RunNativeParallel(reference_Segment2D, segment2Ds_All, count, tolerance);
                    stopwatch_CpuParallel.Stop();

                    double gpuMilliseconds = stopwatch_Gpu.Elapsed.TotalMilliseconds;
                    double cpuMilliseconds = stopwatch_Cpu.Elapsed.TotalMilliseconds;
                    double cpuParallelMilliseconds = stopwatch_CpuParallel.Elapsed.TotalMilliseconds;

                    double speedUp_SingleThread = gpuMilliseconds > 0.0 ? cpuMilliseconds / gpuMilliseconds : double.NaN;
                    double speedUp_MultiThread = gpuMilliseconds > 0.0 ? cpuParallelMilliseconds / gpuMilliseconds : double.NaN;

                    testOutputHelper.WriteLine($"{count,12:N0} | {gpuMilliseconds,10:F2} | {cpuMilliseconds,12:F2} | {cpuParallelMilliseconds,12:F2} | {speedUp_SingleThread,9:F2}x | {speedUp_MultiThread,9:F2}x");

                    // All back-ends must classify roughly the same number of crossings (they can differ only on the
                    // measure-zero near-endpoint band), otherwise a code change has skewed one of them.
                    int tolerantBand = (count / 1000) + 8;
                    Assert.InRange(gpuHits, cpuHits - tolerantBand, cpuHits + tolerantBand);
                    Assert.Equal(cpuHits, cpuParallelHits);
                }
            }
            catch (Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
            {
                testOutputHelper.WriteLine("WARNING: GPU FP64 double-precision operations are not supported on this graphics card: " + exception.Message);
            }
        }

        /// <summary>
        /// Runs the GPU intersection of the reference line against the first <paramref name="count"/> segments,
        /// end-to-end (buffer upload, dispatch, read-back), and returns the number of on-segment single-point hits.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to dispatch on.</param>
        /// <param name="reference_Line2">The reference line intersected against every segment.</param>
        /// <param name="line2s_All">The shared pool of candidate lines.</param>
        /// <param name="count">The number of leading segments to process.</param>
        /// <param name="tolerance">The intersection tolerance.</param>
        /// <returns>The number of segments that produced a single on-segment intersection point.</returns>
        private static int RunGpu(GraphicsDevice graphicsDevice, Line2 reference_Line2, Line2[] line2s_All, int count, double tolerance)
        {
            Line2[] line2s = new Line2[count];
            Array.Copy(line2s_All, line2s, count);

            Line2Intersection[] line2Intersections = new Line2Intersection[count];

            using (ReadOnlyBuffer<Line2> linesBuffer = graphicsDevice.AllocateReadOnlyBuffer(line2s))
            using (ReadWriteBuffer<Line2Intersection> intersectionsBuffer = graphicsDevice.AllocateReadWriteBuffer(new Line2Intersection[count]))
            {
                graphicsDevice.For(count, new Line2IntersectionComputeShader(reference_Line2, linesBuffer, intersectionsBuffer, tolerance));
                intersectionsBuffer.CopyTo(line2Intersections);
            }

            int hits = 0;
            for (int i = 0; i < count; i++)
            {
                Line2Intersection line2Intersection = line2Intersections[i];
                if (!line2Intersection.IsNaN() && line2Intersection.Point_2.IsNaN())
                {
                    hits++;
                }
            }

            return hits;
        }

        /// <summary>
        /// Runs the native single-threaded intersection of the reference segment against the first
        /// <paramref name="count"/> segments and returns the number of on-segment intersection points.
        /// </summary>
        /// <param name="reference_Segment2D">The reference segment intersected against every segment.</param>
        /// <param name="segment2Ds_All">The shared pool of candidate segments.</param>
        /// <param name="count">The number of leading segments to process.</param>
        /// <param name="tolerance">The intersection tolerance.</param>
        /// <returns>The number of segments that produced an on-segment intersection point.</returns>
        private static int RunNative(Segment2D reference_Segment2D, Segment2D[] segment2Ds_All, int count, double tolerance)
        {
            int hits = 0;
            for (int i = 0; i < count; i++)
            {
                Point2D? point2D = DiGi.Geometry.Planar.Query.IntersectionPoint(
                    reference_Segment2D[0], reference_Segment2D[1], segment2Ds_All[i][0], segment2Ds_All[i][1],
                    out Point2D? point2D_Closest_1, out Point2D? point2D_Closest_2, tolerance);

                if (point2D != null && point2D_Closest_1 == null && point2D_Closest_2 == null)
                {
                    hits++;
                }
            }

            return hits;
        }

        /// <summary>
        /// Runs the native intersection of the reference segment against the first <paramref name="count"/> segments
        /// across all available CPU cores using <see cref="Parallel.For(int, int, Func{int, System.Threading.Tasks.ParallelLoopState, int, int}, Func{int, int})"/>
        /// thread-local accumulation, and returns the number of on-segment intersection points.
        /// </summary>
        /// <param name="reference_Segment2D">The reference segment intersected against every segment.</param>
        /// <param name="segment2Ds_All">The shared pool of candidate segments.</param>
        /// <param name="count">The number of leading segments to process.</param>
        /// <param name="tolerance">The intersection tolerance.</param>
        /// <returns>The number of segments that produced an on-segment intersection point.</returns>
        private static int RunNativeParallel(Segment2D reference_Segment2D, Segment2D[] segment2Ds_All, int count, double tolerance)
        {
            Point2D? reference_Start = reference_Segment2D[0];
            Point2D? reference_End = reference_Segment2D[1];

            int hits = 0;

            Parallel.For(0, count, () => 0, (i, loopState, localHits) =>
            {
                Point2D? point2D = DiGi.Geometry.Planar.Query.IntersectionPoint(
                    reference_Start, reference_End, segment2Ds_All[i][0], segment2Ds_All[i][1],
                    out Point2D? point2D_Closest_1, out Point2D? point2D_Closest_2, tolerance);

                if (point2D != null && point2D_Closest_1 == null && point2D_Closest_2 == null)
                {
                    localHits++;
                }

                return localHits;
            }, localHits => Interlocked.Add(ref hits, localHits));

            return hits;
        }
    }
}