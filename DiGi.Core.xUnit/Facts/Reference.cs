using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="Core.Create.UniqueReference(object?)"/> produces the same reference for an
        /// <see cref="IUniqueIdObject"/> whether it is built from the live object or from that object's own
        /// <see cref="JsonObject"/>. Previously the JSON branch only understood the "Guid" property, so a
        /// serialized unique-id object fell through to the generic fallback and was referenced by a hash of the
        /// whole object rather than by its UniqueId, making the two references unequal.
        /// </summary>
        [Fact]
        public void UniqueReference_Create_UniqueIdObject_MatchesJsonObject()
        {
            TestUniqueIdObject testUniqueIdObject = new("Alpha");

            UniqueReference? uniqueReference_Object = Core.Create.UniqueReference(testUniqueIdObject);
            Assert.NotNull(uniqueReference_Object);

            JsonObject? jsonObject = testUniqueIdObject.ToJsonObject();
            Assert.NotNull(jsonObject);

            UniqueReference? uniqueReference_JsonObject = Core.Create.UniqueReference(jsonObject);
            Assert.NotNull(uniqueReference_JsonObject);

            Assert.IsType<UniqueIdReference>(uniqueReference_Object);
            Assert.IsType<UniqueIdReference>(uniqueReference_JsonObject);

            Assert.Equal(testUniqueIdObject.UniqueId, uniqueReference_Object.UniqueId);
            Assert.Equal(testUniqueIdObject.UniqueId, uniqueReference_JsonObject.UniqueId);

            Assert.Equal(uniqueReference_Object.ToString(), uniqueReference_JsonObject.ToString());
            Assert.True(uniqueReference_Object.Equals(uniqueReference_JsonObject));
            Assert.True(uniqueReference_JsonObject.Equals(uniqueReference_Object));
        }

        /// <summary>
        /// Tests that the <see cref="IGuidObject"/> branch of <see cref="Core.Create.UniqueReference(object?)"/> still
        /// resolves a serialized object to the same <see cref="GuidReference"/> as the live object, guarding the
        /// path that already worked against regressions from the added UniqueId branch.
        /// </summary>
        [Fact]
        public void UniqueReference_Create_GuidObject_MatchesJsonObject()
        {
            TestObject testObject = new("Beta");

            UniqueReference? uniqueReference_Object = Core.Create.UniqueReference(testObject);
            Assert.NotNull(uniqueReference_Object);

            JsonObject? jsonObject = testObject.ToJsonObject();
            Assert.NotNull(jsonObject);

            UniqueReference? uniqueReference_JsonObject = Core.Create.UniqueReference(jsonObject);
            Assert.NotNull(uniqueReference_JsonObject);

            Assert.IsType<GuidReference>(uniqueReference_Object);
            Assert.IsType<GuidReference>(uniqueReference_JsonObject);

            Assert.Equal(testObject.UniqueId, uniqueReference_Object.UniqueId);
            Assert.True(uniqueReference_Object.Equals(uniqueReference_JsonObject));
            Assert.True(uniqueReference_JsonObject.Equals(uniqueReference_Object));
        }

        /// <summary>
        /// Tests that <see cref="Core.Query.UniqueId(JsonObject?)"/> returns the object's own UniqueId for a
        /// serialized <see cref="IUniqueIdObject"/>, matching <see cref="Core.Query.UniqueId(IUniqueObject?)"/> for the
        /// live object. Previously the serialized form fell through to the whole-object hash, so the two disagreed.
        /// </summary>
        [Fact]
        public void UniqueId_JsonObject_MatchesUniqueIdObject()
        {
            TestUniqueIdObject testUniqueIdObject = new("Gamma");

            JsonObject? jsonObject = testUniqueIdObject.ToJsonObject();
            Assert.NotNull(jsonObject);

            Assert.Equal(testUniqueIdObject.UniqueId, Core.Query.UniqueId(jsonObject));
            Assert.Equal(Core.Query.UniqueId(testUniqueIdObject), Core.Query.UniqueId(jsonObject));
        }

        /// <summary>
        /// Tests that a JSON object carrying no recognisable identity still falls back to the whole-object hash
        /// rather than throwing, and that the fallback stays stable for equal content.
        /// </summary>
        [Fact]
        public void UniqueId_JsonObject_WithoutIdentity_FallsBackToHash()
        {
            JsonObject jsonObject_1 = new() { ["Value"] = 1 };
            JsonObject jsonObject_2 = new() { ["Value"] = 1 };

            string uniqueId_1 = Core.Query.UniqueId(jsonObject_1);
            string uniqueId_2 = Core.Query.UniqueId(jsonObject_2);

            Assert.False(string.IsNullOrWhiteSpace(uniqueId_1));
            Assert.Equal(uniqueId_1, uniqueId_2);
        }

        /// <summary>
        /// Tests that a reference built with the sparsest possible content still renders, hashes and compares without
        /// throwing.
        /// <para>Under the previous format this reference rendered to null, which propagated a NullReferenceException
        /// through GetHashCode, Equals and every hashed collection. The discriminator is now always emitted, so the
        /// rendered string is never null and the crash is gone by construction rather than by guard.</para>
        /// </summary>
        [Fact]
        public void SerializableReference_GetHashCode_NullToString_DoesNotThrow()
        {
            UniqueIdReference uniqueIdReference = new((TypeReference?)null, string.Empty);

            string? value = uniqueIdReference.ToString();
            Assert.NotNull(value);
            Assert.StartsWith(Constants.Reference.Kind.UniqueId, value);

            int hashCode = uniqueIdReference.GetHashCode();
            Assert.Equal(value!.GetHashCode(), hashCode);

            Assert.True(uniqueIdReference.Equals(uniqueIdReference));
            Assert.False(uniqueIdReference.Equals((IReference?)null));

            HashSet<SerializableReference> serializableReferences = [uniqueIdReference];
            Assert.Single(serializableReferences);
        }

        /// <summary>
        /// Tests that a non-null <see cref="TypeReference"/> whose FullTypeName is null does not compare equal to null.
        /// The operators previously compared only the FullTypeName, so such an instance reported itself as null and
        /// was silently discarded by null guards.
        /// </summary>
        [Fact]
        public void TypeReference_NullComparison()
        {
            TypeReference typeReference = new((string?)null);

            Assert.False(typeReference == null);
            Assert.True(typeReference != null);

            TypeReference? typeReference_Null = null;
            Assert.True(typeReference_Null == null);
            Assert.False(typeReference_Null != null);
        }

        /// <summary>
        /// Tests that <see cref="TypeReference"/> equality and the equality operators agree for equal and differing
        /// full type names.
        /// </summary>
        [Fact]
        public void TypeReference_Equality()
        {
            TypeReference typeReference_1 = new(typeof(TestObject));
            TypeReference typeReference_2 = new(typeof(TestObject));
            TypeReference typeReference_3 = new(typeof(TestUniqueIdObject));

            Assert.True(typeReference_1 == typeReference_2);
            Assert.True(typeReference_1.Equals(typeReference_2));
            Assert.Equal(typeReference_1.GetHashCode(), typeReference_2.GetHashCode());

            Assert.True(typeReference_1 != typeReference_3);
            Assert.False(typeReference_1.Equals(typeReference_3));
        }

        /// <summary>
        /// Tests that reference equality is symmetric across different reference kinds.
        /// <para><see cref="SerializableReference.Equals(IReference?)"/> once compared hash codes only, while
        /// <see cref="TypeReference.Equals(object?)"/> additionally required a matching type, so the two directions
        /// disagreed and violated the equality contract.</para>
        /// <para>These two inputs are the pair that used to collide: a type reference whose name was literally
        /// <c>"X"</c> rendered identically to a unique identifier reference for <c>X</c>, because the old format
        /// encoded the reference kind by shape. The discriminator now distinguishes them, so the strings must differ
        /// as well as the instances.</para>
        /// </summary>
        [Fact]
        public void SerializableReference_Equals_IsSymmetric()
        {
            TypeReference typeReference = new("\"X\"");
            UniqueIdReference uniqueIdReference = new((TypeReference?)null, "X");

            Assert.NotEqual(typeReference.ToString(), uniqueIdReference.ToString());

            Assert.False(uniqueIdReference.Equals((IReference)typeReference));
            Assert.False(typeReference.Equals((object)uniqueIdReference));
        }

        /// <summary>
        /// Tests that two references of the same kind and state are equal, that differing state is not equal, and that
        /// a clone equals its original, for both <see cref="GuidReference"/> and <see cref="UniqueIdReference"/>.
        /// </summary>
        [Fact]
        public void UniqueReference_Equality()
        {
            TypeReference typeReference = new(typeof(TestObject));
            Guid guid = Guid.NewGuid();

            GuidReference guidReference_1 = new(typeReference, guid);
            GuidReference guidReference_2 = new(typeReference, guid);
            GuidReference guidReference_3 = new(typeReference, Guid.NewGuid());

            Assert.True(guidReference_1 == guidReference_2);
            Assert.True(guidReference_1.Equals(guidReference_2));
            Assert.True(guidReference_2.Equals(guidReference_1));
            Assert.True(guidReference_1 != guidReference_3);

            Assert.True(guidReference_1.Equals(guidReference_1.Clone() as IReference));

            UniqueIdReference uniqueIdReference_1 = new(typeReference, "Id_1");
            UniqueIdReference uniqueIdReference_2 = new(typeReference, "Id_1");
            UniqueIdReference uniqueIdReference_3 = new(typeReference, "Id_2");

            Assert.True(uniqueIdReference_1.Equals(uniqueIdReference_2));
            Assert.False(uniqueIdReference_1.Equals(uniqueIdReference_3));

            Assert.False(guidReference_1.Equals((IReference)uniqueIdReference_1));
            Assert.False(uniqueIdReference_1.Equals((IReference)guidReference_1));
        }

        /// <summary>
        /// Tests that a reference compares correctly against null through the equality operators, including the
        /// object-typed operator overloads, without throwing.
        /// </summary>
        [Fact]
        public void SerializableReference_NullComparison()
        {
            GuidReference guidReference = new(new TypeReference(typeof(TestObject)), Guid.NewGuid());

            Assert.False(guidReference == null);
            Assert.True(guidReference != null);
            Assert.False(guidReference.Equals(null));

            GuidReference? guidReference_Null = null;
            Assert.True(guidReference_Null == null);
            Assert.False(guidReference_Null != null);
        }

        /// <summary>
        /// Tests that <see cref="GuidReference"/> and <see cref="UniqueIdReference"/> survive a JSON round trip and a
        /// clone with their type reference and identity intact.
        /// </summary>
        [Fact]
        public void UniqueReference_Serialization()
        {
            TypeReference typeReference = new(typeof(TestObject));

            GuidReference guidReference = new(typeReference, Guid.NewGuid());
            Query.SerializationCheck(guidReference);

            UniqueIdReference uniqueIdReference = new(typeReference, "TestUniqueIdObject_Delta");
            Query.SerializationCheck(uniqueIdReference);

            TestUniqueIdObject testUniqueIdObject = new("Delta");
            Query.SerializationCheck(testUniqueIdObject);
        }
    }
}
