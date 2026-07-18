using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using System;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that reference strings written before the discriminator was introduced still parse.
        /// <para>These strings are ZIP entry names inside pre-existing storage archives. Without this, an old archive
        /// would read back empty and silently, because a missing entry is not an error.</para>
        /// <para>When Query/TryParseLegacy.cs is deleted, this fact goes with it.</para>
        /// </summary>
        [Fact]
        public void TryParseLegacy_ArchiveTypes_StillParse()
        {
            Assert.True(Core.Query.TryParse("DiGi.GIS.Classes.Building2D,DiGi.GIS::0f8fad5bd9cb469fa16570867728950e", out IReference? reference_Guid));
            GuidReference? guidReference = Assert.IsType<GuidReference>(reference_Guid);
            Assert.Equal(Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e"), guidReference.Guid);
            Assert.Equal("DiGi.GIS.Classes.Building2D,DiGi.GIS", guidReference.TypeReference?.FullTypeName);

            Assert.True(Core.Query.TryParse("DiGi.GIS.Classes.Building2D,DiGi.GIS::\"BLD-001\"", out IReference? reference_UniqueId));
            UniqueIdReference? uniqueIdReference = Assert.IsType<UniqueIdReference>(reference_UniqueId);
            Assert.Equal("BLD-001", uniqueIdReference.UniqueId);

            Assert.True(Core.Query.TryParse("DiGi.GIS.Classes.Building2D,DiGi.GIS", out IReference? reference_Type));
            TypeReference? typeReference = Assert.IsType<TypeReference>(reference_Type);
            Assert.Equal("DiGi.GIS.Classes.Building2D,DiGi.GIS", typeReference.FullTypeName);
        }

        /// <summary>
        /// Tests that a legacy string is re-rendered in the current format, so anything read from an old archive is
        /// written back with a discriminator rather than perpetuating the old form.
        /// </summary>
        [Fact]
        public void TryParseLegacy_ReRendersInCurrentFormat()
        {
            const string value_Legacy = "DiGi.GIS.Classes.Building2D,DiGi.GIS::0f8fad5bd9cb469fa16570867728950e";

            Assert.True(Core.Query.TryParse(value_Legacy, out IReference? reference));
            Assert.NotNull(reference);

            Assert.NotEqual(value_Legacy, reference.ToString());
            Assert.StartsWith(Constants.Reference.Kind.Guid + Constants.Reference.Separator, reference.ToString());

            // And the re-rendered form is itself stable.
            Assert.True(Core.Query.TryParse(reference.ToString(), out IReference? reference_Current));
            Assert.Equal(reference.ToString(), reference_Current?.ToString());
        }

        /// <summary>
        /// Tests that the current format wins over the legacy reading of the same string.
        /// <para>A legacy string opens with its target's full type name, which is shape-identical to the full type
        /// name discriminator. The two are separated by resolution - a discriminator has to name a reference type
        /// that has a factory - so a string naming a real reference type must be read as current, not legacy.</para>
        /// </summary>
        [Fact]
        public void TryParseLegacy_DoesNotShadowCurrentFormat()
        {
            TypeReference typeReference = new(typeof(TestObject));

            foreach (IReference reference in References_Core())
            {
                Assert.True(Core.Query.TryParse(reference.ToString(), out IReference? reference_Parsed));
                Assert.Equal(reference.GetType(), reference_Parsed?.GetType());
            }

            // A legacy string whose target happens to be a reference class is the one genuinely ambiguous case: it
            // resolves as a discriminator and is read as current. Documented, and not reachable from real data,
            // because references address domain objects rather than other references.
            string? fullTypeName = Core.Query.FullTypeName(typeof(TypeReference));
            string value_Ambiguous = string.Format("{0}::0f8fad5bd9cb469fa16570867728950e", fullTypeName);

            Assert.True(Core.Query.TryParse(value_Ambiguous, out IReference? reference_Ambiguous));
            Assert.IsType<TypeReference>(reference_Ambiguous);
        }
    }
}
