using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the construction of the two reference sets and that a populated instance survives a JSON round trip and a clone.
        /// <para>The two sets are the whole answer to "what would this move touch and what would it refuse", so a round trip that dropped or swapped a reference would change what a person reviews before a dry run is turned off.</para>
        /// </summary>
        [Fact]
        public void Building2DReferencedObjectStrayResult_Serialization()
        {
            Building2DReferencedObjectStrayResult building2DReferencedObjectStrayResult = new(5, ["272D6AAF-9D86-9B0E-E053-CC2BA8C0B5EA", "9C6F5D2A-1B3E-4F7A-8C2D-6E0F1A2B3C4D"], ["A1B2C3D4-E5F6-4A7B-8C9D-0E1F2A3B4C5D"]);

            Assert.Equal(5, building2DReferencedObjectStrayResult.CountyId);

            HashSet<string> movableReferences = building2DReferencedObjectStrayResult.MovableReferences;
            Assert.Equal(2, movableReferences.Count);
            Assert.Contains("272D6AAF-9D86-9B0E-E053-CC2BA8C0B5EA", movableReferences);
            Assert.Contains("9C6F5D2A-1B3E-4F7A-8C2D-6E0F1A2B3C4D", movableReferences);

            HashSet<string> blockedReferences = building2DReferencedObjectStrayResult.BlockedReferences;
            Assert.Single(blockedReferences);
            Assert.Contains("A1B2C3D4-E5F6-4A7B-8C9D-0E1F2A3B4C5D", blockedReferences);

            string? text = Core.Convert.ToSystem_String(building2DReferencedObjectStrayResult);
            Assert.False(string.IsNullOrWhiteSpace(text));

            Building2DReferencedObjectStrayResult? building2DReferencedObjectStrayResult_Json = Core.Convert.ToDiGi<Building2DReferencedObjectStrayResult>(text)?.FirstOrDefault();
            Assert.NotNull(building2DReferencedObjectStrayResult_Json);
            Assert.Equal(5, building2DReferencedObjectStrayResult_Json.CountyId);

            HashSet<string> movableReferences_Json = building2DReferencedObjectStrayResult_Json.MovableReferences;
            Assert.Equal(2, movableReferences_Json.Count);
            Assert.Contains("272D6AAF-9D86-9B0E-E053-CC2BA8C0B5EA", movableReferences_Json);
            Assert.Contains("9C6F5D2A-1B3E-4F7A-8C2D-6E0F1A2B3C4D", movableReferences_Json);

            HashSet<string> blockedReferences_Json = building2DReferencedObjectStrayResult_Json.BlockedReferences;
            Assert.Single(blockedReferences_Json);
            Assert.Contains("A1B2C3D4-E5F6-4A7B-8C9D-0E1F2A3B4C5D", blockedReferences_Json);

            Building2DReferencedObjectStrayResult building2DReferencedObjectStrayResult_Clone = new(building2DReferencedObjectStrayResult);
            Assert.Equal(5, building2DReferencedObjectStrayResult_Clone.CountyId);

            // The clone has to hold its own sets, or editing one result would rewrite the other.
            Assert.Equal(2, building2DReferencedObjectStrayResult_Clone.MovableReferences.Count);
            Assert.NotSame(building2DReferencedObjectStrayResult.MovableReferences, building2DReferencedObjectStrayResult_Clone.MovableReferences);
            Assert.Single(building2DReferencedObjectStrayResult_Clone.BlockedReferences);
            Assert.NotSame(building2DReferencedObjectStrayResult.BlockedReferences, building2DReferencedObjectStrayResult_Clone.BlockedReferences);

            Core.xUnit.Query.SerializationCheck(building2DReferencedObjectStrayResult);
        }

        /// <summary>
        /// Verifies that a null set comes back empty, and that an empty probe result - a table that holds none of the given references - survives a round trip.
        /// </summary>
        [Fact]
        public void Building2DReferencedObjectStrayResult_Empty()
        {
            Building2DReferencedObjectStrayResult building2DReferencedObjectStrayResult = new(6, null, null);

            Assert.Equal(6, building2DReferencedObjectStrayResult.CountyId);
            Assert.Empty(building2DReferencedObjectStrayResult.MovableReferences);
            Assert.Empty(building2DReferencedObjectStrayResult.BlockedReferences);

            string? text = Core.Convert.ToSystem_String(building2DReferencedObjectStrayResult);
            Building2DReferencedObjectStrayResult? building2DReferencedObjectStrayResult_Json = Core.Convert.ToDiGi<Building2DReferencedObjectStrayResult>(text)?.FirstOrDefault();
            Assert.NotNull(building2DReferencedObjectStrayResult_Json);
            Assert.Equal(6, building2DReferencedObjectStrayResult_Json.CountyId);
            Assert.Empty(building2DReferencedObjectStrayResult_Json.MovableReferences);
            Assert.Empty(building2DReferencedObjectStrayResult_Json.BlockedReferences);

            Core.xUnit.Query.SerializationCheck(building2DReferencedObjectStrayResult);
        }
    }
}
