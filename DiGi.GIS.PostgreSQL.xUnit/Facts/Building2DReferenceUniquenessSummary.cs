using DiGi.GIS.PostgreSQL.Classes;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Building2DReferenceUniquenessSummary"/> properties are correctly initialized and survive serialization and copying.
        /// </summary>
        [Fact]
        public void Building2DReferenceUniquenessSummary_Serialization()
        {
            long totalCount = 1000000;
            long distinctReferenceCount = 999950;
            long duplicateReferenceCount = 50;
            bool isUnique = false;

            Building2DReferenceUniquenessSummary building2DReferenceUniquenessSummary = new(totalCount, distinctReferenceCount, duplicateReferenceCount, isUnique);

            Assert.Equal(totalCount, building2DReferenceUniquenessSummary.TotalCount);
            Assert.Equal(distinctReferenceCount, building2DReferenceUniquenessSummary.DistinctReferenceCount);
            Assert.Equal(duplicateReferenceCount, building2DReferenceUniquenessSummary.DuplicateReferenceCount);
            Assert.False(building2DReferenceUniquenessSummary.IsUnique);

            Core.xUnit.Query.SerializationCheck(building2DReferenceUniquenessSummary);

            Building2DReferenceUniquenessSummary building2DReferenceUniquenessSummary_Clone = new(building2DReferenceUniquenessSummary);

            Assert.Equal(building2DReferenceUniquenessSummary.TotalCount, building2DReferenceUniquenessSummary_Clone.TotalCount);
            Assert.Equal(building2DReferenceUniquenessSummary.DistinctReferenceCount, building2DReferenceUniquenessSummary_Clone.DistinctReferenceCount);
            Assert.Equal(building2DReferenceUniquenessSummary.DuplicateReferenceCount, building2DReferenceUniquenessSummary_Clone.DuplicateReferenceCount);
            Assert.Equal(building2DReferenceUniquenessSummary.IsUnique, building2DReferenceUniquenessSummary_Clone.IsUnique);
        }
    }
}
