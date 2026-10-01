using DiGi.Core.IO.Table.Classes;
using DiGi.Core.IO.Table.Interfaces;
using DiGi.GIS.WebAPI.Classes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DiGi.WebAPI.WindowsService.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that the schema the host generates for a Core.IO table through the extension's <see cref="TableWireFormatSchemaFilter"/> describes the wire format <c>Core.IO.Table.Convert.ToSystem_String</c> writes: an object requiring exactly <c>Columns</c> and <c>Rows</c> with no root <c>_type</c>, open column objects discriminated by <c>_type</c>, and rows as positional arrays (ZiolkowskiJakub/DiGi.GIS.WebAPI#44).
        /// </summary>
        [Fact]
        public void TableWireFormatSchemaFilter()
        {
            OpenApiSchema openApiSchema = SchemaGeneratorFixture_Schema(typeof(Table), new SchemaRepository(), [typeof(TableWireFormatSchemaFilter)]);

            Assert.Equal(JsonSchemaType.Object, openApiSchema.Type);

            Assert.NotNull(openApiSchema.Properties);
            Assert.Equal([nameof(ITable<,>.Columns), nameof(ITable<,>.Rows)], openApiSchema.Properties.Keys.Order(StringComparer.Ordinal));
            Assert.DoesNotContain(Core.Constants.Serialization.PropertyName.Type, openApiSchema.Properties.Keys);

            Assert.NotNull(openApiSchema.Required);
            Assert.Equal([nameof(ITable<,>.Columns), nameof(ITable<,>.Rows)], openApiSchema.Required.Order(StringComparer.Ordinal));

            Assert.False(openApiSchema.AdditionalPropertiesAllowed);

            OpenApiSchema? openApiSchema_Columns = openApiSchema.Properties[nameof(ITable<,>.Columns)] as OpenApiSchema;
            Assert.NotNull(openApiSchema_Columns);
            Assert.Equal(JsonSchemaType.Array, openApiSchema_Columns.Type);

            OpenApiSchema? openApiSchema_Column = openApiSchema_Columns.Items as OpenApiSchema;
            Assert.NotNull(openApiSchema_Column);
            Assert.Equal(JsonSchemaType.Object, openApiSchema_Column.Type);
            Assert.NotNull(openApiSchema_Column.Properties);
            Assert.Equal([Core.Constants.Serialization.PropertyName.Type], openApiSchema_Column.Properties.Keys);
            Assert.NotNull(openApiSchema_Column.Required);
            Assert.Equal([Core.Constants.Serialization.PropertyName.Type], openApiSchema_Column.Required);
            Assert.Equal(JsonSchemaType.String, openApiSchema_Column.Properties[Core.Constants.Serialization.PropertyName.Type].Type);
            Assert.True(openApiSchema_Column.AdditionalPropertiesAllowed);

            OpenApiSchema? openApiSchema_Rows = openApiSchema.Properties[nameof(ITable<,>.Rows)] as OpenApiSchema;
            Assert.NotNull(openApiSchema_Rows);
            Assert.Equal(JsonSchemaType.Array, openApiSchema_Rows.Type);

            OpenApiSchema? openApiSchema_Row = openApiSchema_Rows.Items as OpenApiSchema;
            Assert.NotNull(openApiSchema_Row);
            Assert.Equal(JsonSchemaType.Array, openApiSchema_Row.Type);

            OpenApiSchema? openApiSchema_Cell = openApiSchema_Row.Items as OpenApiSchema;
            Assert.NotNull(openApiSchema_Cell);
            Assert.Null(openApiSchema_Cell.Type);
        }

        /// <summary>
        /// Pins the table response schema against the serializer the operations use, so the two cannot drift apart: a table serialized by <c>Core.IO.Table.Convert.ToSystem_String&lt;Table, Column, Row&gt;</c> - the exact call every <c>buildingdata/table*</c> action makes - conforms strictly (required present, no property the schema does not declare) to the generated schema, and does not conform to what the host generates without the filter.
        /// <para>The payload side is asserted directly too: exactly <c>Columns</c> and <c>Rows</c> at the root, a <c>_type</c> on every column, and one value per column in every row, <c>null</c> included.</para>
        /// </summary>
        [Fact]
        public void TableWireFormatSchemaFilter_Payload()
        {
            ExtendedColumn extendedColumn = new("Reference", typeof(string), "Administrative", "The building's unique reference.");
            Column column = new("County Id", typeof(int));

            Table table = new([extendedColumn, column]);
            Assert.NotNull(table.AddRow(["a71b3f91-819f-489a-93c3-a850948c60af", 10365]));
            Assert.NotNull(table.AddRow([null, 10366]));

            string? json = Core.IO.Table.Convert.ToSystem_String<Table, Column, Row>(table);
            Assert.False(string.IsNullOrWhiteSpace(json));

            JsonNode? jsonNode = JsonNode.Parse(json!);
            Assert.NotNull(jsonNode);

            JsonObject? jsonObject = jsonNode as JsonObject;
            Assert.NotNull(jsonObject);

            Assert.Equal([nameof(ITable<,>.Columns), nameof(ITable<,>.Rows)], jsonObject.Select(keyValuePair => keyValuePair.Key).Order(StringComparer.Ordinal));

            JsonArray? jsonArray_Columns = jsonObject[nameof(ITable<,>.Columns)] as JsonArray;
            Assert.NotNull(jsonArray_Columns);
            Assert.Equal(2, jsonArray_Columns.Count);

            foreach (JsonNode? jsonNode_Column in jsonArray_Columns)
            {
                JsonObject? jsonObject_Column = jsonNode_Column as JsonObject;
                Assert.NotNull(jsonObject_Column);

                string? name_Type = jsonObject_Column[Core.Constants.Serialization.PropertyName.Type]?.GetValue<string>();
                Assert.False(string.IsNullOrWhiteSpace(name_Type));
            }

            Assert.Equal("DiGi.Core.IO.Table.Classes.ExtendedColumn,DiGi.Core.IO", jsonArray_Columns[0]?[Core.Constants.Serialization.PropertyName.Type]?.GetValue<string>());
            Assert.Equal("DiGi.Core.IO.Table.Classes.Column,DiGi.Core.IO", jsonArray_Columns[1]?[Core.Constants.Serialization.PropertyName.Type]?.GetValue<string>());
            Assert.Equal("Administrative", jsonArray_Columns[0]?["Category"]?.GetValue<string>());
            Assert.Equal("The building's unique reference.", jsonArray_Columns[0]?["Description"]?.GetValue<string>());

            JsonArray? jsonArray_Rows = jsonObject[nameof(ITable<,>.Rows)] as JsonArray;
            Assert.NotNull(jsonArray_Rows);
            Assert.Equal(2, jsonArray_Rows.Count);

            foreach (JsonNode? jsonNode_Row in jsonArray_Rows)
            {
                JsonArray? jsonArray_Row = jsonNode_Row as JsonArray;
                Assert.NotNull(jsonArray_Row);
                Assert.Equal(jsonArray_Columns.Count, jsonArray_Row.Count);
            }

            Assert.Equal("a71b3f91-819f-489a-93c3-a850948c60af", jsonArray_Rows[0]?[0]?.GetValue<string>());
            Assert.Equal(10365, jsonArray_Rows[0]?[1]?.GetValue<int>());
            Assert.Null(jsonArray_Rows[1]?[0]);
            Assert.Equal(10366, jsonArray_Rows[1]?[1]?.GetValue<int>());

            // Without the extension's filter the host describes the type's public members (the MVC writer's shape, not the
            // TableConverter's), so the payload the operations serve does not conform to it - the differential proves it is
            // the filter that makes the served schema describe the served wire format.
            OpenApiSchema openApiSchema_HostOnly = SchemaGeneratorFixture_Schema(typeof(Table), new SchemaRepository());
            List<string> errors_HostOnly = [];
            Assert.False(TableWireFormatSchemaFilter_Conforms(jsonObject, openApiSchema_HostOnly, errors_HostOnly), $"The payload conforms to the host-only schema, which the wire format contradicts: {string.Join("; ", errors_HostOnly)}");

            OpenApiSchema openApiSchema = SchemaGeneratorFixture_Schema(typeof(Table), new SchemaRepository(), [typeof(TableWireFormatSchemaFilter)]);
            List<string> errors = [];
            Assert.True(TableWireFormatSchemaFilter_Conforms(jsonObject, openApiSchema, errors), string.Join("; ", errors));
        }

        /// <summary>
        /// Tests that generating the schemas of the two column types one gis document declares together - <see cref="List{T}"/> of the PostgreSQL <c>Column</c> served by <c>buildingdata/columns*</c>, and the Core.IO <see cref="Table"/> served by <c>buildingdata/table*</c> - never registers a component for the Core.IO column: the <c>Column</c> schema id is the PostgreSQL column's, and the table's column items stay inline, in either generation order.
        /// <para>Swashbuckle ids both types' columns <c>Column</c>, so a component registration for the Core.IO column would conflict with - or silently reuse - the PostgreSQL one and break the whole document.</para>
        /// </summary>
        [Fact]
        public void TableWireFormatSchemaFilter_ColumnComponentIds()
        {
            Type type_Columns = typeof(List<DiGi.PostgreSQL.Table.Classes.Column>);
            Type type_Table = typeof(Table);

            List<(Type Type_First, Type Type_Second)> cases =
            [
                (type_Columns, type_Table),
                (type_Table, type_Columns)
            ];

            foreach ((Type Type_First, Type Type_Second) case_Collection in cases)
            {
                SchemaRepository schemaRepository = new();

                // Both types come back inline (an array of columns, the table itself), but generating the list registers the
                // PostgreSQL Column component the columns* endpoints reference.
                _ = SchemaGeneratorFixture_Schema(case_Collection.Type_First, schemaRepository, [typeof(TableWireFormatSchemaFilter)]);

                OpenApiSchema openApiSchema_Table = SchemaGeneratorFixture_Schema(case_Collection.Type_Second, schemaRepository, [typeof(TableWireFormatSchemaFilter)]);
                if (case_Collection.Type_Second != type_Table)
                {
                    openApiSchema_Table = SchemaGeneratorFixture_Schema(type_Table, schemaRepository, [typeof(TableWireFormatSchemaFilter)]);
                }

                OpenApiSchema? openApiSchema_Columns = openApiSchema_Table.Properties?[nameof(ITable<,>.Columns)] as OpenApiSchema;
                Assert.NotNull(openApiSchema_Columns);
                Assert.IsNotType<OpenApiSchemaReference>(openApiSchema_Columns.Items);

                OpenApiSchema? openApiSchema_Column = openApiSchema_Columns.Items as OpenApiSchema;
                Assert.NotNull(openApiSchema_Column);
                Assert.NotNull(openApiSchema_Column.Properties);
                Assert.Equal([Core.Constants.Serialization.PropertyName.Type], openApiSchema_Column.Properties.Keys);

                Assert.True(schemaRepository.TryLookupByType(typeof(DiGi.PostgreSQL.Table.Classes.Column), out OpenApiSchemaReference? openApiSchemaReference_Column));
                Assert.NotNull(openApiSchemaReference_Column);
                Assert.Equal(nameof(DiGi.PostgreSQL.Table.Classes.Column), openApiSchemaReference_Column.Reference.Id);

                Assert.False(schemaRepository.TryLookupByType(typeof(DiGi.Core.IO.Table.Classes.Column), out _));
            }
        }

        /// <summary>
        /// Generates a whole document from the real <see cref="BuildingDataController"/> - every <c>buildingdata/table*</c> operation beside the <c>columns*</c> operations that reference the PostgreSQL <c>Column</c> component, the exact situation of the served gis document - and checks each of the five table responses declares the <c>TableConverter</c> shape through the controller's own <c>ProducesResponseType</c> declarations.
        /// <para>Also checks the document's components: the single <c>Column</c> stays the PostgreSQL column the <c>columns*</c> operations serve, and the Core.IO <c>Row</c> - which generating a table registers as array items and the schema filter then drops - is removed, not left advertising a row object the wire never carries.</para>
        /// </summary>
        [Fact]
        public void TableWireFormatSchemaFilter_Document()
        {
            OpenApiDocument openApiDocument = SchemaGeneratorFixture_Document([typeof(BuildingDataController)], [typeof(TableWireFormatSchemaFilter)], [typeof(TableWireFormatDocumentFilter)]);

            List<(string Path, HttpMethod HttpMethod)> cases =
            [
                ("/gis/buildingdata/tablebybuildingdatabypagingparameter", HttpMethod.Post),
                ("/gis/buildingdata/tablebybuildingdatabyreferencesparameter", HttpMethod.Post),
                ("/gis/buildingdata/tablebybuildingdatabysubdivisionidsparameter", HttpMethod.Post),
                ("/gis/buildingdata/tablebyfiltergroup", HttpMethod.Post),
                ("/gis/buildingdata/tablebyreference", HttpMethod.Get)
            ];

            foreach ((string Path, HttpMethod HttpMethod) case_Operation in cases)
            {
                // The [controller] route token keeps the controller name's casing here, while the served document
                // lowercases it; the lookup takes either.
                string? path_Actual = openApiDocument.Paths.Keys.FirstOrDefault(path => string.Equals(path, case_Operation.Path, StringComparison.OrdinalIgnoreCase));
                Assert.False(string.IsNullOrWhiteSpace(path_Actual), case_Operation.Path);
                Assert.True(openApiDocument.Paths.TryGetValue(path_Actual, out IOpenApiPathItem? openApiPathItem), case_Operation.Path);
                OpenApiPathItem openApiPathItem_Concrete = Assert.IsAssignableFrom<OpenApiPathItem>(openApiPathItem);

                IDictionary<HttpMethod, OpenApiOperation>? operations = openApiPathItem_Concrete.Operations;
                Assert.NotNull(operations);
                Assert.True(operations.TryGetValue(case_Operation.HttpMethod, out OpenApiOperation? openApiOperation), case_Operation.Path);

                IDictionary<string, IOpenApiResponse>? responses = openApiOperation?.Responses;
                Assert.NotNull(responses);
                Assert.True(responses.TryGetValue("200", out IOpenApiResponse? openApiResponse), case_Operation.Path);

                IDictionary<string, OpenApiMediaType>? contents = openApiResponse?.Content;
                Assert.NotNull(contents);
                Assert.True(contents.TryGetValue("application/json", out OpenApiMediaType? openApiMediaType), case_Operation.Path);
                Assert.NotNull(openApiMediaType);

                OpenApiSchema? openApiSchema_Response = TableWireFormatSchemaFilter_Resolve(openApiMediaType.Schema, openApiDocument.Components);
                Assert.NotNull(openApiSchema_Response);

                Assert.Equal(JsonSchemaType.Object, openApiSchema_Response.Type);

                Assert.NotNull(openApiSchema_Response.Properties);
                Assert.Equal([nameof(ITable<,>.Columns), nameof(ITable<,>.Rows)], openApiSchema_Response.Properties.Keys.Order(StringComparer.Ordinal));
                Assert.DoesNotContain(Core.Constants.Serialization.PropertyName.Type, openApiSchema_Response.Properties.Keys);

                Assert.NotNull(openApiSchema_Response.Required);
                Assert.Equal([nameof(ITable<,>.Columns), nameof(ITable<,>.Rows)], openApiSchema_Response.Required.Order(StringComparer.Ordinal));

                Assert.False(openApiSchema_Response.AdditionalPropertiesAllowed);
            }

            OpenApiComponents openApiComponents = Assert.IsAssignableFrom<OpenApiComponents>(openApiDocument.Components);
            IDictionary<string, IOpenApiSchema>? schemas_Component = openApiComponents.Schemas;
            Assert.NotNull(schemas_Component);
            Assert.False(schemas_Component.ContainsKey(nameof(Row)));

            Assert.True(schemas_Component.TryGetValue(nameof(DiGi.PostgreSQL.Table.Classes.Column), out IOpenApiSchema? openApiSchema_Column));
            IDictionary<string, IOpenApiSchema>? properties_Column = openApiSchema_Column.Properties;
            Assert.NotNull(properties_Column);
            Assert.Contains("UniqueId", properties_Column.Keys);
            Assert.Contains("DataType", properties_Column.Keys);
        }

        /// <summary>
        /// Resolves a response schema to the component it references, or returns it unchanged when it is already concrete.
        /// </summary>
        /// <param name="openApiSchema">The schema to resolve.</param>
        /// <param name="openApiComponents">The components carrying the referenced schemas.</param>
        /// <returns>The concrete schema, or null when it cannot be resolved.</returns>
        private static OpenApiSchema? TableWireFormatSchemaFilter_Resolve(IOpenApiSchema? openApiSchema, OpenApiComponents? openApiComponents)
        {
            if (openApiSchema is OpenApiSchemaReference openApiSchemaReference && openApiSchemaReference.Reference.Id is string id)
            {
                IDictionary<string, IOpenApiSchema>? schemas_Component = openApiComponents?.Schemas;
                if (schemas_Component is not null && schemas_Component.TryGetValue(id, out IOpenApiSchema? openApiSchema_Resolved))
                {
                    return openApiSchema_Resolved as OpenApiSchema;
                }
            }

            return openApiSchema as OpenApiSchema;
        }

        /// <summary>
        /// Validates a JSON payload against the keywords the table schema declares - <c>type</c>, <c>properties</c>, <c>required</c>, <c>additionalPropertiesAllowed</c> and <c>items</c> - the way strict JSON Schema validation would, collecting every violation into <paramref name="errors"/>; <c>null</c> items of an array are validated as absent members.
        /// </summary>
        /// <param name="jsonNode">The payload node to validate.</param>
        /// <param name="openApiSchema">The schema to validate against.</param>
        /// <param name="errors">The violations found, one message each.</param>
        /// <returns>True when the payload conforms to the schema.</returns>
        private static bool TableWireFormatSchemaFilter_Conforms(JsonNode? jsonNode, OpenApiSchema? openApiSchema, List<string> errors)
        {
            if (openApiSchema is null)
            {
                errors.Add("Schema is null.");
                return false;
            }

            JsonSchemaType? type = openApiSchema.Type;
            if (type.HasValue)
            {
                bool matchesType = jsonNode switch
                {
                    null => type.Value.HasFlag(JsonSchemaType.Null),
                    JsonArray => type.Value.HasFlag(JsonSchemaType.Array),
                    JsonObject => type.Value.HasFlag(JsonSchemaType.Object),
                    JsonValue jsonValue => jsonValue.GetValueKind() switch
                    {
                        JsonValueKind.String => type.Value.HasFlag(JsonSchemaType.String),
                        JsonValueKind.Number => type.Value.HasFlag(JsonSchemaType.Number) || type.Value.HasFlag(JsonSchemaType.Integer),
                        JsonValueKind.True or JsonValueKind.False => type.Value.HasFlag(JsonSchemaType.Boolean),
                        _ => false
                    },
                    _ => false
                };

                if (!matchesType)
                {
                    errors.Add($"Node does not match schema type {type.Value}.");
                    return false;
                }
            }

            if (jsonNode is JsonObject jsonObject)
            {
                if (openApiSchema.Properties is not null)
                {
                    foreach (string name in openApiSchema.Properties.Keys)
                    {
                        if (!jsonObject.ContainsKey(name) && openApiSchema.Required is not null && openApiSchema.Required.Contains(name))
                        {
                            errors.Add($"'{name}' is a required property.");
                        }
                    }
                }

                if (!openApiSchema.AdditionalPropertiesAllowed && openApiSchema.Properties is not null)
                {
                    foreach (string name in jsonObject.Select(keyValuePair => keyValuePair.Key))
                    {
                        if (!openApiSchema.Properties.ContainsKey(name))
                        {
                            errors.Add($"Additional property '{name}' is not allowed.");
                        }
                    }
                }
            }

            if (jsonNode is JsonArray jsonArray && openApiSchema.Items is OpenApiSchema openApiSchema_Items)
            {
                for (int index = 0; index < jsonArray.Count; index++)
                {
                    if (!TableWireFormatSchemaFilter_Conforms(jsonArray[index], openApiSchema_Items, errors))
                    {
                        return false;
                    }
                }
            }

            if (jsonNode is JsonObject jsonObject_Members && openApiSchema.Properties is not null)
            {
                foreach (KeyValuePair<string, IOpenApiSchema> keyValuePair in openApiSchema.Properties)
                {
                    if (jsonObject_Members.ContainsKey(keyValuePair.Key) && keyValuePair.Value is OpenApiSchema openApiSchema_Member)
                    {
                        if (!TableWireFormatSchemaFilter_Conforms(jsonObject_Members[keyValuePair.Key], openApiSchema_Member, errors))
                        {
                            return false;
                        }
                    }
                }
            }

            return errors.Count == 0;
        }
    }
}
