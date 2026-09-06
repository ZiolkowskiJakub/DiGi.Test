using DiGi.GIS.PostgreSQL.Classes;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a move record carries every field through the constructor, a JSON round trip and a clone.
        /// <para>The record is what a dry run reports and what a live run writes from, so a field lost between the two is a building moved to the wrong place or not moved at all.</para>
        /// </summary>
        [Fact]
        public void Building2DCountyPartMoveResult_Serialization()
        {
            Building2DCountyPartMoveResult building2DCountyPartMoveResult = new("3020", "2EBDA2DF-DD74-D82C-E053-CC2BA8C0C90E", 8_675_309, 97360, 97358, true);

            Assert.Equal("3020", building2DCountyPartMoveResult.Code);
            Assert.Equal("2EBDA2DF-DD74-D82C-E053-CC2BA8C0C90E", building2DCountyPartMoveResult.Reference);
            Assert.Equal(8_675_309, building2DCountyPartMoveResult.Id);
            Assert.Equal(97360, building2DCountyPartMoveResult.CountyId);
            Assert.Equal(97358, building2DCountyPartMoveResult.CountyIdResolved);
            Assert.True(building2DCountyPartMoveResult.DecidedByGeometry);

            string? json = Core.Convert.ToSystem_String(building2DCountyPartMoveResult);
            Assert.NotNull(json);

            Building2DCountyPartMoveResult? building2DCountyPartMoveResult_Json = Core.Convert.ToDiGi<Building2DCountyPartMoveResult>(json)?.FirstOrDefault();
            Assert.NotNull(building2DCountyPartMoveResult_Json);

            Assert.Equal("3020", building2DCountyPartMoveResult_Json.Code);
            Assert.Equal("2EBDA2DF-DD74-D82C-E053-CC2BA8C0C90E", building2DCountyPartMoveResult_Json.Reference);
            Assert.Equal(8_675_309, building2DCountyPartMoveResult_Json.Id);
            Assert.Equal(97360, building2DCountyPartMoveResult_Json.CountyId);
            Assert.Equal(97358, building2DCountyPartMoveResult_Json.CountyIdResolved);
            Assert.True(building2DCountyPartMoveResult_Json.DecidedByGeometry);

            Building2DCountyPartMoveResult building2DCountyPartMoveResult_Clone = new(building2DCountyPartMoveResult);

            Assert.Equal(97358, building2DCountyPartMoveResult_Clone.CountyIdResolved);
            Assert.Equal(8_675_309, building2DCountyPartMoveResult_Clone.Id);

            Core.xUnit.Query.SerializationCheck(building2DCountyPartMoveResult);
        }

        /// <summary>
        /// Verifies that a building no part could be decided for is recorded with a resolved identifier of -1.
        /// <para>That is what separates a building left alone because nothing could be worked out from one left alone because it already sits where it belongs - the second is never recorded at all.</para>
        /// </summary>
        [Fact]
        public void Building2DCountyPartMoveResult_Unresolved()
        {
            Building2DCountyPartMoveResult building2DCountyPartMoveResult = new("2412", "REF", 1, 78238, -1, true);

            Assert.Equal(-1, building2DCountyPartMoveResult.CountyIdResolved);
            Assert.Equal(78238, building2DCountyPartMoveResult.CountyId);

            Core.xUnit.Query.SerializationCheck(building2DCountyPartMoveResult);
        }
    }
}
