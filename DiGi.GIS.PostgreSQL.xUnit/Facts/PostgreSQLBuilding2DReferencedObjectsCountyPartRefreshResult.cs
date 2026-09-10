using DiGi.GIS.PostgreSQL.Classes;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the constructor carries every tally through, and that a populated instance survives a JSON round trip and a clone.
        /// <para><c>MovableReferenceCount</c> is what the probe found and <c>MovedReferenceCount</c> what was written: a dry run reports the first and leaves the second at zero, so a round trip or a copy that conflated the two would misstate what a run did.</para>
        /// <para>A run steps over a county it cannot reach rather than ending, so <c>FailedCodeCount</c> and <c>Cancelled</c> are part of the verdict, not decoration.</para>
        /// </summary>
        [Fact]
        public void PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Serialization()
        {
            PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult = new(3, 7, 120_000, 11, 8, 3, 8, 1, false);

            Assert.Equal(3, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult.CodeCount);
            Assert.Equal(7, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult.PartCount);
            Assert.Equal(120_000, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult.ReferenceCount);
            Assert.Equal(11, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult.StrayReferenceCount);
            Assert.Equal(8, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult.MovableReferenceCount);
            Assert.Equal(3, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult.BlockedReferenceCount);
            Assert.Equal(8, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult.MovedReferenceCount);
            Assert.Equal(1, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult.FailedCodeCount);
            Assert.False(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult.Cancelled);

            string? text = Core.Convert.ToSystem_String(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult);
            Assert.False(string.IsNullOrWhiteSpace(text));

            PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult? postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Json = Core.Convert.ToDiGi<PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult>(text)?.FirstOrDefault();
            Assert.NotNull(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Json);

            Assert.Equal(3, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Json.CodeCount);
            Assert.Equal(7, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Json.PartCount);
            Assert.Equal(120_000, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Json.ReferenceCount);
            Assert.Equal(11, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Json.StrayReferenceCount);
            Assert.Equal(8, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Json.MovableReferenceCount);
            Assert.Equal(3, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Json.BlockedReferenceCount);
            Assert.Equal(8, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Json.MovedReferenceCount);
            Assert.Equal(1, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Json.FailedCodeCount);
            Assert.False(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Json.Cancelled);

            PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Clone = new(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult);

            Assert.Equal(3, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Clone.CodeCount);
            Assert.Equal(7, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Clone.PartCount);
            Assert.Equal(120_000, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Clone.ReferenceCount);
            Assert.Equal(11, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Clone.StrayReferenceCount);
            Assert.Equal(8, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Clone.MovableReferenceCount);
            Assert.Equal(3, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Clone.BlockedReferenceCount);
            Assert.Equal(8, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Clone.MovedReferenceCount);
            Assert.Equal(1, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Clone.FailedCodeCount);
            Assert.False(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_Clone.Cancelled);

            Core.xUnit.Query.SerializationCheck(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult);
        }

        /// <summary>
        /// Verifies that the shape of a completed dry run reads the way the tray row promises it: nothing moved, and the blocked tally is what the probe reported.
        /// </summary>
        [Fact]
        public void PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult_DryRun()
        {
            PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult = new(2, 5, 40_000, 4, 4, 0, 0, 0, false);

            Assert.Equal(0, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult.MovedReferenceCount);
            Assert.Equal(0, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult.BlockedReferenceCount);
            Assert.Equal(4, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult.MovableReferenceCount);

            Core.xUnit.Query.SerializationCheck(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult);
        }
    }
}
