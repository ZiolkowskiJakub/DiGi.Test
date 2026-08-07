using DiGi.Geometry.PointCloud.Spatial.Classes;
using DiGi.Geometry.Spatial.Classes;
using System.Diagnostics;

namespace DiGi.Geometry.PointCloud.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Measures ten thousand nearest neighbour queries against an indexed one million point cloud, and reports the cost per query.
        /// <para>This is what the descent exists for, and this threshold is the only thing guarding it. A descent that failed to prune would still return the correct answer, so no correctness test would notice; only the clock would. Streaming the coordinate arrays of a million points once costs on the order of two thousand microseconds, against roughly four measured here, so the threshold sits far below anything an unpruned traversal could reach while leaving ample room for a loaded machine.</para>
        /// <para>The measurement is reported per call rather than as a total, so the number stays comparable when the iteration count is changed.</para>
        /// </summary>
        [Fact]
        public void PointCloudNearestPerformance_Query()
        {
            PointCloud3D pointCloud3D = PointCloudPerformance_Create(1000000);

            // Warm up, and build the index outside the measured region.
            Assert.True(PointCloud.Spatial.Query.TryGetNearestIndexes(pointCloud3D, 50, 50, 50, out int _, out int _, out int _));
            Assert.True(pointCloud3D.IsIndexed);

            Random random = new(999);

            int count = 10000;

            double[] x = new double[count];
            double[] y = new double[count];
            double[] z = new double[count];

            for (int i = 0; i < count; i++)
            {
                x[i] = random.NextDouble() * 100.0;
                y[i] = random.NextDouble() * 100.0;
                z[i] = random.NextDouble() * 100.0;
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            int sum = 0;
            for (int i = 0; i < count; i++)
            {
                if (PointCloud.Spatial.Query.TryGetNearestIndexes(pointCloud3D, x[i], y[i], z[i], out int index_1, out int _, out int _))
                {
                    sum += index_1;
                }
            }

            stopwatch.Stop();

            double microseconds = stopwatch.Elapsed.TotalMilliseconds * 1000.0 / count;

            Assert.NotEqual(0, sum);
            Assert.True(microseconds < 50.0, $"Nearest neighbour query over 1,000,000 points took {microseconds:F3} us per call.");

            PointCloudPerformance_Settle();
        }

        /// <summary>
        /// Measures the parallel batch against the same work done one query at a time, over an indexed one million point cloud.
        /// <para>Correctness is asserted before the clock is read, on the principle that a faster wrong answer is not an improvement. The threshold is a ratio rather than a duration so that it means the same thing on a machine with a different core count, and it is set well below the theoretical speed-up so that a busy machine does not fail the build.</para>
        /// </summary>
        [Fact]
        public void PointCloudNearestPerformance_Batch()
        {
            PointCloud3D pointCloud3D = PointCloudPerformance_Create(1000000);

            Random random = new(1234);

            int count = 100000;

            double[] x = new double[count];
            double[] y = new double[count];
            double[] z = new double[count];

            for (int i = 0; i < count; i++)
            {
                x[i] = random.NextDouble() * 100.0;
                y[i] = random.NextDouble() * 100.0;
                z[i] = random.NextDouble() * 100.0;
            }

            PointCloud3D pointCloud3D_Query = new(x, y, z);

            // Warm up, and build the index outside both measured regions.
            PointCloud.Spatial.Create.Triangle3D(pointCloud3D, 50, 50, 50);

            Assert.True(pointCloud3D.IsIndexed);

            Stopwatch stopwatch_Serial = Stopwatch.StartNew();

            Triangle3D?[] triangle3Ds_Serial = new Triangle3D?[count];
            for (int i = 0; i < count; i++)
            {
                triangle3Ds_Serial[i] = PointCloud.Spatial.Create.Triangle3D(pointCloud3D, x[i], y[i], z[i]);
            }

            stopwatch_Serial.Stop();

            Stopwatch stopwatch_Parallel = Stopwatch.StartNew();

            List<Triangle3D?>? triangle3Ds = PointCloud.Spatial.Create.Triangle3Ds(pointCloud3D, pointCloud3D_Query);

            stopwatch_Parallel.Stop();

            Assert.NotNull(triangle3Ds);
            Assert.Equal(count, triangle3Ds.Count);

            for (int i = 0; i < count; i++)
            {
                Triangle3D? triangle3D_Serial = triangle3Ds_Serial[i];
                Triangle3D? triangle3D = triangle3Ds[i];

                Assert.NotNull(triangle3D_Serial);
                Assert.NotNull(triangle3D);

                List<Point3D>? point3Ds_Serial = triangle3D_Serial.GetPoints();
                List<Point3D>? point3Ds = triangle3D.GetPoints();

                Assert.NotNull(point3Ds_Serial);
                Assert.NotNull(point3Ds);

                for (int j = 0; j < 3; j++)
                {
                    Assert.Equal(point3Ds_Serial[j].X, point3Ds[j].X);
                    Assert.Equal(point3Ds_Serial[j].Y, point3Ds[j].Y);
                    Assert.Equal(point3Ds_Serial[j].Z, point3Ds[j].Z);
                }
            }

            double microseconds_Serial = stopwatch_Serial.Elapsed.TotalMilliseconds * 1000.0 / count;
            double microseconds = stopwatch_Parallel.Elapsed.TotalMilliseconds * 1000.0 / count;

            double speedUp = stopwatch_Parallel.ElapsedTicks == 0 ? double.PositiveInfinity : stopwatch_Serial.ElapsedTicks / (double)stopwatch_Parallel.ElapsedTicks;

            Assert.True(speedUp > 2.0, $"Batch triangle construction over 100,000 queries ran {microseconds:F3} us per call against {microseconds_Serial:F3} us per call serially, a speed-up of {speedUp:F2}.");

            PointCloudPerformance_Settle();
        }
    }
}
