using DiGi.GIS.WebAPI.Classes;
using Microsoft.OpenApi;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;

namespace DiGi.WebAPI.WindowsService.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Generates the gis document from the real controllers through the host's own Swagger configuration and asserts that a mandatory query parameter is described as <c>required</c> and that a capped one carries its <c>maximum</c>.
        /// <para>This is the served contract, not the attribute: Swashbuckle reads <c>[BindRequired]</c> off a controller parameter and <c>[Range]</c> off its schema, so a fact that only inspected the attributes would still pass if either mapping changed. The reference-by-code type filter and the circle tolerance are asserted optional in the same document, so over-marking is caught beside under-marking (ZiolkowskiJakub/DiGi.GIS.WebAPI#43).</para>
        /// </summary>
        [Fact]
        public void GisDocument_MandatoryAndBoundedQueryParameters()
        {
            OpenApiDocument openApiDocument = SchemaGeneratorFixture_Document([
                typeof(AdministrativeAreal2DController),
                typeof(BuildingController),
                typeof(TerrainController),
                typeof(UnitController)
            ]);

            OpenApiOperation openApiOperation_Mesh3DByCircle = GisDocument_Operation(openApiDocument, "/gis/terrain/mesh3dbycircle", HttpMethod.Get);

            Assert.True(GisDocument_Parameter(openApiOperation_Mesh3DByCircle, "x").Required);
            Assert.True(GisDocument_Parameter(openApiOperation_Mesh3DByCircle, "y").Required);

            Assert.False(GisDocument_Parameter(openApiOperation_Mesh3DByCircle, "radius").Required);
            Assert.False(GisDocument_Parameter(openApiOperation_Mesh3DByCircle, "diameter").Required);
            Assert.False(GisDocument_Parameter(openApiOperation_Mesh3DByCircle, "tolerance").Required);

            Assert.Equal(DiGi.GIS.WebAPI.Constants.Terrain.MaximumRadius.ToString(CultureInfo.InvariantCulture), GisDocument_Maximum(openApiOperation_Mesh3DByCircle, "radius"));
            Assert.Equal((2 * DiGi.GIS.WebAPI.Constants.Terrain.MaximumRadius).ToString(CultureInfo.InvariantCulture), GisDocument_Maximum(openApiOperation_Mesh3DByCircle, "diameter"));

            OpenApiOperation openApiOperation_Compliance = GisDocument_Operation(openApiDocument, "/gis/unit/compliance", HttpMethod.Get);
            Assert.True(GisDocument_Parameter(openApiOperation_Compliance, "administrativearealtype").Required);

            OpenApiOperation openApiOperation_ReferenceByCode = GisDocument_Operation(openApiDocument, "/gis/administrativeareal2d/administrativeareal2dreferencebycode", HttpMethod.Get);
            Assert.True(GisDocument_Parameter(openApiOperation_ReferenceByCode, "code").Required);
            Assert.False(GisDocument_Parameter(openApiOperation_ReferenceByCode, "administrativearealtype").Required);
        }

        private static OpenApiOperation GisDocument_Operation(OpenApiDocument openApiDocument, string path, HttpMethod httpMethod)
        {
            // The [controller] route token keeps the controller name's casing here, while the served document
            // lowercases it; the lookup takes either.
            string? path_Actual = openApiDocument.Paths.Keys.FirstOrDefault(x => string.Equals(x, path, StringComparison.OrdinalIgnoreCase));
            Assert.False(string.IsNullOrWhiteSpace(path_Actual), path);
            Assert.True(openApiDocument.Paths.TryGetValue(path_Actual!, out IOpenApiPathItem? openApiPathItem), path);

            OpenApiPathItem openApiPathItem_Concrete = Assert.IsAssignableFrom<OpenApiPathItem>(openApiPathItem);
            Assert.NotNull(openApiPathItem_Concrete.Operations);
            Assert.True(openApiPathItem_Concrete.Operations.TryGetValue(httpMethod, out OpenApiOperation? openApiOperation), path);
            Assert.NotNull(openApiOperation);

            return openApiOperation;
        }

        private static IOpenApiParameter GisDocument_Parameter(OpenApiOperation openApiOperation, string name)
        {
            Assert.NotNull(openApiOperation.Parameters);

            IOpenApiParameter? openApiParameter = openApiOperation.Parameters.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(openApiParameter);

            return openApiParameter;
        }

        private static string? GisDocument_Maximum(OpenApiOperation openApiOperation, string name)
        {
            OpenApiSchema? openApiSchema = GisDocument_Parameter(openApiOperation, name).Schema as OpenApiSchema;
            Assert.NotNull(openApiSchema);

            return openApiSchema.Maximum;
        }
    }
}
