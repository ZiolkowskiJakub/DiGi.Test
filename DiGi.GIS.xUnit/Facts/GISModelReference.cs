using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using System;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a GIS model areal 2D reference survives a string round trip, resolved by DiGi.Core even though
        /// DiGi.Core holds no reference to DiGi.GIS - the factory is found reflectively.
        /// </summary>
        [Fact]
        public void GISModelAreal2DReference_RoundTrip()
        {
            Classes.GISModelAreal2DReference gISModelAreal2DReference = new("SomeModel", "SomeAreal");

            Assert.True(Core.Query.TryParse(gISModelAreal2DReference.ToString(), out IReference? reference));
            Assert.IsType<Classes.GISModelAreal2DReference>(reference);
            Assert.Equal(gISModelAreal2DReference.ToString(), reference.ToString());
            Assert.True(gISModelAreal2DReference.Equals(reference));
        }

        /// <summary>
        /// Tests that an areal reference containing brackets round-trips. The previous
        /// <c>[gISModelReference]areal2DReference</c> form escaped nothing, so this exact payload could not survive it.
        /// </summary>
        [Fact]
        public void GISModelAreal2DReference_BracketsInPayload()
        {
            Classes.GISModelAreal2DReference gISModelAreal2DReference = new("[X]Y", "[A]B::C");

            Assert.True(Core.Query.TryParse(gISModelAreal2DReference.ToString(), out Classes.GISModelAreal2DReference? gISModelAreal2DReference_Parsed));
            Assert.NotNull(gISModelAreal2DReference_Parsed);
            Assert.Equal("[X]Y", gISModelAreal2DReference_Parsed.GISModelReference);
            Assert.Equal("[A]B::C", gISModelAreal2DReference_Parsed.Areal2DReference);
        }

        /// <summary>
        /// Tests that a null GIS model reference stays distinguishable from an empty one across a round trip.
        /// </summary>
        [Fact]
        public void GISModelAreal2DReference_NullGISModelReference()
        {
            Classes.GISModelAreal2DReference gISModelAreal2DReference = new(null, "SomeAreal");

            Assert.True(Core.Query.TryParse(gISModelAreal2DReference.ToString(), out Classes.GISModelAreal2DReference? gISModelAreal2DReference_Parsed));
            Assert.NotNull(gISModelAreal2DReference_Parsed);
            Assert.Null(gISModelAreal2DReference_Parsed.GISModelReference);
            Assert.Equal("SomeAreal", gISModelAreal2DReference_Parsed.Areal2DReference);
        }

        /// <summary>
        /// Tests that a GIS model file GUID object reference survives a string round trip. It previously had no
        /// ToString at all, so it could not be rendered, let alone parsed.
        /// </summary>
        [Fact]
        public void GISModelFileGuidObjectReference_RoundTrip()
        {
            Classes.GISModelFileGuidObjectReference gISModelFileGuidObjectReference = GISModelFileGuidObjectReference(Guid.NewGuid(), Guid.NewGuid());

            Assert.True(Core.Query.TryParse(gISModelFileGuidObjectReference.ToString(), out Classes.GISModelFileGuidObjectReference? reference_Parsed));
            Assert.NotNull(reference_Parsed);
            Assert.Equal(gISModelFileGuidObjectReference.ToString(), reference_Parsed.ToString());
            Assert.True(gISModelFileGuidObjectReference.Equals(reference_Parsed));
        }

        /// <summary>
        /// Tests that two GIS model file GUID object references holding different GUIDs are not equal.
        /// <para>Regression test. This type declared no ToString, so it rendered as its type name; because equality
        /// and hashing are built on the rendered string, EVERY instance compared equal to every other regardless of
        /// its GUIDs.</para>
        /// </summary>
        [Fact]
        public void GISModelFileGuidObjectReference_DistinctInstances_AreNotEqual()
        {
            Guid guid_1 = Guid.NewGuid();
            Guid guid_2 = Guid.NewGuid();

            Classes.GISModelFileGuidObjectReference gISModelFileGuidObjectReference_1 = GISModelFileGuidObjectReference(guid_1, guid_1);
            Classes.GISModelFileGuidObjectReference gISModelFileGuidObjectReference_2 = GISModelFileGuidObjectReference(guid_2, guid_2);
            Classes.GISModelFileGuidObjectReference gISModelFileGuidObjectReference_3 = GISModelFileGuidObjectReference(guid_1, guid_1);

            Assert.False(gISModelFileGuidObjectReference_1.Equals(gISModelFileGuidObjectReference_2));
            Assert.NotEqual(gISModelFileGuidObjectReference_1.GetHashCode(), gISModelFileGuidObjectReference_2.GetHashCode());

            Assert.True(gISModelFileGuidObjectReference_1.Equals(gISModelFileGuidObjectReference_3));
        }

        /// <summary>
        /// Tests that a GIS model file GUID object reference survives JSON serialization and cloning. It previously
        /// declared no serialization members at all, so it did not round-trip.
        /// </summary>
        [Fact]
        public void GISModelFileGuidObjectReference_SerializationCheck()
        {
            Classes.GISModelFileGuidObjectReference gISModelFileGuidObjectReference = GISModelFileGuidObjectReference(Guid.NewGuid(), Guid.NewGuid());

            Core.xUnit.Query.SerializationCheck(gISModelFileGuidObjectReference);

            Classes.GISModelFileGuidObjectReference? gISModelFileGuidObjectReference_Clone = gISModelFileGuidObjectReference.Clone() as Classes.GISModelFileGuidObjectReference;
            Assert.NotNull(gISModelFileGuidObjectReference_Clone);
            Assert.Equal(gISModelFileGuidObjectReference.ToString(), gISModelFileGuidObjectReference_Clone.ToString());
            Assert.NotNull(gISModelFileGuidObjectReference_Clone.GuidReference);
            Assert.NotNull(gISModelFileGuidObjectReference_Clone.GuidExternalReference);
        }

        /// <summary>
        /// Builds a GIS model file GUID object reference from the two GUIDs it addresses.
        /// </summary>
        /// <param name="guid_File">The GUID of the GIS model file.</param>
        /// <param name="guid_Object">The GUID of the object within that file.</param>
        /// <returns>The reference.</returns>
        private static Classes.GISModelFileGuidObjectReference GISModelFileGuidObjectReference(Guid guid_File, Guid guid_Object)
        {
            TypeReference typeReference = new("DiGi.GIS.Classes.Building2D,DiGi.GIS");

            return new Classes.GISModelFileGuidObjectReference(
                new GuidExternalReference("Revit", new GuidReference(typeReference, guid_File)),
                new GuidReference(typeReference, guid_Object));
        }
    }
}
