using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.EPW.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.WebAPI.Classes;
using DiGi.Weather.Classes;
using DiGi.WebAPI.Classes;
using DiGi.WebAPI.WindowsService.Modify;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DiGi.WebAPI.WindowsService.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Reproduces ZiolkowskiJakub/DiGi.WebAPI.WindowsService#3 on the payload it was reported with: the schema of <see cref="AdministrativeAreal2DReference"/> must name exactly the properties the DiGi serializer writes (PascalCase, <c>_type</c> included), require all of them because the serializer always writes every member (<c>null</c> explicitly), keep a nullable member nullable, and give <c>_type</c> the serialized type name as its example.
        /// <para>The expected names come from serializing a real instance, and the wire itself is pinned against the response quoted in the issue, so the fact cannot pass because the schema and the serializer drifted together.</para>
        /// <para>Also reproduces ZiolkowskiJakub/DiGi.WebAPI.WindowsService#6 on the same payload: its <see cref="AdministrativeArealType"/> travels as an integer, so the property must be an inline integer schema listing the values in numeric order, naming them for code generators and keeping the member's own description - while the shared component stays the string schema query parameters bind against.</para>
        /// </summary>
        [Fact]
        public void ConfigureSchemaGeneration_SerializableObject()
        {
            AdministrativeAreal2DReference administrativeAreal2DReference = new();

            JsonObject? jsonObject = Core.Convert.ToJson(administrativeAreal2DReference);
            Assert.NotNull(jsonObject);

            List<string> names_Wire = SchemaGeneratorFixture_WireNames(administrativeAreal2DReference);
            Assert.Equal(["AdministrativeArealType", "Code", "CountryId", "CountyId", "Id", "MunicipalityId", "Name", "Reference", "VoivodeshipId", Core.Constants.Serialization.PropertyName.Type], names_Wire.Order(StringComparer.Ordinal));

            string? fullTypeName = jsonObject[Core.Constants.Serialization.PropertyName.Type]?.GetValue<string>();
            Assert.Equal("DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference,DiGi.GIS.PostgreSQL", fullTypeName);

            OpenApiSchema openApiSchema = SchemaGeneratorFixture_Schema(typeof(AdministrativeAreal2DReference), out SchemaRepository schemaRepository);

            Assert.NotNull(openApiSchema.Properties);
            Assert.Equal(names_Wire.Order(StringComparer.Ordinal), openApiSchema.Properties.Keys.Order(StringComparer.Ordinal));

            Assert.NotNull(openApiSchema.Required);
            Assert.Equal(names_Wire.Order(StringComparer.Ordinal), openApiSchema.Required.Order(StringComparer.Ordinal));

            Assert.False(openApiSchema.AdditionalPropertiesAllowed);

            IOpenApiSchema openApiSchema_Type = openApiSchema.Properties[Core.Constants.Serialization.PropertyName.Type];
            Assert.Equal(JsonSchemaType.String, openApiSchema_Type.Type);
            Assert.False(string.IsNullOrWhiteSpace(openApiSchema_Type.Description));
            Assert.Equal(fullTypeName, openApiSchema_Type.Example?.GetValue<string>());

            Assert.Equal(JsonSchemaType.Null, openApiSchema.Properties["CountyId"].Type & JsonSchemaType.Null);
            Assert.NotEqual(JsonSchemaType.Null, openApiSchema.Properties["Id"].Type & JsonSchemaType.Null);

            // ZiolkowskiJakub/DiGi.WebAPI.WindowsService#6: the serializer writes the enum as its underlying integer, as in the
            // response quoted there ("AdministrativeArealType": 2), so the property declares exactly those integers.
            JsonNode? jsonNode_AdministrativeArealType = Core.Convert.ToJson(new AdministrativeAreal2DReference() { AdministrativeArealType = AdministrativeArealType.County })?["AdministrativeArealType"];
            Assert.NotNull(jsonNode_AdministrativeArealType);
            Assert.Equal(JsonValueKind.Number, jsonNode_AdministrativeArealType.GetValueKind());
            Assert.Equal((int)AdministrativeArealType.County, jsonNode_AdministrativeArealType.GetValue<int>());

            OpenApiSchema? openApiSchema_AdministrativeArealType = openApiSchema.Properties["AdministrativeArealType"] as OpenApiSchema;
            Assert.NotNull(openApiSchema_AdministrativeArealType);
            Assert.Equal(JsonSchemaType.Integer, openApiSchema_AdministrativeArealType.Type);
            Assert.Equal("int32", openApiSchema_AdministrativeArealType.Format);

            // In numeric order: Enum.GetValues orders by the unsigned bit pattern and would put Undefined = -1 last.
            Assert.NotNull(openApiSchema_AdministrativeArealType.Enum);
            Assert.Equal([-1, 0, 1, 2, 3, 4], openApiSchema_AdministrativeArealType.Enum.Select(jsonNode => jsonNode?.GetValue<int>()));
            Assert.Equal(["Undefined", "Country", "Voivodeship", "County", "Municipality", "Subdivision"], SchemaGeneratorFixture_ExtensionStrings(openApiSchema_AdministrativeArealType, "x-enum-varnames"));
            Assert.Equal(["Undefined", "Country", "Voivodeship", "County", "Municipality", "Subdivision"], SchemaGeneratorFixture_ExtensionStrings(openApiSchema_AdministrativeArealType, "x-enumNames"));

            // The member's own description (lost beside a $ref in OpenAPI 3.0), then the enum's, then the wire mapping.
            Assert.NotNull(openApiSchema_AdministrativeArealType.Description);
            Assert.Contains("Gets or sets the type of the administrative area", openApiSchema_AdministrativeArealType.Description);
            Assert.Contains("Represents the type of administrative area.", openApiSchema_AdministrativeArealType.Description);
            Assert.Contains("Undefined = -1", openApiSchema_AdministrativeArealType.Description);
            Assert.Contains("County = 2", openApiSchema_AdministrativeArealType.Description);

            // The shared component keeps describing what the query parameters bind: member names.
            Assert.True(schemaRepository.Schemas.TryGetValue(nameof(AdministrativeArealType), out IOpenApiSchema? openApiSchema_Component));
            Assert.Equal(JsonSchemaType.String, openApiSchema_Component.Type);
        }

        /// <summary>
        /// Tests that a field-backed payload, whose public properties are <c>[JsonIgnore]</c> and whose serialized members are private <c>[JsonInclude]</c> fields, exposes every serialized member with the description of the public property it is named after.
        /// <para><see cref="ServiceHealthInformation"/> is served by <c>GET /information/health</c>, whose schema was published with no properties at all.</para>
        /// </summary>
        [Fact]
        public void ConfigureSchemaGeneration_FieldBacked()
        {
            ServiceHealthInformation serviceHealthInformation = new("Healthy", DateTime.UtcNow, DateTimeOffset.Now, TimeSpan.FromHours(5), 1234);

            List<string> names_Wire = SchemaGeneratorFixture_WireNames(serviceHealthInformation);
            Assert.Equal(["ProcessId", "ServerTimeLocal", "ServerTimeUtc", "Status", "Uptime", Core.Constants.Serialization.PropertyName.Type], names_Wire.Order(StringComparer.Ordinal));

            OpenApiSchema openApiSchema = SchemaGeneratorFixture_Schema(typeof(ServiceHealthInformation));

            Assert.NotNull(openApiSchema.Properties);
            Assert.Equal(names_Wire.Order(StringComparer.Ordinal), openApiSchema.Properties.Keys.Order(StringComparer.Ordinal));

            foreach (KeyValuePair<string, IOpenApiSchema> keyValuePair in openApiSchema.Properties)
            {
                Assert.False(string.IsNullOrWhiteSpace(keyValuePair.Value.Description), $"Property {keyValuePair.Key} has no description.");
            }
        }

        /// <summary>
        /// Tests that the members the DiGi serializer writes as an explicit <c>null</c> are declared so that <c>null</c> validates: a nullable value type carries the null type, and a nullable member typed by another component is wrapped around its reference, because OpenAPI 3.0 ignores <c>nullable</c> beside a bare <c>$ref</c>.
        /// <para>The rendered OpenAPI 3.0 form is asserted too: 3.0 has no <c>null</c> type, so the wrapper must come out as <c>allOf</c> plus <c>nullable: true</c>.</para>
        /// </summary>
        [Fact]
        public void ConfigureSchemaGeneration_NullableMember()
        {
            SerializableObjectFixture serializableObjectFixture = new(null, null);

            JsonObject? jsonObject = Core.Convert.ToJson(serializableObjectFixture);
            Assert.NotNull(jsonObject);
            Assert.True(jsonObject.ContainsKey(nameof(SerializableObjectFixture.Count)));
            Assert.Null(jsonObject[nameof(SerializableObjectFixture.Count)]);
            Assert.True(jsonObject.ContainsKey(nameof(SerializableObjectFixture.Health)));
            Assert.Null(jsonObject[nameof(SerializableObjectFixture.Health)]);

            OpenApiSchema openApiSchema = SchemaGeneratorFixture_Schema(typeof(SerializableObjectFixture));

            Assert.NotNull(openApiSchema.Properties);
            Assert.Equal(JsonSchemaType.Null, openApiSchema.Properties[nameof(SerializableObjectFixture.Count)].Type & JsonSchemaType.Null);

            OpenApiSchema? openApiSchema_Health = openApiSchema.Properties[nameof(SerializableObjectFixture.Health)] as OpenApiSchema;
            Assert.NotNull(openApiSchema_Health);
            Assert.NotNull(openApiSchema_Health.AnyOf);
            Assert.Equal(2, openApiSchema_Health.AnyOf.Count);
            
            // First item should be the reference to ServiceHealthInformation
            OpenApiSchemaReference? openApiSchemaReference = openApiSchema_Health.AnyOf[0] as OpenApiSchemaReference;
            Assert.NotNull(openApiSchemaReference);
            Assert.Equal(nameof(ServiceHealthInformation), openApiSchemaReference.Reference.Id);
            
            // Second item should be a schema that allows null: an enum listing null, which the writer keeps as a
            // distinct anyOf entry rather than folding into a nullable the typeless wrapper could not widen.
            OpenApiSchema? openApiSchema_Null = openApiSchema_Health.AnyOf[1] as OpenApiSchema;
            Assert.NotNull(openApiSchema_Null);
            Assert.NotNull(openApiSchema_Null.Enum);
            Assert.Contains(openApiSchema_Null.Enum, jsonNode => jsonNode is null);

            StringWriter stringWriter = new();
            openApiSchema_Health.SerializeAsV3(new OpenApiJsonWriter(stringWriter));
            JsonObject? jsonObject_Health = JsonNode.Parse(stringWriter.ToString()) as JsonObject;
            Assert.NotNull(jsonObject_Health);
            Assert.False(jsonObject_Health.ContainsKey("nullable"));
            Assert.NotNull(jsonObject_Health["anyOf"]);
            Assert.False(jsonObject_Health.ContainsKey("allOf"));
            Assert.False(jsonObject_Health.ContainsKey("type"));
        }

        /// <summary>
        /// Reproduces ZiolkowskiJakub/DiGi.WebAPI.WindowsService#6 on every enum shape a DiGi payload can carry, each of which the DiGi serializer writes as integers: a nullable enum, a list and a dictionary of enums, a <c>[Flags]</c> enum and a long-backed enum.
        /// <para>A nullable enum lists <c>null</c> among its values, because OpenAPI 3.0's <c>nullable</c> widens <c>type</c> only and an <c>enum</c> without <c>null</c> still rejects the explicit <c>null</c> the serializer writes; the rendered 3.0 form is asserted for that reason. A <c>[Flags]</c> enum lists no values, because a combined value (<c>Read | Write = 3</c>) is in no member list, and describes its bits instead.</para>
        /// </summary>
        [Fact]
        public void ConfigureSchemaGeneration_EnumMember()
        {
            EnumSerializableObjectFixture enumSerializableObjectFixture = new(null, [AdministrativeArealType.County], new Dictionary<string, AdministrativeArealType>() { ["2412"] = AdministrativeArealType.County }, FlagsEnumFixture.Read | FlagsEnumFixture.Write, LongEnumFixture.Large);

            JsonObject? jsonObject = Core.Convert.ToJson(enumSerializableObjectFixture);
            Assert.NotNull(jsonObject);
            Assert.True(jsonObject.ContainsKey(nameof(EnumSerializableObjectFixture.Level)));
            Assert.Null(jsonObject[nameof(EnumSerializableObjectFixture.Level)]);
            Assert.Equal((int)AdministrativeArealType.County, jsonObject[nameof(EnumSerializableObjectFixture.Levels)]?[0]?.GetValue<int>());
            Assert.Equal((int)AdministrativeArealType.County, jsonObject[nameof(EnumSerializableObjectFixture.LevelsByCode)]?["2412"]?.GetValue<int>());
            Assert.Equal(3, jsonObject[nameof(EnumSerializableObjectFixture.Flags)]?.GetValue<int>());
            Assert.Equal((long)LongEnumFixture.Large, jsonObject[nameof(EnumSerializableObjectFixture.Long)]?.GetValue<long>());

            OpenApiSchema openApiSchema = SchemaGeneratorFixture_Schema(typeof(EnumSerializableObjectFixture));
            Assert.NotNull(openApiSchema.Properties);

            List<int?> values_AdministrativeArealType = [-1, 0, 1, 2, 3, 4];
            List<string?> names_AdministrativeArealType = ["Undefined", "Country", "Voivodeship", "County", "Municipality", "Subdivision"];

            // Nullable: the null type, and null last among the values so the name arrays stay aligned with the integers.
            OpenApiSchema? openApiSchema_Level = openApiSchema.Properties[nameof(EnumSerializableObjectFixture.Level)] as OpenApiSchema;
            Assert.NotNull(openApiSchema_Level);
            Assert.Equal(JsonSchemaType.Integer | JsonSchemaType.Null, openApiSchema_Level.Type);
            Assert.NotNull(openApiSchema_Level.Enum);
            Assert.Null(openApiSchema_Level.Enum[^1]);
            Assert.Equal(values_AdministrativeArealType, openApiSchema_Level.Enum.Take(openApiSchema_Level.Enum.Count - 1).Select(jsonNode => jsonNode?.GetValue<int>()));
            Assert.Equal(names_AdministrativeArealType, SchemaGeneratorFixture_ExtensionStrings(openApiSchema_Level, "x-enum-varnames"));
            Assert.NotNull(openApiSchema_Level.Description);
            Assert.Contains("Gets the administrative level of the fixture", openApiSchema_Level.Description);

            StringWriter stringWriter = new();
            openApiSchema_Level.SerializeAsV3(new OpenApiJsonWriter(stringWriter));
            JsonObject? jsonObject_Level = JsonNode.Parse(stringWriter.ToString()) as JsonObject;
            Assert.NotNull(jsonObject_Level);
            Assert.Equal("integer", jsonObject_Level["type"]?.GetValue<string>());
            Assert.True(jsonObject_Level["nullable"]?.GetValue<bool>());
            Assert.False(jsonObject_Level.ContainsKey("allOf"), stringWriter.ToString());
            JsonArray? jsonArray_Level = jsonObject_Level["enum"] as JsonArray;
            Assert.NotNull(jsonArray_Level);
            Assert.Null(jsonArray_Level[^1]);

            // List and dictionary: the element schema is the inline integer one.
            IOpenApiSchema openApiSchema_Levels = openApiSchema.Properties[nameof(EnumSerializableObjectFixture.Levels)];
            Assert.Equal(JsonSchemaType.Array, openApiSchema_Levels.Type & ~JsonSchemaType.Null);
            OpenApiSchema? openApiSchema_Levels_Item = openApiSchema_Levels.Items as OpenApiSchema;
            Assert.NotNull(openApiSchema_Levels_Item);
            Assert.Equal(JsonSchemaType.Integer, openApiSchema_Levels_Item.Type);
            Assert.NotNull(openApiSchema_Levels_Item.Enum);
            Assert.Equal(values_AdministrativeArealType, openApiSchema_Levels_Item.Enum.Select(jsonNode => jsonNode?.GetValue<int>()));

            IOpenApiSchema openApiSchema_LevelsByCode = openApiSchema.Properties[nameof(EnumSerializableObjectFixture.LevelsByCode)];
            OpenApiSchema? openApiSchema_LevelsByCode_Value = openApiSchema_LevelsByCode.AdditionalProperties as OpenApiSchema;
            Assert.NotNull(openApiSchema_LevelsByCode_Value);
            Assert.Equal(JsonSchemaType.Integer, openApiSchema_LevelsByCode_Value.Type);
            Assert.NotNull(openApiSchema_LevelsByCode_Value.Enum);
            Assert.Equal(values_AdministrativeArealType, openApiSchema_LevelsByCode_Value.Enum.Select(jsonNode => jsonNode?.GetValue<int>()));

            // [Flags]: no value list, which the combined 3 would fail; the bits are in the description.
            OpenApiSchema? openApiSchema_Flags = openApiSchema.Properties[nameof(EnumSerializableObjectFixture.Flags)] as OpenApiSchema;
            Assert.NotNull(openApiSchema_Flags);
            Assert.Equal(JsonSchemaType.Integer, openApiSchema_Flags.Type);
            Assert.True(openApiSchema_Flags.Enum is null || openApiSchema_Flags.Enum.Count == 0);
            Assert.Null(SchemaGeneratorFixture_ExtensionStrings(openApiSchema_Flags, "x-enum-varnames"));
            Assert.NotNull(openApiSchema_Flags.Description);
            Assert.Contains("Read = 1", openApiSchema_Flags.Description);
            Assert.Contains("Write = 2", openApiSchema_Flags.Description);
            Assert.Contains("Execute = 4", openApiSchema_Flags.Description);

            // Long-backed: int64, and a value beyond the int32 range listed exactly.
            OpenApiSchema? openApiSchema_Long = openApiSchema.Properties[nameof(EnumSerializableObjectFixture.Long)] as OpenApiSchema;
            Assert.NotNull(openApiSchema_Long);
            Assert.Equal(JsonSchemaType.Integer, openApiSchema_Long.Type);
            Assert.Equal("int64", openApiSchema_Long.Format);
            Assert.NotNull(openApiSchema_Long.Enum);
            Assert.Equal([(long)LongEnumFixture.Small, (long)LongEnumFixture.Large], openApiSchema_Long.Enum.Select(jsonNode => jsonNode?.GetValue<long>()));
        }

        /// <summary>
        /// Reproduces the document-level half of ZiolkowskiJakub/DiGi.WebAPI.WindowsService#6 on a whole document generated through the host's configuration from <see cref="EnumControllerFixture"/>.
        /// <para>An enum query parameter keeps its string component, which is accurate for binding (names and integers both bind), and its description gains the integer values and the advice to send them (<c>Coding - WebAPI Contracts.md</c>, "Send enum values as integers"). An enum component that only DiGi payload members used is removed once those members are declared inline, because left behind it would still advertise the string form; the one the parameter references stays. No enum component is referenced from inside the DiGi payload schema any more - the detector for an enum shape the payload rewrite does not cover.</para>
        /// </summary>
        [Fact]
        public void ConfigureSchemaGeneration_Document()
        {
            OpenApiDocument openApiDocument = SchemaGeneratorFixture_Document();

            Assert.NotNull(openApiDocument.Paths);
            Assert.True(openApiDocument.Paths.TryGetValue("/fixture/item", out IOpenApiPathItem? openApiPathItem));
            Assert.NotNull(openApiPathItem.Operations);
            Assert.True(openApiPathItem.Operations.TryGetValue(HttpMethod.Get, out OpenApiOperation? openApiOperation));
            Assert.NotNull(openApiOperation.Parameters);

            IOpenApiParameter openApiParameter = Assert.Single(openApiOperation.Parameters);
            Assert.Equal("administrativearealtype", openApiParameter.Name);

            OpenApiSchemaReference? openApiSchemaReference_Parameter = openApiParameter.Schema as OpenApiSchemaReference;
            Assert.NotNull(openApiSchemaReference_Parameter);
            Assert.Equal(nameof(AdministrativeArealType), openApiSchemaReference_Parameter.Reference.Id);

            // The parameter's own XML description is kept and the integer values are appended to it.
            Assert.NotNull(openApiParameter.Description);
            Assert.Contains("The administrative level to return.", openApiParameter.Description);
            Assert.Contains("Undefined = -1", openApiParameter.Description);
            Assert.Contains("County = 2", openApiParameter.Description);

            Assert.NotNull(openApiDocument.Components?.Schemas);
            IDictionary<string, IOpenApiSchema> schemas = openApiDocument.Components.Schemas;

            Assert.True(schemas.TryGetValue(nameof(AdministrativeArealType), out IOpenApiSchema? openApiSchema_AdministrativeArealType));
            Assert.Equal(JsonSchemaType.String, openApiSchema_AdministrativeArealType.Type);

            Assert.True(schemas.ContainsKey(nameof(EnumSerializableObjectFixture)));
            Assert.False(schemas.ContainsKey(nameof(FlagsEnumFixture)), "An enum component no longer referenced by anything was left in the document.");
            Assert.False(schemas.ContainsKey(nameof(LongEnumFixture)), "An enum component no longer referenced by anything was left in the document.");

            // Detector: no $ref to an enum component inside the DiGi payload schema.
            StringWriter stringWriter = new();
            schemas[nameof(EnumSerializableObjectFixture)].SerializeAsV3(new OpenApiJsonWriter(stringWriter));
            string json_Payload = stringWriter.ToString();
            foreach (KeyValuePair<string, IOpenApiSchema> keyValuePair in schemas.Where(keyValuePair => keyValuePair.Value.Enum is not null && keyValuePair.Value.Enum.Count != 0))
            {
                Assert.DoesNotContain($"#/components/schemas/{keyValuePair.Key}\"", json_Payload);
            }
        }

        /// <summary>
        /// Tests that a DiGi payload which is also an <c>IEnumerable</c> is documented as the JSON object the DiGi serializer writes, not as the array Swashbuckle infers from the interface.
        /// <para>Found by strict validation of <c>GET /gis/epwfile/item</c> (2026-10-01): <see cref="EPWFile"/> derives from <c>Weather</c>, an <c>IEnumerable&lt;WeatherRecord&gt;</c>, so its schema was <c>type: array</c> - before the fix bare, after the first fix with the object's properties bolted on - while the wire is an object carrying <c>WeatherRecords</c>, <c>Location</c>, ....</para>
        /// </summary>
        [Fact]
        public void ConfigureSchemaGeneration_Enumerable()
        {
            Assert.True(typeof(IEnumerable<WeatherRecord>).IsAssignableFrom(typeof(EPWFile)));

            EPWFile ePWFile = new(location: null);
            List<string> names_Wire = SchemaGeneratorFixture_WireNames(ePWFile);
            Assert.Contains("WeatherRecords", names_Wire);

            OpenApiSchema openApiSchema = SchemaGeneratorFixture_Schema(typeof(EPWFile));

            Assert.Equal(JsonSchemaType.Object, openApiSchema.Type & ~JsonSchemaType.Null);
            Assert.Null(openApiSchema.Items);
            Assert.NotNull(openApiSchema.Properties);
            Assert.Equal(names_Wire.Order(StringComparer.Ordinal), openApiSchema.Properties.Keys.Order(StringComparer.Ordinal));
        }

        /// <summary>
        /// Tests that a concrete payload type with a loaded subclass lists its own members but admits additional ones, because a member typed by it may hold the subclass, which the DiGi serializer writes with the subclass's extra members; a type nothing derives from stays closed.
        /// <para>Found by strict validation of <c>GET /gis/epwfile/item</c> (2026-10-01): <c>EPWFile.WeatherRecords</c> is a list of <see cref="WeatherRecord"/> holding <see cref="DataRecord"/>s, whose <c>Albedo</c>, <c>Visibility</c>, ... were rejected by the closed <see cref="WeatherRecord"/> schema on all 8 760 records.</para>
        /// </summary>
        [Fact]
        public void ConfigureSchemaGeneration_DerivedType()
        {
            Assert.True(typeof(WeatherRecord).IsAssignableFrom(typeof(DataRecord)));

            List<string> names_Wire = SchemaGeneratorFixture_WireNames(new WeatherRecord((WeatherRecord?)null));
            List<string> names_Wire_Derived = SchemaGeneratorFixture_WireNames(new DataRecord((DataRecord?)null));
            Assert.NotEmpty(names_Wire_Derived.Except(names_Wire, StringComparer.Ordinal));

            OpenApiSchema openApiSchema = SchemaGeneratorFixture_Schema(typeof(WeatherRecord));

            Assert.NotNull(openApiSchema.Properties);
            Assert.Equal(names_Wire.Order(StringComparer.Ordinal), openApiSchema.Properties.Keys.Order(StringComparer.Ordinal));
            Assert.True(openApiSchema.AdditionalPropertiesAllowed);

            OpenApiSchema openApiSchema_Derived = SchemaGeneratorFixture_Schema(typeof(DataRecord));

            Assert.NotNull(openApiSchema_Derived.Properties);
            Assert.Equal(names_Wire_Derived.Order(StringComparer.Ordinal), openApiSchema_Derived.Properties.Keys.Order(StringComparer.Ordinal));
            Assert.False(openApiSchema_Derived.AdditionalPropertiesAllowed);
        }

        /// <summary>
        /// Tests that an interface payload type gets an open schema: <c>_type</c> required and the only declared property, additional properties allowed.
        /// <para>The wire carries the members of the runtime type, which an interface cannot enumerate; the fact serializes a <see cref="Polygon2D"/> to show that a real <see cref="IPolygonal2D"/> payload carries members beyond <c>_type</c>, which a closed schema would reject.</para>
        /// </summary>
        [Fact]
        public void ConfigureSchemaGeneration_Interface()
        {
            Assert.True(typeof(ISerializableObject).IsAssignableFrom(typeof(IPolygonal2D)));

            Polygon2D polygon2D = new([new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 1)]);
            List<string> names_Wire = SchemaGeneratorFixture_WireNames(polygon2D);
            Assert.Contains(Core.Constants.Serialization.PropertyName.Type, names_Wire);
            Assert.True(names_Wire.Count > 1);

            OpenApiSchema openApiSchema = SchemaGeneratorFixture_Schema(typeof(IPolygonal2D));

            Assert.NotNull(openApiSchema.Properties);
            Assert.Equal([Core.Constants.Serialization.PropertyName.Type], openApiSchema.Properties.Keys);

            Assert.NotNull(openApiSchema.Required);
            Assert.Equal([Core.Constants.Serialization.PropertyName.Type], openApiSchema.Required);

            Assert.True(openApiSchema.AdditionalPropertiesAllowed);
        }

        /// <summary>
        /// Tests that a payload type overriding <see cref="SerializableObject.ToJsonObject"/> gets the open schema too: its JSON is not built from its serializable members, so a property list taken from them would describe a format it does not write.
        /// </summary>
        [Fact]
        public void ConfigureSchemaGeneration_CustomJsonObject()
        {
            MethodInfo? methodInfo = typeof(SerializableObjectWrapper).GetMethod(nameof(SerializableObject.ToJsonObject), Type.EmptyTypes);
            Assert.NotNull(methodInfo);
            Assert.NotEqual(typeof(SerializableObject), methodInfo.DeclaringType);

            OpenApiSchema openApiSchema = SchemaGeneratorFixture_Schema(typeof(SerializableObjectWrapper));

            Assert.NotNull(openApiSchema.Properties);
            Assert.Equal([Core.Constants.Serialization.PropertyName.Type], openApiSchema.Properties.Keys);

            Assert.NotNull(openApiSchema.Required);
            Assert.Equal([Core.Constants.Serialization.PropertyName.Type], openApiSchema.Required);

            Assert.True(openApiSchema.AdditionalPropertiesAllowed);
        }

        /// <summary>
        /// Control: a payload written by the MVC formatter (<c>Ok(...)</c>) keeps a schema naming exactly what the MVC formatter writes - camelCase, no <c>_type</c> - and its enum stays a string component (<c>{"reason":"Undefined"}</c>).
        /// <para>The expected names come from serializing an instance with the host's own MVC options. <see cref="MvcPayloadFixture"/> carries a PascalCase <c>[JsonPropertyName]</c>, which the host writes camelCase while Swashbuckle alone would document it as spelled, so the camelCase rename of non-DiGi schemas is load-bearing. Enum declaration on DiGi payloads is ZiolkowskiJakub/DiGi.WebAPI.WindowsService#6; this fact pins the MVC side it must not break.</para>
        /// </summary>
        [Fact]
        public void ConfigureSchemaGeneration_MvcPayload()
        {
            JsonSerializerOptions jsonSerializerOptions = new();
            jsonSerializerOptions.ConfigureJsonSerializerOptions();

            Assert.False(typeof(ISerializableObject).IsAssignableFrom(typeof(UpdateItemsResult)));

            JsonObject? jsonObject_UpdateItemsResult = JsonSerializer.SerializeToNode(new UpdateItemsResult(), jsonSerializerOptions) as JsonObject;
            Assert.NotNull(jsonObject_UpdateItemsResult);
            Assert.Equal(["rejected", "sent", "updated"], jsonObject_UpdateItemsResult.Select(keyValuePair => keyValuePair.Key).Order(StringComparer.Ordinal));

            OpenApiSchema openApiSchema = SchemaGeneratorFixture_Schema(typeof(UpdateItemsResult), out SchemaRepository schemaRepository);

            Assert.NotNull(openApiSchema.Properties);
            Assert.Equal(jsonObject_UpdateItemsResult.Select(keyValuePair => keyValuePair.Key).Order(StringComparer.Ordinal), openApiSchema.Properties.Keys.Order(StringComparer.Ordinal));
            Assert.False(openApiSchema.AdditionalPropertiesAllowed);

            Assert.True(schemaRepository.Schemas.TryGetValue("UpdateRejectionReason", out IOpenApiSchema? openApiSchema_Enum));
            Assert.Equal(JsonSchemaType.String, openApiSchema_Enum.Type);
            Assert.NotNull(openApiSchema_Enum.Enum);
            Assert.Contains("Undefined", openApiSchema_Enum.Enum.Select(jsonNode => jsonNode?.GetValue<string>()));

            JsonObject? jsonObject_Fixture = JsonSerializer.SerializeToNode(new MvcPayloadFixture(), jsonSerializerOptions) as JsonObject;
            Assert.NotNull(jsonObject_Fixture);
            Assert.Equal(["count", "label"], jsonObject_Fixture.Select(keyValuePair => keyValuePair.Key).Order(StringComparer.Ordinal));

            OpenApiSchema openApiSchema_Fixture = SchemaGeneratorFixture_Schema(typeof(MvcPayloadFixture));
            OpenApiSchema openApiSchema_Fixture_Default = SchemaGeneratorFixture_Schema(typeof(MvcPayloadFixture), false);

            Assert.NotNull(openApiSchema_Fixture.Properties);
            Assert.Equal(jsonObject_Fixture.Select(keyValuePair => keyValuePair.Key).Order(StringComparer.Ordinal), openApiSchema_Fixture.Properties.Keys.Order(StringComparer.Ordinal));

            Assert.NotNull(openApiSchema_Fixture_Default.Properties);
            Assert.Contains("Count", openApiSchema_Fixture_Default.Properties.Keys);
        }

        /// <summary>
        /// Guard: on a property-backed DiGi payload Swashbuckle already honours <c>[JsonPropertyName]</c> and documents the PascalCase wire names; the host's schema configuration must add exactly <c>_type</c> to them and must not rename anything.
        /// <para>Measured on the unmodified host (2026-10-01): Swashbuckle alone produced <c>CountyId, Reference, ...</c> while the host produced <c>countyId, reference, ...</c> - the camelCase on DiGi schemas came entirely from the host's own schema filter, not from the MVC JSON options.</para>
        /// </summary>
        [Fact]
        public void ConfigureSchemaGeneration_Differential()
        {
            OpenApiSchema openApiSchema = SchemaGeneratorFixture_Schema(typeof(AdministrativeAreal2DReference));
            OpenApiSchema openApiSchema_Default = SchemaGeneratorFixture_Schema(typeof(AdministrativeAreal2DReference), false);

            Assert.NotNull(openApiSchema.Properties);
            Assert.NotNull(openApiSchema_Default.Properties);
            Assert.DoesNotContain(Core.Constants.Serialization.PropertyName.Type, openApiSchema_Default.Properties.Keys);

            Assert.Equal([Core.Constants.Serialization.PropertyName.Type], openApiSchema.Properties.Keys.Except(openApiSchema_Default.Properties.Keys, StringComparer.Ordinal));
            Assert.Empty(openApiSchema_Default.Properties.Keys.Except(openApiSchema.Properties.Keys, StringComparer.Ordinal));
        }
    }
}
