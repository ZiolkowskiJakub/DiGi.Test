using System.Text.Json.Serialization;

namespace DiGi.WebAPI.WindowsService.xUnit
{
    public partial class Facts
    {
        // An MVC payload carrying a PascalCase [JsonPropertyName]: the host's ForceCamelCaseModifier writes it camelCase
        // regardless, while Swashbuckle on its own documents the attribute's spelling - the case the schema filter's
        // camelCase rename of non-DiGi schemas exists for.
        private sealed class MvcPayloadFixture
        {
            [JsonPropertyName("Count")]
            public int Count { get; set; } = 3;

            public string Label { get; set; } = "Fixture";
        }
    }
}
