using DiGi.Analytical.Building.Classes;
using DiGi.EPW;
using DiGi.EPW.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.GIS.WebAPI.UI.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that the filler snow depth of the IWEC Warsaw file does not turn the ground into snow: a wall's ground-reflected irradiation uses the 0.2 default albedo in every hour.
        /// <para><c>gis/epwfile/item</c> serves this file for Warsaw Ursynów, and it reports lying snow for most hours of the year, a filler value (ZiolkowskiJakub/DiGi.Solar#2). Deriving snow cover from it applied the 0.7 snow albedo almost all year and read a north wall at 658 kWh/m² instead of 410 (DiGi.GIS.WebAPI.UI#59, comment 5830021444). The fact first asserts the file really carries the filler, so it keeps testing the case it exists for. A vertical wall sees half the ground, so its ground component is the annual global horizontal irradiation times 0.2 / 2.</para>
        /// </summary>
        [Fact]
        public void Create_SurfaceSolarRadiationResults_SnowFiller()
        {
            BuildingModel buildingModel = SolarFixture_Box();
            EPWFile ePWFile = SolarFixture_EPWFile();

            IList<DataRecord>? dataRecords = ePWFile.DataRecords;
            Assert.NotNull(dataRecords);

            int count_Snow = 0;
            double globalHorizontalIrradiation = 0;
            foreach (DataRecord dataRecord in dataRecords)
            {
                if (dataRecord.SnowDepthValue() > 0)
                {
                    count_Snow++;
                }

                if (dataRecord.GlobalHorizontalRadiationValue() is float globalHorizontalRadiation && dataRecord.DirectNormalRadiationValue() is not null && dataRecord.DiffuseHorizontalRadiationValue() is not null && Query.SolarReferenceDateTime(dataRecord.DateTime) is not null)
                {
                    globalHorizontalIrradiation += globalHorizontalRadiation;
                }
            }

            Assert.True(count_Snow > 4000, $"The fixture reports snow for {count_Snow} hours only; it no longer carries the filler this fact is about.");

            List<SurfaceSolarRadiationResult>? surfaceSolarRadiationResults = buildingModel.SurfaceSolarRadiationResults(null, ePWFile, SolarFixture_ShadingSolverOptions());
            Assert.NotNull(surfaceSolarRadiationResults);

            Dictionary<string, Vector3D>? normals = buildingModel.SolarReceiverNormals();
            Assert.NotNull(normals);

            double ground_Expected = globalHorizontalIrradiation * 0.2 * 0.5 / 1000;

            SurfaceSolarRadiationResult north = SolarFixture_Result(surfaceSolarRadiationResults, normals, new Vector3D(0, 1, 0));
            Assert.Equal(ground_Expected, north.Ground, 6);
        }
    }
}
