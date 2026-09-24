using ComputeSharp;
using DiGi.ComputeSharp.Core.Constants;
using DiGi.ComputeSharp.Planar.Classes;
using DiGi.ComputeSharp.Spatial.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the public overloads which take no <see cref="GraphicsDevice"/> leave the shared ComputeSharp default device alive (ZiolkowskiJakub/DiGi.ComputeSharp#2).
        /// <para>A device obtained from <see cref="GraphicsDevice.GetDefault"/> before the calls must still allocate buffers afterwards, and <see cref="GraphicsDevice.GetDefault"/> must still return the same instance.
        /// Disposing the default device resets ComputeSharp's cache, so a disposing overload fails this with <see cref="ObjectDisposedException"/>.</para>
        /// </summary>
        [Fact]
        public void GraphicsDevice_KeepsDefaultDevice()
        {
            if (!Query.IsComputeSharpSupported(testOutputHelper))
            {
                return;
            }

            double tolerance = Tolerance.Distance;

            GraphicsDevice graphicsDevice = GraphicsDevice.GetDefault();

            Line2 line2 = new(-1, 0, 1, 0);
            List<Line2> line2s = [new Line2(0, -1, 0, 1)];

            List<Triangle2> triangle2s = [new Triangle2(new Core.Classes.Bool(true), new Coordinate2(-5, -5), new Coordinate2(5, -5), new Coordinate2(0, 5))];

            Line3 line3 = new(-1, 0, 0, 1, 0, 0);
            List<Line3> line3s = [new Line3(0, -1, 0, 0, 1, 0)];

            Triangle3 triangle3 = new(new Core.Classes.Bool(true), -5, -5, 0, 5, -5, 0, 0, 5, 0);
            List<Triangle3> triangle3s = [new Triangle3(new Core.Classes.Bool(true), 0, 0, -1, 0, 0, 1, 2, 2, 0)];

            try
            {
                Assert.NotNull(Planar.Create.Line2Intersections(line2, line2s, tolerance));
                Assert.NotNull(Planar.Create.Line2Intersections(line2s, triangle2s, tolerance));
                Assert.NotNull(Spatial.Create.Line3Intersections(line3, line3s, tolerance));
                Assert.NotNull(Spatial.Create.Triangle3Intersections(triangle3, triangle3s, tolerance));
                Assert.NotNull(Spatial.Create.Triangle3Intersections([triangle3], triangle3s, tolerance));

                using ReadWriteBuffer<int> readWriteBuffer = graphicsDevice.AllocateReadWriteBuffer<int>(1);
                Assert.Equal(1, readWriteBuffer.Length);

                Assert.Same(graphicsDevice, GraphicsDevice.GetDefault());
            }
            catch (Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
            {
                testOutputHelper.WriteLine("WARNING: GPU FP64 double-precision operations are not supported on this graphics card: " + exception.Message);
            }
        }
    }
}
