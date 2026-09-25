using DiGi.Analytical.Building.Classes;
using DiGi.EPW.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.GIS.WebAPI.UI.Classes;
using DiGi.GIS.WebAPI.UI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the solar radiation routes end to end on a scripted GIS Web API: the JSON route answers one DiGi-readable result per external wall and roof, the glb route a binary glTF scene, and the requests sent upstream are the declared ones.
        /// <para>The neighbour request is the wire contract with <c>gis/buildingmodel/itemsbycircle</c> (Coding - WebAPI Contracts, section 4): <c>x</c> and <c>y</c> are the centre of the footprint and <c>radius</c> reaches the requested radius beyond its farthest corner - 50 m plus the half-diagonal of the 10 m box. The weather is asked for at the same centre.</para>
        /// </summary>
        [Fact]
        public async Task SolarController_GetRadiationByBuildingModelIdAsync()
        {
            BuildingModel buildingModel = SolarFixture_BuildingModel(SolarFixture_BoxComponents(SolarFixture_Origin.X, SolarFixture_Origin.Y, 10), new Point3D(SolarFixture_Origin.X + 5, SolarFixture_Origin.Y + 5, 5));
            ScriptedWebApi scriptedWebApi = SolarController_ScriptedWebApi(buildingModel, [buildingModel], SolarFixture_EPWFile());

            IActionResult actionResult = await SolarController_Controller(scriptedWebApi).GetRadiationByBuildingModelIdAsync(7, 1465, null);

            ContentResult contentResult = Assert.IsType<ContentResult>(actionResult);
            Assert.Equal("application/json", contentResult.ContentType);

            List<SurfaceSolarRadiationResult>? surfaceSolarRadiationResults = Core.Convert.ToDiGi<SurfaceSolarRadiationResult>(contentResult.Content);
            Assert.NotNull(surfaceSolarRadiationResults);
            Assert.Equal(5, surfaceSolarRadiationResults.Count);
            Assert.All(surfaceSolarRadiationResults, x => Assert.True(x.Irradiation > 0));

            (HttpMethod Method, Uri? RequestUri, string? Body) request_Circle = Assert.Single(scriptedWebApi.Requests, x => x.RequestUri!.AbsolutePath.EndsWith("/gis/buildingmodel/itemsbycircle", StringComparison.OrdinalIgnoreCase));
            Dictionary<string, StringValues> query_Circle = QueryHelpers.ParseQuery(request_Circle.RequestUri!.Query);
            Assert.Equal(SolarFixture_Origin.X + 5, double.Parse(query_Circle["x"]!, CultureInfo.InvariantCulture), 6);
            Assert.Equal(SolarFixture_Origin.Y + 5, double.Parse(query_Circle["y"]!, CultureInfo.InvariantCulture), 6);
            Assert.Equal(Constants.Default.SolarSurroundingRadius + Math.Sqrt(50), double.Parse(query_Circle["radius"]!, CultureInfo.InvariantCulture), 6);

            (HttpMethod Method, Uri? RequestUri, string? Body) request_EPW = Assert.Single(scriptedWebApi.Requests, x => x.RequestUri!.AbsolutePath.EndsWith("/gis/epwfile/item", StringComparison.OrdinalIgnoreCase));
            Dictionary<string, StringValues> query_EPW = QueryHelpers.ParseQuery(request_EPW.RequestUri!.Query);
            Assert.Equal(SolarFixture_Origin.X + 5, double.Parse(query_EPW["x"]!, CultureInfo.InvariantCulture), 6);
            Assert.Equal(SolarFixture_Origin.Y + 5, double.Parse(query_EPW["y"]!, CultureInfo.InvariantCulture), 6);

            // The glb route runs the same pipeline and streams the coloured scene.
            IActionResult actionResult_GLB = await SolarController_Controller(scriptedWebApi).GetGLBBuildingModelByIdAsync(7, 1465, 20);
            FileContentResult fileContentResult = Assert.IsType<FileContentResult>(actionResult_GLB);
            Assert.Equal("model/gltf-binary", fileContentResult.ContentType);
            Assert.True(fileContentResult.FileContents.Length > 0);

            (HttpMethod Method, Uri? RequestUri, string? Body) request_Circle_GLB = scriptedWebApi.Requests.Last(x => x.RequestUri!.AbsolutePath.EndsWith("/gis/buildingmodel/itemsbycircle", StringComparison.OrdinalIgnoreCase));
            Assert.Equal(20 + Math.Sqrt(50), double.Parse(QueryHelpers.ParseQuery(request_Circle_GLB.RequestUri!.Query)["radius"]!, CultureInfo.InvariantCulture), 6);
        }

        // The controller under test on a scripted GIS Web API, wired as Program.cs wires it: its own one-slot gate.
        private static SolarController SolarController_Controller(ScriptedWebApi scriptedWebApi)
        {
            return new SolarController(new ScriptedHttpClientFactory(scriptedWebApi), new SemaphoreSlim(1, 1), NullLogger<SolarController>.Instance);
        }

        // A GIS Web API serving one building model, its neighbours and a weather file. A null argument answers
        // 404 for that request; the status overrides let a fact break one request at a time.
        private static ScriptedWebApi SolarController_ScriptedWebApi(BuildingModel? buildingModel, List<BuildingModel>? buildingModels_Circle, EPWFile? ePWFile, HttpStatusCode httpStatusCode_Circle = HttpStatusCode.OK)
        {
            string? json_Model = buildingModel is null ? null : Core.Convert.ToSystem_String(new List<BuildingModel>() { buildingModel });
            string? json_Circle = buildingModels_Circle is null ? null : Core.Convert.ToSystem_String(buildingModels_Circle);
            string? json_EPW = ePWFile is null ? null : Core.Convert.ToSystem_String((Core.Interfaces.ISerializableObject)ePWFile);
            string? json_Reference = buildingModel is null ? null : Core.Convert.ToSystem_String(new PostgreSQL.Classes.Building2DReference() { Id = 7, CountyId = 1465, Reference = "B7" });

            return new ScriptedWebApi((httpRequestMessage, body) =>
            {
                string path = httpRequestMessage.RequestUri!.AbsolutePath;

                string? json = null;
                HttpStatusCode httpStatusCode = HttpStatusCode.OK;
                if (path.EndsWith("/gis/building2D/building2Dreferencebyid", StringComparison.OrdinalIgnoreCase))
                {
                    json = json_Reference;
                }
                else if (path.EndsWith("/gis/buildingmodel/itemsbyreferences", StringComparison.OrdinalIgnoreCase))
                {
                    json = json_Model;
                }
                else if (path.EndsWith("/gis/buildingmodel/itemsbycircle", StringComparison.OrdinalIgnoreCase))
                {
                    json = json_Circle;
                    httpStatusCode = httpStatusCode_Circle;
                }
                else if (path.EndsWith("/gis/epwfile/item", StringComparison.OrdinalIgnoreCase))
                {
                    json = json_EPW;
                }

                if (json is null || httpStatusCode != HttpStatusCode.OK)
                {
                    return new HttpResponseMessage(json is null ? HttpStatusCode.NotFound : httpStatusCode) { Content = new StringContent(string.Empty) };
                }

                return Answer(json);
            });
        }
    }
}
