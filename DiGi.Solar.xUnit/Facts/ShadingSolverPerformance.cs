using DiGi.Core.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Solar.Classes;
using DiGi.Solar.ComputeSharp.Classes;
using DiGi.Solar.Interfaces;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Measures the end-to-end execution time of <see cref="ShadingSolver.Solve"/> on a large model.
        /// <para>The model contains a grid of receiver panels shaded by elevated shading-only canopies, evaluated across a full day of sun directions.</para>
        /// <para>A warm-up solve is performed first to trigger JIT compilation and ComputeSharp shader compilation before timing.</para>
        /// </summary>
        [Fact]
        [SupportedOSPlatform("windows")]
        public void ShadingSolver_Solve_Performance()
        {
            if (!IsComputeSharpSupported(testOutputHelper))
            {
                testOutputHelper.WriteLine("Skipping ShadingSolver_Solve_Performance because ComputeSharp is not supported on this machine.");
                return;
            }

            // Warm-up: small model, single date-time, exercises both the self-shading and external-shading shaders so they are compiled before timing.
            ShadingModel shadingModel_WarmUp = CreatePerformanceShadingModel(2, 2);
            DateTime[] dateTimes_WarmUp = [new DateTime(2026, 6, 26, 12, 0, 0)];
            ShadingSolver shadingSolver_WarmUp = new(shadingModel_WarmUp, dateTimes_WarmUp);
            Assert.True(shadingSolver_WarmUp.Solve());

            // Timed run: large model across a full day of daylight.
            ShadingModel shadingModel = CreatePerformanceShadingModel(20, 10);
            DateTime[] dateTimes = CreateDaytimeSeries(10);
            ShadingSolver shadingSolver = new(shadingModel, dateTimes);

            Stopwatch stopwatch = Stopwatch.StartNew();
            bool isSolved = shadingSolver.Solve();
            stopwatch.Stop();

            Assert.True(isSolved);

            testOutputHelper.WriteLine($"ShadingSolver.Solve elapsed: {stopwatch.ElapsedMilliseconds} ms (dateTimes: {dateTimes.Length})");

            // Loose threshold: this test exists to report timing, not to enforce a tight bound that could flake across machines.
            Assert.True(stopwatch.ElapsedMilliseconds < 180000, $"Solve took {stopwatch.ElapsedMilliseconds} ms, which exceeds the safety threshold.");
        }

        /// <summary>
        /// Verifies that a receiver panel covered by an elevated shading-only canopy reports a substantial shading factor at solar noon.
        /// <para>Acts as the behavioural anchor for the solver: re-running it before and after the invariant-hoisting optimization confirms results are unchanged.</para>
        /// </summary>
        [Fact]
        [SupportedOSPlatform("windows")]
        public void ShadingSolver_Solve_Shadows()
        {
            if (!IsComputeSharpSupported(testOutputHelper))
            {
                testOutputHelper.WriteLine("Skipping ShadingSolver_Solve_Shadows because ComputeSharp is not supported on this machine.");
                return;
            }

            Coordinates coordinates = new(50.0, 20.0);
            ShadingModel shadingModel = new(Core.Enums.UTC.Plus0100, coordinates);

            Vector3D vector3D_Normal = new(0.0, 0.0, 1.0);

            DiGi.Geometry.Spatial.Classes.Plane plane_Receiver = new(new Point3D(0.0, 0.0, 0.0), vector3D_Normal);
            DiGi.Geometry.Spatial.Classes.PolygonalFace3D? polygonalFace3D_Receiver = DiGi.Geometry.Spatial.Create.PolygonalFace3D(plane_Receiver, new Point2D(0.0, 0.0), new Point2D(4.0, 0.0), new Point2D(4.0, 4.0), new Point2D(0.0, 4.0));
            Assert.NotNull(polygonalFace3D_Receiver);
            ShadingElement shadingElement_Receiver = new(polygonalFace3D_Receiver, false);
            Assert.True(shadingModel.Update(shadingElement_Receiver));

            DiGi.Geometry.Spatial.Classes.Plane plane_Canopy = new(new Point3D(0.0, 0.0, 2.0), vector3D_Normal);
            DiGi.Geometry.Spatial.Classes.PolygonalFace3D? polygonalFace3D_Canopy = DiGi.Geometry.Spatial.Create.PolygonalFace3D(plane_Canopy, new Point2D(-20.0, -20.0), new Point2D(24.0, -20.0), new Point2D(24.0, 24.0), new Point2D(-20.0, 24.0));
            Assert.NotNull(polygonalFace3D_Canopy);
            ShadingElement shadingElement_Canopy = new(polygonalFace3D_Canopy, true);
            Assert.True(shadingModel.Update(shadingElement_Canopy));

            DateTime dateTime_Noon = new(2026, 6, 26, 12, 0, 0);
            ShadingSolver shadingSolver = new(shadingModel, [dateTime_Noon]);
            Assert.True(shadingSolver.Solve());

            bool hasFactor = shadingModel.TryGetShadingFactor(shadingElement_Receiver, dateTime_Noon, out double factor, false);
            Assert.True(hasFactor);
            Assert.True(factor > 0.5, $"Expected the canopy to substantially shade the receiver, but the factor was {factor}.");
        }

        /// <summary>
        /// Characterizes the current behaviour for a fully sunlit element (finding #1).
        /// <para>A receiver with no obstacle casts no shadow at any time. The solver therefore stores no result for it, so the element gains no relation and <see cref="ShadingModel.TryGetShadingFactor"/> reports failure (NaN) rather than a factor of 0.</para>
        /// <para>This test locks in the present behaviour; if the sparse-result gap is later closed it should be updated to expect a factor of 0.</para>
        /// </summary>
        [Fact]
        [SupportedOSPlatform("windows")]
        public void ShadingSolver_Solve_FullySunlit()
        {
            if (!IsComputeSharpSupported(testOutputHelper))
            {
                testOutputHelper.WriteLine("Skipping ShadingSolver_Solve_FullySunlit because ComputeSharp is not supported on this machine.");
                return;
            }

            Coordinates coordinates = new(50.0, 20.0);
            ShadingModel shadingModel = new(Core.Enums.UTC.Plus0100, coordinates);

            Vector3D vector3D_Normal = new(0.0, 0.0, 1.0);
            DiGi.Geometry.Spatial.Classes.Plane plane_Receiver = new(new Point3D(0.0, 0.0, 0.0), vector3D_Normal);
            DiGi.Geometry.Spatial.Classes.PolygonalFace3D? polygonalFace3D_Receiver = DiGi.Geometry.Spatial.Create.PolygonalFace3D(plane_Receiver, new Point2D(0.0, 0.0), new Point2D(4.0, 0.0), new Point2D(4.0, 4.0), new Point2D(0.0, 4.0));
            Assert.NotNull(polygonalFace3D_Receiver);
            ShadingElement shadingElement_Receiver = new(polygonalFace3D_Receiver, false);
            Assert.True(shadingModel.Update(shadingElement_Receiver));

            DateTime dateTime_Noon = new(2026, 6, 26, 12, 0, 0);
            ShadingSolver shadingSolver = new(shadingModel, [dateTime_Noon]);
            Assert.True(shadingSolver.Solve());

            // No obstacle exists, so the receiver is fully sunlit. Current behaviour: no result is recorded.
            bool hasFactor = shadingModel.TryGetShadingFactor(shadingElement_Receiver, dateTime_Noon, out double factor, false);
            Assert.False(hasFactor);
            Assert.True(double.IsNaN(factor));

            List<IShadingSolverResult>? shadingSolverResults = shadingModel.GetShadingSolverResults<IShadingSolverResult>(shadingElement_Receiver);
            Assert.True(shadingSolverResults == null || shadingSolverResults.Count == 0);
        }

        /// <summary>
        /// Builds a shading model consisting of a grid of horizontal receiver panels overlaid by two elevated shading-only canopies.
        /// </summary>
        /// <param name="countX">The number of receiver panels along the X axis.</param>
        /// <param name="countY">The number of receiver panels along the Y axis.</param>
        /// <returns>A populated <see cref="ShadingModel"/> ready to be solved.</returns>
        private static ShadingModel CreatePerformanceShadingModel(int countX, int countY)
        {
            Coordinates coordinates = new(50.0, 20.0);
            ShadingModel shadingModel = new(Core.Enums.UTC.Plus0100, coordinates);

            Vector3D vector3D_Normal = new(0.0, 0.0, 1.0);

            double size = 4.0;
            double gap = 1.0;
            double step = size + gap;

            for (int i = 0; i < countX; i++)
            {
                for (int j = 0; j < countY; j++)
                {
                    double x = i * step;
                    double y = j * step;

                    DiGi.Geometry.Spatial.Classes.Plane plane = new(new Point3D(x, y, 0.0), vector3D_Normal);
                    DiGi.Geometry.Spatial.Classes.PolygonalFace3D? polygonalFace3D = DiGi.Geometry.Spatial.Create.PolygonalFace3D(plane, new Point2D(0.0, 0.0), new Point2D(size, 0.0), new Point2D(size, size), new Point2D(0.0, size));
                    if (polygonalFace3D == null)
                    {
                        continue;
                    }

                    ShadingElement shadingElement = new(polygonalFace3D, false);
                    shadingModel.Update(shadingElement);
                }
            }

            double width = countX * step;
            double depth = countY * step;

            for (int k = 0; k < 2; k++)
            {
                DiGi.Geometry.Spatial.Classes.Plane plane = new(new Point3D(0.0, 0.0, 3.0 + k), vector3D_Normal);
                DiGi.Geometry.Spatial.Classes.PolygonalFace3D? polygonalFace3D = DiGi.Geometry.Spatial.Create.PolygonalFace3D(plane, new Point2D(-5.0, -5.0), new Point2D(width + 5.0, -5.0), new Point2D(width + 5.0, depth + 5.0), new Point2D(-5.0, depth + 5.0));
                if (polygonalFace3D == null)
                {
                    continue;
                }

                ShadingElement shadingElement = new(polygonalFace3D, true);
                shadingModel.Update(shadingElement);
            }

            return shadingModel;
        }

        /// <summary>
        /// Builds a series of date-times spanning a single summer day at the configured step, covering the full daylight range.
        /// </summary>
        /// <param name="stepMinutes">The interval between consecutive date-times, in minutes.</param>
        /// <returns>An array of date-times across the day.</returns>
        private static DateTime[] CreateDaytimeSeries(int stepMinutes)
        {
            List<DateTime> dateTimes = [];

            DateTime dateTime = new(2026, 6, 26, 4, 0, 0);
            DateTime dateTime_End = new(2026, 6, 26, 21, 0, 0);

            while (dateTime <= dateTime_End)
            {
                dateTimes.Add(dateTime);
                dateTime = dateTime.AddMinutes(stepMinutes);
            }

            return [.. dateTimes];
        }

        /// <summary>
        /// Verifies the shading union de-duplicates overlapping obstacles: two coincident canopies produce the same shading factor as a single canopy, never double.
        /// <para>The canopy only partially covers the receiver, so a summing (double-counting) bug would push the factor toward twice its correct value. This guards the post-processing union semantics end-to-end through the GPU pipeline.</para>
        /// </summary>
        [Fact]
        [SupportedOSPlatform("windows")]
        public void ShadingSolver_Solve_OverlapNotDoubleCounted()
        {
            if (!IsComputeSharpSupported(testOutputHelper))
            {
                testOutputHelper.WriteLine("Skipping ShadingSolver_Solve_OverlapNotDoubleCounted because ComputeSharp is not supported on this machine.");
                return;
            }

            DateTime dateTime_Noon = new(2026, 6, 26, 12, 0, 0);

            double factor_OneCanopy = SolvePartialShadingFactor(1, dateTime_Noon);
            double factor_TwoCanopies = SolvePartialShadingFactor(2, dateTime_Noon);

            // Partial coverage: a double-counting bug would roughly double this value.
            Assert.True(factor_OneCanopy > 0.02 && factor_OneCanopy < 0.5, $"Expected partial shading, got {factor_OneCanopy}.");

            // Coincident canopies cast identical shadows; the union must collapse them rather than sum.
            Assert.Equal(factor_OneCanopy, factor_TwoCanopies, 3);
        }

        /// <summary>
        /// Builds a model with one large receiver partially covered by a number of coincident shading-only canopies, solves it, and returns the receiver's shading factor at the given time.
        /// </summary>
        /// <param name="canopyCount">The number of identical, fully overlapping canopies to add.</param>
        /// <param name="dateTime">The date and time to evaluate.</param>
        /// <returns>The shading factor for the receiver, or <see cref="double.NaN"/> if it could not be evaluated.</returns>
        [SupportedOSPlatform("windows")]
        private static double SolvePartialShadingFactor(int canopyCount, DateTime dateTime)
        {
            Coordinates coordinates = new(50.0, 20.0);
            ShadingModel shadingModel = new(Core.Enums.UTC.Plus0100, coordinates);

            Vector3D vector3D_Normal = new(0.0, 0.0, 1.0);

            DiGi.Geometry.Spatial.Classes.Plane plane_Receiver = new(new Point3D(0.0, 0.0, 0.0), vector3D_Normal);
            DiGi.Geometry.Spatial.Classes.PolygonalFace3D? polygonalFace3D_Receiver = DiGi.Geometry.Spatial.Create.PolygonalFace3D(plane_Receiver, new Point2D(0.0, 0.0), new Point2D(10.0, 0.0), new Point2D(10.0, 10.0), new Point2D(0.0, 10.0));
            if (polygonalFace3D_Receiver == null)
            {
                return double.NaN;
            }

            ShadingElement shadingElement_Receiver = new(polygonalFace3D_Receiver, false);
            shadingModel.Update(shadingElement_Receiver);

            for (int i = 0; i < canopyCount; i++)
            {
                DiGi.Geometry.Spatial.Classes.Plane plane_Canopy = new(new Point3D(0.0, 0.0, 2.0), vector3D_Normal);
                DiGi.Geometry.Spatial.Classes.PolygonalFace3D? polygonalFace3D_Canopy = DiGi.Geometry.Spatial.Create.PolygonalFace3D(plane_Canopy, new Point2D(3.0, 3.0), new Point2D(7.0, 3.0), new Point2D(7.0, 7.0), new Point2D(3.0, 7.0));
                if (polygonalFace3D_Canopy == null)
                {
                    continue;
                }

                ShadingElement shadingElement_Canopy = new(polygonalFace3D_Canopy, true);
                shadingModel.Update(shadingElement_Canopy);
            }

            ShadingSolver shadingSolver = new(shadingModel, [dateTime]);
            shadingSolver.Solve();

            shadingModel.TryGetShadingFactor(shadingElement_Receiver, dateTime, out double factor, false);
            return factor;
        }

        /// <summary>
        /// Verifies that an interior void in a shadow is preserved by the hole-aware union.
        /// <para>Four shading-only bars form a square frame above the receiver, casting a ring shadow with an unshaded centre. The shaded fraction therefore reflects the ring area (~48/196 ≈ 0.245); had the void been filled it would be ~64/196 ≈ 0.327.</para>
        /// </summary>
        [Fact]
        [SupportedOSPlatform("windows")]
        public void ShadingSolver_Solve_HolePreserved()
        {
            if (!IsComputeSharpSupported(testOutputHelper))
            {
                testOutputHelper.WriteLine("Skipping ShadingSolver_Solve_HolePreserved because ComputeSharp is not supported on this machine.");
                return;
            }

            Coordinates coordinates = new(50.0, 20.0);
            ShadingModel shadingModel = new(Core.Enums.UTC.Plus0100, coordinates);

            Vector3D vector3D_Normal = new(0.0, 0.0, 1.0);

            DiGi.Geometry.Spatial.Classes.Plane plane_Receiver = new(new Point3D(0.0, 0.0, 0.0), vector3D_Normal);
            DiGi.Geometry.Spatial.Classes.PolygonalFace3D? polygonalFace3D_Receiver = DiGi.Geometry.Spatial.Create.PolygonalFace3D(plane_Receiver, new Point2D(0.0, 0.0), new Point2D(14.0, 0.0), new Point2D(14.0, 14.0), new Point2D(0.0, 14.0));
            Assert.NotNull(polygonalFace3D_Receiver);
            ShadingElement shadingElement_Receiver = new(polygonalFace3D_Receiver, false);
            Assert.True(shadingModel.Update(shadingElement_Receiver));

            // Four shading-only bars forming a square frame: outer 8x8, inner 4x4 void, at z = 2.
            double[][] bars =
            [
                [3.0, 3.0, 11.0, 5.0],
                [3.0, 9.0, 11.0, 11.0],
                [3.0, 5.0, 5.0, 9.0],
                [9.0, 5.0, 11.0, 9.0]
            ];

            foreach (double[] bar in bars)
            {
                DiGi.Geometry.Spatial.Classes.Plane plane_Bar = new(new Point3D(0.0, 0.0, 2.0), vector3D_Normal);
                DiGi.Geometry.Spatial.Classes.PolygonalFace3D? polygonalFace3D_Bar = DiGi.Geometry.Spatial.Create.PolygonalFace3D(plane_Bar, new Point2D(bar[0], bar[1]), new Point2D(bar[2], bar[1]), new Point2D(bar[2], bar[3]), new Point2D(bar[0], bar[3]));
                Assert.NotNull(polygonalFace3D_Bar);
                ShadingElement shadingElement_Bar = new(polygonalFace3D_Bar, true);
                Assert.True(shadingModel.Update(shadingElement_Bar));
            }

            DateTime dateTime_Noon = new(2026, 6, 26, 12, 0, 0);
            ShadingSolver shadingSolver = new(shadingModel, [dateTime_Noon]);
            Assert.True(shadingSolver.Solve());

            bool hasFactor = shadingModel.TryGetShadingFactor(shadingElement_Receiver, dateTime_Noon, out double factor, false);
            Assert.True(hasFactor);

            Assert.True(factor > 0.15, $"Expected a visible ring shadow, got {factor}.");
            Assert.True(factor < 0.30, $"Expected the unshaded centre to be preserved, got {factor}.");
        }
    }
}