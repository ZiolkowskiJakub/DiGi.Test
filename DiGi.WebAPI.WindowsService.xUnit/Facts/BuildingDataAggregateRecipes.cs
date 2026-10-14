using DiGi.GIS.WebAPI.Classes;
using Microsoft.OpenApi;
using System;
using System.Net.Http;

namespace DiGi.WebAPI.WindowsService.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// The served <c>aggregatesummary/singlevalue</c> operation description carries the row-count recipe and a real request/response anchor, so a client does not have to read the column catalogue or the enum source to count rows in one partition (ZiolkowskiJakub/DiGi.GIS.WebAPI#58).
        /// <para>The document is built the same way the served <c>/swagger/gis/swagger.json</c> is - through the host's own <c>ConfigureSchemaGeneration</c> and <c>IncludeAssemblyXmlComments</c> - and the operation's descriptive text is asserted to carry the <c>reference</c> count column, the <c>Count</c> function, and the Łomianki <c>6798</c> anchor.</para>
        /// </summary>
        [Fact]
        public void BuildingData_SinglevalueAggregateSummary_Description_ContainsTheCountRecipe()
        {
            OpenApiDocument openApiDocument = SchemaGeneratorFixture_Document([typeof(BuildingDataController)]);
            OpenApiOperation openApiOperation = GisDocument_Operation(openApiDocument, "/gis/buildingdata/aggregatesummary/singlevalue", HttpMethod.Post);

            string? description = OperationDescription(openApiOperation);
            Assert.False(string.IsNullOrWhiteSpace(description));
            Assert.Contains("reference", description, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Count", description, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("6798", description, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The served <c>aggregatesummary/multivalue</c> operation description states that <c>SplitValueDistribution</c> on a single-value column is the group-by-count primitive and points a distinct-list reader at <c>uniquevalues</c> (ZiolkowskiJakub/DiGi.GIS.WebAPI#58).
        /// </summary>
        [Fact]
        public void BuildingData_MultivalueAggregateSummary_Description_MentionsTheGroupByCountAndUniqueValues()
        {
            OpenApiDocument openApiDocument = SchemaGeneratorFixture_Document([typeof(BuildingDataController)]);
            OpenApiOperation openApiOperation = GisDocument_Operation(openApiDocument, "/gis/buildingdata/aggregatesummary/multivalue", HttpMethod.Post);

            string? description = OperationDescription(openApiOperation);
            Assert.False(string.IsNullOrWhiteSpace(description));
            Assert.Contains("SplitValueDistribution", description, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("group-by", description, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("uniquevalues", description, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The served <c>tablebyfiltergroup</c> operation description scopes the read to a county partition and points a reader at <c>FilterCondition.Value</c> for the per-operator value shapes (ZiolkowskiJakub/DiGi.GIS.WebAPI#58, ZiolkowskiJakub/DiGi.PostgreSQL#13).
        /// </summary>
        [Fact]
        public void BuildingData_TableByFilterGroup_Description_PointsAtFilterConditionValueShapes()
        {
            OpenApiDocument openApiDocument = SchemaGeneratorFixture_Document([typeof(BuildingDataController)]);
            OpenApiOperation openApiOperation = GisDocument_Operation(openApiDocument, "/gis/buildingdata/tablebyfiltergroup", HttpMethod.Post);

            string? description = OperationDescription(openApiOperation);
            Assert.False(string.IsNullOrWhiteSpace(description));
            Assert.Contains("county_id", description, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("FilterCondition", description, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The descriptive text Swashbuckle 10 (OpenAPI 3.1 / Microsoft.OpenApi 2.x) attaches to an operation from the action's XML <c>summary</c> - <c>Summary</c>, falling back to <c>Description</c> in case a build maps the comment there.
        /// </summary>
        /// <param name="openApiOperation">The operation whose descriptive text to read.</param>
        /// <returns>The operation's descriptive text; <c>null</c> when neither field is set.</returns>
        private static string? OperationDescription(OpenApiOperation openApiOperation)
        {
            return openApiOperation.Summary ?? openApiOperation.Description;
        }
    }
}
