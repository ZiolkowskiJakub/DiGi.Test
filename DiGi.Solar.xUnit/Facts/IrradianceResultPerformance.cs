using DiGi.Core.Classes;
using DiGi.Geometry.Spatial.Classes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Number of hourly evaluations measured by the irradiance throughput benchmark.
        /// </summary>
        private const int IrradiancePerformanceCount = 1000000;

        /// <summary>
        /// Number of distinct hourly timestamps the sun direction is evaluated for, matching a year of hourly weather data.
        /// </summary>
        private const int IrradiancePerformanceHourCount = 8760;

        /// <summary>
        /// Measures the per-call cost of the irradiance layer, and separates it from the cost of the sun position it consumes.
        /// <para>The three measured paths are the irradiance factory alone, the power factory alone, and the whole per-hour pipeline including <see cref="Query.SunDirection(Coordinates, Core.Enums.UTC, System.DateTime, bool)"/>. The comparison shows what a caller pays for a sun direction it could have cached, which is the decision the benchmark exists to inform.</para>
        /// </summary>
        [Fact]
        public void IrradianceResult_Performance()
        {
            Coordinates coordinates = new(52.4064, 16.9252);
            Vector3D vector3D_SurfaceNormal = SurfaceNormal_Tilted35South();
            System.DateTime dateTime_Base = new(2020, 6, 21, 0, 0, 0);

            // Warm up every path on small input so that JIT compilation is excluded.
            Vector3D? vector3D_Warmup = Query.SunDirection(coordinates, Core.Enums.UTC.PlusMinus0000, dateTime_Base.AddHours(12), true);
            Assert.NotNull(vector3D_Warmup);
            Classes.IrradianceResult? irradianceResult_Warmup = Create.IrradianceResult(vector3D_SurfaceNormal, vector3D_Warmup, 500, 800, 100, 0.2);
            Assert.NotNull(irradianceResult_Warmup);
            Assert.NotNull(Create.SolarPowerResult(irradianceResult_Warmup, 10, 6));
            Assert.NotNull(Create.SolarPowerResult_ByShadingFactor(irradianceResult_Warmup, 10, 0.4));

            // Pre-compute one sun direction per hour of a year, which is what a caller batching a
            // weather file would hold.
            List<Vector3D> vector3Ds_SunDirection = [];
            for (int i = 0; i < IrradiancePerformanceHourCount; i++)
            {
                Vector3D? vector3D = Query.SunDirection(coordinates, Core.Enums.UTC.PlusMinus0000, dateTime_Base.AddHours(i), true);
                Assert.NotNull(vector3D);
                vector3Ds_SunDirection.Add(vector3D);
            }

            // The cached and uncached paths must agree before either is timed.
            Vector3D? vector3D_Direct = Query.SunDirection(coordinates, Core.Enums.UTC.PlusMinus0000, dateTime_Base.AddHours(37), true);
            Assert.NotNull(vector3D_Direct);
            Classes.IrradianceResult? irradianceResult_Cached = Create.IrradianceResult(vector3D_SurfaceNormal, vector3Ds_SunDirection[37], 500, 800, 100, 0.2);
            Classes.IrradianceResult? irradianceResult_Uncached = Create.IrradianceResult(vector3D_SurfaceNormal, vector3D_Direct, 500, 800, 100, 0.2);
            Assert.NotNull(irradianceResult_Cached);
            Assert.NotNull(irradianceResult_Uncached);
            Assert.Equal(irradianceResult_Uncached.Total, irradianceResult_Cached.Total, 12);

            // The two power factories must agree before either is timed.
            Classes.SolarPowerResult? solarPowerResult_ByArea = Create.SolarPowerResult(irradianceResult_Cached, 10, 6);
            Classes.SolarPowerResult? solarPowerResult_ByFactor = Create.SolarPowerResult_ByShadingFactor(irradianceResult_Cached, 10, 0.4);
            Assert.NotNull(solarPowerResult_ByArea);
            Assert.NotNull(solarPowerResult_ByFactor);
            Assert.Equal(solarPowerResult_ByArea.Power, solarPowerResult_ByFactor.Power, 10);

            double sum = 0;

            // Path 1 - the irradiance factory alone, against a cached sun direction.
            Stopwatch stopwatch_Irradiance = Stopwatch.StartNew();
            for (int i = 0; i < IrradiancePerformanceCount; i++)
            {
                Classes.IrradianceResult? irradianceResult = Create.IrradianceResult(vector3D_SurfaceNormal, vector3Ds_SunDirection[i % IrradiancePerformanceHourCount], 500, 800, 100, 0.2);
                sum += irradianceResult!.Total;
            }
            stopwatch_Irradiance.Stop();

            // Path 2 - the irradiance factory followed by the power factory.
            Stopwatch stopwatch_Power = Stopwatch.StartNew();
            for (int i = 0; i < IrradiancePerformanceCount; i++)
            {
                Classes.IrradianceResult? irradianceResult = Create.IrradianceResult(vector3D_SurfaceNormal, vector3Ds_SunDirection[i % IrradiancePerformanceHourCount], 500, 800, 100, 0.2);
                Classes.SolarPowerResult? solarPowerResult = Create.SolarPowerResult(irradianceResult, 10, 6);
                sum += solarPowerResult!.Power;
            }
            stopwatch_Power.Stop();

            // Path 3 - the same work with the sun direction recomputed per evaluation.
            Stopwatch stopwatch_Pipeline = Stopwatch.StartNew();
            for (int i = 0; i < IrradiancePerformanceCount; i++)
            {
                Vector3D? vector3D = Query.SunDirection(coordinates, Core.Enums.UTC.PlusMinus0000, dateTime_Base.AddHours(i % IrradiancePerformanceHourCount), true);
                Classes.IrradianceResult? irradianceResult = Create.IrradianceResult(vector3D_SurfaceNormal, vector3D, 500, 800, 100, 0.2);
                Classes.SolarPowerResult? solarPowerResult = Create.SolarPowerResult(irradianceResult, 10, 6);
                sum += solarPowerResult!.Power;
            }
            stopwatch_Pipeline.Stop();

            Assert.True(sum > 0, "The accumulated result guards the loops against being optimized away.");

            double microsecondsIrradiance = stopwatch_Irradiance.Elapsed.TotalMilliseconds * 1000 / IrradiancePerformanceCount;
            double microsecondsPower = stopwatch_Power.Elapsed.TotalMilliseconds * 1000 / IrradiancePerformanceCount;
            double microsecondsPipeline = stopwatch_Pipeline.Elapsed.TotalMilliseconds * 1000 / IrradiancePerformanceCount;

            System.IO.File.WriteAllLines(System.IO.Path.Combine(Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly())!, "IrradianceResult_Performance.txt"),
            [
                $"Evaluations per path : {IrradiancePerformanceCount}",
                $"Create.IrradianceResult                        : {stopwatch_Irradiance.ElapsedMilliseconds} ms total, {microsecondsIrradiance:F4} us per call",
                $"Create.IrradianceResult + Create.SolarPowerResult : {stopwatch_Power.ElapsedMilliseconds} ms total, {microsecondsPower:F4} us per call",
                $"Query.SunDirection + both factories            : {stopwatch_Pipeline.ElapsedMilliseconds} ms total, {microsecondsPipeline:F4} us per call",
                $"Sun direction share of the full pipeline       : {(microsecondsPipeline - microsecondsPower) / microsecondsPipeline * 100:F1} percent"
            ]);

            // The irradiance layer is a handful of arithmetic operations and one small allocation,
            // so it must stay far below the sun position calculation it consumes.
            Assert.True(microsecondsPower < microsecondsPipeline, $"The cached-sun-direction path is expected to be the cheaper one. It was {microsecondsPower:F4} us against {microsecondsPipeline:F4} us.");
            Assert.True(microsecondsIrradiance < 1.0, $"Create.IrradianceResult took {microsecondsIrradiance:F4} us per call.");
        }
    }
}
