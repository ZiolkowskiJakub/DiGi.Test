using System.Text.Json.Nodes;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the TryConvert query methods, verifying conversion safety and validating that invalid formats do not throw exceptions.
        /// </summary>
        [Fact]
        public void TryConvert()
        {
            // Test standard conversions
            Assert.True(Core.Query.TryConvert("123", out int? int_Result));
            Assert.Equal(123, int_Result);

            Assert.True(Core.Query.TryConvert("123.45", out double? double_Result));
            Assert.Equal(123.45, double_Result);

            Assert.True(Core.Query.TryConvert("true", out bool? bool_Result));
            Assert.True(bool_Result);

            // Test JsonNode conversion safety with invalid JSON (Verifying the fix for Bug 2)
            string string_InvalidJson = "Not a valid JSON string";
            Assert.False(Core.Query.TryConvert_JsonNode(string_InvalidJson, out JsonNode? jsonNode_Result));
            Assert.Null(jsonNode_Result);

            // Test JsonNode conversion with valid JSON
            string string_ValidJson = "{\"name\":\"test\"}";
            Assert.True(Core.Query.TryConvert_JsonNode(string_ValidJson, out JsonNode? jsonNode_ValidResult));
            Assert.NotNull(jsonNode_ValidResult);
            Assert.Equal("test", jsonNode_ValidResult["name"]?.ToString());
        }
    }
}