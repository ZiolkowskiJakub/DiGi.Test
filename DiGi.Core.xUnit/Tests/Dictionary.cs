using System.Text.Json;

namespace DiGi.Core.xUnit
{
    public partial class Tests
    {
        [Fact]
        public void Dictionary()
        {
            Dictionary<string, int> dictionary = new()
            {
                { "AAA", 1 },
                { "BBB", 2 }
            };

            JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true };

            string json = JsonSerializer.Serialize(dictionary, jsonSerializerOptions);

            Assert.NotNull(json);

            DictionaryObject dictionaryObject_1 = new();
            dictionaryObject_1.Add("AAAA");
            dictionaryObject_1.Add("BBBB");

            TestObject testObject = new("AAAA");

            string? json_TestObject_1 = Convert.ToSystem_String(testObject);
            Assert.NotNull(json_TestObject_1);

            TestObject? testObject_2 = Convert.ToDiGi<TestObject>(json_TestObject_1)?.FirstOrDefault();
            Assert.NotNull(testObject_2);

            string? json_TestObject_2 = Convert.ToSystem_String(testObject_2);
            Assert.NotNull(json_TestObject_2);

            string? json_1 = Convert.ToSystem_String(dictionaryObject_1);
            Assert.NotNull(json_1);

            DictionaryObject? dictionaryObject_2 = Convert.ToDiGi<DictionaryObject>(json_1)?.FirstOrDefault();
            Assert.NotNull(dictionaryObject_2);

            string? json_2 = Convert.ToSystem_String(dictionaryObject_2);
            Assert.NotNull(json_2);

            Assert.Equal(json_1, json_2);
        }
    }
}