using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that combining two plain references produces a two-step <see cref="ComplexReference"/> in order.
        /// </summary>
        [Fact]
        public void Reference_TwoPlainReferences()
        {
            GuidReference guidReference = new(new TypeReference(typeof(TestObject)), Guid.NewGuid());
            UniqueIdReference uniqueIdReference = new(new TypeReference(typeof(Classes.UniqueIdObject)), "BLD-001");

            IReference? reference = Create.Reference(guidReference, uniqueIdReference);

            ComplexReference? complexReference = Assert.IsType<ComplexReference>(reference);
            Assert.Equal(2, complexReference.Count);
            Assert.Equal(guidReference.ToString(), complexReference[0]?.ToString());
            Assert.Equal(uniqueIdReference.ToString(), complexReference[1]?.ToString());
        }

        /// <summary>
        /// Tests that combining is flattened, not nested: combining two complex references, and combining a complex
        /// with a plain reference, spreads the operands' steps into a single chain rather than adding a path as one
        /// step.
        /// </summary>
        [Fact]
        public void Reference_FlattensComplexReferences()
        {
            GuidReference guidReference_1 = new(new TypeReference(typeof(TestObject)), Guid.NewGuid());
            GuidReference guidReference_2 = new(new TypeReference(typeof(TestObject)), Guid.NewGuid());
            GuidReference guidReference_3 = new(new TypeReference(typeof(TestObject)), Guid.NewGuid());
            GuidReference guidReference_4 = new(new TypeReference(typeof(TestObject)), Guid.NewGuid());

            List<ISerializableReference?> references_1 = [guidReference_1, guidReference_2];
            List<ISerializableReference?> references_2 = [guidReference_3, guidReference_4];
            ComplexReference complexReference_1 = new(references_1);
            ComplexReference complexReference_2 = new(references_2);

            IReference? reference_ComplexComplex = Create.Reference(complexReference_1, complexReference_2);
            ComplexReference? complexReference_Joined = Assert.IsType<ComplexReference>(reference_ComplexComplex);
            Assert.Equal(4, complexReference_Joined.Count);
            Assert.Equal(guidReference_1.ToString(), complexReference_Joined[0]?.ToString());
            Assert.Equal(guidReference_4.ToString(), complexReference_Joined[3]?.ToString());

            IReference? reference_ComplexPlain = Create.Reference(complexReference_1, guidReference_3);
            ComplexReference? complexReference_ComplexPlain = Assert.IsType<ComplexReference>(reference_ComplexPlain);
            Assert.Equal(3, complexReference_ComplexPlain.Count);

            IReference? reference_PlainComplex = Create.Reference(guidReference_1, complexReference_2);
            ComplexReference? complexReference_PlainComplex = Assert.IsType<ComplexReference>(reference_PlainComplex);
            Assert.Equal(3, complexReference_PlainComplex.Count);
            Assert.Equal(guidReference_1.ToString(), complexReference_PlainComplex[0]?.ToString());
        }

        /// <summary>
        /// Tests that combining does not mutate either operand, since a complex reference exposes only a copy of its
        /// steps.
        /// </summary>
        [Fact]
        public void Reference_DoesNotMutateOperands()
        {
            GuidReference guidReference_1 = new(new TypeReference(typeof(TestObject)), Guid.NewGuid());
            GuidReference guidReference_2 = new(new TypeReference(typeof(TestObject)), Guid.NewGuid());

            List<ISerializableReference?> references = [guidReference_1];
            ComplexReference complexReference = new(references);

            Create.Reference(complexReference, guidReference_2);

            Assert.Equal(1, complexReference.Count);
        }

        /// <summary>
        /// Tests that a null operand is a no-op that returns the other operand unchanged, and that combining two nulls
        /// returns null. The surviving operand is not wrapped in a new complex reference.
        /// </summary>
        [Fact]
        public void Reference_NullOperands()
        {
            GuidReference guidReference = new(new TypeReference(typeof(TestObject)), Guid.NewGuid());

            Assert.Same(guidReference, Create.Reference(guidReference, null));
            Assert.Same(guidReference, Create.Reference(null, guidReference));
            Assert.Null(Create.Reference((IReference?)null, null));
        }

        /// <summary>
        /// Tests that combining with a non-serializable reference returns null, because such a reference cannot be a
        /// step of a complex reference.
        /// </summary>
        [Fact]
        public void Reference_NonSerializableReference_ReturnsNull()
        {
            GuidReference guidReference = new(new TypeReference(typeof(TestObject)), Guid.NewGuid());
            Relation.Classes.RelationListClusterReference relationListClusterReference = new(
                new TypeReference(typeof(TestObject)),
                new TypeReference(typeof(Classes.UniqueIdObject)),
                0);

            Assert.Null(Create.Reference(guidReference, relationListClusterReference));
            Assert.Null(Create.Reference(relationListClusterReference, guidReference));
        }

        /// <summary>
        /// Tests that a chain built by successive combines round-trips through the reference string grammar,
        /// confirming the combined result is a well-formed complex reference.
        /// </summary>
        [Fact]
        public void Reference_Chained_RoundTrips()
        {
            GuidReference guidReference = new(new TypeReference(typeof(TestObject)), Guid.NewGuid());
            UniqueIdReference uniqueIdReference = new(new TypeReference(typeof(Classes.UniqueIdObject)), "BLD-001");
            TypeReference typeReference = new(typeof(TestObject));

            IReference? reference = guidReference.Reference(uniqueIdReference).Reference(typeReference);

            ComplexReference? complexReference = Assert.IsType<ComplexReference>(reference);
            Assert.Equal(3, complexReference.Count);

            Assert.True(Core.Query.TryParse(complexReference.ToString(), out ComplexReference? complexReference_Parsed));
            Assert.NotNull(complexReference_Parsed);
            Assert.Equal(complexReference.ToString(), complexReference_Parsed.ToString());
            Assert.True(complexReference.Equals(complexReference_Parsed));
        }
    }
}