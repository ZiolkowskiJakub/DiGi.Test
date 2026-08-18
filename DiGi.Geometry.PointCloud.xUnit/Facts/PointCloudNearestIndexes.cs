using DiGi.Geometry.PointCloud.Spatial.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Geometry.PointCloud.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that the indexed nearest neighbour descent returns exactly the same three points as an exhaustive scan, over many query positions and a deliberately non-uniform cloud.
        /// <para>This is the test that actually proves the descent. A search that prunes a node holding a genuinely nearer point returns a plausible answer that no smoke test would catch, and the three points it returns would still form a triangle.</para>
        /// <para>Three answers are compared, not two. The reference sort establishes the truth, the exhaustive vectorised kernel is checked against it, and the indexed descent is checked against both, so a fault in either search path is attributed rather than merely detected.</para>
        /// <para>The density is deliberately uneven, with a tight cluster inside a sparse halo. Uniform data hides exactly the failure modes a spatial hierarchy is prone to, because every cell then holds a similar number of points.</para>
        /// </summary>
        [Fact]
        public void PointCloudNearestIndexes()
        {
            Random random = new(12345);

            // Above the index threshold, so the indexed path is the one under test.
            int count_Cluster = 150000;
            int count_Halo = 50000;
            int count = count_Cluster + count_Halo;

            double[] x = new double[count];
            double[] y = new double[count];
            double[] z = new double[count];

            for (int i = 0; i < count_Cluster; i++)
            {
                x[i] = 40.0 + (random.NextDouble() * 2.0);
                y[i] = 40.0 + (random.NextDouble() * 2.0);
                z[i] = 40.0 + (random.NextDouble() * 2.0);
            }

            for (int i = count_Cluster; i < count; i++)
            {
                x[i] = random.NextDouble() * 100.0;
                y[i] = random.NextDouble() * 100.0;
                z[i] = random.NextDouble() * 100.0;
            }

            PointCloud3D pointCloud3D = new(x, y, z);

            Assert.False(pointCloud3D.IsIndexed);

            double[][]? coordinates = pointCloud3D.GetCoordinates(false);

            Assert.NotNull(coordinates);

            // Hoisted out of the loop: a stack allocation per iteration would grow the frame two hundred
            // times over, and the search rewrites both buffers before it reads them.
            Span<int> indexes_Exhaustive = stackalloc int[3];
            Span<double> distancesSquared_Exhaustive = stackalloc double[3];
            Span<double> query = stackalloc double[3];

            int[] indexes_Reference = new int[count];
            double[] distancesSquared_Reference = new double[count];

            for (int i = 0; i < 200; i++)
            {
                // A mixture of positions inside the cluster, inside the halo and outside the cloud entirely.
                Point3D point3D = i % 3 == 0
                    ? new Point3D(40.0 + (random.NextDouble() * 2.0), 40.0 + (random.NextDouble() * 2.0), 40.0 + (random.NextDouble() * 2.0))
                    : i % 3 == 1
                        ? new Point3D(random.NextDouble() * 100.0, random.NextDouble() * 100.0, random.NextDouble() * 100.0)
                        : new Point3D(-50.0 + (random.NextDouble() * 10.0), 150.0 + (random.NextDouble() * 10.0), random.NextDouble() * 200.0);

                List<int> indexes_Expected = PointCloudNearestIndexes_Reference(x, y, z, point3D, indexes_Reference, distancesSquared_Reference, 3);

                query[0] = point3D.X;
                query[1] = point3D.Y;
                query[2] = point3D.Z;

                Assert.Equal(3, Core.Query.NearestIndexes(coordinates, query, indexes_Exhaustive, distancesSquared_Exhaustive));

                for (int j = 0; j < 3; j++)
                {
                    Assert.Equal(indexes_Expected[j], indexes_Exhaustive[j]);
                }

                Assert.True(Spatial.Query.TryGetNearestIndexes(pointCloud3D, point3D.X, point3D.Y, point3D.Z, out int index_1, out int index_2, out int index_3));

                Assert.Equal(indexes_Expected[0], index_1);
                Assert.Equal(indexes_Expected[1], index_2);
                Assert.Equal(indexes_Expected[2], index_3);

                // The distances must come back ascending, because the triangle factory relies on the
                // first entry being the nearest point rather than merely one of the three nearest.
                Assert.True(distancesSquared_Exhaustive[0] <= distancesSquared_Exhaustive[1]);
                Assert.True(distancesSquared_Exhaustive[1] <= distancesSquared_Exhaustive[2]);
            }

            Assert.True(pointCloud3D.IsIndexed);

            PointCloudPerformance_Settle();
        }

        /// <summary>
        /// Tests that a cloud below the index threshold, which is answered by the exhaustive vectorised scan, agrees with the reference ordering.
        /// <para>The vectorised sweep skips a whole block of points with a single comparison and only unpacks a block that contains a candidate. A fault in that block test would drop points silently, and only shows up when the answer is compared against an ordering computed without it.</para>
        /// <para>The size is chosen to be several vector widths wide with an awkward remainder, so that both the vectorised body and the scalar tail carry real work.</para>
        /// </summary>
        [Fact]
        public void PointCloudNearestIndexes_BruteForce()
        {
            Random random = new(54321);

            int count = 1013;

            double[] x = new double[count];
            double[] y = new double[count];
            double[] z = new double[count];

            for (int i = 0; i < count; i++)
            {
                x[i] = random.NextDouble() * 100.0;
                y[i] = random.NextDouble() * 100.0;
                z[i] = random.NextDouble() * 100.0;
            }

            PointCloud3D pointCloud3D = new(x, y, z);

            int[] indexes_Reference = new int[count];
            double[] distancesSquared_Reference = new double[count];

            for (int i = 0; i < 200; i++)
            {
                Point3D point3D = new(random.NextDouble() * 100.0, random.NextDouble() * 100.0, random.NextDouble() * 100.0);

                List<int> indexes_Expected = PointCloudNearestIndexes_Reference(x, y, z, point3D, indexes_Reference, distancesSquared_Reference, 3);

                Assert.True(Spatial.Query.TryGetNearestIndexes(pointCloud3D, point3D.X, point3D.Y, point3D.Z, out int index_1, out int index_2, out int index_3));

                Assert.Equal(indexes_Expected[0], index_1);
                Assert.Equal(indexes_Expected[1], index_2);
                Assert.Equal(indexes_Expected[2], index_3);
            }

            // No index is built below the threshold, which is what makes this the exhaustive path.
            Assert.False(pointCloud3D.IsIndexed);
        }

        /// <summary>
        /// Tests that points at exactly equal distances resolve to the same answer on both search paths.
        /// <para>Without an explicit rule the answer would depend on the order the points happen to be visited, which is Z-order in the descent and input order in the exhaustive scan. A cloud with duplicated points would then return different neighbours depending only on whether it was large enough to be indexed, which is the kind of size-dependent behaviour that makes a result impossible to reason about.</para>
        /// <para>The cloud is built above the index threshold so that both paths can be exercised over the same data: the descent through the cloud, the exhaustive kernel directly over its coordinate arrays.</para>
        /// </summary>
        [Fact]
        public void PointCloudNearestIndexes_Ties()
        {
            Random random = new(2024);

            int count = 70000;

            double[] x = new double[count];
            double[] y = new double[count];
            double[] z = new double[count];

            for (int i = 0; i < count; i++)
            {
                x[i] = 500.0 + (random.NextDouble() * 100.0);
                y[i] = 500.0 + (random.NextDouble() * 100.0);
                z[i] = 500.0 + (random.NextDouble() * 100.0);
            }

            // Eight exact duplicates around the origin, all equidistant from a query at the origin and
            // all far closer to it than anything in the bulk of the cloud.
            int[] indexes_Duplicate = [17, 4096, 12000, 30001, 45000, 51234, 60000, 69999];
            for (int i = 0; i < indexes_Duplicate.Length; i++)
            {
                x[indexes_Duplicate[i]] = 1.0;
                y[indexes_Duplicate[i]] = 0.0;
                z[indexes_Duplicate[i]] = 0.0;
            }

            PointCloud3D pointCloud3D = new(x, y, z);

            double[][]? coordinates = pointCloud3D.GetCoordinates(false);

            Assert.NotNull(coordinates);

            Span<int> indexes = stackalloc int[3];
            Span<double> distancesSquared = stackalloc double[3];
            Span<double> query = stackalloc double[3];

            Assert.Equal(3, Core.Query.NearestIndexes(coordinates, query, indexes, distancesSquared));

            // The three lowest indexes among the tied duplicates, and nothing else.
            Assert.Equal(17, indexes[0]);
            Assert.Equal(4096, indexes[1]);
            Assert.Equal(12000, indexes[2]);

            Assert.True(Spatial.Query.TryGetNearestIndexes(pointCloud3D, 0, 0, 0, out int index_1, out int index_2, out int index_3));

            Assert.True(pointCloud3D.IsIndexed);

            Assert.Equal(17, index_1);
            Assert.Equal(4096, index_2);
            Assert.Equal(12000, index_3);

            PointCloudPerformance_Settle();
        }

        /// <summary>
        /// Tests that degenerate and undersized clouds are handled rather than crashing or returning a partly filled answer as if it were complete.
        /// </summary>
        [Fact]
        public void PointCloudNearestIndexes_Degenerate()
        {
            PointCloud3D pointCloud3D_Empty = new((IEnumerable<Point3D?>?)null);
            Assert.False(Spatial.Query.TryGetNearestIndexes(pointCloud3D_Empty, 0, 0, 0, out int _, out int _, out int _));

            // Two points cannot answer a request for three, and must say so rather than reporting two.
            PointCloud3D pointCloud3D_Pair = new([0.0, 1.0], [0.0, 0.0], [0.0, 0.0]);
            Assert.False(Spatial.Query.TryGetNearestIndexes(pointCloud3D_Pair, 0, 0, 0, out int _, out int _, out int _));

            PointCloud3D pointCloud3D_Triple = new([0.0, 1.0, 2.0], [0.0, 0.0, 0.0], [0.0, 0.0, 0.0]);
            Assert.True(Spatial.Query.TryGetNearestIndexes(pointCloud3D_Triple, 0, 0, 0, out int index_1, out int index_2, out int index_3));
            Assert.Equal(0, index_1);
            Assert.Equal(1, index_2);
            Assert.Equal(2, index_3);

            // Coincident points are a legitimate answer, distinguished only by index.
            int count = 70000;
            PointCloud3D pointCloud3D_Coincident = new(new double[count], new double[count], new double[count]);
            Assert.True(Spatial.Query.TryGetNearestIndexes(pointCloud3D_Coincident, 0, 0, 0, out int index_Coincident_1, out int index_Coincident_2, out int index_Coincident_3));
            Assert.Equal(0, index_Coincident_1);
            Assert.Equal(1, index_Coincident_2);
            Assert.Equal(2, index_Coincident_3);

            PointCloudPerformance_Settle();
        }

        /// <summary>
        /// Verifies that a nearest neighbour query against an indexed cloud allocates nothing at all.
        /// <para>This is the property the whole search design exists to provide. A caller sweeping a million positions must not hand the collector a million objects to trace, so the candidate set, the traversal stack and the query itself are all stack-allocated and the search runs on raw coordinates without materializing a point.</para>
        /// </summary>
        [Fact]
        public void PointCloudNearestIndexes_Allocation()
        {
            PointCloud3D pointCloud3D = PointCloudPerformance_Create(200000);

            // Warm up, and build the index outside the measured region: the build itself allocates.
            Assert.True(Spatial.Query.TryGetNearestIndexes(pointCloud3D, 50, 50, 50, out int _, out int _, out int _));
            Assert.True(pointCloud3D.IsIndexed);

            long allocated_Before = GC.GetAllocatedBytesForCurrentThread();

            int sum = 0;
            for (int i = 0; i < 1000; i++)
            {
                double value = i * 0.1;

                if (Spatial.Query.TryGetNearestIndexes(pointCloud3D, value, value, value, out int index_1, out int _, out int _))
                {
                    sum += index_1;
                }
            }

            long allocated = GC.GetAllocatedBytesForCurrentThread() - allocated_Before;

            Assert.NotEqual(0, sum);
            Assert.True(allocated < 1024, $"1,000 nearest neighbour queries allocated {allocated} bytes.");

            PointCloudPerformance_Settle();
        }

        /// <summary>
        /// Produces the nearest point indexes for a query position by ordering every point of the cloud, for use as the reference answer in the agreement tests.
        /// <para>Deliberately the slowest possible implementation. It sorts the whole cloud by squared distance and then by index, so it shares no code and no shortcut with either search under test.</para>
        /// <para>The working buffers come from the caller. Allocating them here would push a pair of large object heap arrays through the collector on every one of the hundreds of queries these tests make, and that churn is enough to provoke a collection pause inside an unrelated timing assertion elsewhere in this class.</para>
        /// </summary>
        /// <param name="x">The X coordinates of the cloud.</param>
        /// <param name="y">The Y coordinates of the cloud.</param>
        /// <param name="z">The Z coordinates of the cloud.</param>
        /// <param name="point3D">The query point.</param>
        /// <param name="indexes">A scratch buffer the length of the cloud, supplied by the caller and reused across queries.</param>
        /// <param name="distancesSquared">A scratch buffer the length of the cloud, supplied by the caller and reused across queries.</param>
        /// <param name="count">The number of neighbours to return.</param>
        /// <returns>A <see cref="List{T}"/> of point indexes ordered nearest first.</returns>
        private static List<int> PointCloudNearestIndexes_Reference(
            double[] x,
            double[] y,
            double[] z,
            Point3D point3D,
            int[] indexes,
            double[] distancesSquared,
            int count)
        {
            for (int i = 0; i < x.Length; i++)
            {
                double dx = x[i] - point3D.X;
                double dy = y[i] - point3D.Y;
                double dz = z[i] - point3D.Z;

                indexes[i] = i;
                distancesSquared[i] = (dx * dx) + (dy * dy) + (dz * dz);
            }

            Array.Sort(indexes, (index_1, index_2) =>
            {
                int result_Temp = distancesSquared[index_1].CompareTo(distancesSquared[index_2]);

                return result_Temp != 0 ? result_Temp : index_1.CompareTo(index_2);
            });

            List<int> result = [];
            for (int i = 0; i < count && i < indexes.Length; i++)
            {
                result.Add(indexes[i]);
            }

            return result;
        }
    }
}
