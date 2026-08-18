using DiGi.Geometry.PointCloud.Planar.Classes;
using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.PointCloud.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests construction of a <see cref="PointCloud2D"/> from a sequence of points, the fidelity of the materialized points, the calculated bounding box, and the JSON round trip.
        /// </summary>
        [Fact]
        public void PointCloud2D()
        {
            List<Point2D?> point2Ds =
            [
                new Point2D(1, 2),
                new Point2D(-3, 4),
                new Point2D(5, -6),
                null
            ];

            PointCloud2D pointCloud2D = new(point2Ds);

            Assert.Equal(3, pointCloud2D.Count);
            Assert.Equal(2, pointCloud2D.Dimension);

            List<Point2D>? point2Ds_Actual = pointCloud2D.GetPoints();

            Assert.NotNull(point2Ds_Actual);
            Assert.Equal(3, point2Ds_Actual.Count);
            Assert.Equal(1.0, point2Ds_Actual[0].X);
            Assert.Equal(-6.0, point2Ds_Actual[2].Y);

            BoundingBox2D? boundingBox2D = pointCloud2D.GetBoundingBox();

            Assert.NotNull(boundingBox2D);
            Assert.Equal(-3.0, boundingBox2D.Min.X);
            Assert.Equal(-6.0, boundingBox2D.Min.Y);
            Assert.Equal(5.0, boundingBox2D.Max.X);
            Assert.Equal(4.0, boundingBox2D.Max.Y);

            DiGi.Core.xUnit.Query.SerializationCheck(pointCloud2D);

            PointCloud2D pointCloud2D_Empty = new((IEnumerable<Point2D?>?)null);

            Assert.Equal(0, pointCloud2D_Empty.Count);
            Assert.Null(pointCloud2D_Empty.GetBoundingBox());

            DiGi.Core.xUnit.Query.SerializationCheck(pointCloud2D_Empty);
        }

        /// <summary>
        /// Tests that translating and transforming a planar cloud produce the same coordinates as applying the same operation to each point individually.
        /// </summary>
        [Fact]
        public void PointCloud2D_MoveTransform()
        {
            int count = 37;

            double[] x = new double[count];
            double[] y = new double[count];

            List<Point2D> point2Ds_Expected = new(count);

            for (int i = 0; i < count; i++)
            {
                x[i] = i;
                y[i] = i * 2;

                point2Ds_Expected.Add(new Point2D(x[i], y[i]));
            }

            PointCloud2D pointCloud2D = new(x, y);

            Assert.True(pointCloud2D.Move(new Vector2D(10, -20)));

            for (int i = 0; i < count; i++)
            {
                Assert.True(pointCloud2D.TryGetPoint(i, out double x_Actual, out double y_Actual));

                Assert.Equal(i + 10.0, x_Actual);
                Assert.Equal((i * 2.0) - 20.0, y_Actual);
            }

            PointCloud2D pointCloud2D_Transform = new(x, y);

            TransformGroup2D transformGroup2D = new(
            [
                Geometry.Planar.Create.Transform2D.Rotation(0.6),
                Geometry.Planar.Create.Transform2D.Translation(new Vector2D(3, -4))
            ]);

            Assert.True(pointCloud2D_Transform.Transform(transformGroup2D));

            for (int i = 0; i < count; i++)
            {
                point2Ds_Expected[i].Transform(transformGroup2D);

                Assert.True(pointCloud2D_Transform.TryGetPoint(i, out double x_Actual, out double y_Actual));

                Assert.Equal(point2Ds_Expected[i].X, x_Actual, 10);
                Assert.Equal(point2Ds_Expected[i].Y, y_Actual, 10);
            }
        }

        /// <summary>
        /// Tests that the planar bounding box filter returns exactly the points an exhaustive scan would.
        /// </summary>
        [Fact]
        public void PointCloud2D_InRange()
        {
            Random random = new(12345);

            int count = 100000;

            double[] x = new double[count];
            double[] y = new double[count];

            for (int i = 0; i < count; i++)
            {
                x[i] = random.NextDouble() * 100.0;
                y[i] = random.NextDouble() * 100.0;
            }

            PointCloud2D pointCloud2D = new(x, y);

            BoundingBox2D boundingBox2D = new(new Point2D(20, 30), new Point2D(60, 70));

            int count_Expected = 0;
            for (int i = 0; i < count; i++)
            {
                if (x[i] >= 20 && x[i] <= 60 && y[i] >= 30 && y[i] <= 70)
                {
                    count_Expected++;
                }
            }

            Assert.Equal(count_Expected, Planar.Query.InRangeCount(pointCloud2D, boundingBox2D, 0));

            PointCloud2D? pointCloud2D_InRange = Planar.Query.InRange(pointCloud2D, boundingBox2D, 0);

            Assert.NotNull(pointCloud2D_InRange);
            Assert.Equal(count_Expected, pointCloud2D_InRange.Count);

            List<int>? indexes = Planar.Query.InRangeIndexes(pointCloud2D, boundingBox2D, 0);

            Assert.NotNull(indexes);
            Assert.Equal(count_Expected, indexes.Count);
        }
    }
}
