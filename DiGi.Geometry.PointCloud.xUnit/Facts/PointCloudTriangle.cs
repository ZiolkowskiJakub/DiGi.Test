using DiGi.Geometry.PointCloud.Planar.Classes;
using DiGi.Geometry.PointCloud.Spatial.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Geometry.PointCloud.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that the triangle built from a cloud takes its corners from the three nearest points whenever those three are usable.
        /// <para>The corners are checked against the neighbour search rather than against hard-coded coordinates, so the test states the actual contract: the triangle is the three nearest points, not merely three nearby ones.</para>
        /// </summary>
        [Fact]
        public void PointCloudTriangle3D()
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

            for (int i = 0; i < 100; i++)
            {
                Point3D point3D = new(random.NextDouble() * 100.0, random.NextDouble() * 100.0, random.NextDouble() * 100.0);

                Assert.True(Spatial.Query.TryGetNearestIndexes(pointCloud3D, point3D.X, point3D.Y, point3D.Z, out int index_1, out int index_2, out int index_3));

                Triangle3D? triangle3D = Spatial.Create.Triangle3D(pointCloud3D, point3D);

                Assert.NotNull(triangle3D);

                List<Point3D>? point3Ds = triangle3D.GetPoints();

                Assert.NotNull(point3Ds);
                Assert.Equal(3, point3Ds.Count);

                // Random points in general position are never collinear, so no widening should occur.
                Assert.Equal(x[index_1], point3Ds[0].X);
                Assert.Equal(y[index_1], point3Ds[0].Y);
                Assert.Equal(z[index_1], point3Ds[0].Z);

                Assert.Equal(x[index_2], point3Ds[1].X);
                Assert.Equal(y[index_2], point3Ds[1].Y);
                Assert.Equal(z[index_2], point3Ds[1].Z);

                Assert.Equal(x[index_3], point3Ds[2].X);
                Assert.Equal(y[index_3], point3Ds[2].Y);
                Assert.Equal(z[index_3], point3Ds[2].Z);

                Assert.True(triangle3D.GetArea() > 0);
                Assert.NotNull(triangle3D.Plane);
            }

            PointCloudPerformance_Settle();
        }

        /// <summary>
        /// Tests that a query whose three nearest points are exactly collinear still yields a usable triangle by reaching past them.
        /// <para>This is not an exotic case. Scanned and gridded data is sampled along lines, so a query landing on one of those lines has three nearest points that lie on it, and three collinear points describe no plane. The rows here are spaced further apart than the columns precisely so that the three nearest points are guaranteed collinear while the fourth is not.</para>
        /// </summary>
        [Fact]
        public void PointCloudTriangle3D_Collinear()
        {
            int count_Column = 100;
            int count_Row = 3;
            int count = count_Column * count_Row;

            double[] x = new double[count];
            double[] y = new double[count];
            double[] z = new double[count];

            // Columns one apart, rows one and a half apart. The two points either side along a row are
            // therefore nearer than anything on the neighbouring rows, and all three are collinear.
            int index = 0;
            for (int i = 0; i < count_Row; i++)
            {
                for (int j = 0; j < count_Column; j++)
                {
                    x[index] = j;
                    y[index] = i * 1.5;
                    z[index] = 0;
                    index++;
                }
            }

            PointCloud3D pointCloud3D = new(x, y, z);

            Point3D point3D = new(50, 1.5, 0);

            Assert.True(Spatial.Query.TryGetNearestIndexes(pointCloud3D, point3D.X, point3D.Y, point3D.Z, out int index_1, out int index_2, out int index_3));

            // The premise of the test: the three nearest points really are collinear.
            Assert.Equal(0.0, PointCloudTriangle_Area(x, y, z, index_1, index_2, index_3));

            Triangle3D? triangle3D = Spatial.Create.Triangle3D(pointCloud3D, point3D);

            Assert.NotNull(triangle3D);
            Assert.True(triangle3D.GetArea() > 0);

            List<Point3D>? point3Ds = triangle3D.GetPoints();

            Assert.NotNull(point3Ds);

            // The nearest point is kept as a corner; only the third was replaced.
            Assert.Equal(x[index_1], point3Ds[0].X);
            Assert.Equal(y[index_1], point3Ds[0].Y);
        }

        /// <summary>
        /// Tests that a cloud with no non-degenerate triple anywhere in reach returns nothing rather than a zero-area triangle.
        /// <para>A cloud sampled along a single line is one-dimensional at every scale, so widening the search cannot help and the honest answer is that there is no triangle. Returning a collapsed one would push the failure downstream into whatever consumes the plane.</para>
        /// </summary>
        [Fact]
        public void PointCloudTriangle3D_Degenerate()
        {
            int count = 1000;

            double[] x = new double[count];
            double[] y = new double[count];
            double[] z = new double[count];

            for (int i = 0; i < count; i++)
            {
                x[i] = i * 0.5;
            }

            PointCloud3D pointCloud3D_Collinear = new(x, y, z);

            Assert.Null(Spatial.Create.Triangle3D(pointCloud3D_Collinear, new Point3D(10, 0, 0)));

            // Every point in the same place: no pair even spans an edge.
            PointCloud3D pointCloud3D_Coincident = new(new double[count], new double[count], new double[count]);

            Assert.Null(Spatial.Create.Triangle3D(pointCloud3D_Coincident, new Point3D(0, 0, 0)));

            // Fewer than three points cannot make a triangle at all.
            PointCloud3D pointCloud3D_Pair = new([0.0, 1.0], [0.0, 1.0], [0.0, 0.0]);

            Assert.Null(Spatial.Create.Triangle3D(pointCloud3D_Pair, new Point3D(0, 0, 0)));
            Assert.Null(Spatial.Create.Triangle3D(null, new Point3D(0, 0, 0)));
            Assert.Null(Spatial.Create.Triangle3D(pointCloud3D_Pair, null));
        }

        /// <summary>
        /// Tests that the parallel batch produces exactly the same triangles, in the same order, as calling the single query form once per point.
        /// <para>The batch exists to use every core, and the only thing that makes that safe is that the queries share nothing writable. Comparing it against the serial form over enough queries to fan out across every partition is what demonstrates that, and it is asserted before any timing is measured.</para>
        /// <para>The result stays aligned with the query cloud, so an entry that could not be built is a null in place rather than a missing element that would shift every later index.</para>
        /// </summary>
        [Fact]
        public void PointCloudTriangle3D_Batch()
        {
            Random random = new(777);

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

            // Enough queries to be split across every partition rather than collapsing to one.
            int count_Query = 50000;

            double[] x_Query = new double[count_Query];
            double[] y_Query = new double[count_Query];
            double[] z_Query = new double[count_Query];

            for (int i = 0; i < count_Query; i++)
            {
                x_Query[i] = random.NextDouble() * 100.0;
                y_Query[i] = random.NextDouble() * 100.0;
                z_Query[i] = random.NextDouble() * 100.0;
            }

            PointCloud3D pointCloud3D_Query = new(x_Query, y_Query, z_Query);

            List<Triangle3D?>? triangle3Ds = Spatial.Create.Triangle3Ds(pointCloud3D, pointCloud3D_Query);

            Assert.NotNull(triangle3Ds);
            Assert.Equal(count_Query, triangle3Ds.Count);

            for (int i = 0; i < count_Query; i++)
            {
                Triangle3D? triangle3D_Expected = Spatial.Create.Triangle3D(pointCloud3D, x_Query[i], y_Query[i], z_Query[i]);
                Triangle3D? triangle3D = triangle3Ds[i];

                Assert.NotNull(triangle3D_Expected);
                Assert.NotNull(triangle3D);

                List<Point3D>? point3Ds_Expected = triangle3D_Expected.GetPoints();
                List<Point3D>? point3Ds = triangle3D.GetPoints();

                Assert.NotNull(point3Ds_Expected);
                Assert.NotNull(point3Ds);

                for (int j = 0; j < 3; j++)
                {
                    Assert.Equal(point3Ds_Expected[j].X, point3Ds[j].X);
                    Assert.Equal(point3Ds_Expected[j].Y, point3Ds[j].Y);
                    Assert.Equal(point3Ds_Expected[j].Z, point3Ds[j].Z);
                }
            }

            Assert.Null(Spatial.Create.Triangle3Ds(pointCloud3D, null));
            Assert.Null(Spatial.Create.Triangle3Ds(null, pointCloud3D_Query));

            PointCloudPerformance_Settle();
        }

        /// <summary>
        /// Tests the planar counterpart, covering the general case, the collinear case that forces the search to reach past the three nearest points, and the cloud that has no triangle to give.
        /// </summary>
        [Fact]
        public void PointCloudTriangle2D()
        {
            Random random = new(4242);

            int count = 100000;

            double[] x = new double[count];
            double[] y = new double[count];

            for (int i = 0; i < count; i++)
            {
                x[i] = random.NextDouble() * 100.0;
                y[i] = random.NextDouble() * 100.0;
            }

            PointCloud2D pointCloud2D = new(x, y);

            for (int i = 0; i < 100; i++)
            {
                Point2D point2D = new(random.NextDouble() * 100.0, random.NextDouble() * 100.0);

                Assert.True(Planar.Query.TryGetNearestIndexes(pointCloud2D, point2D.X, point2D.Y, out int index_1, out int index_2, out int index_3));

                Triangle2D? triangle2D = Planar.Create.Triangle2D(pointCloud2D, point2D);

                Assert.NotNull(triangle2D);

                List<Point2D>? point2Ds = triangle2D.GetPoints();

                Assert.NotNull(point2Ds);
                Assert.Equal(3, point2Ds.Count);

                Assert.Equal(x[index_1], point2Ds[0].X);
                Assert.Equal(y[index_1], point2Ds[0].Y);
                Assert.Equal(x[index_2], point2Ds[1].X);
                Assert.Equal(y[index_2], point2Ds[1].Y);
                Assert.Equal(x[index_3], point2Ds[2].X);
                Assert.Equal(y[index_3], point2Ds[2].Y);

                Assert.True(System.Math.Abs(triangle2D.GetArea()) > 0);
            }

            // A grid whose rows are further apart than its columns, queried on a row.
            int count_Column = 100;
            int count_Row = 3;
            int count_Grid = count_Column * count_Row;

            double[] x_Grid = new double[count_Grid];
            double[] y_Grid = new double[count_Grid];

            int index = 0;
            for (int i = 0; i < count_Row; i++)
            {
                for (int j = 0; j < count_Column; j++)
                {
                    x_Grid[index] = j;
                    y_Grid[index] = i * 1.5;
                    index++;
                }
            }

            PointCloud2D pointCloud2D_Grid = new(x_Grid, y_Grid);

            Triangle2D? triangle2D_Grid = Planar.Create.Triangle2D(pointCloud2D_Grid, new Point2D(50, 1.5));

            Assert.NotNull(triangle2D_Grid);
            Assert.True(System.Math.Abs(triangle2D_Grid.GetArea()) > 0);

            // Sampled along a single line, so no triangle exists at any scale.
            double[] x_Collinear = new double[1000];
            double[] y_Collinear = new double[1000];
            for (int i = 0; i < x_Collinear.Length; i++)
            {
                x_Collinear[i] = i * 0.5;
            }

            Assert.Null(Planar.Create.Triangle2D(new PointCloud2D(x_Collinear, y_Collinear), new Point2D(10, 0)));

            PointCloudPerformance_Settle();
        }

        /// <summary>
        /// Calculates twice the area of the triangle spanned by three points of a cloud, used to assert that a triple really is collinear.
        /// </summary>
        /// <param name="x">The X coordinates of the cloud.</param>
        /// <param name="y">The Y coordinates of the cloud.</param>
        /// <param name="z">The Z coordinates of the cloud.</param>
        /// <param name="index_1">The index of the first corner.</param>
        /// <param name="index_2">The index of the second corner.</param>
        /// <param name="index_3">The index of the third corner.</param>
        /// <returns>A <see cref="double"/> holding the length of the cross product of the two edges, which is zero exactly when the three points are collinear.</returns>
        private static double PointCloudTriangle_Area(double[] x, double[] y, double[] z, int index_1, int index_2, int index_3)
        {
            double dx_1 = x[index_2] - x[index_1];
            double dy_1 = y[index_2] - y[index_1];
            double dz_1 = z[index_2] - z[index_1];

            double dx_2 = x[index_3] - x[index_1];
            double dy_2 = y[index_3] - y[index_1];
            double dz_2 = z[index_3] - z[index_1];

            double normalX = (dy_1 * dz_2) - (dz_1 * dy_2);
            double normalY = (dz_1 * dx_2) - (dx_1 * dz_2);
            double normalZ = (dx_1 * dy_2) - (dy_1 * dx_2);

            return System.Math.Sqrt((normalX * normalX) + (normalY * normalY) + (normalZ * normalZ));
        }
    }
}
