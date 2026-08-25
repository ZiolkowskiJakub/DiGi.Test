using DiGi.GIS.PostgreSQL.Classes;
using System;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the constructor carries every tally through and that a populated instance survives a JSON round trip and a clone.
        /// </summary>
        [Fact]
        public void OrtoDatasQueueResult_Serialization()
        {
            DateTimeOffset dateTimeOffset_First = new(2026, 8, 24, 6, 0, 0, TimeSpan.FromHours(2));
            DateTimeOffset dateTimeOffset_Last = new(2026, 8, 25, 11, 45, 0, TimeSpan.FromHours(2));

            OrtoDatasQueueResult ortoDatasQueueResult = new(55417, 12_683, 9_412, dateTimeOffset_First, dateTimeOffset_Last);

            Assert.Equal(55417, ortoDatasQueueResult.CountyId);
            Assert.Equal(12_683, ortoDatasQueueResult.Count);
            Assert.Equal(9_412, ortoDatasQueueResult.WithSubdivisionIdCount);
            Assert.Equal(3_271, ortoDatasQueueResult.WithoutSubdivisionIdCount);
            Assert.Equal(dateTimeOffset_First, ortoDatasQueueResult.CreatedAt_First);
            Assert.Equal(dateTimeOffset_Last, ortoDatasQueueResult.CreatedAt_Last);

            string? json = Core.Convert.ToSystem_String(ortoDatasQueueResult);
            Assert.NotNull(json);

            OrtoDatasQueueResult? ortoDatasQueueResult_Json = Core.Convert.ToDiGi<OrtoDatasQueueResult>(json)?.FirstOrDefault();
            Assert.NotNull(ortoDatasQueueResult_Json);

            Assert.Equal(55417, ortoDatasQueueResult_Json.CountyId);
            Assert.Equal(12_683, ortoDatasQueueResult_Json.Count);
            Assert.Equal(9_412, ortoDatasQueueResult_Json.WithSubdivisionIdCount);
            Assert.Equal(3_271, ortoDatasQueueResult_Json.WithoutSubdivisionIdCount);
            Assert.Equal(dateTimeOffset_First, ortoDatasQueueResult_Json.CreatedAt_First);
            Assert.Equal(dateTimeOffset_Last, ortoDatasQueueResult_Json.CreatedAt_Last);

            OrtoDatasQueueResult ortoDatasQueueResult_Clone = new(ortoDatasQueueResult);

            Assert.Equal(55417, ortoDatasQueueResult_Clone.CountyId);
            Assert.Equal(12_683, ortoDatasQueueResult_Clone.Count);
            Assert.Equal(9_412, ortoDatasQueueResult_Clone.WithSubdivisionIdCount);
            Assert.Equal(dateTimeOffset_First, ortoDatasQueueResult_Clone.CreatedAt_First);
            Assert.Equal(dateTimeOffset_Last, ortoDatasQueueResult_Clone.CreatedAt_Last);

            Core.xUnit.Query.SerializationCheck(ortoDatasQueueResult);
        }
    }
}
