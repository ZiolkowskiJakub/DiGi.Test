using DiGi.Analytical.Building.Classes;
using DiGi.GIS.Classes;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests how a building known by its reference and a point resolves to its identifier and county on a scripted GIS Web API (<see cref="Query.CountyIdsAsync(HttpClient?, double?, double?, System.Threading.CancellationToken)"/> and <see cref="Query.Building2DReferenceByPointAsync(HttpClient?, string?, int?, double?, double?, System.Threading.CancellationToken)"/>).
        /// <para>The county lookup asks for the county covering the point, then for every polygon part of its code, and answers them in ascending order; a point that is missing or not finite, a point outside every county and an answer that is not a JSON array give none. The building is looked up in each part in turn until one answers - not only the part covering the point (Coding - GIS Administrative Data) - and by the reference alone when no part answers; a viewer node reference carrying its county is unwrapped and goes straight to that county.</para>
        /// </summary>
        [Fact]
        public async Task Query_Building2DReferenceByPointAsync()
        {
            // County code 3020 has three parts; the building is filed under 9102 only.
            string json_County = Core.Convert.ToSystem_String(new List<AdministrativeDivision>() { new(Guid.NewGuid(), "C3020", "3020", null, GIS.Enums.AdministrativeDivisionType.county, "County 3020") })!;
            string json_Reference = Core.Convert.ToSystem_String(new PostgreSQL.Classes.Building2DReference() { Id = 42, CountyId = 9102, Reference = "B42" })!;
            string? json_Ids = "[9102, 1465, 3001]";

            ScriptedWebApi scriptedWebApi = new((httpRequestMessage, body) =>
            {
                string path = httpRequestMessage.RequestUri!.AbsolutePath;
                Dictionary<string, Microsoft.Extensions.Primitives.StringValues> query = QueryHelpers.ParseQuery(httpRequestMessage.RequestUri.Query);

                string? json = null;
                if (path.EndsWith("/gis/administrativeareal2D/itemsbypoint", StringComparison.OrdinalIgnoreCase))
                {
                    json = query["x"] == "1" ? json_County : null;
                }
                else if (path.EndsWith("/gis/administrativeareal2D/idsbycode", StringComparison.OrdinalIgnoreCase))
                {
                    json = json_Ids;
                }
                else if (path.EndsWith("/gis/building2D/building2Dreferencebyreference", StringComparison.OrdinalIgnoreCase))
                {
                    json = query["reference"] == "B42" && query["countyid"] == "9102" ? json_Reference : null;
                }

                return json is null ? new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent(string.Empty) } : Answer(json);
            });

            HttpClient httpClient = new(scriptedWebApi);

            // County parts of the point, ascending.
            Assert.Equal([1465, 3001, 9102], await httpClient.CountyIdsAsync(1, 2));
            Assert.Null(await httpClient.CountyIdsAsync(5, 2));
            Assert.Null(await httpClient.CountyIdsAsync(null, 2));
            Assert.Null(await httpClient.CountyIdsAsync(double.NaN, 2));
            Assert.Null(await ((HttpClient?)null).CountyIdsAsync(1, 2));

            json_Ids = "not json";
            Assert.Null(await httpClient.CountyIdsAsync(1, 2));
            json_Ids = "[9102, 1465, 3001]";

            // Every part is tried until the one holding the building answers.
            int count = scriptedWebApi.Requests.Count;
            PostgreSQL.Classes.Building2DReference? building2DReference = await httpClient.Building2DReferenceByPointAsync("B42", null, 1, 2);
            Assert.NotNull(building2DReference);
            Assert.Equal(42, building2DReference.Id);
            Assert.Equal(9102, building2DReference.CountyId);
            Assert.Equal(5, scriptedWebApi.Requests.Count - count);

            // A point outside every county: the reference alone is looked up, which this script does not answer.
            Assert.Null(await httpClient.Building2DReferenceByPointAsync("B42", null, 5, 2));

            // A viewer node reference carrying its county goes to that county directly.
            string? reference_Node = PostgreSQL.Create.Reference(SolarFixture_BuildingModelWithReference("B42"), null, 9102)?.ToString();
            Assert.NotNull(reference_Node);
            count = scriptedWebApi.Requests.Count;
            building2DReference = await httpClient.Building2DReferenceByPointAsync(reference_Node, null, null, null);
            Assert.NotNull(building2DReference);
            Assert.Equal(42, building2DReference.Id);
            Assert.Equal(1, scriptedWebApi.Requests.Count - count);

            Assert.Null(await httpClient.Building2DReferenceByPointAsync(null, 9102, 1, 2));
        }

        // A building model carrying the given reference, as the GIS Web API stores it on a model.
        private static BuildingModel SolarFixture_BuildingModelWithReference(string reference)
        {
            BuildingModel buildingModel = new();
            Assert.True(buildingModel.SetValue(Analytical.Enums.BuildingModelParameter.Reference, reference, new Core.Parameter.Classes.SetValueSettings(true, false)));
            return buildingModel;
        }
    }
}
