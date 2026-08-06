using DiGi.Geometry.PointCloud.Spatial.Classes;
using DiGi.Geometry.Spatial.Classes;
using System.Diagnostics;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Measures the vectorised, partitioned bounding box calculation over a three million point cloud.
        /// <para>The pass is memory-bound rather than compute-bound, so the threshold is set from the bandwidth needed to stream the coordinate arrays once, not from arithmetic throughput.</para>
        /// </summary>
        [Fact]
        public void PointCloudPerformance_GetBoundingBox()
        {
            PointCloud3D pointCloud3D = PointCloudPerformance_Create(3000000);

            // Warm up so that the measurement excludes jitting.
            pointCloud3D.GetBoundingBox();

            Stopwatch stopwatch = Stopwatch.StartNew();

            BoundingBox3D? boundingBox3D = pointCloud3D.GetBoundingBox();

            stopwatch.Stop();

            Assert.NotNull(boundingBox3D);
            Assert.True(stopwatch.ElapsedMilliseconds < 150, $"GetBoundingBox over 3,000,000 points took {stopwatch.ElapsedMilliseconds} ms.");

            PointCloudPerformance_Settle();
        }

        /// <summary>
        /// Measures construction of the spatial index over a one million point cloud.
        /// <para>The build is a linear counting sort rather than a comparison sort, so the cost should track the point count rather than growing with its logarithm.</para>
        /// </summary>
        [Fact]
        public void PointCloudPerformance_IndexBuild()
        {
            PointCloud3D pointCloud3D_WarmUp = PointCloudPerformance_Create(200000);
            BoundingBox3D boundingBox3D_WarmUp = new(new Point3D(0, 0, 0), new Point3D(1, 1, 1));
            PointCloud.Spatial.Query.InRange(pointCloud3D_WarmUp, boundingBox3D_WarmUp, 0);

            PointCloud3D pointCloud3D = PointCloudPerformance_Create(1000000);

            BoundingBox3D boundingBox3D = new(new Point3D(0, 0, 0), new Point3D(1, 1, 1));

            Stopwatch stopwatch = Stopwatch.StartNew();

            PointCloud.Spatial.Query.InRange(pointCloud3D, boundingBox3D, 0);

            stopwatch.Stop();

            Assert.True(pointCloud3D.IsIndexed);
            Assert.True(stopwatch.ElapsedMilliseconds < 1500, $"Index build over 1,000,000 points took {stopwatch.ElapsedMilliseconds} ms.");

            PointCloudPerformance_Settle();
        }

        /// <summary>
        /// Measures a thousand small bounding box queries against an indexed one million point cloud.
        /// <para>This is what the index exists for. Each box covers a thousandth of the extent, so an exhaustive scan would touch a billion coordinates in total while the index should touch only the few nodes that overlap each box.</para>
        /// </summary>
        [Fact]
        public void PointCloudPerformance_InRange()
        {
            PointCloud3D pointCloud3D = PointCloudPerformance_Create(1000000);

            BoundingBox3D boundingBox3D_WarmUp = new(new Point3D(0, 0, 0), new Point3D(10, 10, 10));

            // Warm up, and force the index to be built outside the measured region.
            PointCloud.Spatial.Query.InRange(pointCloud3D, boundingBox3D_WarmUp, 0);

            Assert.True(pointCloud3D.IsIndexed);

            Random random = new(999);

            BoundingBox3D[] boundingBox3Ds = new BoundingBox3D[1000];
            for (int i = 0; i < boundingBox3Ds.Length; i++)
            {
                double x = random.NextDouble() * 90.0;
                double y = random.NextDouble() * 90.0;
                double z = random.NextDouble() * 90.0;

                boundingBox3Ds[i] = new BoundingBox3D(new Point3D(x, y, z), new Point3D(x + 10.0, y + 10.0, z + 10.0));
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < boundingBox3Ds.Length; i++)
            {
                PointCloud.Spatial.Query.InRange(pointCloud3D, boundingBox3Ds[i], 0);
            }

            stopwatch.Stop();

            Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"1,000 indexed queries over 1,000,000 points took {stopwatch.ElapsedMilliseconds} ms.");

            PointCloudPerformance_Settle();
        }

        /// <summary>
        /// Verifies that walking a million point cloud allocates essentially nothing.
        /// <para>This is the property the whole storage design exists to provide. Materializing each point as an object would allocate roughly eighty megabytes here; the enumerator is a struct over the coordinate arrays and should allocate nothing measurable at all.</para>
        /// </summary>
        [Fact]
        public void PointCloudPerformance_Enumeration()
        {
            PointCloud3D pointCloud3D = PointCloudPerformance_Create(1000000);

            double sum_WarmUp = 0;
            foreach (PointCloud3D.Point point in pointCloud3D)
            {
                sum_WarmUp += point.X;
            }

            Assert.NotEqual(0.0, sum_WarmUp);

            long allocated_Before = GC.GetAllocatedBytesForCurrentThread();

            double sum = 0;
            foreach (PointCloud3D.Point point in pointCloud3D)
            {
                sum += point.X + point.Y + point.Z;
            }

            long allocated = GC.GetAllocatedBytesForCurrentThread() - allocated_Before;

            Assert.NotEqual(0.0, sum);
            Assert.True(allocated < 1024, $"Enumerating 1,000,000 points allocated {allocated} bytes.");

            // The span-based view must be free of allocation too.
            long allocated_View_Before = GC.GetAllocatedBytesForCurrentThread();

            double sum_View = 0;
            PointCloud3DView pointCloud3DView = pointCloud3D.AsView();
            for (int i = 0; i < pointCloud3DView.Count; i++)
            {
                sum_View += pointCloud3DView.X[i];
            }

            long allocated_View = GC.GetAllocatedBytesForCurrentThread() - allocated_View_Before;

            Assert.NotEqual(0.0, sum_View);
            Assert.True(allocated_View < 1024, $"Reading 1,000,000 points through a view allocated {allocated_View} bytes.");

            PointCloudPerformance_Settle();
        }

        /// <summary>
        /// Releases the large arrays these measurements allocate before the next test runs.
        /// <para>Each cloud here occupies tens of megabytes on the large object heap. Left uncollected they make a
        /// collection pause far more likely inside a later timing assertion, which turns an unrelated benchmark
        /// elsewhere in this class into an intermittent failure. Collecting explicitly is not something production
        /// code should do, but leaving a measurement to perturb its neighbours is worse.</para>
        /// </summary>
        private static void PointCloudPerformance_Settle()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary>
        /// Builds a deterministic cloud of the requested size for the performance measurements.
        /// </summary>
        /// <param name="count">The number of points to generate.</param>
        /// <returns>A new <see cref="PointCloud3D"/>.</returns>
        private static PointCloud3D PointCloudPerformance_Create(int count)
        {
            Random random = new(12345);

            double[] x = new double[count];
            double[] y = new double[count];
            double[] z = new double[count];

            for (int i = 0; i < count; i++)
            {
                x[i] = random.NextDouble() * 100.0;
                y[i] = random.NextDouble() * 100.0;
                z[i] = random.NextDouble() * 100.0;
            }

            // The adopting constructor is internal to the library, so the public copying one is used here.
            return new PointCloud3D(x, y, z);
        }
    }
}
