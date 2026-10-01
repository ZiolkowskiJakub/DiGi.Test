using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.WebAPI.Classes;
using DiGi.WebAPI.Classes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            OpenApiSchema openApiSchema = SchemaGeneratorFixture_Schema(typeof(AdministrativeAreal2DReference));

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
            Assert.NotNull(openApiSchema_Health.AllOf);
            OpenApiSchemaReference? openApiSchemaReference = Assert.Single(openApiSchema_Health.AllOf) as OpenApiSchemaReference;
            Assert.NotNull(openApiSchemaReference);
            Assert.Equal(nameof(ServiceHealthInformation), openApiSchemaReference.Reference.Id);

            StringWriter stringWriter = new();
            openApiSchema_Health.SerializeAsV3(new OpenApiJsonWriter(stringWriter));
            JsonObject? jsonObject_Health = JsonNode.Parse(stringWriter.ToString()) as JsonObject;
            Assert.NotNull(jsonObject_Health);
            Assert.True(jsonObject_Health["nullable"]?.GetValue<bool>());
            Assert.NotNull(jsonObject_Health["allOf"]);
            Assert.False(jsonObject_Health.ContainsKey("type"), stringWriter.ToString());
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
