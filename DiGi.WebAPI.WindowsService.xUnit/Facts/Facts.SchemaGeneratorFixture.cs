using DiGi.GIS.PostgreSQL.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace DiGi.WebAPI.WindowsService.xUnit
{
    public partial class Facts
    {
        // Generates the schema of a type - its component, or the inline schema of a type Swashbuckle gives no component -
        // through the host's own MVC JSON and schema configuration
        // (Modify.ConfigureJsonSerializerOptions, Modify.ConfigureSchemaGeneration), with the XML documentation of the
        // type's assembly attached as the host attaches it. configureSchemaGeneration: false leaves out the host's schema
        // configuration and keeps only the camelCase parameters, which is what the served document would be without it.
        private static OpenApiSchema SchemaGeneratorFixture_Schema(Type type, bool configureSchemaGeneration = true)
        {
            return SchemaGeneratorFixture_Schema(type, out _, configureSchemaGeneration);
        }

        private static OpenApiSchema SchemaGeneratorFixture_Schema(Type type, out SchemaRepository schemaRepository, bool configureSchemaGeneration = true)
        {
            ServiceCollection serviceCollection = new();
            serviceCollection.AddLogging();
            serviceCollection.AddControllers().AddJsonOptions(jsonOptions => jsonOptions.JsonSerializerOptions.ConfigureJsonSerializerOptions());
            serviceCollection.AddSwaggerGen(swaggerGenOptions =>
            {
                if (configureSchemaGeneration)
                {
                    swaggerGenOptions.ConfigureSchemaGeneration();
                }
                else
                {
                    swaggerGenOptions.DescribeAllParametersInCamelCase();
                }

                swaggerGenOptions.IncludeAssemblyXmlComments([type.Assembly]);
            });

            using ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            ISchemaGenerator schemaGenerator = serviceProvider.GetRequiredService<ISchemaGenerator>();

            schemaRepository = new();
            IOpenApiSchema openApiSchema_Generated = schemaGenerator.GenerateSchema(type, schemaRepository);

            // Swashbuckle gives an enumerable type (a DiGi Weather is an IEnumerable<WeatherRecord>) no component and
            // returns its schema inline.
            if (openApiSchema_Generated is OpenApiSchema openApiSchema_Inline)
            {
                return openApiSchema_Inline;
            }

            Assert.True(schemaRepository.TryLookupByType(type, out OpenApiSchemaReference? openApiSchemaReference), $"No component schema was generated for {type.FullName}.");
            Assert.NotNull(openApiSchemaReference);

            string? id = openApiSchemaReference.Reference.Id;
            Assert.False(string.IsNullOrWhiteSpace(id));

            OpenApiSchema? openApiSchema = schemaRepository.Schemas[id] as OpenApiSchema;
            Assert.NotNull(openApiSchema);

            return openApiSchema;
        }

        // Generates a whole document through the host's MVC JSON and schema configuration, holding exactly the operations of
        // EnumControllerFixture, with the XML documentation of the test assembly and of the enum's assembly attached: what
        // the host serves for a route prefix, minus the per-prefix document registration in Program. Unlike a schema
        // generated on its own, it runs the parameter and document filters too.
        private static OpenApiDocument SchemaGeneratorFixture_Document()
        {
            const string documentName = "fixture";

            // A web application builder rather than a bare service collection: document generation needs the hosting
            // environment. The application is built but never run.
            WebApplicationBuilder webApplicationBuilder = WebApplication.CreateBuilder();
            webApplicationBuilder.Services.AddControllers()
                .AddJsonOptions(jsonOptions => jsonOptions.JsonSerializerOptions.ConfigureJsonSerializerOptions())
                .ConfigureApplicationPartManager(applicationPartManager => applicationPartManager.FeatureProviders.Add(new ControllerFeatureProviderFixture()));
            webApplicationBuilder.Services.AddSwaggerGen(swaggerGenOptions =>
            {
                swaggerGenOptions.SwaggerDoc(documentName, new OpenApiInfo { Title = documentName, Version = "1" });
                swaggerGenOptions.ConfigureSchemaGeneration();
                swaggerGenOptions.IncludeAssemblyXmlComments([typeof(Facts).Assembly, typeof(AdministrativeArealType).Assembly]);
            });

            using WebApplication webApplication = webApplicationBuilder.Build();

            return webApplication.Services.GetRequiredService<ISwaggerProvider>().GetSwagger(documentName);
        }

        // The strings of an array-valued schema extension (x-enum-varnames, x-enumNames); null when the schema does not
        // carry the extension.
        private static List<string?>? SchemaGeneratorFixture_ExtensionStrings(IOpenApiSchema? openApiSchema, string name)
        {
            if (openApiSchema?.Extensions is null || !openApiSchema.Extensions.TryGetValue(name, out IOpenApiExtension? openApiExtension))
            {
                return null;
            }

            JsonArray? jsonArray = (openApiExtension as JsonNodeExtension)?.Node as JsonArray;
            Assert.NotNull(jsonArray);

            return [.. jsonArray.Select(jsonNode => jsonNode?.GetValue<string>())];
        }

        // The property names the DiGi serializer actually writes for an instance, in the order it writes them: the wire
        // contract a payload schema has to describe.
        private static List<string> SchemaGeneratorFixture_WireNames(Core.Interfaces.ISerializableObject serializableObject)
        {
            JsonObject? jsonObject = Core.Convert.ToJson(serializableObject);
            Assert.NotNull(jsonObject);

            return [.. jsonObject.Select(keyValuePair => keyValuePair.Key)];
        }
    }
}
