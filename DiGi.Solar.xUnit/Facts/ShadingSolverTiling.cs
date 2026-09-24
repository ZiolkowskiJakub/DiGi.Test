using DiGi.ComputeSharp.Spatial.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Solar.Classes;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that tiling the ComputeSharp solver into receiver row blocks (ZiolkowskiJakub/DiGi.Solar#6) does not change its results: the same model solved in one block, in blocks of one row and in blocks of seven rows (leaving a partial last block) gives identical shading factors for every receiver at every timestamp.
        /// <para>Two models cover the two dispatch passes: a 4 x 4 grid of buildings whose walls and roofs shade each other (self-shading pass only) and horizontal panels under shading-only canopies (external pass only; the panels are coplanar, so they never shade each other).
        /// Each model must have partially shaded samples, so the comparison is not vacuous, and the one-row solve of the building grid is also compared with the CPU solver.</para>
        /// </summary>
        [Fact]
        [SupportedOSPlatform("windows")]
        public void ShadingSolver_Solve_TiledEqualsUntiled()
        {
            if (!IsComputeSharpSupported(testOutputHelper))
            {
                testOutputHelper.WriteLine("Skipping ShadingSolver_Solve_TiledEqualsUntiled because ComputeSharp is not supported on this machine.");
                return;
            }

            DateTime[] dateTimes = CreateDaytimeSeries(60);

            testOutputHelper.WriteLine($"Unsafe.SizeOf<Triangle3Intersection>() = {Unsafe.SizeOf<Triangle3Intersection>()} B");

            ShadingModel shadingModel_Grid = CreateBuildingGridShadingModel(4, false);
            ShadingModel shadingModel_Grid_OneRow = AssertTiledEqualsUntiled(shadingModel_Grid, dateTimes, "building grid");

            ShadingModel shadingModel_CPU = new(shadingModel_Grid);
            Assert.True(new ShadingSolver(shadingModel_CPU, dateTimes).Solve());
            AssertSameShadingFactors(shadingModel_CPU, shadingModel_Grid_OneRow, dateTimes);

            AssertTiledEqualsUntiled(CreatePerformanceShadingModel(4, 3), dateTimes, "panels under canopies");
        }

        /// <summary>
        /// Solves a model with the ComputeSharp solver on a model that would not fit the untiled solver, and reports the time and memory of tiled solves.
        /// <para>4 800 receiver triangles under shading-only canopies (<c>CreatePerformanceShadingModel(60, 40)</c>) would need about 3.4 GB for each of the untiled N x N device buffer and its managed readback; at the default <see cref="ComputeSharp.Classes.ShadingSolver.MaxBufferBytes"/> the solve runs in blocks.
        /// The fact also times the 400-triangle performance model at the default budget (one block) against a budget forcing about eight blocks.
        /// Figures are written to <c>ShadingSolver_Tiling.txt</c> in the reports directory. The thresholds are loose: this fact reports, it does not enforce a tight bound.</para>
        /// </summary>
        [Fact]
        [SupportedOSPlatform("windows")]
        public void ShadingSolver_Solve_LargeModel()
        {
            if (!IsComputeSharpSupported(testOutputHelper))
            {
                testOutputHelper.WriteLine("Skipping ShadingSolver_Solve_LargeModel because ComputeSharp is not supported on this machine.");
                return;
            }

            List<string> lines = [$"Unsafe.SizeOf<Triangle3Intersection>() = {Unsafe.SizeOf<Triangle3Intersection>()} B"];

            // Warm-up: JIT and shader compilation of both row-offset shaders.
            SolveTiled(CreatePerformanceShadingModel(2, 2), [new DateTime(2026, 6, 26, 12, 0, 0)], long.MaxValue);

            ShadingModel shadingModel_Budget = CreatePerformanceShadingModel(20, 10);
            DateTime[] dateTimes_Budget = CreateDaytimeSeries(60);
            (int count_Triangle_Budget, int columns_Budget) = TriangleCounts(shadingModel_Budget);
            long maxBufferBytes_Default = new ComputeSharp.Classes.ShadingSolver(shadingModel_Budget, dateTimes_Budget).MaxBufferBytes;
            long maxBufferBytes_EightBlocks = (long)Unsafe.SizeOf<Triangle3Intersection>() * columns_Budget * (long)Math.Ceiling(count_Triangle_Budget / 8.0);

            lines.Add($"Budget comparison: {count_Triangle_Budget} receiver triangles, {columns_Budget} columns, {dateTimes_Budget.Length} timestamps");
            lines.Add("Run | Default budget (ms) | ~8 blocks (ms)");
            for (int run = 1; run <= 3; run++)
            {
                double milliseconds_Default = MeasureTiled(shadingModel_Budget, dateTimes_Budget, maxBufferBytes_Default);
                double milliseconds_EightBlocks = MeasureTiled(shadingModel_Budget, dateTimes_Budget, maxBufferBytes_EightBlocks);

                string line = $"{run} | {milliseconds_Default:F1} | {milliseconds_EightBlocks:F1}";
                lines.Add(line);
                testOutputHelper.WriteLine(line);
            }

            ShadingModel shadingModel_Large = CreatePerformanceShadingModel(60, 40);
            DateTime[] dateTimes_Large = [new DateTime(2026, 6, 26, 9, 0, 0), new DateTime(2026, 6, 26, 12, 0, 0), new DateTime(2026, 6, 26, 15, 0, 0)];
            (int count_Triangle_Large, int columns_Large) = TriangleCounts(shadingModel_Large);

            ComputeSharp.Classes.ShadingSolver shadingSolver_Large = new(shadingModel_Large, dateTimes_Large);
            long bytes_Untiled = (long)Unsafe.SizeOf<Triangle3Intersection>() * count_Triangle_Large * count_Triangle_Large;
            long bytes_Row = (long)Unsafe.SizeOf<Triangle3Intersection>() * columns_Large;
            long blockSize_Large = Math.Clamp(shadingSolver_Large.MaxBufferBytes / bytes_Row, 1L, count_Triangle_Large);

            GC.Collect();
            long bytes_ManagedBefore = GC.GetTotalMemory(true);

            Stopwatch stopwatch = Stopwatch.StartNew();
            bool isSolved = shadingSolver_Large.Solve();
            stopwatch.Stop();

            long bytes_PeakWorkingSet = Process.GetCurrentProcess().PeakWorkingSet64;

            Assert.True(isSolved);

            List<ShadingElement>? receivers = shadingModel_Large.GetShadingElements<ShadingElement>(shadingOnly: false);
            Assert.NotNull(receivers);
            int count_Shaded = receivers.Count(receiver => shadingModel_Large.TryGetShadingFactor(receiver, dateTimes_Large[1], out double factor, false) && factor > 0.0);
            Assert.True(count_Shaded > 0, "No receiver of the large model is shaded at noon.");

            lines.Add($"Large model: {count_Triangle_Large} receiver triangles, {columns_Large} columns, {dateTimes_Large.Length} timestamps");
            lines.Add($"Untiled N x N buffer would be {bytes_Untiled / (1024.0 * 1024.0):F0} MB; default budget {shadingSolver_Large.MaxBufferBytes / (1024.0 * 1024.0):F0} MB gives blocks of {blockSize_Large} rows ({(long)Math.Ceiling(count_Triangle_Large / (double)blockSize_Large)} blocks per pass)");
            lines.Add($"Solve {stopwatch.ElapsedMilliseconds} ms; {count_Shaded} of {receivers.Count} receivers shaded at noon");
            lines.Add($"Managed heap before solve {bytes_ManagedBefore / (1024.0 * 1024.0):F0} MB; process peak working set {bytes_PeakWorkingSet / (1024.0 * 1024.0):F0} MB (whole test process)");

            foreach (string line in lines)
            {
                testOutputHelper.WriteLine(line);
            }

            System.IO.File.WriteAllLines(System.IO.Path.Combine(Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly())!, "ShadingSolver_Tiling.txt"), lines);

            Assert.True(stopwatch.ElapsedMilliseconds < 600000, $"The tiled solve of {count_Triangle_Large} receiver triangles took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Solves copies of a model with the ComputeSharp solver in one block, in blocks of one row and in blocks of seven rows, and asserts every receiver gets the identical shading factor at every timestamp in all three.
        /// </summary>
        /// <param name="shadingModel">The model to solve; it is copied, never solved itself.</param>
        /// <param name="dateTimes">The date-times to solve.</param>
        /// <param name="name">The model name used in the test output.</param>
        /// <returns>The copy solved in blocks of one row.</returns>
        [SupportedOSPlatform("windows")]
        private ShadingModel AssertTiledEqualsUntiled(ShadingModel shadingModel, DateTime[] dateTimes, string name)
        {
            (int count_Triangle, int columns) = TriangleCounts(shadingModel);

            // Seven rows per block must leave a partial last block, or the guard of the row-offset shaders goes untested.
            Assert.True(count_Triangle > 7 && count_Triangle % 7 != 0, $"{name}: {count_Triangle} receiver triangles do not leave a partial block of 7 rows.");

            ShadingModel shadingModel_OneBlock = SolveTiled(shadingModel, dateTimes, long.MaxValue);
            ShadingModel shadingModel_OneRow = SolveTiled(shadingModel, dateTimes, 1);
            ShadingModel shadingModel_SevenRows = SolveTiled(shadingModel, dateTimes, (long)Unsafe.SizeOf<Triangle3Intersection>() * columns * 7);

            List<ShadingElement>? receivers = shadingModel_OneBlock.GetShadingElements<ShadingElement>(shadingOnly: false);
            Assert.NotNull(receivers);
            Assert.NotEmpty(receivers);

            int count = 0;
            int count_Partial = 0;
            foreach (ShadingElement receiver in receivers)
            {
                foreach (DateTime dateTime in dateTimes)
                {
                    bool hasFactor_OneBlock = shadingModel_OneBlock.TryGetShadingFactor(receiver, dateTime, out double factor_OneBlock, false);
                    bool hasFactor_OneRow = shadingModel_OneRow.TryGetShadingFactor(receiver, dateTime, out double factor_OneRow, false);
                    bool hasFactor_SevenRows = shadingModel_SevenRows.TryGetShadingFactor(receiver, dateTime, out double factor_SevenRows, false);

                    Assert.Equal(hasFactor_OneBlock, hasFactor_OneRow);
                    Assert.Equal(hasFactor_OneBlock, hasFactor_SevenRows);
                    if (!hasFactor_OneBlock)
                    {
                        continue;
                    }

                    Assert.Equal(factor_OneBlock, factor_OneRow);
                    Assert.Equal(factor_OneBlock, factor_SevenRows);

                    count++;
                    if (factor_OneBlock > 0.0 && factor_OneBlock < 1.0)
                    {
                        count_Partial++;
                    }
                }
            }

            testOutputHelper.WriteLine($"{name}: {count_Triangle} receiver triangles, {columns} columns; {count} samples identical in 1, {count_Triangle} and {(int)Math.Ceiling(count_Triangle / 7.0)} blocks; {count_Partial} partially shaded.");

            Assert.True(count_Partial > 0, $"{name}: no partially shaded sample, so the comparison is vacuous.");

            return shadingModel_OneRow;
        }

        /// <summary>
        /// Solves a copy of a model with the ComputeSharp solver under an intersection buffer budget.
        /// </summary>
        /// <param name="shadingModel">The model to solve; it is copied, never solved itself.</param>
        /// <param name="dateTimes">The date-times to solve.</param>
        /// <param name="maxBufferBytes">The value of <see cref="ComputeSharp.Classes.ShadingSolver.MaxBufferBytes"/>.</param>
        /// <returns>The solved copy.</returns>
        [SupportedOSPlatform("windows")]
        private static ShadingModel SolveTiled(ShadingModel shadingModel, DateTime[] dateTimes, long maxBufferBytes)
        {
            ShadingModel shadingModel_Copy = new(shadingModel);
            Assert.True(new ComputeSharp.Classes.ShadingSolver(shadingModel_Copy, dateTimes) { MaxBufferBytes = maxBufferBytes }.Solve());

            return shadingModel_Copy;
        }

        /// <summary>
        /// Solves a fresh copy of a model with the ComputeSharp solver under an intersection buffer budget and returns the elapsed time. The copy is made before the stopwatch starts.
        /// </summary>
        /// <param name="shadingModel">The model to solve; it is copied, never solved itself.</param>
        /// <param name="dateTimes">The date-times to solve.</param>
        /// <param name="maxBufferBytes">The value of <see cref="ComputeSharp.Classes.ShadingSolver.MaxBufferBytes"/>.</param>
        /// <returns>The elapsed time of the solve, in milliseconds.</returns>
        [SupportedOSPlatform("windows")]
        private static double MeasureTiled(ShadingModel shadingModel, DateTime[] dateTimes, long maxBufferBytes)
        {
            ComputeSharp.Classes.ShadingSolver shadingSolver = new(new ShadingModel(shadingModel), dateTimes) { MaxBufferBytes = maxBufferBytes };

            Stopwatch stopwatch = Stopwatch.StartNew();
            Assert.True(shadingSolver.Solve());
            stopwatch.Stop();

            return stopwatch.Elapsed.TotalMilliseconds;
        }

        /// <summary>
        /// Counts the triangles the solver dispatches for a model, triangulating each element with the solver's default tolerance.
        /// </summary>
        /// <param name="shadingModel">The model.</param>
        /// <returns>The number of receiver triangles (the rows) and the larger of the receiver and shading-only triangle counts (the row width the block size is computed from).</returns>
        private static (int Receivers, int Columns) TriangleCounts(ShadingModel shadingModel)
        {
            ShadingSolverOptions shadingSolverOptions = new();

            int count_Receiver = 0;
            int count_ShadingOnly = 0;

            List<ShadingElement>? shadingElements = shadingModel.GetShadingElements<ShadingElement>();
            Assert.NotNull(shadingElements);
            foreach (ShadingElement shadingElement in shadingElements)
            {
                int count = shadingElement.PolygonalFace3D?.Triangulate(shadingSolverOptions.Tolerance)?.Count ?? 0;
                if (shadingElement.ShadingOnly)
                {
                    count_ShadingOnly += count;
                }
                else
                {
                    count_Receiver += count;
                }
            }

            return (count_Receiver, Math.Max(count_Receiver, count_ShadingOnly));
        }
    }
}
