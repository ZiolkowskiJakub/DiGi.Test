using DiGi.Geometry.PointCloud.Spatial.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests construction of a <see cref="PointCloud3D"/> from a sequence of points, the fidelity of the materialized points, the calculated bounding box, cloning, and the JSON round trip.
        /// <para>The serialization check is deliberately run on a small cloud: the JSON representation carries the coordinate payload as a single Base64 string, and the helper performs several serializations and clones, so a large cloud would multiply the payload many times over in memory.</para>
        /// </summary>
        [Fact]
        public void PointCloud3D()
        {
            List<Point3D?> point3Ds =
            [
                new Point3D(1, 2, 3),
                new Point3D(-4, 5, -6),
                new Point3D(7, -8, 9),
                null
            ];

            PointCloud3D pointCloud3D = new(point3Ds);

            Assert.Equal(3, pointCloud3D.Count);
            Assert.Equal(3, pointCloud3D.Dimension);

            List<Point3D>? point3Ds_Actual = pointCloud3D.GetPoints();

            Assert.NotNull(point3Ds_Actual);
            Assert.Equal(3, point3Ds_Actual.Count);
            Assert.Equal(1.0, point3Ds_Actual[0].X);
            Assert.Equal(-8.0, point3Ds_Actual[2].Y);
            Assert.Equal(9.0, point3Ds_Actual[2].Z);

            BoundingBox3D? boundingBox3D = pointCloud3D.GetBoundingBox();

            Assert.NotNull(boundingBox3D);
            Assert.Equal(-4.0, boundingBox3D.MinX);
            Assert.Equal(-8.0, boundingBox3D.MinY);
            Assert.Equal(-6.0, boundingBox3D.MinZ);
            Assert.Equal(7.0, boundingBox3D.MaxX);
            Assert.Equal(5.0, boundingBox3D.MaxY);
            Assert.Equal(9.0, boundingBox3D.MaxZ);

            DiGi.Core.xUnit.Query.SerializationCheck(pointCloud3D);
        }

        /// <summary>
        /// Tests that an empty <see cref="PointCloud3D"/> reports a count of zero rather than a negative sentinel, yields no bounding box, and still survives a JSON round trip.
        /// <para>Zero matters: a negative count would make a counted loop a silent no-op and would make an allocation sized from the count throw.</para>
        /// </summary>
        [Fact]
        public void PointCloud3D_Empty()
        {
            PointCloud3D pointCloud3D = new((IEnumerable<Point3D?>?)null);

            Assert.Equal(0, pointCloud3D.Count);
            Assert.Null(pointCloud3D.GetBoundingBox());
            Assert.Null(pointCloud3D.GetX());
            Assert.False(pointCloud3D.TryGetPoint(0, out _, out _, out _));

            DiGi.Core.xUnit.Query.SerializationCheck(pointCloud3D);
        }

        /// <summary>
        /// Tests that a cloud built from a large deterministic sample survives a JSON round trip with its coordinates intact.
        /// </summary>
        [Fact]
        public void PointCloud3D_SerializationCheck()
        {
            Random random = new(12345);

            int count = 1000;

            double[] x = new double[count];
            double[] y = new double[count];
            double[] z = new double[count];

            for (int i = 0; i < count; i++)
            {
                x[i] = (random.NextDouble() * 2000.0) - 1000.0;
                y[i] = (random.NextDouble() * 2000.0) - 1000.0;
                z[i] = (random.NextDouble() * 2000.0) - 1000.0;
            }

            PointCloud3D pointCloud3D = new(x, y, z);

            Assert.Equal(count, pointCloud3D.Count);

            DiGi.Core.xUnit.Query.SerializationCheck(pointCloud3D);

            PointCloud3D pointCloud3D_Clone = new(pointCloud3D);

            double[]? x_Clone = pointCloud3D_Clone.GetX();

            Assert.NotNull(x_Clone);
            for (int i = 0; i < count; i++)
            {
                Assert.Equal(x[i], x_Clone[i]);
            }
        }

        /// <summary>
        /// Tests that translating a cloud shifts every coordinate by the supplied vector.
        /// <para>The sample is deliberately larger than two vector widths so that both the vectorised body and the scalar tail of the offset loop are exercised.</para>
        /// </summary>
        [Fact]
        public void PointCloud3D_Move()
        {
            int count = 37;

            double[] x = new double[count];
            double[] y = new double[count];
            double[] z = new double[count];

            for (int i = 0; i < count; i++)
            {
                x[i] = i;
                y[i] = i * 2;
                z[i] = i * 3;
            }

            PointCloud3D pointCloud3D = new(x, y, z);

            // Fully qualified: this test project declares its own Vector3D struct, which shadows the geometry one.
            Assert.True(pointCloud3D.Move(new Spatial.Classes.Vector3D(10, -20, 30)));

            for (int i = 0; i < count; i++)
            {
                Assert.True(pointCloud3D.TryGetPoint(i, out double x_Actual, out double y_Actual, out double z_Actual));

                Assert.Equal(i + 10.0, x_Actual);
                Assert.Equal((i * 2.0) - 20.0, y_Actual);
                Assert.Equal((i * 3.0) + 30.0, z_Actual);
            }

            Assert.False(pointCloud3D.Move(null));
        }

        /// <summary>
        /// Tests that transforming a cloud produces the same coordinates as transforming each point individually.
        /// <para>The cloud path flattens the transform into an affine matrix once and streams it, while the per-point path walks the transform object for every coordinate; the two must agree exactly.</para>
        /// </summary>
        [Fact]
        public void PointCloud3D_Transform()
        {
            Random random = new(12345);

            int count = 37;

            double[] x = new double[count];
            double[] y = new double[count];
            double[] z = new double[count];

            List<Point3D> point3Ds_Expected = new(count);

            for (int i = 0; i < count; i++)
            {
                x[i] = (random.NextDouble() * 20.0) - 10.0;
                y[i] = (random.NextDouble() * 20.0) - 10.0;
                z[i] = (random.NextDouble() * 20.0) - 10.0;

                point3Ds_Expected.Add(new Point3D(x[i], y[i], z[i]));
            }

            PointCloud3D pointCloud3D = new(x, y, z);

            // A group is used deliberately: the cloud path composes the members into a single affine matrix,
            // while the per-point path replays the whole group for every point. The results must still agree.
            TransformGroup3D transformGroup3D = new(
            [
                Spatial.Create.Transform3D.RotationZ(0.7),
                Spatial.Create.Transform3D.Scale(2.0, 3.0, 0.5),
                Spatial.Create.Transform3D.Translation(3.0, -4.0, 5.0)
            ]);

            Assert.True(pointCloud3D.Transform(transformGroup3D));

            for (int i = 0; i < count; i++)
            {
                point3Ds_Expected[i].Transform(transformGroup3D);

                Assert.True(pointCloud3D.TryGetPoint(i, out double x_Actual, out double y_Actual, out double z_Actual));

                Assert.Equal(point3Ds_Expected[i].X, x_Actual, 10);
                Assert.Equal(point3Ds_Expected[i].Y, y_Actual, 10);
                Assert.Equal(point3Ds_Expected[i].Z, z_Actual, 10);
            }

            Assert.False(pointCloud3D.Transform(null));
        }

        /// <summary>
        /// Tests that the factory removes points carrying a non-finite coordinate.
        /// <para>This is not cosmetic. The vectorised minimum and maximum reduction returns its second operand when either operand is not a number, whereas the scalar equivalent propagates it, so leaving such a value in the data would make the two paths disagree depending on lane alignment.</para>
        /// </summary>
        [Fact]
        public void PointCloud3D_Create_NonFinite()
        {
            double[] x = [1, double.NaN, 3, 4, 5];
            double[] y = [1, 2, double.PositiveInfinity, 4, 5];
            double[] z = [1, 2, 3, double.NegativeInfinity, 5];

            PointCloud3D? pointCloud3D = PointCloud.Spatial.Create.PointCloud3D(x, y, z);

            Assert.NotNull(pointCloud3D);
            Assert.Equal(2, pointCloud3D.Count);

            BoundingBox3D? boundingBox3D = pointCloud3D.GetBoundingBox();

            Assert.NotNull(boundingBox3D);
            Assert.Equal(1.0, boundingBox3D.MinX);
            Assert.Equal(5.0, boundingBox3D.MaxX);

            double[] x_AllBad = [double.NaN, double.NaN];
            double[] y_AllBad = [1, 2];
            double[] z_AllBad = [1, 2];

            Assert.Null(PointCloud.Spatial.Create.PointCloud3D(x_AllBad, y_AllBad, z_AllBad));

            Assert.Null(PointCloud.Spatial.Create.PointCloud3D(null, y, z));
            Assert.Null(PointCloud.Spatial.Create.PointCloud3D([1, 2], [1, 2, 3], [1, 2, 3]));
        }

        /// <summary>
        /// Tests that the calculated bounding box matches a straightforward scalar reference over the same data.
        /// <para>The production path is vectorised with a scalar tail, so the sample length is chosen to leave a partial final vector.</para>
        /// </summary>
        [Fact]
        public void PointCloud3D_GetBoundingBox()
        {
            Random random = new(12345);

            int count = 4099;

            double[] x = new double[count];
            double[] y = new double[count];
            double[] z = new double[count];

            double x_Min = double.MaxValue;
            double x_Max = double.MinValue;

            for (int i = 0; i < count; i++)
            {
                x[i] = (random.NextDouble() * 2000.0) - 1000.0;
                y[i] = (random.NextDouble() * 2000.0) - 1000.0;
                z[i] = (random.NextDouble() * 2000.0) - 1000.0;

                if (x[i] < x_Min)
                {
                    x_Min = x[i];
                }

                if (x[i] > x_Max)
                {
                    x_Max = x[i];
                }
            }

            PointCloud3D pointCloud3D = new(x, y, z);

            BoundingBox3D? boundingBox3D = pointCloud3D.GetBoundingBox();

            Assert.NotNull(boundingBox3D);
            Assert.Equal(x_Min, boundingBox3D.MinX);
            Assert.Equal(x_Max, boundingBox3D.MaxX);
        }
    }
}
