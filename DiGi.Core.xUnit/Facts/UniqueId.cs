using System.Text.Json;
using System.Text.Json.Nodes;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the UniqueId property is excluded from JSON serialization for both standard serialization and custom object-to-JSON conversion.
        /// </summary>
        [Fact]
        public void UniqueId()
        {
            string json = JsonSerializer.Serialize(new Classes.UniqueIdObject());
            Assert.False(string.IsNullOrWhiteSpace(json));
            Assert.DoesNotContain("UniqueId", json);

            TestObject testObject = new("AAAA");

            JsonObject? jsonObject = testObject.ToJsonObject();

            Assert.NotNull(jsonObject);

            Assert.False(jsonObject.ContainsKey(nameof(testObject.UniqueId)));
        }

        /// <summary>
        /// Tests the UniqueId query method for various types, verifying correct ID generation and ensuring that unsupported types are safely handled without runtime binder exceptions.
        /// </summary>
        [Fact]
        public void UniqueId_Generation()
        {
            // Test standard types
            string string_Val = "Hello World";
            string string_Id1 = string_Val.UniqueId();
            Assert.False(string.IsNullOrWhiteSpace(string_Id1));

            // Test fallback overload for unsupported types (Verifying the fix for Bug 4)
            long long_Val = 9876543210L;
            string string_Id2 = Core.Query.UniqueId(long_Val);
            Assert.Equal(long_Val.ToString(), string_Id2);

            // Test JsonValue types
            JsonValue jsonValue_String = JsonValue.Create("JsonString")!;
            string string_Id3 = Core.Query.UniqueId(jsonValue_String);
            Assert.False(string.IsNullOrWhiteSpace(string_Id3));

            JsonValue jsonValue_Long = JsonValue.Create(12345L)!;
            string string_Id4 = Core.Query.UniqueId(jsonValue_Long);
            Assert.Equal("12345", string_Id4);

            // Test custom class fallback
            object object_Custom = new();
            string string_Id5 = Core.Query.UniqueId(object_Custom);
            Assert.Equal(object_Custom.ToString(), string_Id5);

            // Test null safety
            object? object_Null = null;
            string string_IdNull = Core.Query.UniqueId(object_Null);
            Assert.Equal(Constants.UniqueId.Null, string_IdNull);
        }
    }
}