using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="Core.Query.Equals{T}(IEnumerable{T}, IEnumerable{T})"/>
        /// still correctly compares two equal-length sequences with equal elements, after replacing the
        /// Count()+ElementAt(i) double-enumeration with a single paired-enumerator pass.
        /// </summary>
        [Fact]
        public void Equals_Generic_EqualSequences_ReturnsTrue()
        {
            IEnumerable<int> enumerable_1 = Enumerable.Range(1, 5);
            IEnumerable<int> enumerable_2 = Enumerable.Range(1, 5);

            Assert.True(Core.Query.Equals(enumerable_1, enumerable_2));
        }

        /// <summary>
        /// Tests that sequences of different lengths are correctly reported as not equal, including when the
        /// first sequence is longer and when the second sequence is longer.
        /// </summary>
        [Fact]
        public void Equals_Generic_DifferentLengths_ReturnsFalse()
        {
            Assert.False(Core.Query.Equals(Enumerable.Range(1, 5), Enumerable.Range(1, 4)));
            Assert.False(Core.Query.Equals(Enumerable.Range(1, 4), Enumerable.Range(1, 5)));
        }

        /// <summary>
        /// Tests that sequences of the same length with a differing element are reported as not equal.
        /// </summary>
        [Fact]
        public void Equals_Generic_SameLengthDifferentValues_ReturnsFalse()
        {
            List<int> list_1 = [1, 2, 3];
            List<int> list_2 = [1, 2, 4];

            Assert.False(Core.Query.Equals(list_1, list_2));
        }

        /// <summary>
        /// Tests that two empty sequences are considered equal, and that null sequences are handled correctly.
        /// </summary>
        [Fact]
        public void Equals_Generic_EmptyAndNullSequences()
        {
            Assert.True(Core.Query.Equals<int>([], []));

            IEnumerable<int>? null_1 = null;
            IEnumerable<int>? null_2 = null;
            Assert.True(Core.Query.Equals(null_1, null_2));

            Assert.False(Core.Query.Equals(null_1, Enumerable.Range(1, 1)));
            Assert.False(Core.Query.Equals(Enumerable.Range(1, 1), null_2));
        }

        /// <summary>
        /// Tests that nested enumerables (sequences of sequences) are compared element-by-element recursively
        /// via the generic overload, which recurses into the non-generic <see cref="Core.Query.Equals(IEnumerable, IEnumerable)"/>
        /// overload for elements that are themselves <see cref="IEnumerable"/>. This previously failed because that
        /// overload had an index-desync bug when taking the nested-enumerable branch.
        /// </summary>
        [Fact]
        public void Equals_Generic_NestedEnumerables()
        {
            List<List<int>> nested_1 = [[1, 2], [3, 4]];
            List<List<int>> nested_2 = [[1, 2], [3, 4]];
            List<List<int>> nested_3 = [[1, 2], [3, 5]];

            Assert.True(Core.Query.Equals(nested_1, nested_2));
            Assert.False(Core.Query.Equals(nested_1, nested_3));
        }

        /// <summary>
        /// Tests that <see cref="Core.Query.Equals(IEnumerable, IEnumerable)"/> (the non-generic overload) correctly
        /// reports equal sequences as equal. Previously this overload had an inverted equality check
        /// (`if (object_1.Equals(object_2)) return false;`) that made it report equal sequences as unequal.
        /// </summary>
        [Fact]
        public void Equals_NonGeneric_EqualSequences_ReturnsTrue()
        {
            ArrayList arrayList_1 = [1, "a", 2.5];
            ArrayList arrayList_2 = [1, "a", 2.5];

            Assert.True(Core.Query.Equals((IEnumerable)arrayList_1, (IEnumerable)arrayList_2));
        }

        /// <summary>
        /// Tests that the non-generic overload correctly reports sequences with a differing element as not equal.
        /// </summary>
        [Fact]
        public void Equals_NonGeneric_DifferentValues_ReturnsFalse()
        {
            ArrayList arrayList_1 = [1, "a", 2.5];
            ArrayList arrayList_2 = [1, "a", 3.5];

            Assert.False(Core.Query.Equals((IEnumerable)arrayList_1, (IEnumerable)arrayList_2));
        }

        /// <summary>
        /// Tests that the non-generic overload correctly reports sequences of different lengths as not equal,
        /// in both directions.
        /// </summary>
        [Fact]
        public void Equals_NonGeneric_DifferentLengths_ReturnsFalse()
        {
            ArrayList arrayList_1 = [1, 2, 3];
            ArrayList arrayList_2 = [1, 2];

            Assert.False(Core.Query.Equals((IEnumerable)arrayList_1, (IEnumerable)arrayList_2));
            Assert.False(Core.Query.Equals((IEnumerable)arrayList_2, (IEnumerable)arrayList_1));
        }

        /// <summary>
        /// Tests that the non-generic overload treats two null elements at the same position as equal (continue),
        /// rather than the previous backwards behavior of returning false as soon as both were null.
        /// </summary>
        [Fact]
        public void Equals_NonGeneric_NullElementsAtSamePosition_AreEqual()
        {
            ArrayList arrayList_1 = [1, null, 3];
            ArrayList arrayList_2 = [1, null, 3];

            Assert.True(Core.Query.Equals((IEnumerable)arrayList_1, (IEnumerable)arrayList_2));
        }

        /// <summary>
        /// Tests that the non-generic overload correctly compares nested IEnumerable elements without an
        /// index desync, including a case with three top-level elements where the previous bug would have
        /// thrown an IndexOutOfRangeException or produced an incorrect result.
        /// </summary>
        [Fact]
        public void Equals_NonGeneric_NestedEnumerables()
        {
            ArrayList nested_1 = [new ArrayList { 1, 2 }, "middle", new ArrayList { 3, 4 }];
            ArrayList nested_2 = [new ArrayList { 1, 2 }, "middle", new ArrayList { 3, 4 }];
            ArrayList nested_3 = [new ArrayList { 1, 2 }, "middle", new ArrayList { 3, 5 }];

            Assert.True(Core.Query.Equals((IEnumerable)nested_1, (IEnumerable)nested_2));
            Assert.False(Core.Query.Equals((IEnumerable)nested_1, (IEnumerable)nested_3));
        }

        /// <summary>
        /// Tests that the non-generic overload handles null inputs correctly.
        /// </summary>
        [Fact]
        public void Equals_NonGeneric_NullSequences()
        {
            IEnumerable? null_1 = null;
            IEnumerable? null_2 = null;

            Assert.True(Core.Query.Equals(null_1, null_2));
            Assert.False(Core.Query.Equals(null_1, (IEnumerable)new ArrayList { 1 }));
            Assert.False(Core.Query.Equals((IEnumerable)new ArrayList { 1 }, null_2));
        }

        /// <summary>
        /// Tests that <see cref="Core.Query.Equals(IReference, IReference)"/> compares references by value: two
        /// separately constructed references describing the same object are equal, references describing different
        /// objects are not, and two nulls are equal while a single null is not.
        /// </summary>
        [Fact]
        public void Equals_Reference()
        {
            TypeReference typeReference = new(typeof(GuidReference));

            Guid guid = Guid.NewGuid();

            IUniqueReference uniqueReference_1 = new GuidReference(typeReference, guid);
            IUniqueReference uniqueReference_2 = new GuidReference(typeReference, guid);
            IUniqueReference uniqueReference_3 = new GuidReference(typeReference, Guid.NewGuid());

            Assert.True(Core.Query.Equals(uniqueReference_1, uniqueReference_2));
            Assert.False(Core.Query.Equals(uniqueReference_1, uniqueReference_3));

            IReference? reference_Null_1 = null;
            IReference? reference_Null_2 = null;

            Assert.True(Core.Query.Equals(reference_Null_1, reference_Null_2));
            Assert.False(Core.Query.Equals(uniqueReference_1, reference_Null_1));
            Assert.False(Core.Query.Equals(reference_Null_1, uniqueReference_1));
        }

        /// <summary>
        /// Tests the trap <see cref="Core.Query.Equals(IReference, IReference)"/> exists for: the equality operators
        /// are declared on <see cref="SerializableReference"/>, so between two operands whose static type is
        /// <see cref="IUniqueReference"/> the compiler falls back to reference equality and reports two equal
        /// references as different, while the very same values compared through the concrete type - or through the
        /// helper - are equal.
        /// </summary>
        [Fact]
        public void Equals_Reference_OperatorDoesNotApplyToInterfaceOperands()
        {
            TypeReference typeReference = new(typeof(GuidReference));

            Guid guid = Guid.NewGuid();

            GuidReference guidReference_1 = new(typeReference, guid);
            GuidReference guidReference_2 = new(typeReference, guid);

            IUniqueReference uniqueReference_1 = guidReference_1;
            IUniqueReference uniqueReference_2 = guidReference_2;

            Assert.True(guidReference_1 == guidReference_2);
            Assert.False(uniqueReference_1 == uniqueReference_2);

            Assert.True(Core.Query.Equals(uniqueReference_1, uniqueReference_2));
        }
    }
}