using DiGi.Core.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Solar.Classes;
using DiGi.Solar.ComputeSharp.Enums;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Versioning;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Buildings per side of the square building grids swept by the shading solver benchmarks; each building has five surfaces (four walls and a flat roof).
        /// </summary>
        private static readonly int[] ShadingSolverBenchmarkGridSizes = [1, 2, 4, 8, 12];

        /// <summary>
        /// Surface count that sets the number of timed solves per grid: <c>Math.Max(1, ShadingSolverBenchmarkRepeatSurfaceCount / surfaceCount)</c>, so small models are averaged over several solves.
        /// </summary>
        private const int ShadingSolverBenchmarkRepeatSurfaceCount = 720;

        /// <summary>
        /// Largest allowed fraction of compared samples in which a ComputeSharp engine drops a shadow the CPU solver finds (see the parity helper in <c>ShadingSolverCPU.cs</c>).
        /// <para>It is 0. The tolerance of 0.001 covered the failed shadow union of ZiolkowskiJakub/DiGi.Solar#8, fixed there and in ZiolkowskiJakub/DiGi.Geometry#8; the dropped count is still reported as a column, so a drop that comes back is seen rather than absorbed.</para>
        /// </summary>
        private const double ShadingSolverBenchmarkDroppedFraction = 0;

        /// <summary>
        /// Minutes between the timestamps of the benchmark day (2026-06-26, 04:00 to 21:00).
        /// </summary>
        private const int ShadingSolverBenchmarkStepMinutes = 60;

        /// <summary>
        /// Benchmarks the shading solvers on a grid of buildings in which every surface is a receiver, so the buildings shade each other.
        /// <para>Compares the CPU <see cref="ShadingSolver"/> with the ComputeSharp solver on a hardware GPU (<see cref="ComputeDeviceType.Hardware"/>) for grids of 1 to 144 buildings (5 to 720 surfaces).
        /// WARP is not measured: it was withdrawn in ZiolkowskiJakub/DiGi.Solar#10. Results are written to <c>ShadingSolver_Benchmark_Receivers.txt</c> in the reports directory.</para>
        /// </summary>
        [Fact]
        [SupportedOSPlatform("windows")]
        public void ShadingSolver_Benchmark_Receivers()
        {
            RunShadingSolverBenchmark(nameof(ShadingSolver_Benchmark_Receivers), false);
        }

        /// <summary>
        /// Benchmarks the shading solvers on one receiving building surrounded by shading-only buildings, the configuration of a per-building solar analysis.
        /// <para>The receiving building (5 surfaces) sits at the centre of grids of 1 to 144 buildings; every other building only casts shadows. Results are written to <c>ShadingSolver_Benchmark_Surroundings.txt</c> in the reports directory.</para>
        /// </summary>
        [Fact]
        [SupportedOSPlatform("windows")]
        public void ShadingSolver_Benchmark_Surroundings()
        {
            RunShadingSolverBenchmark(nameof(ShadingSolver_Benchmark_Surroundings), true);
        }

        /// <summary>
        /// Runs one CPU versus hardware GPU benchmark sweep over <see cref="ShadingSolverBenchmarkGridSizes"/>, asserting that the GPU solver agrees with the CPU solver before timing either.
        /// </summary>
        /// <param name="name">The benchmark name, used for the report file.</param>
        /// <param name="surroundingsShadingOnly">True to make every building except the central one shading-only; false to make every surface a receiver.</param>
        [SupportedOSPlatform("windows")]
        private void RunShadingSolverBenchmark(string name, bool surroundingsShadingOnly)
        {
            DateTime[] dateTimes = CreateDaytimeSeries(ShadingSolverBenchmarkStepMinutes);

            bool hardware = ComputeSharp.Create.GraphicsDevice(ComputeDeviceType.Hardware) is not null;

            List<string> lines = [$"{name}: {dateTimes.Length} timestamps, step {ShadingSolverBenchmarkStepMinutes} min", "Buildings | Surfaces | Receivers | Casters | Solves | CPU (ms/solve) | Hardware (ms/solve) | Hardware / CPU | Dropped shadows | Factor difference p99 | Factor difference max"];

            // Warm-up on the single-building model: JIT for the CPU solver, shader and pipeline creation for the GPU.
            ShadingModel shadingModel_WarmUp = CreateBuildingGridShadingModel(1, surroundingsShadingOnly);
            Assert.True(new ShadingSolver(new ShadingModel(shadingModel_WarmUp), dateTimes).Solve());
            if (hardware)
            {
                Assert.True(new ComputeSharp.Classes.ShadingSolver(new ShadingModel(shadingModel_WarmUp), dateTimes) { ComputeDeviceType = ComputeDeviceType.Hardware }.Solve());
            }

            foreach (int gridSize in ShadingSolverBenchmarkGridSizes)
            {
                ShadingModel shadingModel = CreateBuildingGridShadingModel(gridSize, surroundingsShadingOnly);

                int count_Receiver = shadingModel.GetShadingElements<ShadingElement>(shadingOnly: false)?.Count ?? 0;
                int count_Caster = shadingModel.GetShadingElements<ShadingElement>(shadingOnly: true)?.Count ?? 0;
                int count_Surface = count_Receiver + count_Caster;
                int repeats = Math.Max(1, ShadingSolverBenchmarkRepeatSurfaceCount / count_Surface);

                // Correctness first: the GPU solver must reproduce the CPU solver on this model.
                (int Compared, int Dropped, double Percentile, double Max) parity = hardware ? AssertSameShadingFactors(shadingModel, dateTimes, ComputeDeviceType.Hardware, ShadingSolverBenchmarkDroppedFraction) : (0, 0, double.NaN, double.NaN);

                double milliseconds_CPU = MeasureShadingSolver(shadingModel, dateTimes, repeats, null);
                double milliseconds_Hardware = hardware ? MeasureShadingSolver(shadingModel, dateTimes, repeats, ComputeDeviceType.Hardware) : double.NaN;

                string line = $"{gridSize * gridSize} | {count_Surface} | {count_Receiver} | {count_Caster} | {repeats} | {milliseconds_CPU:F1} | {milliseconds_Hardware:F1} | {milliseconds_Hardware / milliseconds_CPU:F2} | {parity.Dropped} | {parity.Percentile:E1} | {parity.Max:E1}";
                lines.Add(line);
                testOutputHelper.WriteLine(line);

                Assert.True(milliseconds_CPU < 60000, $"CPU solve of {count_Surface} surfaces took {milliseconds_CPU} ms.");
            }

            System.IO.File.WriteAllLines(System.IO.Path.Combine(Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly())!, name + ".txt"), lines);
        }

        /// <summary>
        /// Solves fresh copies of the model with one engine and returns the mean elapsed time per solve. The copies are made before the stopwatch starts.
        /// </summary>
        /// <param name="shadingModel">The model to solve; it is copied, never solved itself.</param>
        /// <param name="dateTimes">The date-times to solve.</param>
        /// <param name="repeats">The number of timed solves.</param>
        /// <param name="computeDeviceType">The ComputeSharp device, or null for the CPU solver.</param>
        /// <returns>The mean time per solve, in milliseconds.</returns>
        [SupportedOSPlatform("windows")]
        private static double MeasureShadingSolver(ShadingModel shadingModel, DateTime[] dateTimes, int repeats, ComputeDeviceType? computeDeviceType)
        {
            List<ShadingSolver> shadingSolvers = [];
            for (int i = 0; i < repeats; i++)
            {
                ShadingModel shadingModel_Copy = new(shadingModel);
                shadingSolvers.Add(computeDeviceType is null ? new ShadingSolver(shadingModel_Copy, dateTimes) : new ComputeSharp.Classes.ShadingSolver(shadingModel_Copy, dateTimes) { ComputeDeviceType = computeDeviceType.Value });
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach (ShadingSolver shadingSolver in shadingSolvers)
            {
                Assert.True(shadingSolver.Solve());
            }

            stopwatch.Stop();

            return stopwatch.Elapsed.TotalMilliseconds / repeats;
        }

        /// <summary>
        /// Builds a square grid of box buildings (10 x 10 m footprint, 12 m high, 8 m streets), each with four walls and a flat roof, at (50.0, 20.0).
        /// </summary>
        /// <param name="gridSize">The number of buildings per side of the grid.</param>
        /// <param name="surroundingsShadingOnly">True to make every building except the central one shading-only; false to make every surface a receiver.</param>
        /// <returns>The populated <see cref="ShadingModel"/>.</returns>
        private static ShadingModel CreateBuildingGridShadingModel(int gridSize, bool surroundingsShadingOnly)
        {
            ShadingModel shadingModel = new(Core.Enums.UTC.Plus0100, new Coordinates(50.0, 20.0));

            double size = 10.0;
            double height = 12.0;
            double step = size + 8.0;
            int index_Centre = gridSize / 2;

            Vector3D[] vector3Ds_Normal = [new(0.0, -1.0, 0.0), new(1.0, 0.0, 0.0), new(0.0, 1.0, 0.0), new(-1.0, 0.0, 0.0), new(0.0, 0.0, 1.0)];

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    bool shadingOnly = surroundingsShadingOnly && (i != index_Centre || j != index_Centre);

                    double x_Min = i * step, y_Min = j * step;
                    double x_Max = x_Min + size, y_Max = y_Min + size;

                    Point3D[][] point3Ds_Faces =
                    [
                        [new(x_Min, y_Min, 0.0), new(x_Max, y_Min, 0.0), new(x_Max, y_Min, height), new(x_Min, y_Min, height)],
                        [new(x_Max, y_Min, 0.0), new(x_Max, y_Max, 0.0), new(x_Max, y_Max, height), new(x_Max, y_Min, height)],
                        [new(x_Max, y_Max, 0.0), new(x_Min, y_Max, 0.0), new(x_Min, y_Max, height), new(x_Max, y_Max, height)],
                        [new(x_Min, y_Max, 0.0), new(x_Min, y_Min, 0.0), new(x_Min, y_Min, height), new(x_Min, y_Max, height)],
                        [new(x_Min, y_Min, height), new(x_Max, y_Min, height), new(x_Max, y_Max, height), new(x_Min, y_Max, height)],
                    ];

                    for (int k = 0; k < point3Ds_Faces.Length; k++)
                    {
                        Polygon3D? polygon3D = Geometry.Spatial.Create.Polygon3D(vector3Ds_Normal[k], point3Ds_Faces[k]);
                        PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(polygon3D);
                        Assert.NotNull(polygonalFace3D);
                        Assert.True(shadingModel.Update(new ShadingElement(polygonalFace3D, shadingOnly)));
                    }
                }
            }

            return shadingModel;
        }
    }
}
