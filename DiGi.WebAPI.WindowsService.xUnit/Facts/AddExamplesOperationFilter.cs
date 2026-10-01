using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.WebAPI.Classes;
using DiGi.WebAPI.WindowsService.Classes.Filters;
using Microsoft.OpenApi;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Nodes;

namespace DiGi.WebAPI.WindowsService.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Reproduces ZiolkowskiJakub/DiGi.WebAPI.WindowsService#8 on a whole document generated through the host's own Swagger configuration: <see cref="AddExamplesOperationFilter"/> must leave no parameter, request body, or response schema without an example.
        /// <para>The example is asserted on the served shape, not on the filter. A component-typed request body or response is a <c>$ref</c> in the document, so its example can only live on the component; an inline parameter or array schema carries its own. The issue's evidence is that the host did not build at 0.8.8, so no served document had ever carried one.</para>
        /// </summary>
        [Fact]
        public void AddExamplesOperationFilter_Apply()
        {
            OpenApiDocument openApiDocument = SchemaGeneratorFixture_Document([typeof(AdministrativeAreal2DController)], types_OperationFilter: [typeof(AddExamplesOperationFilter)]);

            Assert.NotNull(openApiDocument.Components);
            Assert.NotNull(openApiDocument.Components.Schemas);
            IDictionary<string, IOpenApiSchema> schemas = openApiDocument.Components.Schemas;

            // An inline parameter schema carries its own example.
            OpenApiOperation openApiOperation_ByCode = GisDocument_Operation(openApiDocument, "/gis/administrativeareal2d/administrativeareal2dreferencebycode", HttpMethod.Get);
            OpenApiSchema? openApiSchema_Code = GisDocument_Parameter(openApiOperation_ByCode, "code").Schema as OpenApiSchema;
            Assert.NotNull(openApiSchema_Code);
            Assert.NotNull(openApiSchema_Code.Example);

            // A component-typed response is a $ref, so the example has to land on the component it points to.
            Assert.True(schemas.TryGetValue(nameof(AdministrativeAreal2DReference), out IOpenApiSchema? openApiSchema_Reference));
            Assert.NotNull(openApiSchema_Reference);
            Assert.NotNull(openApiSchema_Reference.Example);

            // ... and so does a component-typed request body.
            Assert.True(schemas.TryGetValue(nameof(AdministrativeAreal2DReferencePathsByNameParameter), out IOpenApiSchema? openApiSchema_RequestBody));
            Assert.NotNull(openApiSchema_RequestBody);
            Assert.NotNull(openApiSchema_RequestBody.Example);

            // An inline array response carries an empty array.
            OpenApiOperation openApiOperation_ByType = GisDocument_Operation(openApiDocument, "/gis/administrativeareal2d/administrativeareal2dreferencesbyadministrativearealtype", HttpMethod.Get);
            Assert.NotNull(openApiOperation_ByType.Responses);
            Assert.True(openApiOperation_ByType.Responses.TryGetValue("200", out IOpenApiResponse? openApiResponse));
            Assert.NotNull(openApiResponse);
            Assert.NotNull(openApiResponse.Content);
            OpenApiSchema? openApiSchema_Array = openApiResponse.Content["application/json"]?.Schema as OpenApiSchema;
            Assert.NotNull(openApiSchema_Array);
            Assert.IsType<JsonArray>(openApiSchema_Array.Example);

            // An enum's example is one of its own values, not the generic string the type dispatch would pick.
            Assert.True(schemas.TryGetValue(nameof(AdministrativeArealType), out IOpenApiSchema? openApiSchema_Enum));
            Assert.NotNull(openApiSchema_Enum);
            Assert.NotNull(openApiSchema_Enum.Example);
            Assert.NotNull(openApiSchema_Enum.Enum);
            Assert.Contains(openApiSchema_Enum.Enum, jsonNode => jsonNode?.GetValue<string>() == openApiSchema_Enum.Example!.GetValue<string>());
        }
    }
}
