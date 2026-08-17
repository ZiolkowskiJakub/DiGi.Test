using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.Geometry.PointCloud.Core.Classes;
using DiGi.Geometry.PointCloud.Core.Enums;
using DiGi.Geometry.PointCloud.Spatial;
using DiGi.Geometry.PointCloud.Spatial.Classes;
using DiGi.Geometry.Spatial.Classes;
using System.Diagnostics;
using System.Reflection;

namespace DiGi.Geometry.PointCloud.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Builds a cloud whose per-point model object identifier is a pure function of the point, so that a filter which loses track of which identifier belongs to which point cannot produce a result that still looks correct.
        /// <para>The X coordinate holds the original point ordinal and the model object is chosen by that ordinal modulo seven, so the identifier of any surviving point must always equal its X coordinate modulo seven.</para>
        /// </summary>
        /// <param name="count">The number of points to build.</param>
        /// <returns>A new <see cref="ReferencedPointCloud3D"/> carrying seven distinct model objects.</returns>
        private static ReferencedPointCloud3D ReferencedPointCloud3D_Derivable(int count)
        {
            TypeReference typeReference = new("DiGi.Geometry.Spatial.Classes.Point3D, DiGi.Geometry");

            List<ISerializableReference> references_Distinct = [];
            for (int i = 0; i < 7; i++)
            {
                references_Distinct.Add(new UniqueIdReference(typeReference, $"Component{i}"));
            }

            Random random = new(12345);

            double[] x = new double[count];
            double[] y = new double[count];
            double[] z = new double[count];

            List<ISerializableReference> references = [];
            for (int i = 0; i < count; i++)
            {
                x[i] = i;
                y[i] = (random.NextDouble() * 2000.0) - 1000.0;
                z[i] = (random.NextDouble() * 2000.0) - 1000.0;

                references.Add(references_Distinct[i % 7]);
            }

            PointCloudReferenceCollection? pointCloudReferenceCollection = PointCloud.Core.Create.PointCloudReferenceCollection(references, out int[]? referenceIndexes);

            Assert.NotNull(pointCloudReferenceCollection);
            Assert.NotNull(referenceIndexes);

            return new ReferencedPointCloud3D(x, y, z, referenceIndexes, pointCloudReferenceCollection);
        }

        /// <summary>
        /// Asserts that every point of a filtered cloud still carries the model object identifier that belongs to it.
        /// </summary>
        /// <param name="referencedPointCloud3D">The filtered cloud to check.</param>
        private static void ReferencedPointCloud3D_AssertDerivable(ReferencedPointCloud3D? referencedPointCloud3D)
        {
            Assert.NotNull(referencedPointCloud3D);
            Assert.True(referencedPointCloud3D.IsReferenced);

            for (int i = 0; i < referencedPointCloud3D.Count; i++)
            {
                Assert.True(referencedPointCloud3D.TryGetPoint(i, out double x, out _, out _));
                Assert.True(referencedPointCloud3D.TryGetReferenceIndex(i, out int referenceIndex));

                Assert.Equal((int)x % 7, referenceIndex);
            }
        }

        /// <summary>
        /// Tests that a cloud carrying per-point model object links survives a JSON round trip with the coordinates, the identifiers and the concrete reference types all intact.
        /// <para>The identifiers and the coordinates are encoded into two separate private properties, and the serializer applies members in the order they appear in the document rather than in the order the type declares them, so this is the test that catches either payload being written but not read back.</para>
        /// </summary>
        [Fact]
        public void ReferencedPointCloud3D_Json()
        {
            TypeReference typeReference = new("DiGi.Geometry.Spatial.Classes.Point3D, DiGi.Geometry");

            GuidReference guidReference = new(typeReference, new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"));
            UniqueIdReference uniqueIdReference = new(typeReference, "Wall");
            ComplexReference complexReference = new([guidReference, uniqueIdReference]);

            PointCloudReferenceCollection pointCloudReferenceCollection = new([guidReference, uniqueIdReference, complexReference]);

            double[] x = [0.0, 1.0, 2.0, 3.0, 4.0];
            double[] y = [5.0, 6.0, 7.0, 8.0, 9.0];
            double[] z = [10.0, 11.0, 12.0, 13.0, 14.0];

            int[] referenceIndexes = [0, 1, -1, 2, 1];

            ReferencedPointCloud3D referencedPointCloud3D = new(x, y, z, referenceIndexes, pointCloudReferenceCollection);

            Assert.Equal(5, referencedPointCloud3D.Count);
            Assert.Equal(3, referencedPointCloud3D.ReferenceCount);
            Assert.True(referencedPointCloud3D.IsReferenced);

            Assert.IsType<GuidReference>(referencedPointCloud3D.GetReference(0));
            Assert.IsType<UniqueIdReference>(referencedPointCloud3D.GetReference(1));
            Assert.Null(referencedPointCloud3D.GetReference(2));
            Assert.IsType<ComplexReference>(referencedPointCloud3D.GetReference(3));

            Assert.False(referencedPointCloud3D.TryGetReferenceIndex(2, out int referenceIndex_Unlinked));
            Assert.Equal(-1, referenceIndex_Unlinked);

            Assert.False(referencedPointCloud3D.TryGetReferenceIndex(5, out _));

            string? @string = DiGi.Core.Convert.ToSystem_String(referencedPointCloud3D);

            Assert.NotNull(@string);

            ReferencedPointCloud3D? referencedPointCloud3D_Actual = DiGi.Core.Convert.ToDiGi<ReferencedPointCloud3D>(@string)?.FirstOrDefault();

            Assert.NotNull(referencedPointCloud3D_Actual);
            Assert.Equal(5, referencedPointCloud3D_Actual.Count);
            Assert.Equal(3, referencedPointCloud3D_Actual.ReferenceCount);
            Assert.True(referencedPointCloud3D_Actual.IsReferenced);

            for (int i = 0; i < 5; i++)
            {
                Assert.True(referencedPointCloud3D_Actual.TryGetPoint(i, out double x_Actual, out double y_Actual, out double z_Actual));

                Assert.Equal(x[i], x_Actual);
                Assert.Equal(y[i], y_Actual);
                Assert.Equal(z[i], z_Actual);
            }

            int[]? referenceIndexes_Actual = referencedPointCloud3D_Actual.GetReferenceIndexes();

            Assert.NotNull(referenceIndexes_Actual);
            Assert.Equal(referenceIndexes, referenceIndexes_Actual);

            Assert.IsType<ComplexReference>(referencedPointCloud3D_Actual.GetReference(3));

            DiGi.Core.xUnit.Query.SerializationCheck(referencedPointCloud3D);
        }

        /// <summary>
        /// Tests that identifiers which do not hold exactly one value per point are dropped rather than adopted.
        /// <para>A cloud with no links is recoverable, while a cloud whose links are offset by one silently attributes every point to the wrong model object, so the mismatch has to fail closed.</para>
        /// </summary>
        [Fact]
        public void ReferencedPointCloud3D_Invariant()
        {
            TypeReference typeReference = new("DiGi.Geometry.Spatial.Classes.Point3D, DiGi.Geometry");

            PointCloudReferenceCollection pointCloudReferenceCollection = new([new UniqueIdReference(typeReference, "Wall")]);

            double[] x = [0.0, 1.0, 2.0];
            double[] y = [3.0, 4.0, 5.0];
            double[] z = [6.0, 7.0, 8.0];

            ReferencedPointCloud3D referencedPointCloud3D_Short = new(x, y, z, [0, 0], pointCloudReferenceCollection);

            Assert.Equal(3, referencedPointCloud3D_Short.Count);
            Assert.False(referencedPointCloud3D_Short.IsReferenced);
            Assert.Null(referencedPointCloud3D_Short.GetReferenceIndexes());

            ReferencedPointCloud3D referencedPointCloud3D_Long = new(x, y, z, [0, 0, 0, 0], pointCloudReferenceCollection);

            Assert.False(referencedPointCloud3D_Long.IsReferenced);
            Assert.Null(referencedPointCloud3D_Long.GetReferenceIndexes());

            ReferencedPointCloud3D referencedPointCloud3D_Empty = new(null, null, null, null, null);

            Assert.Equal(0, referencedPointCloud3D_Empty.Count);
            Assert.Equal(0, referencedPointCloud3D_Empty.ReferenceCount);
            Assert.Null(referencedPointCloud3D_Empty.GetReference(0));

            DiGi.Core.xUnit.Query.SerializationCheck(referencedPointCloud3D_Empty);
        }

        /// <summary>
        /// Tests that a cloud carrying per-point model object links survives a round trip through the binary format, and that a cloud carrying none still encodes exactly as a plain cloud does.
        /// <para>The two blocks are written one after the other and the reader locates the second from the length of the first, so the header bytes of both are asserted explicitly: a change to either layout has to fail loudly rather than silently produce files that cannot be read back.</para>
        /// </summary>
        [Fact]
        public void ReferencedPointCloud3D_Binary()
        {
            ReferencedPointCloud3D referencedPointCloud3D = ReferencedPointCloud3D_Derivable(500);

            byte[]? bytes = PointCloud.Spatial.Convert.ToSystem_Bytes(referencedPointCloud3D, PointCloudFormat.Binary);

            Assert.NotNull(bytes);

            int length_Coordinates = 32 + (500 * 3 * 8);

            Assert.Equal((byte)'D', bytes[0]);
            Assert.Equal((byte)'G', bytes[1]);
            Assert.Equal((byte)'P', bytes[2]);
            Assert.Equal((byte)'C', bytes[3]);
            Assert.Equal(1, bytes[4]);

            Assert.Equal((byte)'D', bytes[length_Coordinates]);
            Assert.Equal((byte)'G', bytes[length_Coordinates + 1]);
            Assert.Equal((byte)'P', bytes[length_Coordinates + 2]);
            Assert.Equal((byte)'R', bytes[length_Coordinates + 3]);
            Assert.Equal(1, bytes[length_Coordinates + 4]);
            Assert.Equal(4, bytes[length_Coordinates + 6]);
            Assert.Equal(1, bytes[length_Coordinates + 16]);

            ReferencedPointCloud3D? referencedPointCloud3D_Actual = PointCloud.Spatial.Create.ReferencedPointCloud3D(bytes);

            Assert.NotNull(referencedPointCloud3D_Actual);
            Assert.Equal(500, referencedPointCloud3D_Actual.Count);
            Assert.Equal(7, referencedPointCloud3D_Actual.ReferenceCount);

            ReferencedPointCloud3D_AssertDerivable(referencedPointCloud3D_Actual);

            // A cloud carrying no links must encode to the coordinate block alone, byte-identical to a plain cloud.
            ReferencedPointCloud3D referencedPointCloud3D_Unlinked = new([0.0, 1.0], [2.0, 3.0], [4.0, 5.0], null, null);

            byte[]? bytes_Unlinked = PointCloud.Spatial.Convert.ToSystem_Bytes(referencedPointCloud3D_Unlinked, PointCloudFormat.Binary);
            byte[]? bytes_Plain = PointCloud.Spatial.Convert.ToSystem_Bytes(new PointCloud3D([0.0, 1.0], [2.0, 3.0], [4.0, 5.0]), PointCloudFormat.Binary);

            Assert.NotNull(bytes_Unlinked);
            Assert.NotNull(bytes_Plain);
            Assert.Equal(bytes_Plain, bytes_Unlinked);

            // A buffer holding coordinates alone decodes to a cloud with no links rather than failing.
            ReferencedPointCloud3D? referencedPointCloud3D_FromPlain = PointCloud.Spatial.Create.ReferencedPointCloud3D(bytes_Plain);

            Assert.NotNull(referencedPointCloud3D_FromPlain);
            Assert.Equal(2, referencedPointCloud3D_FromPlain.Count);
            Assert.Equal(0, referencedPointCloud3D_FromPlain.ReferenceCount);
            Assert.False(referencedPointCloud3D_FromPlain.IsReferenced);

            // Truncated inside the embedded reference table. The identifiers sit before it and are fully
            // present, so they still decode; only the table is lost, and the points then resolve to nothing.
            byte[] bytes_TruncatedCollection = new byte[bytes.Length - 8];
            Array.Copy(bytes, bytes_TruncatedCollection, bytes_TruncatedCollection.Length);

            ReferencedPointCloud3D? referencedPointCloud3D_TruncatedCollection = PointCloud.Spatial.Create.ReferencedPointCloud3D(bytes_TruncatedCollection);

            Assert.NotNull(referencedPointCloud3D_TruncatedCollection);
            Assert.Equal(500, referencedPointCloud3D_TruncatedCollection.Count);
            Assert.True(referencedPointCloud3D_TruncatedCollection.IsReferenced);
            Assert.Equal(0, referencedPointCloud3D_TruncatedCollection.ReferenceCount);
            Assert.Null(referencedPointCloud3D_TruncatedCollection.GetReference(0));

            // Truncated inside the identifier payload itself. Nothing can be recovered from a partial
            // identifier array, so the links are dropped entirely rather than decoded short and misaligned.
            byte[] bytes_TruncatedIndexes = new byte[length_Coordinates + 32 + 100];
            Array.Copy(bytes, bytes_TruncatedIndexes, bytes_TruncatedIndexes.Length);

            ReferencedPointCloud3D? referencedPointCloud3D_TruncatedIndexes = PointCloud.Spatial.Create.ReferencedPointCloud3D(bytes_TruncatedIndexes);

            Assert.NotNull(referencedPointCloud3D_TruncatedIndexes);
            Assert.Equal(500, referencedPointCloud3D_TruncatedIndexes.Count);
            Assert.False(referencedPointCloud3D_TruncatedIndexes.IsReferenced);
            Assert.Null(referencedPointCloud3D_TruncatedIndexes.GetReferenceIndexes());

            Assert.Null(PointCloud.Spatial.Create.ReferencedPointCloud3D((byte[]?)null));
        }

        /// <summary>
        /// Tests that a cloud carrying per-point model object links survives a round trip through a file.
        /// </summary>
        [Fact]
        public void ReferencedPointCloud3D_File()
        {
            ReferencedPointCloud3D referencedPointCloud3D = ReferencedPointCloud3D_Derivable(250);

            byte[]? bytes = PointCloud.Spatial.Convert.ToSystem_Bytes(referencedPointCloud3D, PointCloudFormat.Binary);

            Assert.NotNull(bytes);

            string? directory = DiGi.Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly());

            Assert.NotNull(directory);

            string path = System.IO.Path.Combine(directory, "ReferencedPointCloud3D.dgpc");

            File.WriteAllBytes(path, bytes);

            ReferencedPointCloud3D? referencedPointCloud3D_Actual = PointCloud.Spatial.Create.ReferencedPointCloud3D(new FileInfo(path));

            Assert.NotNull(referencedPointCloud3D_Actual);
            Assert.Equal(250, referencedPointCloud3D_Actual.Count);
            Assert.Equal(7, referencedPointCloud3D_Actual.ReferenceCount);

            ReferencedPointCloud3D_AssertDerivable(referencedPointCloud3D_Actual);

            Assert.Null(PointCloud.Spatial.Create.ReferencedPointCloud3D(new FileInfo(System.IO.Path.Combine(directory, "Absent.dgpc"))));
        }

        /// <summary>
        /// Tests that filtering a cloud keeps every surviving point paired with its own model object, through all three routes the filter can take.
        /// <para>This is the test the whole design exists to pass. The filter runs below the index threshold so that the exhaustive scan branch is exercised, above it so that the indexed branch is exercised, and against a box enclosing the whole cloud so that the early-out copy is exercised. A gather that compacted the coordinates without compacting the identifiers would leave the point count and the reference table looking entirely healthy while attributing every point after the first discarded one to the wrong model object.</para>
        /// </summary>
        [Fact]
        public void ReferencedPointCloud3D_InRange()
        {
            BoundingBox3D boundingBox3D = new(new Point3D(-2000.0, -500.0, -500.0), new Point3D(2000.0, 500.0, 500.0));

            // Below the index threshold, so the exhaustive scan branch runs.
            ReferencedPointCloud3D referencedPointCloud3D_Small = ReferencedPointCloud3D_Derivable(1000);

            Assert.False(referencedPointCloud3D_Small.IsIndexed);

            ReferencedPointCloud3D? referencedPointCloud3D_Small_InRange = referencedPointCloud3D_Small.InRange(boundingBox3D);

            Assert.NotNull(referencedPointCloud3D_Small_InRange);
            Assert.InRange(referencedPointCloud3D_Small_InRange.Count, 1, 999);

            ReferencedPointCloud3D_AssertDerivable(referencedPointCloud3D_Small_InRange);

            // Above the index threshold, so the indexed branch runs.
            ReferencedPointCloud3D referencedPointCloud3D_Large = ReferencedPointCloud3D_Derivable(70000);

            ReferencedPointCloud3D? referencedPointCloud3D_Large_InRange = referencedPointCloud3D_Large.InRange(boundingBox3D);

            Assert.NotNull(referencedPointCloud3D_Large_InRange);
            Assert.InRange(referencedPointCloud3D_Large_InRange.Count, 1, 69999);
            Assert.True(referencedPointCloud3D_Large.IsIndexed);

            ReferencedPointCloud3D_AssertDerivable(referencedPointCloud3D_Large_InRange);

            // A box enclosing the whole cloud, so the early-out copy runs.
            BoundingBox3D? boundingBox3D_Enclosing = referencedPointCloud3D_Small.GetBoundingBox();

            Assert.NotNull(boundingBox3D_Enclosing);

            ReferencedPointCloud3D? referencedPointCloud3D_Enclosed = referencedPointCloud3D_Small.InRange(boundingBox3D_Enclosing);

            Assert.NotNull(referencedPointCloud3D_Enclosed);
            Assert.Equal(1000, referencedPointCloud3D_Enclosed.Count);

            ReferencedPointCloud3D_AssertDerivable(referencedPointCloud3D_Enclosed);

            // The early-out returns a copy, not the source, and the copy shares no identifier array with it.
            Assert.NotSame(referencedPointCloud3D_Small, referencedPointCloud3D_Enclosed);
            Assert.NotSame(referencedPointCloud3D_Small.GetReferenceIndexes(false), referencedPointCloud3D_Enclosed.GetReferenceIndexes(false));
        }

        /// <summary>
        /// Tests that the filter and the index query agree on which points fall inside a box, on both sides of the index threshold.
        /// <para>The two express the same predicate in two places, and the filter for a referenced cloud is built on the index query, so a disagreement between them would silently change which points a referenced cloud keeps.</para>
        /// </summary>
        [Fact]
        public void ReferencedPointCloud3D_InRange_Agreement()
        {
            BoundingBox3D boundingBox3D = new(new Point3D(-2000.0, -500.0, -500.0), new Point3D(2000.0, 500.0, 500.0));

            foreach (int count in new int[] { 1000, 70000 })
            {
                ReferencedPointCloud3D referencedPointCloud3D = ReferencedPointCloud3D_Derivable(count);

                PointCloud3D? pointCloud3D_InRange = PointCloud.Spatial.Query.InRange((PointCloud3D)referencedPointCloud3D, boundingBox3D);
                List<int>? indexes = referencedPointCloud3D.InRangeIndexes(boundingBox3D);
                ReferencedPointCloud3D? referencedPointCloud3D_InRange = referencedPointCloud3D.InRange(boundingBox3D);

                Assert.NotNull(pointCloud3D_InRange);
                Assert.NotNull(indexes);
                Assert.NotNull(referencedPointCloud3D_InRange);

                Assert.Equal(pointCloud3D_InRange.Count, indexes.Count);
                Assert.Equal(pointCloud3D_InRange.Count, referencedPointCloud3D_InRange.Count);
            }
        }

        /// <summary>
        /// Tests that filtering through a variable typed as the base cloud selects the base overload and drops the links.
        /// <para>Extension methods bind statically, so this is a property of the language rather than a defect, but it is the one way a caller can lose the links without any diagnostic. The behaviour is asserted here so that it stays documented and cannot change silently.</para>
        /// </summary>
        [Fact]
        public void ReferencedPointCloud3D_StaticBinding()
        {
            ReferencedPointCloud3D referencedPointCloud3D = ReferencedPointCloud3D_Derivable(1000);

            BoundingBox3D boundingBox3D = new(new Point3D(-2000.0, -500.0, -500.0), new Point3D(2000.0, 500.0, 500.0));

            PointCloud3D pointCloud3D = referencedPointCloud3D;

            PointCloud3D? pointCloud3D_InRange = pointCloud3D.InRange(boundingBox3D);

            Assert.NotNull(pointCloud3D_InRange);
            Assert.IsNotType<ReferencedPointCloud3D>(pointCloud3D_InRange);

            ReferencedPointCloud3D? referencedPointCloud3D_InRange = referencedPointCloud3D.InRange(boundingBox3D);

            Assert.NotNull(referencedPointCloud3D_InRange);
            Assert.IsType<ReferencedPointCloud3D>(referencedPointCloud3D_InRange);
        }

        /// <summary>
        /// Tests that translating and transforming a cloud leave the per-point model object links untouched.
        /// <para>Both preserve the count and the order of the points, so the identifiers continue to line up and nothing has to be gathered. This is asserted rather than assumed, because it is the reason those inherited members are safe to use unchanged.</para>
        /// </summary>
        [Fact]
        public void ReferencedPointCloud3D_Transform()
        {
            ReferencedPointCloud3D referencedPointCloud3D = ReferencedPointCloud3D_Derivable(1000);

            int[]? referenceIndexes_Expected = referencedPointCloud3D.GetReferenceIndexes();

            Assert.NotNull(referenceIndexes_Expected);

            Assert.True(referencedPointCloud3D.Move(new Vector3D(10.0, 20.0, 30.0)));

            Assert.Equal(1000, referencedPointCloud3D.Count);
            Assert.Equal(7, referencedPointCloud3D.ReferenceCount);
            Assert.True(referencedPointCloud3D.IsReferenced);
            Assert.Equal(referenceIndexes_Expected, referencedPointCloud3D.GetReferenceIndexes());
        }

        /// <summary>
        /// Tests that the points of a single model object can be located and extracted as a cloud in its own right.
        /// <para>The extract keeps the identifiers of its source rather than being renumbered, so an identifier means the same thing in both and the two can be compared without a translation step.</para>
        /// </summary>
        [Fact]
        public void ReferencedPointCloud3D_PointIndexes()
        {
            ReferencedPointCloud3D referencedPointCloud3D = ReferencedPointCloud3D_Derivable(700);

            ISerializableReference? reference = referencedPointCloud3D.GetPointCloudReferenceCollection(false)?.GetReference(3);

            Assert.NotNull(reference);

            int[]? indexes = referencedPointCloud3D.PointIndexes(reference);

            Assert.NotNull(indexes);
            Assert.Equal(100, indexes.Length);

            for (int i = 0; i < indexes.Length; i++)
            {
                Assert.Equal(3, indexes[i] % 7);
            }

            ReferencedPointCloud3D? referencedPointCloud3D_Extract = referencedPointCloud3D.ReferencedPointCloud3D(reference);

            Assert.NotNull(referencedPointCloud3D_Extract);
            Assert.Equal(100, referencedPointCloud3D_Extract.Count);
            Assert.Equal(7, referencedPointCloud3D_Extract.ReferenceCount);

            ReferencedPointCloud3D_AssertDerivable(referencedPointCloud3D_Extract);

            TypeReference typeReference = new("DiGi.Geometry.Spatial.Classes.Point3D, DiGi.Geometry");

            Assert.Null(referencedPointCloud3D.PointIndexes(new UniqueIdReference(typeReference, "Absent")));
            Assert.Null(referencedPointCloud3D.ReferencedPointCloud3D(new UniqueIdReference(typeReference, "Absent")));
        }

        /// <summary>
        /// Tests that the per-point links cost one flat integer per point rather than one object per point.
        /// <para>This is the whole premise of the design, so it is measured rather than assumed. A reference object per point would cost well over a hundred bytes and one garbage collected object each, which is two orders of magnitude away from the bound asserted here.</para>
        /// </summary>
        [Fact]
        public void ReferencedPointCloud3D_Memory()
        {
            int count = 2000000;

            // Warm-up, so that the measurement is not dominated by first-call initialisation.
            ReferencedPointCloud3D_Derivable(1000);

            Random random = new(12345);

            double[] x = new double[count];
            double[] y = new double[count];
            double[] z = new double[count];

            int[] referenceIndexes = new int[count];

            for (int i = 0; i < count; i++)
            {
                x[i] = (random.NextDouble() * 2000.0) - 1000.0;
                y[i] = (random.NextDouble() * 2000.0) - 1000.0;
                z[i] = (random.NextDouble() * 2000.0) - 1000.0;

                referenceIndexes[i] = i % 7;
            }

            TypeReference typeReference = new("DiGi.Geometry.Spatial.Classes.Point3D, DiGi.Geometry");

            List<ISerializableReference> references = [];
            for (int i = 0; i < 7; i++)
            {
                references.Add(new UniqueIdReference(typeReference, $"Component{i}"));
            }

            PointCloudReferenceCollection pointCloudReferenceCollection = new(references);

            long memory_Before = GC.GetTotalMemory(true);

            ReferencedPointCloud3D referencedPointCloud3D = new(x, y, z, referenceIndexes, pointCloudReferenceCollection);

            long memory_After = GC.GetTotalMemory(true);

            Assert.True(referencedPointCloud3D.IsReferenced);

            // This constructor copies, so the growth is the whole cloud: three doubles and one integer per point,
            // twenty eight bytes. A reference object per point would be well over a hundred, plus one traced object each.
            long bytesPerPoint = (memory_After - memory_Before) / count;

            Assert.InRange(bytesPerPoint, 24, 34);

            Stopwatch stopwatch = Stopwatch.StartNew();

            ReferencedPointCloud3D? referencedPointCloud3D_InRange = referencedPointCloud3D.InRange(new BoundingBox3D(new Point3D(-500.0, -500.0, -500.0), new Point3D(500.0, 500.0, 500.0)));

            stopwatch.Stop();

            Assert.NotNull(referencedPointCloud3D_InRange);
            Assert.True(referencedPointCloud3D_InRange.IsReferenced);
            Assert.Equal(referencedPointCloud3D_InRange.Count, referencedPointCloud3D_InRange.GetReferenceIndexes(false)?.Length);

            Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Filtering two million referenced points took {stopwatch.ElapsedMilliseconds} ms.");
        }
    }
}
