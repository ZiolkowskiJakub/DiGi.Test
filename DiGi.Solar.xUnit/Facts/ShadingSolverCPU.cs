using DiGi.Core.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Solar.Classes;
using DiGi.Solar.ComputeSharp.Enums;
using System.Reflection;
using System.Runtime.Versioning;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the CPU <see cref="ShadingSolver"/> reproduces the ComputeSharp solver: the same shading factor for every receiver at every daytime timestamp, including those with the sun behind the receiver.
        /// <para>Covers horizontal receivers under shading-only canopies (the 4 x 3 performance grid over an hourly day) and vertical shading-only walls casting oblique shadows (the sunlit-gap fixture).</para>
        /// </summary>
        [Fact]
        [SupportedOSPlatform("windows")]
        public void ShadingSolver_Solve_MatchesComputeSharp()
        {
            if (!IsComputeSharpSupported(testOutputHelper))
            {
                testOutputHelper.WriteLine("Skipping ShadingSolver_Solve_MatchesComputeSharp because ComputeSharp is not supported on this machine.");
                return;
            }

            AssertSameShadingFactors(CreatePerformanceShadingModel(4, 3), CreateDaytimeSeries(60), null);

            ShadingModel shadingModel_Walls = new(Core.Enums.UTC.Plus0100, new Coordinates(50.0, 20.0));
            Plane plane_Receiver = new(new Point3D(0.0, 0.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
            PolygonalFace3D? polygonalFace3D_Receiver = Geometry.Spatial.Create.PolygonalFace3D(plane_Receiver, new Point2D(0.0, 0.0), new Point2D(4.0, 0.0), new Point2D(4.0, 4.0), new Point2D(0.0, 4.0));
            Assert.NotNull(polygonalFace3D_Receiver);
            Assert.True(shadingModel_Walls.Update(new ShadingElement(polygonalFace3D_Receiver, false)));
            CreateVerticalWall(shadingModel_Walls, 6.0);
            CreateVerticalWall(shadingModel_Walls, -2.0);

            AssertSameShadingFactors(shadingModel_Walls, CreateDaytimeSeries(60), null);
        }

        /// <summary>
        /// Verifies that a receiver with the sun behind it (sun direction · normal ≥ 0) is reported fully shaded by both solvers (ZiolkowskiJakub/DiGi.Solar#9).
        /// <para>Geometry: one 10 x 10 x 12 m box (<c>CreateBuildingGridShadingModel(1, false)</c>) on 2026-06-26, hourly, at (50.0, 20.0). Each wall with the sun behind it lies wholly in the shadow of the rest of the box, so its factor is 1.
        /// The ComputeSharp solver used to decide per receiver triangle from the centroid of each intersection piece and reported 0.5 to 0.93 on such samples; since ZiolkowskiJakub/DiGi.Solar#11 it clips and projects every caster exactly as the CPU solver does.</para>
        /// </summary>
        /// <param name="computeSharp">True to solve with the ComputeSharp solver; false for the CPU solver.</param>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [SupportedOSPlatform("windows")]
        public void ShadingSolver_Solve_BackFacingFullyShaded(bool computeSharp)
        {
            if (!IsShadingSolverSupported(computeSharp, testOutputHelper))
            {
                testOutputHelper.WriteLine("Skipping ShadingSolver_Solve_BackFacingFullyShaded because the requested solver is not supported on this machine.");
                return;
            }

            ShadingModel shadingModel = CreateBuildingGridShadingModel(1, false);
            DateTime[] dateTimes = CreateDaytimeSeries(60);

            Assert.True(CreateShadingSolver(shadingModel, dateTimes, computeSharp).Solve());

            List<ShadingElement>? receivers = shadingModel.GetShadingElements<ShadingElement>(shadingOnly: false);
            Assert.NotNull(receivers);
            Assert.NotEmpty(receivers);

            int count_BackFacing = 0;
            List<string> failures = [];
            foreach (ShadingElement receiver in receivers)
            {
                Vector3D? normal = receiver.PolygonalFace3D?.Plane?.Normal;
                Assert.NotNull(normal);

                foreach (DateTime dateTime in dateTimes)
                {
                    if (Query.SunDirection(shadingModel, dateTime, false) is not Vector3D sunDirection || sunDirection.DotProduct(normal) < 0)
                    {
                        continue;
                    }

                    count_BackFacing++;

                    Assert.True(shadingModel.TryGetShadingFactor(receiver, dateTime, out double factor, false), $"No factor at {dateTime:yyyy-MM-dd HH:mm}.");
                    if (Math.Abs(factor - 1.0) > 1e-9)
                    {
                        failures.Add($"normal ({normal.X:F0}, {normal.Y:F0}, {normal.Z:F0}) at {dateTime:HH:mm}: {factor}");
                    }
                }
            }

            testOutputHelper.WriteLine($"{count_BackFacing} back-facing samples, {failures.Count} not fully shaded.");
            foreach (string failure in failures)
            {
                testOutputHelper.WriteLine(failure);
            }

            Assert.True(count_BackFacing > 0);
            Assert.Empty(failures);
        }

        /// <summary>
        /// Verifies that only the part of a caster on the sun side of the receiver plane casts a shadow.
        /// <para>Geometry: a 10 x 10 m horizontal receiver at z = 0 and a shading-only wall in the plane y = 5, spanning x in [2, 8] and z in [-3, 3], so it pierces the receiver.
        /// At noon on 2026-06-26 at (50.0, 20.0) only its upper half (z in [0, 3]) is between the sun and the receiver; its shadow is a parallelogram of area 6 * 3 * |v.Y / v.Z| north of the wall.
        /// Counting the lower half as well would double it.</para>
        /// <para>Both solvers clip the caster the same way since ZiolkowskiJakub/DiGi.Solar#11; before it the ComputeSharp solver decided per intersection piece from its centroid.</para>
        /// </summary>
        /// <param name="computeSharp">True to solve with the ComputeSharp solver; false for the CPU solver.</param>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [SupportedOSPlatform("windows")]
        public void ShadingSolver_Solve_CasterCrossingPlane(bool computeSharp)
        {
            if (!IsShadingSolverSupported(computeSharp, testOutputHelper))
            {
                testOutputHelper.WriteLine("Skipping ShadingSolver_Solve_CasterCrossingPlane because the requested solver is not supported on this machine.");
                return;
            }

            ShadingModel shadingModel = new(Core.Enums.UTC.Plus0100, new Coordinates(50.0, 20.0));

            Plane plane_Receiver = new(new Point3D(0.0, 0.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
            PolygonalFace3D? polygonalFace3D_Receiver = Geometry.Spatial.Create.PolygonalFace3D(plane_Receiver, new Point2D(0.0, 0.0), new Point2D(10.0, 0.0), new Point2D(10.0, 10.0), new Point2D(0.0, 10.0));
            Assert.NotNull(polygonalFace3D_Receiver);
            ShadingElement shadingElement_Receiver = new(polygonalFace3D_Receiver, false);
            Assert.True(shadingModel.Update(shadingElement_Receiver));

            Plane plane_Wall = new(new Point3D(0.0, 5.0, 0.0), new Vector3D(0.0, 1.0, 0.0));
            List<Point2D> point2Ds = [];
            foreach (Point3D point3D in new Point3D[] { new(2.0, 5.0, -3.0), new(8.0, 5.0, -3.0), new(8.0, 5.0, 3.0), new(2.0, 5.0, 3.0) })
            {
                Point2D? point2D = Geometry.Spatial.Query.Convert(plane_Wall, point3D);
                Assert.NotNull(point2D);
                point2Ds.Add(point2D);
            }

            PolygonalFace3D? polygonalFace3D_Wall = Geometry.Spatial.Create.PolygonalFace3D(new Polygon3D(plane_Wall, point2Ds), []);
            Assert.NotNull(polygonalFace3D_Wall);
            Assert.True(shadingModel.Update(new ShadingElement(polygonalFace3D_Wall, true)));

            DateTime dateTime_Noon = new(2026, 6, 26, 12, 0, 0);
            Vector3D? sunDirection = Query.SunDirection(shadingModel, dateTime_Noon, false);
            Assert.NotNull(sunDirection);

            Assert.True(CreateShadingSolver(shadingModel, [dateTime_Noon], computeSharp).Solve());

            Assert.True(shadingModel.TryGetShadingFactor(shadingElement_Receiver, dateTime_Noon, out double factor, false));

            double factor_Expected = 6.0 * 3.0 * Math.Abs(sunDirection.Y / sunDirection.Z) / 100.0;
            testOutputHelper.WriteLine($"Crossing caster: factor {factor}, expected {factor_Expected}.");

            Assert.Equal(factor_Expected, factor, 6);
        }

        /// <summary>
        /// Verifies the device selection of the ComputeSharp solver: the WARP software device is never selected, <see cref="ComputeDeviceType.Hardware"/> selects a hardware-accelerated device,
        /// and a solve forced onto the hardware device matches the CPU solver.
        /// <para>WARP loses shadows cast between buildings and needs about 16 minutes to create its pipeline (ZiolkowskiJakub/DiGi.Solar#10), so the obsolete <c>ComputeDeviceType.Software</c> gets no device
        /// and its solve returns false without assigning results, while <see cref="ComputeDeviceType.Default"/> gets no device rather than WARP on a machine without a hardware adapter.</para>
        /// </summary>
        [Fact]
        [SupportedOSPlatform("windows")]
        public void ShadingSolver_ComputeDeviceType()
        {
#pragma warning disable CS0618 // ComputeDeviceType.Software is obsolete; this fact verifies it is withdrawn (ZiolkowskiJakub/DiGi.Solar#10).
            ComputeDeviceType computeDeviceType_Software = ComputeDeviceType.Software;
#pragma warning restore CS0618

            Assert.Null(ComputeSharp.Create.GraphicsDevice(computeDeviceType_Software));

            ShadingModel shadingModel_Software = CreatePerformanceShadingModel(1, 1);
            DateTime[] dateTimes_Software = CreateDaytimeSeries(120);
            Assert.False(new ComputeSharp.Classes.ShadingSolver(shadingModel_Software, dateTimes_Software) { ComputeDeviceType = computeDeviceType_Software }.Solve());

            List<ShadingElement>? shadingElements_Software = shadingModel_Software.GetShadingElements<ShadingElement>(shadingOnly: false);
            Assert.NotNull(shadingElements_Software);
            Assert.NotEmpty(shadingElements_Software);
            foreach (ShadingElement shadingElement in shadingElements_Software)
            {
                foreach (DateTime dateTime in dateTimes_Software)
                {
                    Assert.False(shadingModel_Software.TryGetShadingFactor(shadingElement, dateTime, out _, false));
                }
            }

            global::ComputeSharp.GraphicsDevice? graphicsDevice_Default = ComputeSharp.Create.GraphicsDevice(ComputeDeviceType.Default);
            if (graphicsDevice_Default is not null)
            {
                Assert.True(graphicsDevice_Default.IsHardwareAccelerated);
            }

            global::ComputeSharp.GraphicsDevice? graphicsDevice_Hardware = ComputeSharp.Create.GraphicsDevice(ComputeDeviceType.Hardware);
            if (graphicsDevice_Hardware is null)
            {
                testOutputHelper.WriteLine("Skipping the hardware solve of ShadingSolver_ComputeDeviceType because no hardware device with double precision is available on this machine.");
                return;
            }

            Assert.True(graphicsDevice_Hardware.IsHardwareAccelerated);

            AssertSameShadingFactors(CreatePerformanceShadingModel(3, 3), CreateDaytimeSeries(120), ComputeDeviceType.Hardware);
        }

        /// <summary>
        /// Percentile of the per-sample |CPU - other| shading factor differences that <see cref="ShadingSolverParityPercentileBound"/> applies to.
        /// </summary>
        private const double ShadingSolverParityPercentile = 0.99;

        /// <summary>
        /// Largest allowed <see cref="ShadingSolverParityPercentile"/> of the per-sample |CPU - other| shading factor differences, asserted once at least <see cref="ShadingSolverParityMinimumCount"/> samples are compared.
        /// <para>Measured for ZiolkowskiJakub/DiGi.Solar#11 on an RTX 5090 over every parity comparison of the suite, the benchmark grids of 5 to 720 surfaces included (up to 11 520 samples, 40 % of them back-facing): 96 to 100 % of the samples are exactly equal, the p99 is 1.1e-16 and the maximum 2.2e-16.
        /// The bound leaves four orders of magnitude for other devices and drivers and still fails on any error in the geometry.</para>
        /// </summary>
        private const double ShadingSolverParityPercentileBound = 1e-12;

        /// <summary>
        /// Largest allowed |CPU - other| shading factor difference of any single sample: a larger one is a different answer, not round-off (measured maximum 2.2e-16, see <see cref="ShadingSolverParityPercentileBound"/>).
        /// <para>It also separates a dropped or phantom shadow from agreement: a shadow cast on the tolerance edge (area about the square of the 1e-6 m distance tolerance) moves a factor by far less.</para>
        /// </summary>
        private const double ShadingSolverParitySampleBound = 1e-9;

        /// <summary>
        /// Smallest number of compared samples for which the percentile bound is asserted; below it a percentile is just the maximum again.
        /// </summary>
        private const int ShadingSolverParityMinimumCount = 100;

        /// <summary>
        /// Solves a copy of the model with the CPU solver and another copy with the ComputeSharp solver, and asserts every receiver has the same shading factor at every daytime timestamp.
        /// </summary>
        /// <param name="shadingModel">The model to solve; it is copied, never solved itself.</param>
        /// <param name="dateTimes">The date-times to solve.</param>
        /// <param name="computeDeviceType">The ComputeSharp device to use, or null for the solver's default.</param>
        /// <param name="droppedFraction">The largest allowed fraction of compared samples in which the ComputeSharp solver reports no shadow (factor 0) where the CPU solver reports one; see the overload taking two solved models.</param>
        /// <returns>The number of compared samples, the number of dropped shadows among them, and the <see cref="ShadingSolverParityPercentile"/> and maximum of the factor differences.</returns>
        [SupportedOSPlatform("windows")]
        private (int Compared, int Dropped, double Percentile, double Max) AssertSameShadingFactors(ShadingModel shadingModel, DateTime[] dateTimes, ComputeDeviceType? computeDeviceType, double droppedFraction = 0.0)
        {
            ShadingModel shadingModel_CPU = new(shadingModel);
            ShadingModel shadingModel_ComputeSharp = new(shadingModel);

            Assert.True(new ShadingSolver(shadingModel_CPU, dateTimes).Solve());

            ComputeSharp.Classes.ShadingSolver shadingSolver_ComputeSharp = new(shadingModel_ComputeSharp, dateTimes);
            if (computeDeviceType is not null)
            {
                shadingSolver_ComputeSharp.ComputeDeviceType = computeDeviceType.Value;
            }

            Assert.True(shadingSolver_ComputeSharp.Solve());

            return AssertSameShadingFactors(shadingModel_CPU, shadingModel_ComputeSharp, dateTimes, droppedFraction);
        }

        /// <summary>
        /// Asserts that two solved copies of the same model give every receiver the same shading factor at every daytime timestamp, back-facing samples (sun direction · normal ≥ 0) included.
        /// <para>Since ZiolkowskiJakub/DiGi.Solar#11 both solvers clip and project the same shadows and merge them with the same <see cref="Query.ShadedFaces(PolygonalFace2D?, IEnumerable{PolygonalFace2D}?)"/>, so they differ by floating point round-off only (fused multiply-add on the GPU).
        /// The bulk agreement is bounded by a percentile (<see cref="ShadingSolverParityPercentileBound"/>) and every sample by <see cref="ShadingSolverParitySampleBound"/>; both solvers must also agree on which samples have a factor, and every factor lies in [0, 1].</para>
        /// <para>A dropped shadow is a sample in which the other engine reports factor 0 while the CPU solver reports shade, and a phantom shadow the reverse. A dropped shadow was the signature of a shadow union that failed and was reported as full sun: one east wall of the 720-surface benchmark grid at a 3 degree sun read 0 against 0.9367 on the CPU solver and on the nine walls of its row with the same neighbour to the east. It was fixed in ZiolkowskiJakub/DiGi.Geometry#8 (the union now retries with snap-rounding) and ZiolkowskiJakub/DiGi.Solar#8 (a union that still fails falls back to the unmerged shadows), so the count is expected to stay at 0.</para>
        /// <para>Dropped shadows are counted separately from the factor differences because losing a shadow and disagreeing about one are different failures; <paramref name="droppedFraction"/> bounds the first, a phantom shadow always fails, and every other disagreement fails on the bounds.</para>
        /// </summary>
        /// <param name="shadingModel_CPU">The model solved by the CPU solver.</param>
        /// <param name="shadingModel_Other">The same model solved by another engine.</param>
        /// <param name="dateTimes">The solved date-times.</param>
        /// <param name="droppedFraction">The largest allowed fraction of compared samples with a dropped shadow; 0 requires exact agreement.</param>
        /// <returns>The number of compared samples, the number of dropped shadows among them, and the <see cref="ShadingSolverParityPercentile"/> and maximum of the factor differences.</returns>
        private (int Compared, int Dropped, double Percentile, double Max) AssertSameShadingFactors(ShadingModel shadingModel_CPU, ShadingModel shadingModel_Other, DateTime[] dateTimes, double droppedFraction = 0.0)
        {
            List<ShadingElement>? receivers = shadingModel_CPU.GetShadingElements<ShadingElement>(shadingOnly: false);
            Assert.NotNull(receivers);
            Assert.NotEmpty(receivers);

            int count = 0;
            int count_BackFacing = 0;
            int count_Dropped = 0;
            int count_Phantom = 0;
            List<double> differences = [];
            foreach (ShadingElement receiver in receivers)
            {
                Vector3D? normal = receiver.PolygonalFace3D?.Plane?.Normal;
                Assert.NotNull(normal);

                foreach (DateTime dateTime in dateTimes)
                {
                    bool hasFactor_CPU = shadingModel_CPU.TryGetShadingFactor(receiver, dateTime, out double factor_CPU, false);
                    bool hasFactor_Other = shadingModel_Other.TryGetShadingFactor(receiver, dateTime, out double factor_Other, false);

                    Assert.Equal(hasFactor_Other, hasFactor_CPU);
                    if (!hasFactor_CPU)
                    {
                        continue;
                    }

                    Assert.InRange(factor_CPU, 0.0, 1.0 + ShadingSolverParitySampleBound);
                    Assert.InRange(factor_Other, 0.0, 1.0 + ShadingSolverParitySampleBound);

                    Vector3D? sunDirection = Query.SunDirection(shadingModel_CPU, dateTime, false);
                    Assert.NotNull(sunDirection);
                    if (sunDirection.DotProduct(normal) >= 0)
                    {
                        count_BackFacing++;
                    }

                    count++;

                    double difference = Math.Abs(factor_CPU - factor_Other);
                    if (difference >= ShadingSolverParitySampleBound && factor_Other == 0.0)
                    {
                        count_Dropped++;
                        testOutputHelper.WriteLine($"Dropped shadow: receiver with normal ({normal.X:F2}, {normal.Y:F2}, {normal.Z:F2}) at {dateTime:yyyy-MM-dd HH:mm}, CPU {factor_CPU}, other 0.");
                        continue;
                    }

                    if (difference >= ShadingSolverParitySampleBound && factor_CPU == 0.0)
                    {
                        count_Phantom++;
                        testOutputHelper.WriteLine($"Phantom shadow: receiver with normal ({normal.X:F2}, {normal.Y:F2}, {normal.Z:F2}) at {dateTime:yyyy-MM-dd HH:mm}, CPU 0, other {factor_Other}.");
                    }

                    differences.Add(difference);
                }
            }

            differences.Sort();
            double difference_Median = differences.Count == 0 ? 0.0 : differences[(differences.Count - 1) / 2];
            double difference_Percentile = differences.Count == 0 ? 0.0 : differences[Math.Max(0, (int)Math.Ceiling(ShadingSolverParityPercentile * differences.Count) - 1)];
            double difference_Max = differences.Count == 0 ? 0.0 : differences[^1];
            int count_Zero = differences.Count(difference => difference == 0.0);

            testOutputHelper.WriteLine($"Compared {count} factors across {receivers.Count} receivers ({count_BackFacing} back-facing, {count_Dropped} dropped shadows, {count_Phantom} phantom shadows); |CPU - other|: {count_Zero} exactly 0, median {difference_Median:E2}, p{ShadingSolverParityPercentile * 100:F0} {difference_Percentile:E2}, max {difference_Max:E2}.");

            Assert.True(count > 0);
            Assert.True(count_Dropped <= droppedFraction * count, $"{count_Dropped} of {count} samples lost their shadow on the other engine.");
            Assert.True(count_Phantom == 0, $"{count_Phantom} of {count} samples gained a shadow on the other engine.");
            Assert.True(difference_Max < ShadingSolverParitySampleBound, $"CPU and ComputeSharp shading factors differ by up to {difference_Max}.");
            if (differences.Count >= ShadingSolverParityMinimumCount)
            {
                Assert.True(difference_Percentile <= ShadingSolverParityPercentileBound, $"The p{ShadingSolverParityPercentile * 100:F0} of the CPU and ComputeSharp shading factor differences is {difference_Percentile}.");
            }

            return (count, count_Dropped, difference_Percentile, difference_Max);
        }

        /// <summary>
        /// Verifies that the CPU <see cref="ShadingSolver"/> is insensitive to the absolute position of the scene.
        /// <para>Solves the 4 x 4 benchmark grid at the origin and again shifted by (+500 000, +500 000) m - the magnitude of EPSG:2180 coordinates - and compares the shading factors of every sun-facing receiver sample. Both solvers work in double precision, so the shift must change the factors by floating point round-off only: the 95th percentile stays below 1e-9 and the maximum below 1e-6.</para>
        /// <para>If this fails, the scene must be recentred on the model origin before solving - open a separate issue for that instead of fixing it here. The measured figures are written to <c>ShadingSolver_AbsoluteCoordinates.txt</c> in the reports directory.</para>
        /// </summary>
        [Fact]
        public void ShadingSolver_Solve_AbsoluteCoordinates()
        {
            DateTime[] dateTimes = CreateDaytimeSeries(60);

            ShadingModel shadingModel = CreateBuildingGridShadingModel(4, false);
            Assert.True(new ShadingSolver(shadingModel, dateTimes).Solve());

            Vector3D vector3D_Shift = new(500000.0, 500000.0, 0.0);
            ShadingModel shadingModel_Shifted = ShiftShadingModel(shadingModel, vector3D_Shift);
            Assert.True(new ShadingSolver(shadingModel_Shifted, dateTimes).Solve());

            List<ShadingElement>? receivers = shadingModel.GetShadingElements<ShadingElement>(shadingOnly: false);
            List<ShadingElement>? receivers_Shifted = shadingModel_Shifted.GetShadingElements<ShadingElement>(shadingOnly: false);
            Assert.NotNull(receivers);
            Assert.NotNull(receivers_Shifted);
            Assert.Equal(receivers.Count, receivers_Shifted.Count);

            List<double> differences = [];
            for (int index = 0; index < receivers.Count; index++)
            {
                ShadingElement receiver = receivers[index];
                ShadingElement receiver_Shifted = receivers_Shifted[index];

                Vector3D? normal = receiver.PolygonalFace3D?.Plane?.Normal;
                Assert.NotNull(normal);

                foreach (DateTime dateTime in dateTimes)
                {
                    bool hasFactor = shadingModel.TryGetShadingFactor(receiver, dateTime, out double factor, false);
                    bool hasFactor_Shifted = shadingModel_Shifted.TryGetShadingFactor(receiver_Shifted, dateTime, out double factor_Shifted, false);
                    Assert.Equal(hasFactor, hasFactor_Shifted);
                    if (!hasFactor)
                    {
                        continue;
                    }

                    Vector3D? sunDirection = Query.SunDirection(shadingModel, dateTime, false);
                    Assert.NotNull(sunDirection);
                    if (sunDirection.DotProduct(normal) >= 0)
                    {
                        continue;
                    }

                    differences.Add(Math.Abs(factor - factor_Shifted));
                }
            }

            Assert.True(differences.Count > 0);

            differences.Sort();
            double difference_95thPercentile = differences[(int)Math.Ceiling(0.95 * differences.Count) - 1];
            double difference_Max = differences[^1];

            string path_Report = System.IO.Path.Combine(Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly())!, "ShadingSolver_AbsoluteCoordinates.txt");
            System.IO.File.WriteAllLines(path_Report, [
                $"{dateTimes.Length} timestamps, {receivers.Count} receivers, {differences.Count} sun-facing samples",
                $"95th percentile difference {difference_95thPercentile:R}",
                $"Maximum difference {difference_Max:R}"
            ]);

            testOutputHelper.WriteLine($"Compared {differences.Count} sun-facing factors across {receivers.Count} receivers after a shift of {vector3D_Shift.X} m; 95th percentile {difference_95thPercentile:R}, maximum {difference_Max}.");

            Assert.True(difference_95thPercentile < 1e-9, $"The 95th percentile shading factor difference is {difference_95thPercentile} after shifting the scene by {vector3D_Shift.X} m.");
            Assert.True(difference_Max < 1e-6, $"The largest shading factor difference is {difference_Max} after shifting the scene by {vector3D_Shift.X} m.");
        }

        /// <summary>
        /// Builds a copy of a shading model with every element translated by a vector, keeping the coordinates and the UTC offset.
        /// </summary>
        /// <param name="shadingModel">The shading model to shift.</param>
        /// <param name="vector3D_Shift">The translation vector.</param>
        /// <returns>The shifted shading model.</returns>
        private static ShadingModel ShiftShadingModel(ShadingModel shadingModel, Vector3D vector3D_Shift)
        {
            ShadingModel shadingModel_Shifted = new(shadingModel.UTC, shadingModel.Coordinates);

            List<ShadingElement>? shadingElements = shadingModel.GetShadingElements<ShadingElement>();
            Assert.NotNull(shadingElements);
            foreach (ShadingElement shadingElement in shadingElements)
            {
                PolygonalFace3D? polygonalFace3D = shadingElement.PolygonalFace3D as PolygonalFace3D;
                Assert.NotNull(polygonalFace3D);
                Assert.True(polygonalFace3D.Move(vector3D_Shift));
                Assert.True(shadingModel_Shifted.Update(new ShadingElement(polygonalFace3D, shadingElement.ShadingOnly)));
            }

            return shadingModel_Shifted;
        }
    }
}
