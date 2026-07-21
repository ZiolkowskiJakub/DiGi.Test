using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.HVAC;
using DiGi.Analytical.Building.HVAC.Enums;
using DiGi.Analytical.Classes;
using DiGi.Core.Classes;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the building model profile assignment and queries the indexed doubles.
        /// </summary>
        [Fact]
        public void ProfileTest()
        {
            InternalGainProfileType internalGainProfileType = InternalGainProfileType.EquipmentLatentGain;

            Profile profile_1 = new([0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23]);

            InternalCondition internalCondition_1 = new("Internal Condition 1") { Description = "Internal Condition 1" };
            internalCondition_1.SetProfile(internalGainProfileType, profile_1);

            Profile profile_2 = new([100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123]);

            InternalCondition internalCondition_2 = new("Internal Condition 2") { Description = "Internal Condition 2" };
            internalCondition_2.SetProfile(internalGainProfileType, profile_2);

            Space space = new(new Geometry.Spatial.Classes.Point3D(0, 0, 0), "Space");

            BuildingModel buildingModel = new();

            buildingModel.Assign(space, internalCondition_1, new HourRange(24, 47));
            buildingModel.Assign(space, internalCondition_2, new HourRange(48, 48 + 23));

            IndexedDoubles? indexedDoubles = Building.HVAC.Query.IndexedDoubles(buildingModel, space, new Range<int>(0, 71), internalGainProfileType);
            Assert.NotNull(indexedDoubles);

            buildingModel = new();

            buildingModel.Assign(space, internalCondition_1);

            indexedDoubles = Building.HVAC.Query.IndexedDoubles(buildingModel, space, new Range<int>(0, 71), internalGainProfileType);
            Assert.NotNull(indexedDoubles);

            int hourIndex = Query.FirstHourIndex(DayOfWeek.Monday, 2025);

            DateTime dateTime = Create.DateTime(2025, hourIndex + (5 * 24));
            Assert.Equal(2025, dateTime.Year);
        }
    }
}