using DiGi.GIS.PostgreSQL.Classes;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the constructor carries every tally through, and that a populated instance survives a JSON round trip and a clone.
        /// <para>The tallies are the whole point of the type. A run steps over a county it cannot reach rather than ending, so a result that reported a completed run without <c>FailedCountyCount</c> would say nothing about whether the run did what it set out to do.</para>
        /// </summary>
        [Fact]
        public void PostgreSQLOrtoDatasRefreshResult_Serialization()
        {
            PostgreSQLOrtoDatasRefreshResult postgreSQLOrtoDatasRefreshResult = new(406, 15_700_000, 1_204_331, 980_007, 3, true);

            Assert.Equal(406, postgreSQLOrtoDatasRefreshResult.CountyCount);
            Assert.Equal(15_700_000, postgreSQLOrtoDatasRefreshResult.ReadCount);
            Assert.Equal(1_204_331, postgreSQLOrtoDatasRefreshResult.EnqueuedCount);
            Assert.Equal(980_007, postgreSQLOrtoDatasRefreshResult.SubdivisionIdCount);
            Assert.Equal(3, postgreSQLOrtoDatasRefreshResult.FailedCountyCount);
            Assert.True(postgreSQLOrtoDatasRefreshResult.Cancelled);

            string? json = Core.Convert.ToSystem_String(postgreSQLOrtoDatasRefreshResult);
            Assert.NotNull(json);

            PostgreSQLOrtoDatasRefreshResult? postgreSQLOrtoDatasRefreshResult_Json = Core.Convert.ToDiGi<PostgreSQLOrtoDatasRefreshResult>(json)?.FirstOrDefault();
            Assert.NotNull(postgreSQLOrtoDatasRefreshResult_Json);

            Assert.Equal(406, postgreSQLOrtoDatasRefreshResult_Json.CountyCount);
            Assert.Equal(15_700_000, postgreSQLOrtoDatasRefreshResult_Json.ReadCount);
            Assert.Equal(1_204_331, postgreSQLOrtoDatasRefreshResult_Json.EnqueuedCount);
            Assert.Equal(980_007, postgreSQLOrtoDatasRefreshResult_Json.SubdivisionIdCount);
            Assert.Equal(3, postgreSQLOrtoDatasRefreshResult_Json.FailedCountyCount);
            Assert.True(postgreSQLOrtoDatasRefreshResult_Json.Cancelled);

            PostgreSQLOrtoDatasRefreshResult postgreSQLOrtoDatasRefreshResult_Clone = new(postgreSQLOrtoDatasRefreshResult);

            Assert.Equal(406, postgreSQLOrtoDatasRefreshResult_Clone.CountyCount);
            Assert.Equal(15_700_000, postgreSQLOrtoDatasRefreshResult_Clone.ReadCount);
            Assert.Equal(1_204_331, postgreSQLOrtoDatasRefreshResult_Clone.EnqueuedCount);
            Assert.Equal(980_007, postgreSQLOrtoDatasRefreshResult_Clone.SubdivisionIdCount);
            Assert.Equal(3, postgreSQLOrtoDatasRefreshResult_Clone.FailedCountyCount);
            Assert.True(postgreSQLOrtoDatasRefreshResult_Clone.Cancelled);

            Core.xUnit.Query.SerializationCheck(postgreSQLOrtoDatasRefreshResult);
        }

        /// <summary>
        /// Verifies that a run which reached the end of its counties reports neither a cancellation nor a failure.
        /// </summary>
        [Fact]
        public void PostgreSQLOrtoDatasRefreshResult_Complete()
        {
            PostgreSQLOrtoDatasRefreshResult postgreSQLOrtoDatasRefreshResult = new(1, 33_687, 0, 0, 0, false);

            Assert.Equal(0, postgreSQLOrtoDatasRefreshResult.FailedCountyCount);
            Assert.False(postgreSQLOrtoDatasRefreshResult.Cancelled);

            // Nothing queued is a perfectly ordinary outcome: it means the county was already complete.
            Assert.Equal(0, postgreSQLOrtoDatasRefreshResult.EnqueuedCount);
            Assert.Equal(33_687, postgreSQLOrtoDatasRefreshResult.ReadCount);

            Core.xUnit.Query.SerializationCheck(postgreSQLOrtoDatasRefreshResult);
        }
    }
}
