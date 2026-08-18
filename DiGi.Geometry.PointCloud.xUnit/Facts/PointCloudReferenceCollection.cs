using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.Geometry.PointCloud.Core.Classes;

namespace DiGi.Geometry.PointCloud.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a reference table round trips through JSON with every entry keeping both its identifier and its concrete reference type.
        /// <para>The table deliberately mixes the three reference shapes that identify a DiGi model object, because the entries are stored behind an interface and only the type discriminator carries the concrete type across the round trip.</para>
        /// <para><see cref="ComplexReference"/> is present specifically because it implements <see cref="IComplexReference"/> and NOT <see cref="IUniqueReference"/>; typing the table on the narrower interface would exclude it, and this test is what would fail if someone narrowed it.</para>
        /// </summary>
        [Fact]
        public void PointCloudReferenceCollection()
        {
            TypeReference typeReference = new("DiGi.Geometry.Spatial.Classes.Point3D, DiGi.Geometry");

            GuidReference guidReference = new(typeReference, new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"));
            UniqueIdReference uniqueIdReference = new(typeReference, "Mazowieckie");
            ComplexReference complexReference = new([guidReference, uniqueIdReference]);

            PointCloudReferenceCollection pointCloudReferenceCollection = new([guidReference, uniqueIdReference, complexReference]);

            Assert.Equal(3, pointCloudReferenceCollection.Count);

            Assert.IsType<GuidReference>(pointCloudReferenceCollection.GetReference(0));
            Assert.IsType<UniqueIdReference>(pointCloudReferenceCollection.GetReference(1));
            Assert.IsType<ComplexReference>(pointCloudReferenceCollection.GetReference(2));

            Assert.Null(pointCloudReferenceCollection.GetReference(-1));
            Assert.Null(pointCloudReferenceCollection.GetReference(3));

            Assert.True(pointCloudReferenceCollection.TryGetId(guidReference, out int id_GuidReference));
            Assert.Equal(0, id_GuidReference);

            Assert.True(pointCloudReferenceCollection.TryGetId(complexReference, out int id_ComplexReference));
            Assert.Equal(2, id_ComplexReference);

            Assert.False(pointCloudReferenceCollection.TryGetId(new UniqueIdReference(typeReference, "Absent"), out int id_Absent));
            Assert.Equal(-1, id_Absent);

            DiGi.Core.xUnit.Query.SerializationCheck(pointCloudReferenceCollection);
        }

        /// <summary>
        /// Tests that the reference table factory removes duplicates while preserving the identifier of the first occurrence.
        /// <para>A duplicate matters more here than in an ordinary collection: it would occupy a second identifier and quietly split the points of one model object into two unrelated groups.</para>
        /// </summary>
        [Fact]
        public void PointCloudReferenceCollection_Create()
        {
            TypeReference typeReference = new("DiGi.Geometry.Spatial.Classes.Point3D, DiGi.Geometry");

            UniqueIdReference uniqueIdReference_1 = new(typeReference, "One");
            UniqueIdReference uniqueIdReference_2 = new(typeReference, "Two");

            PointCloudReferenceCollection? pointCloudReferenceCollection = Core.Create.PointCloudReferenceCollection([uniqueIdReference_1, uniqueIdReference_2, new UniqueIdReference(typeReference, "One")]);

            Assert.NotNull(pointCloudReferenceCollection);
            Assert.Equal(2, pointCloudReferenceCollection.Count);

            Assert.True(pointCloudReferenceCollection.TryGetId(uniqueIdReference_1, out int id_1));
            Assert.Equal(0, id_1);

            Assert.True(pointCloudReferenceCollection.TryGetId(uniqueIdReference_2, out int id_2));
            Assert.Equal(1, id_2);

            Assert.Null(Core.Create.PointCloudReferenceCollection((IEnumerable<ISerializableReference>?)null));
        }

        /// <summary>
        /// Tests that the per-point factory builds the table and the identifier array together, mapping repeated references onto one identifier and unlinked points onto -1.
        /// <para>This is the shape a segmentation pass produces, and building both halves in one place is what keeps them consistent, because nothing downstream can detect a table and an identifier array that disagree.</para>
        /// </summary>
        [Fact]
        public void PointCloudReferenceCollection_Create_ReferenceIndexes()
        {
            TypeReference typeReference = new("DiGi.Geometry.Spatial.Classes.Point3D, DiGi.Geometry");

            UniqueIdReference uniqueIdReference_1 = new(typeReference, "Wall");
            UniqueIdReference uniqueIdReference_2 = new(typeReference, "Roof");

            List<ISerializableReference> references = [uniqueIdReference_1, uniqueIdReference_2, uniqueIdReference_1, null!, uniqueIdReference_2];

            PointCloudReferenceCollection? pointCloudReferenceCollection = Core.Create.PointCloudReferenceCollection(references, out int[]? referenceIndexes);

            Assert.NotNull(pointCloudReferenceCollection);
            Assert.Equal(2, pointCloudReferenceCollection.Count);

            Assert.NotNull(referenceIndexes);
            Assert.Equal(5, referenceIndexes.Length);

            Assert.Equal(0, referenceIndexes[0]);
            Assert.Equal(1, referenceIndexes[1]);
            Assert.Equal(0, referenceIndexes[2]);
            Assert.Equal(-1, referenceIndexes[3]);
            Assert.Equal(1, referenceIndexes[4]);
        }
    }
}
