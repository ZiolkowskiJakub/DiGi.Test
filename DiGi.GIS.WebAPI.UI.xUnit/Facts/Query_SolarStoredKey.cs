using DiGi.Analytical.Building.Classes;
using System;
using System.Collections.Generic;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the helpers that key a stored solar radiation result: the radius rounds to 0.1 m (midpoints away from zero), the building model id is positive, stable for one model - also after its JSON round trip - and different for a regenerated model, and the reference hours run 1 January 00:30 to 31 December 23:30 with indexes 0 to 8 759 and none outside the year.
        /// <para>Reference: DiGi.GIS.WebAPI.UI#74.</para>
        /// </summary>
        [Fact]
        public void Query_SolarStoredKey()
        {
            Assert.Equal(50.0, Query.SolarRadiusKey(50.0));
            Assert.Equal(50.0, Query.SolarRadiusKey(50.04));
            Assert.Equal(50.1, Query.SolarRadiusKey(50.05));
            Assert.Equal(0.0, Query.SolarRadiusKey(0.04));
            Assert.Equal(Query.SolarRadiusKey(37.25), Query.SolarRadiusKey(37.25));

            BuildingModel buildingModel = SolarFixture_Box();
            long? buildingModelId = buildingModel.SolarBuildingModelId();
            Assert.NotNull(buildingModelId);
            Assert.True(buildingModelId > 0);
            Assert.Equal(buildingModelId, buildingModel.SolarBuildingModelId());

            BuildingModel? buildingModel_Read = Core.Convert.ToDiGi<BuildingModel>(Core.Convert.ToSystem_String(new List<BuildingModel>() { buildingModel }))?[0];
            Assert.NotNull(buildingModel_Read);
            Assert.Equal(buildingModelId, buildingModel_Read.SolarBuildingModelId());

            Assert.NotEqual(buildingModelId, SolarFixture_Box().SolarBuildingModelId());
            Assert.Null(((BuildingModel?)null).SolarBuildingModelId());

            List<DateTime> dateTimes = Query.SolarReferenceDateTimes();
            Assert.Equal(Constants.Default.SolarReferenceHourCount, dateTimes.Count);
            Assert.Equal(new DateTime(Constants.Default.SolarReferenceYear, 1, 1, 0, 30, 0), dateTimes[0]);
            Assert.Equal(new DateTime(Constants.Default.SolarReferenceYear, 12, 31, 23, 30, 0), dateTimes[^1]);
            for (int i = 0; i < dateTimes.Count; i++)
            {
                Assert.Equal(i, dateTimes[i].SolarReferenceHourIndex());
            }

            Assert.Equal(1, new DateTime(1992, 1, 1, 2, 0, 0).SolarReferenceDateTime()!.Value.SolarReferenceHourIndex());
            Assert.Null(new DateTime(Constants.Default.SolarReferenceYear + 1, 1, 1, 0, 30, 0).SolarReferenceHourIndex());
            Assert.Null(new DateTime(Constants.Default.SolarReferenceYear - 1, 12, 31, 23, 30, 0).SolarReferenceHourIndex());
        }
    }
}
