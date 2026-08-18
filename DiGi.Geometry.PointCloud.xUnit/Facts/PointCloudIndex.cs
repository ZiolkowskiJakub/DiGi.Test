using DiGi.Geometry.PointCloud.Spatial.Classes;
using DiGi.Geometry.Spatial.Classes;
using System.Threading.Tasks;

namespace DiGi.Geometry.PointCloud.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that the indexed bounding box filter returns exactly the same points as an exhaustive scan, over many random boxes and a deliberately non-uniform cloud.
        /// <para>This is the test that actually proves the spatial index. The index is only useful if it is invisible: a hierarchy that prunes a node holding a qualifying point, or that accepts a node whole when part of it lies outside the box, would produce a plausible but wrong answer that no smoke test would catch.</para>
        /// <para>The density is deliberately uneven, with a tight cluster inside a sparse halo. Uniform data hides exactly the failure modes a spatial hierarchy is prone to, because every cell then holds a similar number of points.</para>
        /// </summary>
        [Fact]
        public void PointCloudIndex()
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

            for (int i = 0; i < 100; i++)
            {
                double x_Min = random.NextDouble() * 100.0;
                double y_Min = random.NextDouble() * 100.0;
                double z_Min = random.NextDouble() * 100.0;

                double size = 0.5 + (random.NextDouble() * 60.0);

                BoundingBox3D boundingBox3D = new(new Point3D(x_Min, y_Min, z_Min), new Point3D(x_Min + size, y_Min + size, z_Min + size));

                List<int> indexes_Expected = [];
                for (int j = 0; j < count; j++)
                {
                    if (x[j] >= boundingBox3D.MinX && x[j] <= boundingBox3D.MaxX && y[j] >= boundingBox3D.MinY && y[j] <= boundingBox3D.MaxY && z[j] >= boundingBox3D.MinZ && z[j] <= boundingBox3D.MaxZ)
                    {
                        indexes_Expected.Add(j);
                    }
                }

                List<int>? indexes_Actual = Spatial.Query.InRangeIndexes(pointCloud3D, boundingBox3D, 0);

                Assert.NotNull(indexes_Actual);
                Assert.Equal(indexes_Expected, indexes_Actual);

                Assert.Equal(indexes_Expected.Count, Spatial.Query.InRangeCount(pointCloud3D, boundingBox3D, 0));

                PointCloud3D? pointCloud3D_InRange = Spatial.Query.InRange(pointCloud3D, boundingBox3D, 0);

                if (indexes_Expected.Count == 0)
                {
                    Assert.Null(pointCloud3D_InRange);
                }
                else
                {
                    Assert.NotNull(pointCloud3D_InRange);
                    Assert.Equal(indexes_Expected.Count, pointCloud3D_InRange.Count);

                    for (int j = 0; j < indexes_Expected.Count; j++)
                    {
                        Assert.True(pointCloud3D_InRange.TryGetPoint(j, out double x_Actual, out double y_Actual, out double z_Actual));

                        Assert.Equal(x[indexes_Expected[j]], x_Actual);
                        Assert.Equal(y[indexes_Expected[j]], y_Actual);
                        Assert.Equal(z[indexes_Expected[j]], z_Actual);
                    }
                }
            }

            Assert.True(pointCloud3D.IsIndexed);
        }

        /// <summary>
        /// Tests that the raw comparison used by the cloud filter agrees exactly with the per-point bounding box test, including at the tolerance boundary.
        /// <para>The filter folds the tolerance into the box bounds once and then compares raw coordinates. That shortcut is only valid if it reproduces the per-point test exactly, so the equivalence is asserted rather than assumed.</para>
        /// </summary>
        [Fact]
        public void PointCloudIndex_Tolerance()
        {
            BoundingBox3D boundingBox3D = new(new Point3D(0, 0, 0), new Point3D(10, 10, 10));

            double tolerance = 1e-3;

            // Just inside and just outside the widened bound on a single axis.
            double[] x = [10.0 + tolerance - 1e-9, 10.0 + tolerance + 1e-9];
            double[] y = [5, 5];
            double[] z = [5, 5];

            PointCloud3D pointCloud3D = new(x, y, z);

            List<int>? indexes = Spatial.Query.InRangeIndexes(pointCloud3D, boundingBox3D, tolerance);

            Assert.NotNull(indexes);
            Assert.Single(indexes);
            Assert.Equal(0, indexes[0]);

            Assert.True(boundingBox3D.InRange(new Point3D(x[0], y[0], z[0]), tolerance));
            Assert.False(boundingBox3D.InRange(new Point3D(x[1], y[1], z[1]), tolerance));
        }

        /// <summary>
        /// Tests that moving a cloud discards its cached index, so that later queries are answered against the new positions rather than the old ones.
        /// <para>An index describes where the points were. Leaving it in place after a mutation is the easiest way to ship a filter that silently returns stale results.</para>
        /// </summary>
        [Fact]
        public void PointCloudIndex_Invalidation()
        {
            Random random = new(12345);

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

            PointCloud3D pointCloud3D = new(x, y, z);

            BoundingBox3D boundingBox3D = new(new Point3D(0, 0, 0), new Point3D(10, 10, 10));

            int count_Before = Spatial.Query.InRange(pointCloud3D, boundingBox3D, 0)?.Count ?? 0;

            Assert.True(count_Before > 0);
            Assert.True(pointCloud3D.IsIndexed);

            // Shift the whole cloud far outside the query box.
            Assert.True(pointCloud3D.Move(new DiGi.Geometry.Spatial.Classes.Vector3D(1000, 1000, 1000)));

            Assert.False(pointCloud3D.IsIndexed);

            Assert.Null(Spatial.Query.InRange(pointCloud3D, boundingBox3D, 0));

            BoundingBox3D boundingBox3D_Moved = new(new Point3D(1000, 1000, 1000), new Point3D(1010, 1010, 1010));

            Assert.Equal(count_Before, Spatial.Query.InRange(pointCloud3D, boundingBox3D_Moved, 0)?.Count ?? 0);
        }

        /// <summary>
        /// Tests that many threads querying a freshly constructed cloud all receive the same correct answer.
        /// <para>The index is built lazily on first use, so the first queries race to construct it. Losing that race must cost at most one discarded build, never a partially initialised index or a wrong result.</para>
        /// </summary>
        [Fact]
        public void PointCloudIndex_Concurrency()
        {
            Random random = new(12345);

            int count = 200000;

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

            BoundingBox3D boundingBox3D = new(new Point3D(10, 10, 10), new Point3D(40, 40, 40));

            int count_Expected = 0;
            for (int i = 0; i < count; i++)
            {
                if (x[i] >= 10 && x[i] <= 40 && y[i] >= 10 && y[i] <= 40 && z[i] >= 10 && z[i] <= 40)
                {
                    count_Expected++;
                }
            }

            int[] counts = new int[32];

            Parallel.For(0, 32, i =>
            {
                counts[i] = Spatial.Query.InRange(pointCloud3D, boundingBox3D, 0)?.Count ?? -1;
            });

            for (int i = 0; i < counts.Length; i++)
            {
                Assert.Equal(count_Expected, counts[i]);
            }
        }

        /// <summary>
        /// Tests that degenerate clouds are filtered correctly rather than crashing or silently returning everything.
        /// <para>Covers an empty cloud, a single point, many coincident points, a collinear cloud with two degenerate axes, and extents at both ends of the practical range, which is where a quantised cell grid is most likely to divide by zero or overflow.</para>
        /// </summary>
        [Fact]
        public void PointCloudIndex_Degenerate()
        {
            BoundingBox3D boundingBox3D = new(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));

            PointCloud3D pointCloud3D_Empty = new((IEnumerable<Point3D?>?)null);
            Assert.Null(Spatial.Query.InRange(pointCloud3D_Empty, boundingBox3D, 0));
            Assert.Equal(-1, Spatial.Query.InRangeCount(pointCloud3D_Empty, boundingBox3D, 0));

            PointCloud3D pointCloud3D_Single = new([0.0], [0.0], [0.0]);
            Assert.Equal(1, Spatial.Query.InRange(pointCloud3D_Single, boundingBox3D, 0)?.Count ?? 0);

            int count = 70000;

            double[] x_Coincident = new double[count];
            double[] y_Coincident = new double[count];
            double[] z_Coincident = new double[count];
            Assert.Equal(count, Spatial.Query.InRange(new PointCloud3D(x_Coincident, y_Coincident, z_Coincident), boundingBox3D, 0)?.Count ?? 0);

            double[] x_Collinear = new double[count];
            double[] y_Collinear = new double[count];
            double[] z_Collinear = new double[count];
            for (int i = 0; i < count; i++)
            {
                x_Collinear[i] = (i / (double)count) * 2.0;
            }

            PointCloud3D pointCloud3D_Collinear = new(x_Collinear, y_Collinear, z_Collinear);

            int count_Expected = 0;
            for (int i = 0; i < count; i++)
            {
                if (x_Collinear[i] <= 1.0)
                {
                    count_Expected++;
                }
            }

            Assert.Equal(count_Expected, Spatial.Query.InRange(pointCloud3D_Collinear, boundingBox3D, 0)?.Count ?? 0);

            double[] x_Tiny = new double[count];
            double[] y_Tiny = new double[count];
            double[] z_Tiny = new double[count];
            for (int i = 0; i < count; i++)
            {
                x_Tiny[i] = i * 1e-9 / count;
            }

            Assert.Equal(count, Spatial.Query.InRange(new PointCloud3D(x_Tiny, y_Tiny, z_Tiny), boundingBox3D, 0)?.Count ?? 0);

            double[] x_Huge = new double[count];
            double[] y_Huge = new double[count];
            double[] z_Huge = new double[count];
            for (int i = 0; i < count; i++)
            {
                x_Huge[i] = (i / (double)count) * 1e9;
            }

            BoundingBox3D boundingBox3D_Huge = new(new Point3D(-1, -1, -1), new Point3D(5e8, 1, 1));

            int count_Expected_Huge = 0;
            for (int i = 0; i < count; i++)
            {
                if (x_Huge[i] <= 5e8)
                {
                    count_Expected_Huge++;
                }
            }

            Assert.Equal(count_Expected_Huge, Spatial.Query.InRange(new PointCloud3D(x_Huge, y_Huge, z_Huge), boundingBox3D_Huge, 0)?.Count ?? 0);
        }
    }
}
