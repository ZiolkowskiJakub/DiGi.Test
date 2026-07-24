using DiGi.Core.Interfaces;

namespace DiGi.PostgreSQL.PartitionReference.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a partition reference survives a string round trip, resolved by DiGi.Core through the
        /// reflective factory lookup even though DiGi.Core holds no reference to this assembly.
        /// </summary>
        [Fact]
        public void PartitionReference_RoundTrip()
        {
            PartitionReference.Classes.PartitionReference partitionReference = new("building2d", Guid.NewGuid().ToString("N"));

            Assert.True(Core.Query.TryParse(partitionReference.ToString(), out IReference? reference));
            Assert.IsType<PartitionReference.Classes.PartitionReference>(reference);
            Assert.Equal(partitionReference.ToString(), reference.ToString());
            Assert.True(partitionReference.Equals(reference));
        }

        /// <summary>
        /// Tests that a partition name containing the arrow survives a round trip. The previous form used
        /// <c>name-&gt;uniqueId</c>, so this exact name would have split incorrectly.
        /// </summary>
        [Fact]
        public void PartitionReference_NameContainingArrow()
        {
            PartitionReference.Classes.PartitionReference partitionReference = new("a->b", "id->1");

            Assert.True(Core.Query.TryParse(partitionReference.ToString(), out PartitionReference.Classes.PartitionReference? partitionReference_Parsed));
            Assert.NotNull(partitionReference_Parsed);
            Assert.Equal("a->b", partitionReference_Parsed.Name);
            Assert.Equal("id->1", partitionReference_Parsed.UniqueId);
        }

        /// <summary>
        /// Tests that a blank partition reference renders and round-trips rather than collapsing to null.
        /// <para>The previous ToString returned null when a field was blank, which made every blank instance compare
        /// equal through the hash-of-null path.</para>
        /// </summary>
        [Fact]
        public void PartitionReference_BlankFields()
        {
            PartitionReference.Classes.PartitionReference partitionReference_1 = new(null, null);
            PartitionReference.Classes.PartitionReference partitionReference_2 = new("a", null);

            Assert.NotNull(partitionReference_1.ToString());
            Assert.NotEqual(partitionReference_1.ToString(), partitionReference_2.ToString());

            Assert.True(Core.Query.TryParse(partitionReference_1.ToString(), out PartitionReference.Classes.PartitionReference? partitionReference_1_Parsed));
            Assert.NotNull(partitionReference_1_Parsed);
            Assert.Null(partitionReference_1_Parsed.Name);
        }
    }
}
