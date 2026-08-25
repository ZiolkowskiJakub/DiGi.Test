using DiGi.GIS.PostgreSQL.Classes;
using System;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the constructor carries every tally through, that the derived count agrees with the two stored ones, and that a populated instance survives a JSON round trip and a clone.
        /// <para><see cref="OrtoDatasCountyResult.WithoutSubdivisionIdCount"/> is derived rather than stored precisely so it cannot drift from the pair it is computed from, and that is what makes it worth asserting.</para>
        /// </summary>
        [Fact]
        public void OrtoDatasCountyResult_Serialization()
        {
            DateTimeOffset dateTimeOffset_First = new(2026, 3, 14, 9, 30, 0, TimeSpan.FromHours(1));
            DateTimeOffset dateTimeOffset_Last = new(2026, 8, 21, 17, 5, 0, TimeSpan.FromHours(2));

            OrtoDatasCountyResult ortoDatasCountyResult = new(55417, 33_687, 21_004, 147, dateTimeOffset_First, dateTimeOffset_Last);

            Assert.Equal(55417, ortoDatasCountyResult.CountyId);
            Assert.Equal(33_687, ortoDatasCountyResult.Count);
            Assert.Equal(21_004, ortoDatasCountyResult.WithSubdivisionIdCount);
            Assert.Equal(12_683, ortoDatasCountyResult.WithoutSubdivisionIdCount);
            Assert.Equal(147, ortoDatasCountyResult.SubdivisionIdCount);
            Assert.Equal(dateTimeOffset_First, ortoDatasCountyResult.CreatedAt_First);
            Assert.Equal(dateTimeOffset_Last, ortoDatasCountyResult.CreatedAt_Last);

            string? json = Core.Convert.ToSystem_String(ortoDatasCountyResult);
            Assert.NotNull(json);

            OrtoDatasCountyResult? ortoDatasCountyResult_Json = Core.Convert.ToDiGi<OrtoDatasCountyResult>(json)?.FirstOrDefault();
            Assert.NotNull(ortoDatasCountyResult_Json);

            Assert.Equal(55417, ortoDatasCountyResult_Json.CountyId);
            Assert.Equal(33_687, ortoDatasCountyResult_Json.Count);
            Assert.Equal(21_004, ortoDatasCountyResult_Json.WithSubdivisionIdCount);
            Assert.Equal(12_683, ortoDatasCountyResult_Json.WithoutSubdivisionIdCount);
            Assert.Equal(147, ortoDatasCountyResult_Json.SubdivisionIdCount);

            // The timestamps are the reason these carry DateTimeOffset rather than DateTime: a DateTime comes
            // back with the local offset applied and no longer equals what went in.
            Assert.Equal(dateTimeOffset_First, ortoDatasCountyResult_Json.CreatedAt_First);
            Assert.Equal(dateTimeOffset_Last, ortoDatasCountyResult_Json.CreatedAt_Last);

            OrtoDatasCountyResult ortoDatasCountyResult_Clone = new(ortoDatasCountyResult);

            Assert.Equal(55417, ortoDatasCountyResult_Clone.CountyId);
            Assert.Equal(33_687, ortoDatasCountyResult_Clone.Count);
            Assert.Equal(21_004, ortoDatasCountyResult_Clone.WithSubdivisionIdCount);
            Assert.Equal(147, ortoDatasCountyResult_Clone.SubdivisionIdCount);
            Assert.Equal(dateTimeOffset_First, ortoDatasCountyResult_Clone.CreatedAt_First);
            Assert.Equal(dateTimeOffset_Last, ortoDatasCountyResult_Clone.CreatedAt_Last);

            Core.xUnit.Query.SerializationCheck(ortoDatasCountyResult);
        }

        /// <summary>
        /// Verifies that a county holding nothing round trips with null timestamps rather than with a default date.
        /// <para>An empty partition has no earliest and no latest row, and a zero date would read as one written in the year one.</para>
        /// </summary>
        [Fact]
        public void OrtoDatasCountyResult_Empty()
        {
            OrtoDatasCountyResult ortoDatasCountyResult = new(55417, 0, 0, 0, null, null);

            Assert.Equal(0, ortoDatasCountyResult.Count);
            Assert.Equal(0, ortoDatasCountyResult.WithoutSubdivisionIdCount);
            Assert.Null(ortoDatasCountyResult.CreatedAt_First);
            Assert.Null(ortoDatasCountyResult.CreatedAt_Last);

            string? json = Core.Convert.ToSystem_String(ortoDatasCountyResult);
            OrtoDatasCountyResult? ortoDatasCountyResult_Json = Core.Convert.ToDiGi<OrtoDatasCountyResult>(json)?.FirstOrDefault();

            Assert.NotNull(ortoDatasCountyResult_Json);
            Assert.Null(ortoDatasCountyResult_Json.CreatedAt_First);
            Assert.Null(ortoDatasCountyResult_Json.CreatedAt_Last);

            Core.xUnit.Query.SerializationCheck(ortoDatasCountyResult);
        }
    }
}
