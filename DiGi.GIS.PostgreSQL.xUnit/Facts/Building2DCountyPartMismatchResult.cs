using DiGi.GIS.PostgreSQL.Classes;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a mismatch measurement carries every field through the constructor, a JSON round trip and a clone.
        /// <para>These figures are the before and after of a repair, read over HTTP from a machine that has no access to the database, so a field lost in serialization is a figure nobody can check.</para>
        /// </summary>
        [Fact]
        public void Building2DCountyPartMismatchResult_Serialization()
        {
            Building2DCountyPartMismatchResult building2DCountyPartMismatchResult = new("3020", 97360, [97358, 97360, 97364], 37_260, 37_260);

            Assert.Equal("3020", building2DCountyPartMismatchResult.Code);
            Assert.Equal(97360, building2DCountyPartMismatchResult.CountyId);
            Assert.Equal(37_260, building2DCountyPartMismatchResult.Count);
            Assert.Equal(37_260, building2DCountyPartMismatchResult.CountOutsideBoundingBox);
            Assert.NotNull(building2DCountyPartMismatchResult.CountyIds);
            Assert.Equal(3, building2DCountyPartMismatchResult.CountyIds.Count);

            string? json = Core.Convert.ToSystem_String(building2DCountyPartMismatchResult);
            Assert.NotNull(json);

            Building2DCountyPartMismatchResult? building2DCountyPartMismatchResult_Json = Core.Convert.ToDiGi<Building2DCountyPartMismatchResult>(json)?.FirstOrDefault();
            Assert.NotNull(building2DCountyPartMismatchResult_Json);

            Assert.Equal("3020", building2DCountyPartMismatchResult_Json.Code);
            Assert.Equal(97360, building2DCountyPartMismatchResult_Json.CountyId);
            Assert.Equal(37_260, building2DCountyPartMismatchResult_Json.Count);
            Assert.Equal(37_260, building2DCountyPartMismatchResult_Json.CountOutsideBoundingBox);
            Assert.NotNull(building2DCountyPartMismatchResult_Json.CountyIds);
            Assert.Contains(97358, building2DCountyPartMismatchResult_Json.CountyIds);

            Building2DCountyPartMismatchResult building2DCountyPartMismatchResult_Clone = new(building2DCountyPartMismatchResult);

            Assert.Equal(37_260, building2DCountyPartMismatchResult_Clone.CountOutsideBoundingBox);
            Assert.NotNull(building2DCountyPartMismatchResult_Clone.CountyIds);
            Assert.Equal(3, building2DCountyPartMismatchResult_Clone.CountyIds.Count);

            Core.xUnit.Query.SerializationCheck(building2DCountyPartMismatchResult);
        }

        /// <summary>
        /// Verifies that a part holding buildings that all sit inside it reports nothing outside, which is what a repaired county looks like.
        /// </summary>
        [Fact]
        public void Building2DCountyPartMismatchResult_Clean()
        {
            Building2DCountyPartMismatchResult building2DCountyPartMismatchResult = new("2405", 76984, [76984, 76989], 42_585, 0);

            Assert.Equal(0, building2DCountyPartMismatchResult.CountOutsideBoundingBox);
            Assert.Equal(42_585, building2DCountyPartMismatchResult.Count);

            Core.xUnit.Query.SerializationCheck(building2DCountyPartMismatchResult);
        }
    }
}
