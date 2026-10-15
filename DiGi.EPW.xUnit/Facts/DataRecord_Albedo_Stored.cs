using System.IO;
using System.Reflection;
using System.Text.Json.Nodes;

namespace DiGi.EPW.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a <see cref="Classes.DataRecord"/> stored before <c>Albedo</c> moved from <see cref="Classes.DataRecord"/> up to <see cref="Weather.Classes.WeatherRecord"/> still reads its albedo, through the base member, from the unchanged wire name <c>Albedo</c>. This guards the <c>epw_file</c> rows already stored.
        /// <para>The fixture <c>DataRecord_BeforeWeatherAlbedo.json</c> is a real <c>ToJsonObject()</c> of the first record of the Warsaw file, written by the build before the change, with its albedo set to 0.23 so that a field left at its default 0 cannot pass. Reference: DiGi.Weather#2.</para>
        /// </summary>
        [Fact]
        public void DataRecord_Albedo_Stored()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "DataRecord_BeforeWeatherAlbedo.json");
            Assert.NotNull(path);

            JsonObject? jsonObject = JsonNode.Parse(File.ReadAllText(path)) as JsonObject;
            Assert.NotNull(jsonObject);

            Classes.DataRecord dataRecord = new(jsonObject);
            Assert.Equal(0.23f, ((Weather.Classes.WeatherRecord)dataRecord).Albedo);
            Assert.Equal(0.23f, dataRecord.Albedo);
            Assert.Equal(0.23f, Query.AlbedoValue(dataRecord));
            Assert.Equal(0.23f, Weather.Query.AlbedoValue(dataRecord));

            Classes.DataRecord? dataRecord_Clone = Core.Query.Clone(dataRecord);
            Assert.NotNull(dataRecord_Clone);
            Assert.Equal(0.23f, dataRecord_Clone.Albedo);
            Assert.Equal(0.23f, new Classes.DataRecord(dataRecord).Albedo);
        }
    }
}
