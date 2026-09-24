using DiGi.Core.Classes;
using DiGi.Solar.Classes;
using System.Runtime.Versioning;
using Xunit.Abstractions;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        private readonly ITestOutputHelper testOutputHelper;

        /// <summary>
        /// Initializes a new instance of the Facts class.
        /// </summary>
        /// <param name="testOutputHelper">The test output helper for logging.</param>
        public Facts(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Tests the instantiation and execution of the ShadingSolver: the CPU solver always, the ComputeSharp (GPU) solver if ComputeSharp is supported on the current system.
        /// </summary>
        /// <param name="computeSharp">True to test the ComputeSharp solver; false to test the CPU solver.</param>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [SupportedOSPlatform("windows")]
        public void ShadingSolver_Solve(bool computeSharp)
        {
            Coordinates coordinates = new(50.0, 20.0);
            ShadingModel shadingModel = new(Core.Enums.UTC.Plus0100, coordinates);

            DateTime[] dateTimes = [new DateTime(2026, 6, 26, 12, 0, 0)];
            ShadingSolver shadingSolver = CreateShadingSolver(shadingModel, dateTimes, computeSharp);

            Assert.NotNull(shadingSolver);
            Assert.Equal(shadingModel, shadingSolver.ShadingModel);
            Assert.Equal(computeSharp, shadingSolver is ComputeSharp.Classes.ShadingSolver);

            if (IsShadingSolverSupported(computeSharp, testOutputHelper))
            {
                bool isSolved = shadingSolver.Solve();
                Assert.True(isSolved);
            }
            else
            {
                testOutputHelper.WriteLine("Skipping GPU ShadingSolver.Solve test because ComputeSharp is not supported on this machine.");
            }
        }

        /// <summary>
        /// Creates the CPU <see cref="ShadingSolver"/> or the ComputeSharp (GPU) solver derived from it for the given model and date-times.
        /// </summary>
        /// <param name="shadingModel">The shading model to solve.</param>
        /// <param name="dateTimes">The date-times to solve.</param>
        /// <param name="computeSharp">True to create the ComputeSharp solver; false to create the CPU solver.</param>
        /// <returns>The created solver.</returns>
        [SupportedOSPlatform("windows")]
        private static ShadingSolver CreateShadingSolver(ShadingModel shadingModel, DateTime[] dateTimes, bool computeSharp)
        {
            return computeSharp ? new ComputeSharp.Classes.ShadingSolver(shadingModel, dateTimes) : new ShadingSolver(shadingModel, dateTimes);
        }

        /// <summary>
        /// Determines whether the requested solver can run on the current machine: the CPU solver always can, the ComputeSharp solver only where ComputeSharp is supported.
        /// </summary>
        /// <param name="computeSharp">True for the ComputeSharp solver; false for the CPU solver.</param>
        /// <param name="testOutputHelper">The test output helper to write warning messages to.</param>
        /// <returns>True if the solver can run; otherwise, false.</returns>
        [SupportedOSPlatform("windows")]
        private static bool IsShadingSolverSupported(bool computeSharp, ITestOutputHelper testOutputHelper)
        {
            return !computeSharp || IsComputeSharpSupported(testOutputHelper);
        }

        /// <summary>
        /// Determines whether ComputeSharp is supported on the current machine and graphics device.
        /// </summary>
        /// <param name="testOutputHelper">The test output helper to write warning messages to.</param>
        /// <returns>True if ComputeSharp is supported; otherwise, false.</returns>
        [SupportedOSPlatform("windows")]
        private static bool IsComputeSharpSupported(ITestOutputHelper testOutputHelper)
        {
            try
            {
                using global::ComputeSharp.GraphicsDevice graphicsDevice = global::ComputeSharp.GraphicsDevice.GetDefault();
                if (graphicsDevice == null)
                {
                    testOutputHelper.WriteLine("WARNING: ComputeSharp is not supported on this machine (no default GraphicsDevice found).");
                    return false;
                }
                if (!graphicsDevice.IsDoublePrecisionSupportAvailable())
                {
                    testOutputHelper.WriteLine("WARNING: ComputeSharp is not supported on this machine (graphics device does not support double precision operations).");
                    return false;
                }
                return true;
            }
            catch (Exception exception)
            {
                testOutputHelper.WriteLine("WARNING: ComputeSharp is not supported on this machine: " + exception.Message);
                return false;
            }
        }

        /// <summary>
        /// Verifies on both solvers that a receiver standing between two shading-only walls never reads fully sunlit while it is shaded, and never reads covered by more than its own area.
        /// <para>Geometry: a 4 x 4 m horizontal receiver at z = 0 with a shading-only wall at x = 6 and one at x = -2 (both spanning y in [-2, 6], 12 m high), so one of them shadows the receiver for most of the day. The two walls' shadows overlap on the receiver, which is exactly the shape the shadow union collapses, so the sample counts are compared between the solvers rather than each factor.</para>
        /// <para>A solver that loses the merged shadow (see ZiolkowskiJakub/DiGi.Solar#8) reads factor 0 on those samples and fails the count assertion; one that double-counts them reads factor above 1 and fails the per-sample bound.</para>
        /// </summary>
        /// <param name="computeSharp">True to solve with the ComputeSharp solver; false for the CPU solver.</param>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [SupportedOSPlatform("windows")]
        public void ShadingSolver_Solve_ShadowedReceiverNeverReadsSunlit(bool computeSharp)
        {
            if (!IsShadingSolverSupported(computeSharp, testOutputHelper))
            {
                testOutputHelper.WriteLine("Skipping ShadingSolver_Solve_ShadowedReceiverNeverReadsSunlit because the requested solver is not supported on this machine.");
                return;
            }

            ShadingModel shadingModel = new(Core.Enums.UTC.Plus0100, new DiGi.Core.Classes.Coordinates(50.0, 20.0));
            Geometry.Spatial.Classes.Plane plane_Receiver = new(new Geometry.Spatial.Classes.Point3D(0.0, 0.0, 0.0), new Geometry.Spatial.Classes.Vector3D(0.0, 0.0, 1.0));
            Geometry.Spatial.Classes.PolygonalFace3D? polygonalFace3D_Receiver = Geometry.Spatial.Create.PolygonalFace3D(plane_Receiver, new Geometry.Planar.Classes.Point2D(0.0, 0.0), new Geometry.Planar.Classes.Point2D(4.0, 0.0), new Geometry.Planar.Classes.Point2D(4.0, 4.0), new Geometry.Planar.Classes.Point2D(0.0, 4.0));
            Assert.NotNull(polygonalFace3D_Receiver);
            ShadingElement shadingElement_Receiver = new(polygonalFace3D_Receiver, false);
            Assert.True(shadingModel.Update(shadingElement_Receiver));
            CreateVerticalWall(shadingModel, 6.0);
            CreateVerticalWall(shadingModel, -2.0);

            DateTime[] dateTimes = CreateDaytimeSeries(60);

            ShadingModel shadingModel_Reference = new(shadingModel);
            ShadingModel shadingModel_Other = new(shadingModel);

            Assert.True(new ShadingSolver(shadingModel_Reference, dateTimes).Solve());
            Assert.True(CreateShadingSolver(shadingModel_Other, dateTimes, computeSharp).Solve());

            Geometry.Spatial.Classes.Vector3D? vector3D_Normal = shadingElement_Receiver.PolygonalFace3D?.Plane?.Normal;
            Assert.NotNull(vector3D_Normal);

            int count_SunFacing = 0;
            int count_Shaded_Reference = 0;
            int count_Shaded_Other = 0;
            foreach (DateTime dateTime in dateTimes)
            {
                // Night timestamps have no sun direction and no results; a back-facing receiver gets no beam, so its factor is not compared either.
                if (Query.SunDirection(shadingModel_Reference, dateTime, false) is not Geometry.Spatial.Classes.Vector3D vector3D_SunDirection || vector3D_SunDirection.DotProduct(vector3D_Normal) >= 0)
                {
                    continue;
                }

                count_SunFacing++;

                Assert.True(shadingModel_Reference.TryGetShadingFactor(shadingElement_Receiver, dateTime, out double factor_Reference, false), $"No reference factor at {dateTime:yyyy-MM-dd HH:mm}.");
                Assert.True(shadingModel_Other.TryGetShadingFactor(shadingElement_Receiver, dateTime, out double factor_Other, false), $"No factor at {dateTime:yyyy-MM-dd HH:mm}.");

                // A receiver cannot be covered by more than itself: the merged shadow is what stops overlapping shadows from double counting.
                Assert.True(factor_Reference >= 0.0 && factor_Reference <= 1.0, $"Reference factor {factor_Reference} outside [0, 1] at {dateTime:yyyy-MM-dd HH:mm}.");
                Assert.True(factor_Other >= 0.0 && factor_Other <= 1.0, $"Factor {factor_Other} outside [0, 1] at {dateTime:yyyy-MM-dd HH:mm} (computeSharp {computeSharp}).");

                if (factor_Reference > 0.0)
                {
                    count_Shaded_Reference++;
                }

                if (factor_Other > 0.0)
                {
                    count_Shaded_Other++;
                }

                testOutputHelper.WriteLine($"{dateTime:HH:mm} | sun facing | reference {factor_Reference:F4} | other {factor_Other:F4} (computeSharp {computeSharp})");
            }

            testOutputHelper.WriteLine($"{count_SunFacing} sun-facing samples | shaded on the reference {count_Shaded_Reference} | shaded on the other {count_Shaded_Other} (computeSharp {computeSharp}).");

            // The fixture is only meaningful while it shades the receiver for most of the day (measured: 15 of 16 sun-facing samples on 2026-06-26 at (50.0, 20.0); the one unshaded sample is the local-noon one, where both walls project past the receiver), and a solver must not lose that shading.
            Assert.True(count_Shaded_Reference >= 12, $"Fixture no longer shades the receiver for most of the day: {count_Shaded_Reference} of {count_SunFacing} sun-facing samples shaded.");
            Assert.Equal(count_Shaded_Reference, count_Shaded_Other);
        }
    }
}