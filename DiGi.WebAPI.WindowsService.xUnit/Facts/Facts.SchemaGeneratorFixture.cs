using DiGi.GIS.PostgreSQL.Enums;
using DiGi.WebAPI.WindowsService.Modify;
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

        // Generates the component schema of a type into a caller-owned repository, so several types can be generated the
        // way one document generates them, with the host's schema configuration and, appended after it, the schema filters
        // the host's Program registers for a loaded extension assembly (its IWebAPISchemaFilter types) - the registration
        // order the served documents get. Returns the generated component.
        private static OpenApiSchema SchemaGeneratorFixture_Schema(Type type, SchemaRepository schemaRepository, Type[]? types_SchemaFilter = null)
        {
            ServiceCollection serviceCollection = new();
            serviceCollection.AddLogging();
            serviceCollection.AddControllers().AddJsonOptions(jsonOptions => jsonOptions.JsonSerializerOptions.ConfigureJsonSerializerOptions());
            serviceCollection.AddSwaggerGen(swaggerGenOptions =>
            {
                swaggerGenOptions.ConfigureSchemaGeneration();
                swaggerGenOptions.IncludeAssemblyXmlComments([type.Assembly]);

                if (types_SchemaFilter is not null)
                {
                    foreach (Type type_SchemaFilter in types_SchemaFilter)
                    {
                        swaggerGenOptions.SchemaFilterDescriptors.Add(new FilterDescriptor
                        {
                            Type = type_SchemaFilter,
                            Arguments = []
                        });
                    }
                }
            });

            using ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            ISchemaGenerator schemaGenerator = serviceProvider.GetRequiredService<ISchemaGenerator>();

            IOpenApiSchema openApiSchema_Generated = schemaGenerator.GenerateSchema(type, schemaRepository);

            // Swashbuckle gives an enumerable type (a DiGi Weather is an IEnumerable<WeatherRecord>, the Core.IO Table an
            // IEnumerable<Row>, a List<T> an array) no component and returns its schema inline - the filters run on that
            // inline schema all the same, which is what the served document describes for such a type.
            if (openApiSchema_Generated is OpenApiSchema openApiSchema_Inline)
            {
                return openApiSchema_Inline;
            }

            Assert.True(schemaRepository.TryLookupByType(type, out OpenApiSchemaReference? openApiSchemaReference), $"No component schema was generated for {type.FullName}.");

            string? id = openApiSchemaReference?.Reference.Id;
            Assert.False(string.IsNullOrWhiteSpace(id));

            OpenApiSchema? openApiSchema = schemaRepository.Schemas[id!] as OpenApiSchema;
            Assert.NotNull(openApiSchema);

            return openApiSchema;
        }

        // Generates a whole document through the host's MVC JSON and schema configuration, holding exactly the operations of
        // EnumControllerFixture - plus those of any controllers named by types_Controller, a real extension's controllers -
        // with the XML documentation of the test assembly, of the enum's assembly and of those controllers' assemblies
        // attached: what the host serves for a route prefix, minus the per-prefix document registration in Program, and,
        // through types_SchemaFilter, types_DocumentFilter and types_OperationFilter, plus the schema, document and operation filters the host's Program
        // appends for a loaded extension assembly, after its own. Unlike a schema generated on its own, it runs the
        // parameter, operation and document filters too.
        private static OpenApiDocument SchemaGeneratorFixture_Document(Type[]? types_Controller = null, Type[]? types_SchemaFilter = null, Type[]? types_DocumentFilter = null, Type[]? types_OperationFilter = null)
        {
            const string documentName = "fixture";

            // A web application builder rather than a bare service collection: document generation needs the hosting
            // environment. The application is built but never run.
            WebApplicationBuilder webApplicationBuilder = WebApplication.CreateBuilder();
            webApplicationBuilder.Services.AddControllers()
                .AddJsonOptions(jsonOptions => jsonOptions.JsonSerializerOptions.ConfigureJsonSerializerOptions())
                .ConfigureApplicationPartManager(applicationPartManager => applicationPartManager.FeatureProviders.Add(new ControllerFeatureProviderFixture(types_Controller)));
            webApplicationBuilder.Services.AddSwaggerGen(swaggerGenOptions =>
            {
                swaggerGenOptions.SwaggerDoc(documentName, new OpenApiInfo { Title = documentName, Version = "1" });
                swaggerGenOptions.ConfigureSchemaGeneration();
                swaggerGenOptions.IncludeAssemblyXmlComments([typeof(Facts).Assembly, typeof(AdministrativeArealType).Assembly, .. (types_Controller ?? []).Select(type => type.Assembly)]);

                foreach (Type type_SchemaFilter in types_SchemaFilter ?? [])
                {
                    swaggerGenOptions.SchemaFilterDescriptors.Add(new FilterDescriptor
                    {
                        Type = type_SchemaFilter,
                        Arguments = []
                    });
                }

                foreach (Type type_DocumentFilter in types_DocumentFilter ?? [])
                {
                    swaggerGenOptions.DocumentFilterDescriptors.Add(new FilterDescriptor
                    {
                        Type = type_DocumentFilter,
                        Arguments = []
                    });
                }

                foreach (Type type_OperationFilter in types_OperationFilter ?? [])
                {
                    swaggerGenOptions.OperationFilterDescriptors.Add(new FilterDescriptor
                    {
                        Type = type_OperationFilter,
                        Arguments = []
                    });
                }
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
