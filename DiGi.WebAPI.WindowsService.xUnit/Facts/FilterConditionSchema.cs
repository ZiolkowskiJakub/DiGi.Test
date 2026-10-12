using DiGi.PostgreSQL.Table.Classes;
using Microsoft.OpenApi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.WebAPI.WindowsService.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// The served document describes which JSON shape <see cref="FilterCondition.Value"/> takes per operator and which column ids it accepts, so a client author does not have to read the SQL builder to learn the contract (ZiolkowskiJakub/DiGi.PostgreSQL#13).
        /// <para>Generates the schema through the host's own <c>ConfigureSchemaGeneration</c> and <c>IncludeAssemblyXmlComments</c> - the same path the served <c>/swagger/gis/swagger.json</c> uses - and asserts the <c>value</c> and <c>columnUniqueId</c> property descriptions carry the contract: a scalar for the comparison operators, a JSON array for <c>In</c>/<c>NotIn</c>, a case-insensitive string for <c>Contains</c>, an ignored value for <c>IsNull</c>/<c>IsNotNull</c>, and the two failure modes (an unconvertible value throws <c>InvalidFilterException</c>; a null or empty value degrades to no match instead of an error).</para>
        /// </summary>
        [Fact]
        public void FilterCondition_ValueDescription_DocumentsTheValueShapePerOperator()
        {
            OpenApiSchema openApiSchema = SchemaGeneratorFixture_Schema(typeof(FilterCondition));

            IOpenApiSchema? openApiSchema_Value = FilterConditionSchema_Property(openApiSchema, "value");
            Assert.NotNull(openApiSchema_Value);
            Assert.False(string.IsNullOrWhiteSpace(openApiSchema_Value.Description));

            // The accepted JSON shape per operator group.
            Assert.Contains("scalar", openApiSchema_Value.Description, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("JSON array", openApiSchema_Value.Description, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("case-insensitive", openApiSchema_Value.Description, StringComparison.OrdinalIgnoreCase);

            // The two failure modes: an unconvertible value throws; a null or empty value degrades to no match.
            Assert.Contains("InvalidFilterException", openApiSchema_Value.Description, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("IS NULL", openApiSchema_Value.Description, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("IS NOT NULL", openApiSchema_Value.Description, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("1=0", openApiSchema_Value.Description, StringComparison.OrdinalIgnoreCase);

            // The column id is whitelisted against the stored table's columns.
            IOpenApiSchema? openApiSchema_Column = FilterConditionSchema_Property(openApiSchema, "columnUniqueId");
            Assert.NotNull(openApiSchema_Column);
            Assert.False(string.IsNullOrWhiteSpace(openApiSchema_Column.Description));
            Assert.Contains("columnuniqueids", openApiSchema_Column.Description, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("InvalidFilterException", openApiSchema_Column.Description, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Looks up a property of a generated schema by name, tolerating the naming policy (camelCase vs PascalCase) the host applies to a plain POCO such as <see cref="FilterCondition"/>.
        /// </summary>
        /// <param name="openApiSchema">The generated schema of the type to inspect.</param>
        /// <param name="name">The property name, matched case-insensitively.</param>
        /// <returns>The property's schema; <c>null</c> when no property by that name is present.</returns>
        private static IOpenApiSchema? FilterConditionSchema_Property(OpenApiSchema openApiSchema, string name)
        {
            IDictionary<string, IOpenApiSchema>? properties = openApiSchema.Properties;
            if (properties is null)
            {
                return null;
            }

            return properties.FirstOrDefault(keyValuePair => string.Equals(keyValuePair.Key, name, StringComparison.OrdinalIgnoreCase)).Value;
        }
    }
}
