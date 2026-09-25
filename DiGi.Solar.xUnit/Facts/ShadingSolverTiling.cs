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
        /// Verifies that the block structure of the ComputeSharp solver does not change its results: the same model solved with an unbounded shadow record buffer, with the smallest one (a buffer of one record per caster triangle, so receiver blocks are split and the buffer regrows from its initial size) and with one of three records per caster triangle gives identical shading factors for every receiver at every timestamp.
        /// <para>Since ZiolkowskiJakub/DiGi.Solar#11 the solver appends only the shadows found, grows the record buffer up to <see cref="ComputeSharp.Classes.ShadingSolver.MaxBufferBytes"/> when a block overflows it, splits the block when it would not fit, and sorts the shadows by receiver and caster triangle before the merge, so no budget may change a single factor.
        /// Two models are covered: a 4 x 4 grid of buildings whose walls and roofs shade each other, and horizontal panels under shading-only canopies. Each must have partially shaded samples, so the comparison is not vacuous, and the smallest-budget solve of the building grid is also compared with the CPU solver.</para>
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

            testOutputHelper.WriteLine($"Unsafe.SizeOf<ShadowPolygon2>() = {Unsafe.SizeOf<ShadowPolygon2>()} B");

            ShadingModel shadingModel_Grid = CreateBuildingGridShadingModel(4, false);
            ShadingModel shadingModel_Grid_Smallest = AssertTiledEqualsUntiled(shadingModel_Grid, dateTimes, "building grid");

            ShadingModel shadingModel_CPU = new(shadingModel_Grid);
            Assert.True(new ShadingSolver(shadingModel_CPU, dateTimes).Solve());
            AssertSameShadingFactors(shadingModel_CPU, shadingModel_Grid_Smallest, dateTimes);

            AssertTiledEqualsUntiled(CreatePerformanceShadingModel(4, 3), dateTimes, "panels under canopies");
        }

        /// <summary>
        /// Solves a large model with the ComputeSharp solver and reports the time and memory of the solve.
        /// <para>2 400 receivers under shading-only canopies (<c>CreatePerformanceShadingModel(60, 40)</c>) with 4 804 caster triangles would need 880 MB for a dense receivers x triangles buffer of <see cref="ShadowPolygon2"/> records (and the pre-ZiolkowskiJakub/DiGi.Solar#11 solver about 3.4 GB for its N x N intersection matrix); the solver allocates only the shadows found, within <see cref="ComputeSharp.Classes.ShadingSolver.MaxBufferBytes"/>.
        /// The fact also times the 200-panel performance model at the default budget against the smallest budget (one record per caster triangle), which forces block splits.
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

            int recordSize = Unsafe.SizeOf<ShadowPolygon2>();

            List<string> lines = [$"Unsafe.SizeOf<ShadowPolygon2>() = {recordSize} B"];

            // Warm-up: JIT and shader compilation of the shadow projection shader.
            SolveTiled(CreatePerformanceShadingModel(2, 2), [new DateTime(2026, 6, 26, 12, 0, 0)], long.MaxValue);

            ShadingModel shadingModel_Budget = CreatePerformanceShadingModel(20, 10);
            DateTime[] dateTimes_Budget = CreateDaytimeSeries(60);
            (int count_Receiver_Budget, int count_Triangle_Budget) = ReceiverAndTriangleCounts(shadingModel_Budget);
            long maxBufferBytes_Default = new ComputeSharp.Classes.ShadingSolver(shadingModel_Budget, dateTimes_Budget).MaxBufferBytes;
            long maxBufferBytes_Smallest = (long)recordSize * count_Triangle_Budget;

            lines.Add($"Budget comparison: {count_Receiver_Budget} receivers, {count_Triangle_Budget} caster triangles, {dateTimes_Budget.Length} timestamps");
            lines.Add($"Run | Default budget {maxBufferBytes_Default / (1024.0 * 1024.0):F0} MB (ms) | Smallest budget {maxBufferBytes_Smallest / 1024.0:F0} KB (ms)");
            for (int run = 1; run <= 3; run++)
            {
                double milliseconds_Default = MeasureTiled(shadingModel_Budget, dateTimes_Budget, maxBufferBytes_Default);
                double milliseconds_Smallest = MeasureTiled(shadingModel_Budget, dateTimes_Budget, maxBufferBytes_Smallest);

                string line = $"{run} | {milliseconds_Default:F1} | {milliseconds_Smallest:F1}";
                lines.Add(line);
                testOutputHelper.WriteLine(line);
            }

            ShadingModel shadingModel_Large = CreatePerformanceShadingModel(60, 40);
            DateTime[] dateTimes_Large = [new DateTime(2026, 6, 26, 9, 0, 0), new DateTime(2026, 6, 26, 12, 0, 0), new DateTime(2026, 6, 26, 15, 0, 0)];
            (int count_Receiver_Large, int count_Triangle_Large) = ReceiverAndTriangleCounts(shadingModel_Large);

            ComputeSharp.Classes.ShadingSolver shadingSolver_Large = new(shadingModel_Large, dateTimes_Large);
            long bytes_Dense = (long)recordSize * count_Receiver_Large * count_Triangle_Large;

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

            lines.Add($"Large model: {count_Receiver_Large} receivers, {count_Triangle_Large} caster triangles, {dateTimes_Large.Length} timestamps");
            lines.Add($"A dense receivers x triangles record buffer would be {bytes_Dense / (1024.0 * 1024.0):F0} MB; the solver holds only the shadows found, within the default budget of {shadingSolver_Large.MaxBufferBytes / (1024.0 * 1024.0):F0} MB");
            lines.Add($"Solve {stopwatch.ElapsedMilliseconds} ms; {count_Shaded} of {receivers.Count} receivers shaded at noon");
            lines.Add($"Managed heap before solve {bytes_ManagedBefore / (1024.0 * 1024.0):F0} MB; process peak working set {bytes_PeakWorkingSet / (1024.0 * 1024.0):F0} MB (whole test process)");

            foreach (string line in lines)
            {
                testOutputHelper.WriteLine(line);
            }

            System.IO.File.WriteAllLines(System.IO.Path.Combine(Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly())!, "ShadingSolver_Tiling.txt"), lines);

            Assert.True(stopwatch.ElapsedMilliseconds < 600000, $"The solve of {count_Receiver_Large} receivers took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Solves copies of a model with the ComputeSharp solver under an unbounded, the smallest and a small shadow record budget, and asserts every receiver gets the identical shading factor at every timestamp in all three.
        /// </summary>
        /// <param name="shadingModel">The model to solve; it is copied, never solved itself.</param>
        /// <param name="dateTimes">The date-times to solve.</param>
        /// <param name="name">The model name used in the test output.</param>
        /// <returns>The copy solved under the smallest budget.</returns>
        [SupportedOSPlatform("windows")]
        private ShadingModel AssertTiledEqualsUntiled(ShadingModel shadingModel, DateTime[] dateTimes, string name)
        {
            (int count_Receiver, int count_Triangle) = ReceiverAndTriangleCounts(shadingModel);

            // The solver never lets the budget fall below one record per caster triangle (one receiver's worst case), so 1 byte means exactly that.
            ShadingModel shadingModel_Unbounded = SolveTiled(shadingModel, dateTimes, long.MaxValue);
            ShadingModel shadingModel_Smallest = SolveTiled(shadingModel, dateTimes, 1);
            ShadingModel shadingModel_Small = SolveTiled(shadingModel, dateTimes, (long)Unsafe.SizeOf<ShadowPolygon2>() * count_Triangle * 3);

            List<ShadingElement>? receivers = shadingModel_Unbounded.GetShadingElements<ShadingElement>(shadingOnly: false);
            Assert.NotNull(receivers);
            Assert.NotEmpty(receivers);

            int count = 0;
            int count_Partial = 0;
            foreach (ShadingElement receiver in receivers)
            {
                foreach (DateTime dateTime in dateTimes)
                {
                    bool hasFactor_Unbounded = shadingModel_Unbounded.TryGetShadingFactor(receiver, dateTime, out double factor_Unbounded, false);
                    bool hasFactor_Smallest = shadingModel_Smallest.TryGetShadingFactor(receiver, dateTime, out double factor_Smallest, false);
                    bool hasFactor_Small = shadingModel_Small.TryGetShadingFactor(receiver, dateTime, out double factor_Small, false);

                    Assert.Equal(hasFactor_Unbounded, hasFactor_Smallest);
                    Assert.Equal(hasFactor_Unbounded, hasFactor_Small);
                    if (!hasFactor_Unbounded)
                    {
                        continue;
                    }

                    Assert.Equal(factor_Unbounded, factor_Smallest);
                    Assert.Equal(factor_Unbounded, factor_Small);

                    count++;
                    if (factor_Unbounded > 0.0 && factor_Unbounded < 1.0)
                    {
                        count_Partial++;
                    }
                }
            }

            testOutputHelper.WriteLine($"{name}: {count_Receiver} receivers, {count_Triangle} caster triangles; {count} samples identical under the unbounded, smallest and small budgets; {count_Partial} partially shaded.");

            Assert.True(count_Partial > 0, $"{name}: no partially shaded sample, so the comparison is vacuous.");

            return shadingModel_Smallest;
        }

        /// <summary>
        /// Solves a copy of a model with the ComputeSharp solver under a shadow record buffer budget.
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
        /// Solves a fresh copy of a model with the ComputeSharp solver under a shadow record buffer budget and returns the elapsed time. The copy is made before the stopwatch starts.
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
        /// Counts the receivers and the caster triangles the solver dispatches for a model, triangulating each element with the solver's default tolerance.
        /// </summary>
        /// <param name="shadingModel">The model.</param>
        /// <returns>The number of receivers (the rows) and the number of triangles of every element, receivers and shading-only casters alike (the columns, and the smallest record budget).</returns>
        private static (int Receivers, int Triangles) ReceiverAndTriangleCounts(ShadingModel shadingModel)
        {
            ShadingSolverOptions shadingSolverOptions = new();

            int count_Receiver = 0;
            int count_Triangle = 0;

            List<ShadingElement>? shadingElements = shadingModel.GetShadingElements<ShadingElement>();
            Assert.NotNull(shadingElements);
            foreach (ShadingElement shadingElement in shadingElements)
            {
                int count = shadingElement.PolygonalFace3D?.Triangulate(shadingSolverOptions.Tolerance)?.Count ?? 0;
                count_Triangle += count;
                if (!shadingElement.ShadingOnly && count != 0)
                {
                    count_Receiver++;
                }
            }

            return (count_Receiver, count_Triangle);
        }
    }
}
