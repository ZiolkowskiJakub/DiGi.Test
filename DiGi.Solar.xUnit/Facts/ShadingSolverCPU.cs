using DiGi.Core.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Solar.Classes;
using DiGi.Solar.ComputeSharp.Enums;
using System.Runtime.Versioning;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the CPU <see cref="ShadingSolver"/> reproduces the ComputeSharp solver: the same shading factor for every receiver and daytime timestamp.
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
        /// Verifies that only the part of a caster on the sun side of the receiver plane casts a shadow.
        /// <para>Geometry: a 10 x 10 m horizontal receiver at z = 0 and a shading-only wall in the plane y = 5, spanning x in [2, 8] and z in [-3, 3], so it pierces the receiver.
        /// At noon on 2026-06-26 at (50.0, 20.0) only its upper half (z in [0, 3]) is between the sun and the receiver; its shadow is a parallelogram of area 6 * 3 * |v.Y / v.Z| north of the wall.
        /// Counting the lower half as well would double it.</para>
        /// </summary>
        [Fact]
        public void ShadingSolver_Solve_CasterCrossingPlane()
        {
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

            ShadingSolver shadingSolver = new(shadingModel, [dateTime_Noon]);
            Assert.True(shadingSolver.Solve());

            Assert.True(shadingModel.TryGetShadingFactor(shadingElement_Receiver, dateTime_Noon, out double factor, false));

            double factor_Expected = 6.0 * 3.0 * Math.Abs(sunDirection.Y / sunDirection.Z) / 100.0;
            testOutputHelper.WriteLine($"Crossing caster: factor {factor}, expected {factor_Expected}.");

            Assert.Equal(factor_Expected, factor, 6);
        }

        /// <summary>
        /// Verifies the device selection of the ComputeSharp solver: <see cref="ComputeDeviceType.Software"/> selects the WARP device, <see cref="ComputeDeviceType.Hardware"/> a hardware-accelerated one,
        /// and a solve forced onto the hardware device matches the CPU solver.
        /// <para>No solve runs on WARP: the shading shaders are double precision, and a WARP solve of a single receiver did not finish within minutes on the development machine, so the CPU solver is the software path.</para>
        /// </summary>
        [Fact]
        [SupportedOSPlatform("windows")]
        public void ShadingSolver_ComputeDeviceType()
        {
            global::ComputeSharp.GraphicsDevice? graphicsDevice_Software = ComputeSharp.Create.GraphicsDevice(ComputeDeviceType.Software);
            if (graphicsDevice_Software is null)
            {
                testOutputHelper.WriteLine("No WARP device is available on this machine.");
            }
            else
            {
                Assert.False(graphicsDevice_Software.IsHardwareAccelerated);
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
        /// Solves a copy of the model with the CPU solver and another copy with the ComputeSharp solver, and asserts every receiver has the same shading factor at every timestamp.
        /// </summary>
        /// <param name="shadingModel">The model to solve; it is copied, never solved itself.</param>
        /// <param name="dateTimes">The date-times to solve.</param>
        /// <param name="computeDeviceType">The ComputeSharp device to use, or null for the solver's default.</param>
        [SupportedOSPlatform("windows")]
        private void AssertSameShadingFactors(ShadingModel shadingModel, DateTime[] dateTimes, ComputeDeviceType? computeDeviceType)
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

            List<ShadingElement>? receivers = shadingModel_CPU.GetShadingElements<ShadingElement>(shadingOnly: false);
            Assert.NotNull(receivers);
            Assert.NotEmpty(receivers);

            int count = 0;
            double difference_Max = 0.0;
            foreach (ShadingElement receiver in receivers)
            {
                foreach (DateTime dateTime in dateTimes)
                {
                    bool hasFactor_CPU = shadingModel_CPU.TryGetShadingFactor(receiver, dateTime, out double factor_CPU, false);
                    bool hasFactor_ComputeSharp = shadingModel_ComputeSharp.TryGetShadingFactor(receiver, dateTime, out double factor_ComputeSharp, false);

                    Assert.Equal(hasFactor_ComputeSharp, hasFactor_CPU);
                    if (!hasFactor_CPU)
                    {
                        continue;
                    }

                    difference_Max = Math.Max(difference_Max, Math.Abs(factor_CPU - factor_ComputeSharp));
                    count++;
                }
            }

            testOutputHelper.WriteLine($"Compared {count} factors across {receivers.Count} receivers; largest difference {difference_Max}.");

            Assert.True(count > 0);
            Assert.True(difference_Max < 1e-6, $"CPU and ComputeSharp shading factors differ by up to {difference_Max}.");
        }
    }
}
