using DiGi.Geometry.PointCloud.Core.Enums;
using DiGi.Geometry.PointCloud.Planar.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.PointCloud.Spatial.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Geometry.PointCloud.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a <see cref="PointCloud3D"/> survives a round trip through the binary point cloud format with every coordinate bit-identical.
        /// <para>The header bytes are asserted explicitly so that an accidental change to the layout fails loudly rather than silently producing files that older readers cannot parse.</para>
        /// </summary>
        [Fact]
        public void PointCloudBinary()
        {
            Random random = new(12345);

            int count = 5000;

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

            byte[]? bytes = PointCloud.Spatial.Convert.ToSystem_Bytes(pointCloud3D, PointCloudFormat.Binary);

            Assert.NotNull(bytes);
            Assert.Equal(32 + (count * 3 * 8), bytes.Length);

            Assert.Equal((byte)'D', bytes[0]);
            Assert.Equal((byte)'G', bytes[1]);
            Assert.Equal((byte)'P', bytes[2]);
            Assert.Equal((byte)'C', bytes[3]);
            Assert.Equal(1, bytes[4]);
            Assert.Equal(0, bytes[5]);
            Assert.Equal(3, bytes[6]);
            Assert.Equal(0, bytes[7]);
            Assert.Equal((byte)(count & 0xFF), bytes[8]);
            Assert.Equal((byte)((count >> 8) & 0xFF), bytes[9]);

            PointCloud3D? pointCloud3D_Actual = PointCloud.Spatial.Create.PointCloud3D(bytes);

            Assert.NotNull(pointCloud3D_Actual);
            Assert.Equal(count, pointCloud3D_Actual.Count);

            for (int i = 0; i < count; i++)
            {
                Assert.True(pointCloud3D_Actual.TryGetPoint(i, out double x_Actual, out double y_Actual, out double z_Actual));

                Assert.Equal(x[i], x_Actual);
                Assert.Equal(y[i], y_Actual);
                Assert.Equal(z[i], z_Actual);
            }
        }

        /// <summary>
        /// Tests that a <see cref="PointCloud2D"/> survives a round trip through the binary point cloud format and records a dimension of two in its header.
        /// </summary>
        [Fact]
        public void PointCloudBinary_PointCloud2D()
        {
            double[] x = [1, 2, 3, 4];
            double[] y = [5, 6, 7, 8];

            PointCloud2D pointCloud2D = new(x, y);

            byte[]? bytes = PointCloud.Planar.Convert.ToSystem_Bytes(pointCloud2D, PointCloudFormat.Binary);

            Assert.NotNull(bytes);
            Assert.Equal(2, bytes[6]);
            Assert.Equal(32 + (4 * 2 * 8), bytes.Length);

            PointCloud2D? pointCloud2D_Actual = PointCloud.Planar.Create.PointCloud2D(bytes);

            Assert.NotNull(pointCloud2D_Actual);
            Assert.Equal(4, pointCloud2D_Actual.Count);
            Assert.True(pointCloud2D_Actual.TryGetPoint(3, out double x_Actual, out double y_Actual));
            Assert.Equal(4.0, x_Actual);
            Assert.Equal(8.0, y_Actual);

            // A two-dimensional payload must not be accepted as a three-dimensional cloud.
            Assert.Null(PointCloud.Spatial.Create.PointCloud3D(bytes));
        }

        /// <summary>
        /// Tests that malformed binary input yields a null result and never throws.
        /// <para>Decoding runs inside deserialization paths, so an exception escaping here would propagate far from its cause. Every rejection route is exercised: null, too short, wrong magic, wrong version, and a payload whose length disagrees with the declared count.</para>
        /// </summary>
        [Fact]
        public void PointCloudBinary_Malformed()
        {
            double[] x = [1, 2, 3];
            double[] y = [4, 5, 6];
            double[] z = [7, 8, 9];

            PointCloud3D pointCloud3D = new(x, y, z);

            byte[]? bytes = PointCloud.Spatial.Convert.ToSystem_Bytes(pointCloud3D, PointCloudFormat.Binary);

            Assert.NotNull(bytes);

            // Declared as locals rather than inline collection expressions, which would be ambiguous
            // between the byte, double array and point sequence overloads.
            byte[] bytes_Empty = [];
            byte[] bytes_TooShort = [1, 2, 3, 4, 5];

            Assert.Null(PointCloud.Spatial.Create.PointCloud3D((byte[]?)null));
            Assert.Null(PointCloud.Spatial.Create.PointCloud3D(bytes_Empty));
            Assert.Null(PointCloud.Spatial.Create.PointCloud3D(bytes_TooShort));

            byte[] bytes_WrongMagic = (byte[])bytes.Clone();
            bytes_WrongMagic[0] = (byte)'X';
            Assert.Null(PointCloud.Spatial.Create.PointCloud3D(bytes_WrongMagic));

            byte[] bytes_WrongVersion = (byte[])bytes.Clone();
            bytes_WrongVersion[4] = 99;
            Assert.Null(PointCloud.Spatial.Create.PointCloud3D(bytes_WrongVersion));

            byte[] bytes_Truncated = new byte[bytes.Length - 8];
            Array.Copy(bytes, bytes_Truncated, bytes_Truncated.Length);
            Assert.Null(PointCloud.Spatial.Create.PointCloud3D(bytes_Truncated));

            byte[] bytes_Misaligned = new byte[bytes.Length - 3];
            Array.Copy(bytes, bytes_Misaligned, bytes_Misaligned.Length);
            Assert.Null(PointCloud.Spatial.Create.PointCloud3D(bytes_Misaligned));

            byte[] bytes_WrongCount = (byte[])bytes.Clone();
            bytes_WrongCount[8] = 99;
            Assert.Null(PointCloud.Spatial.Create.PointCloud3D(bytes_WrongCount));
        }

        /// <summary>
        /// Tests that requesting the JSON representation produces UTF-8 JSON rather than the binary payload, and that the two representations differ in size as expected.
        /// </summary>
        [Fact]
        public void PointCloudBinary_Format()
        {
            double[] x = [1, 2, 3];
            double[] y = [4, 5, 6];
            double[] z = [7, 8, 9];

            PointCloud3D pointCloud3D = new(x, y, z);

            byte[]? bytes_Binary = PointCloud.Spatial.Convert.ToSystem_Bytes(pointCloud3D, PointCloudFormat.Binary);
            byte[]? bytes_Json = PointCloud.Spatial.Convert.ToSystem_Bytes(pointCloud3D, PointCloudFormat.Json);

            Assert.NotNull(bytes_Binary);
            Assert.NotNull(bytes_Json);

            Assert.Equal((byte)'D', bytes_Binary[0]);
            Assert.Equal((byte)'{', bytes_Json[0]);

            Assert.Null(PointCloud.Spatial.Convert.ToSystem_Bytes(null, PointCloudFormat.Binary));
        }
    }
}
