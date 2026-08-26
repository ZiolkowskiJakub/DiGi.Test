using DiGi.GIS.PostgreSQL.Classes;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the <see cref="PostgreSQLBuilding2DRefreshResult"/> constructor carries every tally through, and that a populated instance survives a JSON round trip and a clone.
        /// </summary>
        [Fact]
        public void PostgreSQLBuilding2DRefreshResult_Serialization()
        {
            PostgreSQLBuilding2DRefreshResult postgreSQLBuilding2DRefreshResult = new(15_700_000, 980_007, 3, 12_345_678, true);

            Assert.Equal(15_700_000, postgreSQLBuilding2DRefreshResult.ReadCount);
            Assert.Equal(980_007, postgreSQLBuilding2DRefreshResult.UpdatedCount);
            Assert.Equal(3, postgreSQLBuilding2DRefreshResult.FailedBatchCount);
            Assert.Equal(12_345_678, postgreSQLBuilding2DRefreshResult.LastProcessedId);
            Assert.True(postgreSQLBuilding2DRefreshResult.Cancelled);

            string? json = Core.Convert.ToSystem_String(postgreSQLBuilding2DRefreshResult);
            Assert.NotNull(json);

            PostgreSQLBuilding2DRefreshResult? postgreSQLBuilding2DRefreshResult_Json = Core.Convert.ToDiGi<PostgreSQLBuilding2DRefreshResult>(json)?.FirstOrDefault();
            Assert.NotNull(postgreSQLBuilding2DRefreshResult_Json);

            Assert.Equal(15_700_000, postgreSQLBuilding2DRefreshResult_Json.ReadCount);
            Assert.Equal(980_007, postgreSQLBuilding2DRefreshResult_Json.UpdatedCount);
            Assert.Equal(3, postgreSQLBuilding2DRefreshResult_Json.FailedBatchCount);
            Assert.Equal(12_345_678, postgreSQLBuilding2DRefreshResult_Json.LastProcessedId);
            Assert.True(postgreSQLBuilding2DRefreshResult_Json.Cancelled);

            PostgreSQLBuilding2DRefreshResult postgreSQLBuilding2DRefreshResult_Clone = new(postgreSQLBuilding2DRefreshResult);

            Assert.Equal(15_700_000, postgreSQLBuilding2DRefreshResult_Clone.ReadCount);
            Assert.Equal(980_007, postgreSQLBuilding2DRefreshResult_Clone.UpdatedCount);
            Assert.Equal(3, postgreSQLBuilding2DRefreshResult_Clone.FailedBatchCount);
            Assert.Equal(12_345_678, postgreSQLBuilding2DRefreshResult_Clone.LastProcessedId);
            Assert.True(postgreSQLBuilding2DRefreshResult_Clone.Cancelled);

            Core.xUnit.Query.SerializationCheck(postgreSQLBuilding2DRefreshResult);
        }

        /// <summary>
        /// Verifies that a completed building refresh run reports neither a cancellation nor a failure.
        /// </summary>
        [Fact]
        public void PostgreSQLBuilding2DRefreshResult_Complete()
        {
            PostgreSQLBuilding2DRefreshResult postgreSQLBuilding2DRefreshResult = new(33_687, 5_432, 0, 99_999, false);

            Assert.Equal(0, postgreSQLBuilding2DRefreshResult.FailedBatchCount);
            Assert.False(postgreSQLBuilding2DRefreshResult.Cancelled);
            Assert.Equal(33_687, postgreSQLBuilding2DRefreshResult.ReadCount);
            Assert.Equal(5_432, postgreSQLBuilding2DRefreshResult.UpdatedCount);
            Assert.Equal(99_999, postgreSQLBuilding2DRefreshResult.LastProcessedId);

            Core.xUnit.Query.SerializationCheck(postgreSQLBuilding2DRefreshResult);
        }
    }
}
