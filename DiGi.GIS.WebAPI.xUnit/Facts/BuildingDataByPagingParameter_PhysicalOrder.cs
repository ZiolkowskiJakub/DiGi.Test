using DiGi.GIS.WebAPI.Classes;
using System.Text.Json;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the wire contract of <see cref="BuildingDataByPagingParameter.PhysicalOrder"/> (DiGi.GIS.WebAPI#40).
        /// <para>The flag defaults to false, and a body that omits it binds false, so existing clients keep the reference order they always got. A body carrying <c>true</c> binds true, in any property-name casing, because the controller binds with web defaults and ASP.NET Core binds case-insensitively.</para>
        /// </summary>
        [Fact]
        public void BuildingDataByPagingParameter_PhysicalOrder()
        {
            Assert.False(new BuildingDataByPagingParameter().PhysicalOrder);

            BuildingDataByPagingParameter? parameter_Omitted = JsonSerializer.Deserialize<BuildingDataByPagingParameter>("{ \"CountyId\": 5, \"PageSize\": 10, \"Cursor\": null }", JsonSerializerOptions.Web);
            Assert.NotNull(parameter_Omitted);
            Assert.False(parameter_Omitted.PhysicalOrder);
            Assert.Equal(5, parameter_Omitted.CountyId);

            BuildingDataByPagingParameter? parameter_Physical = JsonSerializer.Deserialize<BuildingDataByPagingParameter>("{ \"CountyId\": 5, \"PhysicalOrder\": true, \"Cursor\": \"(12,3)\" }", JsonSerializerOptions.Web);
            Assert.NotNull(parameter_Physical);
            Assert.True(parameter_Physical.PhysicalOrder);
            Assert.Equal("(12,3)", parameter_Physical.Cursor);

            BuildingDataByPagingParameter? parameter_CamelCase = JsonSerializer.Deserialize<BuildingDataByPagingParameter>("{ \"countyId\": 5, \"physicalOrder\": true }", JsonSerializerOptions.Web);
            Assert.NotNull(parameter_CamelCase);
            Assert.True(parameter_CamelCase.PhysicalOrder);

            string json = JsonSerializer.Serialize(parameter_Physical, JsonSerializerOptions.Default);
            BuildingDataByPagingParameter? parameter_RoundTrip = JsonSerializer.Deserialize<BuildingDataByPagingParameter>(json, JsonSerializerOptions.Web);
            Assert.NotNull(parameter_RoundTrip);
            Assert.True(parameter_RoundTrip.PhysicalOrder);
            Assert.Equal("(12,3)", parameter_RoundTrip.Cursor);
        }
    }
}
