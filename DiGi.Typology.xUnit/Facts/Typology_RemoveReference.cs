using DiGi.Typology.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that RemoveReference reverses AddReference and that the copy-on-read References
        /// property reflects the change, closing the asymmetry where references could be added but
        /// never removed.
        /// </summary>
        [Fact]
        public void Typology_RemoveReference()
        {
            Typology.Classes.Typology typology = new("Root", "Description");

            Assert.True(typology.AddReference("reference_1"));
            Assert.True(typology.Contains("reference_1"));
            Assert.Contains("reference_1", typology.References);

            Assert.True(typology.RemoveReference("reference_1"));
            Assert.False(typology.Contains("reference_1"));
            Assert.DoesNotContain("reference_1", typology.References);

            Assert.False(typology.RemoveReference("reference_1"));
            Assert.False(typology.RemoveReference(null));
        }
    }
}
