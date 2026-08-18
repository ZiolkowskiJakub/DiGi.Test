using DiGi.GIS.PostgreSQL.Classes;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the counts a migration reports for one county row are exposed as given and survive a JSON round trip and a clone.
        /// <para>The four classes are mutually exclusive and cover every row, so they add up to the total. That is what makes the result readable as a verdict rather than as five loose numbers, and a run whose classes did not add up would mean rows were counted twice or not at all.</para>
        /// </summary>
        [Fact]
        public void UniqueIdMigrationResult_Serialization()
        {
            UniqueIdMigrationResult uniqueIdMigrationResult = new(5, 120, 40, 74, 4, 2);

            Assert.Equal(5, uniqueIdMigrationResult.CountyId);
            Assert.Equal(120, uniqueIdMigrationResult.Total);
            Assert.Equal(40, uniqueIdMigrationResult.Done);
            Assert.Equal(74, uniqueIdMigrationResult.Pending);
            Assert.Equal(4, uniqueIdMigrationResult.Blocked);
            Assert.Equal(2, uniqueIdMigrationResult.Missing);

            Assert.Equal(uniqueIdMigrationResult.Total, uniqueIdMigrationResult.Done + uniqueIdMigrationResult.Pending + uniqueIdMigrationResult.Blocked + uniqueIdMigrationResult.Missing);

            Core.xUnit.Query.SerializationCheck(uniqueIdMigrationResult);

            UniqueIdMigrationResult uniqueIdMigrationResult_Clone = new(uniqueIdMigrationResult);

            Assert.Equal(uniqueIdMigrationResult.CountyId, uniqueIdMigrationResult_Clone.CountyId);
            Assert.Equal(uniqueIdMigrationResult.Total, uniqueIdMigrationResult_Clone.Total);
            Assert.Equal(uniqueIdMigrationResult.Done, uniqueIdMigrationResult_Clone.Done);
            Assert.Equal(uniqueIdMigrationResult.Pending, uniqueIdMigrationResult_Clone.Pending);
            Assert.Equal(uniqueIdMigrationResult.Blocked, uniqueIdMigrationResult_Clone.Blocked);
            Assert.Equal(uniqueIdMigrationResult.Missing, uniqueIdMigrationResult_Clone.Missing);
        }

        /// <summary>
        /// Verifies that a county row holding nothing reports zeros rather than failing.
        /// <para>Most of the 406 county rows a national pass visits hold no building models at all, so this is the ordinary case rather than an edge one.</para>
        /// </summary>
        [Fact]
        public void UniqueIdMigrationResult_Empty()
        {
            UniqueIdMigrationResult uniqueIdMigrationResult = new(204, 0, 0, 0, 0, 0);

            Assert.Equal(204, uniqueIdMigrationResult.CountyId);
            Assert.Equal(0, uniqueIdMigrationResult.Total);
            Assert.Equal(0, uniqueIdMigrationResult.Pending);

            Core.xUnit.Query.SerializationCheck(uniqueIdMigrationResult);
        }
    }
}
