using DiGi.GIS.WebAPI.UI.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the serialization of <see cref="SurfaceSolarRadiationResult"/>, which is both the JSON of the radiation route and the properties of a coloured glTF node: every member populated with a distinct value survives the string round trip and <c>SerializationCheck</c>, and the copy constructor - a path <c>SerializationCheck</c> never runs - copies every member.
        /// </summary>
        [Fact]
        public void SurfaceSolarRadiationResult_Serialization()
        {
            SurfaceSolarRadiationResult surfaceSolarRadiationResult = new("DiGi.Core.Classes.GuidReference::6f0c7d4e-2a39-4a57-9d1f-3a1b5c2d7e80", 12.5, 640.25, 410.5, 200.75, 29.0, 701.125, 8003.125);

            Assert.Equal("DiGi.Core.Classes.GuidReference::6f0c7d4e-2a39-4a57-9d1f-3a1b5c2d7e80", surfaceSolarRadiationResult.Reference);
            Assert.Equal(12.5, surfaceSolarRadiationResult.Area);
            Assert.Equal(640.25, surfaceSolarRadiationResult.Irradiation);
            Assert.Equal(410.5, surfaceSolarRadiationResult.Beam);
            Assert.Equal(200.75, surfaceSolarRadiationResult.Diffuse);
            Assert.Equal(29.0, surfaceSolarRadiationResult.Ground);
            Assert.Equal(701.125, surfaceSolarRadiationResult.IrradiationUnshaded);
            Assert.Equal(8003.125, surfaceSolarRadiationResult.Energy);

            string? json = Core.Convert.ToSystem_String(surfaceSolarRadiationResult);
            Assert.False(string.IsNullOrWhiteSpace(json));
            Assert.Contains("\"_type\"", json);

            SurfaceSolarRadiationResult? surfaceSolarRadiationResult_Json = Core.Convert.ToDiGi<SurfaceSolarRadiationResult>(json)?.FirstOrDefault();
            Assert.NotNull(surfaceSolarRadiationResult_Json);
            SurfaceSolarRadiationResult_Serialization_AssertEqual(surfaceSolarRadiationResult, surfaceSolarRadiationResult_Json);

            // The radiation route answers a list; a DiGi client reads it back with ToDiGi.
            string? json_List = Core.Convert.ToSystem_String(new List<SurfaceSolarRadiationResult>() { surfaceSolarRadiationResult, surfaceSolarRadiationResult_Json });
            List<SurfaceSolarRadiationResult>? surfaceSolarRadiationResults = Core.Convert.ToDiGi<SurfaceSolarRadiationResult>(json_List);
            Assert.NotNull(surfaceSolarRadiationResults);
            Assert.Equal(2, surfaceSolarRadiationResults.Count);

            SurfaceSolarRadiationResult_Serialization_AssertEqual(surfaceSolarRadiationResult, new SurfaceSolarRadiationResult(surfaceSolarRadiationResult));

            Core.xUnit.Query.SerializationCheck(surfaceSolarRadiationResult);
        }

        private static void SurfaceSolarRadiationResult_Serialization_AssertEqual(SurfaceSolarRadiationResult expected, SurfaceSolarRadiationResult actual)
        {
            Assert.Equal(expected.Reference, actual.Reference);
            Assert.Equal(expected.Area, actual.Area);
            Assert.Equal(expected.Irradiation, actual.Irradiation);
            Assert.Equal(expected.Beam, actual.Beam);
            Assert.Equal(expected.Diffuse, actual.Diffuse);
            Assert.Equal(expected.Ground, actual.Ground);
            Assert.Equal(expected.IrradiationUnshaded, actual.IrradiationUnshaded);
            Assert.Equal(expected.Energy, actual.Energy);
        }
    }
}
