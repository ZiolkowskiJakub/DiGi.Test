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

        /// <summary>
        /// Generates the gis document from the real controllers through the host's own Swagger configuration - with <c>ParameterMinimumSchemaFilter</c> registered the way the host discovers it for a loaded extension assembly - and asserts that a query parameter whose action guard already refuses a value below a floor carries that floor as <c>minimum</c>, with no <c>maximum</c>, and that a parameter without such a guard carries neither (ZiolkowskiJakub/DiGi.GIS.WebAPI#47).
        /// <para>This is the served contract, not the attribute: the filter turns <c>[Minimum]</c> into <c>minimum</c> and <c>exclusiveMinimum</c> the way Swashbuckle turns <c>[Range]</c> into <c>maximum</c>, so a fact that only inspected the attributes would still pass if that mapping changed. The unguarded <c>commandtimeout</c> endpoints are asserted unbounded beside the guarded ones, so over-marking is caught beside under-marking.</para>
        /// </summary>
        [Fact]
        public void GisDocument_FlooredQueryParameters()
        {
            OpenApiDocument openApiDocument = SchemaGeneratorFixture_Document(
                [
                    typeof(AdministrativeAreal2DController),
                    typeof(BuildingController),
                    typeof(Building2DController),
                    typeof(BuildingDataController),
                    typeof(BuildingModelController),
                    typeof(OccupancyDataController),
                    typeof(OrtoDatasController),
                    typeof(TerrainController),
                    typeof(YearBuiltDataController)
                ],
                types_SchemaFilter: [typeof(ParameterMinimumSchemaFilter)]);

            // commandTimeout: floor 0 where the action guards it - and neither bound where it does not.
            OpenApiOperation openApiOperation_BuildingCount = GisDocument_Operation(openApiDocument, "/gis/building/count", HttpMethod.Get);
            Assert.Equal("0", GisDocument_Minimum(openApiOperation_BuildingCount, "commandtimeout"));
            Assert.Null(GisDocument_Maximum(openApiOperation_BuildingCount, "commandtimeout"));
            Assert.Null(GisDocument_Minimum(openApiOperation_BuildingCount, "countyid"));

            OpenApiOperation openApiOperation_ReferencesByCountyId = GisDocument_Operation(openApiDocument, "/gis/building2d/referencesbycountyid", HttpMethod.Get);
            Assert.Equal("0", GisDocument_Minimum(openApiOperation_ReferencesByCountyId, "commandtimeout"));
            Assert.Null(GisDocument_Maximum(openApiOperation_ReferencesByCountyId, "commandtimeout"));

            OpenApiOperation openApiOperation_BuildingDataCount = GisDocument_Operation(openApiDocument, "/gis/buildingdata/countbycountyid", HttpMethod.Get);
            Assert.Equal("0", GisDocument_Minimum(openApiOperation_BuildingDataCount, "commandtimeout"));
            Assert.Null(GisDocument_Minimum(openApiOperation_BuildingDataCount, "countyid"));

            OpenApiOperation openApiOperation_BuildingModelCount = GisDocument_Operation(openApiDocument, "/gis/buildingmodel/countbycountyid", HttpMethod.Get);
            Assert.Equal("0", GisDocument_Minimum(openApiOperation_BuildingModelCount, "commandtimeout"));

            OpenApiOperation openApiOperation_TerrainSummaries = GisDocument_Operation(openApiDocument, "/gis/terrain/summariesbycountyids", HttpMethod.Get);
            Assert.Null(GisDocument_Minimum(openApiOperation_TerrainSummaries, "commandtimeout"));
            Assert.Null(GisDocument_Maximum(openApiOperation_TerrainSummaries, "commandtimeout"));

            OpenApiOperation openApiOperation_TerrainCount = GisDocument_Operation(openApiDocument, "/gis/terrain/countbycountyid", HttpMethod.Get);
            Assert.Equal("0", GisDocument_Minimum(openApiOperation_TerrainCount, "commandtimeout"));
            Assert.Null(GisDocument_Minimum(openApiOperation_TerrainCount, "countyid"));

            OpenApiOperation openApiOperation_RandomReference = GisDocument_Operation(openApiDocument, "/gis/ortodatas/randombuilding2dreference", HttpMethod.Get);
            Assert.Equal("0", GisDocument_Minimum(openApiOperation_RandomReference, "commandtimeout"));

            OpenApiOperation openApiOperation_OrtoDatasCount = GisDocument_Operation(openApiDocument, "/gis/ortodatas/countbycountyid", HttpMethod.Get);
            Assert.Equal("0", GisDocument_Minimum(openApiOperation_OrtoDatasCount, "commandtimeout"));
            Assert.Null(GisDocument_Minimum(openApiOperation_OrtoDatasCount, "countyid"));

            // limit: floor 1 on the duplicates endpoints, floor 0 on the lattice endpoints.
            OpenApiOperation openApiOperation_ReferenceDuplicates = GisDocument_Operation(openApiDocument, "/gis/building2d/referenceduplicates", HttpMethod.Get);
            Assert.Equal("1", GisDocument_Minimum(openApiOperation_ReferenceDuplicates, "limit"));
            Assert.Equal("0", GisDocument_Minimum(openApiOperation_ReferenceDuplicates, "commandtimeout"));

            OpenApiOperation openApiOperation_DuplicateReferences = GisDocument_Operation(openApiDocument, "/gis/buildingdata/duplicatereferences", HttpMethod.Get);
            Assert.Equal("1", GisDocument_Minimum(openApiOperation_DuplicateReferences, "limit"));

            OpenApiOperation openApiOperation_OccupancyDuplicates = GisDocument_Operation(openApiDocument, "/gis/occupancydata/building2d/duplicatereferences", HttpMethod.Get);
            Assert.Equal("1", GisDocument_Minimum(openApiOperation_OccupancyDuplicates, "limit"));

            OpenApiOperation openApiOperation_YearBuiltDuplicates = GisDocument_Operation(openApiDocument, "/gis/yearbuiltdata/referenceduplicates", HttpMethod.Get);
            Assert.Equal("1", GisDocument_Minimum(openApiOperation_YearBuiltDuplicates, "limit"));
            Assert.Null(GisDocument_Minimum(openApiOperation_YearBuiltDuplicates, "countyid"));

            // gridSize: the lattice floor on the two lattice endpoints, an exclusive zero on the density endpoint.
            string gridSize_Minimum = DiGi.GIS.WebAPI.Constants.Terrain.MinimumGridSize.ToString(CultureInfo.InvariantCulture);
            OpenApiOperation openApiOperation_Coverage = GisDocument_Operation(openApiDocument, "/gis/terrain/coveragebycountyid", HttpMethod.Get);
            Assert.Equal(gridSize_Minimum, GisDocument_Minimum(openApiOperation_Coverage, "gridsize"));
            Assert.Null(GisDocument_Maximum(openApiOperation_Coverage, "gridsize"));
            Assert.Equal("0", GisDocument_Minimum(openApiOperation_Coverage, "limit"));
            Assert.Null(GisDocument_Minimum(openApiOperation_Coverage, "commandtimeout"));

            OpenApiOperation openApiOperation_Gaps = GisDocument_Operation(openApiDocument, "/gis/terrain/gapsbyboundingbox", HttpMethod.Get);
            Assert.Equal(gridSize_Minimum, GisDocument_Minimum(openApiOperation_Gaps, "gridsize"));
            Assert.Equal("0", GisDocument_Minimum(openApiOperation_Gaps, "limit"));
            Assert.Null(GisDocument_Minimum(openApiOperation_Gaps, "commandtimeout"));

            OpenApiOperation openApiOperation_Densities = GisDocument_Operation(openApiDocument, "/gis/terrain/densitiesbycountyids", HttpMethod.Get);
            Assert.Equal("0", GisDocument_Minimum(openApiOperation_Densities, "gridsize"));
            Assert.Equal("0", GisDocument_ExclusiveMinimum(openApiOperation_Densities, "gridsize"));
            Assert.Null(GisDocument_Minimum(openApiOperation_Densities, "commandtimeout"));

            // the update queue: count, claimtimeoutminutes and maxattempts carry floor 1; its unguarded commandtimeout carries nothing.
            OpenApiOperation openApiOperation_NextReferences = GisDocument_Operation(openApiDocument, "/gis/ortodatas/nextbuilding2dreferences", HttpMethod.Post);
            Assert.Equal("1", GisDocument_Minimum(openApiOperation_NextReferences, "count"));
            Assert.Equal("1", GisDocument_Minimum(openApiOperation_NextReferences, "claimtimeoutminutes"));
            Assert.Equal("1", GisDocument_Minimum(openApiOperation_NextReferences, "maxattempts"));
            Assert.Null(GisDocument_Minimum(openApiOperation_NextReferences, "commandtimeout"));

            // positive-identifier selectors: floor 1 exactly where the action guards it.
            OpenApiOperation openApiOperation_ReferenceById = GisDocument_Operation(openApiDocument, "/gis/administrativeareal2d/administrativeareal2Dreferencebyid", HttpMethod.Get);
            Assert.Equal("1", GisDocument_Minimum(openApiOperation_ReferenceById, "id"));
            Assert.Null(GisDocument_Maximum(openApiOperation_ReferenceById, "id"));

            OpenApiOperation openApiOperation_Point2Ds = GisDocument_Operation(openApiDocument, "/gis/building2d/point2dsbyadministrativeareal2Did", HttpMethod.Get);
            Assert.Equal("1", GisDocument_Minimum(openApiOperation_Point2Ds, "administrativeareal2Did"));

            OpenApiOperation openApiOperation_Building2DReferencesByCountyId = GisDocument_Operation(openApiDocument, "/gis/building2d/referencesbycountyid", HttpMethod.Get);
            Assert.Equal("1", GisDocument_Minimum(openApiOperation_Building2DReferencesByCountyId, "countyid"));
            Assert.Equal("1", GisDocument_Minimum(openApiOperation_Building2DReferencesByCountyId, "subdivisionid"));

            OpenApiOperation openApiOperation_YearBuiltReferencesByCountyId = GisDocument_Operation(openApiDocument, "/gis/yearbuiltdata/referencesbycountyid", HttpMethod.Get);
            Assert.Equal("1", GisDocument_Minimum(openApiOperation_YearBuiltReferencesByCountyId, "countyid"));

            OpenApiOperation openApiOperation_OrtoDatasReferenceByReference = GisDocument_Operation(openApiDocument, "/gis/ortodatas/ortodatasreferencebyreference", HttpMethod.Get);
            Assert.Equal("1", GisDocument_Minimum(openApiOperation_OrtoDatasReferenceByReference, "countyid"));

            OpenApiOperation openApiOperation_OrtoDatasReferencesByCountyId = GisDocument_Operation(openApiDocument, "/gis/ortodatas/ortodatasreferencesbycountyid", HttpMethod.Get);
            Assert.Equal("1", GisDocument_Minimum(openApiOperation_OrtoDatasReferencesByCountyId, "countyid"));

            // the one parameter with a real ceiling: samplecount carries both bounds through the built-in [Range].
            OpenApiOperation openApiOperation_SubdivisionLinks = GisDocument_Operation(openApiDocument, "/gis/ortodatas/subdivisionlinksbycountyid", HttpMethod.Get);
            Assert.Equal("0", GisDocument_Minimum(openApiOperation_SubdivisionLinks, "samplecount"));
            Assert.Equal(DiGi.GIS.WebAPI.Constants.OrtoDatas.MaximumSampleCount.ToString(CultureInfo.InvariantCulture), GisDocument_Maximum(openApiOperation_SubdivisionLinks, "samplecount"));
            Assert.Null(GisDocument_Minimum(openApiOperation_SubdivisionLinks, "commandtimeout"));
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

        private static string? GisDocument_Minimum(OpenApiOperation openApiOperation, string name)
        {
            OpenApiSchema? openApiSchema = GisDocument_Parameter(openApiOperation, name).Schema as OpenApiSchema;
            Assert.NotNull(openApiSchema);

            return openApiSchema.Minimum;
        }

        private static string? GisDocument_ExclusiveMinimum(OpenApiOperation openApiOperation, string name)
        {
            OpenApiSchema? openApiSchema = GisDocument_Parameter(openApiOperation, name).Schema as OpenApiSchema;
            Assert.NotNull(openApiSchema);

            return openApiSchema.ExclusiveMinimum;
        }
    }
}
