using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.EPW.Classes;
using DiGi.Geometry.Spatial.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests every refusal of the solar radiation routes on a scripted GIS Web API: 400 for a radius outside (0, <see cref="Constants.Default.SolarSurroundingRadiusMax"/>] before any request is sent, 204 when the building or its weather file is not found, 422 when the building cannot be located or is degenerate, 413 above the receiver and caster triangle limits, and 502 when the neighbours cannot be read.
        /// <para>The 502 is the fail-open case the issue's first design missed: the upstream helpers answer null for every failure, and the neighbour circle always contains the building itself, so a missing answer is a failed read. Treating it as "no neighbours" would have answered an unshaded result with 200.</para>
        /// </summary>
        [Fact]
        public async Task SolarController_GetRadiationByBuildingModelIdAsync_Refusals()
        {
            EPWFile ePWFile = SolarFixture_EPWFile();
            Point3D location = new(SolarFixture_Origin.X + 5, SolarFixture_Origin.Y + 5, 5);
            BuildingModel buildingModel = SolarFixture_BuildingModel(SolarFixture_BoxComponents(SolarFixture_Origin.X, SolarFixture_Origin.Y, 10), location);

            // 400: an invalid radius is refused before anything is fetched.
            foreach (double radius in new double[] { 0, -5, Constants.Default.SolarSurroundingRadiusMax + 1, double.NaN, double.PositiveInfinity })
            {
                ScriptedWebApi scriptedWebApi_Radius = SolarController_ScriptedWebApi(buildingModel, [buildingModel], ePWFile);
                IActionResult actionResult_Radius = await SolarController_Controller(scriptedWebApi_Radius).GetRadiationByBuildingModelIdAsync(7, 1465, radius);
                Assert.IsType<BadRequestObjectResult>(actionResult_Radius);
                Assert.Empty(scriptedWebApi_Radius.Requests);
            }

            // 204: the building is not found.
            IActionResult actionResult_NoModel = await SolarController_Controller(SolarController_ScriptedWebApi(null, [buildingModel], ePWFile)).GetRadiationByBuildingModelIdAsync(7, 1465, null);
            Assert.IsType<NoContentResult>(actionResult_NoModel);

            // 204: the weather file is not found.
            IActionResult actionResult_NoEPW = await SolarController_Controller(SolarController_ScriptedWebApi(buildingModel, [buildingModel], null)).GetRadiationByBuildingModelIdAsync(7, 1465, null);
            Assert.IsType<NoContentResult>(actionResult_NoEPW);

            // 422: a model in local coordinates cannot be located in Poland, so the sun cannot be positioned.
            BuildingModel buildingModel_Local = SolarFixture_BuildingModel(SolarFixture_BoxComponents(0, 0, 10), new Point3D(5, 5, 5));
            SolarController_AssertStatus(await SolarController_Controller(SolarController_ScriptedWebApi(buildingModel_Local, [buildingModel_Local], ePWFile)).GetRadiationByBuildingModelIdAsync(7, 1465, null), StatusCodes.Status422UnprocessableEntity);

            // 422: a degenerate model (two walls and a floor) has no external envelope.
            List<IComponent> components = SolarFixture_BoxComponents(SolarFixture_Origin.X, SolarFixture_Origin.Y, 10);
            BuildingModel buildingModel_Degenerate = SolarFixture_BuildingModel([components[0], components[1], components[3]], location);
            SolarController_AssertStatus(await SolarController_Controller(SolarController_ScriptedWebApi(buildingModel_Degenerate, [buildingModel_Degenerate], ePWFile)).GetRadiationByBuildingModelIdAsync(7, 1465, null), StatusCodes.Status422UnprocessableEntity);

            // 413: a south wall of 100 strips makes 104 receivers, above the synchronous limit; refused before the neighbours are fetched.
            BuildingModel buildingModel_Strips = SolarFixture_BuildingModel(SolarFixture_BoxComponents(SolarFixture_Origin.X, SolarFixture_Origin.Y, 10, false, 100), location);
            ScriptedWebApi scriptedWebApi_Strips = SolarController_ScriptedWebApi(buildingModel_Strips, [buildingModel_Strips], ePWFile);
            SolarController_AssertStatus(await SolarController_Controller(scriptedWebApi_Strips).GetRadiationByBuildingModelIdAsync(7, 1465, null), StatusCodes.Status413PayloadTooLarge);
            Assert.DoesNotContain(scriptedWebApi_Strips.Requests, x => x.RequestUri!.AbsolutePath.EndsWith("/gis/buildingmodel/itemsbycircle", StringComparison.OrdinalIgnoreCase));

            // 413: a neighbour whose roof alone triangulates into more triangles than the caster limit. The polygon is
            // wide enough (1 km) that its corners stay apart from the chords between their neighbours by more than the
            // distance tolerance; on a small radius they would be merged as collinear and the roof would collapse.
            BuildingModel buildingModel_Caster = SolarFixture_BuildingModel([SolarFixture_PolygonRoof(new Point3D(SolarFixture_Origin.X + 1100, SolarFixture_Origin.Y + 5, 12), 1000, Constants.Default.SolarCasterTriangleCountMax + 100)], new Point3D(SolarFixture_Origin.X + 1100, SolarFixture_Origin.Y + 5, 6));
            SolarController_AssertStatus(await SolarController_Controller(SolarController_ScriptedWebApi(buildingModel, [buildingModel, buildingModel_Caster], ePWFile)).GetRadiationByBuildingModelIdAsync(7, 1465, null), StatusCodes.Status413PayloadTooLarge);

            // 502: the neighbour request fails; the building is never solved without its surroundings.
            SolarController_AssertStatus(await SolarController_Controller(SolarController_ScriptedWebApi(buildingModel, [buildingModel], ePWFile, HttpStatusCode.InternalServerError)).GetRadiationByBuildingModelIdAsync(7, 1465, null), StatusCodes.Status502BadGateway);

            // 502: an empty neighbour answer cannot be right either, since the circle contains the building itself.
            SolarController_AssertStatus(await SolarController_Controller(SolarController_ScriptedWebApi(buildingModel, [], ePWFile)).GetRadiationByBuildingModelIdAsync(7, 1465, null), StatusCodes.Status502BadGateway);
        }

        private static void SolarController_AssertStatus(IActionResult actionResult, int statusCode)
        {
            ObjectResult objectResult = Assert.IsAssignableFrom<ObjectResult>(actionResult);
            Assert.Equal(statusCode, objectResult.StatusCode);

            // Every refusal carries a message for the viewer to show.
            List<string> messages = Assert.IsType<List<string>>(objectResult.Value);
            Assert.False(string.IsNullOrWhiteSpace(Assert.Single(messages)));
        }
    }
}
