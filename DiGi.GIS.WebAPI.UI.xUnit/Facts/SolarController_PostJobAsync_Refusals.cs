using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.EPW.Classes;
using DiGi.GIS.WebAPI.UI.Classes;
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
        /// Tests that posting a background solar radiation job answers the refusals of the synchronous routes on a scripted GIS Web API, with the job ceilings in place of the synchronous ones: 400 for an invalid radius before any request is sent, 204 without a building or weather file, 422 for a building that cannot be located or is degenerate, 502 when the neighbours cannot be read, and 413 only above <see cref="Constants.Default.SolarJobReceiverCountMax"/> receivers or <see cref="Constants.Default.SolarJobCasterTriangleCountMax"/> caster triangles. None of them queues a job.
        /// <para>The 104-receiver building the synchronous routes refuse with 413 is accepted as a job with 202.</para>
        /// </summary>
        [Fact]
        public async Task SolarController_PostJobAsync_Refusals()
        {
            EPWFile ePWFile = SolarFixture_EPWFile();
            Point3D location = new(SolarFixture_Origin.X + 5, SolarFixture_Origin.Y + 5, 5);
            BuildingModel buildingModel = SolarFixture_BuildingModel(SolarFixture_BoxComponents(SolarFixture_Origin.X, SolarFixture_Origin.Y, 10), location);
            SolarJobQueue solarJobQueue = new(TimeProvider.System);

            // 400: an invalid radius is refused before anything is fetched.
            foreach (double radius in new double[] { 0, -5, Constants.Default.SolarSurroundingRadiusMax + 1, double.NaN, double.PositiveInfinity })
            {
                ScriptedWebApi scriptedWebApi_Radius = SolarController_ScriptedWebApi(buildingModel, [buildingModel], ePWFile);
                Assert.IsType<BadRequestObjectResult>(await SolarController_Controller(scriptedWebApi_Radius, null, solarJobQueue).PostJobAsync(7, 1465, radius));
                Assert.Empty(scriptedWebApi_Radius.Requests);
            }

            // 204: no building, no weather file.
            Assert.IsType<NoContentResult>(await SolarController_Controller(SolarController_ScriptedWebApi(null, [buildingModel], ePWFile), null, solarJobQueue).PostJobAsync(7, 1465, null));
            Assert.IsType<NoContentResult>(await SolarController_Controller(SolarController_ScriptedWebApi(buildingModel, [buildingModel], null), null, solarJobQueue).PostJobAsync(7, 1465, null));

            // 422: a model in local coordinates cannot be located; a degenerate model has no external envelope.
            BuildingModel buildingModel_Local = SolarFixture_BuildingModel(SolarFixture_BoxComponents(0, 0, 10), new Point3D(5, 5, 5));
            SolarController_AssertStatus(await SolarController_Controller(SolarController_ScriptedWebApi(buildingModel_Local, [buildingModel_Local], ePWFile), null, solarJobQueue).PostJobAsync(7, 1465, null), StatusCodes.Status422UnprocessableEntity);

            List<IComponent> components = SolarFixture_BoxComponents(SolarFixture_Origin.X, SolarFixture_Origin.Y, 10);
            BuildingModel buildingModel_Degenerate = SolarFixture_BuildingModel([components[0], components[1], components[3]], location);
            SolarController_AssertStatus(await SolarController_Controller(SolarController_ScriptedWebApi(buildingModel_Degenerate, [buildingModel_Degenerate], ePWFile), null, solarJobQueue).PostJobAsync(7, 1465, null), StatusCodes.Status422UnprocessableEntity);

            // 502: a failed or empty neighbour answer.
            SolarController_AssertStatus(await SolarController_Controller(SolarController_ScriptedWebApi(buildingModel, [buildingModel], ePWFile, HttpStatusCode.InternalServerError), null, solarJobQueue).PostJobAsync(7, 1465, null), StatusCodes.Status502BadGateway);
            SolarController_AssertStatus(await SolarController_Controller(SolarController_ScriptedWebApi(buildingModel, [], ePWFile), null, solarJobQueue).PostJobAsync(7, 1465, null), StatusCodes.Status502BadGateway);

            // 413 above the job receiver ceiling, refused before the neighbours are fetched.
            BuildingModel buildingModel_Strips_Max = SolarFixture_BuildingModel(SolarFixture_BoxComponents(SolarFixture_Origin.X, SolarFixture_Origin.Y, 10, false, Constants.Default.SolarJobReceiverCountMax), location);
            ScriptedWebApi scriptedWebApi_Strips_Max = SolarController_ScriptedWebApi(buildingModel_Strips_Max, [buildingModel_Strips_Max], ePWFile);
            SolarController_AssertStatus(await SolarController_Controller(scriptedWebApi_Strips_Max, null, solarJobQueue).PostJobAsync(7, 1465, null), StatusCodes.Status413PayloadTooLarge);
            Assert.DoesNotContain(scriptedWebApi_Strips_Max.Requests, x => x.RequestUri!.AbsolutePath.EndsWith("/gis/buildingmodel/itemsbycircle", StringComparison.OrdinalIgnoreCase));

            // 413 above the job caster triangle ceiling (the neighbour's roof alone; see the synchronous fact for its size).
            BuildingModel buildingModel_Caster = SolarFixture_BuildingModel([SolarFixture_PolygonRoof(new Point3D(SolarFixture_Origin.X + 1100, SolarFixture_Origin.Y + 5, 12), 1000, Constants.Default.SolarJobCasterTriangleCountMax + 100)], new Point3D(SolarFixture_Origin.X + 1100, SolarFixture_Origin.Y + 5, 6));
            SolarController_AssertStatus(await SolarController_Controller(SolarController_ScriptedWebApi(buildingModel, [buildingModel, buildingModel_Caster], ePWFile), null, solarJobQueue).PostJobAsync(7, 1465, null), StatusCodes.Status413PayloadTooLarge);

            // No refusal queued anything.
            Assert.Null(solarJobQueue.QueuePosition(Guid.Empty));
            Assert.True(solarJobQueue.TryEnqueue(SolarJobFixture_Job(() => [])));
            Assert.Equal(1, solarJobQueue.QueuePosition(Assert.Single(solarJobQueue.SolarJobs.Keys)));

            // 202: the 104-receiver building above the synchronous ceiling (413 on request) is a job.
            BuildingModel buildingModel_Strips = SolarFixture_BuildingModel(SolarFixture_BoxComponents(SolarFixture_Origin.X, SolarFixture_Origin.Y, 10, false, 100), location);
            ScriptedWebApi scriptedWebApi_Strips = SolarController_ScriptedWebApi(buildingModel_Strips, [buildingModel_Strips], ePWFile);
            SolarController_AssertStatus(await SolarController_Controller(scriptedWebApi_Strips).GetRadiationByBuildingModelIdAsync(7, 1465, null), StatusCodes.Status413PayloadTooLarge);
            AcceptedResult acceptedResult = Assert.IsType<AcceptedResult>(await SolarController_Controller(scriptedWebApi_Strips, null, solarJobQueue).PostJobAsync(7, 1465, null));
            Assert.Equal(104, Assert.IsType<ViewModels.SolarJobViewModel>(acceptedResult.Value).ReceiverCount);
        }
    }
}
