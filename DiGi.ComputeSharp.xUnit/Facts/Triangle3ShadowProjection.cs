using ComputeSharp;
using DiGi.ComputeSharp.Core.Constants;
using DiGi.ComputeSharp.Planar.Classes;
using DiGi.ComputeSharp.Spatial.Classes;
using System.Runtime.CompilerServices;
using Bool = DiGi.ComputeSharp.Core.Classes.Bool;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Contains unit tests for <see cref="Triangle3ShadowProjectionComputeShader"/> and <see cref="ShadowPolygon2"/>.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the size of <see cref="ShadowPolygon2"/> is pinned: four <see cref="Coordinate2"/> (64 B) and three <see cref="int"/> (12 B), padded by the 8-byte alignment to 80 B.
        /// </summary>
        [Fact]
        public void ShadowPolygon2_Size()
        {
            Assert.Equal(80, Unsafe.SizeOf<ShadowPolygon2>());
            Assert.Equal(128, Unsafe.SizeOf<ShadowReceiver>());
        }

        /// <summary>
        /// Verifies that the shadow projection shader reproduces a CPU implementation of the CPU <c>ShadingSolver</c>'s per-caster-triangle steps (self skip, grazing, upstream test, Sutherland-Hodgman clip, projection, bounding-box reject, area), pair for pair and point for point, for two sun directions and two tolerances.
        /// <para>The scene is non-vacuous: it produces hits and misses, at least one clipped 4-point polygon, at least one self skip and at least one grazing receiver.</para>
        /// Handles UnsupportedDoubleOperationException gracefully for FP64 unsupported GPUs.
        /// </summary>
        [Fact]
        public void Triangle3ShadowProjection_MatchesCpuReference()
        {
            if (!Query.IsComputeSharpSupported(testOutputHelper))
            {
                return;
            }

            (ShadowReceiver[] shadowReceivers, Triangle3[] triangle3s, int[] elementIndexes) = CreateShadowProjectionScene();

            int count_Hit = 0, count_Miss = 0, count_FourPoint = 0, count_SelfSkip = 0, count_Grazing = 0;
            List<double> deviations = [];

            try
            {
                GraphicsDevice graphicsDevice = GraphicsDevice.GetDefault();

                foreach (Coordinate3 vector in ShadowProjectionVectors())
                {
                    foreach (double tolerance in (double[])[Tolerance.Distance, Tolerance.MacroDistance])
                    {
                        List<ShadowPolygon2> shadowPolygon2s_Expected = ShadowProjectionReference(shadowReceivers, triangle3s, elementIndexes, vector, tolerance, out int count_SelfSkip_Run, out int count_Grazing_Run);
                        List<ShadowPolygon2> shadowPolygon2s_Actual = DispatchShadowProjection(graphicsDevice, shadowReceivers, triangle3s, elementIndexes, vector, tolerance, 0, shadowReceivers.Length, shadowReceivers.Length * triangle3s.Length, out int counter);

                        Assert.Equal(shadowPolygon2s_Expected.Count, counter);
                        Assert.Equal(shadowPolygon2s_Expected.Count, shadowPolygon2s_Actual.Count);

                        for (int i = 0; i < shadowPolygon2s_Expected.Count; i++)
                        {
                            ShadowPolygon2 shadowPolygon2_Expected = shadowPolygon2s_Expected[i];
                            ShadowPolygon2 shadowPolygon2_Actual = shadowPolygon2s_Actual[i];

                            Assert.Equal(shadowPolygon2_Expected.ReceiverIndex, shadowPolygon2_Actual.ReceiverIndex);
                            Assert.Equal(shadowPolygon2_Expected.TriangleIndex, shadowPolygon2_Actual.TriangleIndex);
                            Assert.Equal(shadowPolygon2_Expected.Count, shadowPolygon2_Actual.Count);

                            Coordinate2[] coordinate2s_Expected = ShadowPolygonPoints(shadowPolygon2_Expected);
                            Coordinate2[] coordinate2s_Actual = ShadowPolygonPoints(shadowPolygon2_Actual);
                            for (int k = 0; k < 4; k++)
                            {
                                if (k >= shadowPolygon2_Expected.Count)
                                {
                                    Assert.True(double.IsNaN(coordinate2s_Actual[k].X) && double.IsNaN(coordinate2s_Actual[k].Y), $"Unused point {k} of pair ({shadowPolygon2_Actual.ReceiverIndex}, {shadowPolygon2_Actual.TriangleIndex}) is not NaN.");
                                    continue;
                                }

                                deviations.Add(Math.Max(Math.Abs(coordinate2s_Expected[k].X - coordinate2s_Actual[k].X), Math.Abs(coordinate2s_Expected[k].Y - coordinate2s_Actual[k].Y)));
                            }

                            if (shadowPolygon2_Expected.Count == 4)
                            {
                                count_FourPoint++;
                            }
                        }

                        count_Hit += shadowPolygon2s_Expected.Count;
                        count_Miss += (shadowReceivers.Length * triangle3s.Length) - shadowPolygon2s_Expected.Count;
                        count_SelfSkip += count_SelfSkip_Run;
                        count_Grazing += count_Grazing_Run;
                    }
                }
            }
            catch (Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
            {
                testOutputHelper.WriteLine($"Warning: {exception.Message}");
                return;
            }

            deviations.Sort();
            double deviation_Max = deviations.Count == 0 ? 0 : deviations[^1];
            double deviation_Median = deviations.Count == 0 ? 0 : deviations[deviations.Count / 2];
            double deviation_P99 = deviations.Count == 0 ? 0 : deviations[Math.Min(deviations.Count - 1, (int)(deviations.Count * 0.99))];

            testOutputHelper.WriteLine($"Hits {count_Hit}, misses {count_Miss}, 4-point {count_FourPoint}, self skips {count_SelfSkip}, grazing {count_Grazing}");
            testOutputHelper.WriteLine($"Point deviation over {deviations.Count} points: median {deviation_Median:E3}, p99 {deviation_P99:E3}, max {deviation_Max:E3}");

            Assert.True(count_Hit > 0 && count_Miss > 0, "Expected a mix of hits and misses.");
            Assert.True(count_FourPoint > 0, "Expected at least one clipped 4-point polygon.");
            Assert.True(count_SelfSkip > 0, "Expected at least one self skip.");
            Assert.True(count_Grazing > 0, "Expected at least one grazing receiver.");

            Assert.True(deviation_Max <= 1e-9, $"Point deviation {deviation_Max:E3} exceeds 1e-9.");
        }

        /// <summary>
        /// Verifies that an output buffer smaller than the hit count is safe and detectable: the counter ends at the true hit count, and every record written below the capacity is a distinct, valid hit of the full result.
        /// Handles UnsupportedDoubleOperationException gracefully for FP64 unsupported GPUs.
        /// </summary>
        [Fact]
        public void Triangle3ShadowProjection_Overflow()
        {
            if (!Query.IsComputeSharpSupported(testOutputHelper))
            {
                return;
            }

            (ShadowReceiver[] shadowReceivers, Triangle3[] triangle3s, int[] elementIndexes) = CreateShadowProjectionScene();
            Coordinate3 vector = ShadowProjectionVectors()[0];
            double tolerance = Tolerance.Distance;

            try
            {
                GraphicsDevice graphicsDevice = GraphicsDevice.GetDefault();

                List<ShadowPolygon2> shadowPolygon2s_Full = DispatchShadowProjection(graphicsDevice, shadowReceivers, triangle3s, elementIndexes, vector, tolerance, 0, shadowReceivers.Length, shadowReceivers.Length * triangle3s.Length, out int counter_Full);
                Assert.True(counter_Full > 3, "Expected enough hits to overflow a smaller buffer.");

                int capacity = counter_Full / 2;
                List<ShadowPolygon2> shadowPolygon2s_Partial = DispatchShadowProjection(graphicsDevice, shadowReceivers, triangle3s, elementIndexes, vector, tolerance, 0, shadowReceivers.Length, capacity, out int counter_Partial);

                Assert.Equal(counter_Full, counter_Partial);
                Assert.Equal(capacity, shadowPolygon2s_Partial.Count);

                HashSet<(int, int)> pairs_Full = [.. shadowPolygon2s_Full.Select(x => (x.ReceiverIndex, x.TriangleIndex))];
                HashSet<(int, int)> pairs_Partial = [];
                foreach (ShadowPolygon2 shadowPolygon2 in shadowPolygon2s_Partial)
                {
                    Assert.True(shadowPolygon2.Count == 3 || shadowPolygon2.Count == 4, "A record below the capacity is not a written hit.");
                    Assert.Contains((shadowPolygon2.ReceiverIndex, shadowPolygon2.TriangleIndex), pairs_Full);
                    Assert.True(pairs_Partial.Add((shadowPolygon2.ReceiverIndex, shadowPolygon2.TriangleIndex)), "A hit was written twice.");
                }
            }
            catch (Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
            {
                testOutputHelper.WriteLine($"Warning: {exception.Message}");
            }
        }

        /// <summary>
        /// Verifies that row-offset dispatches (block sizes 1 and 2, the latter with a partial last block) concatenate to exactly the sorted record set of one full dispatch, and that a negative row offset is refused.
        /// Handles UnsupportedDoubleOperationException gracefully for FP64 unsupported GPUs.
        /// </summary>
        [Fact]
        public void Triangle3ShadowProjection_RowOffset()
        {
            if (!Query.IsComputeSharpSupported(testOutputHelper))
            {
                return;
            }

            (ShadowReceiver[] shadowReceivers, Triangle3[] triangle3s, int[] elementIndexes) = CreateShadowProjectionScene();
            Coordinate3 vector = ShadowProjectionVectors()[0];
            double tolerance = Tolerance.Distance;
            int capacity = shadowReceivers.Length * triangle3s.Length;

            try
            {
                GraphicsDevice graphicsDevice = GraphicsDevice.GetDefault();

                List<ShadowPolygon2> shadowPolygon2s_Full = DispatchShadowProjection(graphicsDevice, shadowReceivers, triangle3s, elementIndexes, vector, tolerance, 0, shadowReceivers.Length, capacity, out _);
                Assert.NotEmpty(shadowPolygon2s_Full);

                foreach (int blockSize in (int[])[1, 2])
                {
                    List<ShadowPolygon2> shadowPolygon2s_Blocks = [];
                    for (int rowOffset = 0; rowOffset < shadowReceivers.Length; rowOffset += blockSize)
                    {
                        shadowPolygon2s_Blocks.AddRange(DispatchShadowProjection(graphicsDevice, shadowReceivers, triangle3s, elementIndexes, vector, tolerance, rowOffset, blockSize, capacity, out _));
                    }

                    shadowPolygon2s_Blocks = SortShadowPolygons(shadowPolygon2s_Blocks);

                    Assert.Equal(shadowPolygon2s_Full.Count, shadowPolygon2s_Blocks.Count);
                    for (int i = 0; i < shadowPolygon2s_Full.Count; i++)
                    {
                        Assert.True(AreBitwiseEqual(shadowPolygon2s_Full[i], shadowPolygon2s_Blocks[i]), $"Record {i} (block size {blockSize}) differs from the full dispatch.");
                    }
                }

                using ReadOnlyBuffer<ShadowReceiver> receiversBuffer = graphicsDevice.AllocateReadOnlyBuffer(shadowReceivers);
                using ReadOnlyBuffer<Triangle3> trianglesBuffer = graphicsDevice.AllocateReadOnlyBuffer(triangle3s);
                using ReadOnlyBuffer<int> elementIndexesBuffer = graphicsDevice.AllocateReadOnlyBuffer(elementIndexes);
                using ReadWriteBuffer<ShadowPolygon2> polygonsBuffer = graphicsDevice.AllocateReadWriteBuffer<ShadowPolygon2>(1);
                using ReadWriteBuffer<int> counterBuffer = graphicsDevice.AllocateReadWriteBuffer<int>(1);
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    _ = new Triangle3ShadowProjectionComputeShader(receiversBuffer, trianglesBuffer, elementIndexesBuffer, polygonsBuffer, counterBuffer, vector, -1, tolerance);
                });
            }
            catch (Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
            {
                testOutputHelper.WriteLine($"Warning: {exception.Message}");
            }
        }

        /// <summary>
        /// Creates the shadow projection test scene: a ground square, a raised square that is both caster and receiver, and a vertical wall (receivers 0 to 2), plus shading-only triangles, one of which pierces the ground plane (clipped to 4 points) and one of which lies far outside every receiver.
        /// </summary>
        private static (ShadowReceiver[] ShadowReceivers, Triangle3[] Triangle3s, int[] ElementIndexes) CreateShadowProjectionScene()
        {
            ShadowReceiver[] shadowReceivers =
            [
                new(new Coordinate3(0, 0, 0), new Coordinate3(0, 0, 1), new Coordinate3(1, 0, 0), new Coordinate3(0, 1, 0), new Coordinate2(0, 0), new Coordinate2(10, 10)),
                new(new Coordinate3(0, 0, 3), new Coordinate3(0, 0, 1), new Coordinate3(1, 0, 0), new Coordinate3(0, 1, 0), new Coordinate2(2, 2), new Coordinate2(4, 4)),
                new(new Coordinate3(12, 0, 0), new Coordinate3(-1, 0, 0), new Coordinate3(0, 1, 0), new Coordinate3(0, 0, 1), new Coordinate2(0, 0), new Coordinate2(10, 5)),
            ];

            List<Triangle3> triangle3s = [];
            List<int> elementIndexes = [];

            void Add(int elementIndex, double x_1, double y_1, double z_1, double x_2, double y_2, double z_2, double x_3, double y_3, double z_3)
            {
                triangle3s.Add(new Triangle3(new Bool(true), x_1, y_1, z_1, x_2, y_2, z_2, x_3, y_3, z_3));
                elementIndexes.Add(elementIndex);
            }

            // Receiver triangles.
            Add(0, 0, 0, 0, 10, 0, 0, 10, 10, 0);
            Add(0, 0, 0, 0, 10, 10, 0, 0, 10, 0);
            Add(1, 2, 2, 3, 4, 2, 3, 4, 4, 3);
            Add(1, 2, 2, 3, 4, 4, 3, 2, 4, 3);
            Add(2, 12, 0, 0, 12, 10, 0, 12, 10, 5);
            Add(2, 12, 0, 0, 12, 10, 5, 12, 0, 5);

            // Shading-only canopy.
            Add(-1, 5, 5, 5, 7, 5, 5.5, 6, 7, 6);
            Add(-1, 1, 6, 4, 3, 6, 4, 2, 8, 4.5);
            Add(-1, 8, 1, 2, 10, 2, 3, 9, 4, 2.5);
            Add(-1, 6, 1, -1, 7, 2, 2, 8, 1, 2);
            Add(-1, 100, 100, 5, 101, 100, 5, 100, 101, 5);

            Random random = new(7);
            for (int i = 0; i < 6; i++)
            {
                double x = random.NextDouble() * 9, y = random.NextDouble() * 9, z = 1 + (random.NextDouble() * 5);
                Add(-1, x, y, z, x + 1 + random.NextDouble(), y + random.NextDouble(), z + random.NextDouble(), x + random.NextDouble(), y + 1 + random.NextDouble(), z - random.NextDouble());
            }

            return (shadowReceivers, [.. triangle3s], [.. elementIndexes]);
        }

        /// <summary>
        /// Returns the two sun propagation directions of the shadow projection facts: an oblique one that reaches the wall, and one parallel to the wall's plane (grazing).
        /// </summary>
        private static Coordinate3[] ShadowProjectionVectors()
        {
            double length_1 = Math.Sqrt((0.3 * 0.3) + (0.2 * 0.2) + 1.0);
            double length_2 = Math.Sqrt((0.4 * 0.4) + 1.0);

            return [new Coordinate3(0.3 / length_1, -0.2 / length_1, -1.0 / length_1), new Coordinate3(0, 0.4 / length_2, -1.0 / length_2)];
        }

        /// <summary>
        /// Dispatches the shadow projection shader over one receiver block and returns the written records sorted by receiver and triangle index.
        /// </summary>
        private static List<ShadowPolygon2> DispatchShadowProjection(GraphicsDevice graphicsDevice, ShadowReceiver[] shadowReceivers, Triangle3[] triangle3s, int[] elementIndexes, Coordinate3 vector, double tolerance, int rowOffset, int rowCount, int capacity, out int counter)
        {
            using ReadOnlyBuffer<ShadowReceiver> receiversBuffer = graphicsDevice.AllocateReadOnlyBuffer(shadowReceivers);
            using ReadOnlyBuffer<Triangle3> trianglesBuffer = graphicsDevice.AllocateReadOnlyBuffer(triangle3s);
            using ReadOnlyBuffer<int> elementIndexesBuffer = graphicsDevice.AllocateReadOnlyBuffer(elementIndexes);
            using ReadWriteBuffer<ShadowPolygon2> polygonsBuffer = graphicsDevice.AllocateReadWriteBuffer<ShadowPolygon2>(Math.Max(1, capacity));
            using ReadWriteBuffer<int> counterBuffer = graphicsDevice.AllocateReadWriteBuffer<int>([0]);

            graphicsDevice.For(rowCount, triangle3s.Length, new Triangle3ShadowProjectionComputeShader(receiversBuffer, trianglesBuffer, elementIndexesBuffer, polygonsBuffer, counterBuffer, vector, rowOffset, tolerance));

            int[] counters = new int[1];
            counterBuffer.CopyTo(counters);
            counter = counters[0];

            ShadowPolygon2[] shadowPolygon2s = new ShadowPolygon2[polygonsBuffer.Length];
            polygonsBuffer.CopyTo(shadowPolygon2s);

            return SortShadowPolygons(shadowPolygon2s.Take(Math.Min(counter, capacity)));
        }

        /// <summary>
        /// CPU reference of the shadow projection: a port of the CPU <c>ShadingSolver</c>'s per-caster-triangle loop, returning the hits sorted by receiver and triangle index, and counting self skips and grazing receivers.
        /// </summary>
        private static List<ShadowPolygon2> ShadowProjectionReference(ShadowReceiver[] shadowReceivers, Triangle3[] triangle3s, int[] elementIndexes, Coordinate3 vector, double tolerance, out int count_SelfSkip, out int count_Grazing)
        {
            count_SelfSkip = 0;
            count_Grazing = 0;

            List<ShadowPolygon2> result = [];
            for (int i = 0; i < shadowReceivers.Length; i++)
            {
                ShadowReceiver shadowReceiver = shadowReceivers[i];
                Coordinate3 normal = shadowReceiver.Normal;
                Coordinate3 origin = shadowReceiver.Origin;

                double dotProduct = (normal.X * vector.X) + (normal.Y * vector.Y) + (normal.Z * vector.Z);
                if (Math.Abs(dotProduct) <= tolerance)
                {
                    count_Grazing++;
                    continue;
                }

                double sign = dotProduct > 0 ? 1 : -1;

                for (int j = 0; j < triangle3s.Length; j++)
                {
                    if (elementIndexes[j] == i)
                    {
                        count_SelfSkip++;
                        continue;
                    }

                    Triangle3 triangle3 = triangle3s[j];
                    Coordinate3[] coordinate3s = [triangle3.Point_1, triangle3.Point_2, triangle3.Point_3];

                    double[] xs = new double[3], ys = new double[3], zs = new double[3], ss = new double[3];
                    double upstream_Max = double.MinValue;
                    for (int k = 0; k < 3; k++)
                    {
                        xs[k] = coordinate3s[k].X - origin.X;
                        ys[k] = coordinate3s[k].Y - origin.Y;
                        zs[k] = coordinate3s[k].Z - origin.Z;
                        ss[k] = (normal.X * xs[k]) + (normal.Y * ys[k]) + (normal.Z * zs[k]);
                        upstream_Max = Math.Max(upstream_Max, -sign * ss[k]);
                    }

                    if (upstream_Max <= tolerance)
                    {
                        continue;
                    }

                    double[] xs_Clip = new double[4], ys_Clip = new double[4], zs_Clip = new double[4], ss_Clip = new double[4];
                    int count = 0;
                    for (int k = 0; k < 3; k++)
                    {
                        int k_2 = (k + 1) % 3;
                        double upstream_1 = -sign * ss[k];
                        double upstream_2 = -sign * ss[k_2];
                        bool inside_1 = upstream_1 >= -tolerance;
                        bool inside_2 = upstream_2 >= -tolerance;

                        if (inside_1)
                        {
                            xs_Clip[count] = xs[k];
                            ys_Clip[count] = ys[k];
                            zs_Clip[count] = zs[k];
                            ss_Clip[count] = ss[k];
                            count++;
                        }

                        if (inside_1 != inside_2 && Math.Abs(upstream_1 - upstream_2) > tolerance)
                        {
                            double factor = upstream_1 / (upstream_1 - upstream_2);
                            if (factor > 0 && factor < 1)
                            {
                                xs_Clip[count] = xs[k] + (factor * (xs[k_2] - xs[k]));
                                ys_Clip[count] = ys[k] + (factor * (ys[k_2] - ys[k]));
                                zs_Clip[count] = zs[k] + (factor * (zs[k_2] - zs[k]));
                                ss_Clip[count] = 0;
                                count++;
                            }
                        }
                    }

                    if (count < 3)
                    {
                        continue;
                    }

                    double minX = shadowReceiver.Min.X - tolerance, minY = shadowReceiver.Min.Y - tolerance;
                    double maxX = shadowReceiver.Max.X + tolerance, maxY = shadowReceiver.Max.Y + tolerance;

                    Coordinate2[] coordinate2s = [new(), new(), new(), new()];
                    bool inRange_X_Min = false, inRange_X_Max = false, inRange_Y_Min = false, inRange_Y_Max = false;
                    for (int k = 0; k < count; k++)
                    {
                        double factor = -ss_Clip[k] / dotProduct;
                        double x = xs_Clip[k] + (factor * vector.X);
                        double y = ys_Clip[k] + (factor * vector.Y);
                        double z = zs_Clip[k] + (factor * vector.Z);

                        double u = (shadowReceiver.AxisX.X * x) + (shadowReceiver.AxisX.Y * y) + (shadowReceiver.AxisX.Z * z);
                        double v = (shadowReceiver.AxisY.X * x) + (shadowReceiver.AxisY.Y * y) + (shadowReceiver.AxisY.Z * z);
                        coordinate2s[k] = new Coordinate2(u, v);

                        inRange_X_Min |= u >= minX;
                        inRange_X_Max |= u <= maxX;
                        inRange_Y_Min |= v >= minY;
                        inRange_Y_Max |= v <= maxY;
                    }

                    if (!inRange_X_Min || !inRange_X_Max || !inRange_Y_Min || !inRange_Y_Max)
                    {
                        continue;
                    }

                    double area = 0;
                    for (int k = 0; k < count; k++)
                    {
                        Coordinate2 coordinate2_1 = coordinate2s[k];
                        Coordinate2 coordinate2_2 = coordinate2s[(k + 1) % count];
                        area += (coordinate2_1.X * coordinate2_2.Y) - (coordinate2_2.X * coordinate2_1.Y);
                    }

                    if (Math.Abs(area / 2) <= tolerance * tolerance)
                    {
                        continue;
                    }

                    result.Add(new ShadowPolygon2(coordinate2s[0], coordinate2s[1], coordinate2s[2], coordinate2s[3], i, j, count));
                }
            }

            return SortShadowPolygons(result);
        }

        /// <summary>
        /// Sorts shadow polygons by receiver index, then triangle index.
        /// </summary>
        private static List<ShadowPolygon2> SortShadowPolygons(IEnumerable<ShadowPolygon2> shadowPolygon2s)
        {
            return [.. shadowPolygon2s.OrderBy(x => x.ReceiverIndex).ThenBy(x => x.TriangleIndex)];
        }

        /// <summary>
        /// Returns whether two shadow polygons are bitwise equal, NaN points included.
        /// </summary>
        private static bool AreBitwiseEqual(ShadowPolygon2 shadowPolygon2_1, ShadowPolygon2 shadowPolygon2_2)
        {
            if (shadowPolygon2_1.ReceiverIndex != shadowPolygon2_2.ReceiverIndex || shadowPolygon2_1.TriangleIndex != shadowPolygon2_2.TriangleIndex || shadowPolygon2_1.Count != shadowPolygon2_2.Count)
            {
                return false;
            }

            Coordinate2[] coordinate2s_1 = ShadowPolygonPoints(shadowPolygon2_1);
            Coordinate2[] coordinate2s_2 = ShadowPolygonPoints(shadowPolygon2_2);
            for (int k = 0; k < 4; k++)
            {
                if (BitConverter.DoubleToInt64Bits(coordinate2s_1[k].X) != BitConverter.DoubleToInt64Bits(coordinate2s_2[k].X) || BitConverter.DoubleToInt64Bits(coordinate2s_1[k].Y) != BitConverter.DoubleToInt64Bits(coordinate2s_2[k].Y))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the four points of a shadow polygon as an array.
        /// </summary>
        private static Coordinate2[] ShadowPolygonPoints(ShadowPolygon2 shadowPolygon2)
        {
            return [shadowPolygon2.Point_1, shadowPolygon2.Point_2, shadowPolygon2.Point_3, shadowPolygon2.Point_4];
        }
    }
}
