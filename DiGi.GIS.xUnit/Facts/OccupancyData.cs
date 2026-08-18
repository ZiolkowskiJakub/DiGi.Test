using DiGi.Core;
using System.Linq;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that occupancy data survives a JSON round trip and a copy, keeping the two values that address it in the database.
        /// <para>A stored occupancy row is addressed by the reference of the building it describes together with the unique identifier of the object itself, so a copy that loses either one can no longer be matched to the row it came from.</para>
        /// </summary>
        [Fact]
        public void OccupancyData()
        {
            Classes.OccupancyData occupancyData = new("272D6AAF-9D86-9B0E-E053-CC2BA8C0B5EA", 342.75, 7);

            Assert.Equal("272D6AAF-9D86-9B0E-E053-CC2BA8C0B5EA", occupancyData.Reference);
            Assert.Equal(342.75, occupancyData.OccupancyArea);
            Assert.Equal<uint?>(7, occupancyData.Occupancy);

            //Round trip

            string? json = Core.Convert.ToSystem_String(occupancyData);
            Assert.False(string.IsNullOrWhiteSpace(json));

            Interfaces.IOccupancyData? occupancyData_Json = Core.Convert.ToDiGi<Interfaces.IOccupancyData>(json)?.FirstOrDefault();
            Assert.NotNull(occupancyData_Json);
            Assert.Equal(occupancyData.Reference, occupancyData_Json.Reference);
            Assert.Equal(occupancyData.UniqueId, occupancyData_Json.UniqueId);

            Core.xUnit.Query.SerializationCheck(occupancyData);

            //Copy

            Classes.OccupancyData occupancyData_Copy = new(occupancyData);

            Assert.Equal(occupancyData.Reference, occupancyData_Copy.Reference);
            Assert.Equal(occupancyData.OccupancyArea, occupancyData_Copy.OccupancyArea);
            Assert.Equal(occupancyData.Occupancy, occupancyData_Copy.Occupancy);
            Assert.Equal(occupancyData.UniqueId, occupancyData_Copy.UniqueId);
        }
    }
}
