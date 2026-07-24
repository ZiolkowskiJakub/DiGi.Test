using DiGi.Core.Classes;
using DiGi.Core.Interfaces;

namespace DiGi.PostgreSQL.PartitionUniqueReference.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a partition unique reference round-trips with a nested GUID reference, resolved by DiGi.Core
        /// through the reflective factory lookup.
        /// </summary>
        [Fact]
        public void PartitionUniqueReference_RoundTrip_GuidReference()
        {
            TypeReference typeReference = new("DiGi.GIS.Classes.Building2D,DiGi.GIS");
            GuidReference guidReference = new(typeReference, Guid.NewGuid());

            PartitionUniqueReference.Classes.PartitionUniqueReference partitionUniqueReference = new("building2d", guidReference);

            Assert.True(Core.Query.TryParse(partitionUniqueReference.ToString(), out IReference? reference));
            Assert.IsType<PartitionUniqueReference.Classes.PartitionUniqueReference>(reference);
            Assert.Equal(partitionUniqueReference.ToString(), reference.ToString());
            Assert.True(partitionUniqueReference.Equals(reference));
        }

        /// <summary>
        /// Tests that the nested reference may be any unique reference, here a unique identifier reference, proving
        /// the nested slot resolves back through DiGi.Core rather than assuming a concrete type.
        /// </summary>
        [Fact]
        public void PartitionUniqueReference_RoundTrip_UniqueIdReference()
        {
            TypeReference typeReference = new("DiGi.GIS.Classes.Building2D,DiGi.GIS");
            UniqueIdReference uniqueIdReference = new(typeReference, "BLD-001");

            PartitionUniqueReference.Classes.PartitionUniqueReference partitionUniqueReference = new("building2d", uniqueIdReference);

            Assert.True(Core.Query.TryParse(partitionUniqueReference.ToString(), out PartitionUniqueReference.Classes.PartitionUniqueReference? partitionUniqueReference_Parsed));
            Assert.NotNull(partitionUniqueReference_Parsed);
            Assert.Equal("building2d", partitionUniqueReference_Parsed.Name);
            Assert.IsType<UniqueIdReference>(partitionUniqueReference_Parsed.UniqueReference);
            Assert.Equal("BLD-001", partitionUniqueReference_Parsed.UniqueReference?.UniqueId);
        }

        /// <summary>
        /// Tests that a partition unique reference with no nested reference renders and round-trips rather than
        /// collapsing to null, and stays distinguishable from a populated one.
        /// </summary>
        [Fact]
        public void PartitionUniqueReference_NullUniqueReference()
        {
            PartitionUniqueReference.Classes.PartitionUniqueReference partitionUniqueReference_1 = new("building2d", null);
            PartitionUniqueReference.Classes.PartitionUniqueReference partitionUniqueReference_2 = new("building2d", new UniqueIdReference(new TypeReference("DiGi.GIS.Classes.Building2D,DiGi.GIS"), "BLD-001"));

            Assert.NotNull(partitionUniqueReference_1.ToString());
            Assert.NotEqual(partitionUniqueReference_1.ToString(), partitionUniqueReference_2.ToString());

            Assert.True(Core.Query.TryParse(partitionUniqueReference_1.ToString(), out PartitionUniqueReference.Classes.PartitionUniqueReference? partitionUniqueReference_1_Parsed));
            Assert.NotNull(partitionUniqueReference_1_Parsed);
            Assert.Null(partitionUniqueReference_1_Parsed.UniqueReference);
        }
    }
}
