using DiGi.Communication.Classes;
using DiGi.Communication.Enums;
using DiGi.Communication.Interfaces;
using DiGi.Geometry.Spatial.Classes;
using System.Diagnostics;
using Xunit.Abstractions;

namespace DiGi.Communication.xUnit
{
    public partial class Facts
    {
        private readonly ITestOutputHelper testOutputHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="Facts"/> class.
        /// </summary>
        /// <param name="testOutputHelper">The xUnit test output helper.</param>
        public Facts(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Compares the CPU-based ScatteringSolver and GPU-based (ComputeSharp) ScatteringSolver
        /// on multiple dome surface mesh sizes to verify correctness and compare performance.
        /// </summary>
        [Fact]
        public void ScatteringSolver_CPU_vs_GPU_Comparison()
        {
            double distance = 100.0;
            Antenna antenna_Transmitter = new(new Point3D(0, 0, 5), Function.Transmitter);
            Antenna antenna_Receiver = new(new Point3D(distance, distance, 5), Function.Receiver);

            // A multi-path profile with multiple delay points
            Dictionary<double, double> values = new()
            {
                [0.5e-6] = 2.0,
                [1.0e-6] = 5.0,
                [1.5e-6] = 3.0
            };
            SimpleMultipathPowerDelayProfile profile = new(values);

            // 1. Warm Up Run (to JIT compile CPU and compile GPU shaders)
            {
                GeometricalPropagationModel warmModel = new();
                warmModel.Assign(profile, antenna_Transmitter, antenna_Receiver);
                List<Point3D> warmPoints = [new Point3D(0, 0, 0), new Point3D(10, 0, 0), new Point3D(0, 10, 0)];
                List<int[]> warmIndexes = [[0, 1, 2]];
                Mesh3D warmMesh = new(warmPoints, warmIndexes);
                warmModel.Update(new ScatteringObject("Warm", warmMesh));

                ScatteringSolverOptions warmOptions = new(0.2, 0.5, 0.001);
                ScatteringSolver warmCpu = new() { GeometricalPropagationModel = warmModel, ScatteringSolverOptions = warmOptions };
                warmCpu.Solve();

                ComputeSharp.Classes.ScatteringSolver warmGpu = new() { GeometricalPropagationModel = warmModel, ScatteringSolverOptions = warmOptions };
                warmGpu.Solve();
            }

            // 2. Measured Sweep Runs
            // gridSide values target approx: 100, 500, 1000, 10000 triangles (since TrianglesCount = 2 * gridSide^2)
            List<Tuple<int, int>> sweepSizes =
            [
                new Tuple<int, int>(7, 98),     // ~100 triangles
                new Tuple<int, int>(16, 512),   // ~500 triangles
                new Tuple<int, int>(22, 968),   // ~1000 triangles
                new Tuple<int, int>(71, 10082)  // ~10000 triangles
            ];

            foreach (Tuple<int, int> size in sweepSizes)
            {
                int gridSide = size.Item1;
                int expectedTriangles = size.Item2;

                // Create dome mesh
                List<Point3D> points = [];
                List<int[]> indexes = [];
                for (int x = 0; x <= gridSide; x++)
                {
                    for (int y = 0; y <= gridSide; y++)
                    {
                        double z = 15.0 * Math.Sin(Math.PI * x / gridSide) * Math.Sin(Math.PI * y / gridSide);
                        points.Add(new Point3D(x * (distance / gridSide), y * (distance / gridSide), z));
                    }
                }
                for (int x = 0; x < gridSide; x++)
                {
                    for (int y = 0; y < gridSide; y++)
                    {
                        int i0 = x * (gridSide + 1) + y;
                        int i1 = i0 + 1;
                        int i2 = i0 + (gridSide + 1);
                        int i3 = i2 + 1;
                        indexes.Add([i0, i1, i2]);
                        indexes.Add([i1, i3, i2]);
                    }
                }
                Mesh3D complexMesh = new(points, indexes);
                ScatteringObject scatteringObject = new("Dome", complexMesh);

                GeometricalPropagationModel model = new();
                Assert.True(model.Assign(profile, antenna_Transmitter, antenna_Receiver));
                Assert.True(model.Update(scatteringObject));

                // Options
                double angleFactor = 0.2;
                double pointDensityFactor = 0.5;
                double tolerance = 0.001;

                ScatteringSolverOptions options = new(angleFactor, pointDensityFactor, tolerance);

                // CPU Solving
                ScatteringSolver cpuSolver = new()
                {
                    GeometricalPropagationModel = model,
                    ScatteringSolverOptions = options
                };

                Stopwatch cpuSw = Stopwatch.StartNew();
                bool cpuSuccess = cpuSolver.Solve();
                cpuSw.Stop();

                // GPU Solving
                ComputeSharp.Classes.ScatteringSolver gpuSolver = new()
                {
                    GeometricalPropagationModel = model,
                    ScatteringSolverOptions = options
                };

                Stopwatch gpuSw = Stopwatch.StartNew();
                bool gpuSuccess = gpuSolver.Solve();
                gpuSw.Stop();

                testOutputHelper.WriteLine($"[Triangles={expectedTriangles}] CPU Solve Success: {cpuSuccess} in {cpuSw.Elapsed.TotalMilliseconds:F3} ms");
                testOutputHelper.WriteLine($"[Triangles={expectedTriangles}] GPU Solve Success: {gpuSuccess} in {gpuSw.Elapsed.TotalMilliseconds:F3} ms");

                Assert.True(cpuSuccess);
                Assert.True(gpuSuccess);

                // Compare results
                Assert.NotNull(cpuSolver.ScatteringProfiles);
                Assert.NotNull(gpuSolver.ScatteringProfiles);
                Assert.Equal(gpuSolver.ScatteringProfiles.Count, cpuSolver.ScatteringProfiles.Count);

                for (int i = 0; i < cpuSolver.ScatteringProfiles.Count; i++)
                {
                    IScatteringProfile cpuProfile = cpuSolver.ScatteringProfiles[i];
                    IScatteringProfile gpuProfile = gpuSolver.ScatteringProfiles[i];

                    Assert.Equal(gpuProfile.Visible, cpuProfile.Visible);
                    Assert.NotNull(cpuProfile.Scatterings);
                    Assert.NotNull(gpuProfile.Scatterings);

                    List<Scattering> cpuScatterings = new(cpuProfile.Scatterings);
                    List<Scattering> gpuScatterings = new(gpuProfile.Scatterings);

                    Assert.Equal(gpuScatterings.Count, cpuScatterings.Count);

                    for (int j = 0; j < cpuScatterings.Count; j++)
                    {
                        Scattering cpuScattering = cpuScatterings[j];
                        Scattering gpuScattering = gpuScatterings[j];

                        Assert.Equal(gpuScattering.Delay, cpuScattering.Delay, 9);
                        Assert.NotNull(cpuScattering.ScatteringPointGroups);
                        Assert.NotNull(gpuScattering.ScatteringPointGroups);
                        Assert.Equal(gpuScattering.ScatteringPointGroups.Count, cpuScattering.ScatteringPointGroups.Count);

                        for (int k = 0; k < cpuScattering.ScatteringPointGroups.Count; k++)
                        {
                            ScatteringPointGroup cpuGroup = cpuScattering.ScatteringPointGroups[k];
                            ScatteringPointGroup gpuGroup = gpuScattering.ScatteringPointGroups[k];

                            Assert.Equal(gpuGroup.Reference, cpuGroup.Reference);
                            Assert.NotNull(cpuGroup.Points);
                            Assert.NotNull(gpuGroup.Points);

                            // Check point count matching approximately (due to slightly different precision on GPU vs CPU)
                            Assert.True(Math.Abs(gpuGroup.Points.Count - cpuGroup.Points.Count) <= 2,
                                $"Point count mismatch on group {cpuGroup.Reference}: CPU={cpuGroup.Points.Count}, GPU={gpuGroup.Points.Count}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tests that <see cref="ScatteringSolver"/> successfully runs and handles a face with a null bounding box without throwing any exceptions.
        /// </summary>
        [Fact]
        public void ScatteringSolver_NullBoundingBox_DoesNotThrow()
        {
            double distance = 10.0;
            Antenna antenna_Transmitter = new(new Point3D(0, 0, 5), Function.Transmitter);
            Antenna antenna_Receiver = new(new Point3D(distance, 0, 5), Function.Receiver);

            Dictionary<double, double> values = new()
            {
                [1.0e-6] = 5.0
            };
            SimpleMultipathPowerDelayProfile profile = new(values);

            GeometricalPropagationModel model = new();
            Assert.True(model.Assign(profile, antenna_Transmitter, antenna_Receiver));

            List<Point3D> point3Ds = [new Point3D(0, 0, 0), new Point3D(0, 0, 0), new Point3D(0, 0, 0)];
            List<int[]> indexArrays = [[0, 1, 2]];
            Mesh3D complexMesh = new(point3Ds, indexArrays);

            ScatteringObject scatteringObject = new("Degenerate", complexMesh);
            Assert.True(model.Update(scatteringObject));

            ScatteringSolverOptions options = new(0.2, 0.5, 0.001);
            ScatteringSolver cpuSolver = new()
            {
                GeometricalPropagationModel = model,
                ScatteringSolverOptions = options
            };

            bool success = cpuSolver.Solve();
            Assert.True(success);
            Assert.NotNull(cpuSolver.ScatteringProfiles);
        }

        /// <summary>
        /// Tests that both CPU and GPU solvers partition disconnected intersection segments of a single scattering object
        /// into separate <see cref="ScatteringPointGroup"/> instances instead of merging them.
        /// </summary>
        [Fact]
        public void ScatteringSolver_DisconnectedSegments_CreatesSeparateGroups()
        {
            double distance = 20.0;
            Antenna antenna_Transmitter = new(new Point3D(0, -10, 5), Function.Transmitter);
            Antenna antenna_Receiver = new(new Point3D(0, 10, 5), Function.Receiver);

            double delay = 2.88 / Constants.Physical.LightSpeed;
            Dictionary<double, double> values = new()
            {
                [delay] = 5.0
            };
            SimpleMultipathPowerDelayProfile profile = new(values);

            GeometricalPropagationModel model = new();
            Assert.True(model.Assign(profile, antenna_Transmitter, antenna_Receiver));

            // Create two disconnected triangles representing the same scattering object,
            // shifted in X to avoid mutual path occlusion.
            List<Point3D> point3Ds = [
                // Triangle 1 (negative Y region, negative X)
                new Point3D(-6, -5, 0),
                new Point3D(-2, -5, 0),
                new Point3D(-4, -5, 4),
                // Triangle 2 (positive Y region, positive X)
                new Point3D(2, 5, 0),
                new Point3D(6, 5, 0),
                new Point3D(4, 5, 4)
            ];
            List<int[]> indexArrays = [
                [0, 1, 2],
                [3, 4, 5]
            ];
            Mesh3D complexMesh = new(point3Ds, indexArrays);

            ScatteringObject scatteringObject = new("U-Shape", complexMesh);
            Assert.True(model.Update(scatteringObject));

            ScatteringSolverOptions options = new(0.2, 0.5, 0.001);

            // CPU Solver verification
            ScatteringSolver cpuSolver = new()
            {
                GeometricalPropagationModel = model,
                ScatteringSolverOptions = options
            };
            Assert.True(cpuSolver.Solve());
            Assert.NotNull(cpuSolver.ScatteringProfiles);
            Assert.Single(cpuSolver.ScatteringProfiles);
            IScatteringProfile cpuProfile = cpuSolver.ScatteringProfiles[0];
            Assert.NotNull(cpuProfile.Scatterings);
            List<Scattering> cpuScatterings = [.. cpuProfile.Scatterings];
            Assert.Single(cpuScatterings);
            Scattering cpuScattering = cpuScatterings[0];
            Assert.NotNull(cpuScattering.ScatteringPointGroups);
            // Since the two triangles are separated, it must create 2 separate groups for the reference "U-Shape"
            Assert.Equal(2, cpuScattering.ScatteringPointGroups.Count);
            Assert.Equal("U-Shape", cpuScattering.ScatteringPointGroups[0].Reference);
            Assert.Equal("U-Shape", cpuScattering.ScatteringPointGroups[1].Reference);

            // GPU Solver verification
            ComputeSharp.Classes.ScatteringSolver gpuSolver = new()
            {
                GeometricalPropagationModel = model,
                ScatteringSolverOptions = options
            };
            Assert.True(gpuSolver.Solve());
            Assert.NotNull(gpuSolver.ScatteringProfiles);
            Assert.Single(gpuSolver.ScatteringProfiles);
            IScatteringProfile gpuProfile = gpuSolver.ScatteringProfiles[0];
            Assert.NotNull(gpuProfile.Scatterings);
            List<Scattering> gpuScatterings = [.. gpuProfile.Scatterings];
            Assert.Single(gpuScatterings);
            Scattering gpuScattering = gpuScatterings[0];
            Assert.NotNull(gpuScattering.ScatteringPointGroups);
            Assert.Equal(2, gpuScattering.ScatteringPointGroups.Count);
            Assert.Equal("U-Shape", gpuScattering.ScatteringPointGroups[0].Reference);
            Assert.Equal("U-Shape", gpuScattering.ScatteringPointGroups[1].Reference);
        }
    }
}