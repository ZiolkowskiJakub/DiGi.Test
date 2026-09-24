using DiGi.Core.IO.Table.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the paged read of one county part, <see cref="Query.BuildingDataTableAsync(HttpClient?, int, List{string}, System.Action{string}?, int, int, System.Threading.CancellationToken)"/>, against a scripted GIS Web API (DiGi.GIS.WebAPI.UI#51).
        /// <para><b>Physical order.</b> Every request asks for <c>PhysicalOrder</c>. The read follows the <c>DiGi-Next-Cursor</c> header from page to page, sending it back as <c>Cursor</c>, and stops on the first response without it. A building repeated across pages - a row rewritten while its part was paged - is kept once.</para>
        /// <para><b>No reference-order fallback.</b> A full page without the header still ends the part: the read never continues from the last row's reference (DiGi.GIS.WebAPI.UI#54).</para>
        /// <para><b>Failures.</b> A physical cursor that does not move fails the part rather than looping, and an upstream 404 is a part with no building data - an empty table.</para>
        /// </summary>
        [Fact]
        public async Task Query_BuildingDataTableAsync_PhysicalOrder()
        {
            List<string> columnUniqueIds = ["floor_area", "reference"];

            // ----- physical order: header to header, a repeated building kept once -----
            List<(string[] References, string? Cursor)> pages_Physical =
            [
                (["R1", "R2", "R3"], "(0,4)"),
                (["R3", "R4"], "(1,2)"),
                (["R5"], null),
            ];
            int index_Physical = 0;
            ScriptedWebApi scriptedWebApi_Physical = new((request, body) => Page(pages_Physical[index_Physical].References, pages_Physical[index_Physical++].Cursor));

            Table? table_Physical = await new HttpClient(scriptedWebApi_Physical).BuildingDataTableAsync(55417, columnUniqueIds);
            Assert.NotNull(table_Physical);
            Assert.Equal(["R1", "R2", "R3", "R4", "R5"], References(table_Physical));
            Assert.Equal(3, scriptedWebApi_Physical.Requests.Count);

            List<JsonObject> bodies_Physical = [.. scriptedWebApi_Physical.Requests.Select(x => JsonNode.Parse(x.Body!)!.AsObject())];
            Assert.All(bodies_Physical, x => Assert.True(x["PhysicalOrder"]!.GetValue<bool>()));
            Assert.All(bodies_Physical, x => Assert.Equal(55417, x["CountyId"]!.GetValue<int>()));
            Assert.All(bodies_Physical, x => Assert.Equal(Constants.Default.BuildingDataPageSize, x["PageSize"]!.GetValue<int>()));
            Assert.Null(bodies_Physical[0]["Cursor"]);
            Assert.Equal("(0,4)", bodies_Physical[1]["Cursor"]!.GetValue<string>());
            Assert.Equal("(1,2)", bodies_Physical[2]["Cursor"]!.GetValue<string>());
            Assert.All(scriptedWebApi_Physical.Requests, x => Assert.EndsWith("/gis/BuildingData/tablebybuildingdatabypagingparameter", x.RequestUri!.AbsolutePath));

            // ----- a full headerless page ends the part: no reference-order continuation -----
            string[] references_Full = [.. Enumerable.Range(0, Constants.Default.BuildingDataPageSize).Select(i => $"S{i:D5}")];
            ScriptedWebApi scriptedWebApi_Full = new((request, body) => Page(references_Full, null));

            Table? table_Full = await new HttpClient(scriptedWebApi_Full).BuildingDataTableAsync(55417, columnUniqueIds);
            Assert.NotNull(table_Full);
            Assert.Equal(Constants.Default.BuildingDataPageSize, table_Full.RowCount);
            Assert.Single(scriptedWebApi_Full.Requests);

            // ----- a physical cursor that does not move fails the part instead of looping -----
            ScriptedWebApi scriptedWebApi_Stuck = new((request, body) => Page(["U1"], "(0,4)"));
            Assert.Null(await new HttpClient(scriptedWebApi_Stuck).BuildingDataTableAsync(55417, columnUniqueIds));
            Assert.Equal(2, scriptedWebApi_Stuck.Requests.Count);

            // ----- an upstream 404 is a part with no building data -----
            ScriptedWebApi scriptedWebApi_Missing = new((request, body) => new HttpResponseMessage(HttpStatusCode.NotFound));
            Table? table_Missing = await new HttpClient(scriptedWebApi_Missing).BuildingDataTableAsync(55417, columnUniqueIds);
            Assert.NotNull(table_Missing);
            Assert.Equal(0, table_Missing.RowCount);

            // One page of the upstream's Table JSON (Reference, County Id), with the next physical cursor when given.
            static HttpResponseMessage Page(string[] references, string? cursor)
            {
                StringBuilder stringBuilder = new();
                stringBuilder.Append("{\"Columns\":[");
                stringBuilder.Append("{\"_type\":\"DiGi.Core.IO.Table.Classes.ExtendedColumn,DiGi.Core.IO\",\"Name\":\"Reference\",\"Type\":\"System.String,System.Private.CoreLib\",\"Index\":0},");
                stringBuilder.Append("{\"_type\":\"DiGi.Core.IO.Table.Classes.ExtendedColumn,DiGi.Core.IO\",\"Name\":\"County Id\",\"Type\":\"System.Int32,System.Private.CoreLib\",\"Index\":1}");
                stringBuilder.Append("],\"Rows\":[");
                stringBuilder.Append(string.Join(",", references.Select(x => $"[\"{x}\",55417]")));
                stringBuilder.Append("]}");

                HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK) { Content = new StringContent(stringBuilder.ToString(), Encoding.UTF8, "application/json") };
                if (cursor is not null)
                {
                    httpResponseMessage.Headers.Add(Constants.Default.NextCursorHeaderName, cursor);
                }

                return httpResponseMessage;
            }

            static List<string> References(Table table)
            {
                int index = table.GetColumnIndex("Reference");
                return [.. table.Select(x => (string)x[index]!)];
            }
        }
    }
}
